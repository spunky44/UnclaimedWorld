using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Biological;
using System.Linq;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// this goal moves the agent to the food source, and, if needed, extracts/converts a cosumable item from it, then consumes the item
    /// </summary>
    class GoalEat : CompositeGoal, IIDEventSubscriber, ITopLevelGoal
    {
        private enum State { Moving, Extracting, Consuming };

        private State currentState;
        public double TimeSpentInTopLevelGoal { get; set; }

        private EntityID? itemToConsumeID;
        private EntityID? itemToExtractFromID;

        private List<EntityID> additionalItemsToConsume = new List<EntityID>();

        private EntityID? extractedItemID;
        
        EntityID? placeToEat;

        EntityGroupID? ownerOfFoodItem;

        bool itemIsDrunk = false;

        /// <summary>
        /// can be null
        /// </summary>
        private ProcessType extractionProcessToUse;


        private float? maxAmountToExtract = null;

        MethodID? extractionCompleteMethodID;


        public GoalEat(Entity entity, EntityID foodItem, EntityID? placeToEat, EntityGroupID? ownerOfFoodItem, ProcessType extractionProcessToUse,  List<EntityGroupID> ownersVehicles)
            : base(entity)
        {

            if (entity.ID == (EntityID)4600)
            {

            }

            this.ownerOfFoodItem = ownerOfFoodItem;
            this.ownersOfVehicles = ownersVehicles;

            if (extractionProcessToUse == null)
            {
                this.itemToConsumeID = foodItem;
            }
            else
            {
                this.itemToExtractFromID = foodItem;
                this.extractionProcessToUse = extractionProcessToUse;
            }

            this.placeToEat = placeToEat;

        }

       
       

        public GoalEat()
        {
        }


        private bool IsConsuming()
        {
            return itemToConsumeID.HasValue;
        }


        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            IKnownEntityData foodData;
           


            // get the first item to move to:
            EntityID? foodItem = itemToConsumeID ?? itemToExtractFromID;


            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(foodItem.Value, out foodData)))
            {
                return;
            }

            if (foodData.EntityType.ItemType.FoodType != null)
            {
                itemIsDrunk = foodData.EntityType.ItemType.FoodType.IsDrunk;
            }
    


            if (IsConsuming()
                && entity.EntityType.BiologicalType.HoldsFoodWhenEating) // hold the item we are consuming in the 'hands'
            {
                IKnownEntityData firstFoodItem = foodData;

                List<IKnownEntityData> foodItemsToConsume = new List<IKnownEntityData>();

                foodItemsToConsume.Add(firstFoodItem);

                BiologicalEntity bioEntity = entity.BiologicalEntity;
                float totalFoodBulk;
                // gather additional food items in the vicinity, but only if consuming
                if (foodData.NutrientBulkAmounts.Any(k => bioEntity.Needs.IsEssential(k.Key)))
                {                 
                    GetAdditionalItemsToConsume(firstFoodItem, foodItemsToConsume, out totalFoodBulk);
                }
                else
                {
                    totalFoodBulk = firstFoodItem.Bulk;
                }


                foreach (var item in foodItemsToConsume)
                {
                    entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(item.EntityID, entity.EntityID);                    

                    AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, item) { IsFinalDestination = true });


                    // when reaching the first item:
                    // drop items not in use to make room for all the food items:
                    if (item == firstFoodItem)
                    {
                        float totalDropped;
                      //  DropUnneededItemsToMakeCapacity(foodData.Bulk, e => entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(e.ID) != entity.ID, out totalDropped, StorageCompartment.Haul);
                        DropUnneededItemsToMakeCapacity(totalFoodBulk, 
                            e => entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(e.ID) != entity.ID, 
                            out totalDropped, 
                            StorageCompartment.Haul);
                    }

                    if (!PickupItemOrUnloadFirst(item))
                    {
                        return;
                    }
                }
            }
            else
            {
                // when reaching the first item:
                // drop items not in use to make room for the food item:
                float totalDropped;
                DropUnneededItemsToMakeCapacity(foodData.Bulk, e => entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(e.ID) != entity.ID, out totalDropped, StorageCompartment.Haul);
                
                // when extracting
                // this creature prefers to eat the food on the ground:
                if (!UnloadOrDropUnToGround(foodData))
                {
                    return;
                }
            }
           
                      
            if (placeToEat != null) // this could be improved so it is not just for people, but also for critters. The place where the critter wants to eat its food, or where it wants to extract from it
            {
                IKnownEntityData placeToEatData;
                entityIntelligence.GetKnownData(placeToEat.Value, out placeToEatData);

                if (placeToEatData != null)
                {
                    if (placeToEatData.GatheringSite != null)
                    {
                        if (placeToEatData.GatheringSite.CanAddVisitor(ref entity))
                        {
                            AddSubgoal(new GoalArriveAsVisitor(entity, placeToEat.Value, ownersOfVehicles)
                            {
                                //IsFinalDestination = true
                            });
                        }
                        // else???
                    }
                    else
                    {
                        // go into the house, cave or whatever to eat:
                        AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, placeToEatData, GoalMoveToPosition.VehicleUse.FreeUpAfterUse)
                        {
                            IsFinalDestination = true
                        });
                    }                  
                }
            }

            currentState = State.Moving;
            
        }

     //   const int maxAdditionalFoodItems = 5;

        GoalEvaluator.CalculateResult GetAdditionalItemsToConsume(IKnownEntityData firstItem, /* Vector3 nearLocation,*/ List<IKnownEntityData> allFoodItems, out float totalFoodBulk)
        {
            Dictionary<FoodNutrientType, float> essentialBulkNeeds = entity.BiologicalEntity.Needs.GetEssentialNeedBulkAmounts();
            
            totalFoodBulk = firstItem.Bulk; // 0f; // allFoodItems.Sum(f => f.Bulk);
            SubtractNutrients(firstItem, essentialBulkNeeds);


            if (!NeedsMoreFood(essentialBulkNeeds))
            {
                return Goals.GoalEvaluator.CalculateResult.Done;
            }

            additionalItemsToConsume = new List<EntityID>();

            BiologicalEntity bioEntity = entity.BiologicalEntity;
            SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
            IKnownEntityData foodData;
           // List<Pair<EntityID, Vector2>> items = new List<Pair<EntityID, Vector2>>();

            EntityGroup foodItemsGroup = EvaluateEat.GetFoodEntityGroup(entity);

          /*  sharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetEntitiesInRange(firstItem.PlaySiteLocation.ToVector2(),
                                                                     GameData.Instance.AIConstants.MaxDistanceToLookForAdditionalFood, //  200f,
                                                                     (item) => IsValidFoodItem(item, firstItem.EntityID, entity, bioEntity, sharedKnowledge, foodItemsGroup, out foodData),
                                                                     ref items);*/

            ThreatStance stanceToUse = entityIntelligence.ThreatStance;
            RegionMap regionMapToUse = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(
                                           ProtectionLevel.Exposed,
                                           entity.EntityType,
                                           stanceToUse).Layers[SurfaceType.TransportType.Foot].RegionMap;

            // store the distances for scoring
            Dictionary<EntityID, float> cachedDistances = new Dictionary<EntityID, float>();

            List<IKnownEntityData> foodItems = null;
            GoalEvaluator.CalculateResult result = GetNearbyEntities(GameData.Instance.AIConstants.MaxDistanceToLookForAdditionalFood, out foodItems,
                    stanceToUse, regionMapToUse, 
                    (item) => IsValidFoodItem(item, firstItem.EntityID, entity, bioEntity, sharedKnowledge, 
                        foodItemsGroup, out foodData), 
                    cachedDistances);

            if (result == GoalEvaluator.CalculateResult.Processing)
                return GoalEvaluator.CalculateResult.Processing;


            int i = 0;
            float distance;

            // score them so we don't eat the preserved food:
            List<Tuple<IKnownEntityData, float>> scores = new List<Tuple<IKnownEntityData, float>>();

            foreach (var item in foodItems)
            {
                if (cachedDistances.TryGetValue(item.EntityID, out distance))
                {
                    double distanceScore = EvaluateAttackJobs.GetTravelScoreFromDistance(entity, distance);
                    //  double score = distanceScore * 0.6f + weapon.EntityType.ItemType.WeaponType.DesirabilityForUseDefensive * 0.4f;

                    double conditionScore = EvaluateEat.ScoreCondition(item, null);

                    float score = (float)(distanceScore * 0.6f + conditionScore * 0.4f);

                    scores.Add(new Tuple<IKnownEntityData, float>(item, score));
                }

            }

            // sort:
            var sortedList = scores.OrderByDescending(s => s.Item2);
         
            // get the top items:
            foreach (var item in sortedList)
            {
                foodData = item.Item1;

                if (!entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(foodData.Bulk + totalFoodBulk))
                {
                    continue;
                }

                allFoodItems.Add(foodData);
                additionalItemsToConsume.Add(foodData.EntityID);

                totalFoodBulk += foodData.Bulk;

                SubtractNutrients(foodData, essentialBulkNeeds);

                i++;

                if (!NeedsMoreFood(essentialBulkNeeds)
                    || Common.IsGreaterThan(totalFoodBulk, entity.AgentStorage.ItemStorage.TotalCapacity))
                {
                    return Goals.GoalEvaluator.CalculateResult.Done;
                }


                if (i >= GameData.Instance.AIConstants.MaxAdditionalFoodItems)
                {
                    break;
                }
            }

         /*   while (i < foodItems.Count && i < GameData.Instance.AIConstants.MaxAdditionalFoodItems)
            {
                foodData = sortedList[i].it;

                if (!entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(foodData.Bulk + totalFoodBulk))
                {
                    continue;
                }


                allFoodItems.Add(foodData);
                additionalItemsToConsume.Add(foodData.EntityID);

                totalFoodBulk += foodData.Bulk;

                SubtractNutrients(foodData, essentialBulkNeeds);

                i++;

                if (!NeedsMoreFood(essentialBulkNeeds)
                    || Common.IsGreaterThan(totalFoodBulk, entity.AgentStorage.ItemStorage.TotalCapacity))
                {
                    return Goals.GoalEvaluator.CalculateResult.Done;
                }

            }*/

            return Goals.GoalEvaluator.CalculateResult.Done;
        }


        private bool NeedsMoreFood(Dictionary<FoodNutrientType, float> essentialBulkNeeds)
        {
            return essentialBulkNeeds.Any(n => Common.IsGreaterThan(n.Value, 0f));
        }

        private void SubtractNutrients(IKnownEntityData foodData, Dictionary<FoodNutrientType, float> essentialBulkNeeds)
        {
            foreach (var item in foodData.NutrientBulkAmounts)
            {
                float currentValue;
                if (essentialBulkNeeds.TryGetValue(item.Key, out currentValue))
                {
                    currentValue -= item.Value;

                    currentValue = Common.ClampBottom(currentValue, 0f);

                    essentialBulkNeeds[item.Key] = currentValue;
                }
                
            }
        }


     /*   private bool IsFoodForEquipment(EntityID entityID)
        {
            IKnownEntityData entityData;

            if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entityID, out entityData)))
            {
                if (entityData.EntityType.ItemType != null && entityData.EntityType.ItemType.FoodType != null)
                {
                    if (entity.IsOwnedByUs(entityData)) // IsToolOrWeaponOK(entityData))
                    {
                        if (entity.BiologicalEntity.IsEatable(entityData)
                            && GoalEvaluator.IsValidPlaysiteItem(entity, entityData, true))
                        {
                            return true;
                        }
                    }

                }
            }


            return false;
        }*/

        bool IsValidFoodItem(EntityID foodID, EntityID firstFoodItem, Entity entity, BiologicalEntity bioEntity, SharedKnowledge sharedKnowledge, EntityGroup foodItemsGroup, out IKnownEntityData foodData)
        {
            foodData = null;
            if (foodID != firstFoodItem &&
                EvaluateEat.IsValidFoodItem(foodID, entity, bioEntity, sharedKnowledge, foodItemsGroup, out foodData))
            {
                if (entity.IsOwnedByUs(foodData)) 
                {
                    if (ItemIsNotAssignedToImportantJobs(foodData, sharedKnowledge))
                    {
                        // items being carried by others are filtered in GetDistance
                        
                        return true;
                    }
                }

            }
            
            return false;
        }

        public double ScoreGoal()
        {
            double score = 0d;
        
            IKnownEntityData foodData;
            GoalEvaluator.CalculateResult? result = null;

            // which phase are we in..?
            if (currentState == State.Moving)
            {
                if (itemToExtractFromID.HasValue)
                {
                    // score using extraction:
                    result = ((EvaluateEat)GoalEvaluator).ScoreFoodItem(null, null, this.itemToExtractFromID.Value, out foodData, ref extractionProcessToUse, ref score);
                }
                else if (itemToConsumeID.HasValue)
                {
                    // score consuming directly:
                    ProcessType process = null;
                    result = ((EvaluateEat)GoalEvaluator).ScoreFoodItem(null, null, this.itemToConsumeID.Value, out foodData, ref process, ref score);
                }
            }
            else if (currentState == State.Extracting)
            {
                // if we are extracting, the substance has already been consumed, but the products are not finished yet.
                // so we should not score the carcass, since it may contain 0 meat.

                // OLD:
                //result = ((EvaluateEat)GoalEvaluator).ScoreFoodItem(null, null, this.itemToExtractFromID.Value, out foodData, ref extractionProcessToUse, ref score);             
                              
                // let's just return the maximum score - there can hardly be a better goal for us at this point.
                result = Goals.GoalEvaluator.CalculateResult.Done;
                score = 2f * GoalEvaluator.Priority; // #SHREDFIX 1d;
            }
            else if (currentState == State.Consuming)
            {                     
                /*EntityID? itemToScore = itemToConsumeID ?? extractedItemID;

                ProcessType process = null;
                result = ((EvaluateEat)GoalEvaluator).ScoreFoodItem(null, null, itemToScore.Value, out foodData, ref process, ref score);*/

                // if we are eating, the item we want to consume is already destroyed and a product is placed in the stomach. 
                // let's just return the maximum score - there can hardly be a better goal for us at this point.
                result = Goals.GoalEvaluator.CalculateResult.Done;
                score = 2f * GoalEvaluator.Priority; // #SHREDFIX 1d;

            }

            if (result == Goals.GoalEvaluator.CalculateResult.Done)
            {
                return score;
            }
            else
            {
                // we don't have time to wait for the score... return the cached score.
                // also, we want to disregard the answer when it comes back...
                return GetCurrentGoalScore();
            }          

        }

        protected override bool ArePreconditionsOK()
        {
            // don't test preconditions when actually eating/extracting, only when moving towards the meal
       
            if (currentState == State.Moving)
            {
                EntityID? foodItem = itemToConsumeID ?? itemToExtractFromID;

                IKnownEntityData foodData;
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(foodItem.Value, out foodData)))
                {
                    return false;
                }

                if (!EvaluateEat.IsValidPlaysiteItem(entity, foodData, entity.EntityType.BiologicalType.HoldsFoodWhenEating))
                {
                    Status = Goals.Status.Failed;
                    return false;
                }

                // TODO: validate that the food is still in the same location!
            }
          

            return true;
        }

        public override string GetStatus()
        {
            if (itemIsDrunk)
            {
                return "Drinking";

            }
            else
            {
                return "Eating";
            }
        }

       
        protected override void ProcessWhileActive(GameTime elapsed)
        {           
            if (!preconditionsRegulator.IsReady() || ArePreconditionsOK()) 
            {
                //process the subgoals
                Status = ProcessSubgoals(elapsed);

                if (Status == Goals.Status.Completed)
                {                   

                    if (currentState == State.Moving && itemToExtractFromID.HasValue)
                    {                        
                        // we have reached the item we want to extract from - start extracting!
                       
                        currentState = State.Extracting;

                        Entity itemToExtractFrom;
                        if (EntityIsNotSeenDirectly(itemToExtractFromID.Value, out itemToExtractFrom))
                        {
                            return;
                        }

                      //  maxAmountToExtract = CapAmountToConsume(itemToConsume);
                        maxAmountToExtract = entity.AgentStorage.GetFreeStomachCapacity();// don't bite off more than can fit in the stomach... ;)

                        ChangeToEatingStance(extractionProcessToUse);

                        GoalDoProduce extractGoal = new GoalDoProduce(entity, extractionProcessToUse, itemToExtractFrom, maxAmountToExtract, false,
                          null, null, null, null, null);

                        // the process must be valid:
                        SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(extractGoal.ProductionProcess);

                        // we need the extracted item when it exists:
                        process.ProcessCompletedEvent.AddAndRegister(extractGoal_ProductionComplete, 
                            this, out extractionCompleteMethodID);

                        AddSubgoal(extractGoal);

                        Status = Goals.Status.Active;

                    }
                    else if (currentState != State.Consuming)
                    {
                        // we have reached the item we want to consume
                        currentState = State.Consuming;

                        EntityID? finalItemToConsumeID = itemToConsumeID ?? extractedItemID;

                        Entity itemToConsume;
                        if (finalItemToConsumeID == null 
                            || EntityIsNotSeenDirectly(finalItemToConsumeID.Value, out itemToConsume))
                        {
                            return;
                        }

                        // TODO: check that it is close enough and valid!

                      /*  if (maxAmountToExtract == null)
                        {
                            // when carcass eating, this will probably leave some on the ground
                            maxAmountToExtract = CapAmountToConsume(itemToConsume, entity);  
                        }*/


                        // now consume:

                        // fire triggers, events etc:            
                        List<ActionSets> defaultActionSets;
                        entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.Eating, out defaultActionSets);

                        Goal.FireEventActions(entity, null, defaultActionSets, null);


                        AddConsumeGoals(itemToConsume);


                        // NEW: consume any additional items we carry too:
                        if (additionalItemsToConsume != null && additionalItemsToConsume.Count > 0)
                        {
                            Entity additionalFoodItem;
                            foreach (var item in additionalItemsToConsume)
                            {
                                if (!EntityIsNotSeenDirectly(item, out additionalFoodItem))
                                {
                                    AddConsumeGoals(additionalFoodItem);
                                }
                            }
                        }

                        Status = Goals.Status.Active;
                    }
                    else
                    {
                        // done consuming.
                        Status = Goals.Status.Completed;
                    }
                }
            }
            else
            {
                Status = Goals.Status.Failed;
            }          
        }

        private void AddConsumeGoals(Entity itemToConsume) //, float maxBulkToConsume)
        {
            // eat using a consume process - this will only consume what we can store in the stomach:                        
            ProcessType consumeProcess = entity.BiologicalEntity.ConsumeProcesses[itemToConsume.EntityType];

            ChangeToEatingStance(consumeProcess);

            AddSubgoal(new GoalDoProduce(entity, consumeProcess, itemToConsume, /*maxAmountToExtract*/ null, true, null, null, null,
                entity.EntityID, StorageCompartment.Stomach)); // the product should be placed in the entity (its stomach...)
        }

        /// <summary>
        /// cap the bulk based on both stomach capacity and ability to use nutrients
        /// </summary>
        /// <param name="itemToConsume"></param>
        /// <returns></returns>
        public static float CapAmountToConsume(Entity itemToConsume, Entity entity)
        {            
          
            float? bulkExceedingSatisfaction = null;

            // cap the amount to max satisfaction, to avoid wasting food
            BiologicalEntity bioEntity = entity.BiologicalEntity;
            FoodNutrientType foodNutrientType;
            float nutrientBulk;

            foreach (var need in bioEntity.Needs.NeedsList)
            {
                if (need.Value.NeedType.FoodNeedType != null)
                {
                    foodNutrientType = need.Value.NeedType.FoodNeedType.FoodNutrientType; // need.Value.NeedType.FoodNutrientType;

                    if (itemToConsume.Item.Food.NutrientBulkAmounts.TryGetValue(foodNutrientType, out nutrientBulk) //nutrientBulkAmounts.TryGetValue(foodNutrientType, out nutrientBulk)
                        && nutrientBulk > 0f)
                    {
                        float nutrientBulkExceedingSatisfaction = nutrientBulk - need.Value.FoodNeed.CurrentNeededNutrientBulk;
                        if (nutrientBulkExceedingSatisfaction <= 0f)
                        {
                            bulkExceedingSatisfaction = null;
                            break; // one need will not be fully sated, so eat the full amount.
                        }
                        else
                        {
                            // this nutrient will go to waste, compute the item bulk in excess
                            float nutrientPerBulk = itemToConsume.EntityType.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes.FirstOrDefault(n => n.Nutrient == foodNutrientType).Amount;

                            float thisBulkExceedingSatisfaction = nutrientBulkExceedingSatisfaction / nutrientPerBulk;
                            if (bulkExceedingSatisfaction == null)
                            {
                                bulkExceedingSatisfaction = thisBulkExceedingSatisfaction;
                            }
                            else if (thisBulkExceedingSatisfaction < bulkExceedingSatisfaction.Value)
                            {
                                bulkExceedingSatisfaction = thisBulkExceedingSatisfaction;
                            }
                        }
                    }
                }
            }

            float maxAmountToExtract = entity.AgentStorage.GetFreeStomachCapacity();// don't bite off more than can fit in the stomach... ;)
            if (bulkExceedingSatisfaction.HasValue)
            {
                float? maxAmountToExtractNutrientCapped;
                maxAmountToExtractNutrientCapped = itemToConsume.Bulk - bulkExceedingSatisfaction.Value;

                return Math.Min(maxAmountToExtract, maxAmountToExtractNutrientCapped.Value);
            }
            else
            {                
                return maxAmountToExtract;
            }
            
        }

        private void ChangeToEatingStance(ProcessType processType)
        {
            if (entity.HasStance())
            {
                ChangeStance(entity.Locomotor.Stance.PickRandomProcessStance(processType.StanceTypes));
            }
        }

        void extractGoal_ProductionComplete(IKnownProcess process) 
        {
            if (process.OutputEntities != null && process.OutputEntities.Count > 0)
            {
                extractedItemID = process.OutputEntities[0];

                // mark the product as In use:
                Entity extractedItem = Entity.FindByID(extractedItemID.Value);

                if (extractedItem != null && extractedItem.EntityType.ItemType != null && extractedItem.EntityType.ItemType.FoodType != null)
                {                   
                    entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(extractedItem.EntityID, entity.EntityID);     
                }
            }

            System.Diagnostics.Debug.Assert(extractedItemID.HasValue, "No extracted product, why..?");
        }


        private bool IsInRangeOfFood(Entity food)
        {
            if (Common.DistanceOctile(entity.PlaySiteLocation, food.PlaySiteLocation) > 30f)// distance check... don't care if we are not completely within reach here. We have done the fine movement we need.
            {
                Status = Goals.Status.Failed;

                //   ExitIfFailedOrCompleted();
                return false; 
            }

            return true;
        }


        public override void Deactivate()
        {
            // i think only people will respect locks on food...
            if (itemToConsumeID.HasValue)
            {
                ReleaseLockOnItem(itemToConsumeID.Value);
            }

            if (itemToExtractFromID.HasValue)
            {
                ReleaseLockOnItem(itemToExtractFromID.Value);
            }

            if (extractedItemID.HasValue)
            {
                ReleaseLockOnItem(extractedItemID.Value);
            }

            if (additionalItemsToConsume != null)
            {
                foreach (var item in additionalItemsToConsume)
                {
                    ReleaseLockOnItem(item);
                }
            }


            if (placeToEat.HasValue)
            {
                IKnownEntityData containerData;
                entityIntelligence.GetKnownData(placeToEat.Value, out containerData);

                if (containerData != null && containerData.GatheringSite != null)
                {
                    containerData.GatheringSite.RemoveVisitor(entity.EntityID);
                }
            }

            // consume food stomach contents (will already have been done if the processes completed):
            ConsumeStomachContents(entity);

            if (extractionCompleteMethodID.HasValue)
            {
                // cleanup the callback method
                ActionLookup<List<EntityID>>.Remove(extractionCompleteMethodID.Value);
            }

            //base.Terminate();
        }

        private void ReleaseLockOnItem(EntityID itemID)
        {
            IKnownEntityData foodData;
            entityIntelligence.GetKnownData(itemID, out foodData);
            if (foodData != null)
            {
                entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(foodData.EntityID, entity.EntityID);                 
                     
            }
        }

        public static void ConsumeStomachContents(Entity entity)
        { 
            //  consumes nutrients and destroys the item
            entity.AgentStorage.IterateContained(StorageCompartment.Stomach,
                foodItem =>               
                {
                    if (foodItem.EntityType.ItemType != null 
                        && foodItem.EntityType.ItemType.FoodType != null)
                    {
                        // here we should gather nutrition stats... how do we get the wasted nutrients? Too hard?
                        foodItem.Item.Food.ConsumeBy(entity);

                        // NEW: gather stats also:
                       // entity.Intelligence.Allegiance.Statistics.AddProductionEvent(foodItem.EntityType, Allegiances.Statistics.ProductionStatistics.StatTypes.Consumed, 1);

                        /*                      
                        IOwner owner;
                        LookUpOwners.ResolveEntityOwner(foodItem, out owner);

                        IKnownEntityData entityData;
                        if (owner != null && owner.Allegiance != entity.Intelligence.Allegiance 
                            && owner.Allegiance.SharedKnowledge.GetKnownData(foodItem.ID, out entityData) == EntityResult.SeenDirectly)
                        {
                            owner.Allegiance.Statistics.AddProductionEvent(foodItem.EntityType, Allegiances.Statistics.ProductionStatistics.StatTypes.EatenByCreatures, 1);
                        }
                        */

                        foodItem.Destroy();
                    }
                });
        }


        public override bool HandleMessage(Message message)
        {
            //first, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            //if the msg was not handled, test to see if this goal can handle it
            if (handled == false)
            {
                switch (message.MessageType)
                {
                    // someone wants us to stop eating this item:
                    case Message.MessageTypes.CancelJobOrItemInUse:
                    //case Message.MessageTypes.CancelJobForAIReset:

                        Status = Status.Failed;

                        return true; //msg handled

                    default: return false;
                }
            }
            else
            {
                return true;
            }
        }

        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion((Snapshotter.Version)2);  // Dec, 2016.  increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.itemToConsumeID = sn.DoEntityIDNullable(itemToConsumeID);
            this.itemToExtractFromID = sn.DoEntityIDNullable(itemToExtractFromID);
            this.placeToEat = sn.DoEntityIDNullable(placeToEat);
            this.extractedItemID = sn.DoEntityIDNullable(extractedItemID);

            this.currentState = sn.DoEnum(currentState);
            this.maxAmountToExtract = sn.DoFloatNullable(maxAmountToExtract);
            this.extractionProcessToUse = sn.DoGameData(extractionProcessToUse);
            this.itemIsDrunk = sn.DoBool(itemIsDrunk);
            this.extractionCompleteMethodID = sn.DoEnumNullable(extractionCompleteMethodID);
            this.ownerOfFoodItem = sn.DoEnumNullable(ownerOfFoodItem);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            if ((uint)version >= 2)
            {
                this.additionalItemsToConsume = sn.DoList(additionalItemsToConsume);  // #EATFIX
            }
           /* else
            {
                this.CreatedOn = The.Sim.TotalUnPausedGameTimeInSeconds;
            }*/


            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            LoadPostProcessRegisterMethodIDs();

        }

        public void LoadPostProcessRegisterMethodIDs()
        {
            if (extractionCompleteMethodID.HasValue)
            {
                ActionLookup<IKnownProcess>.Add(extractionCompleteMethodID.Value,
                    extractGoal_ProductionComplete);
            }
        }

        #endregion
    }
}
