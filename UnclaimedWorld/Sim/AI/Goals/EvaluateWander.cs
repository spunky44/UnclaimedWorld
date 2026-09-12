using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.SimSide.Maps;
namespace UWGame.SimSide.AI.Goals
{
    class EvaluateWander: GoalEvaluator
    {
        //private Rectangle stayInside;

        private double mostDesirableScore = 0;

        public override float Priority
        {
            get
            {
                return 1000f;
            }
        }

        public EvaluateWander(Entity entity/*, Rectangle stayInside*/): base(entity)
        {
          //  this.stayInside = stayInside;
        }

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            if (entity.Locomotor.LeggedLocomotor.TestWander) 
            {
                result = 1000; // GameData.Instance.AIConstants.IdleGoalDesirability + GameData.Instance.AIConstants.AmountNewGoalMustBeBetterToForceSwitch; 
                mostDesirableScore = result;
                //  result = score;
            }
            else
            {
                result = 0;
                mostDesirableScore = 0;
            }

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
            Intelligence entityIntelligence = entity.Intelligence;

            //entityIntelligence.CurrentGoalUtility = mostDesirableScore;

            // let's head the other direction:
            int horizShift = -1 * Math.Sign(entity.FacingNormal.X);
            int vertShift = -1 * Math.Sign(entity.FacingNormal.Y);

            Rectangle stayInside; // = new Rectangle(entity.MapPosition.X - 2 + 2 * horizShift, entity.MapPosition.Y - 2 + 2 * vertShift, 4, 4);

           // int maxX, maxY, minX, minY;
            stayInside = The.Map.GetClampedMapAreaUsingTiles(new Point(entity.MapPosition.Value.X - 2 + 2 * horizShift, entity.MapPosition.Value.Y - 2 + 2 * vertShift),
                4, 4);

            // we will wander through anything...
            entityIntelligence.SetBoldStance();

            entityIntelligence.SetTopLevelGoal(new GoalWander(entity, stayInside), mostDesirableScore);
            return true;
        }
    }
}
