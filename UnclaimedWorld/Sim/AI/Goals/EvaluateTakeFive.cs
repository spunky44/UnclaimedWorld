using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.Control;
namespace UWGame.SimSide.AI.Goals
{
    class EvaluateTakeFive: GoalEvaluator
    {
        private double mostDesirableScore = 0;

        public EvaluateTakeFive(Entity entity): base(entity)
        {           
        }

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            // use a random spread... forgot the reason...
            result = GameData.Instance.AIConstants.IdleGoalDesirability + (The.Sim.GameplayRandomGenerator.NextDouble("EvaluateTakeFive") * 3d - 1d) * IdleGoalDesirabilitySpread;
            mostDesirableScore = result;

            return CalculateResult.Done;
        }

        public override bool CancelCurrentTakers()
        {
            return true;// throw new NotImplementedException();
        }
        public override bool CanTakeGoal()
        {
            return true;// throw new NotImplementedException();
        }
        public override bool SetGoal()
        {
            base.SetGoal();

            // remember to set this! Otherwise the previous value will be used again.
           // entityIntelligence.CurrentGoalUtility = mostDesirableScore;
            
            // don't use random durations... pick the whole lenght of one idle animation.
            entityIntelligence.SetTopLevelGoal(new GoalTakeFive(entity), mostDesirableScore); //, Globals.Instance.Random.Next(1, 4)));



            return true;
        }


        public override bool IsIdleActivity()
        {
            return true;
        }
    }
}
