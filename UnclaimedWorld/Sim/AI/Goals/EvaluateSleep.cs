using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Needs;
using System.Diagnostics;
using UWGame.SimSide.Jobs;
using System.Linq;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Resources;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// see if it is time to sleep, and find a place to sleep.
    /// the agent have a preferred time of day to sleep, but if the need is great enough, he will sleep at other times also. 
    /// </summary>F
    public class EvaluateSleep : GoalEvaluator
    {
        private EntityID? bestContainerToSleepIn;
        private WorldLocation? bestGroundLocationToSleepOn;
        private ExpeditionID? bestExpeditionToSleepAt;
        private GroundSleepArea? bestGroundSleepArea;

        private List<EntityGroup> ownersOfVehicles;

        /// <summary>
        /// places to sleep...
        /// </summary>
        private List<EntityGroup> otherOwnersOfStructures;


        private enum Progress { NotStarted, ScoreNeed, GetLocations, ScoreContainers, ScoreGroundLocations, FindFinalLocation } //, ReplenishTools }
        private Progress progress = Progress.NotStarted;


        private List<SleepLocation> containerSleepLocations = new List<SleepLocation>();
        private Dictionary<TilePos, SleepLocation> groundSleepLocations = new Dictionary<TilePos, SleepLocation>();
        private List<SleepLocation> listOfGroundSleepLocations; // for iterating

        double sleepNeedsScore;

        // progress variables - keep track of how far we have come between calls to CalculateResult
        private int scoreContainerIndex = 0;
        private int scoreGroundLocationIndex = 0;

        private int finalComboSelectionIndex = 0;

        private bool scoringWasInterrupted = false; // this keeps track of us getting interrupted and then having to resume in a later frame.

        //***********************

        float MaxSleepIncreasePerDay = GameData.Instance.Constants.SleepNeed.GainPerDayWhenSleeping * GameData.Instance.Constants.SleepNeed.MaximumSleepNeedGainFactorForPeople;

        private float priority;
        public override float Priority
        {
            get
            {
                return priority;
            }
        }

        /// <summary>
        /// will score the entity's home as well as structures supplied in the list of owners
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ownersOfVehicles"></param>
        /// <param name="otherOwnersOfStructures"></param>
        public EvaluateSleep(Entity entity, List<EntityGroup> ownersOfVehicles, List<EntityGroup> otherOwnersOfStructures)
            : base(entity)
        {
            this.priority = GameData.Instance.AIConstants.PriorityOfNeeds; //1f;

            this.ownersOfVehicles = ownersOfVehicles;
            this.otherOwnersOfStructures = otherOwnersOfStructures;

        }
       

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            result = 0;
            bestScore = 0;

            if (entityIntelligence.IsIndependent()
                && entityIntelligence.CurrentExpedition.Policy.SleepInShiftsIsActive()) // shiftSleepingBehaviour == true) //Check if we can manage to keep preferably half of the group awake
            {
                if (!IsPermittedToSleep())
                {
                    return CalculateResult.Done;
                }
            }

            if (progress == Progress.NotStarted)
            {               

                if (!NeedsSleep())
                {
                    return CalculateResult.Done;
                }
                else
                {
                    // start - do init:
                    sleepNeedsScore = 0;

                    scoreContainerIndex = 0;
                    scoreGroundLocationIndex = 0;
                    bestGroundLocationToSleepOn = null;
                    bestExpeditionToSleepAt = null;
                    bestContainerToSleepIn = null;
                    containerSleepLocations.Clear();
                    groundSleepLocations.Clear();

                    progress = Progress.ScoreNeed;
                }
            }

            if (progress == Progress.ScoreNeed)
            {
                ScoreNeed();

                progress = Progress.GetLocations;

            }

            if (progress == Progress.GetLocations)
            {
                GatherAllSleepLocations();

                progress = Progress.ScoreContainers;

            }

            if (progress == Progress.ScoreContainers)
            {
                if (ScoreContainerLocations() == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }

                progress = Progress.ScoreGroundLocations;
            }

            if (progress == Progress.ScoreGroundLocations)
            {
                if (ScoreGroundLocations() == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }

                progress = Progress.FindFinalLocation;

            }

            if (progress == Progress.FindFinalLocation)
            {
                SelectBestLocation();

                result = bestScore;
              //  result = ApplyPriority(result);


                return CalculateResult.Done;
            }


            return CalculateResult.Done;
        }

        /// <summary>
        /// was: EvaluateShiftSleeping
        /// 
        /// returns true if we are not allowed to sleep.
        /// </summary>
        /// <returns></returns>
        private bool IsPermittedToSleep()
        {
            Expedition e = entityIntelligence.CurrentExpedition;
            int noOfSleepingIndependents = e.GetNumberOfSleepingIndependents(); // EvaluateSleep.GetNumberOfSleepingAgents(); d
            float numberOfIndependentAgents = e.IndependentMembers.Count; 


            if (noOfSleepingIndependents >= e.NoOfIndependentMembersAllowedToSleep()) // GameData.Instance.Constants.NoOfMembersAllowedToSleep)
            {
                Need sleepNeed = entity.BiologicalEntity.Needs.NeedsList["sleep"];

                //If we are not really tired and there already are agents sleeping, we should not
                if (sleepNeed.CurrentLevel < GameData.Instance.AIConstants.SleepNeedLimitToSwapWithASleeper)
                {
                    foreach (var item in entityIntelligence.CurrentExpedition.IndependentMembers)
                    {
                        Entity otherMember = Entity.FindByID(item);
                        if (otherMember != null)
                        {
                            if (otherMember.Intelligence.IsSleeping() == true)
                            {
                                if (otherMember.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel > GameData.Instance.AIConstants.SleepNeedLimitToLetSomeoneElseSleepInstead)
                                {
                                    return true;// false;
                                }
                            }
                        }
                    }

                    /* why was this repeated..?
                    for (int i = 0; i < numberOfIndependentAgents; i++)
                    {
                        Entity person = entityIntelligence.Allegiance.MembersList[i]; // Persons[i]; // The.Sim.PlaySite.Persons[i]; 
                        if (person.EntityType.BiologicalType != null 
                            && person.Intelligence.IsIndependent() 
                            && person.Intelligence.IsSleeping() == true)
                        {
                            if (person.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel > GameData.Instance.AIConstants.SleepNeedLimitToLetSomeoneElseSleepInstead)
                            {
                                return true; // false;
                            }
                        }
                    }*/
                }

                if (sleepNeed.CurrentLevel > GameData.Instance.AIConstants.SleepNeedToIgnoreSleepPolicy)
                {
                    return false; // true;
                }
            }

            return true; // false; 
        }

        private void ScoreNeed()
        {
            sleepNeedsScore = ScoreSleepNeed();

        }

        private void SelectBestLocation()
        {
            // merge the 2 collections:
            containerSleepLocations.AddRange(groundSleepLocations.Values);

            containerSleepLocations.RemoveAll(l => l.Score == 0d);

            if (containerSleepLocations.Count > 0)
            {
                // sort results:
                containerSleepLocations.Sort((a, b) => b.Score.CompareTo(a.Score));

                // pick the best:
                SleepLocation bestLocation = containerSleepLocations[0];

                bestScore = bestLocation.Score;


                bestContainerToSleepIn = bestLocation.ContainerEntity;
                bestExpeditionToSleepAt = bestLocation.ExpeditionID;
                bestGroundLocationToSleepOn = bestLocation.GroundLocation;
                bestGroundSleepArea = bestLocation.GroundSleepArea;
            }


            progress = Progress.NotStarted; // all done. Start from the top next time!

        }

        private bool NeedsSleep()
        {
            double needsScore = ScoreSleepNeed();


            if (needsScore < 1d)
            {
                double timeOfDayContribution = ScoreTimeOfDay();

                if (timeOfDayContribution < 0.95) // late evening?
                {
                    return false; // don't go to sleep during day unless sleep starved
                }

            }

            return true; // go on with scoring sleep locations
        }

        private CalculateResult ScoreContainerLocations()
        {
            SleepLocation sleepLocation;

            ThreatStance threatStanceToUse;
            RegionMap regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);

            double score = 0;

            for (; scoreContainerIndex < containerSleepLocations.Count; scoreContainerIndex++)
            {
                sleepLocation = containerSleepLocations[scoreContainerIndex];

                if (ScoreSleep(sleepNeedsScore, regionMapToUse, threatStanceToUse, sleepLocation.ExpeditionID,
                    sleepLocation.ContainerEntity, sleepLocation.GroundLocation, sleepLocation.GroundSleepArea,
                    sleepLocation.TravelTimeToCenterScore, ref score) == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }
                else
                {
                    sleepLocation.Score = score;
                }
            }

            return CalculateResult.Done;

        }

        private CalculateResult ScoreGroundLocations()
        {
            SleepLocation sleepLocation;

            ThreatStance threatStanceToUse;
            RegionMap regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);

            double score = 0;

            for (; scoreGroundLocationIndex < listOfGroundSleepLocations.Count; scoreGroundLocationIndex++)
            {
                sleepLocation = listOfGroundSleepLocations[scoreGroundLocationIndex];

            
                if (ScoreSleep(sleepNeedsScore,regionMapToUse, threatStanceToUse, sleepLocation.ExpeditionID, sleepLocation.ContainerEntity, sleepLocation.GroundLocation, sleepLocation.GroundSleepArea,
                    sleepLocation.TravelTimeToCenterScore, ref score) == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }
                else
                {
                    sleepLocation.Score = score;
                }
            }

            return CalculateResult.Done;

        }


        private CalculateResult ScoreLocation(RegionMap regionMapToUse, ThreatStance threatStanceToUse, ExpeditionID? expeditionToSleepAt, EntityID? containerEntity, WorldLocation? groundLocation, GroundSleepArea? groundSleepArea, double travelTimeToCenterScore, ref double score)
        {
            // score the varaibles that have to do with sleeping in this location - distance, comfort, danger and so on
            // combine this partial result with the agents' sleep need and time of day
            WorldLocation location;
            IKnownEntityData containerData = null;
            if (expeditionToSleepAt.HasValue)
            {
                location = new WorldLocation(Expedition.FindByID(expeditionToSleepAt.Value).Center.Value);
                //location = new WorldLocation(expeditionToSleepAt.Value.Center);
            }
            else if (containerEntity.HasValue)
            {
                if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(containerEntity.Value, out containerData)))
                {
                    score = 0;
                    return CalculateResult.Done;
                }

                location = new WorldLocation(containerData.AccessPoint.Value);

            }
            else
            {
                location = groundLocation.Value;
            }


            double travelTimeScore = 0;
            RegionMap.Result result = ScoreTravelTime(regionMapToUse, threatStanceToUse, entity.AccessPoint.Value, // #ACCESS .Location.Value, 
                location.ToVector3(), entity, ref travelTimeScore);

            if (result == RegionMap.Result.Wait)
            {
                // wait for the result...     
                return CalculateResult.Processing;
            }
            else if (result == RegionMap.Result.NoAccess)
            {
                score = 0f;
                return CalculateResult.Done;
            }


            double crowdedScore = ScoreCrowdedTile(containerData,expeditionToSleepAt, groundLocation); // sleepLocation);           

            double comfortScore = ScoreComfort(containerData);

            double safetyScore = ScoreSafety(containerData, groundSleepArea);


            GoalEvaluator.WeightedRating weightedRating = new GoalEvaluator.WeightedRating();

            weightedRating.AddScore(0.20, travelTimeScore);
            weightedRating.AddScore(0.05, travelTimeToCenterScore);
            weightedRating.AddScore(0.35, comfortScore);
            weightedRating.AddScore(0.1, crowdedScore);
            weightedRating.AddScore(0.3, safetyScore);

          /*  weightedRating.AddScore(0.25, travelTimeScore);
            weightedRating.AddScore(0.05, travelTimeToCenterScore);
            weightedRating.AddScore(0.45, comfortScore);
            weightedRating.AddScore(0.1, crowdedScore);
            weightedRating.AddScore(0.15, safetyScore);
            */

            /*sleepLocation.Score*/
            score = weightedRating.Result;

            return CalculateResult.Done;

        }

     /*   private bool IsCampfire(IKnownEntityData containerData)
        {
            return containerData.EntityType.GatheringSiteType != null && containerData.EntityType.RequiresEnergyType != null;
        }*/

        private double ScoreSafety(IKnownEntityData containerData, GroundSleepArea? area)
        {
            // TODO: replace this score with a true 'safety' score??
            double safetyScore = 1d; // buildings and campfire score highest

            if (area.HasValue)
            {   // score sleeping in the open
                switch (area.Value)
                {
                    case GroundSleepArea.Self:
                        safetyScore = 0;
                        break;
                    case GroundSleepArea.Expedition:
                        safetyScore = 0.5; // when sleeping on the ground, choose the expedition center over the entity's present location
                        break;
                    case GroundSleepArea.Home:
                        safetyScore = 0.75;
                        break;
                }
            }

            return safetyScore;
        }


        private double ScoreComfort(IKnownEntityData containerData)
        {
            double comfortScore;
            float maxGain, increaseAmountPerDay;
            GoalDoSleep.ComputeSleepConditions(entity, containerData, out maxGain, out increaseAmountPerDay);

            double maxGainScore;
            if (maxGain >= 1d)
            {
                maxGainScore = 1d;
            }
            else
            {
                maxGainScore = 0.5d; // for sleeping in open
            }


            comfortScore = 0.5 * maxGainScore + 0.5 * increaseAmountPerDay / MaxSleepIncreasePerDay;


            comfortScore = Common.Clamp(comfortScore, 0d, 1d);

            return comfortScore;
        }

        private double ScoreCrowdedTile(IKnownEntityData ContainerEntity, ExpeditionID? expeditionID, WorldLocation? GroundLocation) // SleepLocation sleepLocation)
        {
            double crowdedScore = 1;

            if (ContainerEntity != null || expeditionID.HasValue)
            {
                return crowdedScore;
            }
            TerrainTile tile;
            tile = The.Map.GetTile(MapManager.WorldPosToTilePos(GroundLocation.Value));
            
            if (tile.EntitiesOnTile != null)
            {
                //float totalBulk = tile.GetTotalBulkOfItems();
                //GameData.Instance.Constants.
                // just count the entities (persons/items in the tile...)
                int noOfEntities = 0;
                if (tile.AllegiancesThatSeeThisTile.Contains(entityIntelligence.Allegiance))
                {
                    // if we can see the tile...
                    noOfEntities = tile.EntitiesOnTile.Sum(e => e != entity ? 1 : 0); // don't count ourselves
                }
                else
                {
                    List<MemoryFact> memoryFacts;
                    if (tile.RememberedRootEntitiesOnTile != null && tile.RememberedRootEntitiesOnTile.TryGetValue(entityIntelligence.Allegiance.SharedKnowledge, out memoryFacts))
                    {
                        noOfEntities = memoryFacts.Count;
                    }
                }

                crowdedScore = (1d - (double)noOfEntities * 0.1d);
            }
            return crowdedScore;
        }


        public enum GroundSleepArea { Self, Home, Expedition }

        /// <summary>
        /// this gets called from GoalSleep as well, to get the most up to date score. NEW: includes Priority
        /// </summary>
        /// <param name="sleepNeedsScore"></param>
        /// <param name="regionMapToUse"></param>
        /// <param name="containerToSleepIn"></param>
        /// <param name="expeditionToSleepAt"></param>
        /// <param name="groundLocation"></param>
        /// <param name="travelTimeToCenterScore"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public CalculateResult ScoreSleep(double? sleepNeedsScore, RegionMap regionMapToUse, ThreatStance? threatStance, ExpeditionID? expeditionToSleepAt, EntityID? containerToSleepIn, 
            WorldLocation? groundLocation, GroundSleepArea? groundSleepArea, double? travelTimeToCenterScore, ref double score)
        {
            
            bestGroundLocationToSleepOn = null;
            bestContainerToSleepIn = null;
            bestExpeditionToSleepAt = null;
            // when not really tired, only sleep when the time is right:
            if (!sleepNeedsScore.HasValue)
            {
                sleepNeedsScore = ScoreSleepNeed(); // cache this value
            }

            if (!travelTimeToCenterScore.HasValue)
            {
                travelTimeToCenterScore = 1; //?
            }

            ThreatStance threatStanceToUse;
            if (regionMapToUse == null || threatStance == null)
            {
                ThreatStance newThreatStance;
                regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out newThreatStance);

                threatStanceToUse = newThreatStance;
            }
            else
            {
                threatStanceToUse = threatStance.Value;
            }


            /*  if (entity.PersonEntity != null) // same as critter sleep?
              {*/
            double timeOfDayScore = ScoreTimeOfDay();
            double locationScore = 0;

            if (sleepNeedsScore < 1d) // not too sleepy yet...
            {
                if (timeOfDayScore < entity.EntityType.BiologicalType.TimeOfDayToGoToSleep) // 0.95) // late evening?
                {
                    score = 0d; // don't go to sleep during day unless really tired

                    return CalculateResult.Done;
                }

            }


            if (ScoreLocation(regionMapToUse, threatStanceToUse, expeditionToSleepAt, containerToSleepIn, groundLocation, groundSleepArea,
                        travelTimeToCenterScore.Value, ref locationScore) == CalculateResult.Processing)
            {
                return CalculateResult.Processing;
            }

            if (Common.IsZero(locationScore))
            {   
                // NEW - don't sleep at expedition if it can't be reached
                score = 0d;
                return CalculateResult.Done;
            }

           
            GoalEvaluator.WeightedRating weightedRating = new GoalEvaluator.WeightedRating();

            weightedRating.AddScore(0.4f, locationScore);
            weightedRating.AddScore(0.1f, timeOfDayScore);
            weightedRating.AddScore(0.5f, sleepNeedsScore.Value);

            score = weightedRating.Result;

            score = ApplyPriority(score); 

            return CalculateResult.Done;

        }

        /*
        public CalculateResult ScoreSleep(ref double score)
        {
            bestGroundLocationToSleepOn = null;
            bestContainerToSleepIn = null;

            if (entity.PersonEntity != null)
            {
                if (entity.PersonEntity.Household.Home != null)
                {
                    bestContainerToSleepIn = entity.PersonEntity.Household.Home.EntityID;

                    // double distanceScore;

                    double travelTimeScore = 0;
                    RegionMap regionMapToUse = GoalEvaluator.GetRegionMapToUseForEntity(entity);

                    RegionMap.Result result = ScoreTravelTime(regionMapToUse, entity.Location, entity.PersonEntity.Household.Home.AccessPoint, entity, ref travelTimeScore);
                    if (result == RegionMap.Result.Wait)
                    {
                        // wait for the result...                    
                        score = 0d;
                        return CalculateResult.Processing;
                    }
                    else if (result == RegionMap.Result.NoAccess)
                    {

                        // we cannot go home.  
                        // TODO: find another place to sleep! and use the same methods for critters without a lair/nest...
                        score = 0d; // <- delete this
                        return CalculateResult.Done;
                    }

                    //also use travelTimeScore to determine if we should go home or sleep somewhere else...



                    // when not really tired, only sleep when the time is right:
                    double needsScore = ScoreSleepNeed();
                    // TimePhaseWithFalloff(UWGame.SimSide.SleepPhaseStarts, UWGame.SimSide.WorkPhaseStarts, TimePhaseFalloff, DateAndTime.Instance.TimeOfDay);

                    if (needsScore < 1d)
                    {
                        double timeOfDayContribution = ScoreTimeOfDay();

                        if (timeOfDayContribution < 0.95) // late evening?
                        {
                            score = 0d; // don't go to sleep during day unless really tired
                        }
                        else
                        {

                            score = 0.9 * timeOfDayContribution + 0.1 * needsScore;
                        }

                        return CalculateResult.Done;
                    }
                    else // very sleepy
                    {
                        score = 1d; // sleep now if we can, regardless what time it is...
                        return CalculateResult.Done;
                    }

                }
                else
                {
                    // sleeping on the ground... only when we are really tired...
                    double needsScore = ScoreSleepNeed();

                    if (needsScore < 1d)
                    {
                        score = 0d;
                        return CalculateResult.Done;
                    }
                    else
                    {
                        score = 1d; // sleep now if we can, regardless what time it is...
                        return CalculateResult.Done;
                    }
                }

            }
            else if (entity.BiologicalEntity.DwellingSpot.HasValue) // critter sleep - TODO!!!
            {
                double timeOfDayContribution = ScoreTimeOfDay();

                double needsScore = ScoreSleepNeed();

                score = 0.9 * timeOfDayContribution + 0.1 * needsScore; //0.1 * ageContribution;
                return CalculateResult.Done;
            }
            else // TODO!!!
            {
                score = 0; // never sleep... or rather, find a dwelling spot first. (for critters: low threat, nearby..., for persons: near expedition center)
                return CalculateResult.Done;
            }

        }
        */

        private void GatherAllSleepLocations()
        {
            containerSleepLocations.Clear();

            TilePos? groundSleepCenterTilePos = null;

            // we can sleep near the expedition center (critters also):
            if (entityIntelligence.CurrentExpedition != null) 
            {

                TilePos tilePosition = MapManager.WorldPosToTilePos(entityIntelligence.CurrentExpedition.Center.Value);
                groundSleepLocations.Add(tilePosition, new SleepLocation()
                {
                    ExpeditionID = entityIntelligence.CurrentExpedition.ID,

                    GroundSleepArea = GroundSleepArea.Expedition,
                    TravelTimeToCenterScore = HaulingJobManager.ScoreTravelTimeToCenter(entity.PlaySiteLocation.ToPoint(), entityIntelligence.CurrentExpedition.Center.Value.ToPoint())
                });

               // GatherGroundSleepLocations(MapManager.WorldPosToTilePos(entity.PersonEntity.CurrentExpedition.Center), 3, GroundSleepArea.Expedition);
            }

            // or sleep in/near the home:
            if (entity.EntityType.Person != null
                && entity.PersonEntity.Household != null && entity.PersonEntity.Household.Home != null)
            {
                IKnownEntityData homeData;
                if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entity.PersonEntity.Household.Home.Value, out homeData)))
                {
                    containerSleepLocations.Add(new SleepLocation() { ContainerEntity = entity.PersonEntity.Household.Home, TravelTimeToCenterScore = 1 });

                    groundSleepCenterTilePos = MapManager.WorldPosToTilePos(homeData.PlaySiteLocation);

                    GatherGroundSleepLocations(groundSleepCenterTilePos.Value, 3, GroundSleepArea.Home);
                }
                else
                {
                    entity.PersonEntity.Household.Home = null;
                }

            }

            // or sleep near the campfire:
            if (otherOwnersOfStructures != null)
            {
                foreach (var owner in otherOwnersOfStructures)
                {
                    foreach (var list in owner.Structures)
                    {
                        if (list.Key.GatheringSiteType != null)
                        {
                            foreach (var structure in list.Value)
                            {
                                containerSleepLocations.Add(new SleepLocation() { ContainerEntity = structure, TravelTimeToCenterScore = 1 });
                            }
                        }

                        /*  CalculateResult result = GetSortedListOfEntities(entity, owner, sharedKnowledge, footRegionMap,
                                  list.Value, listOfGatheringPlaces,
                              e => e.GatheringSite != null,
                              240f);

                          if (result == CalculateResult.Processing)
                          {
                              // wait for the result...                    
                              return CalculateResult.Processing;
                          }*/
                    }
                }
            }

            // or sleep near where we are
            groundSleepCenterTilePos = MapManager.WorldPosToTilePos(entity.Location.Value);

            GatherGroundSleepLocations(groundSleepCenterTilePos.Value, 2, GroundSleepArea.Self);

            listOfGroundSleepLocations = groundSleepLocations.Values.ToList();
        }



        private void GatherGroundSleepLocations(TilePos center, int radius, GroundSleepArea sleepArea)
        {

            int minX, maxX, minY, maxY;

            Rectangle area = MapManager.GetClampedMapAreaUsingTiles(center, radius, out minX, out maxX, out minY, out maxY);

            SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot]; // moveMap.Map[TerrainType.TransportType.Foot];

            Allegiances.Allegiance allegiance = entityIntelligence.Allegiance;

            EntityGroup householdOwner = null, expeditionOwner = null;

            /*if (entity.EntityType.Person != null && entity.PersonEntity.CurrentExpedition != null) 
                expeditionOwner = entity.PersonEntity.CurrentExpedition.OwnedEntities;
            */
            if (entity.Intelligence.CurrentExpedition != null)
                expeditionOwner = entity.Intelligence.CurrentExpedition.OwnedEntities;


            if (entity.PersonEntity != null)
            {
                householdOwner = entity.PersonEntity.Household.OwnedEntities;
            }

            TerrainTile tile;
            TilePos point;
            SubtilePos centerSubtile;
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    point = new TilePos(x, y);

                    centerSubtile = MapManager.TileCenterToSubTile(point);

                    if (!The.Map.TileIsCompletelyBlocked(map, point.ToPoint())
                        && !The.Map.SubtileIsCompletelyBlocked(map, centerSubtile) // the center subtile must not be blocked...
                        && !The.Map.FlagIsSet(centerSubtile, SurfaceType.TransportType.Foot, MapManager.SubtileValue.Reserved)) // try to avoid areas being built on
                    {
                        tile = The.Map.GetTile(point);

                        if (tile.GeoLayoutEntitiesOnTile == null || tile.GeoLayoutEntitiesOnTile.Count == 0) // not on buildings...
                        {
                            if (tile.Owner == null || 
                                (expeditionOwner != null && tile.Owner == expeditionOwner.ID) || 
                                (householdOwner != null && tile.Owner == householdOwner.ID)) // only on owned terrain
                            {
                                if (entity.EntityType.Person != null)
                                {
                                    Zone stockpile = tile.GetStockpileZone(entityIntelligence.Allegiance); // not on our stockpiles
                                    if (stockpile != null)
                                    {
                                        continue;
                                    }
                                }

                                if (!groundSleepLocations.ContainsKey(tile.TilePos))
                                {
                                    groundSleepLocations.Add(tile.TilePos, new SleepLocation()
                                    {
                                        GroundLocation = MapManager.TilePosToWorldLocation(point),

                                        GroundSleepArea = sleepArea,
                                        TravelTimeToCenterScore = HaulingJobManager.ScoreTravelTimeToCenter(point.ToPoint(), center.ToPoint())
                                    });
                                }

                            }
                        }
                    }
                }
            }
        }

        private RegionMap.Result ScoreDistanceFromSleepLocation(Vector3 sleepLocation, out double distanceScore)
        {
            distanceScore = 0;
            ThreatStance threatStance;
            RegionMap regionMapToUse = GetRegionMapAndStanceForEvaluator(entity, null, out threatStance);

            Point fromSubtilePos = MapManager.WorldPosToSubtile(entity.AccessPoint.Value); // #ACCESS
            Point destinationSubtilePos = MapManager.WorldPosToSubtile(sleepLocation); //entity.PersonEntity.CurrentExpedition.Center);

            float distance = -1f;


            RegionMap.Result result1 = regionMapToUse.GetDistance(entity, fromSubtilePos, destinationSubtilePos, ref distance);

            if (result1 != RegionMap.Result.OK)
            {
                return result1;
            }
            else
            {
                //   ScoreTravelTime()
                // got the distance. Compute the score:
                // double timeCost;

                if (distance > 400f)
                {
                    distanceScore = Common.Clamp(distance * 0.0001f, 0f, 1f);
                }
                else distanceScore = 0f;

            }

            return result1;
        }

        public double ScoreSleepNeed()
       {
            //  double needsScore = Math.Pow(1d - entity.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel, 2d); // let's square it, so we can stay awake for longer.

            Need sleepNeed = entity.BiologicalEntity.Needs.NeedsList["sleep"];
            double needsScore = 0.9d * (1d - sleepNeed.CurrentLevel);

            // return 1 if the sleep need has been 0 for a time:
            needsScore += MathHelper.Lerp(0f, 0.1f, 10f * sleepNeed.PhysicalNeed.DaysAtZero / (sleepNeed.NeedType.PhysicalEffects.DaysAtZeroCausingDeath ?? 1f));


            return needsScore;
        }

       

        private void WakeSomeoneUpIfNeeded()
        {            

            Expedition e = entityIntelligence.CurrentExpedition;

            if (!e.Policy.SleepInShiftsIsActive())
            {
                return;
            }

            int numberOfAgentsSleeping = e.GetNumberOfSleepingIndependents(); // GetNumberOfSleepingAgents();// The.Sim.Site.Persons.Count;
            //Dictionary<Entity,Entity> members = entity.Intelligence.Allegiance.Members;

            if (numberOfAgentsSleeping >= e.NoOfIndependentMembersAllowedToSleep()) // NoOfMembersAllowedToSleep) // GameData.Instance.Constants.NoOfMembersAllowedToSleep)//Is 50% of more of us currently sleeping?
            {
                Need sleepNeed = entity.BiologicalEntity.Needs.NeedsList["sleep"];

                Entity entityToWakeUp = null;
                
                foreach (var id in e.IndependentMembers) // members)
                {
                    Entity otherMember = Entity.FindByID(id);
                    if (otherMember != null)
                    {
                        if (otherMember != entity && otherMember.Intelligence.IsSleeping() == true)
                        {
                            float otherMembersSleepLevel = otherMember.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel;
                            if (otherMembersSleepLevel > GameData.Instance.AIConstants.SleepNeedLimitToLetSomeoneElseSleepInstead)
                            {
                                if (entityToWakeUp != null)
                                {
                                    if (entityToWakeUp.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel < otherMembersSleepLevel)
                                    {
                                        entityToWakeUp = otherMember;
                                    }
                                }
                                else
                                {
                                    entityToWakeUp = otherMember;
                                }

                            }
                        }
                    }
                }

                if (entityToWakeUp != null)
                {
                    Message wakeUp = new Message(Message.MessageTypes.StopGoalSleep);
                    entityToWakeUp.SendMessage(wakeUp);                    
                }

            }
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

            Intelligence entityIntelligence = entity.Intelligence;
            WakeSomeoneUpIfNeeded();
           

           entityIntelligence.SetTopLevelGoal(
                new GoalSleep(entity, bestGroundLocationToSleepOn,
                                      bestExpeditionToSleepAt,
                                      bestContainerToSleepIn,
                                      bestGroundSleepArea,
                                      GetOwnerIDs(ownersOfVehicles)) { GoalEvaluator = this },bestScore
                );

           return true;
        }


        [DebuggerDisplay("{ContainerEntity}, {GroundLocation}")]
        private class SleepLocation
        {
            // Add expedition (ID)? parameter

            public ExpeditionID? ExpeditionID;

            public EntityID? ContainerEntity;

            public WorldLocation? GroundLocation;

            public GroundSleepArea? GroundSleepArea;

            public double Score = 0;

            /// <summary>
            /// cached distance (by air) of location to the home/expedition/person
            /// gives higher weight to the center...
            /// </summary>
            public double TravelTimeToCenterScore;
        }
    }
}
