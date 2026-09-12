using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI.Activities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Biological;
namespace UWGame.SimSide.AI.Goals
{
    class EvaluateLeisureWalk: GoalEvaluator
    {
        private Rectangle stayInside;
        private LeisureWalkActivity bestActivity;

        public List<EntityGroup> OwnersOfActivities;

        private double score = 0.1;

        public EvaluateLeisureWalk(Entity entity, List<EntityGroup> OwnersOfActivities)
            : base(entity)
        {
            this.OwnersOfActivities = OwnersOfActivities;
            //this.stayInside = stayInside;
        }

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            bestActivity = null;

            Intelligence entityIntelligence = entity.Intelligence;

            // TURNED OFF:
            return 0;

            double ageContribution = 1;
            BiologicalEntity bioEntity;

            if (entity.Find(out bioEntity))
            {
                ageContribution = ScoreAge(bioEntity);

                if (ageContribution == 0)
                {
                    result = 0;
                    return CalculateResult.Done;
                }
            }

            double timeOfDayContribution = ScoreTimeOfDay();

            foreach (EntityGroup ownerOfActivities in OwnersOfActivities) // 1 or more sets of activities can be searched...
            {
                foreach (Activity activity in ownerOfActivities.Activities)
                {
                    LeisureWalkActivity leisureWalkActivity = activity as LeisureWalkActivity;
                    if (leisureWalkActivity != null)
                    {
                        if (!activity.HasStarted)
                        {
                            bestActivity = leisureWalkActivity;

                            //float loadedVehicleSpeed = vehicle.CalculateSpeed(item.Bulk);

                            /*    timeSquared = Common.DistanceSquared(entity.MapPosition, vehicle.MapPosition) / entity.CurrentSpeed
                                        + Common.DistanceSquared(vehicle.MapPosition, item.MapPosition) / vehicle.CurrentSpeed
                                        + Common.DistanceSquared(item.MapPosition, to) / loadedVehicleSpeed; // include other items carried?

                                if (bestVehicle == null || timeSquared < bestTimeSquared)
                                {
                                    bestVehicle = vehicle;
                                    bestTimeSquared = timeSquared;
                                }
                                */
                        }
                    }
                }
            }


            if (bestActivity == null)
            {
                if (entity.Intelligence.Memory.TimePointOfFailedLeisureWalkAttempt == 0 ||
                    The.Sim.TotalUnPausedGameTimeInSeconds > (entity.Intelligence.Memory.TimePointOfFailedLeisureWalkAttempt + 5))
                {
                   // create new
                    result = score;
                }
                else
                {
                    result = 0;
                }
            }
            else
            {
                result = score;
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

            if (bestActivity == null)
            {
                // add to the first list? household / colony?
                Vector3 dest = new Vector3(80, 40, 0);
                if (Common.DistanceOctile(entity.PlaySiteLocation, dest) < 50)
                {
                    dest = new Vector3(120, 20, 0);
                }
                bestActivity = new LeisureWalkActivity(OwnersOfActivities[0].Activities, dest);
            }

            //entityIntelligence.CurrentGoalUtility = score;
            entityIntelligence.SetTopLevelGoal(new GoalLeisureWalk(entity, bestActivity), score);
            return true;
        }
    }
}
