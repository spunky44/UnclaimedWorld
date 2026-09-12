using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI.Constants;

namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// hunt all the creatures that are permitted in the zone
    /// </summary>
    public class FindPreyJob : Job, IRequiresWeapon
    {
        /// <summary>
        /// a spot on the edge of the searched area
        /// </summary>
      //  public Vector3? Location;

        public Zone Zone;
        ZoneID? snapshotZone;

        // should read Zone to find the creature types that can be hunted.
      //  public EntityType TypeToHunt;

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

        public ItemType.TaskType TaskType
        {
            get
            {
                return ItemType.TaskType.UnspecifiedHunting;
            }
        }

        public FindPreyJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public FindPreyJob(Zone zone, EntityGroup entityGroup) //, Priority priority)
            : base(entityGroup, false) //, priority)
        {
            this.Zone = zone;

            zone.ZoneHunt.FindPreyJobs.Add(this);

            entityGroup.AddJob(this); // needs the zone

            ComputeJobType();
            SetDefaultPriority(entityGroup);
        }

        /* public FindPreyJob(Zone zone, EntityType entityType, EntityGroup entityGroup) //, Priority priority)
             : base(entityGroup) //, priority)
         {
             this.Zone = zone;
            // TypeToHunt = entityType;

             ComputeJobType();
             SetDefaultPriority(entityGroup);
         }*/

        public override Vector3? GetCircaLocation()
        {
            return Zone?.MapArea?.GetCenter();
        }

        public GoalEvaluator.CalculateResult ScoreWeapon(Entity entity, IKnownEntityData weapon, Vector3 fromLocation,Vector3 LocationToGetTo,
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


            double effectivenessScore = weapon.EntityType.ItemType.GetTaskAppropriateLevel(TaskType);
            if (Common.IsGreaterThan(effectivenessScore, 0f))
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

                return GoalEvaluator.CalculateResult.Done;
            }

        }

       

        public GoalEvaluator.CalculateResult ScoreThisJobWithoutWeapons(RegionMap regionMap, ThreatStance threatStance, Entity entity, int proposedNumberOfWorkers,
           double? ageContribution, double? timeContribution, ref double rating,
            /*Owner ownerOfInputItems,*/ float priority, ref Vector3? currentJobLocation, EntityGroup ownerOfJobs)
        {
            if (entity.EntityType.IntelligenceType.CanHunt != false) 
            {
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

                double skillScore = GoalEvaluator.ScoreSkill(entity.Intelligence.GetSkillValue(GameData.Instance.AllSkillTypes["hunting"]));

                double outputImportance = ScorePreyImportance(ownerOfJobs);

                EvaluatorWeights weights = GameData.Instance.AIConstants.EvaluatorWeights;

               
                double jobScore =
                    weights.FindPreyTravelTimeWeight * travelTimeScore + // 0.45
                    weights.FindPreySkillWeight * skillScore + //  0.25
                    weights.FindPreyImportanceWeight * outputImportance + // 0.10                         
                    0.2 * marginalLaborScore;

               /* double jobScore =
                    0.7 * travelTimeScore +
                    0.2 * marginalLaborScore;
                */


                rating = jobScore;

                rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
            }
            else
            {
                rating = 0;
            }

            return GoalEvaluator.CalculateResult.Done;
        }

        public double ScorePreyImportance(EntityGroup owner)
        {
            if (this.Zone.ZoneHunt.CreaturesToHunt != null)
            {
                // use the max importance
                float maxImportance = 0f;
                foreach (var item in Zone.ZoneHunt.CreaturesToHunt)
                {
                    if (item.Value > 0)
                    {
                        float thisImportance = owner.GetImportance(item.Key.BiologicalType.CarcassType);

                        maxImportance = Math.Max(maxImportance, thisImportance);
                    }
                }

                return maxImportance;
            }
            else return 0.5;
        }

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
                   
                    if(weaponresult == GoalEvaluator.CalculateResult.Processing)
                    {
                        return GoalEvaluator.CalculateResult.Processing;
                    }
                }
                rating = CombineJobAndWeaponScore(jobData.JobScore, weaponScore);

                EvaluateJob.ApplyJobPriorityModifier(Priority, ref rating); // apply the user priority

            }
            return GoalEvaluator.CalculateResult.Done;
        }

        public override string GetName()
        {
            return "Finding prey";
        }

        public override void Destroy(bool removeTakers, Entity entityToExcludeFromCancel = null)
        {
             //remove the zone order
            if (Zone != null)
            {
                Zone.RemoveFindPreyJob(this); // ZoneHunt.FindPreyJobs.Remove(this);
              /*  if (Zone.FindPreyJob != null)
                {
                    Zone.FindPreyJob = null;
                }*/
            }

            base.Destroy(removeTakers, entityToExcludeFromCancel);

        }

        public override bool UserCanCancel(out string reason)
        {
            reason = "";
            return false;
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
        
           // TypeToHunt = sn.DoGameData(TypeToHunt);

            weaponsAreAvailable = sn.DoBool(weaponsAreAvailable);

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
