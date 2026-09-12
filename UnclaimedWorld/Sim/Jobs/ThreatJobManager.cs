using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Policies;
namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// Manages threat responses by an allegiance.
    /// 
    /// tasks:
    ///      create threat jobs for entities that we want to attack
    ///      
    ///      assign prey for hunters too? no, that belongs in the production entity like the expedition or household...
    /// </summary>
    public class ThreatJobManager : ICyclable, ISnapshot
    {

        public enum ThreatEvaluationStatus { Done, Processing }
        private Regulator regulator;

        //Owner owner;

        private Allegiances.Allegiance allegiance;
        AllegianceID snapshotAllegiance;

        private enum Phase { CleanupJobs, CreateThreatJobs }
        private Phase phase = Phase.CleanupJobs;

        public bool IsPaused { get; private set; }
        public double StartedOnTimeInSeconds { get; set; }
        public static double totalComputationAllInstancesInSeconds;
        public double TotalComputationAllInstancesInSeconds
        {
            get
            {
                return totalComputationAllInstancesInSeconds;
            }
            set
            {
                totalComputationAllInstancesInSeconds = value;
            }
        }
        public double ComputationTimeSpentInSeconds { get; set; }


        public double? UpdateInterval
        {
            get
            {
                return 1d;
            }
        }

        public ThreatJobManager()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }


        public ThreatJobManager(Allegiances.Allegiance allegiance)
        {

            this.allegiance = allegiance;

            AddToLookup();

            setToNotWaitingID = ActionLookup.AddWithNewID(SetToNotWaiting); // add this now so the holder can retrieve the ID when snapshotting.

            CreateRegulators();
        }

        void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / UpdateInterval.Value, "ThreatJobManager"); // run this more often...

        }

        public void Update(GameTime gameTime)
        {
            if (!The.Sim.CycleManager.IsRegistered(this))
            {
                double milliSecondsSinceLastReady = 0;
                if (regulator.IsReady(ref milliSecondsSinceLastReady))
                {
                    The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);

                    phase = Phase.CleanupJobs;
                }
            }

        }

        public void Destroy()
        {
            if (The.Sim.CycleManager.IsRegistered(this))
            {
                The.Sim.CycleManager.UnRegister(this);
            }

            ActionLookup.Remove(setToNotWaitingID);

            RemoveIDEntry();

        }


        /*  private bool JobTypeAlreadyExists(Type typeOfJob)
          {

              foreach (Job job in owner.OwningBody.Jobs)
              {
                  if (typeOfJob.IsInstanceOfType(job))
                  {
                      return true;
                  }                
              }

              return false;
          }*/

        #region ILookup

        private CyclableID id = CyclableID.Invalid;

        //=================== ILookup Methods =====================
        public CyclableID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public CyclableID GetUniqueID()
        {
            return Cyclable.GetUniqueID(); // share the counter with the other ICyclable classes!
        }

        public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
        {
            return (CyclableID)sn.DoEnum(id);
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != CyclableID.Invalid)
                LookUp<ICyclable, CyclableID>.Add(ID, this);
        }

        public void RemoveIDEntry()
        {
            LookUp<ICyclable, CyclableID>.Remove(this);
        }

        public void SetInvalid()
        {
            id = CyclableID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing... Sim will call Cyclable.ResetIDCounter.
        {
        }

        void ILookUp<ICyclable, CyclableID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ICyclable, CyclableID>.Create();
        }

        #endregion

        #region ICyclable Members

       
        public void CreateThreatJobsFromAttackOutOfBand(Entity attacker, Entity attacked)
        {
            if (GetThreatJobIfExists(attacker) == null)
            {
                float? aggroRange = attacked.Intelligence.Allegiance.GetMaximumAggroRange();
                HandleEntity(attacker, aggroRange);
            }
        }

        public void CreateCombosForAttacksOutOfBand()
        {

        }
        public void ScoreCombosAndCreateJobsOutOfBand()
        {

        }

        private ThreatJob GetThreatJobIfExists(Entity entity)
        {
            Job job;
            if (allegiance.SharedKnowledge.AllKnownEntities.ThreatJobsByTarget.TryGetValue(entity.ID, out job))
            {
                return (ThreatJob)job;
            }

            if (allegiance.SharedKnowledge.AllKnownEntities.AssetThreatJobsByTarget.TryGetValue(entity.ID, out job))
            {
                return (ThreatJob)job;
            }

            return null;

           /* foreach (var job in allegiance.SharedKnowledge.AllKnownEntities.ThreatJobs)
            {
                if (job.Target == entity.EntityID)
                {
                    return job;
                }
            }

            foreach (ThreatJob job in allegiance.SharedKnowledge.AllKnownEntities.AssetThreatJobs)
            {
                if (job.Target == entity.EntityID)
                {
                    return job;
                }
            }
            return null;*/
        }

        private static double ScorePolicy(Entity entity, Allegiances.Allegiance thisAllegiance)
        {
            /*  if (thisAllegiance.AllegianceType == Allegiances.AllegianceType.Player)
              {*/
            AllegiancePolicy.CreaturePolicy policy;
            if (thisAllegiance.Policy.PolicyTowardsCreatures.TryGetValue(entity.EntityType, out policy))
            {
                switch (policy)
                {
                    case AllegiancePolicy.CreaturePolicy.HuntToDestroy:
                        return 1;
                    case AllegiancePolicy.CreaturePolicy.NeverAttack:
                        return 0;
                    case AllegiancePolicy.CreaturePolicy.HuntForProducts:
                        return 0; // handled in JobManager.
                    /*  case Policy.PlayerPolicy.CreaturePolicy.Defend: // must use sharedknowledge properties for Target...
                          if (entity.Intelligence.CombatInfo.Target.Intelligence.Allegiance == thisAllegiance.ID)
                          {
                              return 1;
                          }
                          else return 0;*/

                }
            }
            // }

            return 0;
        }

        /*private bool EntityIsAThreatToUs(Entity entity)
        {
            return false; // GameData.Instance.GlobalThreatenedBy[entity.EntityType]
        }*/

        /// <summary>
        /// TODO: score IKnownEntityData instead of Entity
        /// scores how far the entity is inside our 'aggro range'
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="allegianceAggroRange"></param>
        /// <param name="closestMemberOfAllegiance"></param>
        /// <param name="nearnessScore"></param>
        /// <returns></returns>
        private ThreatEvaluationStatus ScoreNearness(Entity entity, bool isVermin, float? allegianceAggroRange, ref EntityID? closestMemberOfAllegiance, out double nearnessScore, out bool isInAttackZone)
        {
            bool entityCanAttackOrDefend = entity.CanDefendItself();
            bool isOnlyVermin = isVermin && !entityCanAttackOrDefend;
            float? maxDistanceToHuntVermin = allegiance.RepresentativeEntityType.IntelligenceType.MaxDistanceFromExpeditionsToHuntVermin;

            double patrolZoneDistanceScore;

            if (ScoreDistanceToPatrolZones(entity, isOnlyVermin, out patrolZoneDistanceScore, out isInAttackZone) == ThreatEvaluationStatus.Processing)
            {
                nearnessScore = 0.0;
                return ThreatEvaluationStatus.Processing;
            }


            if (allegiance.RepresentativeEntityType.IntelligenceType.AggroRange == null)
            {
                nearnessScore = 1.0; //No aggro radius means infinite aggro range, kind of silly/extreme
                return ThreatEvaluationStatus.Done;
            }

            /*
             * - threats are always attacked if they are close.
               - non-threats are only attacked if:
                we are aggressive
                they are too close
             * Is this done? Does it need to be improved?
             * 
             * define threats based on what is edible (carcass/meat) and what they can catch (strength versus old/young)
             * */

            int addends;

            bool scoreExpedition = allegiance.RepresentativeEntityType.IntelligenceType.OtherAgentsNearExpeditionCenterAreConsideredThreats;
            if (scoreExpedition)
            {
                addends = 3;
            }
            else
            {
                addends = 2;
            }

          
            // also consider distance to any important assets that we own (just the expedition for now, could also be a cave for critters? )
            double expeditionDistanceScore = 0;
            float? closestExpeditionDistance = null;
            if (scoreExpedition)
            {
                if (ScoreDistanceToExpeditionCenter(entity, allegianceAggroRange, 
                    isVermin, out closestExpeditionDistance, out expeditionDistanceScore) == ThreatEvaluationStatus.Processing)
                {
                    nearnessScore = 0.0;
                    return ThreatEvaluationStatus.Processing;
                }
            }
                       

            double closestDistanceScore;
            if (ScoreClosestDistanceWithinAggroRange(entity, allegianceAggroRange, 
                isOnlyVermin, closestExpeditionDistance, maxDistanceToHuntVermin, 
                ref closestMemberOfAllegiance, out closestDistanceScore) == ThreatEvaluationStatus.Processing)
            {
                nearnessScore = 0.0;
                return ThreatEvaluationStatus.Processing;
            }
                        

           

            if (isInAttackZone)
            {
                nearnessScore = 1;
            }
            else
            {
                nearnessScore = closestDistanceScore + expeditionDistanceScore + patrolZoneDistanceScore / (double)addends;
            }

            nearnessScore = Common.ClampTop(nearnessScore, 1d);
           

            return ThreatEvaluationStatus.Done;
        }

        /// <summary>
        /// this is in addition to aggro range from agents
        /// </summary>
        /// <param name="entityToCheckDistanceTo"></param>
        /// <param name="distanceToPatrolZoneScore"></param>
        /// <returns></returns>
        public ThreatEvaluationStatus ScoreDistanceToPatrolZones(Entity entityToCheckDistanceTo, bool isOnlyVermin, out double distanceToPatrolZoneScore, out bool isInAttackZone)
        {
            isInAttackZone = false;

            foreach (var expedition in allegiance.Expeditions)
            {
                foreach (var job in expedition.OwnedEntities.PatrolJobs)
                {
                    PatrolJob patrolJob = job as PatrolJob;
                    
                    if (patrolJob.ThreatArea.Contains(entityToCheckDistanceTo.MapPosition.Value))
                    {
                        if (!isOnlyVermin || patrolJob.AttackVermin)
                        {
                            distanceToPatrolZoneScore = 1d;
                            return ThreatEvaluationStatus.Done; // don't give overlapping zones a higher score.
                        }
                    }
                }

                // attack zones should always create threat jobs for anything inside.
                foreach (var job in expedition.OwnedEntities.AttackAreaJobs)
                {
                    AttackAreaJob attackAreaJob = job as AttackAreaJob;

                    if (attackAreaJob.ThreatArea.Contains(entityToCheckDistanceTo.MapPosition.Value))
                    {
                        if (!isOnlyVermin || attackAreaJob.AttackVermin)
                        {
                            isInAttackZone = true;
                            distanceToPatrolZoneScore = 1d; // max it out!
                            return ThreatEvaluationStatus.Done; 
                        }
                    }
                }
            }

            distanceToPatrolZoneScore = 0d;

            return ThreatEvaluationStatus.Done;
        }



        /// <summary>
        ///  TODO: score IKnownEntityData instead of Entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="allegianceAggroRange"></param>
        /// <param name="distanceToExpeditionScore"></param>
        /// <returns></returns>
        public ThreatEvaluationStatus ScoreDistanceToExpeditionCenter(Entity entity, float? allegianceAggroRange, bool isVermin,
            out float? closestExpeditionDistance,
            out double distanceToExpeditionScore)
        {
            closestExpeditionDistance = null;

            if (allegianceAggroRange == null)
            {
                distanceToExpeditionScore = 1.0f; //If aggro radius is null then we are aggrevated by everything we see
                return ThreatEvaluationStatus.Done;
            }

            float maximumDistanceToCareAbout = allegianceAggroRange.Value; // allegiance.RepresentativeEntityType.IntelligenceType.AggroRange.Value;

            RegionMap footRegionMap = null;

            footRegionMap = allegiance.SharedKnowledge.GetMovementMap(
                ProtectionLevel.Exposed,
                allegiance.RepresentativeEntityType,
                ThreatStance.Bold).Layers[SurfaceType.TransportType.Foot].RegionMap;

            Vector3 location = entity.AccessPoint.Value; // #ACCESS allegiance.SharedKnowledge.GetLocation(entityToCheckDistanceTo); // TODO: don't use this method - deprecate it. call Location directly on either Entity or IKnownEntityData

            float minimumDistance = 10000000f;
            foreach (var expedition in allegiance.Expeditions)
            {
                Vector3 expeditionCenter = expedition.Center.Value;          
               
                Point sourceSubtile = MapManager.WorldPosToSubtile(location);
                Point destinationSubtile = MapManager.WorldPosToSubtile(expeditionCenter);

                float distanceToCenter = 0.0f;
              
                RegionMap.Result result = footRegionMap.GetDistance(entity, sourceSubtile, destinationSubtile, ref distanceToCenter, false, setToNotWaitingID); // callback);
                if (result == RegionMap.Result.Wait)
                {
                    distanceToExpeditionScore = 0.0;
                    return ThreatEvaluationStatus.Processing;
                }

                if (distanceToCenter < minimumDistance)
                {
                    minimumDistance = distanceToCenter;
                    closestExpeditionDistance = distanceToCenter;
                }

                /*
                if (distanceToCenter <= maximumDistanceToCareAbout)
                {
                    double fractionOfMaximumDistance = distanceToCenter / maximumDistanceToCareAbout;
                    distanceToExpeditionScore = 1d - fractionOfMaximumDistance;
                    return ThreatEvaluationStatus.Done;
                }   */             
            }

            if (minimumDistance <= maximumDistanceToCareAbout)
            {                              
                double fractionOfMaximumDistance = minimumDistance / maximumDistanceToCareAbout;
                distanceToExpeditionScore = 1d - fractionOfMaximumDistance;               
               
            }
            else
            {
                distanceToExpeditionScore = 0.0;
            }

            return ThreatEvaluationStatus.Done;
        }

        

        const float aggroFalloffRegion = 0.25f;

        static Vector2[] aggroScoreFunctionPoints = new[] { new Vector2(0f, 1f), new Vector2(0.75f, 1f), new Vector2(1f, 0f) };


        /// <summary>
        /// does it make sense to use the same algo for vermin and threats?
        /// </summary>
        /// <param name="entityToCheckDistanceTo"></param>
        /// <param name="maximumAggroRange"></param>
        /// <param name="isOnlyVermin"></param>
        /// <param name="closestAllegianceMember"></param>
        /// <param name="highestAggroScore"></param>
        /// <returns></returns>
        private ThreatEvaluationStatus ScoreClosestDistanceWithinAggroRange(Entity entityToCheckDistanceTo, float? maximumAggroRange, 
            bool isOnlyVermin, float? distanceToExpedition, float? maxDistanceToHuntVermin,
            ref EntityID? closestAllegianceMember, out double highestAggroScore)
        {

            if (maximumAggroRange == null)
            {
                highestAggroScore = 1.0f; //If aggro radius is null then we are aggrevated by everything we see
                return ThreatEvaluationStatus.Done;
            }

            if (isOnlyVermin
                && maxDistanceToHuntVermin.HasValue
                && distanceToExpedition > maxDistanceToHuntVermin.Value)
            {
                highestAggroScore = 0f;
                return ThreatEvaluationStatus.Done;
            }


            highestAggroScore = 0;

            float closestDistance = 100000f;
            float currentDistance = 1000000f;

            List<Pair<Entity, Vector2>> allegianceMembersCloseEnoughToBeRelevant = null;
            Predicate<Entity> inAllegianceFilter = 
                (Entity entityToCheck) => 
                { 
                    return allegiance.Members.Contains(entityToCheck); 
                };

            // first get entities inside air radius, using the allegiance's maximum aggro range:
            The.AgentQuadTree.GetEntitiesInRange(
                        entityToCheckDistanceTo.PlaySiteLocation.ToVector2(),
                        maximumAggroRange.Value,
                        inAllegianceFilter,
                        ref allegianceMembersCloseEnoughToBeRelevant
                    );

            if (allegianceMembersCloseEnoughToBeRelevant == null
                || allegianceMembersCloseEnoughToBeRelevant.Count == 0)
            {
                highestAggroScore = 0.0;
                return ThreatEvaluationStatus.Done;
            }


            // now check ground distance to each member, using its specific aggro range:
            RegionMap footRegionMap = null;

            float? aggroRange;

            float aggroScore;
            float normalizedDistance;

            foreach (var allegianceMember in allegianceMembersCloseEnoughToBeRelevant)
            {
                // if aggrorange is null (sensor?) just continue - or do we want them to hunt critters near their sensors???
                aggroRange = allegianceMember.First.GetAggroRange(); // ?? maximumAggroRange;

                if (aggroRange == null)
                {
                    continue;
                }

                float? coneWidth, coneLength;
                float attackRange = allegianceMember.First.GetAttackRange(out coneWidth, 
                    out coneLength);

                 if (attackRange > 0f)
                 {
                     // use air distance if ranged attack is available
                     currentDistance = Common.DistanceOctile(allegianceMember.First.PlaySiteLocation, entityToCheckDistanceTo.PlaySiteLocation);
                 }
                 else
                 {
                     // else use the terrain distance:     
                     footRegionMap = The.Map.FootTerrainRegionMap;

                     RegionMap.Result result = footRegionMap.GetDistanceToEntity(allegianceMember.First, entityToCheckDistanceTo, allegianceMember.First, ref currentDistance, null, null, false, setToNotWaitingID);


                     if (result == RegionMap.Result.Wait)
                     {
                         highestAggroScore = 0.0;
                         return ThreatEvaluationStatus.Processing;
                     }
                 }
                               

                if (currentDistance < closestDistance)
                {
                    closestAllegianceMember = allegianceMember.First.EntityID;
                    closestDistance = currentDistance;
                }


                if (currentDistance < aggroRange.Value) // inside the aggro range
                {
                    if (isOnlyVermin
                        && maxDistanceToHuntVermin.HasValue
                        && currentDistance > maxDistanceToHuntVermin.Value)
                    {
                        aggroScore = 0f;
                    }
                    else
                    {
                        // normalize and invert to get the score:
                        normalizedDistance = currentDistance / aggroRange.Value;

                        // let the score fall off only at the outer .25 part of the aggro range, inside this radius the score is 1:      
                        aggroScore = Common.GetInterpolatedFunctionValue(normalizedDistance, aggroScoreFunctionPoints);
                    }
                }
                else // outside
                {
                    aggroScore = 0f;
                }

                if (aggroScore > highestAggroScore)
                {
                    highestAggroScore = aggroScore;
                }

            }


            return ThreatEvaluationStatus.Done;
        }

        private MethodID setToNotWaitingID;
        public void SetToNotWaiting()
        {
            IsPaused = false;
        }

        /// <summary>
        /// give a high score if we consider this entity particularly dangerous
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="thisAllegiance"></param>
        /// <param name="aggressionScore"></param>
        /// <returns></returns>
        private static ThreatEvaluationStatus ScoreAggressionOfEntity(Entity entity, Allegiances.Allegiance thisAllegiance, out double aggressionScore)
        {
            if (thisAllegiance.WasRecentlyAttackedBy(entity)) // also if currently attacking the allegiance
            {
                aggressionScore = 1.0;
                return ThreatEvaluationStatus.Done;
            }

            aggressionScore = 0.0;
            return ThreatEvaluationStatus.Done;

            /* this code used to classify an entity as a threat if we are standing inside its aggro radius. But, it would be better to try to move away before attacking..
            if (entity.EntityType.BiologicalType.IsPredator)
            {
                bool isInAggrevationRange = false;
                ThreatEvaluationStatus result = entity.AllegianceIsAggravatedByMe(thisAllegiance, ref isInAggrevationRange);
                if (result == ThreatEvaluationStatus.Done)
                {
                    if (isInAggrevationRange == true)
                    {
                        aggressionScore = 1.0;
                    }
                    else
                    {
                        aggressionScore = 0.0;
                    }
                    return ThreatEvaluationStatus.Done;
                }
                else
                {
                    aggressionScore = 0.0;
                    return ThreatEvaluationStatus.Processing;
                }
            }
            else
            {
                aggressionScore = 0.0;
                return ThreatEvaluationStatus.Done;
            }*/
        }

        private ThreatEvaluationStatus ScoreCommonThreat(Entity entity, double nearnessScore, ref double threatScore)
        {
            // see if it's dead
            bool isDead;
            bool isUnconscious;

            CauseOfDeath? causeOfDeath;
            CauseOfUnconsciousness? causeOfUnconsciousness;

            entity.GetStatus(out isDead, out isUnconscious, out causeOfDeath, out causeOfUnconsciousness);
            if (isDead)
            {
                // the target is probably playing its death/collapse anim. don't kill it again!
                threatScore = 0.0;
                return ThreatEvaluationStatus.Done;
            }


            if (Common.IsZero(nearnessScore)) // = outside aggro range. Do we want to hunt down aggressive critters outside the range?
            {
                threatScore = 0.0;
                return ThreatEvaluationStatus.Done;
            }

            threatScore = 1d;
            return ThreatEvaluationStatus.Done;
        }

        /// <summary>
        /// Only score those entities that might threaten agents
        /// TODO: score IKnownEntityData instead of Entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="allegianceAggroRange"></param>
        /// <param name="threatScore"></param>
        /// <param name="closestMemberOfAllegiance"></param>
        /// <returns></returns>
        private ThreatEvaluationStatus ScoreThreatToAgents(Entity entity, double nearnessScore, bool isInAttackZone, ref double threatScore)
        {
            // NOTE: Vermin are not considered here - they have their own method!

            // TODO: redo this with better modeling of species


            // humans should not attack bush dragons needlessly
            // twinklers should attack...
            // this code is not working:
            /*   if (   allegiance.RepresentativeEntityType.ThreatCategory == ThreatCategory.IndigHerbivore 
                   && allegiance.RepresentativeEntityType.ThreatCategory != ThreatCategory.Predator)
               {
                   // makes bush dragons never fight back
                   threatScore = 0.0;//Test, temporary solution for needlessly aggressive behaviour towards bush dragons
                   return ThreatEvaluationStatus.Done;
               }*/

            // Human.IsPredator = true

            bool entityCanAttackOrDefend = entity.CanDefendItself();

            //Creatures with no wish to fight will never be a threat, however if we are predators we still want to attack them

            if (allegiance.RepresentativeEntityType.IntelligenceType.WillAttackNonThreatsNearby == false)
            {
                if (entityCanAttackOrDefend == false)
                {
                    threatScore = 0.0;
                    return ThreatEvaluationStatus.Done;
                }

                //If the creature is not out to kill us by nature then perhaps we can let it flee in peace? Yes, but not inside attack zones.
                if (!isInAttackZone)
                {
                    if (entity.IsFleeing() && entity.EntityType.IntelligenceType.IsPredator == false)
                    {
                        threatScore = 0.0;
                        return ThreatEvaluationStatus.Done;
                    }
                }
            }

            // don't attack robots without fighting ability...
            if (entity.EntityType.BiologicalType == null
                && !entityCanAttackOrDefend)
            {
                threatScore = 0.0;
                return ThreatEvaluationStatus.Done;
            }


            if (isInAttackZone)
            {
                // these creatures should always be attacked inside attack zones
                threatScore = 1;
                return ThreatEvaluationStatus.Done;
            }


            double aggressionScore;
            if (ScoreAggressionOfEntity(entity, allegiance, out aggressionScore) == ThreatEvaluationStatus.Processing)
            {
                return ThreatEvaluationStatus.Processing;
            }

            // Human has IsPredator = true!!

            // this makes humans ignore bush dragons unless it's self-defense:
            if ((entity.EntityType.IntelligenceType == null || entity.EntityType.IntelligenceType.IsPredator == false) // if the entity is not a predator, 
               && Common.IsZero(aggressionScore)                    // and not agressive, 
                //&& (allegiance.RepresentativeEntityType.IntelligenceType.IsPredator == false || allegiance.RepresentativeEntityType.ThreatCategory == ThreatCategory.Human) // human - hack?
               && (allegiance.RepresentativeEntityType.IntelligenceType.IsPredator == false || allegiance.RepresentativeEntityType.IntelligenceType.WillAttackNonThreatsNearby == false)
               && allegiance.RepresentativeEntityType.BiologicalType.IsTerritorial == false) //and we are not territorial - don't attack:
            {
                threatScore = 0.0;
                return ThreatEvaluationStatus.Done;
            }

            threatScore = GameData.Instance.AIConstants.AggressionScoreFraction * aggressionScore + GameData.Instance.AIConstants.NearnessScoreFraction * nearnessScore;// +0.2 * ScorePolicy(entity, thisAllegiance);

            return ThreatEvaluationStatus.Done;
        }

        private ThreatEvaluationStatus ScoreThreatToAssets(Entity entity, bool isVermin, bool isInAttackZone, ref double threatRating)
        {
            if (isVermin) 
            {
                if (isInAttackZone)
                {
                    threatRating = 1f;
                }
                else
                {
                    threatRating = 0.1f;
                }
            }

            return ThreatEvaluationStatus.Done;

        }

        /// <summary>
        /// a number from 0 to 1 that tells how strong the ally is compared to global strength levels      
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ourStrengthRating"></param>
        /// <returns></returns>
        /*   public static double GetAllyStrengthLevel(Entity entity, StrengthRating ourStrengthRating) // Entity representativeEntity)
           {
               double strengthLevel = 0.2 * ((float)ourStrengthRating) + 0.1; // LikeHumans = 0.5

               switch (entity.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup)
               {
                   case Entities.AIAgeGroup.Baby:
                       strengthLevel = 0;
                       break;
                   case Entities.AIAgeGroup.Child:
                       strengthLevel *= 0.5;
                       break;
                   case Entities.AIAgeGroup.YoungAdult:
                       strengthLevel *= 0.7;
                       break;
                   case Entities.AIAgeGroup.Old:
                       strengthLevel *= 0.8;
                       break;
               }
           
          
               Body body;

               if (entity.Find(out body))
               {
                   strengthLevel *= body.FunctionalScore;
               }

               strengthLevel = Common.Clamp(strengthLevel, 0, 1);

               // todo: add points if the ally is carrying weapons

               return strengthLevel;
           }
           */

        public enum SpeciesThreatLevel { NoThreat, ThreatToYoung, }
        public void GetSpeciesThreatLevel()
        {

        }


        /*private void RemoveNonAggrevatingThreats()
        {
            var queue = new Queue<Action>();
            foreach (ThreatJob job in allegiance.ThreatJobs)
            {   
                if(allegiance.IsInAggrevationRange(job.Target.Value) == false)
                {
                    queue.Enqueue(() => job.CancelAllTakersAndRemoveJob());
                }
            }
            foreach (var action in queue)
            {
                action();
            }
        }*/

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("ThreatJobManager {0}: {1}", ID, phase));

        }


        public bool CycleOnce()
        {
            // Currently, we don't keep progress here... just a paused flag while we wait for region results...
            
            switch (phase)
            {
                case Phase.CleanupJobs:
                    {
                        //RemoveNonAggrevatingThreats();

                        phase = Phase.CreateThreatJobs;
                        break;
                    }
                // we take time of day into account when adding/removing these jobs - so agents don't need to.
                // This is factoring out calculations!
                case Phase.CreateThreatJobs:
                    {
                        // look at all spotted entities to determine their threat level:
                        //allegiance.SharedKnowledge.AllKnownOutsideEntities
                        if (allegiance.SharedKnowledge.PlaySiteKnowledge.AllKnownOutsideAgentsOnPlaySite.Count > 0)
                        {
                            float? allegianceAggroRange = allegiance.GetMaximumAggroRange();

                            List<EntityID> invalidEntityIDs = null;
                            IKnownEntityData entityData;
                            Entity entity;
                            foreach (var kvp in allegiance.SharedKnowledge.PlaySiteKnowledge.AllKnownOutsideAgentsOnPlaySite)
                            {
                                if (!GoalEvaluator.EntityDataResultCausesSkip(allegiance.SharedKnowledge.GetKnownData(kvp.Key, out entityData)))
                                {
                                    // TODO: handle memory facts also
                                    entity = entityData as Entity;
                                    if (entity != null)
                                    {
                                        if (HandleEntity(entity, allegianceAggroRange) == ThreatEvaluationStatus.Processing)
                                        {
                                            return true;
                                        }
                                    }
                                }
                                else
                                {
                                    Common.AddToList(ref invalidEntityIDs, kvp.Key);
                                }
                            }

                            allegiance.SharedKnowledge.RemoveInvalidEntityIDs(invalidEntityIDs);
                        }


                        phase = Phase.CleanupJobs;
                        return true;
                    }


            }

            return false;
        }

        private ThreatEvaluationStatus HandleEntity(Entity entity, float? allegianceAggroRange)
        {
            ThreatJob threatJob;

            EntityID? closestMemberOfAllegiance = null;

            bool isVermin = false; // some critters are both threats AND vermin?
            if (entity.EntityType.BiologicalType != null && entity.EntityType.BiologicalType.IsVermin)
            {
                isVermin = true;
            }

            //total rating should probably be 0 if nearness score = 0
            // nearness is scored depending on vermin/threat status
            double nearnessScore;
            bool isInAttackZone;
            if (ScoreNearness(entity, isVermin, allegianceAggroRange, ref closestMemberOfAllegiance, out nearnessScore, out isInAttackZone) == ThreatEvaluationStatus.Processing)
            {
                return ThreatEvaluationStatus.Processing;
            }


            // tODO: use memory facts

            double commonThreat = 0d;
            if (ScoreCommonThreat(entity, nearnessScore, ref commonThreat) == ThreatEvaluationStatus.Processing)
            {
                IsPaused = true;
                return ThreatEvaluationStatus.Processing;
            }

            double threatRating = 0.0;
            bool isThreatToAssetsOnly = false;
            if (!Common.IsZero(commonThreat))
            {              

                if (ScoreThreatToAgents(entity, nearnessScore, isInAttackZone, ref threatRating) == ThreatEvaluationStatus.Processing)
                {
                    IsPaused = true;
                    return ThreatEvaluationStatus.Processing;
                }

                if (threatRating < GameData.Instance.AIConstants.MinimumThreatRatingToBeAThreatToAgents)
                {
                    threatRating = 0;
                }

                if (Common.IsZero(threatRating))
                {
                    if (ScoreThreatToAssets(entity, isVermin, isInAttackZone, ref threatRating) == ThreatEvaluationStatus.Processing)
                    {
                        IsPaused = true;
                        return ThreatEvaluationStatus.Processing;
                    }

                    if (Common.IsGreaterThan(threatRating, 0d))
                    {
                        isThreatToAssetsOnly = true;
                    }
                }
                
            }

            threatJob = GetThreatJobIfExists(entity);

            if (threatRating > 0)
            {
                // only if not exist!
                if (threatJob == null)
                {
                    // create if not already exists
                    threatJob = new ThreatJob(entity, allegiance.SharedKnowledge.AllKnownEntities, isThreatToAssetsOnly)
                        {
                            ThreatRating = threatRating,
                            ClosestMemberOfAllegiance = closestMemberOfAllegiance
                        };
                }
                else
                {
                    // update threat rating only:
                    threatJob.ThreatRating = threatRating;
                    threatJob.IsVermin = isThreatToAssetsOnly;
                    threatJob.ClosestMemberOfAllegiance = closestMemberOfAllegiance;
                }
            }
            else
            {
                if (threatJob != null)
                {
                    // cancel / remove any existing job:
                    threatJob.Destroy(true);
                }
            }
            return ThreatEvaluationStatus.Done;
        }


        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return false;
            }
        }

        #endregion


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // the manager may be waiting for calls to RegionMap when snapshotted.

            // Currently, we don't keep progress here... just a paused flag while we wait for region results...


            this.id = SnapshotID(sn, id);
            this.snapshotAllegiance = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(allegiance);
            setToNotWaitingID = (MethodID)sn.DoEnum(setToNotWaitingID);
            IsPaused = sn.DoBool(IsPaused);

            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

            sn.Ignore(allegiance);
            sn.Ignore(phase);
            sn.Ignore(aggroScoreFunctionPoints);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);

            // this makes sure that the holder of the method ID can get the delegate after load:
            ActionLookup.Add(setToNotWaitingID, SetToNotWaiting);

            CreateRegulators();
        }
    }
}
