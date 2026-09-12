using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Needs;
using GameStateManagement;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities.Body;
using UWGame.ClientSide.Log;
using UWGame.Control;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    public abstract class GoalEvaluator
    {
        public virtual float Priority
        {
            get
            {
                return 1f;
            }
        }

        public enum CalculateResult { Done, Processing }

        protected double bestScore = 0;

        protected double debugRating = 0;

        protected static float maxMapOctileDistance;

        public static double HighestTravelTimeOnFoot;
        public static double OneOverHighestTravelTimeOnFoot;

        protected const double IdleGoalDesirabilitySpread = 0.005;

        protected Entity entity;
        protected Intelligence entityIntelligence;

        /// <summary>
        /// can be null!
        /// </summary>
        protected Person personEntity;


        // protected BiologicalEntity biologicalEntity;

        public GoalEvaluator(Entity entity)
        {
            this.entity = entity;
            this.entityIntelligence = this.entity.Intelligence;
            this.personEntity = this.entity.PersonEntity; // can be null!
            //  this.biologicalEntity = entity.BiologicalEntity; // can be null!
        }

        public GoalEvaluator(Entity entity, float ageIntervalStart, float ageIntervalEnd, float maxAge)
            : this(entity) // AgeGroup activeInAgeGroup)
        {
            ActiveInAgeInterval = new Tuple<double, double>(ageIntervalStart, ageIntervalEnd);
            ageFalloff = Math.Min(0.1 * (ageIntervalEnd - ageIntervalStart), 0.02 * maxAge); // set a reasonable falloff

        }

        static GoalEvaluator()
        {
           // maxMapOctileDistance = Common.DistanceOctile(Point.Zero, new Point(The.Map.mapTileWidth, The.Map.mapTileHeight));
            maxMapOctileDistance = Common.DistanceOctile(Point.Zero, new Point(128, 128)); // NEW: use a constant size 

            // use this time amount to normalize a travel time to that of a person's walking speed across the whole map:
            HighestTravelTimeOnFoot = (MapManager.tileSize * maxMapOctileDistance) / GameData.Instance.AllEntityTypes["entity:human"].LocomotorType.LeggedLocomotorType.WalkNormalSpeed; //OverStandardTerrain; // PersonEntityType.Instance.BaseSpeedOverStandardTerrain;
            OneOverHighestTravelTimeOnFoot = 1.0 / HighestTravelTimeOnFoot;
        }

        //returns a score between 0 and 1 representing the desirability of the
        //strategy the concrete subclass represents
        public abstract CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result);


        public virtual void PreSetGoal()
        {

        }


        /// <summary>
        /// return true if a new top level goal has been set, false otherwise
        /// </summary>
        public virtual bool SetGoal()
        {
            entityIntelligence.Memory.ClearNeededItemsForNextGoal();

            return true;
        }

        //public abstract bool SetGoal();

        /// <summary>
        /// re-evaluate the found goal and make sure that is up to date and there are no higher scorers
        /// </summary>
        /// <returns></returns>
        public abstract bool CanTakeGoal();


        /// <summary>
        /// if needed, cancel the current takers of jobs, items or other resources - none of whom should have a higher score
        /// </summary>
        /// <returns></returns>
        public abstract bool CancelCurrentTakers();


        public virtual bool IsIdleActivity()
        {
            return false;
        }

        public bool IsActive = true;

        //public bool IsColonyWork = false;

        public Tuple<double, double> ActiveInTime;



        public Tuple<double, double> ActiveInAgeInterval;
        protected double ageFalloff;

        /// <summary>
        /// Provides greater incentive to finish a job than starting a new one.
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static double ScoreJobProgress(float progress)
        {
            return Math.Pow(progress, 2);
        }

        public static double ScoreNumberOfWorkers(int proposedNumber, int maxNumber)
        {
            return ProcessJob.GetMarginalLaborReturn(proposedNumber, maxNumber);
        }

        /// <summary>
        /// Scores skill using a quadratic function to give even higher weight to high skill levels.
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public static double ScoreSkill(float skill)
        {
            return Math.Pow(skill, 2);
        }

        /// <summary>
        /// return 1 if no other members possess the skill
        /// </summary>
        /// <param name="skillType"></param>
        /// <returns></returns>
        public static double ScoreUniqueSkill(Intelligence intelligence, /* Entity agent,*/ SkillType skillType)
        {
            if (intelligence.HasSkill(skillType)
                && intelligence.CurrentExpedition.SkillIsUnique(skillType)) // IterateMembers();
            {
                return 1d;
            }

            return 0d;
        }


        /// <summary>
        /// returns -1 if the agent possesses a unique skill
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public static double ScoreUniqueSkillPenalty(Entity agent)
        {
            if (agent.Intelligence.CurrentExpedition.HasUniqueSkill(agent))
            {
                return -1d;
            }

            return 0d;

        }

        /// <summary>
        /// when called from ArbitrateWhileBusy, we DON'T want the entity's current stance (which could be bold) - this would make the agent reckless in all other evaluators too...
        /// 
        /// if the agent's stance is cautious (because of injury) we want to use that, since the agent cannot change the stance to normal after switching!
        ///         
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static RegionMap GetRegionMapAndStanceForEvaluator(Entity entity, Job job, out ThreatStance threatStanceToUse, bool? requiresBoldStance = null)
        {
           
            bool boldStanceRequired = false;
            if (job != null)
            {
                boldStanceRequired = job.RequiresBoldStance;
            }
            else if (requiresBoldStance == true)
            {
                boldStanceRequired = true;
            }

            if (boldStanceRequired)
            {
                threatStanceToUse = ThreatStance.Bold;
            }
            else
            {
                // we DON'T want the entity's current stance (which could be bold)
                threatStanceToUse = MinimumThreatStance(entity.Intelligence.ThreatStance, ThreatStance.Normal); // normal or cautious                    
            }

            if (entity.EntityType.IntelligenceType.IsMobile)
            {
                return entity.Intelligence.Allegiance.SharedKnowledge.GetRegionMapToUseForEntity(entity, threatStanceToUse);
            }
            else return null;
        }


        private static ThreatStance MinimumThreatStance(ThreatStance stance1, ThreatStance stance2)
        {
            if ((int)stance1 < (int)stance2)
            {
                return stance1;
            }
            else return stance2;
        }


      
        public static void UpdateJobAccessibility(Entity entity, Point fromSubile, Point toSubtile, Job job, IHasEntityGroup ownerOfJob, RegionMap.Result moveResult)
        {
            if (job != null && ownerOfJob != null)
            {
                if (moveResult == RegionMap.Result.NoAccess)
                {
                    if (job != null && ownerOfJob != null)
                    {
                        // give client feedback on inaccessible job:
                        The.Client.SetJobInaccessible(job, ownerOfJob, true);

                        // test terrain accessiblity - skip if Wait:
                        bool? isBlockedByThreat = ComputeIsBlockedByThreat(entity, fromSubile, toSubtile);

                        // set the blocked by threat flag via deduction:
                        if (isBlockedByThreat.HasValue)
                        {
                            The.Client.SetJobBlockedByThreat(job, ownerOfJob, isBlockedByThreat.Value);
                        }
                    }
                }

                if (moveResult == RegionMap.Result.OK)
                {
                    The.Client.SetJobInaccessible(job, ownerOfJob, false);
                }
            }
        }

        public static bool? ComputeIsBlockedByThreat(Entity activeEntity, IKnownEntityData toEntity)
        {
            float distance = 0f;
            // we got here from a call from the same method - avoid running the feedback code again:
            RegionMap.Result terrainResult = The.Map.TerrainCosts[SurfaceType.TransportType.Foot].RegionMap.GetDistanceToEntity(activeEntity, activeEntity, toEntity, ref distance, null, null, false, giveClientFeedback: false);

            // return the blocked by threat status via deduction:
            if (terrainResult == RegionMap.Result.OK)
            {
                return true;
            }
            else if (terrainResult == RegionMap.Result.NoAccess)
            {
                return false;
            }

            return null;
        }

        public static bool? ComputeIsBlockedByThreat(Entity entity, Point fromSubile, Point toSubtile)
        {
            //TODO: Add assert about the region map transportation if it isnt by foot
            float terrainDistance = 0f;
            RegionMap.Result terrainResult = The.Map.TerrainCosts[SurfaceType.TransportType.Foot].RegionMap.GetDistance(entity, fromSubile, toSubtile, ref terrainDistance, false);

            // return the blocked by threat status via deduction:
            if (terrainResult == RegionMap.Result.OK)
            {
                return true;
            }
            else if (terrainResult == RegionMap.Result.NoAccess)
            {
                return false;
            }

            return null;

        }


        /// <summary>
        /// WHY call this with entity.Location instead of entity.AccessPoint??? Will always be blocked in buildings and trigger a costly brute-force search for the closest region!
        /// 
        /// Called from EvaluateJob (Construction, production, smoke bomb...) and EvaluateScoutingJob (scouting/foraging) let's call this from other evaluators too
        /// Warning: if getting distance to an entity (item, building...) - don't use this! It does not take containers into account.
        /// 
        /// Note: Region map should be the correct one - vehicle or foot!
        /// 
        /// Note: from/to sequence matters! There is logic in GetDistance to allow the entity standing at the From location to escape from a blocked area... meaning that if From is blocked,
        /// the result will be OK!
        /// 
        /// job is only needed to provide client feedback about accessibility.
        /// </summary>
        /// <param name="regionMap"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="entity"></param>
        /// <param name="travelTimeScore"></param>
        /// <returns></returns>
        public static RegionMap.Result ScoreTravelTime(RegionMap regionMap, ThreatStance threatStanceToUse, Vector3 from, Vector3 to, Entity entity, ref double travelTimeScore, Job job = null, IHasEntityGroup ownerOfJob = null)
        {

            Point fromSubtilePos = MapManager.WorldPosToSubtile(from);
            Point destinationSubtilePos = MapManager.WorldPosToSubtile(to);

            float distance = -1f;

            // NEW: I added this test as a precaution, to catch the case where the agent is standing on top of the work site, but it is in a danger zone:
            if (!WorkSiteIsSafe(entity, to, threatStanceToUse, false))
            {
                if (job != null && ownerOfJob != null)
                {
                    //TODO: Confirm that this check should be placed here as it is done in another place in this class already.
                    // give client feedback on inaccessible job:
                    The.Client.SetJobInaccessible(job, ownerOfJob, true);
                    //job.SetIsInaccessible(ownerOfJob, true);
                }
                return RegionMap.Result.NoAccess;
            }


           // RegionMap.Result result1 = GetDistanceAndSetJobAccessibility(regionMap, entity, fromSubtilePos, destinationSubtilePos, ref distance, job, ownerOfJob);

            RegionMap.Result result1 = regionMap.GetDistance(entity, fromSubtilePos, destinationSubtilePos, ref distance);
            UpdateJobAccessibility(entity, fromSubtilePos, destinationSubtilePos, job, ownerOfJob, result1);


            if (result1 != RegionMap.Result.OK)
            {
                return result1;
            }
            else
            {
                // got the distance. Compute the score:
                travelTimeScore = GetTravelScoreFromDistance(entity, distance);

                return RegionMap.Result.OK;
            }
        }



        public static double GetTravelScoreFromDistance(Entity entity, float distance)
        {
            double timeCost;

            timeCost = GetEvaluatorTimeCostOfDistance(entity, distance);

            return ScoreTravelTime(timeCost);
        }

        /// <summary>
        /// use this to correctly handle containers on both ends of 2 entities
        /// </summary>
        /// <param name="regionMap"></param>
        /// <param name="entity"></param>
        /// <param name="targetEntityData"></param>
        /// <param name="travelTimeScore"></param>
        /// <returns></returns>
        public static RegionMap.Result ScoreTravelTime(RegionMap regionMap, Entity entity, IKnownEntityData targetEntityData, ref double travelTimeScore)
        {
            float distance = -1f;

            return ScoreTravelTime(regionMap, entity, targetEntityData, ref travelTimeScore, ref distance);
        }

        /// <summary>
        /// use this to correctly handle containers on both ends of 2 entities
        /// 
        /// TODO: pass Allegiance to allow access to carried items???
        /// </summary>
        /// <param name="regionMap"></param>
        /// <param name="entity"></param>
        /// <param name="targetEntityData"></param>
        /// <param name="travelTimeScore"></param>
        /// <returns></returns>
        public static RegionMap.Result ScoreTravelTime(RegionMap regionMap, Entity entity, IKnownEntityData targetEntityData, ref double travelTimeScore, ref float distance)
        {
            distance = -1f;

            RegionMap.Result result1 = regionMap.GetDistanceToEntity(entity, entity, targetEntityData, ref distance);

            if (result1 != RegionMap.Result.OK)
            {
                return result1;
            }
            else
            {
                // got the distance. Compute the score:
                travelTimeScore = GetTravelScoreFromDistance(entity, distance);

                return RegionMap.Result.OK;
            }
        }


        /// <summary>
        /// does not consider burden or terrain to prevent scoring changes that lead to job switching after starting the job/goal
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static double GetEvaluatorTimeCostOfDistance(Entity entity, float distance)
        {
            double timeCost;
            Entity vehicleEntity;
            if (!entity.GetDrivenVehicle(out vehicleEntity))
            {
                // handle vehicle blown up...
                return 100d; //!?
            }

            if (vehicleEntity != null)
            {

                // TODO: For cars, roads matter  a lot. Look into estimating road connections between sectors...
                timeCost = ((double)distance / vehicleEntity.Locomotor.CurrentMaximumSpeedForEvaluator) * // CurrentMaximumSpeedNoTerrain) *
                    PlainsType.Instance.MovementFactor(((VehicleContainerType)vehicleEntity.EntityType.ContainerType).Transport, SurfaceType.TerrainFeatures.None);
            }
            else
            {
                timeCost = ((double)distance / entity.Locomotor.CurrentMaximumSpeedForEvaluator) * //.CurrentMaximumSpeedNoTerrain) *
                    PlainsType.Instance.MovementFactor(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.None);
            }
            return timeCost;
        }

        public struct WeightedRating
        {
            private double rating;
            private double weightTotal;
            public double Result
            {
                get
                {
                    if (!Common.IsEqual(weightTotal, 1.0))
                        throw new Exception("WeightedRating weights do not total 1.0 at Result time.");

                    return rating;
                }
            }
            public void AddScore(double weight, double score)
            {
                rating += weight * score;
                weightTotal += weight;
            }
        }

        /*  protected static double ScoreTravelTime(Vector3 from, Vector3 to, Entity entity) 
          {
              float distance = Common.DistanceOctile(from, to);

              // entity.EntityType.BaseSpeed;
              double timeCost;
              if (entity.DrivingVehicle != null)
              {
                  // TODO: For cars, roads matter  a lot. Look into estimating road connections between sectors...
                  timeCost = ((double)distance / entity.DrivingVehicle.Locomotor.CurrentMaximumSpeed) *
                      PlainsType.Instance.MovementFactor(entity.DrivingVehicle.EntityType.VehicleType.Transport, TerrainType.TerrainFeatures.None);
              }
              else
              {
                  timeCost = ((double)distance / entity.Locomotor.CurrentMaximumSpeed) *
                      PlainsType.Instance.MovementFactor(TerrainType.TransportType.Foot, TerrainType.TerrainFeatures.None);
              }


              return ScoreTravelTime(timeCost, HighestTravelTimeOnFoot);
          }*/

        /*  public static double ScoreTravelTime(double time, double highestTravelTimeToUse)
          {
              double travelTimeScore = Common.Clamp(1.0 - (time / highestTravelTimeToUse), 0.0, 1.0);
              // square the score, to achieve faster falloff from the highest score (1):
              travelTimeScore = Math.Pow(travelTimeScore, 2.0);

              return travelTimeScore;
          }*/

        /// <summary>
        /// Does this cause issues when the entityGroup is the SharedKnowledge collection itself? Too early to delete entities?
        /// 
        /// removes the item from the owner list if it is known to be destroyed or if status is 'unknown'.
        /// returns false if the evaluator should skip the entity.
        /// 
        /// Remember to use the (reverse) for loop instead of foreach to prevent an exception being thrown.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="entityGroup"></param>
        public static bool HandleOwnerDataResult(SharedKnowledge sharedKnowledge, EntityID entityID, EntityGroup entityGroup, out IKnownEntityData entityData, EntityType entityType = null)
        {
            EntityResult result = sharedKnowledge.GetKnownData(entityID, out entityData);

            if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown) // consider unknown entity locations as destroyed.             
            {
                if (entityGroup != null)
                {
                    entityGroup.DeleteEntity(entityID, entityType);
                }

                return false;
            }
            else if (result == EntityResult.NewUnknownEntity) // NEW! skip unknown owned items too.
            {
                entityData = null;
                return false;
            }


            return true;
        }

        /// <summary>
        /// this overload won't crash when iterating dictionaries and such. The invalid items are added to a list, to be deleted after looping.
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        /// <param name="entityID"></param>
        /// <param name="entityGroup"></param>
        /// <param name="entityData"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static bool HandleOwnerDataResult(SharedKnowledge sharedKnowledge, EntityID entityID, EntityGroup entityGroup, out IKnownEntityData entityData, ref List<EntityID> invalidEntities,  EntityType entityType = null)
        {
            EntityResult result = sharedKnowledge.GetKnownData(entityID, out entityData);

            if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown) // consider unknown entity locations as destroyed.             
            {
                Common.AddToList(ref invalidEntities, entityID);
                return false;
            }
            else if (result == EntityResult.NewUnknownEntity) // NEW! skip unknown owned items too.
            {
                entityData = null;
                return false;
            }
            
            return true;
        }


        /// <summary>
        /// the purpose of this method is to handle invalid jobs encountered during evaluators in a uniform way.
        /// I think we should cancel and remove such jobs... we may also want to inform the player in some cases. Particularly for building jobs,
        /// but not for production, since that is more indirect - the orders are unchanged and the job can be created again automatically.
        /// </summary>
        /// <param name="job"></param>
        public static void HandleInvalidJob(Job job)
        {
            job.Destroy(true);
        }

        public static bool EntityDataResultCausesSkip(EntityResult result)
        {
            if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown // consider unknown entity locations as destroyed.
                || result == EntityResult.NewUnknownEntity) // NEW!
            {
                return true;
            }

            return false;
        }

        public static bool ProcessDataResultCausesSkip(ProcessResult result)
        {
            if (result == ProcessResult.Destroyed) 
            {
                return true;
            }

            return false;
        }

        protected void GetReplenishItemsToCancel(Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> replenishItemsForTools,
            ref List<Entity> needsToBeCancelled, ref List<Entity> itemsToBeDropped) // ToolInstanceCombo combo)
        {
            if (replenishItemsForTools != null)
            {
                foreach (var tool in replenishItemsForTools)
                {
                    foreach (var replenishAction in tool.Value)
                    {
                        foreach (var item in replenishAction.Item2)
                        {
                            GetItemUsersToCancel(item, ref needsToBeCancelled, ref itemsToBeDropped, false); //, false);
                        }
                    }

                }
            }

            // remove duplicates:
            needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
            itemsToBeDropped = itemsToBeDropped.Distinct().ToList();
        }


        protected CalculateResult FindReplenishItemsForToolOrWeapon(Entity entity, EntityGroup ownerOfItems, List<EntityGroup> listOfOwnersOfItems,
            ref Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> allFoundReplenishItems,
            double ourScore, SharedKnowledge sharedKnowledge, RegionMap footRegionMap,
            IKnownEntityData entityToReplenish,
            Dictionary<EntityType, List<ItemDistance>> energyItemsSortedByDistanceToEntity,
            out bool success,
            EntityType neededAmmoType = null, int? neededAmmoRounds = null,
            float? neededElectricalEnergy = null,
            float? neededFuelBulk = null)
        {
            bool hasEnoughFuel = true, hasEnoughPower = true, hasEnoughAmmo = true;
            //RequiresEnergy energy;

            success = false;


            //entityToReplenish.Find(out energy);
            if (neededFuelBulk.HasValue)
            {
                 hasEnoughFuel = entityToReplenish.HasEnoughFuel(neededFuelBulk.Value); // energy.RequiresFuel.HasEnoughFuel(neededFuelBulk.Value);

            }

            if (neededElectricalEnergy.HasValue)
            {
                throw new NotImplementedException("Power for tools");
                //hasEnoughPower = energy.RequiresFuel.HasEnoughFuel(neededFuelBulk.Value);
            }

            if (neededAmmoRounds.HasValue)
            {
                // Ammunition ammo;
                //  entityToReplenish.Find(out ammo);
                hasEnoughAmmo = entityToReplenish.HasEnoughAmmo(neededAmmoType, neededAmmoRounds.Value);
                // hasEnoughEnergy = energy.HasEnergyForDuration(jobDuration);
            }

            if (hasEnoughFuel && hasEnoughPower && hasEnoughAmmo)
            {
                // the tool already has enough energy - replenishment is not needed
                success = true;
                return CalculateResult.Done;
            }


            if (entity.EntityType.IntelligenceType.CanReplenish != true)
            {
                success = false; // we cannot replenish ourselves. we need someone to do it...
                return CalculateResult.Done;
            }

            float maxDistance = GameData.Instance.AIConstants.MaxDistanceForReplenishItems;



            //   List<Entity> energyItemTakersNeedToBeCancelled = null;
            //   List<Entity> energyItemsNeedToBeDropped = null;

            //   int? noOfAmmoItems;

            if (!hasEnoughFuel && footRegionMap != null) // non-mobiles can't replenish...
            {

                RequiresFuelType requiresFuelType = entityToReplenish.EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType;

                // float neededFuelBulk = (float)(requiresFuelType.BurnRatePerDay * jobDuration * DateAndTime.DaysPerSeconds);

                float? availableFuelBulk = 0f;

                List<IKnownEntityData> foundReplenishItems = new List<IKnownEntityData>();

                // GoalReplenish.ReplenishAction action = GoalReplenish.ReplenishAction.Refuel;

                foreach (var fuelEntityType in requiresFuelType.FuelEntityTypes)
                {
                    ProcessType action = entityToReplenish.EntityType.ContainerType.GetReplenishProcesses()[fuelEntityType];

                    if (FindReplenishItemsForToolAndAction(entity, ownerOfItems, listOfOwnersOfItems,
                            ref allFoundReplenishItems,
                            ourScore, sharedKnowledge, footRegionMap,
                            entityToReplenish, action, fuelEntityType,
                            energyItemsSortedByDistanceToEntity, ref foundReplenishItems,
                            ref availableFuelBulk, neededFuelBulk,
                            null,
                            out success) == CalculateResult.Processing)
                    {
                        return CalculateResult.Processing;
                    }

                    if (success == true)
                    {
                        break;
                    }

                }
                if (success == false)
                {
                    return CalculateResult.Done;
                }
            }

            if (!hasEnoughAmmo && footRegionMap != null) // non-mobiles can't replenish...
            {
                float? availableFuelBulk = null;

                List<IKnownEntityData> foundReplenishItems = new List<IKnownEntityData>();
                ProcessType action = entityToReplenish.EntityType.ContainerType.GetReplenishProcesses()[neededAmmoType];

                if (FindReplenishItemsForToolAndAction(entity, ownerOfItems, listOfOwnersOfItems,
                                ref allFoundReplenishItems, // <-- the result
                                ourScore, sharedKnowledge, footRegionMap,
                                entityToReplenish, action, neededAmmoType,
                                energyItemsSortedByDistanceToEntity, ref foundReplenishItems,
                                ref availableFuelBulk, null,
                                neededAmmoRounds,
                                out success) == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }

                if (success == false)
                {
                    return CalculateResult.Done;
                }

            }

            //  success = false;
            return CalculateResult.Done;

        }

        public bool IsItemAlreadyAdded(Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> replenishItemsForTools, /*ToolInstanceCombo combo*/ ItemDistance item)
        {
            if (replenishItemsForTools != null)
            {
                foreach (var toolReplenishItems in replenishItemsForTools)
                {

                    foreach (var replenishAction in toolReplenishItems.Value)
                    {
                        foreach (var itemAlreadyAdded in replenishAction.Item2)
                        {
                            if (itemAlreadyAdded == item.Entity)
                            {
                                // was already taken by us. go to the next item
                                return true;
                            }
                        }
                    }

                }
            }

            return false;
        }

        protected static bool EntityResultCausesFailedGoal(EntityResult result)
        {
            if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown)
            {
                return true;
            }
            else return false;
        }

        /*  protected static bool IsReplenishItemOk(EntityID entityID, Intelligence entityIntelligence)
          {
              // lookup in sharedknowledge - handle null result with call to cleanup method - but without crashing foreach!!! use established patterns/methods to prevent bugs
              IKnownEntityData data;

              if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entityID, out data)))
              {
                  return false;
              }

              EntityGroup group = entityIntelligence.Allegiance.SharedKnowledge.AllKnownEntities;

              if (GoalEvaluator.ScoreIsEntityFunctional(data) <= 0d // test for broken parts 
                  && data.OwnedBy == null // test not discarded...
                  && data.CanBeHauled()
                  && data.IsUnassigned(group)) 
              {
                  return true;

              }

              return false;             
          }*/

        protected static bool IsReplenishItemOk(IKnownEntityData data, Intelligence entityIntelligence, EntityGroup owner, EntityType entityType)
        {
                  
            if (data.EntityType == entityType
                && owner.Contains(data) //Do we own the ammunition, else we cannot use it)
                && data.CanBeHauled()    
                && Entity.IsFunctional(data)) // GoalEvaluator.ScoreIsEntityFunctional(data) > 0d) // test for broken parts 
            {
                if (!data.IsUnassigned(entityIntelligence.Allegiance.SharedKnowledge))          
                { 
                    // allow items assigned for hauling if not taken yet... the agent may even be carrying this ammo.
                    if (IsAssignedToUntakenHaulingJob(data))
                    {           
                        // it would be best if we could destroy this job when taking the goal...
                        return true;                           
                    }

                    return false;
                }

                return true;
            }

            return false;
        }


        public static bool IsAssignedToUntakenHaulingJob(IKnownEntityData data)
        {
            if (data.AssignedToJob.HasValue)
            {
                // allow items assigned for hauling if not taken yet... the agent may even be carrying this ammo.
                Job job = LookUp<Job, JobID>.FindByID(data.AssignedToJob);
                HaulingJob haulingJob = job as HaulingJob;
                if (haulingJob != null && haulingJob.Item == data.EntityID
                    && haulingJob.TakenBy.Count == 0)
                {
                    return true;
                }

            }

            return false;

        }

        public static CalculateResult GetAllReplenishItemsSortedByDistance(Entity entity, EntityType entityType, EntityGroup owner,
            SharedKnowledge sharedKnowledge, RegionMap footRegionMap, ref List<ItemDistance> sortedList, float? maxDistanceToWalk, Vector2 positionToSearchFrom, float rangeToSearchIn) //List<Entity> sortedList)
        {

            List<Pair<EntityID, Vector2>> items = new List<Pair<EntityID, Vector2>>();
            sharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetEntitiesInRange(positionToSearchFrom,
                                                                     rangeToSearchIn,
                                                                     null,
                                                                     ref items);

            List<EntityID> listOfItems = new List<EntityID>();
            foreach (var item in items)
            {
                listOfItems.Add(item.First);
            }

            return GetSortedListOfEntities(entity, owner, sharedKnowledge, footRegionMap, listOfItems, ref sortedList,
                     itemData => IsReplenishItemOk(itemData, entity.Intelligence, owner, entityType),
                     maxDistanceToWalk);

        }



        //private CalculateResult GetAllReplenishItemsSortedByDistance(Entity entity, EntityType entityType, Owner owner,
        //   SharedKnowledge sharedKnowledge, RegionMap footRegionMap, List<ItemDistance> sortedList, float? maxDistance = null) //List<Entity> sortedList)
        //{
        //    List<EntityID> items;
        //    if (owner.InternalOwner.OwnerContent.Items.TryGetValue(entityType, out items)) // TODO: change it so the there is no lookup in Owner
        //    {
        //        return GetSortedListOfEntities(entity, owner, sharedKnowledge, footRegionMap, items, sortedList, 
        //            itemData => itemData.Replenishes == null // itemComponent.Replenishes == null
        //                && itemData.IsCompleted() // TODO: add a test for ownership when we change to use the quad tree instead
        //                && ScoreIsEntityFunctional(itemData) > 0d,
        //            maxDistance);                
        //    }

        //    return CalculateResult.Done;
        //}

        /// <summary>
        /// perhaps we can use the quad trees set up for collision detection to speed up this search...
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ownerOfEntitiesToSort"></param>
        /// <param name="sharedKnowledge"></param>
        /// <param name="footRegionMap"></param>
        /// <param name="items"></param>
        /// <param name="distanceList"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static CalculateResult GetSortedListOfEntities(Entity entity, EntityGroup ownerOfEntitiesToSort, SharedKnowledge sharedKnowledge,
            RegionMap footRegionMap, List<EntityID> items, ref List<ItemDistance> distanceList, Predicate<IKnownEntityData> entityPredicate, float? maxDistance = null)
        {
            IKnownEntityData itemData;
            EntityID item;

            // TODO: instead of passing a list of items as a parameter, pass a predicate/filter function. Then look up items in the KnownEntities quad tree

            for (int i = items.Count - 1; i >= 0; i--)
            {
                item = items[i];

                if (!HandleOwnerDataResult(sharedKnowledge, item, ownerOfEntitiesToSort, out itemData))
                {
                    continue;
                }

                if (entityPredicate(itemData))
                {
                    // pixels
                    float distanceOnFoot = -1f;
                    //RegionMap.Result result = footRegionMap.GetDistance(entity, entitySubtilePos, destinationSubtilePos, ref distanceOnFoot);
                    RegionMap.Result result = footRegionMap.GetDistanceToEntity(entity, entity, itemData, ref distanceOnFoot);

                    if (result == RegionMap.Result.Wait)
                    {
                        return CalculateResult.Processing;
                    }
                    else if (result == RegionMap.Result.NoAccess)
                    {
                        continue;
                    }
                    else
                    {
                        // exclude items too far away...
                        if (maxDistance == null || distanceOnFoot < maxDistance.Value)
                        {
                            distanceList.Add(new ItemDistance() { Entity = itemData, Distance = distanceOnFoot });
                        }
                    }
                }

            }

            distanceList = distanceList.OrderBy(d => d.Distance).ToList();

            return CalculateResult.Done;

        }




        private CalculateResult GetAllEntitiesSortedByDistance(Entity entity, EntityType entityType, List<EntityGroup> listOfOwners,
            SharedKnowledge sharedKnowledge, RegionMap footRegionMap, ref List<ItemDistance> sortedList, float maxDistance)
        {
            foreach (var owner in listOfOwners)
            {
                if (GetAllReplenishItemsSortedByDistance(entity, entityType, owner, sharedKnowledge, footRegionMap, ref sortedList, maxDistance, entity.PlaySiteLocation.ToVector2(), maxDistance) == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }

            }

            return CalculateResult.Done;

        }

        private CalculateResult FindReplenishItemsForToolAndAction(Entity entity, EntityGroup groupOfItems, List<EntityGroup> listOfGroupsOfItems,
            ref Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> allFoundReplenishItems, // <-- the result
            double ourScore, SharedKnowledge sharedKnowledge, RegionMap footRegionMap,
            IKnownEntityData entityToReplenish, ProcessType action, EntityType replenishEntityType,
            Dictionary<EntityType, List<ItemDistance>> energyItemsSortedByDistanceToEntity, ref List<IKnownEntityData> foundReplenishItems,
            ref float? availableFuelBulk, float? neededFuelBulk,
            int? neededAmmoRounds,
            out bool success)
        {
            // sort the items by distance, excluding the ones far away...
            List<ItemDistance> distanceSortedList;

            success = false;

            if (!energyItemsSortedByDistanceToEntity.TryGetValue(replenishEntityType, out distanceSortedList))
            {
                float maxDistance = GameData.Instance.AIConstants.MaxDistanceForReplenishItems;

                distanceSortedList = new List<ItemDistance>();
                CalculateResult result;

                if (groupOfItems != null)
                {
                    result = GetAllReplenishItemsSortedByDistance(entity, replenishEntityType, groupOfItems, sharedKnowledge, footRegionMap, ref distanceSortedList, maxDistance, entity.Location.Value.ToVector2(), maxDistance);
                }
                else
                {
                    result = GetAllEntitiesSortedByDistance(entity, replenishEntityType, listOfGroupsOfItems, sharedKnowledge, footRegionMap, ref distanceSortedList, maxDistance);
                }

                if (result == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }

                energyItemsSortedByDistanceToEntity.Add(replenishEntityType, distanceSortedList);
            }

            // we have the items sorted by distance. start creating the set of valid items:

            bool targetReached = false;

            List<Entity> itemTakersNeedToBeCancelled = null;
            List<Entity> itemsNeedToBeDropped = null;

            int totalRounds = 0;
            foreach (var item in distanceSortedList)
            {
                //first see that we did not take this item already:
                if (IsItemAlreadyAdded(allFoundReplenishItems, item))
                {
                    continue;
                }

                // now check our score compared to anyone else currently using the item:
                GetItemUsersToCancel(item.Entity,
                    ref itemTakersNeedToBeCancelled, ref itemsNeedToBeDropped, true); //, true);

                if (IsScoreBetterThanAllInvolveds(ourScore, itemTakersNeedToBeCancelled))
                {
                    // good - we found a useable item:
                    /*  if (foundReplenishItems == null)
                      {
                          foundReplenishItems = new List<Entity>();
                      }*/

                    if (item.Distance > 20f)
                    {

                    }

                    foundReplenishItems.Add(item.Entity);
                    if (action.ReplenishAction == GoalReplenish.ReplenishAction.Refuel)
                    {
                        availableFuelBulk += item.Entity.Bulk;

                        if (availableFuelBulk >= neededFuelBulk)
                        {
                            targetReached = true;
                        }
                    }
                    else if (action.ReplenishAction == GoalReplenish.ReplenishAction.Reload)
                    {
                        totalRounds += item.Entity.NoOfRounds.Value;
                        if (totalRounds >= neededAmmoRounds)
                        {
                            targetReached = true;
                        }

                    }


                    if (targetReached)
                    {
                        // we have found all the items we need. store them:
                        if (allFoundReplenishItems == null)
                        {
                            allFoundReplenishItems = new Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>>(); // new Dictionary<Entity, List<Entity>>();
                        }

                        allFoundReplenishItems[entityToReplenish] = new List<Tuple<ProcessType, List<IKnownEntityData>>>();
                        allFoundReplenishItems[entityToReplenish].Add(new Tuple<ProcessType, List<IKnownEntityData>>(action, foundReplenishItems));

                        //done. handle next tool...
                        success = true;
                        return CalculateResult.Done;
                    }
                }
            }
            return CalculateResult.Done;
        }

        protected List<EntityGroupID> GetOwnerIDs(List<EntityGroup> owners)
        {
            if (owners != null)
            {
                return owners.Select(o => o.ID).ToList();
            }

            return null;
        }

        /// <summary>
        /// returns null if the parameter is null, else returns its ID
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static EntityGroupID? GetOwnerID(EntityGroup owner)
        {
            return owner != null ? owner.ID : (EntityGroupID?)null;
        }



        /// <summary>
        /// the entities in the needsToBeCancelled list must be sent a message to cancel their job.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="needsToBeCancelled"></param>
        /// <param name="itemsToBeDropped"></param>
        /// <param name="clearLists"></param>
        /// <param name="removeDuplicates"></param>
        protected void GetItemUsersToCancel(IKnownEntityData item, ref List<Entity> needsToBeCancelled, ref List<Entity> itemsToBeDropped, bool clearLists) //, bool removeDuplicates)
        {

            if (clearLists)
            {
                if (needsToBeCancelled == null)
                {
                    needsToBeCancelled = new List<Entity>();
                }
                needsToBeCancelled.Clear();

                if (itemsToBeDropped == null)
                {
                    itemsToBeDropped = new List<Entity>();
                }
                itemsToBeDropped.Clear();
            }

            // handle items in use, but have no job assigned - like food items..
            GetItemUser(item, needsToBeCancelled);

            Job assignedToJob = EvaluateJob.ResolveAssignedToJob(item);

            if (assignedToJob != null)
            {             
                // NOTE: unattended jobs are not taken by any agents, but their tools are still in use! we should skip them.

                if (assignedToJob.TakenBy.Count > 0)
                {                       
                    if (assignedToJob.TakenBy.Count == 1)
                    { 
                        // System.Diagnostics.Debug.Assert(entity != item.AssignedToJob.TakenBy.Get(0), "Cannot cancel self.");

                        needsToBeCancelled.Add(assignedToJob.TakenBy.Get(0)); 
                    }
                    else 
                    {
                        // CombatArea jobs

                        // get the entity using the item with inUseBy - gets set together with AssignedToJob
                        // will already have been added... is there other ways?
                        // the entity may not be carrying it yet...
                        EntityID? inUseBy =  entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(item.EntityID);
                        if (inUseBy.HasValue)
                        {
                            Entity agentNeedsToBeCancelled = assignedToJob.TakenBy.Get(e => e.ID == inUseBy);
                            if (agentNeedsToBeCancelled != null)
                            {
                                needsToBeCancelled.Add(agentNeedsToBeCancelled);
                            }
                        }
                    }
                }
            }

            Entity itemEntity = item as Entity;
            if (itemEntity != null) // will be non-null if carried by someone in our allegiance
            {

                Entity carrier;
                // if carried by an agent in our allegiance, tell them to drop:
                if (itemEntity.CarriedByAgent(out carrier)
                    && carrier != null
                    && carrier != entity)
                {
                    //  System.Diagnostics.Debug.Assert(itemEntity.AssignedToJob == null, "drop without cancel, bad.");

                    // force drop the item.
                    // case 1. the item is being used - The message to drop the item should be preceded by a cancel job message.
                    // case 2. the item is not curently used, the agent is just carrying it around. It is sufficient to send the Drop item message.
                    itemsToBeDropped.Add(itemEntity);
                }
            }

                       
            // remove duplicates:
            needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
            itemsToBeDropped = itemsToBeDropped.Distinct().ToList();
            
        }

        protected void GetItemUser(IKnownEntityData itemData, List<Entity> listOfUsers) // out Entity user)
        {
            // EntityGroup group = entity.Intelligence.Allegiance.SharedKnowledge.AllKnownEntities;


            EntityID? userID = entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(itemData.EntityID);
            /*  if () // itemData.GetInUseBy(group) != null)
              {
                  EntityID? userID = itemData.GetInUseBy(group);*/
            if (userID.HasValue)
            {
                Entity user = Entity.FindByID(userID);
                if (user != null)
                {
                    if (itemData.EntityID == (EntityID)18)
                    {

                    }
                    //   System.Diagnostics.Debug.Assert(entity != user, "Cannot cancel self.");

                    listOfUsers.Add(user);

                }
                else
                {
                    // clean up invalid entity
                    entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(itemData.EntityID, userID.Value);
                    //itemData.ClearInUseBy(group, userID.Value);
                }
            }
            // }
        }

        /// <summary>
        /// 0 - 1
        /// </summary>
        /// <param name="time"></param>
        /// <param name="oneOverHighestTravelTimeToUse"></param>
        /// <returns></returns>
        public static double ScoreTravelTime(double time, double oneOverHighestTravelTimeToUse)
        {
            double travelTimeScore = Common.Clamp(1.0 - (time * oneOverHighestTravelTimeToUse), 0.0, 1.0);

            // square the score, to achieve faster falloff from the highest score (1):
            //   travelTimeScore = Math.Pow(travelTimeScore, 2.0);
            travelTimeScore = Math.Pow(travelTimeScore, 2.0);

            return travelTimeScore;
        }

        /// <summary>
        /// 0 - 1
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double ScoreTravelTime(double time)
        {
            return ScoreTravelTime(time, OneOverHighestTravelTimeOnFoot);

            /*    double travelTimeScore = Common.Clamp(1.0 - (time * OneOverHighestTravelTimeOnFoot), 0.0, 1.0);
                // square the score, to achieve faster falloff from the highest score (1):
                travelTimeScore = Math.Pow(travelTimeScore, 2.0);

                return travelTimeScore;*/
        }


        public static bool IsOnPlaySite(IKnownEntityData entityData)
        {
            return entityData.Location.HasValue;
        }


        public static bool IsValidPlaysiteItem(Entity entity, IKnownEntityData food, bool mustCarryItem)
        {

            return (food.NotOnboardDrivenVehicle // ???
                  && IsOnPlaySite(food)
                  && (mustCarryItem == false || //holdsFoodWhenEating == false || // must be able to carry food - only for some creatures!
                    (entity.AgentStorage != null && entity.AgentStorage.ItemStorage != null && entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(food.Bulk))))
                  && Entity.IsFunctional(food) // ScoreIsEntityFunctional(food) > 0d
                  && food.IsCompleted();

        }

      /*  public static bool EntityIsFunctional(IKnownEntityData entity)
        {
            return Common.IsGreaterThan(ScoreIsEntityFunctional(entity), 0d); 
        }*/

        /// <summary>
        /// 0 = broken and unusable
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static double ScoreIsEntityFunctional(IKnownEntityData entity)
        {           
            if (Entity.IsFunctional(entity))
            {
                return 1d;
            }
            else
            {
                return 0d;
            }

            /*

            if (entity.FunctionalScore.HasValue)
            {
                return entity.FunctionalScore.Value;
              
            }
            else
            {
                if (entity.PartIsBroken)
                {
                    return 0d; // unusable
                }
            }

            return 1d;*/

        }

        /// <summary>
        /// we don't crave activity at night...
        /// this should make it possible to perform leisure activities and household work if there is a long idle period during work.
        /// </summary>
        /// <param name="intelligence"></param>
        /// <returns></returns>
        /*  protected double? ScoreNonIdleActivity(Intelligence intelligence)
          {
              // include "tiredness" / energy?
              Need need;
              if (intelligence.Needs.NeedsList.TryGetValue(NeedClass.Activity, out need))
              {

                  if (The.Sim.DateAndTime.CurrentPhase == UWGame.SimSide.DayPhases.Work)
                  {

                      if (need.CurrentLevel > 0.2) // this limit should ensure that the townies have a higher tolerance for idleness during the day, 
                      //and will wait around a bit for work.
                      {
                          return need.CurrentLevel;
                      }
                  }
                  else if (The.Sim.DateAndTime.CurrentPhase == UWGame.SimSide.DayPhases.Leisure)
                  {
                      return need.CurrentLevel;
                  }
              }

              return 0;

          }*/

        /// <summary>
        /// it should be possible to perform leisure activities and household work if there is a long idle period during work.
        /// </summary>
        /// <returns></returns>
        /*  protected double? ScoreLeisureActivity(Intelligence intelligence)
          {
              if (The.Sim.DateAndTime.CurrentPhase == UWGame.SimSide.DayPhases.Work)
              {
                  return intelligence.Needs.NeedForActivity.CurrentLevel;
              }
              else if (The.Sim.DateAndTime.CurrentPhase == UWGame.SimSide.DayPhases.Leisure)
              {
                  return 1;
              }
              else return 0;

          }*/


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entitiesToCancel">a list of agents in our allegiance</param>
        /// <param name="itemsToBeDropped"></param>
        /// <returns></returns>
        protected bool CancelEntities(List<Entity> entitiesToCancel, List<Entity> itemsToBeDropped)
        {
            // Cancel Stuff!!! Let there be chaos!

            //cancel agents. Not only jobs, also InUse items, assigned in GoalEat!
            if (entitiesToCancel.Count > 0)
            {
                Entity cancelThis;
                do
                {
                    cancelThis = entitiesToCancel[0];

                    // if an agent is arbitrating while busy, he may have found a better job for his currently assigned tool. So it is permitted to use his currently locked items in evaluating.
                    // however, it should never be necessary to cancel himself. The current goal will release all locks before the new goal is set as top goal.
                    if (cancelThis != entity)
                    {

                       
                        // removes from job.TakenBy:
                        // When do we need his vehicle also?
                        if (!cancelThis.SendMessage(new Message(entity, Message.MessageTypes.CancelJobOrItemInUse, Message.CancelJobKeepVehicle.LeaveVehicle)))
                        {
                            return false;
                        }
                    }

                    entitiesToCancel.RemoveAll((a) => a == cancelThis);
                }
                while (entitiesToCancel.Count > 0);
            }


            if (itemsToBeDropped.Count > 0)
            {
                IKnownEntityData dropThis;
                do
                {
                    dropThis = itemsToBeDropped[0];

                    // Item itemComponent = dropThis.Item;

                    // is the item being carried by an agent from our allegiance? then it is directly seen!
                    Entity carriedItem = dropThis as Entity;
                    if (carriedItem != null)
                    {

                        // disabled this assert because movie. review later.
                      //  System.Diagnostics.Debug.Assert(carriedItem.AssignedToJob == null || carriedItem.AssignedToJob.TakenBy.Count == 0, "drop without cancel, bad."); // allow haul jobs that are not taken...


                        Entity carrier;
                        if (!carriedItem.CarriedByAgent(out carrier))
                        {
                            return false;
                        }

                        if (carrier != null && carrier != entity)
                        {
                            if (!carrier.SendMessage(new Message(entity, Message.MessageTypes.OtherAgentRequestsDropItem, dropThis)))
                            {
                                return false;
                            }
                        }

                        // test that the item was in fact dropped:
                        if (!carriedItem.CarriedByAgent(out carrier))
                        {
                            return false;
                        }

                        if (carrier != null)
                        {
                            return false; // the item was not dropped!
                        }

                        itemsToBeDropped.RemoveAll((a) => a == dropThis);
                    }
                }
                while (itemsToBeDropped.Count > 0);
            }

            return true;
        }

        /// <summary>
        /// this should give precedence to Colony work activities
        /// really needed??? time of day is enough?
        /// </summary>
        /// <param name="intelligence"></param>
        /// <returns></returns>
        /*   protected double? ScoreWorkActivity(Intelligence intelligence)
           {
               if (IsColonyWork && The.Sim.DateAndTime.CurrentPhase == Sim.DayPhases.Work)
               {
                   return 1;
               }
               else return 0;
           }*/


        public double ScoreTimeOfDay()
        {
            if (this.ActiveInTime != null) //.HasValue)
            {
                return TimePhaseWithFalloff(ActiveInTime.Item1, ActiveInTime.Item2, TimePhaseFalloff, The.Sim.DateAndTime.TimeOfDay);
            }
            else return 1; // null;
        }

        /// <summary>
        /// make it random if calculate now or not...
        /// </summary>
        /// <param name="timeOfDayScore"></param>
        /// <returns></returns>
        /*protected bool DoHeavyCalculations(double timeOfDayScore)
        {
            if (Globals.Instance.RandomPredictable.NextDouble() < Common.Clamp(timeOfDayScore, 0.1, 1)) // 0.1 is the smallest chance...
            {
                return true;
            }

            return false;

        }*/

        protected double ScoreAge(BiologicalEntity biologicalEntity)
        {
            if (this.ActiveInAgeInterval != null) //HasValue)
            {
                return TimePhaseWithFalloff(ActiveInAgeInterval.Item1, ActiveInAgeInterval.Item2, ageFalloff, biologicalEntity.AgeGroup.Age); //The.Sim.DateAndTime.TimeOfDay);
            }
            else return 1; //null;
        }


        protected const double timeOfDayContributionWeight = 0.2;
        protected const double ageContributionWeight = 0.1;

        /// <summary>
        /// also factors in priority!
        /// </summary>
        /// <param name="rating"></param>
        /// <param name="timeOfDayContribution"></param>
        /// <param name="ageContribution"></param>
        /// <returns></returns>
        /*  public static double AddTimeAndAgeContributions(double rating, double timeOfDayContribution, double ageContribution, float priority)
          {
              return ((1 - timeOfDayContributionWeight - ageContributionWeight) * rating) 
                  + priority * (timeOfDayContributionWeight * timeOfDayContribution + ageContributionWeight * ageContribution);

          }*/

        public static double AddTimeAgeAndPriority(double rating, double timeOfDayContribution, double ageContribution, float priority)
        {
            return priority *
                (((1 - timeOfDayContributionWeight - ageContributionWeight) * rating)
                + (timeOfDayContributionWeight * timeOfDayContribution
                + ageContributionWeight * ageContribution));

        }


        protected double ApplyPriority(double rating)
        {
            return Priority * rating;
        }

        public double GetAgeContribution()
        {
            double ageContribution = 1;
            BiologicalEntity bioEntity;
            if (entity.Find(out bioEntity))
            {
                ageContribution = ScoreAge(bioEntity);
            }
            return ageContribution;
        }


        /// <summary>
        /// an overload that takes entity data instead of a location and will set client feedback if the item is in a danger zone.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="targetEntity"></param>
        /// <param name="approachToUse"></param>
        /// <param name="isAttacking"></param>
        /// <returns></returns>
        public static bool WorkSiteIsSafe(Entity entity, IKnownEntityData targetEntity, ThreatStance approachToUse, bool isAttacking = false)
        {
            if (!WorkSiteIsSafe(entity, targetEntity.PlaySiteLocation, approachToUse, isAttacking))
            {
                The.Client.SetEntityBlockedByThreat(entity.Intelligence.Allegiance, targetEntity, true);

                return false;
            }

            // don't clear the threat - we don't have enough information because we have only looked at the particular spot, not the path

            return true;
        }

        /// <summary>
        /// NEVER call this from evaluators using the entity's CURRENT threat stance! Always use the stance from GetRegionMap!!!
        /// 
        /// is the discomfort level such that work can take place in the tile.
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public static bool WorkSiteIsSafe(Entity entity, Vector3 location, ThreatStance approachToUse, bool isAttacking = false)
        {

            /*  if (approachToUse == null)
              {
                  approachToUse = entity.Intelligence.ThreatStance;
              }*/


            if (!isAttacking && approachToUse != ThreatStance.Bold) // only do this test for regular jobs
            {

                // test the discomfort level at the working site:
                DiscomfortMap dMap = entity.Intelligence.Allegiance.SharedKnowledge.GetDiscomfortMap(entity.Intelligence.ProtectionLevel, // TODO: protection level should be a parameter also
                           entity.EntityType, approachToUse);

                Point tile = MapManager.WorldPosToTile(location);

                byte targetDiscomfortValue = dMap.Map.GetValue(tile);

                if (targetDiscomfortValue > GameData.Instance.AIConstants.HighestDiscomfortLevelForWorkToContinue) // don't enforce this limit when we are fighting.
                {

                    return false;
                }
            }


            return true; // 1d;

        }

        protected bool CanTakeStanceForJob(Job job, ThreatStance normalThreatStance, out ThreatStance threatStanceToUse)
        {
            RegionMap notUsed = null;


            return CanTakeStanceForJob(job, null, normalThreatStance, out notUsed, out threatStanceToUse, false);
        }


        /// <summary>
        /// call this inside the IScoreJob procedure, then it will be re-evaluated coorectly and the job score function will use the correct region map...
        /// </summary>
        /// <param name="job"></param>
        /// <param name="normalRegionMap"></param>
        /// <param name="regionMapToUse"></param>
        /// <param name="threatStanceToUse"></param>
        /// <returns></returns>
        protected bool CanTakeStanceForJob(Job job, RegionMap normalRegionMap, ThreatStance normalThreatStance, out RegionMap regionMapToUse, out ThreatStance threatStanceToUse, bool getRegionMap = true)
        {
            bool jobRequiresBoldStance = job.RequiresBoldStance;


            regionMapToUse = normalRegionMap;
            threatStanceToUse = normalThreatStance; // ThreatStance.Normal;

            if (jobRequiresBoldStance)
            {

                if (!entityIntelligence.StanceCanBeBold())
                {

                    EvaluateAttackJobs.SetJobInaccessibleDueToBoldStance(job, true);

                    // probably cannot reach the job... but it may still be possible...? remove this test?
                    return false;
                }
                else
                {
                    EvaluateAttackJobs.SetJobInaccessibleDueToBoldStance(job, false);

                    // be bold, be brave...
                    threatStanceToUse = ThreatStance.Bold;

                    if (getRegionMap)
                    {
                        // get the bold region map for this job:
                        regionMapToUse =
                              entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(
                                  entity.Intelligence.ProtectionLevel,
                                  entity.EntityType, // entity.Intelligence.Allegiance.RepresentativeEntityType.ThreatCategory, 
                                  ThreatStance.Bold).Layers[SurfaceType.TransportType.Foot].RegionMap;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intervalStart">start of rise up</param>
        /// <param name="intervalEnd">end of falloff</param>
        /// <param name="falloff">the length of falloff/riseup</param>
        /// <param name="input"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool TestWithinInterval(double intervalStart, double intervalEnd, double falloff, double input, ref double value)
        {
            if (input >= intervalStart && input <= intervalEnd)
            {
                //float riseUpEnd = intervalStart + falloff;
                if (input <= intervalStart + falloff) //riseUpEnd)
                {
                    // the rising segment
                    value = MathHelper.SmoothStep(0f, 1f, (float)((input - intervalStart) / falloff));
                    return true;
                }
                else
                {
                    if (input >= intervalEnd - falloff)
                    {   // the falloff segment
                        value = MathHelper.SmoothStep(1f, 0f, (float)((input - (intervalEnd - falloff)) / falloff));
                        return true;
                    }
                    else
                    {
                        // the middle part - return 1:
                        value = 1;
                        return true;
                    }
                }

            }

            // outside the segment.
            return false;

        }

        public const float TimePhaseFalloff = 0.06f;


        /// <summary>
        /// it should be safe to compare this result with zero, since it was directly assigned, rather than calculated.
        /// </summary>
        /// <param name="phaseStart"></param>
        /// <param name="phaseEnd"></param>
        /// <param name="falloff"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public static double TimePhaseWithFalloff(double phaseStart, double phaseEnd, double falloff, double currentTime)
        {
            double smoothStepInterval = falloff; // 0.15f * (phaseEnd - phaseStart);

            double riseUpStart = phaseStart - smoothStepInterval;

            double fallOffEnd = phaseEnd + smoothStepInterval;

            double value = 0.0;

            if (phaseStart > phaseEnd)
            {
                // the interval starts before the 0/1 wrapping point and goes over it. (like 0.8 -> 0.2)
                // "expand" the interval into 2 and test currentTime against both of them:

                if (TestWithinInterval(riseUpStart - 1f, fallOffEnd, smoothStepInterval, currentTime, ref value)) // test -0.2 => 0.2 interval
                {
                    return value;
                }
                else if (TestWithinInterval(riseUpStart, fallOffEnd + 1f, smoothStepInterval, currentTime, ref value)) // test 0.8 => 1.2 interval
                {
                    return value;
                }
                else
                {
                    return 0.0;
                }
            }
            else if (TestWithinInterval(riseUpStart, fallOffEnd, smoothStepInterval, currentTime, ref value))
            {
                return value;
            }
            else return 0.0;

            /*
            // shift the interval?
            float amountToShift = 0f;
            if (riseUpStart < 0)
            {   
                //riseUpStart += 1f;
                amountToShift = Math.Abs(riseUpStart);

                currentTime += amountToShift;
                phaseStart += amountToShift;
                phaseEnd += amountToShift;
            }


            if (currentTime >= riseUpStart)
            {
                float riseUpEnd = phaseStart + smoothStepInterval;
                
                if (currentTime <= riseUpEnd)
                {
                    // in the smooth step interval:
                    return MathHelper.SmoothStep(0f, 1f, (currentTime - riseUpStart) / smoothStepInterval );
                }

            }*/



        }


        /// <summary>
        /// Don't permit items to be dropped near threats (exception when all agents are near each other?)
        /// 
        /// Also, patrollers or attackers (also hunters? scouters?) should never drop their weapons or gear.
        /// Otherwise, this case happens:
        /// 
        /// a mob of attackers are going to an area, where one is carrying a rifle (or better weapon)
        /// a threat job appears.
        /// the first agent (either back in camp or part of the mob) that evaluates the threat job will demand the rifle (because attack job scores are much higher than patrol/attack area scores)
        /// so a small fight over the best weapons ensues before the battle can start.
        /// 
        /// In fact, it makes little sense to force a self defense weapon to be dropped ever... but ESPECIALLY when the threat is right in front of the agent
        /// </summary>
        /// <returns></returns>
        protected bool CanForceDropItems(List<Entity> itemsToDrop)
        {
            foreach (var item in itemsToDrop)
            {
                 if (!CarrierPermitsForceDrop(item)
                     || (Common.DistanceOctile(entity.PlaySiteLocation, item.PlaySiteLocation) > GameData.Instance.AIConstants.MaxDistanceBetweenAgentsToAlwaysAllowForceDrop
                     && IsNearThreats(item)))
                 {
                     return false;
                 }
            }

            return true;
        }

        private bool CarrierPermitsForceDrop(Entity item)
        {
            Entity carrier;
            if (item.CarriedByAgent(out carrier))
            {
                if (carrier != null)
                {
                    return carrier.Intelligence.CanDropRequestedItem(item);
                }
                else
                {
                    return true;
                }               
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// we don't have a quad tree for threats, so sample the threat map instead
        /// we use the cautious map to get a bigger threat radius
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool IsNearThreats(Entity item)
        {
            
          /*  DiscomfortMap dMap = entity.Intelligence.Allegiance.SharedKnowledge.GetDiscomfortMap(entity.Intelligence.ProtectionLevel, 
                           entity.EntityType, ThreatStance.Normal);*/

            ThreatMap map = entity.Intelligence.Allegiance.SharedKnowledge.GetThreatMap(entity.EntityType, ThreatStance.Cautious);

            Point tile = MapManager.WorldPosToTile(item.PlaySiteLocation);

            byte value = map.Map.GetValue(tile);

            if (value > GameData.Instance.AIConstants.HighestThreatLevelToAllowForceDrop)
            {
                return true;
            }

            // the threat map may not show the recent threats yet. So check nearby agents too:

            List<Pair<EntityID, Vector2>> results = null;
            entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), GameData.Instance.AIConstants.MinimumDistanceToThreatsToAllowForceDrop, //  140f, 
               e => IsThreat(e), ref results);

            if (results != null && results.Count > 0)
            {
                return true;
            }

            return false;

            /*
            Dictionary<ThreatStance, ThreatMap> threatmaps;
            if (entityIntelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.ThreatMaps.TryGetValue(entity.EntityType, out threatmaps))
            {
                threatmaps[ThreatStance.Normal].
            }*/
        }

        private bool IsThreat(EntityID entityID)
        {
            // not asset threats..?
            if (entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AllKnownThreatSources.ContainsKey(entityID))
            {
               // entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.
                return true;
            }

            return false;
        }

        protected static bool IsScoreBetterThanAllInvolveds(double ourScore, List<Entity> listOfEntities, Entity thisEntity = null)
        {
            /*if (thisEntity != null && listOfEntities.Contains(thisEntity))
            {
                throw new Exception("!!");
            }
            */

            //   ourScore = ourScore - GameData.Instance.AIConstants.AmountNewGoalMustBeBetterThanOtherEntityToCancel;

            double? otherScore;
            foreach (Entity entityAboutToBeCancelled in listOfEntities)
            {
                otherScore = entityAboutToBeCancelled.Intelligence.GetScore();

                if (otherScore > ourScore)
                {
                    return false;
                }
                else
                {
                    if (entityAboutToBeCancelled.PersonEntity != null && entityAboutToBeCancelled.Name.Contains("Augustine Yeboah"))
                    {

                    }

                    if (thisEntity != null)
                    {
                       /* The.Client.AddLogEvent(The.Client.Log.DebugEvent, thisEntity, string.Format("'s score was {0} > {2} by {1}. Job taken.",
                                                  ourScore.ToString("N3"), entityAboutToBeCancelled, otherScore.Value.ToString("N3")));
                        */
                    }
                }
            }

            return true;
        }
    }
}
