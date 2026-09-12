using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using Xclna.Xna.Animation;
using UWGame.SimSide.Entities.Body;
using GameStateManagement;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Renderables;//TODO DECOUPLE
using UWGame;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Entities.Containers;
using UWGame.ClientSide;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Systems.Triggers;
using Microsoft.Xna.Framework.Audio;
using UWGame.Control;
using UWGame.Client.Particles;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Containers.Components;


namespace UWGame.SimSide.AI.Goals
{
    
    class GoalDoAttack: CompositeGoal
    {
        public AttackJob job;
        JobID? snapshotJob;

        private OwnerID? ownerOfCarcass;
        GoalID snapshotParentGoal;
        GoalAttack parentGoal;
        AttackType attackType;
        BodyPartID bodyPartToAttackID;
        EntityAndRoot? weapon;

    //    private List<ReplenishItemsForAction> replenishActions;
        // we roll to hit at the beginning in order to play the correct hit/miss animation from the start.
        bool willHitTarget = false;
       
        bool hasCompletedFirstPhase = false;
        bool hasCompletedRestPhase = false;

        public GoalDoAttack(Entity entity, AttackJob job, OwnerID? ownerOfCarcass, AttackType attackType, /*BodyPart bodyPartToAttack*/BodyPartID bodyPartToAttackID, EntityAndRoot? weapon, GoalAttack parentGoal)
            : base(entity)
        {
            
            this.job = job;
            this.parentGoal = parentGoal;
            //replenishActions = parentGoal.ReplenishActions;
            
          //  this.replenishActions = replenishActions;
            this.ownerOfCarcass = ownerOfCarcass;
            this.attackType = attackType;
            //this.bodyPartToAttack = bodyPartToAttack;
            this.bodyPartToAttackID = bodyPartToAttackID;
            this.weapon = weapon;
            if (weapon != null)
            {
                int i = 0;
            }
        }

      


        public GoalDoAttack()
        {
        }     
            

         

        protected override void Activate()
        {
            if (weapon != null)
            {
                Entity item;
                if (EntityIsNotSeenDirectly(weapon.Value.Entity, out item))
                {
                    return;
                }
            }

            // we can't attack into the fog of war. even homing missiles need to see the target...
            Entity targetEntity;
            BodyPart targetBodyPart;
            if (ArePreconditionsOK(out targetEntity, out targetBodyPart))
            {

                if (AreWeStandingInAStack(targetEntity))
                {
                    Status = Goals.Status.Failed;

                    return;
                }

                Status = Status.Active;

                BodyPart.AttackDirection attackDirection = GetAttackDirection(entity, targetEntity.PlaySiteLocation, targetEntity.Rotation);
                     
                // probably only hunt needs this...
                entityIntelligence.Memory.SetLastAttackTarget(targetEntity.EntityID);

                // NEW: mount/unmount weapon:
                if (entity.AgentStorage != null)
                {
                    entity.AgentStorage.MountedToolOrWeapon = EntityAndRoot.GetEntity(weapon);
                }

                  // we roll to hit at the beginning in order to play the correct hit/miss animation from the start.
                BodyPart hitBodypart = null;
                if (RollToHit(entity, targetEntity, attackType, targetBodyPart, attackDirection, out hitBodypart))
                {
                    willHitTarget = true;
                                       

                    SetFirstPartAnimationState();

                }
                else
                {
                    // play miss anim:
                    willHitTarget = false;
                                         
                    // has a miss anim been defined for this attack?
                    float missDuration = attackType.MissDurationInSeconds ?? GetAttackTypeDurationOrDefault();
                    
                    AddSubgoal(new GoalWait(entity, missDuration, AnimAction.Attacking, AnimModifier.Fail, true));  

                }

                attackType.StartStartEffects(targetEntity, entity);                

            }
            else
            {
                Status = Status.Failed;
            }
        }


        private float GetMissDuration()
        {
            return 0.72f;
        }

        private void SetFirstPartAnimationState()
        {
            // the flags are cleared in GoalDoAttack.OnExit!

            float actionPoint = GetAttackTypeActionPointOrDefault();
            // no scaling (we only play half the anim)... so default duration will look very odd
            AddSubgoal(new GoalWait(entity, actionPoint, AnimAction.Attacking, attackType.AnimationStatesList, false, GoalWait.OnExitFlagAction.Leave));
        }

        private float GetAttackTypeActionPointOrDefault()
        {
            if (attackType.ActionPointInSeconds > 0f)
            {
                return attackType.ActionPointInSeconds;
            }
            else return 0.5f;
        }

        private float GetAttackTypeDurationOrDefault()
        {
            if (attackType.DurationInSeconds > 0f)
            {
                return attackType.DurationInSeconds;
            }
            else return 1f;
        }

        /// <summary>
        /// Wake up nearby allies so they can assist in the fight
        /// </summary>
        private void WakeUpNearbyAllies()
        {
            //If the fight is nearby a sleeping agent he should wake up to help out.
            //This will be done by using an set range in constants
            //And in the predicate we will check if the entity is sleeping.
            //If this is true we will send them an wakeup message.
                      
            ThreatJob threatJob = job as ThreatJob;
            if (threatJob != null && !threatJob.IsVermin)
            {
                //var agents = entity.Intelligence.Allegiance.Members;

                List<Pair<Entity, Vector2>> agents = new List<Pair<Entity,Vector2>>();

                //perhaps the waking range should increase over time as the fight drags on
                The.AgentQuadTree.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), GameData.Instance.AIConstants.Combat.CombatWakeUpRange,
                    e => e != entity
                        && e.Intelligence.Allegiance == entityIntelligence.Allegiance &&
                           e.Intelligence.IsSleeping(),
                    ref agents);
                              
                foreach (var agent in agents)
                {
                   /* if (agent.Intelligence.IsSleeping())
                    {*/
                        agent.First.SendMessage(new Message(Message.MessageTypes.WakeUpCombatAlert));                       
                   // }
                }
            }

        }
        /// <summary>
        /// make sure that everyone sees us now that we have broken cover
        /// </summary>
        private void SetDetectedByAllWatchers()
        {
            TerrainTile tile = The.Map.GetTile(entity.MapPosition.Value);

            if (tile.EntitiesThatSeeThisTile != null)
            {
                foreach (var e in tile.EntitiesThatSeeThisTile)
                {
                    if (e != entity)
                    {
                        
                        try
                        {
                            e.SendMessage(new Message() { MessageType = Message.MessageTypes.AlertToPresence, Sender = entity });
                        }
                        catch (Exception ex)
                        {

                            string exceptionString = ex.Message;
                            exceptionString += Entity.GetExceptionInformation(e);
                            throw new Exception(exceptionString);

                        }
                        
                    }
                }

            }

            
        }

      

        /// <summary>
        /// Always called BEFORE the OnEnter of a next subgoal of the same parent
        /// </summary>
    /*    public override void OnExit()
        {
            base.OnExit();

            //ClearAttackAnimFlags();

        }*/

        public override void Deactivate()
        {
            ClearAttackAnimFlags();
        }

        private void ClearAttackAnimFlags()
        {
            // don't clear a flag for a mounted weapon:
            AnimModifier[] attachedFlags = null;

            if (entity.AgentStorage != null)
            {
                EntityID? mountedWeapon = entity.AgentStorage.MountedToolOrWeapon;

                if (mountedWeapon.HasValue)
                {
                    Entity weapon = Entity.FindByID(mountedWeapon.Value);
                    if (weapon != null)
                    {
                        attachedFlags = weapon.EntityType.ItemType.AnimStatesWhenAttached;
                    }
                }
            }

            entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Attacking);

            foreach (AnimModifier state in attackType.AnimationStates)
            {
                if (attachedFlags == null
                    || !Array.Exists(attachedFlags, f => f == state))
                {
                    entity.Renderable.ClearAnimationStateFlag(state);
                }
            }
        }



        private bool AreWeStandingInAStack(Entity targetEntity)
        {         
            // is this still necessary after collision responses were implemented???

            // TODO: prevent stacking of larger creatures
   
            // if we are no longer chasing, see if we are in a stack:
            if (!targetEntity.Locomotor.IsMoving())
            {
                if (targetEntity.Intelligence.CombatInfo.Target == entity.EntityID)
                {
                    // if our target is not moving, and it is fighting back, we ignore stacks. We won't be interrupted.
                    // UNLESS we are standing on top of our target.
                    if (MapManager.WorldPosToSubtile(entity.PlaySiteLocation) == MapManager.WorldPosToSubtile(targetEntity.PlaySiteLocation))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return MapManager.IsStandingOnNonMovingEntity(entity);
            }

            return false;
        }


       
        protected bool ArePreconditionsOK(out Entity targetEntity, out BodyPart bodyPart)
        {
            bodyPart = null;

            if (EntityIsNotSeenDirectly(job.Target.Value, out targetEntity))
            {
                return false;
            }
           
            bool isInRange;

            isInRange = IsInRange(entity, targetEntity, attackType);


            bodyPart = targetEntity.Body.FindBodyPart(this.bodyPartToAttackID);
           

            return isInRange;

                /*EvaluateThreatJobs.IsCorrectMeleeDistance(entity, job.Entity) && 
                GoalTurnToFace.IsFacing(entity, job.Entity.Location.ToVector2(), out rota, out closestCorner);*/
        }

        public static bool IsInRange(Entity entity, IKnownEntityData targetData, AttackType attackType)
        {
            bool isInRange;
            if (attackType.RangeType == AttackType.RangeTypes.Melee)
            {
                Entity targetEntity = targetData as Entity;
                if (targetEntity == null)
                {
                    isInRange = false;
                }
                else
                {
                    isInRange = IsInStrikingPosition(entity, targetEntity);
                }
            }
            else
            { // ranged case

                Vector3 offsetToTarget = entity.PlaySiteLocation - targetData.PlaySiteLocation;
                double distanceToTarget = offsetToTarget.Length();

                //  --  Change 2014-08-06  -- 
                //TODO: Perhaps use Common.DistanceOctile in comparison.
                //
                //distanceToTarget and MaxRange was compared in squared forms before but the tolerance variable was then less effective
                //the further away from the target we where so at max rifle range it was not enough.
                //Changed it to non squared variables and it seems to be working fine now.
                //Also reduced the distance check from 40 pixels to 15 from 40 as that should be enough in theese cases now.


                if (distanceToTarget < attackType.MaxRange + 15) // give a little tolerance here... 15 pixels
                {
                    isInRange = true;
                }
                else isInRange = false;
               

            }
            return isInRange;
        }

        public static bool IsInStrikingPosition(Entity attacker, Entity target)
        {
            float rota;
            int closestCorner;

            return AttackJob.IsCorrectMeleeDistanceRoundedToSubtiles(attacker, target) &&
                GoalTurnToFace.IsFacing(attacker, target.PlaySiteLocation.ToVector2(), out rota, out closestCorner, 0.3f);
        }

        /// <summary>
        /// damages the weapon itself
        /// </summary>
        /// <param name="hasHit"></param>
        private void DamageWeaponCondition(bool hasHit)
        {
            if (weapon != null && attackType.ConditionDamageMean.HasValue)
            {
                Entity weaponEntity = Entity.FindByID(weapon.Value.Entity);

                if (weaponEntity != null)
                {

                    float conditionDamage = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(attackType.ConditionDamageMean.Value, attackType.ConditionDamageStandardDeviation.Value);

                    if (attackType.RangeType == AttackType.RangeTypes.Melee && !hasHit)
                    {
                        // deal reduced damage to melee weapons if we missed:
                        conditionDamage *= 0.3f;
                    }

                    if (conditionDamage > 0f)
                    {
                        // damage the weapon parts               
                        bool partWasDestroyed = weaponEntity.DoDamage(conditionDamage); //, true);


                        if (partWasDestroyed)
                        {


                            The.Client.AddLogEvent(entityIntelligence.Allegiance, The.Client.Log.CombatEvent, entity, "A " + weaponEntity.EntityType.Name.ToLower(Config.Culture) + " broke while " + entity + " was using it.");
                            


                        }

                        //  tool.Item.DoConditionDamage((float)(elapsedTime * destructibility * degradeFactor), rollForChanceToDestroy); // rollForChanceToDestroy);
                    }
                }
            }
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {

            //process the subgoals       

            if (ProcessSubgoals(elapsed) == Goals.Status.Completed)
            {
                if (!ValidateSafetyAndTakeAction(null))
                {
                    // ExitIfFailedOrCompleted();
                    return; //return Status;
                }

                if (!hasCompletedFirstPhase)
                {
                    // action point reached.

                    hasCompletedFirstPhase = true;

                    Entity targetAsEntity = Entity.FindByID(job.Target.Value);
                    if (targetAsEntity == null)
                    {
                       
                        Status = Goals.Status.Failed; // new!

                        return; 
                    }

                    // log this and show as an alert:
                    The.Client.AddLogEvent(The.Client.Log.CombatEvent, entity,
                        string.Format("is attacking {0}!", targetAsEntity.ToLink()),
                        ClientSide.Log.Priority.High);


                    

                    if (targetDied)
                    {
                        Status = Goals.Status.Completed;                       
                        return; 
                    }

                    double timeToWait = 0;

                    Status = Status.Active;

                    IKnownEntityData targetData;

                    if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Target.Value, out targetData)))
                    {
                        // avoid 'killing' the target twice... also, if the target does not exist, we don't know where to shoot the bullet. so just abort.  
                       
                        return; 
                    }


                    bool killedTarget = false;
                    if (!HandleWeaponAndAmmo(targetAsEntity))
                    {
                       
                        return; 
                    }


                    if (willHitTarget && attackType.RangeType != AttackType.RangeTypes.Ballistic)
                    {
                        killedTarget = attackType.HitTargets(entity, job, ownerOfCarcass, bodyPartToAttackID, targetAsEntity);

                        if (killedTarget)
                        {                            
                            DestroyJobAndRemoveLocks(ref job, null, null, parentGoal.GetReplenishActions(), weapon);
                        }

                        // wait for the rest of the animation to play out:
                        timeToWait = GetAttackTypeDurationOrDefault() - GetAttackTypeActionPointOrDefault();

                    }

                    if (!willHitTarget)
                    {
                        // fire any miss target event actions:
                        FireEventActionsWhenMissed(targetAsEntity);
                    }

                    DamageWeaponCondition(willHitTarget);
                    
                   
                    attackType.StartActionPointEffects(targetAsEntity, entity, willHitTarget);


                    //If the target has died or dies during this attack we dont want to add more subgoals to this goal.
                    //At this point the job can have been removed but we need to run the rest of the code after HitTarget to play sounds etc. (?!?! which ones?)
                    if (killedTarget)
                    {
                        Status = Status.Completed; // NEW: prevents parent goal (GoalHunt > GoalAttack) from seeing the destroyed AttackJob and reporting a Failed goal. Are there problems with it???

                    }
                    else if (timeToWait > 0 && killedTarget == false)
                    {
                        // wait for the attack cycle to complete:
                        AddSubgoal(new GoalWait(entity, timeToWait));

                        //Wake up nearby allies so they can help with the ongoing fight.
                        WakeUpNearbyAllies();
                    }
           
                }
                else if (!hasCompletedRestPhase)
                {
                    // if everything went well:
                    hasCompletedRestPhase = true;

                    if (!SetRestPeriodAfterAttack())
                    {
                        Status = Goals.Status.Completed;
                    }
                }
                else
                {

                    Status = Status.Completed;
                }
            }

            //everyone that can see our tile will see us now:
            SetDetectedByAllWatchers();

            //Wake up nearby allies so they can help with the ongoing fight.
          //  WakeUpNearbyAllies();

        }

     

        private void FireEventActionsWhenMissed(Entity targetAsEntity)
        {
            attackType.FireEventActions(job, targetAsEntity, entity, AgentActionHooks.MissedAnAttackOnAnEnemy, AgentActionHooks.MissedAnAttackOnPrey);    
        }

        /// <summary>
        /// sometimes, let the agent rest a bit before the next attack - looks better than continous pounding
        /// </summary>
        private bool SetRestPeriodAfterAttack()
        {
            double? chanceToRest = null;
            if (attackType.ChanceToRest != null)
            {
                chanceToRest = attackType.ChanceToRest.Value;
            }
            else
            {
                // use the default (if set):
                if (attackType.RangeType == AttackType.RangeTypes.Melee)
                {
                    chanceToRest = entity.EntityType.IntelligenceType.ChanceToRestAfterMeleeAttack;
                }
                else
                {
                    chanceToRest = entity.EntityType.IntelligenceType.ChanceToRestAfterRangedAttack;
                }
            }

            if (chanceToRest.HasValue)
            {
                float? meanTimeToRest;
                float? stdDevTimeToRest;

                if (attackType.RestTimeMean != null)
                {
                    meanTimeToRest = attackType.RestTimeMean.Value;
                    stdDevTimeToRest = attackType.RestTimeStandardDeviation.Value;
                }
                else
                {
                    // use the default (if set):
                    meanTimeToRest = entity.EntityType.IntelligenceType.RestTimeAfterAttackingMean;
                    stdDevTimeToRest = entity.EntityType.IntelligenceType.RestTimeAfterAttackingStandardDeviation;
                   
                }


                if (meanTimeToRest.HasValue &&
                    stdDevTimeToRest.HasValue &&
                    The.Sim.GameplayRandomGenerator.NextDouble(null) < chanceToRest.Value)
                {
                    double waitPeriod = The.Sim.GameplayRandomGenerator.RandomNormalDistribution(meanTimeToRest.Value, stdDevTimeToRest.Value);

                    AddSubgoal(new GoalWait(entity, waitPeriod, AnimAction.Idle, AnimModifier.Bold, false, GoalWait.OnExitFlagAction.Clear));

                    // clear these now (not Bold!) to produce an idle anim:
                    ClearAttackAnimFlags();

                    return true;
                }
            }

            return false;
        }

       

   

        private bool HandleWeaponAndAmmo(Entity target)
        {
            if (weapon.HasValue)
            {
                Entity weaponEntity = Entity.FindByID(weapon.Value.Entity);
                if (weaponEntity != null)
                {
                    // spend ammo:
                    if (!SpendAmmo(weaponEntity))
                    {
                        Status = Goals.Status.Failed;
                        return false; // Status;
                    }

                    if (attackType.RangeType == AttackType.RangeTypes.Ballistic)
                    {
                        // let the projectile entity fly
                        //entity.AgentStorage.Remove(weaponEntity, null);
                        entity.AgentStorage.Uncontain(weaponEntity);

                        if (weaponEntity.Locomotor != null) // should we give all items a ballistic locomotor by default?
                        {

                            weaponEntity.Locomotor.StartMoving(Locomotor.Mode.Ballistic, target.PlaySiteLocation, 380f, 0f, 
                                entity.EntityID, entityIntelligence.Allegiance, attackType, ownerOfCarcass, job); // lead moving targets???
                        }
                        // whether we hit or not will be decided when the projectile reaches the target location
                    }
                }
                else
                {
                    Status = Goals.Status.Failed;
                    return false;
                }
            }

            return true;
        }

        private bool SpendAmmo(Entity weaponEntity)
        {
            float ammoUseDamageFactor = 1f;
           
            // spend ammo:
            if (attackType.UsesAmmo != null)
            {                 
                 
                MagazineContainer magazine;
                magazine = (MagazineContainer)weaponEntity.Contains; // .Find(out magazine);

               
                int roundsSpent = magazine.SpendAmmo(attackType.UsesAmmoType, attackType.RoundsToSpend.Value, weaponEntity.GetOwner().OwnedEntities);

                if (roundsSpent == 0)
                {
                    // out of ammo?!?! this should rarely happen, only if the ammo degraded just as we were shooting
                    //cancel the attack:                  

                    return false;
                }
                else if (roundsSpent < attackType.RoundsToSpend.Value)
                {
                    // couldn't spend all the ammo in the burst - reduce the damage?
                    ammoUseDamageFactor = roundsSpent / attackType.RoundsToSpend.Value;

                }
                 
            }

            return true;
        }

        public static float GetEnergyLevelFactorOnDamage(Entity entity, AttackType attackType)
        {

            if (attackType.RangeType == AttackType.RangeTypes.Melee) // only for melee attacks...
            {
                return GetEnergyLevelFactorOnDamageForMelee(entity);
            }
            else return 1f;

        }

        public static float GetEnergyLevelFactorOnDamageForMelee(Entity entity)
        {
            float energyLevelFactor = 1f;

             // only for melee attacks...
            BiologicalEntity bioEntity;
            if (entity.Find(out bioEntity))
            {
                energyLevelFactor = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyMeleeDamageFactor, 1f, bioEntity.EnergyLevel);
            }

            return energyLevelFactor;
        }

        public override float GetExertionLevel()
        {
            if (attackType.RangeType == AttackType.RangeTypes.Melee)
            {
                return GameData.Instance.Constants.PhysicalWork.MeleeFighting; // PhysicalWork.Hard;
            }
            else return GameData.Instance.Constants.PhysicalWork.RangedFighting; // PhysicalWork.Light;
        }


        public override Goal.StealthFactor GetStealthFactor()
        {
            if (attackType.RangeType == AttackType.RangeTypes.Melee)
            {
                return StealthFactor.ExtremelyBad;
            }
            else return StealthFactor.NotGood;
        }

       

        public static BodyPart.AttackDirection GetAttackDirection(Entity attacker, Vector3 targetLocation, float targetRotation) // Entity target)
        {
            
            Vector3 distanceVector = targetLocation - attacker.PlaySiteLocation;
            distanceVector.Normalize();
            distanceVector *= -1f;
     
            float attackAngle = Common.VectorToAngle(distanceVector);
            attackAngle = attackAngle - targetRotation; // target.Locomotor.Rotation; 

            if (attackAngle < 0f)
            {
                attackAngle += MathHelper.TwoPi;
            }
         

            if (attackAngle > MathHelper.PiOver4 && attackAngle < MathHelper.PiOver4 + MathHelper.PiOver2)
            {
                return BodyPart.AttackDirection.Right;
            }
            else if (attackAngle > MathHelper.PiOver4 + MathHelper.PiOver2 && attackAngle < MathHelper.Pi + MathHelper.PiOver4)
            {
                return BodyPart.AttackDirection.Back;
            }
            else if (attackAngle > MathHelper.Pi + MathHelper.PiOver4 && attackAngle < MathHelper.Pi + MathHelper.PiOver2 + MathHelper.PiOver4)
            {
                return BodyPart.AttackDirection.Left;
            }
            else
            {
                return BodyPart.AttackDirection.Front;
            }

        }

        public static bool RollToHit(Entity entity, Entity target, AttackType attackType, BodyPart bodyPart, BodyPart.AttackDirection direction, out BodyPart hitBodyPart)
        {
            hitBodyPart = bodyPart;

            float chanceToHit =  ComputeChanceToHit(entity, target, bodyPart, direction, attackType);

            float random = The.Sim.GameplayRandomGenerator.RandomBetween(0f, 1f);


            if (random < chanceToHit)
            {
                return true;
            }
            else if (random < chanceToHit + 0.1f) // miss, but see if we could hit something else
            {
                hitBodyPart = bodyPart.Body.GetRandomBodyPartToHit(direction);
                return true;
            }

            return false;
        }


        public static float ComputeChanceToHit(Entity entity, IKnownEntityData target, BodyPartID bodyPartID, BodyPart.AttackDirection direction, AttackType attackType)
        {
            BodyPart bodyPartToHit = target.Body.FindBodyPart(bodyPartID);
            return ComputeChanceToHit(entity, target, bodyPartToHit, direction, attackType);
        }

        /// <summary>
        /// attacker is optional!
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="bodyPartToHit"></param>
        /// <param name="direction"></param>
        /// <param name="attackType"></param>
        /// <returns></returns>
        public static float ComputeChanceToHit(Entity attacker, IKnownEntityData target, BodyPart bodyPartToHit, BodyPart.AttackDirection direction, AttackType attackType)
        {
            
            // agent-dependent modifiers:
            float skill = 1f; 
            float toHitModifier = 1f;
            float distractedBonus = 0f;
            float energyLevelFactor = 1f;
            float distanceFactor = 1f;

            if (attacker != null)
            {
                toHitModifier = bodyPartToHit.GetToHitModifier(attacker.Bulk, direction);

                if (attackType.RequiredSkillType != null)
                {
                    skill = attacker.Intelligence.GetSkillValue(attackType.RequiredSkillType);
                }

                Entity targetAsEntity = target as Entity;
                if (targetAsEntity != null && targetAsEntity.Intelligence.CombatInfo.Target != attacker.EntityID) // is target preoccupied (fighting another?)
                {
                    distractedBonus = GameData.Instance.Constants.MeleeToHitBonusOnDistractedTarget;
                }

                BiologicalEntity bioEntity;
                if (attacker.Find(out bioEntity))
                {
                    energyLevelFactor = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyToHitFactor, 1f, bioEntity.EnergyLevel);
                }

                if (attackType.RangeType != AttackType.RangeTypes.Melee)
                {
                    float distance = Common.DistanceOctile(attacker.Location.Value, target.Location.Value);

                    if (distance < GameData.Instance.Constants.CloseDistanceForRangedAttack)
                    {
                        distanceFactor = GameData.Instance.Constants.CloseDistanceRangedAttackToHitFactor;
                    }
                }
            }

            float attackProneBonus = 0f;
            if (target.Stance != null && target.Stance.IsProne == true) // == LeggedLocomotor.Stance.Lying) // || target.Locomotor.MobileEntity.CurrentStance == MobileEntity.Stance.Sitting)
            {
                attackProneBonus = GameData.Instance.Constants.MeleeToHitBonusOnProneTarget;
            }
                                    

            float chanceToHit = distanceFactor * attackType.AccuracyFactor * energyLevelFactor * toHitModifier * (skill + distractedBonus + attackProneBonus); //???!!!???
           
            return chanceToHit;
        }





    
       


        bool targetDied = false;

        public override bool HandleMessage(Message message)
        {
            //first, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            //if the msg was not handled, test to see if this goal can handle it
            if (handled == false)
            {
                switch (message.MessageType)
                {
                    case Message.MessageTypes.Hit:

                        if (!hasCompletedFirstPhase)
                        {
                            // if we are performing the pre-hit phase of an attack, make it less likely that we are interrupted. (to prevent hit-starvation)
                            if (The.Sim.GameplayRandomGenerator.NextDouble("GoalDoAttack") < 0.6)
                            {
                                return true; // cancel hit anim for us, we continue the attack.
                            }
                        }

                        return false; // handle the hit in GoalThink

                        /*
                        float damageDone = ((float)message.OtherInfo);
                        Body body;
                        entity.Find(out body);
                        if (damageDone > GameData.Instance.Constants.DamageAmountFractionCausingHitReaction * body.MaxHitpoints)
                        {
                            // is this needed???
                            RemoveAllSubgoals();

                            AddSubgoal(new GoalBeingHit(entity));
                        }

                        return true;*/

                    case Message.MessageTypes.EntityDied:
                         Trigger sourceTrigger = (Trigger)message.OtherInfo;
                          
                          Tuple<EntityID, EntityType> info = (Tuple<EntityID, EntityType>)sourceTrigger.messageInfo;
                          EntityID entityID = info.Item1;
                          EntityType entityType = info.Item2;

                          if (this.job != null)
                          {
                              if (entityID == this.job.Target)
                              {
                                  // our target was killed - goal is Complete
                                  targetDied = true;
                              }
                          }
                          else
                          {
                              //We have ended the goal by killing our entity. Job has been removed.
                              targetDied = true;
                          }

                        return false; // handle in GoalAttack too

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

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.snapshotJob = sn.SnapshotID<Job, JobID>(job);
            this.ownerOfCarcass = sn.DoEnumNullable(ownerOfCarcass);
            this.attackType = sn.DoGameData(attackType);           
            this.bodyPartToAttackID = (BodyPartID)sn.DoEnum(bodyPartToAttackID);
            this.weapon = sn.DoEntityAndRootNullable(weapon);
            this.willHitTarget = sn.DoBool(willHitTarget);
            this.hasCompletedFirstPhase = sn.DoBool(hasCompletedFirstPhase);
            this.hasCompletedRestPhase = sn.DoBool(hasCompletedRestPhase);
            this.targetDied = sn.DoBool(targetDied);
            this.snapshotParentGoal = (GoalID)sn.SnapshotID<Goal, GoalID>(parentGoal);
            
            
            sn.Ignore(job);
            sn.Ignore(parentGoal);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            if (snapshotJob != null)
            {
                job = (AttackJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }
            parentGoal = (GoalAttack)LookUpGoals.FindByID(snapshotParentGoal);
            //replenishActions = parentGoal.ReplenishActions;

           
        }

        #endregion
    }
}
