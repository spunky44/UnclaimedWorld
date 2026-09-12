using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Buildings;
using GameStateManagement;
using UWGame.SimSide.Maps;
using UWGame.Control;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// if needed, find a (new) home. Should be active in leisure and sleep periods
    /// </summary>
    class EvaluateFindHome: GoalEvaluator
    {
        IKnownEntityData mostDesirableHome;
       
      //  bool buildNewHome = false;

        private float priority;
        public override float Priority
        {
            get
            {
                return priority;
            }
        }

        public EvaluateFindHome(Entity entity)
            : base(entity)
        {
            this.priority = GameData.Instance.AIConstants.PriorityOfFindingANewHome;
        }

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {

            if (entity.PersonEntity.IsHeadOfHousehold())
            {
                /* if (entity.PersonEntity.Household.Home == null)
                 {*/
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


                // often/sometimes the expedition point is blocked by buildings:
                Vector3 closestUnblockedExpeditionLocation = EntityGroup.GetFreeGroundLocation(entityIntelligence.CurrentExpedition); //.GetFreeGroundLocation();


                // TODO: after idling for a long while, unable to get home, we could supply Bold here... 
                ThreatStance threatStanceToUse;
                RegionMap regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);


                bool unused = false;
                double mostDesirableScore = 0; // = minimumRatingToConsider;
                double staticScoreOfCurrentHome = 0;
                if (entity.PersonEntity.Household.Home != null)
                {
                    IKnownEntityData currentHome;
                    if (!EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(entity.PersonEntity.Household.Home.Value, out currentHome)))
                    {
                        double ownHomeScore = 0;

                        bool belongsToHousehold = currentHome.OwnedBy == entity.PersonEntity.Household.GetOwnerID();

                        CalculateResult ownHomeResult = ScoreResidence(entity, regionMapToUse, threatStanceToUse, closestUnblockedExpeditionLocation, currentHome, belongsToHousehold, ref ownHomeScore, ref staticScoreOfCurrentHome, true);
                        if (ownHomeResult == CalculateResult.Processing)
                        {
                            return CalculateResult.Processing;
                        }
                    }
                }

                mostDesirableHome = null;

                double timeOfDayContribution = ScoreTimeOfDay();


                // look at communal buildings
                if (entityIntelligence.CurrentExpedition != null)
                {
                    CalculateResult communalResult = LookForSuitableHomeInList(entity, entityIntelligence.CurrentExpedition.OwnedEntities, regionMapToUse, threatStanceToUse,
                        closestUnblockedExpeditionLocation, false, ref unused, ref mostDesirableScore, staticScoreOfCurrentHome);

                    if (communalResult == CalculateResult.Processing)
                    {
                        return CalculateResult.Processing;
                    }
                }

                bool incompleteHomesInHousehold = false;

                // look at household buildings
                CalculateResult privateResult = LookForSuitableHomeInList(entity, entity.PersonEntity.Household.OwnedEntities, regionMapToUse, threatStanceToUse, closestUnblockedExpeditionLocation,
                    true, ref incompleteHomesInHousehold, ref mostDesirableScore, staticScoreOfCurrentHome);

                if (privateResult == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }

                // only if we found a home do we get a score other than 0:
                if (mostDesirableHome != null) //mostDesirableScore > minimumRatingToConsider)
                {

                    if (entity.ID == (EntityID)5142)
                    {

                    }

                    result = 0.8 * mostDesirableScore + 0.2 * timeOfDayContribution;

                    result *= Priority;

                    return CalculateResult.Done;
                }
                // }
            }

            result = 0;
            return CalculateResult.Done;
        }


        private CalculateResult LookForSuitableHomeInList(Entity entity, EntityGroup ownerOfBuildings, RegionMap regionMapToUse, ThreatStance threatStanceToUse, 
            Vector3 closestUnblockedExpeditionLocation, bool belongsToHousehold, ref bool incompleteHomesInHousehold, ref double bestScore, double staticScoreOfCurrentHome)
        {
            double currentScore = 0;
            double currentStaticScore = 0;
            
            double minStaticScore = 0;
            if (Common.IsGreaterThan(staticScoreOfCurrentHome, 0d))
            {
                minStaticScore = staticScoreOfCurrentHome + GameData.Instance.AIConstants.ResidenceScoreMustBeBetterToMove;
            }

            foreach (KeyValuePair<EntityType, List<EntityID>> kvp in ownerOfBuildings.Structures)
            {
                if (kvp.Key.ContainerType != null && kvp.Key.ContainerType.ResidenceType != null)
                {
                    EntityID id;
                    IKnownEntityData residence;
                    for (int i = kvp.Value.Count - 1; i >= 0; i--)
                    {
                        id = kvp.Value[i];
                        if (id == entity.PersonEntity.Household.Home)
                        {
                            continue;
                        }

                        if (!HandleOwnerDataResult(entityIntelligence.Allegiance.SharedKnowledge, id, ownerOfBuildings, out residence))
                        {
                            continue;
                        }

                        if (IsOnPlaySite(residence)                            
                            && residence.IsCompleted())
                        {
                            CalculateResult result = ScoreResidence(entity, regionMapToUse, threatStanceToUse, closestUnblockedExpeditionLocation, residence, belongsToHousehold, ref currentScore, ref currentStaticScore, false);

                            if (result == CalculateResult.Processing)
                            {
                                return result;
                            }

                            if (currentScore > bestScore
                                && currentStaticScore > minStaticScore) // currentScore > minScore)
                            {
                                bestScore = currentScore;

                                // TODO: depends on distance, location, personal preference, family members
                                mostDesirableHome = residence;
                               
                            }
                        }
                        else
                        {
                            incompleteHomesInHousehold = true;
                        }
                    }
                }
            }

            return CalculateResult.Done;
        }

        private CalculateResult ScoreResidence(Entity entity, RegionMap regionMap, ThreatStance threatStanceToUse, Vector3 closestUnblockedExpeditionLocation, 
            IKnownEntityData residence, bool belongsToHousehold, ref double score, ref double staticScore, bool isOwnResidence)
        {
            //Residence residenceComponent;
           // residence.Find(out residenceComponent);

            bool hasRoom = true; 
            // NEW: must have room for entire household:
            if (isOwnResidence)
            {
                if (!Residence.HasCapacity(entity.PersonEntity.Household, 0, residence.EntityType)) // residence.Residents.Value > residence.EntityType.ContainerType.ResidenceType.LivingCapacity)
                {
                    hasRoom = false; // time to move out
                }
            }
            else if (!Residence.HasCapacity(entity.PersonEntity.Household, residence.Residents.Value, residence.EntityType)) // (residence.Residents.Value + entity.PersonEntity.Household.NoOfMembers > residence.EntityType.ContainerType.ResidenceType.LivingCapacity)
            {
                hasRoom = false;
            }
            //if (residence.Residents.Value >= residence.EntityType.ContainerType.ResidenceType.LivingCapacity)

            if (!hasRoom)
            {
                // no room...
                staticScore = 0;
                score = 0;
                return CalculateResult.Done;
            }

            
            double travelTimeFromEntityScore = 0;
            RegionMap.Result entityResult = ScoreTravelTime(regionMap, threatStanceToUse, entity.AccessPoint.Value, // #ACCESS .PlaySiteLocation, 
                                                            residence.AccessPoint.Value, entity, ref travelTimeFromEntityScore);
            if (entityResult == RegionMap.Result.Wait)
            {
                // wait for the result...              
                return CalculateResult.Processing;
            }
            else if (entityResult == RegionMap.Result.NoAccess)
            {                   
                score = 0;
                return CalculateResult.Done;
            }

            // see if the residence is within reach of the expedition that we belong to:
            double travelTimeFromExpeditionScore = 0;
            RegionMap.Result expeditionResult = ScoreTravelTime(regionMap, threatStanceToUse, residence.AccessPoint.Value, closestUnblockedExpeditionLocation, entity, ref travelTimeFromExpeditionScore);
            if (expeditionResult == RegionMap.Result.Wait)
            {
                // wait for the result...             
                return CalculateResult.Processing;
            }
            else if (expeditionResult == RegionMap.Result.NoAccess)
            {
                score = 0;
                return CalculateResult.Done;
            }

            // capacity scoring leads to dithering/agents moving in circles between homes, possibly only when there are 3 or more homes.
            double capacityScore = 1d; // 1.0 - (residence.Residents.Value / residence.EntityType.ContainerType.ResidenceType.LivingCapacity);
                       
            // depends on distance, location, personal preference, family members   
            double privateResidenceScore = 0;
            if (belongsToHousehold)
            {
                privateResidenceScore = 1;
            }

            double conditionScore = EvaluateHaulingJobs.ScoreIsEntityFunctional(residence);
           // double comfortScore = residence.EntityType.ContainerType.ResidenceType.ComfortLevel;

            if (Common.IsZero(conditionScore))
            {
                score = 0;
            }
            else
            {
                double comfortScore = residence.ComfortLevel.Value; // Residence.GetComfortRating(residence);

                staticScore = 0.4 * travelTimeFromExpeditionScore + 0.1 * travelTimeFromEntityScore + 0.1 * capacityScore + 0.1 * privateResidenceScore + 0.3 * comfortScore; // does not depend on agent location
                score = staticScore + 0.1 * travelTimeFromEntityScore;
            }

            return CalculateResult.Done;
           
        }

       

        private Point GetRandomTilePosNearExpeditionForNewHome()
        {
            // TODO: make a proper analysis...

            Point centerPoint = MapManager.WorldPosToTile(entityIntelligence.CurrentExpedition.Center.Value);

            Point pos = new Point(centerPoint.X + The.Sim.GameplayRandomGenerator.RandomSign() * The.Sim.GameplayRandomGenerator.Next(8, 15, "EvaluateFindHome"),
                                  centerPoint.Y + The.Sim.GameplayRandomGenerator.RandomSign() * The.Sim.GameplayRandomGenerator.Next(8, 15, "EvaluateFindHome"));

            return pos;

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

            Intelligence entityIntelligence = entity.Intelligence;

            List<EntityGroup> householdAndPrivateOwnersOfVehicles = new List<EntityGroup>();
            householdAndPrivateOwnersOfVehicles.Add(entity.PersonEntity.OwnedEntities);
            householdAndPrivateOwnersOfVehicles.Add(entity.PersonEntity.Household.OwnedEntities);

         
            entityIntelligence.SetTopLevelGoal(new GoalMoveInToNewHome(entity, mostDesirableHome.EntityID, GetOwnerIDs(householdAndPrivateOwnersOfVehicles)), bestScore);
         
            return true;
        }
    }
}
