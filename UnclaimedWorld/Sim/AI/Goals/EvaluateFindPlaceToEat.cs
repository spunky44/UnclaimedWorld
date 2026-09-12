using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Jobs;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// if needed, find a place to eat. Should be active in work, leisure period
    /// for persons, join a meal session if possible. otherwise, grab some item to eat...
    /// </summary>
    public class EvaluateFindPlaceToEat: GoalEvaluator
    {
       
        public EvaluateFindPlaceToEat(Entity entity): base(entity)
        {
            
        }

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            
            Person personEntity = entity.PersonEntity;

          /*  double timeOfDayContribution = ScoreTimeOfDay(); // ???
            
            if (!(personEntity.HasEatenToday))
            {
                if (!(personEntity.HasPlaceToEat))
                {
                    Household household = personEntity.Household;
                    if (household.Home != null)
                    {
                        if (household.CookingJob.Count > 0)
                        {
                            if (!((CookingJob)household.CookingJob[0]).IsInProgress())
                            {
                                ((CookingJob)household.CookingJob[0]).WillBeEating.Add(entity);
                                personEntity.HasPlaceToEat = true;
                            }
                        }
                        else
                        {
                            CookingJob cJob = new CookingJob(household.Home, household.Ownership, household.CookingJob);
                            ((CookingJob)household.CookingJob[0]).WillBeEating.Add(entity);

                            personEntity.HasPlaceToEat = true;
                        }
                    }

                }
                // TODO: if it is getting late, and still not eaten, find food some other way...
            }*/
         
            result = 0;
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

            return false;//No goals?
      //      AddGoalIfNotPresent(entity, new GoalMoveInToNewHome(entity, mostDesirableHome, ((PersonEntity)entity).Household.Ownership));          
        }
    }
}
