using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Jobs
{
    public class LightFireJob: Job
    {
        
        public float ManSecondsOfWorkNeeded = 0f;

        /// <summary>
        /// also a tile?
        /// </summary>
        public Entity FireSite;
       

        /// <summary>
        /// Value between 0 - 1
        /// </summary>
        public float Progress;

        public LightFireJob(Entity fireSite, EntityGroup entityGroup)
            : base(entityGroup) //, Priority.Normal) 
        {           
            FireSite = fireSite;

            ComputeJobType();
            SetDefaultPriority(entityGroup);
        }

        public LightFireJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public bool IsInProgress()
        {
            return Progress > 0f;
        }


        public GoalEvaluator.CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, double? ageContribution, double? timeContribution, ref double rating, float priority)
        {
            double travelTimeScore = 0;
            RegionMap.Result result = GoalEvaluator.ScoreTravelTime(regionMap, threatStanceToUse, entity.AccessPoint.Value, // #ACCESS .Location.Value, 
                FireSite.Location.Value, entity, ref travelTimeScore);

            if (result == RegionMap.Result.Wait)
            {
                // wait for the result...
                //rating = 0;
                return GoalEvaluator.CalculateResult.Processing;
            }
            else if (result == RegionMap.Result.NoAccess)
            {
                rating = 0;
                return GoalEvaluator.CalculateResult.Done;
            }

            double materialsEstimate = 1f; // = EstimateMaterialsReady(cookingJob, entity);

            RequiresEnergy energy;
            Tool tool;

            if (FireSite.Find(out tool) && 
                  FireSite.EntityType.ContainerType != null && FireSite.EntityType.ContainerType.GetRequiresReplenishType() != null)         
              //  FireSite.Find(out energy)) 
            {
               
                if (tool.IsPrepared == true)//FireSite.Structure.Fireplace.IsLit)
                {
                    rating = 0f;
                    return GoalEvaluator.CalculateResult.Done;
                }
                else
                {
                    materialsEstimate = Common.Clamp(((IHasReplenishItems)FireSite.Contains).ReplenishItems.RequiresFuel.Fuel, 0f, 1f);
                }
              
            }

            // weigh ready materials higher than distance to site:
            if (Common.IsEqual(materialsEstimate, 0))
            {
                rating = GameData.Instance.AIConstants.JobWithZeroMaterialsDesirability;
            }
            else
            {
                rating = 0.2 * travelTimeScore + 0.4 * materialsEstimate + 0.4;
                //0.1 * progressScore + 0.1 * marginalLaborScore + 0.2 * skillScore;

              //  rating *= priority; // NEW!

                rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
            }

            return GoalEvaluator.CalculateResult.Done;
        }


        public override Vector3? GetCircaLocation()
        {
            return null; // TODO if needed
        }
    }
}
