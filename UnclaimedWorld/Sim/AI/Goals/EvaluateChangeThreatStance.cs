using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// the purpose here is to set threat stance beck to normal or cautious when there are no more threats around.
    /// </summary>
    class EvaluateChangeThreatStance: GoalEvaluator
    {

        public EvaluateChangeThreatStance(Entity entity)
            : base(entity)
        {
            
        }

        public override float Priority
        {
            get
            {
                return 10000f;
            }
        }

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            if (entityIntelligence.ThreatStance != ThreatStance.Bold || // is it necessary to change stance?
                (entityIntelligence.Brain.Subgoals.Count > 0 &&
                entityIntelligence.Brain.Subgoals.Peek().RequiresBoldStance()
               /* (entityIntelligence.Brain.Subgoals.Peek() is GoalAttack // don't change stance if we are currently attacking
                || entityIntelligence.Brain.Subgoals.Peek() is GoalHunt)*/// don't change stance if we are perfomring a job that requires Bold stance                                                                        
                ))
            {   
                // don't change threat stance
                bestScore = 0;

                result = bestScore;

                return CalculateResult.Done;       
            }

            if (entityIntelligence.Allegiance.SharedKnowledge.AllKnownEntities.ThreatJobs.Count == 0)
            {
                bestScore = 1 * Priority;
            }
            else if (!entityIntelligence.StanceCanBeBold()) // we are not in a state to fight...
            {
                bestScore = 1 * Priority;
            }
            else
            {
                IKnownEntityData targetData;
                foreach (ThreatJob job in entityIntelligence.Allegiance.SharedKnowledge.AllKnownEntities.ThreatJobs)
                {                    
                    // if one threat is less than x pixels away, keep the bold stance.

                    if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(job.Target.Value, out targetData)))
                    {
                        continue;
                    }

                    if (Common.DistanceOctile(entity.Location.Value, targetData.Location.Value) < 400f)
                    {
                        bestScore = 0;

                        result = bestScore;

                        return CalculateResult.Done;

                    }
                }

                // change stance from bold to normal if all threats are more than x pixels away.
                bestScore = 1d * Priority;

            }

            result = bestScore;

            return CalculateResult.Done;            
        }



        public override bool CancelCurrentTakers()
        {
            return true;
        }
        public override bool CanTakeGoal()
        {
            return true;
        }
      
        public override bool SetGoal()
        {
            base.SetGoal();

            // no goal is needed.

            entityIntelligence.ResetThreatStance();
            return true;
           // entityIntelligence.CurrentGoalUtility = bestScore;
        }
    }
}
