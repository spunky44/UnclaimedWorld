using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.AI.Goals;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.Jobs
{
    public abstract class AttackJob : Job
    {
        /// <summary>
        /// why nullable..?
        /// a ThreatJob will always have a target, I guess
        /// </summary>
        public EntityID? Target;

        public AttackJob(EntityGroup entityGroup, bool addToJobsGroupNow = true)
            : base(entityGroup, /*Priority.Normal,*/ addToJobsGroupNow)
        {

            /*
            ComputeJobType();
            SetDefaultPriority(entityGroup);*/
        }

       
        public AttackJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public GoalEvaluator.CalculateResult ScoreThisJobWithoutWeapon(Entity entity, Intelligence entityIntelligence,
            WeaponInstanceCombo combo,          
            int proposedNumberOfWorkers, double? ageContribution,
                                      double? timeContribution, ref double rating,
            double? fitnessScore, /*float estimatedDamageScore,*/ float priority,          
            float? energyLevelFactor)
        {
            rating = 0;
            SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;

            IKnownEntityData targetData;
            if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(combo.AttackData.JobData.Job.Target.Value, out targetData)))
            {
                return GoalEvaluator.CalculateResult.Done;
            }

            double canHuntScore;

            HuntingJob huntingJob = this as HuntingJob;
            if (huntingJob != null)
            {
                canHuntScore = ScoreHuntSuccessEstimate(entity, combo.AttackData.AttackType, targetData);

                if (canHuntScore <= 0d)
                {                    
                    // feedback
                    EvaluateAttackJobs.SetHuntingJobNotFeasible(this, true);
                    return GoalEvaluator.CalculateResult.Done;
                }
                else
                {
                    EvaluateAttackJobs.SetHuntingJobNotFeasible(this, false);
                }

                if (GoalHunt.TargetIsTooFarFromExpedition(entity, targetData.PlaySiteLocation, 100f))
                {
                    // feedback - hmm, not agent dependent...
                    EvaluateAttackJobs.SetJobTooFarFromExpedition(this, true);
                    return GoalEvaluator.CalculateResult.Done;
                }
                else
                {
                    EvaluateAttackJobs.SetJobTooFarFromExpedition(this, false);
                }
            }

                      
            if (!combo.AttackData.JobData.AttackDirection.HasValue)
            {
                combo.AttackData.JobData.AttackDirection = GoalDoAttack.GetAttackDirection(entity,
                   targetData.PlaySiteLocation,
                   targetData.Rotation);
            }

                    
            double retaliationScore = ScoreRetaliation(entity, targetData);

            double threatScore = 0;

            ThreatJob threatJob = this as ThreatJob;
            if (threatJob != null)
            {     
                // handle patrol settings and vermin hunting
                CombatAreaJob combatAreaJob = null;
                
                if (threatJob.IsVermin)
                {
                    if (!entityIntelligence.CanAttackVermin(out combatAreaJob))
                    {
                        return GoalEvaluator.CalculateResult.Done;
                    }    
                }

                if (combatAreaJob != null && !combatAreaJob.CanAttackTargetsOutsideZone)
                { 
                    // don't leave the patrol area:
                    Rectangle bounds = combatAreaJob.Zone.MapArea.BoundingRectangle.Value;  
                   
                    if (!bounds.Contains(targetData.MapPosition.Value))
                    {
                        // test a radius around the agent too, to make tiny zones usable and allow chasing:
                        if (Common.DistanceOctile(entity.PlaySiteLocation, targetData.PlaySiteLocation) > GameData.Instance.AIConstants.MaxDistanceOutsidePatrolZoneToChaseTargets)
                        {
                            return GoalEvaluator.CalculateResult.Done;
                        }
                    }                    
                }


                threatScore = ScoreThreatRating(threatJob);
            }
                       

            double damageAndChanceToHitScore = ScoreAttackDamageAndChanceToHit(entity, combo, targetData, energyLevelFactor.Value, combo.AttackData.EstimatedDamageScore);


            double inertiaScore = ScoreInertia(entity, targetData);
            if (inertiaScore == 0.0)
            {
                inertiaScore = entity.Intelligence.Memory.GetRecentlyFoundPreyScore(targetData.EntityID);
            }

            GoalEvaluator.WeightedRating WeightedRating = new GoalEvaluator.WeightedRating();
           
            if (this is HuntingJob)
            {
                ScoreHunting(entity, combo, ref rating, targetData, retaliationScore, threatScore, damageAndChanceToHitScore, inertiaScore, ref WeightedRating);
            }
            else
            {
                ScoreAttack(entity, combo, ref rating, targetData, retaliationScore, threatScore, damageAndChanceToHitScore, inertiaScore, ref WeightedRating);
            }
            
            rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);



            return GoalEvaluator.CalculateResult.Done;
        }

        public override bool RequiresBoldStance
        {
            get
            {
                return true;
            }
        }

        public override Vector3? GetCircaLocation()
        {
            IKnownEntityData targetData;
            if (Target.HasValue 
                && base.ResolveOwner(out EntityGroup owner))
            {
                if (!GoalEvaluator.EntityDataResultCausesSkip(owner.GetAllegiance().SharedKnowledge.GetKnownData(Target.Value, out targetData)))
                {
                    return targetData.Location;
                }
            }

            return null;
        }

        private void ScoreAttack(Entity entity, WeaponInstanceCombo combo, ref double rating, IKnownEntityData targetData, double retaliationScore, double threatScore, double damageAndChanceToHitScore, double inertiaScore, ref GoalEvaluator.WeightedRating WeightedRating)
        {
            switch (combo.AttackData.AttackType.RangeType)
            {
                case AttackType.RangeTypes.Melee:
                    {
                        double adjacencyScore = ScoreAdjacencyToTarget(entity, targetData);

                        //this assortment of scores is tailored for melee attacks
                        WeightedRating.AddScore(0.23, inertiaScore);
                        WeightedRating.AddScore(0.28, adjacencyScore);
                        WeightedRating.AddScore(0.23, retaliationScore);
                        //WeightedRating.AddScore(0.40, combo.AttackData.JobData.TravelTimeToTargetScore.Value);
                        WeightedRating.AddScore(0.13, damageAndChanceToHitScore);
                        WeightedRating.AddScore(0.13, threatScore);
                        break;
                    }
                case AttackType.RangeTypes.Ballistic:
                case AttackType.RangeTypes.Ray:
                case AttackType.RangeTypes.Rocket:
                    {
                        //we need a different assortment for ranged attacks
                        double stationaryScore = ScoreStationaryTarget(targetData);
                        double rangeScore = ScoreRange(entity, targetData, combo.AttackData.AttackType);
                        double inFieldOfViewScore = ScoreInFieldOfView(entity, targetData);

                        WeightedRating.AddScore(0.23, inertiaScore);
                       // WeightedRating.AddScore(0.40, combo.AttackData.JobData.TravelTimeToTargetScore.Value); //TODO should compute travel time to within range, not through to target entity
                        WeightedRating.AddScore(0.13, damageAndChanceToHitScore);
                        WeightedRating.AddScore(0.23, stationaryScore); // some homing weapons could disregard this...
                        WeightedRating.AddScore(0.23, rangeScore);
                        WeightedRating.AddScore(0.18, inFieldOfViewScore);


                        // Lars test:
                        //  WeightedRating.AddScore(1d, 1d);

                        // other scores for the ranged cases may include some of the following:                        
                        // ScoreMovingOutOfRange()
                        // ScoreClosestTarget()
                        // ScoreMinimalCollateralDamage()                        
                        // ScoreTimeToKillTarget()                        

                        break;
                    }

            }

            rating = WeightedRating.Result;
        }

        /// <summary>
        /// the weights for hunting are different than for combat
        /// </summary>     
        private void ScoreHunting(Entity entity, WeaponInstanceCombo combo, ref double rating, IKnownEntityData targetData, double retaliationScore, double threatScore, double damageAndChanceToHitScore, double inertiaScore, ref GoalEvaluator.WeightedRating WeightedRating)
        {
            switch (combo.AttackData.AttackType.RangeType)
            {
                case AttackType.RangeTypes.Melee:
                    {
                        double adjacencyScore = ScoreAdjacencyToTarget(entity, targetData);

                        //this assortment of scores is tailored for melee attacks
                        if (!Common.IsZero(inertiaScore))
                        {
                            WeightedRating.AddScore(0.55, inertiaScore);
                            WeightedRating.AddScore(0.30, adjacencyScore);                       
                            WeightedRating.AddScore(0.15, damageAndChanceToHitScore);
                           // WeightedRating.AddScore(0.15, threatScore);
                        }
                        else
                        {
                            WeightedRating.AddScore(0.23, inertiaScore);
                            WeightedRating.AddScore(0.28, adjacencyScore);
                            WeightedRating.AddScore(0.23, retaliationScore);                      
                            WeightedRating.AddScore(0.13, damageAndChanceToHitScore);
                            WeightedRating.AddScore(0.13, threatScore);
                        }
                        break;
                    }
                case AttackType.RangeTypes.Ballistic:
                case AttackType.RangeTypes.Ray:
                case AttackType.RangeTypes.Rocket:
                    {
                        //we need a different assortment for ranged attacks
                        double stationaryScore = ScoreStationaryTarget(targetData);
                        double rangeScore = ScoreRange(entity, targetData, combo.AttackData.AttackType);
                        double inFieldOfViewScore = ScoreInFieldOfView(entity, targetData);
                        if (!Common.IsZero(inertiaScore))
                        {
                            WeightedRating.AddScore(0.8, inertiaScore); // this score needs to be high... otherwise GoalFindPrey will not transfer to GoalHunt when prey is found (instead he will haul his rifle back home)
                            WeightedRating.AddScore(0.05, damageAndChanceToHitScore);
                            WeightedRating.AddScore(0.05, stationaryScore); // some homing weapons could disregard this...
                            WeightedRating.AddScore(0.05, rangeScore);
                            WeightedRating.AddScore(0.05, inFieldOfViewScore);
                        }
                        else
                        {
                            WeightedRating.AddScore(0.23, inertiaScore);
                            WeightedRating.AddScore(0.13, damageAndChanceToHitScore);
                            WeightedRating.AddScore(0.23, stationaryScore); // some homing weapons could disregard this...
                            WeightedRating.AddScore(0.23, rangeScore);
                            WeightedRating.AddScore(0.18, inFieldOfViewScore);
                        }

                        break;
                    }

            }

            rating = WeightedRating.Result;
        }
        /*
        private void ScoreAttackTypeAgainstTarget(Entity entity, SharedKnowledge sharedKnowledge, AttackJob threatJob)
        {
            Body targetBody;
            threatJob.Target.Find(out targetBody);

            Body entityBody;
            entity.Find(out entityBody);

            double bestScore = 0;

            BodyPart.AttackDirection attackDirection = GoalDoAttack.GetAttackDirection(entity,
                sharedKnowledge.GetLocation(threatJob.Target),
                sharedKnowledge.GetRotation(threatJob.Target));

            float energyLevelFactor;

            foreach (AttackType attackType in entity.EntityType.IntelligenceType.AttackTypes)
            {
                if (AttackTypeGeometryIsCorrect(attackType, sharedKnowledge)
                    && IntelligenceType.AttackTypeIsFunctional(entityBody, attackType))
                {
                    // take into account our physical state:
                    energyLevelFactor = GoalDoAttack.GetEnergyLevelFactorOnDamage(entity, attackType);

                    foreach (BodyPart bodyPart in targetBody.BodyParts)
                    {
                        if (bodyPart.IsFunctional()) // we will not attack body parts that are already destroyed.
                        {
                            ScoreAttackTypeAgainstBodyPart(entity, threatJob.Target, threatJob, attackType, 
                                bodyPart, attackDirection, energyLevelFactor, , ref bestScore);
                        }
                    }
                }
            }

           // return bestScore;
        }
        */


      /*  private bool AttackTypeGeometryIsCorrect(AttackType attackType, SharedKnowledge sharedKnowledge)
        {
            object targetStance = sharedKnowledge.GetProperty(Target, MemoryFact.StatusProperty.Stance);

            if (targetStance != null)
            {
                MobileEntity.Stance stance = (MobileEntity.Stance)targetStance;
                if (stance == MobileEntity.Stance.Sitting || stance == MobileEntity.Stance.Laying)
                {
                    if (!attackType.IsDownAttack)
                    {
                        return false;
                    }
                }
            }

            return true;

        }*/
        
        public GoalEvaluator.CalculateResult ScoreWeapon(Entity entity, IKnownEntityData weapon, double locationScore,
                   double? ageContribution, double? timeContribution,
                   float priority, AttackType attackType, out double score)
        {
            score = 0d;

          
            double conditionScore = ScoreWeaponCondition(weapon);
            
            if (Common.IsZero(conditionScore) || Common.IsZero(locationScore))
            {
                score = 0f;
            }
            else
            {
                score = 0.7f * locationScore + 0.3f * conditionScore; // +0.2f * ammoScore;
            }

            double policyScore = GetWeaponPolicyScore(entity, weapon, attackType);

            if (Common.IsZero(policyScore))
            {
                score = 0f;
                return GoalEvaluator.CalculateResult.Done;
            }

            if (locationScore == 0)
            {
                score = 0f;
                return GoalEvaluator.CalculateResult.Done;
            }
            else
            {
                score = 0.7f * locationScore + 0.3f * conditionScore; // +0.2f * ammoScore;
            }


            if (Common.IsZero(score))
            {
                return GoalEvaluator.CalculateResult.Done;
            }
            else
            {

                score = GoalEvaluator.AddTimeAgeAndPriority(score, timeContribution.Value, ageContribution.Value, priority);

                return GoalEvaluator.CalculateResult.Done;
            }

        }

        public virtual double GetWeaponPolicyScore(Entity entity, IKnownEntityData weapon, AttackType attackType)
        {
            return 1d;
        }

       


        private double ScoreWeaponCondition(IKnownEntityData weapon)
        {
            return weapon.Condition.Value; // weapon.NonLivingEntity.Condition;

        }


      
        /// <summary>
        /// precomputed damage scores for estimator
        /// </summary>
        /// <returns></returns>
        public static float ComputeEstimatedDamageScore(AttackType attackType, BodyPartType bodyPart, float bodyHitpoints, out float meanDamage)
        {
            float estimatedDamage;

            float resistance;
            float reductionConstant;
            BodyLayerType armorLayer;

            attackType.ComputeDamage(bodyPart, out estimatedDamage, out resistance, out reductionConstant, out armorLayer, false, 1f, attackType.DamageMean);


            float damageDone = BodyPart.GetDamageDone(estimatedDamage, bodyPart.HitpointsFraction * bodyHitpoints);
            meanDamage = damageDone;

            // if the estimated damage is less than 0, consider the upper part of the spread too, as we might get lucky:
            // Two standard deviations away from the mean account for roughly 95 percent chance 
            if (estimatedDamage == 0f)
            {
                attackType.ComputeDamage(bodyPart, out estimatedDamage, out resistance, out reductionConstant, out armorLayer, false, 1f,
                    attackType.DamageMean + GameData.Instance.AIConstants.UpperRangeOfEffectiveDamageInStandardDeviations * attackType.DamageStandardDeviation);

                estimatedDamage = 0.1f * estimatedDamage; // compensate for low chance for this attack to succeed.

                estimatedDamage = Common.ClampBottom(estimatedDamage, 0f);
            }

            // return estimatedDamage;

            float vitalBonusFactor = 1f;
            if (bodyPart.IsVital())
            {
                vitalBonusFactor = 2f; // increased to make leg hits more seldom... perhaps weigh differently from agent type - // 1.5f;
            }

            estimatedDamage *= vitalBonusFactor;

            // modify ba accuracy:
            estimatedDamage *= attackType.AccuracyFactor;

            // an attempt to scale the damage...
            float score = /*chanceToHit **/ GameData.Instance.OneOverMeanDamageFromHumanPunch * estimatedDamage;

            score = 0.3f * score; //?

            score = Common.Clamp(score, 0f, 1f);
            return score;
        }


        /// <summary>
        /// computes the estimated score for damage and chance to hit
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="attackType"></param>
        /// <param name="bodyPart"></param>
        /// <param name="attackDirection"></param>
        /// <param name="energyLevelFactor"></param>
        /// <param name="estimatedDamageScore"></param>
        /// <returns></returns>
        public static float ScoreAttackDamageAndChanceToHit(Entity attacker,
            WeaponInstanceCombo combo, IKnownEntityData targetData,
            float energyLevelFactor,
             float estimatedDamageScore)
        {
            float chanceToHit = GoalDoAttack.ComputeChanceToHit(attacker, targetData,
                combo.AttackData.bodyPartID, combo.AttackData.JobData.AttackDirection.Value, combo.AttackData.AttackType);

            float score = chanceToHit * estimatedDamageScore;
            return score;
        }



        /// <summary>
        /// if hunting, score our chances to get in range of the target (possibly without being seen).
        /// Hunting thunder chickens with melee weapons will fail if we have been seen - but the player AI only knows this once it has tried..
        /// </summary>
        /// <returns></returns>
        public static double ScoreHuntSuccessEstimate(Entity attacker, AttackType attackType, IKnownEntityData targetData)
        {
            // should there be 'dumb' critters that will always attempt to hunt humans? swarming/exhaustion tactics?

            bool research;
            if (attacker.Intelligence.Allegiance.HumanActivities != null
                && !attacker.Intelligence.Allegiance.HumanActivities.HasChasedHuntedCritter.TryGetValue(targetData.EntityType, out research))
            {
                return 1d; // can't use our research yet - scrap this??? there is no feedback for this "feature"!
            }
            
            if (targetData.EntityType.LocomotorType.LeggedLocomotorType.WalkFastSpeed < attacker.EntityType.LocomotorType.LeggedLocomotorType.WalkFastSpeed) 
            {
                // can be chased down:
                return 1d;
            }
            else
            {
                // ranged attacks are ok
                if (attackType.RangeType != AttackType.RangeTypes.Melee)
                {
                    return 1d;
                }
                else
                {
                    // we cannot chase the prey down if it has spotted us
                    // only if we can see the target, will we know if it has seen us:

                    bool weHaveBeenSeen = false;
                    Entity targetEntity = targetData as Entity;
                    if (targetEntity != null)
                    {
                        if (targetEntity.CanSeeEntity(attacker))
                        {
                            weHaveBeenSeen = true;
                        }
                    }

                    if (weHaveBeenSeen)
                    {
                        return 0d;
                    }
                    else
                    {
                        return 1d;
                    }
                }
                
            }

        }



        /*   private void ScoreAttackTypeAgainstBodyPart(Entity attacker, Entity target, AttackJob attackJob, AttackType attackType, BodyPart bodyPart,
                                          BodyPart.AttackDirection attackDirection, float energyLevelFactor, Dictionary<BodyPartType, float> precomputedScores, ref double bestScore)
          {
              float currentScore = ScoreThisAttack(attacker, target, 
                  attackType, bodyPart, attackDirection, energyLevelFactor, precomputedScores);// = 0.5;

           
              if (currentScore > 0)
              {
                  // store the combo
                  List<WeaponInstanceCombo> listOfAttackCombos;
                  if (!currentAttackCombos.TryGetValue(attackJob, out listOfAttackCombos))
                  {
                      listOfAttackCombos = new List<WeaponInstanceCombo>();
                      currentAttackCombos.Add(attackJob, listOfAttackCombos);
                  }
                  listOfAttackCombos.Add(new WeaponInstanceCombo() { AttackType = attackType, BodyPart = bodyPart, Score = currentScore });
              }

              if (currentScore > bestScore)
              {
                  bestScore = currentScore;
              }*/

        /* if (bodyPart.BodyParts != null)
         {
             foreach (BodyPart bodyPart2 in bodyPart.BodyParts)
             {
                 if (bodyPart2.IsFunctional())
                 {
                     ScoreAttackTypeAgainstBodyPart(attacker, target, attackJob, attackType, bodyPart2, 
                         attackDirection, energyLevelFactor, precomputedScores, ref bestScore);
                 }
             }
         }
     }*/

        public double CombineJobAndWeaponScore(double jobScore, double? totalweaponsScore)
        {

            if (totalweaponsScore == null)
            {
                return 0.9 * jobScore;
            }
            else if (totalweaponsScore.Value == 0)
            {
                return 0;
            }
            double rating = 0.9 * jobScore + 0.1 * totalweaponsScore.Value;

            return rating;
        }



        private double ScoreDistribution(AttackJob threatJob)
        {
            // rate low if target is already getting attacked by others

            // how do we implement this... when we also want to take positions from others...
            /*   int noOfAttackers = 0;
               if (allegiance.SharedKnowledge.IsSeen(threatJob.EntityThreat))
               {
                   noOfAttackers = threatJob.EntityThreat.Intelligence.CombatInfo.MeleeAttackers.Count;                
               }

               noOfAttackers = Math.Max(noOfAttackers, threatJob.TakenBy.Count);

               return */

            return 0;
        }




        /// <summary>
        /// score high if the target is within the semi-circle that the agent is currently facing
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        private double ScoreInFieldOfView(Entity entity, IKnownEntityData targetData)
        {           
            // the dot product is positive if the angle between the two vectors is acute (less than 90 degrees)
            float dotProduct = Vector3.Dot(entity.FacingNormal, targetData.PlaySiteLocation - entity.PlaySiteLocation);

            // is more accuracy needed...? probably not. the Inertia weight will keep us shooting at the same target...
            if (dotProduct > 0)
            {
                return 1d;
            }
            else
            {
                return 0;
            }
        }

        private double ScoreMovingOutOfRange()
        {
            return 0;
        }

        private double ScoreClosestTarget()
        {
            return 0;
        }

        private double ScoreMinimalCollateralDamage()
        {
            return 0;
        }

        private double ScoreStationaryTarget(IKnownEntityData targetData)
        {
            //TODO DECOUPLE -- belongs in client
            Entity targetAsEntity = targetData as Entity;
            if (targetAsEntity != null && targetAsEntity.Locomotor.IsMoving())
            {
                return 0d;
            }
            else return 1d;
        }

        private double ScoreTimeToKillTarget()
        {
            return 0;
        }



        private double ScoreRetaliation(Entity attacker, IKnownEntityData targetData) // SharedKnowledge sharedKnowledge)
        {
            //score high if we are currently being attacked by this target
            // but only if we can see the entity...
            Entity targetAsEntity = targetData as Entity;
            if (targetAsEntity != null)// sharedKnowledge.IsSeen(Target))
            {
                if (targetAsEntity.Intelligence.CombatInfo.Target == attacker.EntityID)
                {
                    return 1d;
                }
            }

            return 0d;
        }

        private double ScoreInertia(Entity attacker, IKnownEntityData targetData)
        {
            //score high if we are currently attacking this target

            return attacker.Intelligence.Memory.GetLastAttackScore(targetData.EntityID);

        }

        /// <summary>
        /// we need to round to subtile positions to avoid a mismatch with how the melee positions are calculated.
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsCorrectMeleeDistanceRoundedToSubtiles(Entity attacker, Entity target)
        {
            // if we are attacking a moving target, let's add a displacement amount equal to movement over 0.5 s.

            Vector3 targetLocation;
            if (target.Locomotor.IsMoving() == true)
            {
                Vector3 offset = target.Locomotor.MoveSpeed * target.FacingNormal * 0.5f;
                targetLocation = target.PlaySiteLocation + offset;
            }
            else
            {
                targetLocation = target.PlaySiteLocation;
            }

            Point attackerSubtile = MapManager.WorldPosToSubtile(attacker.PlaySiteLocation);
            Point targetSubtile = MapManager.WorldPosToSubtile(targetLocation);

          
            if (Math.Abs(Common.DistanceOctile(attackerSubtile, targetSubtile) * MapManager.subTileSize - (attacker.EntityType.LocomotorType.MeleeRadius + target.EntityType.LocomotorType.MeleeRadius))
                <= GameData.Instance.AIConstants.Combat.DistanceToleranceInMeleeCombat)
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// TODO: perhaps we also need an outer reach limit for attacking...
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="correctMeleeDistance"></param>
        /// <returns></returns>
        public static bool IsCorrectMeleeDistance(float distance, float correctMeleeDistance)
        {
            if (Math.Abs(distance - correctMeleeDistance)
                <= GameData.Instance.AIConstants.Combat.DistanceToleranceInMeleeCombat)
            {
                return true;
            }

            return false;
        }

        private double ScoreAdjacencyToTarget(Entity entity, IKnownEntityData targetData)//SharedKnowledge sharedKnowledge)
        {
            // give a big bonus if we are at striking range of the target
            Entity targetEntity = targetData as Entity;
            if (targetEntity != null) // sharedKnowledge.IsSeen(Target))
            {
                if (IsCorrectMeleeDistanceRoundedToSubtiles(entity, targetEntity))
                {
                    return 1d;
                }
            }

            return 0d;
        }


        private double ScoreRange(Entity entity, IKnownEntityData targetData, AttackType attackType)
        {
            // ranged attacks may get a penalty if the target is too close

            // we don't use 'optimal' range...

            Vector3 offsetToTarget = entity.PlaySiteLocation - targetData.PlaySiteLocation;

            double distanceToTargetSquared = offsetToTarget.LengthSquared();

            if (distanceToTargetSquared > attackType.MaxRangeSquared.Value)
            {
                return 0.5d; // score more than 0... we can always close with the target
            }

            if (attackType.MinRange.HasValue && distanceToTargetSquared < attackType.MinRange.Value)
            {
                return 0d;
            }

            return 1d;

        }

        private double ScoreThreatRating(ThreatJob threatJob)
        {
            return threatJob.ThreatRating;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.Target = sn.DoEnumNullable(Target);

            return this;
        }


    }
}
