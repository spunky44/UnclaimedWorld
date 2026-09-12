using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.AI.Goals
{
    public struct ToolParams
    {
        public List<EntityID> Tools;
        public EntityID? ImmovableTool;
        public float? ToolProductivity;
        public Dictionary<EntityType, ReplenishStatus> ReplenishStatus;
        public float? JobDurationInDays;
    }

    public struct AttackParams
    {
        public AttackType AttackType;
        public EntityAndRoot? Weapon;
        public BodyPartID BodyPartToAttackID;       
    }

    public struct HaulingParams
    {
        public EntityID Item;
        public IKnownEntityData Vehicle;
    }

    public struct ScoutParams
    {
        public Vector3? LocationInScoutArea;
        
    }

    interface IScoreJob
    {
        /// <summary>
        /// when this method is called by the evaluator during scoring, typically a lot of the parameters are computed once and used in each call.
        /// but when called when the updated score is required, the parameters will often have the value null and need to be recomputed by the method to give an accurate score.
        /// </summary>
        /// <param name="regionMap"></param>
        /// <param name="entity"></param>
        /// <param name="job"></param>
        /// <param name="proposedNumberOfWorkers"></param>
        /// <param name="ageContribution"></param>
        /// <param name="timeContribution"></param>
        /// <param name="rating"></param>
        /// <param name="fitnessScore"></param>
        /// <param name="toolParams"></param>
        /// <returns></returns>
        GoalEvaluator.CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Jobs.Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating, 
            ToolParams? toolParams = null, AttackParams? attackParams = null, HaulingParams? haulingParams = null); // List<Entity> tools = null, float? toolProductivity = null);
        
    }
}
