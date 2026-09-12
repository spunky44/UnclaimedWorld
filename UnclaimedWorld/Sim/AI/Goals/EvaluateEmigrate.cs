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
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// only reacts to the decision already made by EmigrateDecider...
    /// 
    /// Priority needs to be higher than all jobs.
    /// 
    /// </summary>
    class EvaluateEmigrate: GoalEvaluator
    {
        EntityID? mostDesirableTerminal;

        RouteID? routeToUse;
       
        //AllegianceID? emigrateTarget;

        private float priority;
        public override float Priority
        {
            get
            {
                return priority;
            }
        }


        public EvaluateEmigrate(Entity entity)
            : base(entity)
        {
            this.priority = GameData.Instance.AIConstants.PriorityOfEmigrating; //1f;

        }


        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            mostDesirableTerminal = null;
            routeToUse = null;
            result = 0;

            if (!entityIntelligence.Brain.IsSame(typeof(GoalEmigrate))
                && entityIntelligence.Memory.EmigrateTarget.HasValue)
            {
                Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(entityIntelligence.Memory.EmigrateTarget.Value);
                if (allegiance != null)
                {
                    // if we cannot find a terminal, abort the emigration attempt...

                    // prefer daytime..?

                    //double mostDesirableScore = minimumRatingToConsider;

                    // find the closest reachable land terminal:
                    RouteID? route = GetRoute(RouteType.Land, allegiance.Site);

                    if (route.HasValue)
                    {
                        CalculateResult calcResult = FindClosestTerminal(TerminalType.TypesOfTerminal.Land);

                        if (calcResult == CalculateResult.Processing)
                        {
                            return CalculateResult.Processing;
                        }

                        // only if we found a terminal to go to do we get a score other than 0:
                        if (mostDesirableTerminal.HasValue) //bestScore > minimumRatingToConsider)
                        {
                            routeToUse = route;
                            //bestScore *= Priority; // 0.8 * mostDesirableScore + 0.2 * timeOfDayContribution;

                            // set a constant result. this makes it easier to balance against jobs etc.
                            result = Priority;
                            bestScore = result;
                      
                            return CalculateResult.Done;
                        }                  
                    }
                }

                entityIntelligence.Memory.SetEmigrateDecision(null, entity); // abort.
                
            }
         
            return CalculateResult.Done;
        }

        private CalculateResult ScoreTerminal(Entity entity, RegionMap regionMap, ThreatStance threatStanceToUse, Vector3 closestUnblockedExpeditionLocation, 
            IKnownEntityData terminal, ref double score)
        {
                      

            double travelTimeFromEntityScore = 0;
            RegionMap.Result entityResult = ScoreTravelTime(regionMap, threatStanceToUse, entity.AccessPoint.Value, // #ACCESS .PlaySiteLocation, 
                terminal.AccessPoint.Value, entity, ref travelTimeFromEntityScore);

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
         /*   double travelTimeFromExpeditionScore = 0;
            RegionMap.Result expeditionResult = ScoreTravelTime(regionMap, threatStanceToUse, terminal.AccessPoint.Value, closestUnblockedExpeditionLocation, entity, ref travelTimeFromExpeditionScore);
            if (expeditionResult == RegionMap.Result.Wait)
            {
                // wait for the result...             
                return CalculateResult.Processing;
            }
            else if (expeditionResult == RegionMap.Result.NoAccess)
            {
                score = 0;
                return CalculateResult.Done;
            }*/
                       

            double conditionScore = EvaluateHaulingJobs.ScoreIsEntityFunctional(terminal);
          
            if (Common.IsZero(conditionScore))
            {
                score = 0;
            }
            else
            {               
              //  score = 0.5 * travelTimeFromExpeditionScore + 0.5 * travelTimeFromEntityScore;
                score = travelTimeFromEntityScore;
            }

            return CalculateResult.Done;

        }

        private CalculateResult FindClosestTerminal(TerminalType.TypesOfTerminal terminalType) //ref double mostDesirableScore)
        {
            ThreatStance threatStanceToUse;
            RegionMap regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);

            // often/sometimes the expedition point is blocked by buildings:
            Vector3 closestUnblockedExpeditionLocation = EntityGroup.GetFreeGroundLocation(entityIntelligence.CurrentExpedition);


            double currentScore = 0;
            bestScore = 0;

            SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
            foreach (var item in entityIntelligence.Allegiance.SharedKnowledge.AllKnownEntities.Terminals)
            {
                if (item.Key == terminalType) //TerminalType.TypesOfTerminal.Land)
                {
                    for (int i = item.Value.Count - 1; i >= 0; i--)
                    {
                        EntityID entityID = item.Value[i];
                        IKnownEntityData terminalData;
                        if (EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID, out terminalData)))
                        {
                            continue;
                        }

                        if (IsOnPlaySite(terminalData)
                            && terminalData.IsCompleted())
                        {
                            CalculateResult result = ScoreTerminal(entity, regionMapToUse, threatStanceToUse, closestUnblockedExpeditionLocation, terminalData, ref currentScore);

                            if (result == CalculateResult.Processing)
                            {
                                return result;
                            }

                            if (currentScore > bestScore)
                            {
                                bestScore = currentScore;
                                // TODO: depends on distance, location, personal preference, family members
                                mostDesirableTerminal = terminalData.EntityID;     
                            }
                        }                       
                    }

                }
            }

            return CalculateResult.Done;

        }


        private RouteID? GetRoute(RouteType routeType, Site toSite)
        {
            var routes = The.Sim.World.GetRoutesAndDistances(entity.Site, toSite);

            if (routes != null)
            {
                foreach (var route in routes)
                {
                    double distance = route.Item2;

                    if (route.Item1 != null && route.Item1.RouteType == routeType)
                    {
                        return route.Item1.ID;
                    }

                    /*
                    if (route.Item1 == null)
                    {
                        if (vehicle.CanUseRoute(null, true, distance))
                        {
                            TravelAction.SetRoute(null, true);
                            break;
                        }
                    }
                    else if (vehicle.CanUseRoute(route.Item1.RouteType, false, distance))
                    {
                        TravelAction.SetRoute(route.Item1, false);
                        break;
                    }*/
                }
            }

            return null;

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

            Allegiance toAllegiance = LookUp<Allegiance, AllegianceID>.FindByID(entityIntelligence.Memory.EmigrateTarget.Value);

            if (toAllegiance != null)
            {
                entityIntelligence.SetTopLevelGoal(new GoalEmigrate(entity, toAllegiance.Site.ID, toAllegiance.ID, toAllegiance.Expeditions[0].ID,
                    mostDesirableTerminal.Value, routeToUse, GetOwnerIDs(householdAndPrivateOwnersOfVehicles)), bestScore);


                return true;
            }
            else return false;
        }
    }
}
