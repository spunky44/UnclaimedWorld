using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs
{
    public class CheckProcessJob: Job
    {
        public Vector3 Location;

       // public HashSet<JobID> ProcessJobsToCheckOn;
        public List<JobID> ProcessJobsToCheckOn = new List<JobID>();

        public CheckProcessJob(Vector3 location, EntityGroup entityGroup) 
            : base(entityGroup) 
        {
            this.Location = location;

            ComputeJobType();
            SetDefaultPriority(entityGroup);
        }



        public CheckProcessJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }



        public override void GetLocation(out Point? tile, out EntityID? targetEntity, out ZoneID? zoneID)
        {
            tile = null;
            targetEntity = null;
            zoneID = null;

            tile = MapManager.WorldPosToTile(Location);           
        }


        public GoalEvaluator.CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStance, Entity entity, int proposedNumberOfWorkers,
          double? ageContribution, double? timeContribution, ref EvaluateJob.ToolOrWeaponInstanceComboJobData jobData, ToolParams? toolParams, ref double rating, ref bool jobIsValid,
             Dictionary<Job, EvaluateJob.ToolOrWeaponInstanceComboJobData> cachedJobScores, Intelligence entityIntelligence, float priority, EntityGroup ownerOfJobs, bool cacheScore)
        {

            if (!cachedJobScores.TryGetValue(this, out jobData))
            {
                // double jobScore = 0d;
                jobData = new EvaluateJob.ToolOrWeaponInstanceComboJobData();

                GoalEvaluator.CalculateResult jobResult;

                jobResult = ScoreThisJobWithoutWeapons(regionMap, threatStance, entity, proposedNumberOfWorkers, ageContribution, timeContribution, ref jobData.JobScore, priority, ref jobData.Location, ownerOfJobs);
               

                if (jobResult != GoalEvaluator.CalculateResult.Done)
                {
                    return GoalEvaluator.CalculateResult.Processing;
                }
                else
                {
                    // don't cache when we are calculating an updated job score:
                    if (cacheScore)
                    {
                        cachedJobScores.Add(this, jobData);
                    }
                }
            }

            if (jobData.JobScore > 0d) // continue scoring the tools if the job is valid.
            {

                rating = 0.7f * jobData.JobScore; // CombineJobAndWeaponScore(jobData.JobScore, weaponScore);

                EvaluateJob.ApplyJobPriorityModifier(Priority, ref rating); // apply the user priority

            }
            return GoalEvaluator.CalculateResult.Done;
        }



        public GoalEvaluator.CalculateResult ScoreThisJobWithoutWeapons(RegionMap regionMap, ThreatStance threatStance, Entity entity, int proposedNumberOfWorkers,
           double? ageContribution, double? timeContribution, ref double rating,
            /*Owner ownerOfInputItems,*/ float priority, ref Vector3? currentJobLocation, EntityGroup ownerOfJobs)
        {
            if (entity.EntityType.IntelligenceType.CanCheckProgress != false) 
            {
               
                double travelScore = 0;
                RegionMap.Result travelScoreResult = GoalEvaluator.ScoreTravelTime(regionMap, threatStance, entity.AccessPoint.Value,
                        Location, entity, ref travelScore, this, ownerOfJobs.Parent);

                if (travelScoreResult == RegionMap.Result.Wait)
                {
                    // wait for the result...                       
                    return GoalEvaluator.CalculateResult.Processing;
                }
                else if (travelScoreResult == RegionMap.Result.NoAccess)
                {
                    rating = 0;
                    return GoalEvaluator.CalculateResult.Done;
                }

                
                double marginalLaborScore = GoalEvaluator.ScoreNumberOfWorkers(proposedNumberOfWorkers, MaxJobPositions);


                double jobScore = travelScore;
                /*
                    0.7 * travelTimeScore +
                    0.2 * marginalLaborScore;*/

                rating = jobScore;

                rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
            }
            else
            {
                rating = 0;
            }

            return GoalEvaluator.CalculateResult.Done;
        }

        public override Vector3? GetCircaLocation()
        {
            return Location;         
        }

        public override string GetName()
        {
            return "Checking progress"; 
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
                       
            Location = sn.DoVector3(Location);
            ProcessJobsToCheckOn = sn.DoList(ProcessJobsToCheckOn);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

        }


        #endregion
    }
}
