using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Body;
using GameStateManagement;
using System.Linq;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.AI.Constants;
namespace UWGame.SimSide.AI.Goals
{
    public struct HaulingCombo
    {
        public HaulingJob Job;


        //   public IKnownEntityData Item;
        public EntityID Item;
        public EntityID? Vehicle;
        public double Score; // = -1.0;
    }

    public class EvaluateHaulingJobs : GoalEvaluator, IScoreJob
    {
        private HaulingJob mostDesirableJob = null;
        // private double mostDesirableScore = 0; // use bestScore instead?

        //  private IKnownEntityData bestItem = null;
        private EntityID? bestItem;
        //  private IKnownEntityData bestVehicle;
        private EntityID? bestVehicle;

        /// <summary>
        /// a list of agents in our allegiance
        /// </summary>
        private List<Entity> needsToBeCancelled = new List<Entity>();
        /// <summary>
        /// items carried by agents in our allegiance - always seen
        /// </summary>
        private List<Entity> itemsToBeDropped = new List<Entity>();

        //  private Dictionary<Entity, Entity> itemsCarriedByUsWhichWeHaveConsideredHauling = new Dictionary<Entity, Entity>();
        private List<HaulingCombo> haulingCombos = new List<HaulingCombo>();

        static double oneOverMaxHaulingTime;

        private EntityGroup ownerOfJobs;
        //   public Owner OwnerOfItemsToHaul; 

        //*************
        // progress variables for timeslicing/interrupt and continue:
        private enum Progress { NotStarted, GetCombos, ScoreCombos }
        private Progress progress = Progress.NotStarted; // Progress.GetCombos;
        // these are stored between interruptions.
        private double ageContribution = 1;
        double timeOfDayContribution = 0;

        private int haulingComboProgress = 0;
        private bool scoringWasInterrupted = false; // this keeps track of us getting interrupted and then having to resume in a later frame.

        //****************

        public List<EntityGroup> OwnersOfVehicles;

        //    public Owner NewOwner;

        /*      public EvaluateHaulingJobs(JobCollection jobs, ItemCollection items)
              {
                  maxHaulingTimeSquared = Common.DistanceSquared(new Point(0, 0), new Point(UWGame.SimSide.Instance.map.noOfTilesToDisplayHorizontally, UWGame.SimSide.Instance.map.noOfTilesToDisplayVertically))
                                                      / Entity.CommonLowestHaulingSpeed;

                  this.JobCollection = jobs;
                  this.EvaluatorItems = items;
              }*/

        private float priority;
        public override float Priority
        {
            get
            {
                return priority;
            }
        }

        static EvaluateHaulingJobs()
        {
            // we want it to be pixel distances now...
            oneOverMaxHaulingTime = 1f / (MapManager.tileSize * maxMapOctileDistance / Locomotor.CommonLowestHaulingSpeed);
        }

        public EvaluateHaulingJobs(Entity entity,
            EntityGroup ownerOfJobs,
            EntityGroup newOwner, List<EntityGroup> ownersOfVehicles) //, bool isColonyWork)
            : base(entity)
        {
            // this.NewOwner = newOwner;
            this.OwnersOfVehicles = ownersOfVehicles;

            //  this.OwnerOfItemsToHaul = ownerOfItems;
            this.ownerOfJobs = ownerOfJobs;

            //  IsColonyWork = isColonyWork;

            this.priority = GameData.Instance.AIConstants.PriorityOfHauling; //1f;

        }

        private static double ScoreNeededForTrade(IKnownEntityData item, Jobs.HaulingJob haulingJob)
        {
            if (haulingJob.IsToTradeOfferStorage)
            {
                return 1.0;
            }

            return 0;
        }

        public static double ScoreJobMaterialUrgency(Jobs.HaulingJob haulingJob, EntityGroup owner)
        {

            // see if the hauling job is needed in a process:
            if (haulingJob.RequiredByProcessJob != null)
            {
                return haulingJob.RequiredByProcessJob.GetImportance(owner); // 1.0; // -haulingJob.RequiredByProcessJob.MaterialsScore[item.EntityType];
            }

            return 0; // no urgency.
        }


        /// <summary>
        /// implements IScoreJob for dynamic re-evaluation!
        /// </summary>       
        public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStance, Entity entity, Jobs.Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution,
           out double rating, ToolParams? toolParams, AttackParams? attackParams, HaulingParams? haulingParams = null) // List<Entity> tools, float? toolsetProductivity, Dictionary<EntityType, ReplenishStatus> availableFuel)
        {
            IKnownEntityData item = null;
            RegionMap regionMapToUse;
            ThreatStance threatStanceToUse;

            if (haulingParams.Value.Item == (EntityID)38)
            {

            }

            if (haulingParams.Value.Item == (EntityID)39)
            {

            }

            //some hauling jobs require Bold stance!
            if (!CanTakeStanceForJob(job, regionMap, threatStance, out regionMapToUse, out threatStanceToUse))
            {
                rating = 0;
                return CalculateResult.Done;
            }

            HaulingJob haulingJob = job as HaulingJob;

            if (haulingJob.IsCompleted)
            {
                rating = 0;
                return CalculateResult.Done;
            }


            if (!ageContribution.HasValue)
            {
                ageContribution = GetAgeContribution();
            }

            if (!timeContribution.HasValue)
            {
                timeContribution = ScoreTimeOfDay();
            }

            double time = 0;
            rating = 0d;

            if (haulingParams.Value.Vehicle != null)
            {
                if (EstimateHaulingTimeByVehicle(entity, haulingParams.Value.Item, haulingParams.Value.Vehicle, haulingJob.GetToStorageEntity /*ToStorageEntity*/, haulingJob.ToLocation, regionMapToUse, ref time) == CalculateResult.Processing)
                {
                    // wait for result!
                    return CalculateResult.Processing;
                }
            }
            else
            {
                if (EstimateHaulingTimeByFoot(entity, haulingParams.Value.Item, haulingJob, regionMapToUse, ref time, out item) == CalculateResult.Processing)
                {

                    return CalculateResult.Processing;
                }
            }

            if (time < 0)
            {
                // no access...
                rating = 0;
            }
            else
            {
               
                double urgencyScore = 0;
 
                // only one of these will be filled:
                double materialUrgencyScore = ScoreJobMaterialUrgency(haulingJob, ownerOfJobs);
                if (Common.IsZero(materialUrgencyScore))
                {
                    double neededForTrade = ScoreNeededForTrade(item, haulingJob);
                    urgencyScore = neededForTrade;
                }
                else
                {
                    urgencyScore = materialUrgencyScore;
                }
               

                // hauling jobs should in general be lower priority than all other types of jobs, even low importance process jobs. 
                //(use this factor to control by how much)
                double dummyFactor = 0d;

                // take care when we compare on foot score to vehicle score...

                // TODO: use normal value as default
                double valueScore = item.EntityType.ItemType.GetHauledItemValueModifier();

                double travelTimeScore = ScoreTravelTime(time, oneOverMaxHaulingTime);

                double memoryScore = ScoreRecentlyHauledItem(item); // Causes dithering!!!  needs a pretty big weight...

                double timePassedScore = ScoreTimePassed(haulingJob); 

               /* double importanceScore = 0;
                if (haulingJob.RequiredByProcessJob != null)
                {
                    importanceScore = haulingJob.RequiredByProcessJob.GetImportance(ownerOfJobs);
                }*/

                EvaluatorWeights weights = GameData.Instance.AIConstants.EvaluatorWeights;

                if (haulingParams.Value.Vehicle == null)
                {                    
                   // rating = 0.35 * travelTimeScore + 0.2 * urgencyScore + 0.2 * memoryScore + 0.15 * dummyFactor + 0.10 + 0.05f * valueScore;
                    rating = 
                        travelTimeScore * weights.HaulJobTravelWeight + //  0.3 
                        urgencyScore * weights.HaulJobUrgencyWeight +  //  0.2 
                        memoryScore * weights.HaulJobMemoryWeight + //  0.2 
                        //0.15 * dummyFactor +
                        0.1 * dummyFactor +
                        timePassedScore * weights.HaulJobStarvationWeight + // 0.1 NEW - reduce travel score since this should affect far away items
                        weights.HaulJobAddend + //   0.15 + 
                        0.05f * valueScore;
                }
                else
                {                    
                    double vehicleConditionScore = ScoreIsEntityFunctional(haulingParams.Value.Vehicle);
                    
                   // rating = 0.4 * travelTimeScore + 0.2 * urgencyScore + 0.2 * vehicleConditionScore + 0.15 * dummyFactor + 0.05f * valueScore;
                    rating =
                        travelTimeScore * weights.HaulJobTravelWeight + //  0.35 
                        urgencyScore * weights.HaulJobUrgencyWeight +  //  0.2 
                        memoryScore * weights.HaulJobMemoryWeight + //  0.2 
                        timePassedScore * weights.HaulJobStarvationWeight +
                        0.15 * vehicleConditionScore + 
                        0.1 * dummyFactor + 
                        0.05f * valueScore;
                }

                // Lars: looks very hackish... can we delete this?
                double recentlyHuntedCarcassScore = entity.Intelligence.Memory.GetRecentlyHuntedCarcassScore(haulingParams.Value.Item);
                if (recentlyHuntedCarcassScore != 0.0)
                {
                    rating = rating * (1.0f - GameData.Instance.AIConstants.RecentlyKilledCarcassScoreFraction) + recentlyHuntedCarcassScore * GameData.Instance.AIConstants.RecentlyKilledCarcassScoreFraction;
                }

                rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, Priority);

                if (haulingJob.RequiredByProcessJob != null)
                {
                    EvaluateJob.ApplyJobPriorityModifier(haulingJob.RequiredByProcessJob.Priority, ref rating);

                }
                else
                {
                   
                    HaulingJobSpecificItem haulSpecific = haulingJob as HaulingJobSpecificItem;
                    if (haulSpecific != null && haulSpecific.IsHaulJobToStorage)
                    {
                        EvaluateJob.ApplyJobPriorityModifier(haulSpecific.Priority, ref rating); // set priority on the job instead of pulling it
                       // EvaluateJob.ApplyJobPriorityModifier(entityIntelligence.CurrentExpedition.Policy.HaulToStoragePriority, ref rating);
                    }
                }

            }


            EvaluateJob.SetDebugScore(entity, haulingJob, rating);


            return CalculateResult.Done;

        }

        private double ScoreRecentlyHauledItem(IKnownEntityData entityData) // EntityID entityID)
        {
            // for non-hauled items only. replaces inertia.
          /*  if (!IsCurrentlyHaulingItem(entityData)) // causes dithering when picking up??? much larger than inertia... it is hard to match inertia with this.
            {*/
                return entityIntelligence.Memory.GetRecentlyHauledItemScore(entityData.EntityID);
           /* }

            return 0d;*/
        }

        /// <summary>
        /// prevent starvation of far-away items
        /// </summary>
        /// <returns></returns>
        private double ScoreTimePassed(HaulingJob job)
        {
            return job.ScoreTimePassed();
        }

        private bool IsCurrentlyHaulingItem(IKnownEntityData entityData)
        {
            if (entityData.AssignedToJob.HasValue)
            {
                Job job = LookUp<Job, JobID>.FindByID(entityData.AssignedToJob.Value);
                if (job != null)
                {
                    HaulingJob haulingJob = job as HaulingJob;
                    if (haulingJob != null)
                    {
                        if (haulingJob.TakenBy.Contains(entity))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }



        /// <summary>
        /// NOTE: does not include age and time contributions, and does not factor in Priority.
        /// </summary>
        /// <param name="regionMap"></param>
        /// <param name="entity"></param>
        /// <param name="job"></param>
        /// <param name="item"></param>
        /// <param name="vehicle"></param>
        /// <param name="result"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        /* public static CalculateResult ScoreThisJob(RegionMap regionMap, Entity entity, Jobs.HaulingJob job, EntityID item, IKnownEntityData vehicle, ref double result, float priority)
         {
             double time = 0;
                      
             if (vehicle != null)
             {
                 if (EstimateHaulingTimeByVehicle(entity, item, vehicle, job.ToStorageEntity, job.ToLocation, regionMap, ref time) == CalculateResult.Processing)
                 {  
                     // wait for result!
                     return CalculateResult.Processing;
                 }
             }
             else
             {
                 if (EstimateHaulingTimeByFoot(entity, item, job.ToStorageEntity, job.ToLocation, regionMap, ref time) == CalculateResult.Processing)
                 {
                     return CalculateResult.Processing;
                 }
             }

             if (time < 0)
             {
                 // no access...
                 result = 0;
             }
             else
             {
                 // production input stages has been dropped. but perhaps we still need to judge urgency some other way?
                 double materialUrgencyScore = 0.1d; // ScoreJobMaterialUrgency(item, job);

                 // hauling jobs should in general be lower priority than all other types of jobs. use this factor to control by how much:
                 double dummyFactor = 0d;

                 // take care when we compare on foot score to vehicle score...

                
                 if (vehicle == null)
                 {
                     double travelTimeScore = ScoreTravelTime(time, oneOverMaxHaulingTime);


                     result = 0.4 * travelTimeScore + 0.2 * materialUrgencyScore + 0.2 * dummyFactor + 0.2;
                 }
                 else
                 {
                     double travelTimeScore = ScoreTravelTime(time, oneOverMaxHaulingTime);

                     double vehicleConditionScore = ScoreIsEntityFunctional(vehicle);


                     result = 0.4 * travelTimeScore + 0.2 * materialUrgencyScore + 0.2 * vehicleConditionScore + 0.2 * dummyFactor; 
                 }

                 EvaluateJob.ApplyJobPriorityModifier(job, ref result); // apply the user priority

               //  result *= priority;
             }

             return CalculateResult.Done;

         }
         */





        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double desirability)
        {
            desirability = 0;

            if (progress == Progress.NotStarted)
            {
                BiologicalEntity bioEntity;
                if (entity.Find(out bioEntity))
                {
                    ageContribution = ScoreAge(bioEntity);

                    if (ageContribution == 0)
                    {
                        return CalculateResult.Done;
                    }
                }

                timeOfDayContribution = ScoreTimeOfDay();

                // to ease the load, we don't calculate the combos too often when the time is not right:
                // will this be confusing for the player??? agents may be standing around...
                /*  if (DoHeavyCalculations(timeOfDayContribution)) 
                  {*/
                progress = Progress.GetCombos;
                //  }
            }

            if (progress == Progress.GetCombos)
            {
                /* 1. Nested hauling goals all fail if anyone fails.
                 * 2. The Outer goal is loaded first.
                 * 3. 
                 * 
                 * */
                mostDesirableJob = null;
                bestScore = 0;

                bestItem = null;


                //List<HaulingJob> outdatedJobs = null;

                if (ownerOfJobs.HaulingJobs.Count > 0)
                {
                    needsToBeCancelled.Clear();

                    // do a list of all combinations, then rate them all. 
                    // Perhaps later we can replace this brute force approach with a "quad tree" system where we search outwards through regions...
                    haulingCombos.Clear();
                    haulingComboProgress = 0;
                    scoringWasInterrupted = false;

                    //  Intelligence entityIntelligence = entity.Intelligence;

                    // find the best job:
                    Job job;
                    //  for (int i = 0; i < OwnerOfJobs.InternalOwner.OwnerContent.HaulingJobs.Count; i++)                   
                    IKnownEntityData itemData;

                    ThreatStance normalThreatStance;
                    RegionMap footRegionMap = GetRegionMapAndStanceForEvaluator(entity, null, out normalThreatStance);


                    for (int i = ownerOfJobs.HaulingJobs.Count - 1; i >= 0; i--)
                    {
                        job = ownerOfJobs.HaulingJobs[i];

                        if (!entityIntelligence.Brain.IsSame(job)) // don't consider a job we are already doing!                    
                        {
                            if (job is HaulingJob)
                            {
                                /* TODO: rate jobs by priority... for example: 
                                 * items for stalled production jobs
                                 * high profit items
                                 * rare items                                                    
                                 * heavy items because the vehicles are assigned first
                                 * */


                                ThreatStance threatStanceToUse;
                                //some hauling jobs require Bold stance! Cull these here. This test is repeated in Scoring...
                                if (!CanTakeStanceForJob(job, normalThreatStance, out threatStanceToUse))
                                {
                                    continue;
                                }

                                HaulingJobAnyItemOfType anyItemJob = job as HaulingJobAnyItemOfType;
                                if (anyItemJob != null)
                                {
                                    EntityGroup itemsEntityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(anyItemJob.ItemsToHaulGroup);
                                    if (itemsEntityGroup == null)
                                    {
                                        // let's remove the outdated job:
                                        anyItemJob.Destroy(true);
                                        continue;
                                    }

                                  
                                    GetAllHaulingCombosForItemType(anyItemJob, anyItemJob.RequiredItemType, itemsEntityGroup, threatStanceToUse);
                                }
                                else
                                {   // specific item to haul:
                                    HaulingJobSpecificItem haulingJob = (HaulingJobSpecificItem)job;

                                    EntityResult result = entityIntelligence.GetKnownData(haulingJob.Item.Value, out itemData);

                                    if (result == EntityResult.EntityStatusIsNowUnknown || result == EntityResult.Destroyed)
                                    {
                                        // let's remove the outdated job:
                                        haulingJob.Destroy(true);
                                        continue;
                                    }
                                  
                                    if (itemData.IsUnassignedToAnythingButThisJob(haulingJob, entityIntelligence.Allegiance.SharedKnowledge))
                                    {
                                        GetAllHaulingCombosForItem(itemData, haulingJob, threatStanceToUse);

                                    }

                                }
                            }


                        }
                    }

                    // start scoring the combos now:
                    progress = Progress.ScoreCombos;
                }
                else
                {
                    // nothing was found
                    progress = Progress.NotStarted; // Start from the top next time!
                }
            }

            if (progress == Progress.ScoreCombos)
            {
                // the reason we gather all combos first before scoring, is because scoring might be interrupted when we score distances.
                // doing it this way, we can keep track of where we are and don't need to start over.               
                HaulingCombo? bestCombo = null;
                if (ScoreAllCombosAndReturnBest(minimumRatingToConsider, ref bestCombo) == CalculateResult.Done)
                {
                    progress = Progress.NotStarted; // Progress.GetCombos; // all done. Start from the top next time!

                    if (bestCombo.HasValue)
                    {
                        mostDesirableJob = bestCombo.Value.Job;

                        bestItem = bestCombo.Value.Item;
                        bestVehicle = bestCombo.Value.Vehicle;

                        desirability = AddTimeAgeAndPriority(bestCombo.Value.Score, timeOfDayContribution, ageContribution, Priority);

                        bestScore = desirability;

                        // mostDesirableScore *= Priority; // NEW

                        //progress = Progress.GetCombos; // start from the top next time!

                        // for debugging feedback. 
                        entityIntelligence.TopScoringJobs.Add(new GoalAndScore() { Score = bestScore, Goal = mostDesirableJob.ToString() });

                        return CalculateResult.Done;
                    }
                }
                else
                {
                    /* if (entity.ID == (EntityID)4324)
                     {
                                        
                     }*/

                    return CalculateResult.Processing;

                }

            }

            // no vacant jobs...

            return CalculateResult.Done;
        }

        /* OLD
        public override double CalculateDesirability(Entity entity, double minimumRatingToConsider)
        {
            Intelligence entityIntelligence = entity.Intelligence;

           
            mostDesirableJob = null;
            mostDesirableScore = 0;

            bestItem = null;
            
            Item currentItem;
            Entity currentVehicle;
            double currentRating, distanceRating, valueRating;

            List<HaulingJob> outdatedJobs = null;

            if (OwnerOfJobs.OwningBody.HaulingJobs.Count > 0)
            {
                needsToBeCancelled.Clear();

                // do a list of all combinations, then rate them all. 
                // Perhaps we can replace this brute force approach with a "quad tree" system where we search outwards through regions...
                haulingCombos.Clear();

                // find the best job:
                Job job;
                for (int i = 0; i < OwnerOfJobs.OwningBody.HaulingJobs.Count; i++)  // time slicing? how... maintain a 'done' list? no pointers, keep an id instead...              
                {
                    job = OwnerOfJobs.OwningBody.HaulingJobs[i];

                    if (!entityIntelligence.Brain.IsSame(typeof(GoalHaul), job)) // don't consider a job we are already doing!                    
                    {                                                             
                        if (job is HaulingJob)
                        {                           
                            if (job is HaulingJobAnyItemOfType) 
                            {
                                // this is the distance score:
                                currentRating =
                                FindBestItemToHaul(entity, (HaulingJobAnyItemOfType)job, ((HaulingJobAnyItemOfType)job).RequiredItemType,
                                    out currentItem, out currentVehicle);

                                // OLD - don't compute the score until last.
                                if (currentRating > mostDesirableScore)
                                {                                    
                                    if (job.TakenBy.Count < job.MaxJobPositions || job.TakenBy.IsScoreGreaterThanAnyTaker(currentRating))
                                    {   // if job roster is full, is our score better than the current takers'?

                                        bestItem = currentItem;
                                        mostDesirableScore = currentRating;
                                        if (currentVehicle != null)
                                        {
                                            bestVehicle = currentVehicle;
                                        }
                                        mostDesirableJob = (HaulingJob)job;
                                    }
                                }
                                
                              //  entityIntelligence.TopScoringJobs.Add(new GoalAndScore() { Score = currentRating, Goal = job.ToString() });
                            }
                            else
                            {   // specific item to haul:
                                if (((HaulingJob)job).Item.IsUnassigned() && ((HaulingJob)job).Item.MapPosition.X != -1)
                                {
                                    currentRating = GetAllHaulingCombos(((HaulingJob)job).Item, (HaulingJob)job, entity,
                                        out currentVehicle);
                                    currentItem = ((HaulingJob)job).Item;

                                    if (bestItem == null || currentRating > mostDesirableScore)
                                    {
                                        mostDesirableScore = currentRating;
                                        bestItem = currentItem;
                                        if (currentVehicle != null)
                                        {
                                            bestVehicle = currentVehicle;
                                        }
                                        mostDesirableJob = ((HaulingJob)job);
                                    }
                                    entityIntelligence.TopScoringJobs.Add(new GoalAndScore() { Score = currentRating, Goal = job.ToString() });

                                }
                                else if (((HaulingJob)job).TakenBy.Count == 0)
                                {
                                    // the item is no longer free, it is being picked up by someone else, possibly to fulfill an ItemType hauling job.
                                    // we want to delete this job.
                                    RecordOutdatedJob(ref outdatedJobs, (HaulingJob)job);
                                }
                            }
                        }

                        
                    }
                }

                // clean up any outdated jobs that we found:
                CleanupOutdatedJobs(outdatedJobs, OwnerOfJobs.OwningBody.HaulingJobs);

                // do all calculations here! can easily be interrupted and resumed.
                HaulingCombo? bestCombo = ScoreAllCombosAndReturnBest(entity, minimumRatingToConsider);

                if (bestCombo.HasValue)
                {
                    mostDesirableJob = bestCombo.Value.Job;
                    bestVehicle = bestCombo.Value.Vehicle;
                    bestItem = bestCombo.Value.Item;
                    return bestCombo.Value.Score;
                }
                
            }

            // no vacant jobs...
            return 0;
        }*/

        /// <summary>
        /// Find the best item of the given type, rated by approximated hauling time to destination and travel time to item site. 
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="to"></param>
        /// <param name="itemType"></param>
        /// <param name="bestItem"></param>
        /// <returns>should return value in range 0 - 1 </returns>
        /*   private double FindBestItemToHaul(Entity entity, HaulingJobAnyItemOfType job, ItemType itemType, out Item bestItem, out Entity bestVehicle)
           {
               Intelligence entityIntelligence = entity.Intelligence;

               double bestTime = maxHaulingTime; 
               double time;
               bestItem = null;

               Entity vehicle;
            
               bestVehicle = null;

               // test for empty:
               List<Item> items; 
          
               if (job.OwnerOfItemsToHaul.OwningBody.Items.TryGetValue(itemType, out items))
               {

                   MovementMap moveMap = UWGame.SimSide.Instance.Map.GetMovementMap(entityIntelligence.ProtectionLevel,
                              entity.EntityType.ThreatCategory, entityIntelligence.ThreatStance);

                   foreach (Item item in items) 
                   {
                       // NEW! Can take other people's items! Not if they are onboard a driven vehicle, or if they are assigned to anything other than haulingjobs though.
                       if (item.IsAccessible() && (item.OnBoard == null || item.OnBoard.Vehicle.DrivenBy == null) && (item.AssignedToJob == null || item.AssignedToJob is HaulingJob)//OLD: This line is replaced by: item.IsUnassigned() 
                           && item.MapPosition != job.To // ADDED AGAIN - entrance to buildings should not be used for dumping, piling etc. if an item is stored inside a building, and the base tile of the building is next to the destination, we don't want to disregard it.
                           && EstimateIsAccessible(moveMap, entity.MapPosition, item.MapPosition) > 0)
                       {

                           time = EstimateHaulingTime(item, job, entity, out vehicle);

                           if (bestItem == null || time < bestTime)
                           {
                               if (vehicle != null)
                               {
                                   bestVehicle = vehicle;
                               }
                               else
                               {
                                   bestVehicle = null;
                               }

                               bestItem = item;
                               bestTime = time;
                           }

                       }

                   }
               }

               if (bestItem != null)
               {                
                   return ScoreTravelTime(bestTime, maxHaulingTime);

               }
               else return 0;
           }
           */

        /// <summary>      
        /// NEW: don't do calculations, just store all combos!
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="to"></param>
        /// <param name="itemType"></param>
        /// <param name="bestItem"></param>
        /// <returns>should return value in range 0 - 1 </returns>
        private void GetAllHaulingCombosForItemType(HaulingJobAnyItemOfType job, EntityType itemType, EntityGroup entityGroup, ThreatStance threatStance) //, out Item bestItem, out Entity bestVehicle)
        {
            // test for empty:
            List<EntityID> items;

            if (entityGroup.Items.TryGetValue(itemType, out items))
            {
                EntityID item;
                IKnownEntityData itemData;
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    item = items[i];

                    if (!HandleOwnerDataResult(entityIntelligence.Allegiance.SharedKnowledge, item, entityGroup, out itemData))
                    {
                        continue;
                    }

                    if (itemData.IsUnassignedToAnythingButThisJob(job, entityIntelligence.Allegiance.SharedKnowledge))
                    {
                        GetAllHaulingCombosForItem(itemData, job, threatStance);
                    }
                }

            }
        }


        private CalculateResult EstimateHaulingTimeByFoot(Entity entity, EntityID item, HaulingJob haulingJob, RegionMap regionMap,
            ref double haulingTime, out IKnownEntityData itemToHaul)
        {
            EntityID? storageEntity = haulingJob.GetToStorageEntity; // ToStorageEntity;
            Vector3? groundLocation = haulingJob.ToLocation;

            float distanceToItem = 0f;
            float distanceFromItemToDestination = 0f;

            SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;


            // first ensure entity integrity:
            IKnownEntityData itemData, storageData = null;



            EntityResult itemResult = sharedKnowledge.GetKnownData(item, out itemData);
            if (itemResult == EntityResult.Destroyed || itemResult == EntityResult.EntityStatusIsNowUnknown)
            {
                itemToHaul = null;
                // fail... do cleanup in iterated list????
                haulingTime = -1;
                return CalculateResult.Done;
            }
            itemToHaul = itemData;
            Point? toGroundSubtile = null;

            if (storageEntity.HasValue)
            {
                EntityResult storageResult = sharedKnowledge.GetKnownData(storageEntity.Value, out storageData);

                if (storageResult == EntityResult.Destroyed || storageResult == EntityResult.EntityStatusIsNowUnknown)
                {
                    // fail... do cleanup in iterated list????
                    haulingTime = -1;
                    return CalculateResult.Done;
                }
            }
            else
            {
                toGroundSubtile = MapManager.WorldPosToSubtile(groundLocation.Value); // crash here? http://steamcommunity.com/app/284100/discussions/2/540741131163590563/
            }


            // agent to item:
            // if the agent is already carrying the item, give an extra bonus

            //RegionMap.Result result1 = regionMap.GetDistance(entity, MapManager.WorldPosToSubtile(entity.Location), itemSubtilePos, ref distanceToItem);
            RegionMap.Result result1 = regionMap.GetDistanceToEntity(entity, entity, itemData, ref distanceToItem);
            if (result1 == RegionMap.Result.Wait)
            {
                return CalculateResult.Processing;
            }
            else if (result1 == RegionMap.Result.NoAccess)
            {
                haulingTime = -1;
                return CalculateResult.Done;
            }

            // if the agent is already carrying the item, give an extra bonus:           
            bool isCarryingItem = false;

            Entity itemAsEntity = itemData as Entity;
            if (itemAsEntity != null)
            {
                if (itemAsEntity.ContainedBy == entity.EntityID)
                {
                    isCarryingItem = true;
                }
            }

            if (isCarryingItem)
            {
                distanceToItem = 0f;
            }
            else
            {
                distanceToItem = Common.ClampBottom(distanceToItem, 15f);
            }

            // item to storage/ground location:
            Point? fromSubtileResult, toSubtileResult;
            // this will set accessibility feedback on the target:
            RegionMap.Result result2 = regionMap.GetDistanceToEntity(entity, itemData, storageData, ref distanceFromItemToDestination, out fromSubtileResult, out toSubtileResult,
                null, toGroundSubtile);

            // new: also set inaccessibility feedback on process job if it exists
            if (fromSubtileResult.HasValue && toSubtileResult.HasValue)
            {
                GoalEvaluator.UpdateJobAccessibility(entity, fromSubtileResult.Value, toSubtileResult.Value, haulingJob.RequiredByProcessJob, ownerOfJobs.Parent, result2);
            }

            if (result2 == RegionMap.Result.Wait)
            {
                return CalculateResult.Processing;
            }
            else if (result2 == RegionMap.Result.NoAccess)
            {

                haulingTime = -1;
                return CalculateResult.Done;
            }

            double timeWithoutTerrain = distanceToItem / entity.Locomotor.CalculateSpeed(0f, false)
                    + distanceFromItemToDestination / entity.Locomotor.CalculateSpeed(itemData.Bulk, false); // include other items carried?

            // by foot, roads don't matter. Use Plains for the estimate:
            haulingTime = timeWithoutTerrain * PlainsType.Instance.MovementFactor(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.None);
            // Cost(TerrainType.TransportType.Foot, TerrainType.TerrainFeatures.None);
            if (isCarryingItem)
            {
                haulingTime *= 0.8;
            }

            return CalculateResult.Done;
        }

        private static CalculateResult EstimateHaulingTimeByVehicle(Entity entity, EntityID item, IKnownEntityData vehicle, EntityID? storageEntity, Vector3? groundLocation,
            RegionMap footRegionMap, ref double haulingTime)
        {
            // make this distance in pixels
            // TODO: use real speeds to get real times in seconds!
            float distanceToVehicle = 0f;
            float distanceFromVehicleToItem = 0f;
            float distanceFromItemToDestination = 0f;

            SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;

            // first ensure entity integrity:
            IKnownEntityData vehicleData, itemData, storageData = null;
            EntityResult vehicleResult = sharedKnowledge.GetKnownData(vehicle.EntityID, out vehicleData);

            if (vehicleResult == EntityResult.Destroyed || vehicleResult == EntityResult.EntityStatusIsNowUnknown)
            {
                // fail... do cleanup in iterated list????
                haulingTime = -1;
                return CalculateResult.Done;
            }

            EntityResult itemResult = sharedKnowledge.GetKnownData(item, out itemData);

            if (itemResult == EntityResult.Destroyed || itemResult == EntityResult.EntityStatusIsNowUnknown)
            {
                // fail... do cleanup in iterated list????
                haulingTime = -1;
                return CalculateResult.Done;
            }

            if (storageEntity.HasValue)
            {
                EntityResult storageResult = sharedKnowledge.GetKnownData(storageEntity.Value, out storageData);

                if (storageResult == EntityResult.Destroyed || storageResult == EntityResult.EntityStatusIsNowUnknown)
                {
                    // fail... do cleanup in iterated list????
                    haulingTime = -1;
                    return CalculateResult.Done;
                }
            }


            //     Vector3 knownVehicleLocation = The.Sim.GetKnownLocation(entity.Intelligence.Allegiance, vehicle);
            //   Point vehicleSubtilePos = MapManager.WorldPosToSubtile(knownVehicleLocation); 

            // move to vehicle by foot:
            RegionMap.Result result1 = footRegionMap.GetDistanceToEntity(entity, entity, vehicleData, ref distanceToVehicle);



            if (result1 == RegionMap.Result.Wait)
            {
                return CalculateResult.Processing;
            }
            else if (result1 == RegionMap.Result.NoAccess)
            {
                haulingTime = -1;
                return CalculateResult.Done;
            }


            // Cars are very dependent on roads.
            // In case of cars, this estimate could be improved by dividing the map into sectors and see if they are road-linked.

            double timeByVehicleOverTerrain;
            float loadedVehicleSpeed = vehicle.CalculateSpeed(itemData.Bulk);

            // Vector3 knownItemLocation = The.Sim.GetKnownLocation(entity.Intelligence.Allegiance, item);

            if (((VehicleContainerType)vehicle.EntityType.ContainerType).Transport == SurfaceType.TransportType.Air)
            {
                timeByVehicleOverTerrain = EstimateAirTime(vehicle,
                    groundLocation.Value, vehicleData.PlaySiteLocation, loadedVehicleSpeed, itemData.PlaySiteLocation);

            }
            else
            {
                // include threats too                
                RegionMap vehicleRegionMap = sharedKnowledge.GetVehicleRegionMap(entity); // UWGame.SimSide.Instance.Map.RegionMapManager.HumanVehicleExposedNormalRegionMap; 

                // move over land to the item:
                //  Point itemSubtilePos = MapManager.WorldPosToSubtile(knownItemLocation); 

                //  RegionMap.Result result2 = vehicleRegionMap.GetDistance(entity, vehicleSubtilePos, itemSubtilePos, ref distanceFromVehicleToItem);
                //   EntityResult itemResult;

                RegionMap.Result result2 = vehicleRegionMap.GetDistanceToEntity(entity, vehicleData, itemData,
                      ref distanceFromVehicleToItem);

                if (result2 == RegionMap.Result.Wait)
                {
                    return CalculateResult.Processing;
                }
                else if (result2 == RegionMap.Result.NoAccess)
                {
                    haulingTime = -1;
                    return CalculateResult.Done;
                }

                // move to destination:
                RegionMap.Result result3;
                if (storageData != null)
                {
                    // hauling to storage:
                    result3 = vehicleRegionMap.GetDistanceToEntity(entity, itemData, storageData, ref distanceFromItemToDestination, null);
                }
                else
                {
                    // hauling to ground spot:
                    result3 = vehicleRegionMap.GetDistanceToEntity(entity, itemData, null, ref distanceFromItemToDestination, null,
                            groundLocation.HasValue ? (Point?)MapManager.WorldPosToSubtile(groundLocation.Value) : null);
                }

                if (result3 == RegionMap.Result.Wait)
                {
                    return CalculateResult.Processing;
                }
                else if (result3 == RegionMap.Result.NoAccess)
                {
                    haulingTime = -1;
                    return CalculateResult.Done;
                }

                timeByVehicleOverTerrain =
                    (distanceFromVehicleToItem / vehicle.CurrentMaximumSpeed.Value
                  + distanceFromItemToDestination / loadedVehicleSpeed)
                  * PlainsType.Instance.MovementFactor(((VehicleContainerType)vehicle.EntityType.ContainerType).Transport, SurfaceType.TerrainFeatures.None);

            }

            double timeByFootOverTerrain = (distanceToVehicle / entity.Locomotor.CurrentMaximumSpeedForEvaluator) // CurrentMaximumSpeedNoTerrain)
                    * PlainsType.Instance.MovementFactor(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.None);

            haulingTime = timeByFootOverTerrain + timeByVehicleOverTerrain + GameData.Instance.AIConstants.EvaluatorTimePenaltyForUsingVehicles; // 4.0; // add a time penalty for using a vehicle!


            return CalculateResult.Done;
        }

        public static double EstimateAirTime(IKnownEntityData vehicle, Vector3 destination, Vector3 knownVehicleLocation, float estimatedLoadedVehicleSpeed, Vector3 knownItemLocation)
        {
            double timeByVehicleOverTerrain;

            // distances in pixels!
            timeByVehicleOverTerrain =
                Common.DistanceOctile(knownVehicleLocation, knownItemLocation) / vehicle.CurrentMaximumSpeed.Value
                + Common.DistanceOctile(knownItemLocation, destination) / estimatedLoadedVehicleSpeed; // include other items carried?


            // aircraft take some time taking off and landing.             
            // time should be in seconds now:

            timeByVehicleOverTerrain += 2 * ((VehicleContainerType)vehicle.EntityType.ContainerType).Aircraft.EstimatedTakeOffLandingTime;
            return timeByVehicleOverTerrain;
        }

        /*  private double EstimateHaulingTime(Item item, HaulingJob job, Entity entity, out Entity bestRatedVehicle)
          {
              double bestTime = maxHaulingTime;
              double timeByFoot;
            
              bestRatedVehicle = null;

              //TODO: rate based on value
              timeByFoot = EstimateHaulingTimeByFoot(entity, item, job.To);

              // NEW: Store this combo:
              haulingCombos.Add(new HaulingCombo() { Vehicle = null, Item = item, Job = job });

              // compare this time to time using a vehicle:
              double timeUsingVehicle = BestVehicleTime(entity, item, job, out bestRatedVehicle);

              if (timeByFoot > timeUsingVehicle && bestRatedVehicle != null)
              {
                  return timeUsingVehicle;
              }
              else
              {

                  bestRatedVehicle = null;
                  return timeByFoot;
              }

          }*/

        /// <summary>
        /// NEW: don't do calculations yet, just store the combos.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="job"></param>
        /// <param name="entity"></param>
        /// <param name="bestRatedVehicle"></param>
        /// <returns></returns>
        private void GetAllHaulingCombosForItem(IKnownEntityData itemData, HaulingJob job, ThreatStance threatStance)
        {

            ThreatStance threatStanceToUse;

            //some hauling jobs require Bold stance! cull these here...
            if (!CanTakeStanceForJob(job, threatStance, out threatStanceToUse))
            {
                return;
            }


            // Store this combo:
            // NEW! Can take other people's items! Not if they are onboard a driven vehicle, or if they are assigned to anything other than haulingjobs though.
            if (itemData.IsItemValidForHauling(entity, entityIntelligence, job) // cull the invalid items here...
                && WorkSiteIsSafe(entity, itemData, threatStanceToUse)
                )
            {

                haulingCombos.Add(new HaulingCombo() { Vehicle = null, Item = itemData.EntityID, Job = job });

                // find all vehicles too:
                IKnownEntityData vehicleData;
                EntityID vehicleID;
                if (personEntity != null && personEntity.CanDrive())
                {
                    foreach (EntityGroup ownerOfvehicles in OwnersOfVehicles) // 1 or more sets of vehicles can be searched...
                    {
                        for (int i = ownerOfvehicles.Vehicles.Count - 1; i >= 0; i--)
                        {
                            vehicleID = ownerOfvehicles.Vehicles[i];

                            if (GoalEvaluator.HandleOwnerDataResult(entityIntelligence.Allegiance.SharedKnowledge, vehicleID, ownerOfvehicles, out vehicleData))
                            {

                                // NEW: Also include any vehicle occupied by the entity himself:                   
                                // Can also take vehicles reserved by others!
                                if (vehicleData.IsVehicleValidForHauling(entity, itemData)) // moved to IKnownData
                                {
                                    // Store this combo:
                                    // what about unseen vehicles...?
                                    haulingCombos.Add(new HaulingCombo() { Vehicle = vehicleID, Item = itemData.EntityID, Job = job });
                                }

                            }
                        }
                    }
                }

            }
        }


        /*

        private double BestVehicleTime(Entity entity, Item item, HaulingJob job, out Entity bestVehicle)
        {
            double bestTime = maxHaulingTime;
            double time;
            bestVehicle = null;

            foreach (Owner ownerOfvehicles in OwnersOfVehicles) // 1 or more sets of vehicles can be searched...
            {
                foreach (Entity vehicle in ownerOfvehicles.OwningBody.Vehicles)
                {
                    Vehicle vehicleComponent;
                    vehicle.Find(out vehicleComponent);

                    // NEW: Also include any vehicle occupied by the entity himself:
                   // if (((vehicle.DrivenBy == null && vehicle.TakenBy == null) || vehicle.DrivenBy == entity)
                    // NEW NEW: Can take vehicles reserved by others!
                    if (((vehicleComponent.DrivenBy == null) || vehicleComponent.DrivenBy == entity)
                        && vehicle.ItemStorage != null && vehicle.ItemStorage.TotalCapacity >= item.Bulk)
                    {
                        time = EstimateHaulingTimeByVehicle(entity, item, vehicle, job.To);
                        
                        // NEW: Store this combo:
                        haulingCombos.Add(new HaulingCombo(){ Vehicle = vehicle, Item = item, Job = job});

                        if (bestVehicle == null || time < bestTime)
                        {
                            bestVehicle = vehicle;
                            bestTime = time;
                        }

                    }
                }
            }

            return bestTime;
        }*/

        /*    private double BestVehicleTimeSquared(Entity entity, Item item, Point to, out Vehicle bestVehicle)
            {
                double bestTimeSquared = maxHaulingTimeSquared;
                double timeSquared;
                bestVehicle = null;

                foreach (Owner ownerOfvehicles in OwnersOfVehicles) // 1 or more sets of vehicles can be searched...
                {
                    foreach (Vehicle vehicle in ownerOfvehicles.OwningBody.Vehicles)
                    {
                        // NEW: Also include any vehicle occupied by the entity:
                        if (((vehicle.DrivenBy == null && vehicle.TakenBy == null) || vehicle.DrivenBy == entity) 
                            && vehicle.VehicleType.CarryLimit >= item.Bulk)
                        {
                            float loadedVehicleSpeed = vehicle.CalculateSpeed(item.Bulk);

                            timeSquared = Common.DistanceSquared(entity.MapPosition, vehicle.MapPosition) / entity.CurrentMaximumSpeed
                                    + Common.DistanceSquared(vehicle.MapPosition, item.MapPosition) / vehicle.CurrentMaximumSpeed
                                    + Common.DistanceSquared(item.MapPosition, to) / loadedVehicleSpeed; // include other items carried?

                            if (bestVehicle == null || timeSquared < bestTimeSquared)
                            {
                                bestVehicle = vehicle;
                                bestTimeSquared = timeSquared;
                            }

                        }
                    }
                }

                return bestTimeSquared;
            }*/




        public override bool CancelCurrentTakers()
        {
            return (CancelEntities(needsToBeCancelled, itemsToBeDropped));
        }

        public override bool CanTakeGoal()
        {
            if (mostDesirableJob != null)
            {
                if (EvaluateJob.IsJobValid(mostDesirableJob) == false)
                {
                    return false;
                }
                needsToBeCancelled.Clear();
                itemsToBeDropped.Clear();

                IKnownEntityData bestItemData, bestVehicleData = null;
                if (!EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(bestItem.Value, out bestItemData)))
                {
                    if (bestVehicle.HasValue)
                    {
                        if (EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(bestVehicle.Value, out bestVehicleData)))
                        {
                            return false;
                        }
                    }

                    if (GetJobsToCancel(mostDesirableJob, bestItemData, bestVehicleData))
                    {

                        if (IsScoreBetterThanAllInvolveds(bestScore, needsToBeCancelled))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // we need the runner-up score here!
        public override bool SetGoal()
        {
            base.SetGoal();

            if (mostDesirableJob != null)
            {
                // if the item is currently carried by someone else, we must first tell them to drop it:
                // we won't set the goal now. But next time it will be available... WHY? it can be dropped immediately
                // TODO: do the same for Additional items/ hauling jobs!

                EvaluateJob.SetDebugScore(entity, mostDesirableJob, bestScore);
               
                GoalHaul goalHaul = new GoalHaul(entity, mostDesirableJob, bestItem.Value,
                    bestVehicle,
                    mostDesirableJob.NewOwner,
                    GetOwnerIDs(OwnersOfVehicles),
                    ownerOfJobs.ID) { GoalEvaluator = this };

                entityIntelligence.SetTopLevelGoal(goalHaul, bestScore);


                return true;


            }
            return false;
        }



        /*  private bool IsItemValidForHauling(Entity entity, Intelligence entityIntelligence, HaulingJob job, Entity item)
          {
              Vector3 knownItemLocation = The.Sim.GetKnownLocation(entityIntelligence.Allegiance, item);

              Item itemComponent = item.Item;

              if (itemComponent.IsAccessible()
                  && !The.Sim.IsKnownToBeDestroyed(entity, item)
                  && (itemComponent.OKToTakeThisItemFromCarrier != false || entity.Storage.Contains(item)) // only consider items that we are carrying or which are OK to take from others carrying them.
                  && (itemComponent.OnBoard == null || itemComponent.OnBoard.Vehicle.DrivenBy == null)
                  && (item.AssignedToJob == null || item.AssignedToJob is HaulingJob)
                  && (entity.Storage.ItemStorage.HasCapacityForItemWhenEmpty(item)) // NEW!!(?)
                  && (job.ToStorage == null || itemComponent.StoredIn != job.ToStorage)
                  && (job.ToLocation != item.Location)) //MapManager.WorldPosToTile(knownItemLocation) != job.ToTilePos) // ADDED AGAIN - entrance to buildings should not be used for dumping, piling etc. if an item is stored inside a building, and the base tile of the building is next to the destination, we don't want to disregard it.
              {
               
                  if (EstimateWorkSiteDiscomfort(entity, knownItemLocation) > 0) // item.MapPosition) > 0)
                  {
                      return true;
                  }
              }

              return false;

          }*/




        /*
        private bool IsVehicleValidForHauling(Entity entity, Entity vehicle, Entity item)
        {
            Vehicle vehicleComponent;
            vehicle.Find(out vehicleComponent);

            if (((vehicleComponent.DrivenBy == null) || vehicleComponent.DrivenBy == entity)
                && vehicle.Storage != null
                && vehicle.Storage.ItemStorage.TotalCapacity >= item.Bulk
                && vehicle.IsCompleted()
                && ScoreIsEntityFunctional(vehicle) > 0.0) // omit broken down vehicles
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        */

        private bool IsComboValidForHauling(Entity entity, Intelligence entityIntelligence, ThreatStance threatStance, HaulingJob comboJob, IKnownEntityData comboItemData, IKnownEntityData comboVehicleData) //HaulingCombo combo)
        {

            if (comboItemData.IsItemValidForHauling(entity, entityIntelligence, comboJob)
                && WorkSiteIsSafe(entity, comboItemData, threatStance))
            {
                if (comboVehicleData != null)
                {
                    return comboVehicleData.IsVehicleValidForHauling(entity, comboItemData);
                }
                else return true;

            }
            else return false;
        }

        private CalculateResult ScoreAllCombosAndReturnBest(double minimumRatingToConsider, ref HaulingCombo? bestCombo)
        {
            HaulingCombo combo;
            double score = -1.0;

            ThreatStance normalThreatStance;
            RegionMap footRegionMap = GetRegionMapAndStanceForEvaluator(entity, null, out normalThreatStance);
            //  entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel, entity.Intelligence.Allegiance.RepresentativeEntityType.ThreatCategory, entity.Intelligence.ThreatStance).RegionMap[SurfaceType.TransportType.Foot];



            IKnownEntityData itemData, vehicleData;

            double ageContribution = GetAgeContribution();

            double timeContribution = ScoreTimeOfDay();

            for (; haulingComboProgress < haulingCombos.Count; haulingComboProgress++)
            {   // it is a struct.... we don't get a reference.
                combo = haulingCombos[haulingComboProgress];


                if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(combo.Item, out itemData)))
                {
                    combo.Score = 0;
                    haulingCombos[haulingComboProgress] = combo;

                   // combo.Job.DebugScore = 0;
                    continue;
                }

                vehicleData = null;
                if (combo.Vehicle.HasValue)
                {
                    if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(combo.Vehicle.Value, out vehicleData)))
                    {
                        combo.Score = 0;
                        haulingCombos[haulingComboProgress] = combo;

                      //  combo.Job.DebugScore = 0;
                        continue;
                    }
                }



                //Before scoring, check that the job and items are still valid!
                if (!scoringWasInterrupted || IsComboValidForHauling(entity, entityIntelligence, normalThreatStance, combo.Job, itemData, vehicleData)) // only need to check for validity if we are resuming.
                {

                    if (ScoreThisJob(footRegionMap, normalThreatStance,
                            entity,
                            combo.Job,
                            1,
                            ageContribution, timeContribution,
                            out score, null, null,
                            new HaulingParams() { Item = combo.Item, Vehicle = vehicleData })
                                == CalculateResult.Done)
                    {
                        combo.Score = score;
                        haulingCombos[haulingComboProgress] = combo; // structs require this!

                        EvaluateJob.SetDebugScore(entity, combo.Job, score);

                       // combo.Job.DebugScore = score;
                    }
                    else
                    {
                        scoringWasInterrupted = true; // now we need to check all the combos again for availability...

                        /*    if (entity.ID == (EntityID)4324)
                            {

                            }*/

                        return CalculateResult.Processing; // come back later...
                    }
                }
                else
                {
                    combo.Score = 0;
                    haulingCombos[haulingComboProgress] = combo; // structs require this!

                    EvaluateJob.SetDebugScore(entity, combo.Job, 0);
                    //combo.Job.DebugScore = 0;
                }

            }

            // set this flag on carried items we have considered hauling:
            entity.AgentStorage.IterateContained(SetItemOKToHaulByOthers);


            // Important! Remove the combos with zero score:
            haulingCombos.RemoveAll(c => c.Score == 0);

            haulingCombos.Sort((a, b) => b.Score.CompareTo(a.Score));

            bestCombo = null;


            for (int i = 0; i < haulingCombos.Count; i++)
            {
                combo = haulingCombos[i];

                if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(combo.Item, out itemData)))
                {
                    continue;
                }

                vehicleData = null;
                if (combo.Vehicle.HasValue)
                {
                    if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(combo.Vehicle.Value, out vehicleData)))
                    {
                        continue;
                    }
                }

                if (!scoringWasInterrupted || IsComboValidForHauling(entity, entityIntelligence, normalThreatStance, combo.Job, itemData, vehicleData)) // only need to check for validity if we are resuming.
                {
                    if (combo.Score < minimumRatingToConsider)
                    {
                        break; // the score is not good enough. we have reached the low scores and are now finished
                    }
                }
                else
                {
                    continue; // this combo is no longer available, skip it.
                    //break; 
                }


                if (!GetJobsToCancel(combo.Job, itemData, vehicleData))
                {
                    continue;
                }

                if (needsToBeCancelled.Count == 0)
                {   // WINAR!!! No one to cancel
                    bestCombo = combo;
                    break;
                }
                else if (IsScoreBetterThanAllInvolveds(combo.Score, needsToBeCancelled))
                {   // there are people doing this job. Only take it from them if our score is better than theirs by a certain margin.
                    bestCombo = combo;
                    break;
                }

            }

            // return bestCombo;
            return CalculateResult.Done;
        }

        public void SetItemOKToHaulByOthers(Entity item)
        {
            Item itemComponent = item.Item;
            if (itemComponent != null)
            {
                itemComponent.OKToTakeThisItemFromCarrier = true;
            }
        }

        private bool GetJobsToCancel(Job job, IKnownEntityData itemData, IKnownEntityData vehicleData)
        {
            //Item itemComponent = combo.Item.Item;

            needsToBeCancelled.Clear();
            itemsToBeDropped.Clear();



            List<Entity> takenBy = new List<Entity>();

            if (job.TakenBy.Count > 0)
            {
                job.TakenBy.GetLowestScorer();
                for (int i = 0; i < job.TakenBy.Count; i++)
                {
                    takenBy.Add(job.TakenBy.Get(i));
                }

                while (takenBy.Count >= job.MaxJobPositions)
                {
                    if (takenBy[0] == entity)
                    {
                        // error...
                        System.Diagnostics.Debug.Assert(false, "Error, agent is about to cancel self");

                        entityIntelligence.Brain.IsSame(job);
                    }

                    // entityIntelligence.Brain.IsSame(job);

                    needsToBeCancelled.Add(takenBy[0]);
                    takenBy.RemoveAt(0);
                }
            }

            //if (job.TakenBy.Count > 0)
            //{
            //    needsToBeCancelled.Add(job.TakenBy.Get(0));
            //}

            GetItemUser(itemData, needsToBeCancelled);

            Job assignedToJob = EvaluateJob.ResolveAssignedToJob(itemData);

            if (assignedToJob != null)
            {
                if (assignedToJob.TakenBy.Count > 0)
                {
                    if (assignedToJob.TakenBy.Get(0) == entity)
                    {
                        // error...                       
                        System.Diagnostics.Debug.Assert(false, "Error, agent is about to cancel self");
                    }

                    needsToBeCancelled.Add(assignedToJob.TakenBy.Get(0));
                }
            }
            /*  if (itemData.EquippedBy != null)
              {
                  // drop the item
                  Entity itemEntity = itemData as Entity;
                  if (itemEntity != null) // will be non-null if carried by someone in our allegiance
                  {
                      itemsToBeDropped.Add(itemEntity); //itemComponent.EquippedBy);
                  }
              }*/

            Entity carriedItem = itemData as Entity;
            if (carriedItem != null) // will be non-null if carried by someone in our allegiance
            {
                Entity carrier;
                if (!carriedItem.CarriedByAgent(out carrier))
                {
                    return false;
                }

                if (carrier != null && carrier != entity)
                {
                    itemsToBeDropped.Add(carriedItem);
                }
            }

            if (vehicleData != null)
            {
                // we are taking this vehicle!
                GetItemUser(vehicleData, needsToBeCancelled);
            }

            // remove duplicates:
            needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
            itemsToBeDropped = itemsToBeDropped.Distinct().ToList();

            return true;
        }



        /*    private void CompileListOfInvolved(HaulingJob job)
            {
                if (job.TakenBy.Count > 0)
                {
                    // kick him:
                    needsToBeCancelled.Add(job.TakenBy.Get(0));
                }

                if (bestItem.TargetedForPickupBy != null)
                {
                    needsToBeCancelled.Add(bestItem.TargetedForPickupBy);
                }

                if (bestItem.CarriedBy != null)
                {
                    // drop the item
                    needsToBeCancelled.Add(bestItem.CarriedBy);
                }

                if (bestVehicle.TakenBy != null)
                {
                    // we are taking this vehicle!
                    needsToBeCancelled.Add(bestVehicle.TakenBy);
                }
                        
            }
            */






    }
}




