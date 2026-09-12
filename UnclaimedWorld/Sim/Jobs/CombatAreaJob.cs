using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs
{
    public abstract class CombatAreaJob : Job, IRequiresWeapon, IVariableMaxTakerJob
    {
        /// <summary>
        /// a spot on the edge of the searched area
        /// </summary>
        //  public Vector3? Location;

        public Zone Zone;
        ZoneID? snapshotZone;

        /// <summary>
        /// extends a bit beyond the bounding rect of the zone
        /// </summary>
        public Rectangle ThreatArea;


        /// <summary>
        /// the agent is allowed to leave the job/zone briefly to attack threats, during this period other agents should ignore the job.
        /// </summary>
        /* public EntityID? LastAgentOnPost;
         public double? TimepointForLastAgentOnPost;*/

        /// <summary>
        /// This is a pulse or heartbeat for all patrollers to update every frame AFTER they have reached the area.
        /// They can abandon the job for X seconds max.
        /// X seconds after their last update the agents are removed. Before that happens, no-one is allowed to take their slot.    
        /// </summary>
        private List<Tuple<EntityID, double>> lastAgentsOnPosts = new List<Tuple<EntityID, double>>();

        private int maxTakers;


        bool weaponsAreAvailable = true;
        /// <summary>
        /// used in task panel feedback
        /// </summary>
        public bool WeaponsAreAvailable
        {
            get
            {
                return weaponsAreAvailable;
            }

            set
            {
                weaponsAreAvailable = value;
            }
        }

        public abstract bool CanAttackVermin { get; }

        public abstract bool CanAttackTargetsOutsideZone { get; }


        public override int MaxJobPositions
        {
            get
            {
                return maxTakers;
            }
        }

        public CombatAreaJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public CombatAreaJob(Zone zone, EntityGroup entityGroup, int noOfPatrollers)
            : base(entityGroup) 
        {
            this.Zone = zone;
         
            this.maxTakers = noOfPatrollers;

            Rectangle inflatedRectangle = zone.MapArea.BoundingRectangle.Value;

            int zoneDistanceInTiles = (int)Math.Round(GameData.Instance.AIConstants.MaxDistanceOutsidePatrolZoneToChaseTargets * MapManager.oneOverTileSize);
            

            inflatedRectangle.Inflate(zoneDistanceInTiles, zoneDistanceInTiles);

            ThreatArea = The.Map.GetClampedMapAreaUsingTiles(inflatedRectangle);

            ComputeJobType();
            SetDefaultPriority(entityGroup);
        }


        public override Vector3? GetCircaLocation()
        {
            return Zone?.MapArea?.GetCenter();


        }

        public void SetJobPositions(int positions)
        {
            maxTakers = positions;
        }

        public ItemType.TaskType TaskType
        {
            get
            {
                return ItemType.TaskType.PatrolOrAttack;
            }
        }



        public void RemoveLastAgentOnPost(EntityID entity)
        {
            lastAgentsOnPosts.RemoveAll(t => t.Item1 == entity);
        }

        public void SetLastAgentOnPost(EntityID entity)
        {
            lastAgentsOnPosts.RemoveAll(t => t.Item1 == entity);
            lastAgentsOnPosts.Add(new Tuple<EntityID, double>(entity, The.Sim.TotalUnPausedGameTimeInSeconds));
        }


        public GoalEvaluator.CalculateResult ScoreWeapon(Entity entity, IKnownEntityData weapon, Vector3 fromLocation, Vector3 LocationToGetTo,
                double? ageContribution, double? timeContribution,
                Intelligence entityIntelligence, RegionMap regionMap, float priority, out double score)
        {
            score = 0d;
            double locationScore;
            float distanceToWeapon; // not used

            SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;

            GoalEvaluator.CalculateResult locationResult = ProcessJob.ScoreToolLocation(entity, weapon, fromLocation, LocationToGetTo, sharedKnowledge,
                                                                                    regionMap, out distanceToWeapon, out locationScore);
            if (locationResult == GoalEvaluator.CalculateResult.Processing)
            {
                return GoalEvaluator.CalculateResult.Processing;
            }

            double conditionScore = weapon.Condition.Value;

            EntityGroup owner;
            if (!ResolveOwner(out owner))
            {
                return GoalEvaluator.CalculateResult.Done;
            }
                       
            double effectivenessScore = weapon.EntityType.ItemType.GetTaskAppropriateLevel(TaskType);

            double policyScore = 1f;
            Expedition expedition = owner.GetExpedition();
            if (expedition != null && expedition.Policy != null)
            {
                policyScore = expedition.Policy.GetWeaponPolicyScore(CanAttackVermin, weapon.EntityType, null);
            }           
           

            if (Common.IsGreaterThan(effectivenessScore, 0f)
               && Common.IsGreaterThan(policyScore, 0f))
            {
                score = 0.5f * locationScore + 0.2f * conditionScore + 0.3f * effectivenessScore; // +0.2f * ammoScore;
            }

            if (Common.IsZero(score))
            {
                return GoalEvaluator.CalculateResult.Done;
            }
            else
            {

                score = GoalEvaluator.AddTimeAgeAndPriority(score, timeContribution.Value, ageContribution.Value, priority);
                score = Common.Min((float)score, 1f);
                return GoalEvaluator.CalculateResult.Done;
            }

        }




        public override bool RequiresBoldStance
        {
            get
            {
                return true;
            }
        }

        public GoalEvaluator.CalculateResult ScoreThisJobWithoutWeapons(RegionMap regionMap, ThreatStance threatStance, Entity entity, int proposedNumberOfWorkers,
           double? ageContribution, double? timeContribution, ref double rating,
            /*Owner ownerOfInputItems,*/ float priority, ref Vector3? currentJobLocation, EntityGroup ownerOfJobs)
        {
            if (entity.EntityType.IntelligenceType.CanPatrol != false)
            {
                if (!EntityIsAllowedToTakeJob(entity)) // AnyTakerMayStillReturn(entity))
                {
                    return GoalEvaluator.CalculateResult.Done;
                }

                float? distanceToClosestTile;
                TerrainTile closestTile;

                //TODO: Add assert about the region map transporation if it isnt by foot
                RegionMap.Result getClosestTileResult = Zone.GetClosestSafeEdgeTile(this, entity.PlaySiteLocation, regionMap, threatStance, entity, out distanceToClosestTile, out closestTile, ownerOfJobs);

                if (getClosestTileResult == RegionMap.Result.Wait)
                {
                    return GoalEvaluator.CalculateResult.Processing;
                }
                else if (getClosestTileResult == RegionMap.Result.NoAccess)
                {
                    return GoalEvaluator.CalculateResult.Done;//throw new Exception("Could not find a valid tile to move to!");
                }

                if (closestTile != null)
                {
                    currentJobLocation = MapManager.TileToWorldPos(closestTile); // MapManager.EdgeOfTileToWorldPos(closestTile.X, closestTile.Y);
                }
                else
                {
                    return GoalEvaluator.CalculateResult.Done;
                    //return GoalEvaluator.CalculateResult.Done;
                }

                double travelTimeScore = GoalEvaluator.GetTravelScoreFromDistance(entity, distanceToClosestTile.Value);

                double marginalLaborScore = GoalEvaluator.ScoreNumberOfWorkers(proposedNumberOfWorkers, MaxJobPositions);

                double specialistPenaltyScore = GoalEvaluator.ScoreUniqueSkillPenalty(entity); // give a negative weight for specialists with unique skills. prefer someone else to do the patrolling...

                float penaltyWeight = GameData.Instance.AIConstants.EvaluatorWeights.MundaneJobSpecialistPenalty; // 0.1

                float awayFromPostInertiaScore = ScoreAwayFromPostInertia(entity); // gives incentive to take the area job again after leaving (for instance to fight)

                // use lower weights since there are fewer components:
              /*  double jobScore =
                    0.65 * travelTimeScore + 
                    0.2 * marginalLaborScore +
                    penaltyWeight * specialistPenaltyScore; // weight is 0.1, score is negative or zero!
                */

                double jobScore =
                    0.65 * travelTimeScore +
                    0.2 * marginalLaborScore +
                    0.1 * awayFromPostInertiaScore + // NEW
                    penaltyWeight * specialistPenaltyScore; // weight is 0.1, score is negative or zero!
             

                rating = jobScore;

                rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
            }
            else
            {
                rating = 0;
            }

            return GoalEvaluator.CalculateResult.Done;
        }


        private float ScoreAwayFromPostInertia(Entity entity)
        {
            // only for non-taken jobs. replaces inertia.
            CombatAreaJob lastAreaJob = entity.Intelligence.Memory.GetLastCombatAreaJob();
            if (lastAreaJob != null && lastAreaJob.ID == this.ID)
            {
                if (!lastAreaJob.TakenBy.Contains(entity))
                {
                    return 1f;
                }              
            }

            return 0f;
        }

        /// <summary>
        /// does cleanup first
        /// </summary>
        /// <returns></returns>
        private List<Tuple<EntityID, double>> GetLastAgentsOnPost()
        {

            for (int i = lastAgentsOnPosts.Count - 1; i >= 0; i--)
            {
                Tuple<EntityID, double> lastAgent = lastAgentsOnPosts[i];

                if (Common.TimepointIsOutDated(lastAgent.Item2, GameData.Instance.AIConstants.MaxTimeForLeavingPatrolPostUntilFreed))
                {
                    lastAgentsOnPosts.RemoveAt(i);
                }
            }

            return lastAgentsOnPosts;
        }


        public override void Abandon(Entity entity, bool isDestroyingJob = false)
        {
            base.Abandon(entity, isDestroyingJob);
        }


        /// <summary>
        /// The purpose here is to leave job slots closed while the patrollers are fighting.
        /// 
        /// Those who are allowed:
        /// 1. Current takers, (perhaps rescoring their current job)
        /// 2. Previous takers who have briefly vacated their post
        /// 
        /// 1 can be in the lastAgentsOnPost list.
        /// 2 is in the lastAgentsOnPost list.
        /// 
        /// 3. New takers, if there are slots left not taken or briefly vacated. MaxJobs - lastAgentsOnPost.Count
        /// 
        /// fail the scoring if any agents have left within t time, and it is not ourselves.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool EntityIsAllowedToTakeJob(Entity entity)
        {
            var listOfTakers = GetLastAgentsOnPost();

            if (TakenBy.Contains(entity)
                || listOfTakers.Exists(t => t.Item1 == entity.ID))
            {
                return true;
            }
 
            // new taker. check that there are slots left:
            if (TakenBy.Count < MaxJobPositions)
            {
                int agentsWhoHaveLeft = listOfTakers.Count(t => !TakenBy.Contains(t.Item1));
               
                if (agentsWhoHaveLeft + TakenBy.Count < MaxJobPositions)
                {
                    return true;
                }
            }

            return false;

         /*   int agentsWhoHaveLeft = listOfTakers.Count(t => t.Item1 != entity.ID && !TakenBy.Contains(t.Item1));
           
            if (agentsWhoHaveLeft > 0)
            {
                return true;
            }
            else return false;*/
        }

        /*
        private bool AnyTakerMayStillReturn(Entity entity)
        {
            var listOfTakers = GetLastAgentsOnPost();

            int agentsWhoHaveLeft = listOfTakers.Count(t => t.Item1 != entity.ID && !TakenBy.Contains(t.Item1));

            if (agentsWhoHaveLeft > 0)
            {
                return true;
            }
            else return false;
        }*/


        public double CombineJobAndWeaponScore(double jobScore, double weaponScore)
        {
            if (Common.IsZero(jobScore) || Common.IsZero(weaponScore))
            {
                return 0d;
            }
            else
            {
                double rating = 0.9 * jobScore + 0.1 * weaponScore;
                return rating;
            }
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
                //ref jobData.JobScore, ref jobData.InputItem, //ref jobData.AgentAssignedLocation,
                //ownerOfJobs, 

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
                // now score the tools:
                double weaponScore = 1d;
                if (toolParams.HasValue && toolParams.Value.Tools.Count > 0)
                {
                    //  Vector3 jobLocation;
                    IKnownEntityData weapon;
                    entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(toolParams.Value.Tools[0], out weapon);

                    if (jobData.Location == null)
                    {
                        jobIsValid = false;
                        return GoalEvaluator.CalculateResult.Done;
                    }
                    else
                    {
                        //  jobLocation = weapon.AccessPoint;//currentJobLocation.Value;
                    }

                    GoalEvaluator.CalculateResult weaponresult = ScoreWeapon(entity, weapon, entity.PlaySiteLocation, jobData.Location.Value,
                         ageContribution, timeContribution, entityIntelligence, regionMap, priority, out weaponScore);

                    if (weaponresult == GoalEvaluator.CalculateResult.Processing)
                    {
                        return GoalEvaluator.CalculateResult.Processing;
                    }
                }
                rating = CombineJobAndWeaponScore(jobData.JobScore, weaponScore);

                EvaluateJob.ApplyJobPriorityModifier(Priority, ref rating); // apply the user priority

            }
            return GoalEvaluator.CalculateResult.Done;
        }

        public override void GetLocation(out Point? tile, out EntityID? targetEntity, out ZoneID? zoneID)
        {
            tile = null;
            targetEntity = null;
            zoneID = null;

            if (Zone != null)
            {
                zoneID = Zone.ID;
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

            snapshotZone = sn.SnapshotID<Zone, ZoneID>(Zone);          
            ThreatArea = sn.DoRectangle(ThreatArea);
            lastAgentsOnPosts = sn.DoList(lastAgentsOnPosts);
            weaponsAreAvailable = sn.DoBool(weaponsAreAvailable);
            maxTakers = sn.DoInt32(maxTakers);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            Zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone);
        }


        #endregion
    }
}
