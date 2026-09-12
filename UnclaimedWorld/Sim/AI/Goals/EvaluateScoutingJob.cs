using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.Body;
using GameStateManagement;
using System.Linq;
using UWGame.SimSide.Entities.Biological;
namespace UWGame.SimSide.AI.Goals
{

    /// <summary>
    /// 
    /// TODO: consider merging this with EvaluateJob even if no weapons should be carried on scouting missions - create empty combos!
    /// 
    /// used for scout and forage jobs
    /// 
    /// 
    /// score areas:
    /// see if: 
    /// agent can reach edge of area
    /// get closest point on edge of area
    /// score the distance
    /// 
    /// </summary>
    class EvaluateScoutingJobs : GoalEvaluator, IScoreJob
    {
        public override float Priority
        {
            get
            {
                return 1f;
            }
        }

        //*************
        // progress variables for timeslicing/interrupt and continue:
        private enum Progress { NotStarted, ScoreJobs }
        private Progress progress = Progress.NotStarted;

        private ScoutingJob mostDesirableJob = null;

        private Vector3? locationInScoutingArea = null;


        private Dictionary<Job, Job> processedJobs = new Dictionary<Job, Job>();              

       // private Allegiance.Allegiance allegiance;

        private List<EntityGroup> ownersOfVehicles;

        private EntityGroup ownerOfJobs;

        private List<Entity> needsToBeCancelled = new List<Entity>();
        private List<Entity> itemsToBeDropped = new List<Entity>();

        public EvaluateScoutingJobs(Entity entity, /*Allegiance.Allegiance allegiance,*/ EntityGroup ownerOfJobs, List<EntityGroup> ownersOfVehicles = null)
            : base(entity)
        {
          //  this.allegiance = allegiance;
            this.ownerOfJobs = ownerOfJobs;
            this.ownersOfVehicles = ownersOfVehicles;
        }

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {          

            if (progress == Progress.NotStarted)
            {
                mostDesirableJob = null;
                                
                bestScore = minimumRatingToConsider;
                locationInScoutingArea = null;
               
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

                /* if (DoHeavyCalculations(timeOfDayContribution))
                 {*/
                progress = Progress.ScoreJobs; // start!

                processedJobs.Clear();

                // }

            }

            if (progress == Progress.ScoreJobs)
            {

                double ageContribution = GetAgeContribution();

                double timeOfDayContribution = ScoreTimeOfDay();

                
                // find the best job:
                foreach (ScoutingJob scoutingJob in ownerOfJobs.ScoutingJobs)
                    //allegiance.ScoutingJobs)
                {
                    //scoutingJob = job as ThJob;

                    if (!processedJobs.ContainsKey(scoutingJob)) // maintain a 'done' list
                    {
                        if (HandleJob(scoutingJob, ageContribution, timeOfDayContribution) == CalculateResult.Processing)
                        {
                            return CalculateResult.Processing;
                        }
                        else
                        {
                            processedJobs.Add(scoutingJob, scoutingJob);
                        }
                    }
                }


                // done - start over from the top next time
                progress = Progress.NotStarted;

                if (mostDesirableJob != null)
                {

                    result = bestScore;

                    return CalculateResult.Done;
                }
                else
                {
                    result = 0;
                    return CalculateResult.Done;
                }

            }

            // no vacant jobs...
            result = 0;
            return CalculateResult.Done;
        }

        /// <summary>
        /// implements IScoreJob!
        /// </summary>      
        public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Jobs.Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating,
           ToolParams? toolParams, AttackParams? attackParams, HaulingParams? haulingParams)
        {
            Vector3? closestLocation;
            return ScoreThisJob(regionMap, threatStanceToUse, entity, job, proposedNumberOfWorkers, ageContribution, timeContribution, out rating, out closestLocation); 
        }

        public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Jobs.Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating, out Vector3? closestLocation)
        {
            rating = 0;
            closestLocation = null;

            ScoutingJob scoutingJob = job as ScoutingJob;


            if (!ageContribution.HasValue)
            {
                ageContribution = GetAgeContribution();
            }

            if (!timeContribution.HasValue)
            {
                timeContribution = ScoreTimeOfDay();
            }
            

            double travelTimeScore = 0;          
            RegionMap.Result result;
           
            if (scoutingJob.Location.HasValue)
            {
                Vector3 jobLocation = scoutingJob.Location.Value;

                result = ScoreTravelTime(regionMap, threatStanceToUse, entity.AccessPoint.Value, // #ACCESS .PlaySiteLocation, 
                    jobLocation, entity, ref travelTimeScore, job, ownerOfJobs.Parent);
               
                if (result == RegionMap.Result.Wait)
                {
                    // wait for the result...
                    //rating = 0;
                    return CalculateResult.Processing;

                }
                else if (result == RegionMap.Result.NoAccess)
                {                   
                    return CalculateResult.Done;
                }              

            }
            else
            {
                float? distanceToClosestTile;
                TerrainTile closestTile;

                //TODO: Add assert about the region map transporation if it isnt by foot
              //  RegionMap.Result getClosestTileResult = scoutingJob.Zone.GetClosestSafeEdgeTile(job, entity.PlaySiteLocation, regionMap, threatStanceToUse, entity, out distanceToClosestTile, out closestTile, ownerOfJobs);
                RegionMap.Result getClosestTileResult = scoutingJob.Zone.GetClosestSafeEdgeTile(job, entity.AccessPoint.Value, regionMap, threatStanceToUse, entity, out distanceToClosestTile, out closestTile, ownerOfJobs);


                if (getClosestTileResult == RegionMap.Result.Wait)
                {
                    return GoalEvaluator.CalculateResult.Processing;
                }
                else if (getClosestTileResult == RegionMap.Result.NoAccess)
                {
                    
                    return GoalEvaluator.CalculateResult.Done;
                }
                
                if (closestTile != null)
                {
                    closestLocation = MapManager.TileToWorldPos(closestTile); // MapManager.EdgeOfTileToWorldPos(closestTile.X, closestTile.Y);
                }
                else
                {
                    return GoalEvaluator.CalculateResult.Done;
                    //return GoalEvaluator.CalculateResult.Done;
                }

                travelTimeScore = GoalEvaluator.GetTravelScoreFromDistance(entity, distanceToClosestTile.Value);
           

                /* OLD: test just 4 corners:
                TerrainTile currentTile;
                bool isLastCorner;
                for (int index = 0; index < 4; index++)// Loop through the corners,  test discomfort in tiles - distance is OK if we are standing on a blocked subtile!
                {
                    isLastCorner = index == 3;

                    currentTile = scoutingJob.Zone.MapArea.GetCornerFromIndex(index);

                    jobLocation = MapManager.TileToWorldPos(currentTile);

                    result = ScoreTravelTime(regionMap, entity.Location, jobLocation, entity, ref travelTimeScore);

                    if (result == RegionMap.Result.OK)
                    {
                        // good, now see if it is accessible (Result is OK if we are standing on an inaccessible subtile...)
                        if (!WorkSiteIsSafe(entity, jobLocation, threatStanceToUse, false))
                        {
                            if (isLastCorner)// are we on the last corner, all corners have failed to deliver a result
                            {
                                return CalculateResult.Done;
                            }
                        }
                        else
                        {
                            break;// we have a valid tile, no need to check the rest
                        }
                    }

                    if (result == RegionMap.Result.Wait)
                    {
                        return CalculateResult.Processing;
                    }
                    else if (result == RegionMap.Result.NoAccess)
                    {
                        if (isLastCorner)// are we on the last corner, all corners have failed to deliver a result
                        {
                            return CalculateResult.Done;
                        }
                    }
                }
                */

            }

            double skillScore = ScoreSkill(entityIntelligence.GetSkillValue(GameData.Instance.AllSkillTypes["foraging"]));

            rating =
                0.2 * skillScore +
                0.8 * travelTimeScore;


            rating = AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, Priority);

            //   rating *= Priority; 

            EvaluateJob.ApplyJobPriorityModifier(scoutingJob.Priority, ref rating); // apply the user priority

            return CalculateResult.Done;


        }



        private CalculateResult HandleJob(ScoutingJob job, double? ageContribution, double? timeContribution)
        {
            if (entity.EntityType.IntelligenceType.CanExamine != true
                && job.Examine)
            {
                return CalculateResult.Done;
            }

            if (!entityIntelligence.Brain.IsSame(job)) // don't consider a job we are already doing!
            // goal type is don't care in this case. 
            {
               // some jobs could be armed scouting (search and destroy?) mission with bold stance...
                ThreatStance threatStanceToUse;
                RegionMap regionMapToUse = GetRegionMapAndStanceForEvaluator(entity, job, out threatStanceToUse);
                
                double currentRating;
                Vector3? closestLocation = null;

                int proposedNumberOfWorkers = Common.Clamp(job.TakenBy.Count + 1, 0, job.MaxJobPositions);
                if (ScoreThisJob(regionMapToUse, threatStanceToUse, entity, job, proposedNumberOfWorkers, ageContribution, timeContribution, out currentRating, out closestLocation) == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }

                GetJobsToCancel(job);

                double ourScore = currentRating;

                if (currentRating > bestScore 
                    && IsScoreBetterThanAllInvolveds(ourScore, needsToBeCancelled, entity))
                {
                    
                   /* if (job.TakenBy.Count < job.MaxJobPositions || job.TakenBy.IsScoreGreaterThanAnyTaker(utilityToTest))
                    {   // if job roster is full, is our score better than the current takers'?
                    */
                        bestScore = currentRating;
                        mostDesirableJob = job;

                        locationInScoutingArea = closestLocation;

                        if (locationInScoutingArea.HasValue &&
                            Common.DistanceOctile(locationInScoutingArea.Value, entity.PlaySiteLocation) > 1500) // 30 tiles
                        {

                        }
                   // }
                }

                // for debugging feedback. also include hauling jobs?
                entityIntelligence.TopScoringJobs.Add(new GoalAndScore() { Score = currentRating, Goal = job.ToString() });
            }

            return CalculateResult.Done;

        }

        private void GetJobsToCancel(ScoutingJob job)
        {
           
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
                    needsToBeCancelled.Add(takenBy[0]);
                    takenBy.RemoveAt(0);
                }
            }
            //if (job.TakenBy.Count >= job.MaxJobPositions)
            //{
            //    needsToBeCancelled.Add(job.TakenBy.GetLowestScorer());

            //    //Can try to cancel a already canceled job
            //    //Only adds one job instead of as many needed untill job.TakenBy.Count is less than job.MaxJobPositions
            //    //Logic problem?
            //}

          /*  if (combo.Weapon != null)
            {

                if (combo.Weapon.EntityType.ItemType != null)
                {
                    if (combo.Weapon.TargetedForPickupBy != null)
                    {
                        needsToBeCancelled.Add(combo.Weapon.TargetedForPickupBy);
                    }

                    Entity weaponEntity = combo.Weapon as Entity;
                    if (weaponEntity != null) // is non-null if equipped by allegiance
                    {
                        if (combo.Weapon.EquippedBy != null)
                        {
                            // force drop the item
                            itemsToBeDropped.Add(weaponEntity);
                        }
                    }
                }

                if (combo.Weapon.AssignedToJob != null)
                {
                    if (combo.Weapon.AssignedToJob.TakenBy.Count > 0)
                    {
                        needsToBeCancelled.Add(combo.Weapon.AssignedToJob.TakenBy.Get(0));
                    }
                }



            }*/

            // remove duplicates:
            needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
            itemsToBeDropped = itemsToBeDropped.Distinct().ToList();
        }


        public override bool CancelCurrentTakers()
        {
            return CancelEntities(needsToBeCancelled, itemsToBeDropped);
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

                GetJobsToCancel(mostDesirableJob);
                {

                    if (IsScoreBetterThanAllInvolveds(bestScore, needsToBeCancelled))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool SetGoal()
        {
            base.SetGoal();

            Intelligence entityIntelligence = entity.Intelligence;

            //if (!CancelEntities(needsToBeCancelled, itemsToBeDropped))
            //{
            //    return; // failed to cancel jobs/ force drop of tools
            //}

            if (entity.Name != null && (entity.Name.Contains("onlan")))
            {
                int i = 0;
            }

            entityIntelligence.SetTopLevelGoal(
                new GoalScouting(entity, mostDesirableJob, GetOwnerIDs(ownersOfVehicles), locationInScoutingArea) { GoalEvaluator = this }, bestScore);
            return true;
        }
    }
}
