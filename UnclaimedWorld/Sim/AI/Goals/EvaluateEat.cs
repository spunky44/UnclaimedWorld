using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Needs;
using System.Linq;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Processes;
using UWGame.SimSide.GatheringSites;
using System.Diagnostics;

namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// look for items to eat (owned by us or owned by enemy allegiances/not owned)
    /// for each nutrient:
    /// for people: if starving -> eat anything, but prefer cooked meals 
    /// if not starving, but hasn't eaten for a while:
    ///     if it is dinnertime -> eat cooked meals only
    ///     if it is after dinnertime -> eat anything
    /// if almost full
    ///     if it is dinnertime -> eat cooked meals only
    /// </summary>
    public class EvaluateEat : GoalEvaluator
    {
        private const float fullLevel = 0.9f;
        private const float almostFullLevel = 0.7f;
     //   private const float hasNotEatenLevel = 0.5f;

        private enum FoodLevels { Full, AlmostFull, HasNotEaten, Starving };

        /// <summary>
        /// only for people, for creatures AllKnownFoodItems is used
        /// </summary>
        private EntityGroup foodItemsGroup;
     
        BiologicalEntity bioEntity;

        // don't save allegiance references for long... perhaps, at some point, it will become possible for entites to switch allegiances.
       // SharedKnowledge sharedKnowledge;

        private List<Entity> needsToBeCancelled = new List<Entity>();
        private List<Entity> itemsToBeDropped = new List<Entity>();

        private List<EntityGroup> ownersOfVehicles;

        /// <summary>
        /// places to eat...
        /// </summary>
        private List<EntityGroup> ownersOfGatheringPlaces;

        private float currentStomachRoom;

        private enum Progress { NotStarted, GetFoodItems, ScoreFood }
        private Progress progress = Progress.NotStarted;

        private int currentIndex = 0;
        private bool scoringWasInterrupted = false; // this keeps track of us getting interrupted and then having to resume in a later frame.

        private List<Tuple<IKnownEntityData, ProcessType, double>> allScores = new List<Tuple<IKnownEntityData, ProcessType, double>>();

        private EntityID? bestFoodItem;
        private EntityID? gatheringPlaceToEat;
        private ProcessType bestExtractionProcess;
        

        private float priority;
        public override float Priority
        {
            get
            {
                return priority;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="groupOfFoodItems"></param>
        /// <param name="ownersOfVehicles"></param>
        /// <param name="ownersOfGatheringPlaces"></param>
        public EvaluateEat(Entity entity, /*EntityGroup groupOfFoodItems,*/ List<EntityGroup> ownersOfVehicles, List<EntityGroup> ownersOfGatheringPlaces)
            : base(entity)
        {
            bioEntity = entity.BiologicalEntity;

            this.ownersOfVehicles = ownersOfVehicles;
            this.ownersOfGatheringPlaces = ownersOfGatheringPlaces;

            //for food items, people should supply an ownership list (from expedition, household etc.). Animals should supply their SharedKnowledge list.
            this.foodItemsGroup = GetFoodEntityGroup(entity); // groupOfFoodItems;           

            this.priority = GameData.Instance.AIConstants.PriorityOfNeeds; //1f;

        }

        public static EntityGroup GetFoodEntityGroup(Entity entity)
        {
            if (entity.PersonEntity != null)
            {
                return entity.Intelligence.CurrentExpedition.OwnedEntities;
            }
            else
            {
                return entity.Intelligence.Allegiance.SharedKnowledge.AllKnownEntities;

            }
        }

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            if (progress == Progress.NotStarted)
            {
                if (entityIntelligence.Brain.IsSame(typeof(GoalEat)))
                {
                    // quick bailout if we're already eating something
                    result = 0;
                    return CalculateResult.Done;
                }


                if (GetLowestFoodLevel() == FoodLevels.Full
                    || entity.BiologicalEntity.StomachContents > 0.94f)
                {
                    // quick bailout if we're full
                    result = 0;
                    return CalculateResult.Done;
                }                               

                currentStomachRoom = entity.AgentStorage.GetFreeStomachCapacity(); // bioEntity.GetFreeStomachCapacity();

                //if (currentStomachRoom)

                allScores.Clear();
                cachedNutritionScores.Clear();

                needsToBeCancelled.Clear();
                itemsToBeDropped.Clear();

                currentIndex = 0;

                scoringWasInterrupted = false;


                progress = Progress.GetFoodItems;
            }

            if (progress == Progress.GetFoodItems)
            {
                if (entity.PersonEntity != null)
                {

                }
              
                GetAllFoodItems();

                progress = Progress.ScoreFood;

            }

            if (progress == Progress.ScoreFood)
            {

                if (ScoreEating(minimumRatingToConsider, ref result) == CalculateResult.Done)
                {
                    progress = Progress.NotStarted;

                    bestScore = result;

                    return CalculateResult.Done;
                }
                else
                {
                    return CalculateResult.Processing;

                }

            }


            return CalculateResult.Done;
        }

        List<ItemDistance> listOfGatheringPlaces = new List<ItemDistance>();
        private CalculateResult FindGatheringPlaceToEatMeal()
        {
            SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;

            RegionMap footRegionMap = sharedKnowledge.GetMovementMap(entity).Layers[SurfaceType.TransportType.Foot].RegionMap; // UWGame.SimSide.Instance.Map.RegionMapManager.HumanFootExposedNormalRegionMap;

            listOfGatheringPlaces.Clear();

            foreach (var owner in ownersOfGatheringPlaces)
            {
                foreach (var list in owner.Structures)
                {
                    CalculateResult result = GetSortedListOfEntities(entity, owner, sharedKnowledge, footRegionMap,
                            list.Value, ref listOfGatheringPlaces,
                        e => e.GatheringSite != null,
                        240f);

                    if (result == CalculateResult.Processing)
                    {
                        // wait for the result...                    
                        return CalculateResult.Processing;
                    }      
                }
                          
            }

            // sort the list:
            if (listOfGatheringPlaces.Count > 0)
            {
                listOfGatheringPlaces = listOfGatheringPlaces.OrderBy(e => e.Distance).ToList();

                gatheringPlaceToEat = listOfGatheringPlaces[0].Entity.EntityID;
            }
            else
            {
                gatheringPlaceToEat = null;
            }

            return CalculateResult.Done;

        }

        private CalculateResult ScoreGatheringSite(RegionMap regionMapToUse, ThreatStance threatStanceToUse, IKnownEntityData food, ref double score)
        {
            score = 0;
            double travelTimeScore = 0;

            RegionMap.Result result = ScoreTravelTime(regionMapToUse, threatStanceToUse, entity.AccessPoint.Value, // #ACCESS .PlaySiteLocation, 
                food.PlaySiteLocation, entity, ref travelTimeScore);

            if (result == RegionMap.Result.Wait)
            {
                // wait for the result...                    
                return CalculateResult.Processing;
            }
            else if (result == RegionMap.Result.NoAccess)
            {
                return CalculateResult.Done;
            }

                       

            return CalculateResult.Done;
        }

     

        private FoodLevels GetLowestFoodLevel()
        {

            //bool isFull = true;

            FoodLevels lowestFoodLevel = FoodLevels.Full;

            float minLevel = 1f;

            foreach (var need in bioEntity.Needs.NeedsList)
            {
                if (need.Value.NeedType.FoodNeedType != null) //NeedClass == AINeedClass.Food)
                {
                    if (need.Value.CurrentLevel < minLevel)
                    {
                        minLevel = need.Value.CurrentLevel;
                    }
                    /*
                    if (need.Value.CurrentLevel > fullLevel)
                    {
                        lowestFoodLevel = 
                    }*/
                }
            }

            if (minLevel > fullLevel)
            {
                lowestFoodLevel = FoodLevels.Full;
            }
            else if (minLevel > almostFullLevel)
            {
                lowestFoodLevel = FoodLevels.AlmostFull;
            }
            else if (minLevel > 0f)
            {
                lowestFoodLevel = FoodLevels.HasNotEaten;
            }
            else if (Common.IsZero(minLevel))
            {
                lowestFoodLevel = FoodLevels.Starving;
            }

            return lowestFoodLevel;
        }

        /*  private bool IsStarving()
          {
              double needsScore = 1d - entity.BiologicalEntity.Needs.NeedsList["energy"].CurrentLevel; 

          }*/

        List<EntityID> allFoodItems = new List<EntityID>();

        private void GetAllFoodItems()
        {
            allFoodItems.Clear();

            EntityID foodID;
            IKnownEntityData itemData;
            SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
            
            foreach (var list in foodItemsGroup.Food) // this can trigger recompute of Food list, if dirty.
            {
                for (int i = list.Value.Count - 1; i >= 0; i--)
                {
                    foodID = list.Value[i];

                    if (IsValidFoodItem(foodID, entity, bioEntity, sharedKnowledge, foodItemsGroup, out itemData))
                    {
                        allFoodItems.Add(itemData.EntityID);
                    }

                   /* if (!HandleOwnerDataResult(sharedKnowledge, foodID, foodItemsGroup, out itemData))
                    {
                        continue;
                    }

                    GetFoodItem(itemData);*/
                }
            }  
        }


        public static bool IsValidFoodItem(EntityID foodID, Entity entity, BiologicalEntity bioEntity, SharedKnowledge sharedKnowledge, EntityGroup foodItemsGroup, out IKnownEntityData itemData) // BiologicalEntity bioEntity,)
        {
            if (!HandleOwnerDataResult(sharedKnowledge, foodID, foodItemsGroup, out itemData))
            {
                return false;
            }

            if (bioEntity.IsEatable(itemData)
                && IsValidPlaysiteItem(entity, itemData, entity.EntityType.BiologicalType.HoldsFoodWhenEating))
            {
                return true;
            }

            return false;
        }

       /* private void GetFoodItem(IKnownEntityData itemData)
        {
            if (bioEntity.IsEatable(itemData)     
                && IsValidPlaysiteItem(entity, itemData, entity.EntityType.BiologicalType.HoldsFoodWhenEating))
            {
                allFoodItems.Add(itemData.EntityID);
            }
        }*/

        /*       
        private bool IsEatable(IKnownEntityData itemData) //EntityType item)
        {
            return entity.BiologicalEntity.IsEatable(itemData);              
        }*/

        private bool HasRoomInStomach(IKnownEntityData item)
        {
            return currentStomachRoom > item.Bulk;
        }


      /*  private double ScoreDinnerTime()
        {
            return Math.Pow(The.Sim.GetTimePhaseProgress(Sim.DayPhases.Leisure), 3);

        }

        private double ScoreAfterDinnerTime()
        {
            return Math.Pow(The.Sim.GetTimePhaseProgress(Sim.DayPhases.Sleep), 3);
        }*/


        /// <summary>
        /// TODO: make sure this score is PER BULK!!!
        /// cannot be used for substance item types (carcasses)
        /// </summary>
        private Dictionary<EntityType, double> cachedNutritionScores = new Dictionary<EntityType, double>();



       

        private double ScoreNutrientsInExtractedItems(IKnownEntityData foodSource, ref ProcessType extractionProcess)
        {
            List<Tuple<ProcessType, EntityType, float>> listOfFoodTypes = null;
             if (extractionProcess != null)
            { 
                // score the output of this process only:
                bioEntity.GetConsumableFoodExtractionResult(foodSource, extractionProcess, true, ref listOfFoodTypes);
            }
            else
            {
                // score all possible extraction processes and their outputs          
                bioEntity.GetConsumableFoodExtractionResults(foodSource, true, ref listOfFoodTypes);
            }

             double bestScore = 0;
             if (listOfFoodTypes != null)
             {
                 // return the best one:     
                 Dictionary<FoodNutrientType, float> nutrientBulkAmounts = new Dictionary<FoodNutrientType, float>();
                 double score;

                 ProcessType bestProcessType = null;
                 foreach (var item in listOfFoodTypes)
                 {
                     nutrientBulkAmounts.Clear();
                     Food.UpdateNutrientAmounts(item.Item2, item.Item3, nutrientBulkAmounts);

                     score = ScoreNutrients(item.Item2, item.Item3, nutrientBulkAmounts);

                     if (score > bestScore)
                     {
                         bestScore = score;
                         bestProcessType = item.Item1;
                     }

                 }

                 if (extractionProcess == null)
                 {
                     extractionProcess = bestProcessType;
                 }
             }
             else //bso not necessarily an illegal outcome; agent begins job with substance still available but arrives after the last substance have been removed
             {
                     // ??? meat substance is 0?
        //             Debug.Assert(false); MP may 27: removed this because it was annoying me
             }

            return bestScore;

        }

        /// <summary>
        /// score the food item according to how well it satisfies the current food need.
        /// </summary>
        /// <param name="foodItem"></param>
        /// <returns></returns>
        private double ScoreNutrients(EntityType entityType, float bulk, Dictionary<FoodNutrientType, float> nutrientBulkAmounts) 
        {
            // NOTE: score scraps the same as full bulk items. Otherwise we get undesired behaviour.
            // TODO: to ensure agents eat until they are full, they should gather all items before eating them.

            double score;

            bool hasFullBulk = false;

            if (entityType.ItemType.MaximumBulk.HasValue
                && Common.IsEqual(bulk, entityType.ItemType.MaximumBulk.Value)
                && entityType.SubstancesType == null)
            {
                hasFullBulk = true;
            }

            if (hasFullBulk) // only cache scores for whole items...
            {
                if (cachedNutritionScores.TryGetValue(entityType, out score))
                {
                    return score;
                }
            }

            FoodNutrientType foodNutrientType;
            float nutrientBulk;


            float nutrientsScore = 0f;
                       
            // first compute the 'lengths', for normalizing        
            float unsatisfiedEssentialNeedsLength = bioEntity.Needs.UnsatisfiedEssentialNeedsLength; // 0f;
            float unsatisfiedNonEssentialNeedsLength = bioEntity.Needs.UnsatisfiedNonEssentialNeedsLength; // 0f;

           // bioEntity.Needs.UpdateUnsatisfiedNeedsLength(out unsatisfiedEssentialNeedsLength, out unsatisfiedNonEssentialNeedsLength);
            
        

            float essentialNutrientLength = 0f;
            float nonEssentialNutrientLength = 0f;
            foreach (var need in bioEntity.Needs.NeedsList)
            {
                if (need.Value.NeedType.FoodNeedType != null)
                {
                    foodNutrientType = need.Value.NeedType.FoodNeedType.FoodNutrientType;
                    nutrientBulk = 0f;
                    nutrientBulkAmounts.TryGetValue(foodNutrientType, out nutrientBulk);

                    // first get the relative value (0 - 1?):
                    float relativeValue = nutrientBulk / need.Value.FoodNeed.TotalNeededNutrientBulk;
                         
                    if (need.Value.NeedType.FoodNeedType.IsEssential)
                    {
                        essentialNutrientLength += relativeValue;
                    }
                    else
                    {
                        nonEssentialNutrientLength += relativeValue;
                    }
                }
            }

            // don't mix essential and non-essential needs when computing the score.
            if (nonEssentialNutrientLength > 0f)
            {
                nutrientsScore = ScoreNutrients(false, nutrientBulkAmounts, unsatisfiedNonEssentialNeedsLength, nonEssentialNutrientLength, bioEntity.Needs.NoOfNonEssentialFoodNeeds);
            }
            else
            {
                nutrientsScore = ScoreNutrients(true, nutrientBulkAmounts, unsatisfiedEssentialNeedsLength, essentialNutrientLength, bioEntity.Needs.NoOfEssentialFoodNeeds);
            }


         /*   foreach (var need in bioEntity.Needs.NeedsList)
            {
                if (need.Value.NeedType.FoodNeedType != null) // need.Value.NeedType.NeedClass == AINeedClass.Food)
                {
                    foodNutrientType = need.Value.NeedType.FoodNeedType.FoodNutrientType;
                    
                    if (nutrientBulkAmounts.TryGetValue(foodNutrientType, out nutrientBulk) 
                        && nutrientBulk > 0f) // if the nutrient bulk is 0, then the satisfaction score is also 0.
                    {                       

                        // NEW: compute the distribution of nutrients. 
                       
                        float relativeValue = nutrientBulk / need.Value.FoodNeed.TotalNeededNutrientBulk;
                        // add them together to get the length.

                        // normalize, so small bulk items are equal to large bulk items.
                        float normalizedValue = relativeValue / essentialNutrientLength;
                        
                       
                        // compare with needs distribution and score those matching the best
                        float unsatisfiedNeed = 1f - need.Value.CurrentLevel;
                        
                        float normalizedNeed = unsatisfiedNeed / unsatisfiedEssentialNeedsLength;

                   
                        float difference = Math.Abs(normalizedValue - normalizedNeed);
                        float matchScore = 1f - difference; // 0: 1, >1: 0
                        satisfaction = 0.5f * matchScore;

                      

                        // weigh the satisfaction so that if our current level is very low, we get higher satisfaction from the nutrients:
                      //  satisfactionScore = 4f * (1f - need.Value.CurrentLevel) * satisfaction; // 0.5 -> 1f
                       // satisfactionScore = 2f * (1f - need.Value.CurrentLevel) * satisfaction; // 0 -> 1f
                        // try to avoid reaching starvation level for any need:
                        satisfactionScore = 3f * (float)Math.Pow(unsatisfiedNeed, 2f) * satisfaction; // 0 -> 2f

                        // clamp 0 - 1
                        //  satisfactionScore = Common.ClampTop(satisfactionScore, 1f);

                      
                        if (need.Value.NeedType.FoodNeedType.IsEssential)
                        {
                            satisfiesEssentialNeed = true;
                        }
                        else
                        {
                            satisfiesNonEssentialNeed = true;
                        }
                      
                        satisfactionScoreTotal += satisfactionScore;
                    }
                }
            }


            int needsToDivideBy = 0;

            // divide the total satisfaction by the number of needs:
            // for regular food, don't include stimulants and other needs in the score:
            if (satisfiesEssentialNeed)
            {
                needsToDivideBy += bioEntity.Needs.NoOfEssentialFoodNeeds;
            }

            if (satisfiesNonEssentialNeed)
            {
                needsToDivideBy += bioEntity.Needs.NoOfNonEssentialFoodNeeds;
            }
                       
            if (needsToDivideBy > 0)
            {
                satisfactionScoreTotal = satisfactionScoreTotal / needsToDivideBy;
            }
            */


            score = Common.ClampTop(nutrientsScore, 1f);

            // cache it by type if unmodified bulk...
            if (hasFullBulk)
            {
                cachedNutritionScores.Add(entityType, score);
            }

            return score;
        }


        private float ScoreNutrients(bool countEssentialNeeds,  Dictionary<FoodNutrientType, float> nutrientBulkAmounts, float needsLength, float nutrientsLength, int noOfNeeds)
        {
            FoodNutrientType foodNutrientType;
            float nutrientBulk;

          //  float satisfaction;
          //  float satisfactionScore = 0f;

            float satisfactionScoreTotal = 0f;

            foreach (var need in bioEntity.Needs.NeedsList)
            {
                if (need.Value.NeedType.FoodNeedType != null) // need.Value.NeedType.NeedClass == AINeedClass.Food)
                {
                    if ((need.Value.NeedType.FoodNeedType.IsEssential && countEssentialNeeds)
                        || (!need.Value.NeedType.FoodNeedType.IsEssential && !countEssentialNeeds))
                    {
                        foodNutrientType = need.Value.NeedType.FoodNeedType.FoodNutrientType;

                        if (nutrientBulkAmounts.TryGetValue(foodNutrientType, out nutrientBulk)
                            && nutrientBulk > 0f) // if the nutrient bulk is 0, then the satisfaction score is also 0.
                        {

                            // NEW: compute the distribution of nutrients. The best match to current needs should score highest, regardless of bulk.

                            // first get the relative value (0 - 1?):
                            float relativeValue = nutrientBulk / need.Value.FoodNeed.TotalNeededNutrientBulk;
                         
                            // normalize, so small bulk items get equal score to large bulk items.
                            float normalizedValue = relativeValue / nutrientsLength;
                           // float normalizedValue = nutrientBulk / nutrientsLength;


                            // compare with needs distribution and score those matching the best
                            float unsatisfiedNeed = 1f - need.Value.CurrentLevel;

                            float normalizedNeed = unsatisfiedNeed / needsLength;

                                                        
                            float difference = Math.Abs(normalizedValue - normalizedNeed);
                            float matchScore = Common.ClampBottom(1f - difference, 0f); // 0: 1, >1: 0
                            matchScore = (float)Math.Pow(matchScore, 2d); // weigh quadratic
                            matchScore = 0.5f * matchScore; // keep this, or adjust the satisfaction score formula below

                            /* OLD:
                            // disregard amounts to give equal behaviour for small scraps and bigger food items:
                            if (Common.IsGreaterThan(need.Value.FoodNeed.CurrentNeededNutrientBulk, 0f))
                            {
                                satisfaction = 0.5f;
                            }
                            else
                            {
                                satisfaction = 0f;
                            }*/


                            // weigh the satisfaction so that if our current level is very low, we get higher satisfaction from the nutrients:
                            //  satisfactionScore = 4f * (1f - need.Value.CurrentLevel) * satisfaction; // 0.5 -> 1f
                            // satisfactionScore = 2f * (1f - need.Value.CurrentLevel) * satisfaction; // 0 -> 1f
                            // try to avoid reaching starvation level for any need:
                            float satisfactionScore = 3f * (float)Math.Pow(unsatisfiedNeed, 2f) * matchScore; // 0 -> 2f

                            // clamp 0 - 1
                            //  satisfactionScore = Common.ClampTop(satisfactionScore, 1f);

                            // total > 0.9: starving
                            // total > 0.45: will eat

                            satisfactionScoreTotal += satisfactionScore;
                        }
                    }
                }
            }

            satisfactionScoreTotal = satisfactionScoreTotal / noOfNeeds; // since we are adding up scores based on differences, divide by the number of needs too, so multiple needs won't score higher
            return satisfactionScoreTotal;
        }


        private CalculateResult GetAllGatheringPlacesSortedByDistance(Entity entity, EntityType entityType, EntityGroup owner,
           SharedKnowledge sharedKnowledge, RegionMap footRegionMap, ref List<ItemDistance> sortedList, float? maxDistance = null) //List<Entity> sortedList)
        {
            List<EntityID> items;
            if (owner.Structures.TryGetValue(entityType, out items))
            {
                return GetSortedListOfEntities(entity, owner, sharedKnowledge, footRegionMap, items, ref sortedList, 
                    itemData => itemData.EntityType.GatheringSiteType != null && itemData.IsCompleted(),
                    maxDistance);
            }

            return CalculateResult.Done;
        }
        

       

        public CalculateResult ScoreEating(double minimumRatingToConsider, /*ref IKnownEntityData bestFoodItem,*/ ref double bestScore) // ref Tuple<Entity, double> bestItem) //ref double score)
        {
            ThreatStance threatStanceToUse;
            RegionMap regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);

            EntityID item;
            double score = 0d;
            IKnownEntityData itemData;
            ProcessType extractionProcess;
            for (; currentIndex < allFoodItems.Count; currentIndex++)
            {
                item = allFoodItems[currentIndex];

               
                extractionProcess = null;

                // the score includes Priority!
                CalculateResult result = ScoreFoodItem(regionMapToUse, threatStanceToUse, item, out itemData, ref extractionProcess, ref score);
                if (result == CalculateResult.Processing)
                {
                    // wait for the result...                    
                    return result;
                }
                else 
                {
                    allScores.Add(new Tuple<IKnownEntityData, ProcessType, double>(itemData, extractionProcess, score));
                }

            }



            // Important! Remove the combos with zero score:
            allScores.RemoveAll(c => c.Item3 == 0);

            allScores.Sort((a, b) => b.Item3.CompareTo(a.Item3));

            Tuple<IKnownEntityData, ProcessType, double> bestItem = null;
            Tuple<IKnownEntityData, ProcessType, double> foodItem = null;

            for (int i = 0; i < allScores.Count; i++)
            {
                foodItem = allScores[i];
                if (!scoringWasInterrupted || IsValidPlaysiteItem(entity, foodItem.Item1, 
                    entity.EntityType.BiologicalType.HoldsFoodWhenEating)) // only need to check for validity if we are resuming.
                {
                    if (foodItem.Item3 < minimumRatingToConsider)
                    {
                        break; // the score is not good enough. we have reached the low scores and are now finished
                    }
                }
                else
                {
                    continue; // this combo is no longer available, skip it.
                    //break; 
                }

                /*
                when considering food not owned by us:
                * - item must not be carried
                * - not be equipped
                */
                // we have the best item now. See if we need to cancel anyone:              
                if (entity.IsOwnedByUs(foodItem.Item1))
                {
                    GetItemUsersToCancel(foodItem.Item1, ref needsToBeCancelled, ref itemsToBeDropped, true); //, true);                  
                }

                if (needsToBeCancelled.Count == 0 && itemsToBeDropped.Count == 0)
                {   // WINAR!!! No one to cancel
                    bestItem = foodItem;
                    break;
                }
                else if (IsScoreBetterThanAllInvolveds(foodItem.Item3, needsToBeCancelled))
                {   // there are people doing this job. Only take it from them if our score is better than theirs by a certain margin.


                    bestItem = foodItem;
                   
                    /*bestResourceItem = currentResourceItem; // only relevant for gather job
                    bestInputItem = currentInputItem; // only relevant for process jobs without a job location
                    bestLocation = currentLocation;*/

                    break;
                }

                // the job was taken and we didn't score high enough to take it from them. continue looking.
            }

            if (bestItem != null)
            {
                if (entity.PersonEntity != null)
                {
                    // see if we can find a place to eat:
                    CalculateResult result = FindGatheringPlaceToEatMeal();

                    if (result == CalculateResult.Processing)
                    {
                        return CalculateResult.Processing;
                    }
                }    

                bestFoodItem = bestItem.Item1 != null? bestItem.Item1.EntityID : (EntityID?)null;
                bestScore = bestItem.Item3;
                bestExtractionProcess = bestItem.Item2;
            }
            else
            {
                bestFoodItem = null;
                bestScore = 0d;
            }
            

            // return bestCombo;
            return CalculateResult.Done;
            
        }

      /*  private static bool ItemIsOwnedByUs(Entity agent, IKnownEntityData food)
        {


            return food.OwnedBy != null && ownerOfFoodItems.Contains(food); // food.OwnedBy == ownerOfFoodItems.ID; 

           // return ownerOfFoodItems != null && food.OwnedBy != null && ownerOfFoodItems.Contains(food); // food.OwnedBy == ownerOfFoodItems.ID; 
        }*/


        

        private double ScoreFreshMeal(IKnownEntityData food)
        {
            if (food.EntityType.ItemType.FoodType.IsMeal == true
                && food.Condition.Value > 0.95f) // freshness
            {   
                return 1.0;
            }
            else return 0.0;
        }

        /// <summary>
        /// score should be higher the closer the food is to spoiling! To avoid food waste and preserve rations
        /// </summary>
        /// <param name="food"></param>
        /// <returns></returns>
        public static double ScoreCondition(IKnownEntityData food, double? minimumDaysLeftUntilSpoiling)
        {
            if (food.Condition.HasValue)
            {
                if (food.ConditionChangeSpeed.HasValue && !Common.IsZero(food.ConditionChangeSpeed.Value))
                {
                    double timeInDaysUntilSpoiling = NonLivingEntity.GetDaysLeftUntilBreakdown(food.Condition.Value, food.ConditionChangeSpeed.Value);
                    
                    // closest to zero should give highest score:
                    double maxTimeConsidered = 6d;
                    timeInDaysUntilSpoiling = Common.Clamp(timeInDaysUntilSpoiling, 0d, maxTimeConsidered);

                    if (minimumDaysLeftUntilSpoiling.HasValue
                        && timeInDaysUntilSpoiling < minimumDaysLeftUntilSpoiling.Value)
                    {
                        return 0d;
                    }

                    // invert:
                    double inverted = maxTimeConsidered - timeInDaysUntilSpoiling;

                    double scaled = inverted / maxTimeConsidered;

                    double score = Math.Pow(scaled, 2d);

                    score = Common.Clamp(score, 0d, 1d);

                    return score;
                }
                else return 0.1;
            }
            else return 0.0;
        }

        /// <summary>
        /// the item can either be consumed directly, or food can be extracted from it (carcasses) - includes Priority!
        /// </summary>
        /// <param name="regionMapToUse"></param>
        /// <param name="threatStance"></param>
        /// <param name="foodID"></param>
        /// <param name="foodData"></param>
        /// <param name="extractionProcess"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public CalculateResult ScoreFoodItem(RegionMap regionMapToUse, ThreatStance? threatStance, EntityID foodID, out IKnownEntityData foodData, ref ProcessType extractionProcess, ref double score)
        {
            score = 0;
            foodData = null;

            if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(foodID, out foodData)))
            {
                return CalculateResult.Done;
            }

           
           
            if (regionMapToUse == null || threatStance == null)
            {
                ThreatStance newThreatStance;
                regionMapToUse = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out newThreatStance);

                threatStance = newThreatStance;
            }

            double travelTimeScore = 0;

         //   RegionMap.Result result = ScoreTravelTime(regionMapToUse, entity.Location, foodData.Location, entity, ref travelTimeScore);
            RegionMap.Result result = ScoreTravelTime(regionMapToUse, entity, foodData, ref travelTimeScore);
            if (result == RegionMap.Result.Wait)
            {
                // wait for the result...                    
                return CalculateResult.Processing;
            }
            else if (result == RegionMap.Result.NoAccess)
            {
                return CalculateResult.Done;
            }

            if (entity.ID == (EntityID)4951)
            {
                if (foodData.EntityType.KeyName.Contains("item:smokedStreakFin"))             
                {

                }
                else if (foodData.EntityType.KeyName.Contains("item:hardtack")) 
                {

                }
            }
    
            // score food extraction result if not consumable (GoalEat will extract the food item):
            double nutrientsScore;
            if (bioEntity.ConsumeProcesses.ContainsKey(foodData.EntityType))
            {
                // score consuming the item directly:
                nutrientsScore = ScoreNutrients(foodData.EntityType, foodData.Bulk, foodData.NutrientBulkAmounts); // , ref extractionProcess);
            }
            else
            {
                // score the possible extracted products:
                nutrientsScore = ScoreNutrientsInExtractedItems(foodData, ref extractionProcess);               
            }

            if (Common.IsZero(nutrientsScore))
            {
                // bail out here - extraction process is probably invalid also.
                score = 0d;
                return CalculateResult.Done;
            }

            double conditionScore = ScoreCondition(foodData, null);

            double consumeDirectlyScore = ScoreCanConsumeDirectly(extractionProcess); 

            if (entity.PersonEntity != null)
            {
                // TODO: replace 'dinnertime' with increased score for eating right before sleeping. To avoid sleep-starving...
                // also remove cooked meals consideration to decrease complexity.

                // people score - eat at regular times and prefer cooked meals

                /*double dinnerTimeScore = ScoreDinnerTime();

                double freshMealScore = ScoreFreshMeal(foodData);
                */

                // now clamp the score so we only eat at opportune times:

                /// if starving -> eat anything, but prefer cooked meals 
                /// if not starving, but hasn't eaten for a while:
                ///     if it is dinnertime -> eat cooked meals only
                ///     if it is after dinnertime -> eat anything
                /// if almost full
                ///     if it is dinnertime -> eat cooked meals only
                 
                // nutrients score 0: full, 1: starving

               /* if (nutrientsScore < 0.1f) // almost full?
                {                   
                    // SOCIAL EATING - waste of food???
                    // if it is dinnertime -> eat cooked meals only
                    if (entity.EntityType.Person.AlwaysEatAtDinnerTime
                        && dinnerTimeScore >= 1d
                        && freshMealScore >= 1d)
                    {
                        score = ComputePeopleEatScore(travelTimeScore, nutrientsScore, conditionScore, dinnerTimeScore, freshMealScore, consumeDirectlyScore);
                    }
                    else
                    {
                        // don't eat.
                        score = 0d;
                    }
                   
                }
                else if (nutrientsScore < GameData.Instance.AIConstants.NutrientsScoreForEating) // 0.5) /// if not starving, but hasn't eaten for a while:
                {                   
                    ///     if it is dinnertime -> eat cooked meals only
                    if (dinnerTimeScore >= 1d
                        && freshMealScore >= 1d)
                    {
                        score = ComputePeopleEatScore(travelTimeScore, nutrientsScore, conditionScore, dinnerTimeScore, freshMealScore, consumeDirectlyScore);
                    }
                    else if (ScoreAfterDinnerTime() > 0.8)
                    {  ///     if it is after dinnertime -> eat anything
                        score = ComputePeopleEatScore(travelTimeScore, nutrientsScore, conditionScore, dinnerTimeScore, freshMealScore, consumeDirectlyScore);
                    }
                    else
                    {
                        // don't eat yet.
                        score = 0d;
                    }
                }
                if (nutrientsScore >= GameData.Instance.AIConstants.NutrientsScoreToTriggerStarvedEating) // 1) // if starving -> eat anything, but prefer cooked meals. ignore dinner time
                {
                    score = ComputePeopleStarvingScore(travelTimeScore, nutrientsScore, conditionScore, freshMealScore, consumeDirectlyScore);
                }
                else // 0.5 - 1: if not really starving yet -> eat anything, but prefer cooked meals.
                {
                    score = ComputePeopleEatScore(travelTimeScore, nutrientsScore, conditionScore, dinnerTimeScore, freshMealScore, consumeDirectlyScore);               
                }*/

                if (nutrientsScore >= GameData.Instance.AIConstants.NutrientsScoreToTriggerStarvedEating) // 1) // if starving -> eat anything, but prefer cooked meals. ignore dinner time
                {
                    score = ComputePeopleStarvingScore(travelTimeScore, nutrientsScore, conditionScore, consumeDirectlyScore);
                }
                else if (nutrientsScore >= GameData.Instance.AIConstants.NutrientsScoreForEating) // 0.5 - 1: if not really starving yet -> eat anything, but prefer cooked meals.
                {
                    score = ComputePeopleEatScore(travelTimeScore, nutrientsScore, conditionScore, consumeDirectlyScore);
                }
                else
                {
                    score = 0d;   // don't eat yet.
                }

                score = ApplyPriority(score);


                return CalculateResult.Done;

            }
            else 
            {
                // critter score - eat at all times.
                if (nutrientsScore < 0.5)
                {
                    // don't eat yet.
                    score = 0d;
                }
                else
                {
                    score = 0.45 * travelTimeScore + 0.40 * nutrientsScore + 0.15 * consumeDirectlyScore;
                }

                score = ApplyPriority(score);

            }
            
            return CalculateResult.Done;
        }

        private double ScoreCanConsumeDirectly(ProcessType extractProcess)
        {
            if (extractProcess == null)
            {
                return 1d;
            }
            else
            {
                return 0d;
            }
        }

        private static double ComputePeopleEatScore(double travelTimeScore, double nutrientsScore, double conditionScore, double consumeDirectlyScore)
        {
            return 0.45 * travelTimeScore + 0.4 * nutrientsScore + 0.1 * conditionScore + 0.05 * consumeDirectlyScore;
        }

        private static double ComputePeopleStarvingScore(double travelTimeScore, double nutrientsScore, double conditionScore, double consumeDirectlyScore)
        {
            return 0.05 * travelTimeScore + 0.85 * nutrientsScore + 0.02 * conditionScore + 0.08 * consumeDirectlyScore;
        }

        /*

        private static double ComputePeopleEatScore(double travelTimeScore, double nutrientsScore, double conditionScore, double dinnerTimeScore, double freshMealScore, double consumeDirectlyScore)
        {
            return 0.45 * travelTimeScore + 0.25 * nutrientsScore + 0.1 * dinnerTimeScore + 0.05 * freshMealScore + 0.1 * conditionScore + 0.05 * consumeDirectlyScore;
        }

        private static double ComputePeopleStarvingScore(double travelTimeScore, double nutrientsScore, double conditionScore, double freshMealScore, double consumeDirectlyScore)
        {
            return 0.05 * travelTimeScore + 0.85 * nutrientsScore + 0.03 * freshMealScore + 0.02 * conditionScore + 0.05 * consumeDirectlyScore;
        }*/

        public override bool CancelCurrentTakers()
        {
            return CancelEntities(needsToBeCancelled, itemsToBeDropped);
        }

        public override bool CanTakeGoal()
        {
            if (bestFoodItem != null)
            {
                needsToBeCancelled.Clear();
                itemsToBeDropped.Clear();

                 IKnownEntityData bestItemData = null;
                 if (!EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(bestFoodItem.Value, out bestItemData)))
                 {
                     if (entity.IsOwnedByUs(bestItemData))
                     {
                         GetItemUsersToCancel(bestItemData, ref needsToBeCancelled, ref itemsToBeDropped, true); //, true);
                     }

                     if (needsToBeCancelled != null)
                     {

                         if (IsScoreBetterThanAllInvolveds(bestScore, needsToBeCancelled))
                         {
                             return true;
                         }
                     }
                 }
            }

            return false;
        }

        public override bool SetGoal()
        {
            base.SetGoal();

            //if (!CancelEntities(needsToBeCancelled, itemsToBeDropped))
            //{
            //    return false; // failed to cancel jobs/ force drop of food
            //}

            Intelligence entityIntelligence = entity.Intelligence;


            entityIntelligence.SetTopLevelGoal(new GoalEat(entity, bestFoodItem.Value, 
                gatheringPlaceToEat, 
                GetOwnerID(foodItemsGroup),
                bestExtractionProcess,
                GetOwnerIDs(ownersOfVehicles)) { GoalEvaluator = this },bestScore);

            return true;
         
        }
    }
}
