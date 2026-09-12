using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using GameStateManagement;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Resources;
using System.Linq;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Combat;
using System.Diagnostics;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    public struct ItemDistance
    {
        public IKnownEntityData Entity;
        public float Distance;
    }

    public struct ReplenishStatus
    {
        /// <summary>
        /// do we have the replenish item in the inventory? (does not consider accessibility!)
        /// </summary>
        public bool? OwnsItem;

        /// <summary>
        /// are we carrying the item?
        /// </summary>
      //  public bool? CarriesItem;

        /// <summary>
        /// for how long time the above properties hold.
        /// </summary>
        public float? DurationInDays;
    }

    /// <summary>
    /// make this the ONE evaluator that deals with jobs that are not attack or hauling jobs!!!
    /// </summary>
    public class EvaluateJob : GoalEvaluator, IScoreJob
    {

        //public enum Distance { VeryNear, Near,   }
        // private Jobs.Job bestJob = null;
        private ToolOrWeaponInstanceCombo bestCombo;


        /// <summary>
        /// gather jobs will find a resource:
        /// </summary>
        //   private IResourceItem bestResourceItem = null;
        //   private IResourceItem currentResourceItem = null;

        /// <summary>
        /// Process jobs may find an input that is too large to haul (carcasses, logs)
        /// </summary>
        //   private Entity bestInputItem = null;
        //    private Entity currentInputItem = null;
        //   private Vector3? bestLocation = null;
        //    private Vector3? currentLocation = null;
        //   private List<Entity> currentTools = new List<Entity>();

        /// <summary>
        /// the best tools for the selected job
        /// </summary>
        //  private List<Entity> bestTools = new List<Entity>();


        // whole classs should be reinstantiated when ownership changes...
        private EntityGroup itemGroup;
        private List<EntityGroup> vehicleGroups;
        private EntityGroup ownerOfJobs;

        private OwnerID? ownerOfNewProducts;

        //*************
        // progress variables for timeslicing/interrupt and continue:
        private enum Progress { NotStarted, GetCombos, ScoreCombos, FindFinalCombo } 
        private Progress progress = Progress.NotStarted;

        // private List<Entity> needsToBeCancelled = new List<Entity>();

        /// <summary>
        /// these Entities are agents in our allegiance and so are alway directly seen/never in fog of war.
        /// </summary>
        private List<Entity> needsToBeCancelled = new List<Entity>();
        private List<Entity> itemsToBeDropped = new List<Entity>();


        private Dictionary<EntityType, bool> gatherJobTypeComboAlreadyAdded = new Dictionary<EntityType, bool>();

        // private Dictionary<Job, double> cachedJobScores = new Dictionary<Job, double>();
        private Dictionary<Job, ToolOrWeaponInstanceComboJobData> cachedJobScores = new Dictionary<Job, ToolOrWeaponInstanceComboJobData>();
        // private Dictionary<Entity, double> cachedToolScores = new Dictionary<Entity, double>();



        // private Dictionary<EntityType, bool> cachedToolEnergyAvailableStates = new Dictionary<EntityType, bool>();
        public Dictionary<EntityType, ReplenishStatus> cachedToolEnergyAvailableStates = new Dictionary<EntityType, ReplenishStatus>();

        private Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates = new Dictionary<AttackType, ReplenishWeaponStatus>();

        /// <summary>
        /// store the tools that we have found succesful replenish results for, with this job duration and job score
        /// </summary>
        //  private Dictionary<Entity, List<Entity>> cachedReplenishResults = new Dictionary<Entity, List<Entity>>();

        private Dictionary<EntityType, List<ItemDistance>> energyItemsSortedByDistanceToEntity = new Dictionary<EntityType, List<ItemDistance>>();


        // progress variables - keep track of how far we have come between calls to CalculateResult
        private int scoreComboIndex = 0;
        private int finalComboSelectionIndex = 0;

        private bool scoringWasInterrupted = false; // this keeps track of us getting interrupted and then having to resume in a later frame.

        //***********************

        private float priority;
        public override float Priority
        {
            get
            {
                return priority;
            }
        }

        public enum JobType { ColonyWork, NonColonyWork }
        public EvaluateJob(Entity entity, EntityGroup ownerOfJobs, EntityGroup itemGroup, List<EntityGroup> vehicleGroups, OwnerID? ownerOfNewProducts) //, JobType jobtype)
            : base(entity)
        {
            this.ownerOfNewProducts = ownerOfNewProducts;
            this.ownerOfJobs = ownerOfJobs;
            this.itemGroup = itemGroup;
            this.vehicleGroups = vehicleGroups;

          //  this.IsColonyWork = jobtype == JobType.ColonyWork; // ???

            this.priority = GameData.Instance.AIConstants.PriorityOfWorkJobs; 

        }


        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            if (entity.Name != null && (entity.Name.Contains("zov")))
            {
                int i = 0;
            }

            bestCombo = null;
            bestScore = minimumRatingToConsider;


            if (progress == Progress.NotStarted)
            {

                BiologicalEntity bioEntity;
                if (entity.Find(out bioEntity))
                {
                    double ageContribution = 1;

                    ageContribution = ScoreAge(bioEntity);

                    if (ageContribution == 0)
                    {
                        result = 0;
                        return CalculateResult.Done;
                    }
                }

                double timeOfDayContribution = ScoreTimeOfDay();

                // to ease the load, we don't calculate the combos too often when the time is not right:
                /*  if (DoHeavyCalculations(timeOfDayContribution)) // Delete this??? too confusing? do production at night???
                  {*/
                progress = Progress.GetCombos; // start!

                //  processedJobs.Clear();

                needsToBeCancelled.Clear();
                itemsToBeDropped.Clear();
                // }

                scoringWasInterrupted = false;

                /*bestResourceItem = null;
                bestLocation = null;
                bestInputItem = null;*/
                //bestTools.Clear();

                /*  currentInputItem = null;
                  currentResourceItem = null;
                  currentLocation = null;
                  currentTools.Clear();*/

            }

            // the reason we gather all combos first before scoring, is because scoring might be interrupted when we score distances to the tools.
            // doing it this way, we can easily keep track of where we are and don't need to start over.             

            ThreatStance threatStance;
            RegionMap footRegionMap = GetRegionMapAndStanceForEvaluator(entity, null, out threatStance);
            /* entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(
                 entity.Intelligence.ProtectionLevel, 
                 entity.Intelligence.Allegiance.RepresentativeEntityType.ThreatCategory, 
                 entity.Intelligence.ThreatStance).RegionMap[SurfaceType.TransportType.Foot];*/


            if (progress == Progress.GetCombos)
            {
                GetAllJobCombos(footRegionMap);
            }

            if (progress == Progress.ScoreCombos)
            {
                if (ScoreAllCombos(footRegionMap, threatStance) == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }

            }

           
            if (progress == Progress.FindFinalCombo)
            {

                // if (ScoreAllCombosAndReturnBest(minimumRatingToConsider, ref bestCombo) == CalculateResult.Done)
                if (FindBestCombo(minimumRatingToConsider, ref bestCombo) == CalculateResult.Done)
                {

                    if (bestCombo != null) //bestCombo.HasValue)
                    {

                        bestScore = bestCombo.Score;
                        result = bestCombo.Score;

                        //progress = Progress.GetCombos; // start from the top next time!

                        // for debugging feedback. 
                        entityIntelligence.TopScoringJobs.Add(new GoalAndScore() { Score = bestScore, Goal = bestCombo.Job.ToString() });

                        return CalculateResult.Done;
                    }
                }
                else return CalculateResult.Processing;

            }

            // no vacant jobs...
            if (bestCombo != null)
            {
                int i = 3;
            }
            result = 0;
            return CalculateResult.Done;
        }

        public static void SetAreaNotCleared(Job job, bool value)
        {
            EntityGroup owner;
            job.ResolveOwner(out owner);

            if (owner != null)
            {
                The.Client.SetAreaNotCleared(job, owner.Parent, value);
            }
        }


        private bool IsGatherJobAccessible(ProcessJob pJob, RegionMap regionMap)
        {
           //
            Point fromSubtilePos = MapManager.WorldPosToSubtile(entity.PlaySiteLocation);

            if (pJob.HarvestJob != null)
            {
                Point destinationSubtilePos;
                destinationSubtilePos = MapManager.WorldPosToSubtile(pJob.HarvestJob.Item.Container.AccessPoint);


                float distanceFromToolToJob = -1;

                //TODO: Check if there is a better function to check accessibility
                RegionMap.Result result = regionMap.GetDistanceToEntity(entity, entity, null, ref distanceFromToolToJob, fromSubtilePos, destinationSubtilePos);
                if (result == RegionMap.Result.Wait)
                {
                    return false;
                }

                if (result == RegionMap.Result.NoAccess)
                {
                    UpdateJobAccessibility(entity, fromSubtilePos,
                       destinationSubtilePos, pJob, ownerOfJobs.Parent, result);
                    
                    // The.Client.SetJobInaccessible(pJob, ownerOfJobs.Parent, true);

                    Crop cropContainer = pJob.HarvestJob.Item.Container as Crop;
                    if (cropContainer != null && cropContainer.ParentEntity != null)
                    {
                        //cropContainer.ParentEntity.Renderable.SetOverlayTintColor(Color.Red); // why not set the regular tint instead..?
                        cropContainer.ParentEntity.Renderable.SetTintColor(Color.Red); // why not set the regular tint instead..?
                    }
                    else
                    {
                        TileResourceContainer container = pJob.HarvestJob.Item.Container as TileResourceContainer;
                        if (container != null)
                        {
                            container.Renderable.SetResourceContainerTintColor(Color.Red);
                            // container.Renderable.SetTintColor(Color.Red);
                        }
                    }

                    return false;

                }
                else
                {
                    The.Client.SetJobInaccessible(pJob, ownerOfJobs.Parent, false);
                    //    job.SetIsInaccessible(ownerOfJobs.InternalOwner, false); // = true;


                    Crop cropContainer = pJob.HarvestJob.Item.Container as Crop;
                    if (cropContainer != null && cropContainer.ParentEntity != null)
                    {
                        cropContainer.ParentEntity.Renderable.SetResourceContainerTintColor(Color.Yellow);
                    }
                    else
                    {
                        TileResourceContainer container = pJob.HarvestJob.Item.Container as TileResourceContainer;
                        if (container != null)
                        {
                            container.Renderable.SetResourceContainerTintColor(Color.Yellow);
                        }
                    }
                    return true;
                }
            }

            return true;
        }

        private void GetAllJobCombos(RegionMap regionMap)
        {
            scoreComboIndex = 0;
            allCombos.Clear();

            Job job;
            foreach (var kvp in ownerOfJobs.ProductionJobs) // also gather jobs
            {
                for (int i = kvp.Value.Count - 1; i >= 0; i--)
                {
                    job = kvp.Value[i];
                   /* if (job is HuntingJob)
                    {
                        continue; //We don´t handle hunting jobs here
                    }
                    */

                    if (!entityIntelligence.Brain.IsSame(job)) // don't consider a job we are already doing!                    
                    {
                        
                        // for sets of harvest jobs for instance, it is enough to consider one... this is because we use the CropsMap to find harvesting locations
                        // So, gather job combos are optimized/culled in the following way:
                        // the first job that we find which is 
                        // 1. Accessible and 
                        // 2. Not taken by any agent
                        // is added to a list, so we can disregard all other jobs and combos for that resource type
                     
                     
                        ProcessJob pJob = job as ProcessJob;
                        if (pJob != null)
                        {   
                            // the accessibility check is done for all jobs in order to provide feedback to the player:
                            bool isAccessible = IsGatherJobAccessible(pJob, regionMap);

                          
                            if (isAccessible
                                && !SkipCombos(pJob)) 
                            {
                                bool jobIsDestroyed;
                                GetAllToolInstanceCombos(pJob, out jobIsDestroyed);

                                // - Be careful... If the first harvest job is taken by another, but the rest are not, then we still want to look at those jobs!
                                // for jobs that haven't been taken by others: skip the rest. we don't need to look at other similar harvest jobs (there could be hundreds):
                                if (!jobIsDestroyed && pJob.TakenBy.Count == 0)
                                {
                                    SetGatherJobTypeComboAsAlreadyAdded(pJob);
                                }

                                if (jobIsDestroyed)
                                {
                                    pJob.Destroy(true); // NEW
                                }
                            }
                        }
                    }
                }
            }

            // TODO: merge ScoutingEvaluator og FindPreyEvaluator with this:
            //*** scout jobs ***
            foreach (ScoutingJob scoutingJob in ownerOfJobs.ScoutingJobs)
            {
                if (!entityIntelligence.Brain.IsSame(scoutingJob)) // don't consider a job we are already doing!                    
                {
                    //  GetAllToolInstanceCombos(scoutingJob);

                }
            }

            // **** find prey jobs ***
            foreach (FindPreyJob findPreyJob in ownerOfJobs.FindPreyJobs)
            {
                if (!entityIntelligence.Brain.IsSame(findPreyJob)) // don't consider a job we are already doing!                    
                {
                    GetAllWeaponInstanceCombos(findPreyJob);

                }
            }

            foreach (CheckProcessJob checkProcessJob in ownerOfJobs.CheckProcessJobs)
            {
                if (!entityIntelligence.Brain.IsSame(checkProcessJob)) // don't consider a job we are already doing!                    
                {
                    GetEmptyCombo(checkProcessJob);
                }
            }

            foreach (PatrolJob patrolJob in ownerOfJobs.PatrolJobs)
            {
                if (!entityIntelligence.Brain.IsSame(patrolJob)) // don't consider a job we are already doing!                    
                {
                    GetAllWeaponInstanceCombos(patrolJob);

                }
            }

            foreach (AttackAreaJob attackAreaJob in ownerOfJobs.AttackAreaJobs)
            {
                if (!entityIntelligence.Brain.IsSame(attackAreaJob)) // don't consider a job we are already doing!                    
                {
                    GetAllWeaponInstanceCombos(attackAreaJob);
                }
            }

            for (int i = ownerOfJobs.OtherJobs.Count - 1; i >= 0; i--)// Construction, smoke bomb and stoke fire/light fire (not implemented yet) also add healing etc.
            {
                Jobs.Job job1 = ownerOfJobs.OtherJobs[i];

                if (job1 is HuntingJob)
                {
                    continue; //We don´t handle hunting jobs here
                }

                if (!entityIntelligence.Brain.IsSame(job1)) // don't consider a job we are already doing!                    
                {
                    bool jobIsDestroyed;
                    GetAllToolInstanceCombos(job1, out jobIsDestroyed);
                }
            }       
    
            if (ownerOfJobs.RepairJobs.Count > 0)
            {
                // copy the job keys to allow cleanup:
                foreach (var item in ownerOfJobs.RepairJobs.Keys)
                {
                    List<ProcessJob> jobs = ownerOfJobs.RepairJobs[item];

                    for (int i = jobs.Count - 1; i >= 0; i--)
                    {
                        ProcessJob pJob = jobs[i];
                        if (!entityIntelligence.Brain.IsSame(pJob)) // don't consider a job we are already doing!                    
                        {
                            bool jobIsDestroyed;
                            GetAllToolInstanceCombos(pJob, out jobIsDestroyed);
                        }                        
                    }
                }
            }

            progress = Progress.ScoreCombos;
        }

        private bool IsComboValid(Entity entity, Intelligence entityIntelligence, ToolOrWeaponInstanceCombo combo)
        {
            if (combo.Job == null // we can cleanup invalid jobs while iterating, and the combo Job property will be set to null. This means we want to skip it.
                || combo.Job.ID == JobID.Invalid) 
                return false;

            foreach (var tool in combo.ToolsAndWeapons)
            {
                bool inUseByNonWorkerProcess;
                bool isBroken;
                if (!IsValidPlaysiteTool(tool, null, entityIntelligence.Allegiance.SharedKnowledge, ownerOfJobs, out inUseByNonWorkerProcess, out isBroken))
                {
                    return false;
                }
            }

            return true;
        }

        private CalculateResult ScoreAllCombos(RegionMap footRegionMap, ThreatStance threatStance)
        {
            ToolOrWeaponInstanceCombo combo;
            double score = -1.0;

         
            double ageContribution = GetAgeContribution();
            double timeOfDayContribution = ScoreTimeOfDay();

            //float estimatedJobDuration = 

            bool jobIsValid;

            for (; scoreComboIndex < allCombos.Count; scoreComboIndex++)
            {
                
                combo = allCombos[scoreComboIndex];

                //Before scoring, check that the job and tools are still valid!
                if (!scoringWasInterrupted || IsComboValid(entity, entityIntelligence, combo)) // only need to check for validity if we are resuming.
                {
                    int proposedNumberOfWorkers = Common.Clamp(combo.Job.TakenBy.Count + 1, 0, combo.Job.MaxJobPositions);

                    float toolsetProductivity = 1f;
                    if (combo.ToolTypeCombination != null)
                    {
                        toolsetProductivity = combo.ToolTypeCombination.Productivity;
                    }

                    ToolParams toolParams = new ToolParams()
                    {
                        Tools = combo.ToolsAndWeapons,
                        ImmovableTool = combo.ImmovableTool,
                        ToolProductivity = toolsetProductivity,
                        ReplenishStatus = cachedToolEnergyAvailableStates,
                        JobDurationInDays = null
                    };

                    if (ScoreThisWorkJob(footRegionMap, threatStance, entity, combo.Job, proposedNumberOfWorkers, ageContribution, timeOfDayContribution,
                        ref combo.JobData, toolParams, out score, out jobIsValid)
                        //  ref combo.InputItem, ref combo.Location, ref combo.ResourceItem, out score) 
                        == CalculateResult.Done)
                    {

                        combo.Score = score;

                        EvaluateJob.SetDebugScore(entity, combo.Job, score);               
                    }
                    else
                    {
                        scoringWasInterrupted = true; // now we need to check all the combos again for availability...
                        return CalculateResult.Processing; // come back later...
                    }

                    if (!jobIsValid)
                    {                      
                        //remove the job:
                        Job jobToRemove = combo.Job;
                        HandleInvalidJob(jobToRemove);

                        // and set the Job property to null for the relevant combos:
                        allCombos.FindAll(c => c.Job == jobToRemove)
                            .ForEach(c =>
                            {
                                c.Job = null;
                                combo.Score = 0;
                            });

                        // this will check combos again for validity, and skip those we just handled
                        scoringWasInterrupted = true;
                    }
                }
                else
                {
                    combo.Score = 0;

                    EvaluateJob.SetDebugScore(entity, combo.Job, 0);               
                
                }
            }


            AssignLocations();


            // reset variables and progress to next phase:
            // clear the cache now so it won't get used when an updated score is requested:
            gatherJobTypeComboAlreadyAdded.Clear();
            cachedJobScores.Clear();
            cachedToolEnergyAvailableStates.Clear();
            cachedWeaponReplenishStates.Clear();

            scoreComboIndex = 0;

            progress = Progress.FindFinalCombo;

            return CalculateResult.Done;

        }


        /// <summary>
        /// go through all combos, also the zero-scoring ones (WHY?), to assign a location if needed
        /// use info gathered while evaluating
        /// </summary>
        private void AssignLocations()
        {
            ToolOrWeaponInstanceCombo combo;
          
            for (int i = 0; i < allCombos.Count; i++)
            {                
                combo = allCombos[i];

                // remember that jobData is shared between many combos.
                
                if (combo.JobData != null 
                    && combo.JobData.HighestToolScore.HasValue // this means it has no location
                    && combo.JobData.HighestScoringToolCombo == combo.ToolsAndWeapons) // do we have the correct combo?
                {
                    ProcessJob processJob = combo.Job as ProcessJob;
                    
                    if (combo.ImmovableTool.HasValue)
                    {
                        IKnownEntityData itemData;
                        if (!EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(combo.ImmovableTool.Value, out itemData))
                            && itemData.AssignedToJob == null) // NEW - don't assign the same tool to more than one job...
                        {   
                            // WHY assign broken tools???

                            // also sets the lock on the tool!
                            processJob.AssignImmovableTool(itemData); 
                        }

                    }
                    else if (combo.JobData.TemporaryGroundLocation.HasValue)
                    {
                        processJob.AssignJobLocation(combo.JobData.TemporaryGroundLocation.Value);
                    }

                    combo.JobData.HighestToolScore = null; // skip the following combos - no matter what. Start the scoring again next time...
                }
            }
        }

        private void ResetAllToolsInUse(ToolOrWeaponInstanceCombo combo)
        {
            if (combo.Job is ProcessJob)
            {
                ProcessJob currentComboJob = combo.Job as ProcessJob;
                if (combo.ToolTypeCombination != null) // tools needed?
                {
                    currentComboJob.AllToolsInUse = false;

                    currentComboJob.AllToolsAreBroken = false; // NEW
                }
            }
        }

        private CalculateResult FindBestCombo(double minimumRatingToConsider, ref ToolOrWeaponInstanceCombo bestCombo)
        {
            ToolOrWeaponInstanceCombo combo;

            // Important! Remove the combos with zero score:
            FilterAndSortCombos();

            bestCombo = null;


            // final combo selection.
            // find the replenish items that we need for the tools.
            if (allCombos.Count > 0 && entity.Name != null && entity.Name.Contains("coyd"))
            {

            }
            

            for (; finalComboSelectionIndex < allCombos.Count; finalComboSelectionIndex++)
            {
                combo = allCombos[finalComboSelectionIndex];

                ResetAllToolsInUse(combo); // reset the "All tools in use" flag on all the combos to avoid having outdated info

                if (!scoringWasInterrupted || IsComboValid(entity, entityIntelligence, combo)) // only need to check for validity if we are resuming.
                {
                    if (combo.Score < minimumRatingToConsider)
                    {
                        for (int i = finalComboSelectionIndex; i < allCombos.Count; i++)
                        {
                            ResetAllToolsInUse(allCombos[i]); // reset the "All tools in use" flag on all the combos that we skip to avoid having outdated info
                        }

                        break; // the score is not good enough. we have reached the low scores and are now finished
                    }
                }
                else
                {
                    continue; // this combo is no longer available, skip it.
                    //break; 
                }


                // we have the best job now. See if we need to cancel anyone:

                // TODO: set a score on equipped items!
                if (!GetJobsToCancel(combo))
                {
                    continue; // this combo is no longer available, skip it.
                }
                

                // if there are people doing this job, only take it from them if our score is better than theirs by a certain margin.
                // what about itemsToBeDropped???
                if (IsScoreBetterThanAllInvolveds(combo.Score, needsToBeCancelled, entity))
                {
                    // Time to see if the selected tool set needs replenishment.
                    // This step may fail. In which case we go to the next combo.

                    // The reason I decided not build this into the combo/scoring system was to avoid an explosion in item combinations.
                    // Doing it this way, makes the code a bit more complex, and the results less optimal, but should cut down on total computations..

                    // Important: the replenishment items are not scored as such, only rated for distance to the agent.
                    // There is also a distance cutoff, that disregards items more than one screen away.
                    if (combo.Job is ProcessJob)
                    {
                        ProcessJob currentComboJob = combo.Job as ProcessJob;
                        if (combo.ToolTypeCombination != null) // tools needed?
                        {
                           
                            // get the approximate duration:

                            //     float jobDuration = ProcessJob.CalculateWorkDurationInDays(entity, currentComboJob.GetTimeNeeded(), currentComboJob.ProcessType.RequiredSkillType, combo.ToolTypeCombination.Productivity); //  combo.Job.GetWorstCaseDurationForEnergyEstimation(); 
                            float jobDuration = currentComboJob.ProcessType.EstimateTotalDurationInDays(entity, currentComboJob.ProcessType.GetTimeNeeded(), combo.ToolTypeCombination.Productivity);

                            bool replenishResult;
                            // see if we can find the items that we need:
                            CalculateResult result = FindReplenishItems(combo, jobDuration, out replenishResult);

                            if (result == CalculateResult.Processing)
                            {
                                return CalculateResult.Processing;
                            }
                            else if (replenishResult == false)
                            {
                                // couldn't replenish the needed tools. Look at next combo.
                               
                                continue;
                            }
                            else
                            {
                                

                                // got it.
                                bestCombo = combo;

                                //  GetReplenishItemsToCancel(bestCombo);
                                GetReplenishItemsToCancel(bestCombo.ReplenishItemsForTools,
                                    ref needsToBeCancelled, ref itemsToBeDropped);

                                break;
                            }
                        }
                        else
                        {
                            bestCombo = combo;


                            break;
                        }
                    }
                    else
                    {
                        //this code assumes that the combo attack type is not null, this should be the case
                        if (combo.ToolsAndWeapons.Count > 0 && combo.attackType.UsesAmmo != null)//do we have a weapon to find prey with?
                        {
                            bool replenishResult;
                            // see if we can find the items that we need:
                            IKnownEntityData weapon;
                            entityIntelligence.GetKnownData(combo.ToolsAndWeapons[0], out weapon);

                            CalculateResult result = FindReplenishItems
                                (combo, 0.0f, out replenishResult,
                                combo.attackType.UsesAmmoType,
                                combo.attackType.RoundsToSpend.Value
                                );


                            if (result == CalculateResult.Processing)
                            {
                                return CalculateResult.Processing;
                            }
                            else if (replenishResult == false)
                            {
                                // couldn't replenish the needed tools. Look at next combo.
                                continue;
                            }
                            else
                            {
                                // got it.
                                bestCombo = combo;

                                //  GetReplenishItemsToCancel(bestCombo);
                                GetReplenishItemsToCancel(bestCombo.ReplenishItemsForTools,
                                    ref needsToBeCancelled, ref itemsToBeDropped);

                                break;
                            }
                        }
                        else
                        {
                            bestCombo = combo;

                            break;
                        }

                        // do hunting stuff here or adapt above if statement to hunting
                    }
                }
                else
                {//We are not good enough to take a needed tool from someone else.

                    //We need to get a tool someone else is using
                    if (itemsToBeDropped.Count > 0 && combo.ToolTypeCombination != null)
                    {
                        //Noone is working with this job
                        if (combo.Job.TakenBy.Count == 0)
                        {
                            ProcessJob pJob = combo.Job as ProcessJob;
                            if (pJob != null)
                            {
                                pJob.AllToolsInUse = true;
                            }
                        }
                    }
                }

                // the job was taken and we didn't score high enough to take it from them. continue looking.
            }

           
#if !RELEASE
            if (bestCombo != null && bestCombo.ToolsAndWeapons != null)
            {
                foreach (var item in bestCombo.ToolsAndWeapons)
                {
                    Entity itemAsEntity = Entity.FindByID(item);
                    if (itemAsEntity != null) // && itemAsEntity.AssignedToJob.HasValue && itemAsEntity.AssignedToJob != processjob.ID)
                    {
                        bool inUseByUnattendedProcess;
                        if (IsInUseByNonWorkerProcess(itemAsEntity, out inUseByUnattendedProcess)
                            && inUseByUnattendedProcess)
                        {
                            // should not be allowed...
                        }
                    }
                }
            }
#endif


            // reset progress variables for next time
            finalComboSelectionIndex = 0;
            energyItemsSortedByDistanceToEntity.Clear();
            progress = Progress.NotStarted; // all done. Start from the top next time!

            return CalculateResult.Done;

        }

        private void FilterAndSortCombos()
        {
            allCombos.RemoveAll(c => c.Score == 0);

            allCombos.Sort((a, b) => b.Score.CompareTo(a.Score));
        }
        


        private CalculateResult FindReplenishItems(ToolOrWeaponInstanceCombo combo, float jobDuration, out bool success, EntityType neededAmmoType = null, int? neededAmmoRounds = null) // out double replenishResult)
        {
            // find energy items:

            //replenishResult = 0d;

            double ourScore = combo.Score;

            SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;

            RegionMap footRegionMap = null;

            if (entity.EntityType.IntelligenceType.IsMobile)
            {
                footRegionMap = sharedKnowledge.GetMovementMap(entity).Layers[SurfaceType.TransportType.Foot].RegionMap; // UWGame.SimSide.Instance.Map.RegionMapManager.HumanFootExposedNormalRegionMap;
            }

            IKnownEntityData toolData;
            RequiresFuelType requiresFuelType;
            RequiresPowerType requiresPowerType;
            float? neededFuelBulk = null;
            float? neededElectricalEnergy = null;
            Expedition expedition = itemGroup.GetExpedition();

            foreach (var entityToReplenish in combo.ToolsAndWeapons)
            {
                if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entityToReplenish, out toolData)))
                {
                    success = false;
                    return CalculateResult.Done;
                }

                if (toolData.EntityType.ContainerType != null &&
                    toolData.EntityType.ContainerType.GetRequiresReplenishType() != null)
                {
                    // TODO: move branching to RequiresReplenishType..?
                    requiresFuelType = toolData.EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType;
                    if (requiresFuelType != null)
                    {
                        neededFuelBulk = requiresFuelType.GetNeededFuel(jobDuration);
                    }
                    else
                    {
                        neededFuelBulk = null;
                    }

                  /*  requiresPowerType = toolData.EntityType.ContainerType.RequiresReplenishType.RequiresPowerType;
                    if (requiresPowerType != null)
                    {
                        neededElectricalEnergy = (float)(requiresPowerType.EnergyUsePerDay * jobDuration);
                    }
                    else
                    {
                        neededElectricalEnergy = null;
                    }*/
                }

                CalculateResult result = FindReplenishItemsForToolOrWeapon(entity, itemGroup, null, ref combo.ReplenishItemsForTools, ourScore, sharedKnowledge, footRegionMap,
                    toolData, energyItemsSortedByDistanceToEntity, out success,
                    neededAmmoType, neededAmmoRounds, neededElectricalEnergy, neededFuelBulk);

                if (result == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }
                else if (success == false)
                {   // failed to replenish this tool in the set...

                    // set a flag also:                   
                    if (expedition != null)
                    {
                        The.Client.SetToolReplenishStatusItemAvailable(expedition, toolData.EntityType, false, neededFuelBulk);
                    }

                    return CalculateResult.Done;
                }
                else
                {                  
                    if (expedition != null)
                    {
                        The.Client.SetToolReplenishStatusItemAvailable(expedition, toolData.EntityType, true, neededFuelBulk);
                    }
                }
            }

            success = true;
            return CalculateResult.Done;
        }

        

        private bool GetJobsToCancel(ToolOrWeaponInstanceCombo combo) 
        {
            needsToBeCancelled.Clear();
            itemsToBeDropped.Clear();
            List<Entity> takenBy = new List<Entity>();
            Job job = combo.Job;
             
            if (job.TakenBy.Count > 0)
            {
                job.TakenBy.GetLowestScorer(); // ???
                for (int i = 0; i < job.TakenBy.Count; i++)
                {
                    System.Diagnostics.Debug.Assert(entity != job.TakenBy.Get(i), "Cannot cancel self.");
                    takenBy.Add(job.TakenBy.Get(i));
                }
                while (takenBy.Count >= job.MaxJobPositions)
                {
                    needsToBeCancelled.Add(takenBy[0]);
                    takenBy.RemoveAt(0);
                }
            }


            if (combo.ToolsAndWeapons != null && combo.ToolsAndWeapons.Count > 0)
            {
                foreach (var tool in combo.ToolsAndWeapons)
                {
                    IKnownEntityData toolData;
                    if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(tool, out toolData)))
                    {
                        return false;
                    }

                    GetItemUsersToCancel(toolData, ref needsToBeCancelled, ref itemsToBeDropped, false); //, false);


                   /* if (toolData != null && toolData.AssignedToJob.HasValue && toolData.AssignedToJob != processjob.ID)
                    {

                    }   */                     
                    
                }
            }

            ProcessJob processJob = combo.Job as ProcessJob;
            if (processJob != null)
            {
               
                if (processJob.HarvestJob != null)
                {
                    // for gather jobs, if the crop/resource has been taken, we must also score higher than the taker:
                    if (combo.JobData.ResourceItem.AssignedToJob != null
                            && combo.JobData.ResourceItem.AssignedToJob.TakenBy.Count > 0)
                    {
                        needsToBeCancelled.Add(combo.JobData.ResourceItem.AssignedToJob.TakenBy.Get(0));
                    }
                }


                // NEW: Acting on Entity requires AssignedToJob also (same as for tools)
                // it is no longer okay to repair/upgrade tools in use!
                EntityID? actingOn;
                if (processJob.GetActingOnEntity(out actingOn)
                    && actingOn.HasValue)
                {
                    IKnownEntityData actingOnData;
                    if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(actingOn.Value, out actingOnData)))
                    {
                        return false;
                    }

                    GetItemUsersToCancel(actingOnData, ref needsToBeCancelled, ref itemsToBeDropped, false); //, false);

                }
            }

            // remove duplicates:
            needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
            itemsToBeDropped = itemsToBeDropped.Distinct().ToList();

            return true;
        }



        /// <summary>
        /// implements IScoreJob!
        /// </summary>
        /// <param name="regionMap"></param>
        /// <param name="entity"></param>
        /// <param name="job"></param>
        /// <param name="proposedNumberOfWorkers"></param>
        /// <param name="ageContribution"></param>
        /// <param name="timeContribution"></param>
        /// <param name="rating"></param>
        /// <param name="toolParams"></param>
        /// <param name="attackParams"></param>
        /// <returns></returns>       
        public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStance, Entity entity, Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution,
            out double rating, ToolParams? toolParams, AttackParams? attackParams, HaulingParams? haulingParams) // List<Entity> tools, float? toolsetProductivity, Dictionary<EntityType, ReplenishStatus> availableFuel)
        {

            ToolOrWeaponInstanceComboJobData jobData = null;
            bool jobIsValid;

            return ScoreThisWorkJob(regionMap, threatStance, entity, job,
                proposedNumberOfWorkers, ageContribution, timeContribution,
                ref jobData,
                toolParams, //  toolsetProductivity.HasValue ? toolsetProductivity.Value : 1f,
                out rating, out jobIsValid, false);
        }


        public CalculateResult ScoreThisWorkJob(RegionMap regionMap, ThreatStance threatStance, Entity entity, Jobs.Job Job/*ToolOrWeaponInstanceCombo combo*/, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution,
            ref ToolOrWeaponInstanceComboJobData jobData, ToolParams? toolParams
            , out double rating, out bool jobIsValid, /*out double debugRating,*/ bool cacheScore = true)
        {
            rating = 0;
          //  debugRating = 0;

            jobIsValid = true;


            if (!ageContribution.HasValue)
            {
                ageContribution = GetAgeContribution();
            }

            if (!timeContribution.HasValue)
            {
                timeContribution = ScoreTimeOfDay();
            }

            RegionMap regionMapToUse;
            ThreatStance threatStanceToUse;
            // this test does not affect the score...
            if (!CanTakeStanceForJob(Job, regionMap, threatStance, out regionMapToUse, out threatStanceToUse))
            {
                rating = 0;

                return CalculateResult.Done;
            }

            ProcessJob pJob = Job as ProcessJob;
            if (pJob != null)
            {
                return pJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, proposedNumberOfWorkers, ageContribution, timeContribution,
                    ref jobData, toolParams, ref rating, ref jobIsValid, cacheScore,
                    cachedJobScores, entityIntelligence, Priority, ownerOfJobs);
            }
            
            LightFireJob lightFireJob = Job as LightFireJob;
            if (lightFireJob != null)
            {
                return lightFireJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, ageContribution, timeContribution, ref rating, Priority);
            }

            FindPreyJob findPreyJob = Job as FindPreyJob;
            if (findPreyJob != null)
            {
                return findPreyJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, proposedNumberOfWorkers, ageContribution, timeContribution,
                    ref jobData, toolParams, ref rating, ref jobIsValid, cachedJobScores, entityIntelligence, priority, ownerOfJobs, cacheScore);//, combo.Tools[0]);

            }

            CheckProcessJob checkProcessJob = Job as CheckProcessJob;
            if (checkProcessJob != null)
            {
                return checkProcessJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, proposedNumberOfWorkers, ageContribution, timeContribution,
                    ref jobData, toolParams, ref rating, ref jobIsValid, cachedJobScores, entityIntelligence, priority, ownerOfJobs, cacheScore);//, combo.Tools[0]);

            }

            CombatAreaJob combatAreaJob = Job as CombatAreaJob;
            if (combatAreaJob != null)
            {
                return combatAreaJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, proposedNumberOfWorkers, ageContribution, timeContribution,
                   ref jobData, toolParams, ref rating, ref jobIsValid, cachedJobScores, entityIntelligence, priority, ownerOfJobs, cacheScore);
            }

          /*  PatrolJob patrolJob = Job as PatrolJob;
            if(patrolJob != null)
            {
                return patrolJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, proposedNumberOfWorkers, ageContribution, timeContribution,
                    ref jobData, toolParams, ref rating, ref jobIsValid, cachedJobScores, entityIntelligence, priority, ownerOfJobs, cacheScore);
            }
            AttackAreaJob attackAreaJob = Job as AttackAreaJob;
            if (attackAreaJob != null)
            {
                return attackAreaJob.ScoreThisJob(regionMapToUse, threatStanceToUse, entity, proposedNumberOfWorkers, ageContribution, timeContribution,
                    ref jobData, toolParams, ref rating, ref jobIsValid, cachedJobScores, entityIntelligence, priority, ownerOfJobs, cacheScore);
            }*/
       

            // unknown job???
            throw new Exception("!!");


        }


        /// <summary>
        /// use one agent for all scores
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="job"></param>
        /// <param name="score"></param>
        public static void SetDebugScore(Entity entity, Job job, double score)
        {

#if !RELEASE
            if (entity.Intelligence.Allegiance.AllegianceType == Allegiances.AllegianceType.Player
                && (entity.ID == entity.Intelligence.CurrentExpedition.IndependentMembers[0]))
            {
                job.DebugScore = score;
            }
#endif

        }

        public static void SetDebugScoreNoTools(Entity entity, Job job, double score)
        {

#if !RELEASE
            if (entity.Intelligence.Allegiance.AllegianceType == Allegiances.AllegianceType.Player
                && (entity.ID == entity.Intelligence.CurrentExpedition.IndependentMembers[0]))
            {
                job.DebugScoreNoTools = score;
            }
#endif

        }
        

        public static void ApplyJobPriorityModifier(/*Job job,*/ Priority priority, ref double score)
        {
            if (priority == Jobs.Priority.High)
            {
                score *= GameData.Instance.AIConstants.HighJobModifier;
            }
            else if (priority == Jobs.Priority.Low)
            {
                score *= GameData.Instance.AIConstants.LowJobModifier;
            }
                /*
            else if (priority == Jobs.Priority.Urgent)
            {
                score *= GameData.Instance.AIConstants.UrgentJobModifier;
            }*/
        }

        /* class ToolAndEnergy
         {
             public Entity Tool;

             /// <summary>
             /// fuel items or power cells
             /// </summary>
             public List<Entity> EnergyItems;
         }*/

        /// <summary>
        /// a set of tool or weapon entities that we want to rate
        /// </summary>
        [DebuggerDisplay("Job: {Job}, ToolsAndWeapons: {ToolsAndWeapons}, ImmovableTool: {ImmovableTool}, Score: {Score}")]
        public class ToolOrWeaponInstanceCombo
        {
            // these are set during GetCombos:
            public Job Job;//ProcessJob Job;
            public List<EntityID> ToolsAndWeapons = new List<EntityID>();
          //  public List<EntityAndRoot> ToolsAndWeapons = new List<EntityAndRoot>();
            public EntityID? ImmovableTool; // optional

            
            /// <summary>
            /// for each tool, a list of replenish actions (refuel, recharge..) and replenishment items that belong to that action.
            /// </summary>
            public Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> ReplenishItemsForTools;


            /// <summary>
            /// has productivity info
            /// is null for findprey (and other non-process?) jobs!
            /// </summary>
            public ToolTypeCombination ToolTypeCombination;

            // these data (can be) set during ScoreCombos. they do not depend on the tools, only the job.
            public ToolOrWeaponInstanceComboJobData JobData;
            public AttackType attackType = null;


            public double Score;
        }

       
        public class ToolOrWeaponInstanceComboJobData
        {
            // these (can be) set during ScoreCombos. they do not depend on the tools, only the job.
            public IResourceItem ResourceItem = null;
            public IKnownEntityData InputItem = null;

          //  public IKnownEntityData ImmovableTool = null;
            
            /// <summary>
            /// use this when assigning a location while going through tool combos
            /// </summary>
            public double? HighestToolScore;
            //public EntityID? ImmovableItem; 
            public List<EntityID> /*ToolParams*/ HighestScoringToolCombo; 
            public Vector3? TemporaryGroundLocation;


            public Vector3? Location = null; // ??

            /// <summary>
            /// score for the job, excluding the tool score
            /// </summary>
            public double JobScore;

            
        }


        List<ToolOrWeaponInstanceCombo> allCombos = new List<ToolOrWeaponInstanceCombo>();

        
        /// <summary>
        /// can be either items or structures. they are in 2 collections...
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private bool GetOwnedTools(EntityType entityType, out List<EntityID> items, IKnownEntityData onlyValidToolOfType)
        {
            items = null;
            // List<Entity> items;

            // if immovable tool has already been selected, filter the tool options.
            // for example, if "kitchen" is assigned, reject all immovable tool types other than "kitchen", and reject all other kitchen instances.   
            if (onlyValidToolOfType != null)                
            {
                if (onlyValidToolOfType.EntityType == entityType)
                {
                    Common.AddToList(ref items, onlyValidToolOfType.EntityID);
                    return true;
                }
                else if (ToolType.IsImmovable(onlyValidToolOfType.EntityType)
                    && ToolType.IsImmovable(entityType))
                {
                    return false;
                }                
            }

            if (entityType.ToolType.ToolHandling == ToolHandlingType.Intrinsic) // .IsIntrinsic == true)
            {
                EntityID intrinsicTool;
                if (entityIntelligence.IntrinsicTools != null && entityIntelligence.IntrinsicTools.TryGetValue(entityType, out intrinsicTool))
                {
                    Common.AddToList(ref items, intrinsicTool);
                    return true;
                }
            }
            else if (entityType.ItemType != null && entity.EntityType.IntelligenceType.CanMountTools == true) // NEW: must be able to mount tools
            {
                if (ownerOfJobs.Items.TryGetValue(entityType, out items))
                {
                    return items.Count > 0;
                }

            }
            else if (entityType.StructureType != null)
            {
                if (ownerOfJobs.Structures.TryGetValue(entityType, out items))
                {
                    return items.Count > 0;
                }
            }

            return false;
        }

        private bool GetOwnedWeapons(out HashSet<IKnownEntityData> weapons) //out List<EntityID> items)
        {
            // items = null;
            // List<Entity> items;

            // keep weapons unique:
            weapons = new HashSet<IKnownEntityData>();

            EntityID weaponID;
            IKnownEntityData weapon;
            SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;

            if (ownerOfJobs.WeaponsByAttackType != null)
            {
                foreach (var list in ownerOfJobs.WeaponsByAttackType) // the same weapon may appear under different attack types
                {
                    for (int i = list.Value.Count - 1; i >= 0; i--)
                    {
                        weaponID = list.Value[i];
                        if (!HandleOwnerDataResult(sharedKnowledge, weaponID, ownerOfJobs, out weapon))
                        {
                            continue;
                        }

                        weapons.Add(weapon);
                    }


                    // weapons.AddRange(list.Value);
                }

                return true;
            }

            return false;
        }

        private void GetAllWeaponInstanceCombos(Job job)
        {
            // if EntityToHuntType is null, pick the best weapon?
            // - gather all weapons and score them later

            // if entity type is not null, only gather weapons effective against that type (TODO)

            // List<EntityID> weapons;

            IRequiresWeapon requiresWeapon = job as IRequiresWeapon;

            HashSet<IKnownEntityData> weapons;
           
            if (entity.EntityType.IntelligenceType.CanUseWeapons != false)
            {
                List<EntityGroup> ownersOfWeapons = new List<EntityGroup>(); 
                ownersOfWeapons.Add(ownerOfJobs);

                requiresWeapon.WeaponsAreAvailable = false; // reset, but only by weapon carriers

                if (GetOwnedWeapons(out weapons))
                {                   
                    foreach (var weapon in weapons)
                    {
                        GetWeaponInstanceCombos(job, requiresWeapon, weapon, ownersOfWeapons);

                        /*
                        // range
                        // damage
                        //TODO: Make the weapon attack type selection more sophisticated (as described above?)

                        //Is the current method sophisticated enough?
                       
                        AttackType preferredWeaponAttackType = null;
                        int attackTypeIndex = 0;
                        bool hasFoundValidWeapon = false;

                        double effectivenessScore = weapon.EntityType.ItemType.GetTaskAppropriateLevel(requiresWeapon.TaskType); //ItemType.TaskType.Patrol);
                        if (Common.IsZero(effectivenessScore))
                        {
                            continue;
                        }          

                        do
                        {
                            if (attackTypeIndex >= weapon.EntityType.ItemType.WeaponType.AttackTypes.Length)
                            {
                                break;//No attack type could make this a valid weapon 
                            }

                            preferredWeaponAttackType = weapon.EntityType.ItemType.WeaponType.AttackTypes[attackTypeIndex];//currently we choose the first valid attack type
                            hasFoundValidWeapon = EvaluateAttackJobs.IsValidPlaysiteWeapon(entity, weapon, preferredWeaponAttackType, ownersOfWeapons, cachedWeaponReplenishStates);

                            if (hasFoundValidWeapon)
                            {
                                weaponInstanceCombo = new ToolOrWeaponInstanceCombo() { Job = job };
                                allCombos.Add(weaponInstanceCombo);
                                weaponInstanceCombo.ToolsAndWeapons.Add(weapon.EntityID);
                                weaponInstanceCombo.attackType = preferredWeaponAttackType;

                                requiresWeapon.WeaponsAreAvailable = true;


                                break;
                            }

                            attackTypeIndex++;
                        }
                        while (hasFoundValidWeapon == false);
                        */
                    }
                }
            }

            if (entity.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
            {
                List<EntityGroup> ownersOfWeapons = new List<EntityGroup>();
                ownersOfWeapons.Add(ownerOfJobs); // not needed, must carry internal ammo

                // add intrinsic weapons
                foreach (var item in entityIntelligence.IntrinsicWeapons)
                {
                    Entity weapon = Entity.FindByID(item.Value);
                    if (weapon != null)
                    {
                        GetWeaponInstanceCombos(job, requiresWeapon, weapon, ownersOfWeapons); 
                    }
                }
            }
        }


        private void GetWeaponInstanceCombos(Job job, IRequiresWeapon requiresWeapon, IKnownEntityData weapon, List<EntityGroup> ownersOfWeapons)
        {
            ToolOrWeaponInstanceCombo weaponInstanceCombo;

            // range
            // damage
            //TODO: Make the weapon attack type selection more sophisticated (as described above?)

            //Is the current method sophisticated enough?

            AttackType preferredWeaponAttackType = null;
            int attackTypeIndex = 0;
            bool hasFoundValidWeapon = false;

            double effectivenessScore = weapon.EntityType.ItemType.GetTaskAppropriateLevel(requiresWeapon.TaskType); 
            if (Common.IsZero(effectivenessScore))
            {
                return; // continue;
            }

            do
            {
                if (attackTypeIndex >= weapon.EntityType.ItemType.WeaponType.AttackTypes.Length)
                {
                    break;//No attack type could make this a valid weapon 
                }

                preferredWeaponAttackType = weapon.EntityType.ItemType.WeaponType.AttackTypes[attackTypeIndex];//currently we choose the first valid attack type
                hasFoundValidWeapon = EvaluateAttackJobs.IsValidPlaysiteWeapon(entity, weapon, preferredWeaponAttackType, ownersOfWeapons, cachedWeaponReplenishStates);

                if (hasFoundValidWeapon)
                {
                    weaponInstanceCombo = new ToolOrWeaponInstanceCombo() { Job = job };
                    allCombos.Add(weaponInstanceCombo);
                    weaponInstanceCombo.ToolsAndWeapons.Add(weapon.EntityID);
                    weaponInstanceCombo.attackType = preferredWeaponAttackType;

                    requiresWeapon.WeaponsAreAvailable = true;

                    break;
                }

                attackTypeIndex++;
            }
            while (hasFoundValidWeapon == false);

        }

        private void GetEmptyCombo(Job job)
        {
            ToolOrWeaponInstanceCombo toolInstanceCombo = new ToolOrWeaponInstanceCombo() { Job = job, ToolTypeCombination = null };
            allCombos.Add(toolInstanceCombo);

        }

        /// <summary>
        ///  get all possible tool combos for this process job (construction, harvest, production) with the tools available
        ///  
        /// clean up any outdated entities from the owner
        /// </summary>
        private void GetAllToolInstanceCombos(Job job, out bool jobIsDestroyed)
        {
          //  bool jobHasTools = false;
            ProcessJob processJob = job as ProcessJob;
            if (processJob != null)
            {
                if (processJob.ProcessType.ProcessToolSet != null)
                {
                    //float jobDuration = processJob.GetWorstCaseDurationForEnergyEstimation();

                    SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;

                    IKnownEntityData immovableToolData = null;
                    EntityID? immovableTool;
                    if (!processJob.GetImmovableTool(out immovableTool))
                    {                      
                        jobIsDestroyed = true;  // this should destroy the job
                        return;
                    }

                    if (immovableTool.HasValue) // processJob.ImmovableTool.HasValue)
                    {
                        // if immovable tool has already been selected, filter the tool options.
                        // if "kitchen" is assigned, reject all immovable tool types other than "kitchen", and reject all other kitchen instances.
                        if(EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(immovableTool.Value, out immovableToolData)))
                        {
                            // destroys the job too.
                            jobIsDestroyed = true;
                            return;  
                        }

                    }
                    
                    float baseTimeNeeded = processJob.ProcessType.GetTimeNeeded();
                    float skillProductivity = entity.Intelligence.GetSkillProductionFactor(processJob.ProcessType.RequiredSkillType);
                    float energyProductivity = processJob.ProcessType.GetEnergyProductivity(entity); //null;

                   /* if (entity.BiologicalEntity != null)
                    {
                        energyLevel = entity.BiologicalEntity.EnergyLevel;
                    }*/                    

                    foreach (var toolTypeCombo in processJob.ProcessType.ProcessToolSet.ToolTypeCombinations) // one example combination is: knife + chainsaw, prod. bonus 0.8
                    {
                        // must have all tools in inventory for valid combos 
                        if (HasAllTools(toolTypeCombo.Tools))
                        {

                            float jobDurationInDays = processJob.ProcessType.EstimateTotalDurationInDays(baseTimeNeeded, skillProductivity, energyProductivity, toolTypeCombo.Productivity);
                            
                            // set a limit on 3 types of tools in each set/combo. Reduces complexity quite a lot.
                            int noOfTools = toolTypeCombo.Tools.Count;
                            EntityType toolType0 = null, toolType1 = null, toolType2 = null;

                            toolType0 = toolTypeCombo.Tools[0].Item1;
                            if (noOfTools > 1)
                            {
                                toolType1 = toolTypeCombo.Tools[1].Item1;
                                if (noOfTools > 2)
                                {
                                    toolType2 = toolTypeCombo.Tools[2].Item1;
                                }
                            }

                            // we clean up the owner lists as we go - therefore use reverse for loops!
                            List<EntityID> items0, items1, items2;
                            EntityID? immovableItem = null; // store this info for convenience
                            ToolOrWeaponInstanceCombo toolInstanceCombo = null;
                            bool inUseByNonWorkerProcess;
                            bool anyToolInUse = false;
                            bool anyToolIsBroken = false;

                            if (GetOwnedTools(toolType0, out items0, immovableToolData))
                            {
                                EntityID toolInstance0, toolInstance1, toolInstance2;
                                for (int i = items0.Count - 1; i >= 0; i--)
                                {
                                    toolInstance0 = items0[i];
                                    if (IsValidTool(toolInstance0, jobDurationInDays, sharedKnowledge, ownerOfJobs, out inUseByNonWorkerProcess, ref anyToolInUse, ref anyToolIsBroken))
                                    {
                                        immovableItem = null;
                                        SaveImmovableItem(toolType0, toolInstance0, ref immovableItem);

                                        if (toolType1 != null && GetOwnedTools(toolType1, out items1, immovableToolData))
                                        {
                                            for (int j = items1.Count - 1; j >= 0; j--)
                                            {
                                                toolInstance1 = items1[j];
                                                if (IsValidTool(toolInstance1, jobDurationInDays, sharedKnowledge, ownerOfJobs, out inUseByNonWorkerProcess, ref anyToolInUse, ref anyToolIsBroken))
                                                {
                                                    immovableItem = null;
                                                    SaveImmovableItem(toolType0, toolInstance0, ref immovableItem);
                                                    SaveImmovableItem(toolType1, toolInstance1, ref immovableItem);

                                                    if (toolType2 != null && GetOwnedTools(toolType2, out items2, immovableToolData))
                                                    {
                                                        for (int k = items2.Count - 1; k >= 0; k--)
                                                        {
                                                            toolInstance2 = items2[k];
                                                            if (IsValidTool(toolInstance2, jobDurationInDays, sharedKnowledge, ownerOfJobs, out inUseByNonWorkerProcess, ref anyToolInUse, ref anyToolIsBroken))
                                                            {
                                                                immovableItem = null;
                                                                SaveImmovableItem(toolType0, toolInstance0, ref immovableItem);
                                                                SaveImmovableItem(toolType1, toolInstance1, ref immovableItem);
                                                                SaveImmovableItem(toolType2, toolInstance2, ref immovableItem);

                                                              //  jobHasTools = true;
                                                                toolInstanceCombo = new ToolOrWeaponInstanceCombo() { Job = processJob, ToolTypeCombination = toolTypeCombo, ImmovableTool = immovableItem };
                                                                allCombos.Add(toolInstanceCombo);
                                                                toolInstanceCombo.ToolsAndWeapons.Add(toolInstance0);
                                                                toolInstanceCombo.ToolsAndWeapons.Add(toolInstance1);
                                                                toolInstanceCombo.ToolsAndWeapons.Add(toolInstance2);

                                                            }

                                                        }
                                                    }
                                                    else
                                                    {                                                       
                                                        toolInstanceCombo = new ToolOrWeaponInstanceCombo() { Job = processJob, ToolTypeCombination = toolTypeCombo, ImmovableTool = immovableItem };
                                                        allCombos.Add(toolInstanceCombo);
                                                        toolInstanceCombo.ToolsAndWeapons.Add(toolInstance0);
                                                        toolInstanceCombo.ToolsAndWeapons.Add(toolInstance1);

                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                          //  jobHasTools = true;
                                            toolInstanceCombo = new ToolOrWeaponInstanceCombo() { Job = processJob, ToolTypeCombination = toolTypeCombo, ImmovableTool = immovableItem };
                                            allCombos.Add(toolInstanceCombo);
                                            toolInstanceCombo.ToolsAndWeapons.Add(toolInstance0);
                                        }
                                    }
                                }
                            }

                            if (toolInstanceCombo == null)
                            {
                                if (anyToolInUse)
                                {
                                    bool isStarted;
                                    if (processJob.IsStarted(out isStarted) && !isStarted)
                                    {
                                        processJob.AllToolsInUse = true;
                                    }
                                }
                                else if (anyToolIsBroken)
                                {
                                    bool isStarted;
                                    if (processJob.IsStarted(out isStarted) && !isStarted)
                                    {
                                        processJob.AllToolsAreBroken = true;
                                    }
                                }                                

                            }

                        }


                    }
                }
                else
                {
                  //  jobHasTools = true;//We dont need any tools.                 
                    ToolOrWeaponInstanceCombo toolInstanceCombo = new ToolOrWeaponInstanceCombo() { Job = processJob, ToolTypeCombination = null };
                    allCombos.Add(toolInstanceCombo);
                }

            }
            else
            {
                // non-process jobs
               // jobHasTools = true;//We dont need any tools.             
                ToolOrWeaponInstanceCombo toolInstanceCombo = new ToolOrWeaponInstanceCombo() { Job = job, ToolTypeCombination = null };
                allCombos.Add(toolInstanceCombo);
            }

            jobIsDestroyed = false;
        }

        private void SaveImmovableItem(EntityType entityType, EntityID item, ref EntityID? immovableItem)
        {
            if (ToolType.IsImmovable(entityType))
            {
                immovableItem = item;
            }
        }

        private bool CanCarryAllTools(EntityType tool0, EntityType tool1, EntityType tool2 = null)
        {
            float totalBulk = 0f;

            if (!ToolType.IsImmovable(tool0))
            {
                totalBulk += tool0.ItemType.MaximumBulk.Value;
            }
            if (!ToolType.IsImmovable(tool1))
            {
                totalBulk += tool1.ItemType.MaximumBulk.Value;
            }
            if (!ToolType.IsImmovable(tool2))
            {
                totalBulk += tool2.ItemType.MaximumBulk.Value;
            }

            /*  foreach (var tool in tools)
              {
                  if (!ToolType.IsImmobile(tool.EntityType))
                  {
                      totalBulk += tool.Bulk;
                  }
              }*/

            return entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(totalBulk);
        }

        private bool CanCarryAllTools(Entity tool0, Entity tool1, Entity tool2 = null)
        {
            float totalBulk = 0f;

            if (!ToolType.IsImmovable(tool0.EntityType))
            {
                totalBulk += tool0.Bulk;
            }
            if (!ToolType.IsImmovable(tool1.EntityType))
            {
                totalBulk += tool1.Bulk;
            }
            if (!ToolType.IsImmovable(tool2.EntityType))
            {
                totalBulk += tool2.Bulk;
            }
            /*  foreach (var tool in tools)
              {
                  if (!ToolType.IsImmobile(tool.EntityType))
                  {
                      totalBulk += tool.Bulk;
                  }
              }*/

            return entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(totalBulk);
        }

        private bool CanCarryAllTools(List<Entity> tools)
        {
            float totalBulk = 0f;
            foreach (var tool in tools)
            {
                if (!ToolType.IsImmovable(tool.EntityType))
                {
                    totalBulk += tool.Bulk;
                }
            }

            return entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(totalBulk);
        }


        private bool IsValidTool(EntityID toolID, float? jobDurationInDays, SharedKnowledge sharedKnowledge, EntityGroup ownerOfTools, out bool inUseByNonWorkerProcess, ref bool anyToolInUse, ref bool anyToolIsBroken)
        {
            bool isBroken;
            bool isValid = IsValidPlaysiteTool(toolID, jobDurationInDays, sharedKnowledge, ownerOfTools, out inUseByNonWorkerProcess, out isBroken);

            anyToolInUse = anyToolInUse || inUseByNonWorkerProcess;

            anyToolIsBroken = anyToolIsBroken || isBroken;

            return isValid;
        }

        /// <summary>
        /// see: EvaluateAttackJob.IsValidWeapon
        ///       
        /// </summary>
        /// <param name="toolID"></param>
        /// <param name="jobDurationInDays"></param>
        /// <param name="sharedKnowledge"></param>
        /// <param name="ownerOfTools"></param>
        /// <returns></returns>
        private bool IsValidPlaysiteTool(EntityID toolID, float? jobDurationInDays, SharedKnowledge sharedKnowledge, EntityGroup ownerOfTools, out bool inUseByNonWorkerProcess, out bool isBroken)
        {
            IKnownEntityData tool;
            inUseByNonWorkerProcess = false;
            isBroken = false;

            if (!HandleOwnerDataResult(sharedKnowledge, toolID, ownerOfTools, out tool))
            {
                return false;
            }


            bool toolItemIsOK = true;
            bool isImmobile = ToolType.IsImmovable(tool.EntityType);
            bool isIntrinsic = tool.EntityType.IsIntrinsic();
            bool isMountable = tool.EntityType.IsMountable();
            bool isUpgrade = tool.EntityType.Upgrader != null;

            bool isCarriedByOccupiedAgent = false;
            if (isMountable) //!isImmobile && !isIntrinsic)
            {
                bool itemIsDestroyed;
                isCarriedByOccupiedAgent = EvaluateAttackJobs.IsCarriedByEntityWhoIsOccupied(tool, out itemIsDestroyed); // we don't want agents who are busy dropping their gear.    

                if (itemIsDestroyed)
                {
                    return false;
                }
            }
            
            if (isUpgrade)
            {
                if (tool.ContainedBy.HasValue)
                {
                    // host cannot be broken:
                    IKnownEntityData containerData;
                    if (!HandleOwnerDataResult(sharedKnowledge, tool.ContainedBy.Value, ownerOfTools, out containerData))
                    {
                        return false;
                    }
                    else
                    {
                        if (!Entity.IsFunctional(containerData)) //  EntityIsFunctional(containerData))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            if (!IsInUseByNonWorkerProcess(tool, out inUseByNonWorkerProcess))
            {
                return false;
            }

            if (inUseByNonWorkerProcess) // we cannot cancel non-worker processes...
                return false;

            toolItemIsOK = 
                isImmobile || isIntrinsic
                ||
                (!isCarriedByOccupiedAgent // don't commandeer tools from busy agents
                && tool.PartOfID == null // sometimes tools can also be parts...
                && tool.NotOnboardDrivenVehicle // (itemComponent.OnBoard == null || itemComponent.OnBoard.Vehicle.DrivenBy == null)
                && (entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(tool.Bulk)));

            if (!toolItemIsOK)
                return false;

            bool hasEnergyForTool;
            if (jobDurationInDays.HasValue)
            {
                hasEnergyForTool = HasEnergyForTool(tool, jobDurationInDays.Value, ownerOfJobs, null, cachedToolEnergyAvailableStates, sharedKnowledge);
            }
            else hasEnergyForTool = true;

            isBroken = !Entity.IsFunctional(tool); // !(ScoreIsEntityFunctional(tool) > 0d);

            return hasEnergyForTool
                  && IsOnPlaySite(tool)
                  && !isBroken // ScoreIsEntityFunctional(tool) > 0d 
                  && tool.IsCompleted();

        }


        public static Job ResolveAssignedToJob(IKnownEntityData entityData)
        {
            Job assignedToJob = null;

            if (entityData.AssignedToJob.HasValue)
            {
                assignedToJob = LookUp<Job, JobID>.FindByID(entityData.AssignedToJob.Value);
                if (assignedToJob == null)
                {
                    // cleanup
                    entityData.AssignedToJob = null;
                }               
            }

            return assignedToJob;
        }

        public static bool ResolveAssignedToProcessJob(IKnownEntityData entityData, out ProcessJob processJob)
        {
            Job assignedToJob = null;
            processJob = null;
            if (entityData.AssignedToJob.HasValue)
            {
                assignedToJob = LookUp<Job, JobID>.FindByID(entityData.AssignedToJob.Value);
                if (assignedToJob == null)
                {
                    // cleanup
                    entityData.AssignedToJob = null;
                }
                else
                {
                    processJob = assignedToJob as ProcessJob;

                    return processJob != null;
                }
            }

            return false;
        }

        public static bool IsInUseByNonWorkerProcess(IKnownEntityData tool, out bool isInUseByNonWorkerProcess)
        {
            isInUseByNonWorkerProcess = false;
            if (tool.AssignedToJob != null)
            {
                // exclude tools in use for non-worker processes (we never cancel those after they have started... there is no good way to score them.)
                ProcessJob processJob; // = tool.AssignedToJob as ProcessJob;
                if (ResolveAssignedToProcessJob(tool, out processJob)) // processJob != null)
                {
                    //if (!processJob.ProcessType.RequiresWork)
                    if (processJob.ProcessType.WorkNeeded != WorkerNeededOptions.WorkerNeeded)
                    {
                        bool isStarted;
                        if (processJob.IsStarted(out isStarted))
                        {
                            if (isStarted)
                            {
                                isInUseByNonWorkerProcess = true;
                            }
                        }
                        else
                        {
                             return false;
                        }
                    }
                }
            }

            return true;
        }

        /* private bool IsToolStillValid(Entity tool)
         {

         }*/

        private bool HasAllTools(List<Tuple<EntityType, float>> listOfTools) //List<EntityType> listOfTools)
        {
            List<EntityID> items;

            foreach (var tool in listOfTools)
            {
                if (tool.Item1.IsMountable() && entity.EntityType.IntelligenceType.CanMountTools != true)
                {
                    return false;
                }
                else if (tool.Item1.ToolType.ToolHandling == ToolHandlingType.Intrinsic // IsIntrinsic == true
                    && (entityIntelligence.IntrinsicTools == null || !entityIntelligence.IntrinsicTools.ContainsKey(tool.Item1)))
                {
                    return false;
                }
                else if (!GetOwnedTools(tool.Item1, out items, null))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// in order to cut down on tool combinations, check to see if our inventory supports replenishment of this tool
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="jobDurationInDays"></param>
        /// <returns></returns>
        public static bool HasEnergyForTool(IKnownEntityData tool, float jobDurationInDays, EntityGroup ownerOfItems, List<EntityGroup> ownersOfItems, Dictionary<EntityType, ReplenishStatus> cachedToolEnergyAvailableStates, SharedKnowledge sharedKnowledge)
        {
            //   RequiresEnergy energy;
            bool hasEnergy = true;
            /*  if (tool.Find(out energy))
              {*/

            hasEnergy = tool.HasEnergyForDuration(jobDurationInDays);

            if (!hasEnergy)
            {
                // see if we can refuel the tool:

                //bool hasEnergyForToolType;
                ReplenishStatus replenishStatus;
                if (!cachedToolEnergyAvailableStates.TryGetValue(tool.EntityType, out replenishStatus)) 
                {
                    replenishStatus = new ReplenishStatus() { DurationInDays = jobDurationInDays };

                    // handle single/multiple ownership lists                  
                    replenishStatus.OwnsItem = HasEnergyForToolType(tool.EntityType, jobDurationInDays, ownerOfItems, ownersOfItems, sharedKnowledge);                  

                    hasEnergy = replenishStatus.OwnsItem.Value;

                    cachedToolEnergyAvailableStates.Add(tool.EntityType, replenishStatus); 
                }
                else
                {
                    // we already cached a value
                    // see if the duration can still be fulfilled:
                    if (jobDurationInDays <= replenishStatus.DurationInDays)
                    {
                        // we can still use this status:
                        hasEnergy = replenishStatus.OwnsItem.Value;
                    }
                    else
                    {
                        // the new duration is longer. recompute:
                        replenishStatus.OwnsItem = HasEnergyForToolType(tool.EntityType, jobDurationInDays, ownerOfItems, ownersOfItems, sharedKnowledge);
                        
                        replenishStatus.DurationInDays = jobDurationInDays;

                        cachedToolEnergyAvailableStates[tool.EntityType] = replenishStatus; // struct

                    }
                }               
            }
            
            return hasEnergy;
        }

     /*   private static void UpdateExpeditionOwnersWithReplenishStatus(EntityType entityType, Owner singleOwner, List<Owner> listOfOwners, bool hasEnergy)
        {
            if (singleOwner != null)
            {
                Expedition expedition = singleOwner.GetExpedition();
                if (expedition != null)
                {
                    expedition.SetToolReplenishStatus(entityType, hasEnergy);
                }
                //replenishStatus.OwnsItem = HasEnergyForToolType(tool.EntityType, jobDuration, ownerOfItems, sharedKnowledge);
            }
            else
            {
                foreach (var item in listo)
                {
                    
                }
               // replenishStatus.OwnsItem = HasEnergyForToolType(tool.EntityType, jobDuration, ownersOfItems, sharedKnowledge);
            }
            
        }*/

        public static bool HasEnergyForToolType(EntityType tool, float durationInDays, EntityGroup ownerOfItem, List<EntityGroup> ownersOfItems, SharedKnowledge sharedKnowledge)
        {
            // can handle single/multiple ownership lists            

            if (ownerOfItem != null)
            {
                if (HasEnergyForToolType(tool, durationInDays, ownerOfItem, sharedKnowledge) == true)
                {
                    return true;
                }
            }

            if (ownersOfItems != null)
            {
                foreach (var owner in ownersOfItems)
                {
                    if (owner.Items.Count > 0)
                    {
                        bool hasEnergy = HasEnergyForToolType(tool, durationInDays, owner, sharedKnowledge);

                        if (hasEnergy)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// can we supply a tool of this type with energy?
        ///     
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="durationInDays"></param>
        /// <returns></returns>
        public static bool HasEnergyForToolType(EntityType tool, float durationInDays, EntityGroup ownerOfItems, SharedKnowledge sharedKnowledge, bool giveClientFeedback = true)
        {
            bool hasEnergy = false;

            if (tool.ContainerType != null && tool.ContainerType.GetRequiresReplenishType() != null)  //.RequiresEnergyType != null)
            {
                // TODO: expand this...
                RequiresFuelType requiresFuelType = tool.ContainerType.GetRequiresReplenishType().RequiresFuelType;

                if (requiresFuelType != null)
                {
                    float neededFuelBulk = (float)(requiresFuelType.BurnRatePerDay * durationInDays); // * The.Sim.DateAndTime.DaysPerSecond);

                    List<EntityID> items;
                    EntityID itemID;
                    IKnownEntityData itemData;
                    // find energy items:
                    float availableFuelBulk = 0f;
                    foreach (var item in requiresFuelType.FuelEntityTypes)
                    {

                        if (ownerOfItems.Items.TryGetValue(item, out items))
                        {
                            for (int i = items.Count - 1; i >= 0; i--)
                            {
                                itemID = items[i];

                                if (HandleOwnerDataResult(sharedKnowledge, itemID, ownerOfItems, out itemData)
                                    && itemData.IsCompleted()
                                    && Entity.IsOnPlaySite(itemData))
                                {
                                    availableFuelBulk += itemData.Bulk;

                                    if (availableFuelBulk >= neededFuelBulk)
                                    {
                                        hasEnergy = true;

                                        break;
                                        //return true;
                                    }
                                }
                            }
                        }

                        if (hasEnergy)
                        {
                            // break out of the outer loop too:
                            break;
                        }
                    }

                    if (availableFuelBulk >= neededFuelBulk)
                    {
                        hasEnergy = true;
                        //return true;
                    }
                    else
                    {
                        hasEnergy = false;
                        //return false;
                    }

                    if (giveClientFeedback)
                    {
                        Expedition expedition = ownerOfItems.GetExpedition();
                        if (expedition != null)
                        {
                            The.Client.SetToolReplenishStatusItemOwned(expedition, tool, hasEnergy, neededFuelBulk);
                        }
                    }

                    return hasEnergy;

                }
            }

            return true;
        }

        private void SetGatherJobTypeComboAsAlreadyAdded(Job job)
        {
            ProcessJob processJob = job as ProcessJob;
            if (processJob != null && processJob.HarvestJob != null)
            {
                gatherJobTypeComboAlreadyAdded[processJob.HarvestJob.ResourceType.ResourceItemType] = true;
            }
        }

        private bool SkipCombos(ProcessJob harvestJob)
        {           
            if (harvestJob.HarvestJob != null)
            {
                if (!harvestJob.HarvestJob.CanBeQueuedInGoal())
                {  
                    // this makes it possible to gather more than one marshcot sap at a time, by each agent. Queuing in the goal is disabled for unattended gather processes...              
                    return false;
                }
                else
                {
                    // we can skip these because GoalHarvest will queue up more jobs when it executes.
                    return gatherJobTypeComboAlreadyAdded.ContainsKey(harvestJob.HarvestJob.ResourceType.ResourceItemType);
                }
            }

            return false;
        }

        // private enum JobTypeToHandle { ProductionJob, OtherJob }
        // OLD:
        /*  private CalculateResult HandleJob(Jobs.Job job, double? ageContribution, double? timeContribution)
          {

              if (!entityIntelligence.Brain.IsSame(typeof(GoalConstruct), job)) // don't consider a job we are already doing!
              // goal type is don't care in this case. 
              {

                  RegionMap regionMapToUse = GetRegionMapToUseForEntity(entity);

                  double currentRating;

                  int proposedNumberOfWorkers = Common.Clamp(job.TakenBy.Count + 1, 0, job.MaxJobPositions);
                  if (ScoreThisJob(regionMapToUse, entity, job, proposedNumberOfWorkers, ageContribution, timeContribution, out currentRating) == CalculateResult.Processing)
                  {
                      return CalculateResult.Processing;
                  }
                            
                  if (currentRating > mostDesirableScore)
                  {
                      // TODO: introduce a waiting period before starting a goal. During that period, anyone with a higher score may take the job. Outside it, apply the required difference as now.
                      double utilityToTest = currentRating - GameData.Instance.AIConstants.AmountNewGoalMustBeBetterThanOtherEntityToCancel;

                      // if job roster is full, is our score better than the current takers'?
                      if (job.TakenBy.Count < job.MaxJobPositions || job.TakenBy.IsScoreGreaterThanAnyTaker(utilityToTest))
                      {   
                          bool okToTakeJob = true;
                          ProcessJob processJob = job as ProcessJob;
                          if (processJob.HarvestJob != null)
                          {
                              // for gather jobs, if the crop/resource has been taken, we must also score higher than the taker:
                              if (currentResourceItem.AssignedToJob == null
                              || currentResourceItem.AssignedToJob.TakenBy.Count == 0
                              || (currentResourceItem.AssignedToJob.TakenBy.Count > 0 && currentResourceItem.AssignedToJob.TakenBy.IsScoreGreaterThanAnyTaker(utilityToTest))) // TakenBy.IsScoreGreater(utilityToTest, currentResourceItem.AssignedToJob.TakenBy)))// .TargetedForHarvestingBy)))
                              {
                                  okToTakeJob = true;
                              }
                              else
                              {
                                  okToTakeJob = false;
                              }
                          }

                          if (okToTakeJob)
                          {
                              mostDesirableScore = currentRating;
                              bestJob = job;

                              bestResourceItem = currentResourceItem; // only relevant for gather job
                              bestInputItem = currentInputItem; // only relevant for process jobs without a job location
                              bestLocation = currentLocation;
                          }
                                                           
     
                      }
                  }

                  // for debugging feedback. also include hauling jobs?
                  entityIntelligence.TopScoringJobs.Add(new GoalAndScore() { Score = currentRating, Goal = job.ToString() });
              }

              return CalculateResult.Done;
          
          }*/

        //private static bool Job








        /*
        private static RegionMap.Result GetBestHarvestLocation(Entity entity, HarvestJob job, out Vector3 bestLocation)
        {
            bestLocation = Vector3.Zero;

         

            CropsMapForAgentRequest.GetBestHarvestLocation(entity, job);

        }*/

        /*
        public CalculateResult ScoreThisProductionJob(Entity entity, Jobs.Job job, int proposedNumberOfWorkers, out double currentRating)
        {

           currentRating = 0;
            ProductionJob pJob = job as ProductionJob;

            if (pJob != null) 
            {
                if (personEntity.HasSkillForJob(pJob.ProducerType.RequiredSkill)) //pJob))
                {
                    double accessibleEstimate = EstimateIsAccessible(entity, pJob.ProductionSite.MapPosition);
                    if (Common.IsEqual(accessibleEstimate, 0))
                    {   // if not accessible, skip this.
                        currentRating = 0;
                    }
                    else
                    {
                        double travelTimeScore = ScoreTravelTime(pJob.ProductionSite.Location, entity.Location, entity);

                        double materialsEstimate = EstimatePowerAndMaterialsReady(pJob, entity);

                        double progressScore = ScoreJobProgress(pJob.Progress);

                        double marginalLaborScore = ScoreNumberOfWorkers(proposedNumberOfWorkers, pJob.MaxJobPositions);

                        double skillScore = ScoreSkill(entity.PersonEntity.Skills[pJob.ProducerType.RequiredSkill].Value);

                        // weigh ready materials higher than distance to site:
                        if (Common.IsEqual(materialsEstimate, 0))
                        {
                            currentRating = GameData.Instance.AIConstants.JobWithZeroMaterialsDesirability;
                        }
                        else
                        {
                            currentRating = 0.2 * travelTimeScore + 0.4 * materialsEstimate +
                                0.1 * progressScore + 0.1 * marginalLaborScore + 0.2 * skillScore;
                        }

                    }
                    //return currentRating;
                }

            }

            HarvestJob harvestJob = job as HarvestJob;
            if (harvestJob != null)
            {
                if (personEntity.HasSkillForJob(harvestJob.CropType.HarvestType.RequiredSkill))
                {
                   
                    double travelTimeScore = ScoreTravelTime(pJob.ProductionSite.Location, entity.Location, entity);
                    

                    double skillScore = ScoreSkill(entity.PersonEntity.Skills[harvestJob.CropType.HarvestType.RequiredSkill].Value);

                    
                    //currentRating = 0.2 * travelTimeScore + 0.4 * materialsEstimate +
                    //        0.1 * progressScore + 0.1 * marginalLaborScore + 0.2 * skillScore;
                    //
                    currentRating = 0.8 * travelTimeScore + 0.2 * skillScore;
                    
                   // return currentRating;
                }

                // we don't need to look at other similar harvest jobs:
                uniqueProcessedJobs.Add(harvestJob.CropType.CropItem, harvestJob);

               // return currentRating;
            }

           // return currentRating;
        }*/


        /// <summary>
        /// replace the entity collections with ones using EntityIDs
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private Dictionary<EntityAndRoot, List<ReplenishItemsForAction>>
            GetReplenishItemsEntityIDs(Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> items)
        {
            if (items != null)
            {
                Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> dict = new Dictionary<EntityAndRoot, List<ReplenishItemsForAction>>();

                foreach (var tool in items)
                {
                    List<ReplenishItemsForAction> listOfActions = new List<ReplenishItemsForAction>();
                    foreach (var action in tool.Value)
                    {
                        List<EntityID> listOfIDs = action.Item2.Select(e => e.EntityID).ToList();

                        ReplenishItemsForAction listOfReplenishItems = new ReplenishItemsForAction(action.Item1, listOfIDs);

                        listOfActions.Add(listOfReplenishItems);
                    }

                    dict.Add(tool.Key.GetAsEntityAndRoot() /* .EntityID*/, listOfActions);
                }

                return dict;
            }
            else return null;

        }

        public override bool CancelCurrentTakers()
        {

            /*  
             * ProcessJob processJob;
#if !RELEASE
            List<Entity> needsToBeCancelledCopy = new List<Entity>();

            processJob = this.bestCombo.Job as ProcessJob;


            if (needsToBeCancelled != null && needsToBeCancelled.Count > 0)
            {
                needsToBeCancelledCopy.AddRange(needsToBeCancelled);
                                
                if (processJob != null && processJob.ProcessType.KeyName == "makePowderedCrystalBerries")
                {

                }

            }
#endif*/

            bool result = CancelEntities(needsToBeCancelled, itemsToBeDropped);

            /*
#if !RELEASE
          
            if (processJob != null)
            {
                // assert that all non-immovable tools have been unassigned:
                foreach (var item in processJob.AssignedTools)
                {
                    IKnownEntityData toolData;
                    entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(item, out toolData);
                    if (toolData != null)
                    {
                        if (!ToolType.IsImmovable(toolData.EntityType))
                        {
                            throw new Exception("Tool still assigned? " + needsToBeCancelled.Count);
                        }
                    }
                }                
            }

#endif
            */

            return result;
           
        }

        public static bool IsJobValid(Job job)
        {
            if (job.ID == JobID.Invalid)
            {
                return false;
            }
            return true;
        }

        public override bool CanTakeGoal()
        {
            
            if (bestCombo != null)
            {
                if (EvaluateJob.IsJobValid(bestCombo.Job) == false)
                {
                    return false;
                }
                needsToBeCancelled.Clear();
                itemsToBeDropped.Clear();

                if (GetJobsToCancel(bestCombo))
                {

                    if (IsScoreBetterThanAllInvolveds(bestCombo.Score, needsToBeCancelled))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void PreSetGoal()
        {
            base.PreSetGoal();


            // prevent dropping hauled items that we need:
            if (bestCombo != null)
            {
                if (bestCombo.ToolsAndWeapons != null)
                {
                    foreach (var item in bestCombo.ToolsAndWeapons)
                    {
                        entityIntelligence.Memory.SetNeededItemForSwitchedGoal(item);
                    }                   
                }

                if (bestCombo.ReplenishItemsForTools != null)
                {
                    foreach (var item in bestCombo.ReplenishItemsForTools)
                    {
                        foreach (var item2 in item.Value)
                        {
                            foreach (var item3 in item2.Item2)
                            {
                                entityIntelligence.Memory.SetNeededItemForSwitchedGoal(item3.EntityID);
                            }
                        }
                    }
                }
            }
        }

        public override bool SetGoal()
        {
            base.SetGoal();

            if (bestCombo != null)
            {               
                Job bestJob = bestCombo.Job;               
                           
             
                IKnownEntityData toolData;
                List<IKnownEntityData> listOfToolData = new List<IKnownEntityData>();
                foreach (var tool in bestCombo.ToolsAndWeapons)
                {
                    entityIntelligence.GetKnownData(tool, out toolData);

                    if (toolData != null)
                    {
                        listOfToolData.Add(toolData);
                    }
                    else
                    {
                        return false;
                    }
                }


                ProcessJob processjob = bestJob as ProcessJob;
                if (processjob != null)
                {                    

                    if (processjob.BuildingJob != null)
                    {
                        entityIntelligence.SetTopLevelGoal(new GoalConstruct(entity, processjob, GetOwnerIDs(vehicleGroups), bestCombo.ToolsAndWeapons,
                            GetReplenishItemsEntityIDs(bestCombo.ReplenishItemsForTools), bestCombo.ToolTypeCombination) { GoalEvaluator = this }, bestScore);
                      
                    }
                    else if (processjob.HarvestJob != null)
                    {

#if !RELEASE
                        if (bestCombo.ToolsAndWeapons != null)
                        {
                            foreach (var item in bestCombo.ToolsAndWeapons)
                            {
                                Entity itemAsEntity = Entity.FindByID(item);
                                if (itemAsEntity != null && itemAsEntity.AssignedToJob.HasValue && itemAsEntity.AssignedToJob != processjob.ID)
                                {
                                    bool inUseByUnattendedProcess;
                                    if (IsInUseByNonWorkerProcess(itemAsEntity, out inUseByUnattendedProcess)
                                        && inUseByUnattendedProcess)
                                    {
                                        // should not be allowed...
                                    }
                                }
                            }
                        }
#endif


                        entityIntelligence.SetTopLevelGoal(
                            new GoalHarvest(entity, processjob, ownerOfNewProducts, GetOwnerIDs(vehicleGroups),
                                bestCombo.ToolsAndWeapons, GetReplenishItemsEntityIDs(bestCombo.ReplenishItemsForTools), bestCombo.ToolTypeCombination) { GoalEvaluator = this }, bestScore);
                       
                    }
                    else
                    {
                        EntityID? inputItem = null;
                        if (bestCombo.JobData.InputItem != null)
                        {
                            inputItem = bestCombo.JobData.InputItem.EntityID;
                        }

                       
                        entityIntelligence.SetTopLevelGoal(
                                new GoalProduce(entity, processjob, ownerOfNewProducts, GetOwnerIDs(vehicleGroups),
                                    bestCombo.ToolsAndWeapons,
                                    GetReplenishItemsEntityIDs(bestCombo.ReplenishItemsForTools), 
                                    bestCombo.ToolTypeCombination,
                                    inputItem) 
                                    { GoalEvaluator = this }, 
                                bestScore);
                                              
                    }
                }
                else if (bestJob is FindPreyJob)
                {
                    FindPreyJob bestFindPreyJob = bestJob as FindPreyJob;
                    entityIntelligence.SetTopLevelGoal(new GoalFindPrey(entity, bestFindPreyJob, GetOwnerIDs(vehicleGroups), listOfToolData[0].GetAsEntityAndRoot(), // bestCombo.ToolsAndWeapons[0], 
                        GetReplenishItemsEntityIDs(bestCombo.ReplenishItemsForTools), bestCombo.JobData.Location.Value) { GoalEvaluator = this }, bestScore);
                   
                }             
                else if (bestJob is CombatAreaJob)
                {
                    CombatAreaJob bestFindPreyJob = bestJob as CombatAreaJob;
                    entityIntelligence.SetTopLevelGoal(new GoalPatrol(entity, bestFindPreyJob, GetOwnerIDs(vehicleGroups), listOfToolData[0].GetAsEntityAndRoot(), //bestCombo.ToolsAndWeapons[0], 
                        GetReplenishItemsEntityIDs(bestCombo.ReplenishItemsForTools), bestCombo.JobData.Location.Value) { GoalEvaluator = this }, bestScore);

                }
                else if (bestJob is CheckProcessJob)
                {
                    CheckProcessJob checkJob = bestJob as CheckProcessJob;
                    entityIntelligence.SetTopLevelGoal(new GoalChecking(entity, checkJob, GetOwnerIDs(vehicleGroups)) { GoalEvaluator = this }, bestScore);

                }

                return true;
            }

            return false;
        }

    }
}
