using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Body;
using System.Xml.Serialization;
using UWGame.ClientSide.Renderables;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using UWGame.Client.Audio;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.ClientSide;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.AI.Goals;
using System.Diagnostics;
using UWGame.SimSide.SimEffects;
using UWGame.Steam;

namespace UWGame.SimSide.Combat
{
   // public enum DefenseRatings { None = 0, VeryLow = 1, Low = 3, Middle = 5, High = 7, Highest = 10 }
            
    [DebuggerDisplay("{KeyName}")]
    public class AttackType : IGameData, IXmlSerializable
    {
        public string KeyName { get; set; }

        public string Name { get; set; }

        public bool DeleteRecord
        {
            get;
            set;
        }

        public string Comments;
    
        public enum RangeTypes
        {
            Melee = 0, // hand to hand (default)
            Ray, //no projectile (bullet-like)
            Ballistic, //thrown projectile (grenade) 
            Rocket //propelled projectile (rpg) 
        }

       // public DefenseRatings? DefenseRating;
        public float? DefenseRating;
       
        //antiTwinkler works against twinklerPlating but not ordinary shell. Should give morale damage
        // antiThunderChicken is there so the demontree can easier kill the chickens without them running away
       // public enum DamageTypes { Sharp, Blunt, Piercing, Fire, Bite, AntiTwinkler, SmallAnimalGrapple } 
        public string Damage;

        [XmlIgnore]
        public DamageType DamageFinal;


        public float DamageMean;
        public float DamageStandardDeviation;


        public AreaAttack AreaAttack;

        /// <summary>
        /// an optional modifier to accuracy/to-hit chance
        /// </summary>
        public float AccuracyFactor = 1f;

        /// <summary>
        /// victim effects are not blocked by armour?
        /// </summary>
        public string[] EffectsOnVictim;

        [XmlIgnore]
        public List<EffectProfileType> FinalEffectsOnVictim;

        public BodyPartType[] DependsOn;

        public float DurationInSeconds;

        /// <summary>
        /// if a missed attack takes a different amount of time
        /// </summary>
        public float? MissDurationInSeconds;

        /// <summary>
        /// the time point at which we should connect with our target
        /// </summary>
        public float ActionPointInSeconds;

        public float? ElectricEnergyCost;

        public float? WeaponConditionDamageMaxFraction;
        public float? WeaponConditionDamageMinFraction;


        /// <summary>
        /// special effect that shows either a hit or miss on a ranged attack
        /// </summary>
        public BulletEffect BulletEffect;

        #region Additional effects, not affected by hit/miss roll

        public Effects StartEffects;
        public Effects ImpactEffects;
        public Effects ActionPointEffects;

        #endregion

        public SoundData ImpactSound;
        public SoundData ActionPointSound;
        public SoundData SoundAtStart;

       

        [XmlIgnore]
        public Dictionary<AgentActionHooks, List<ActionSets>> EventActions = new Dictionary<AgentActionHooks,List<ActionSets>>();

       
     
        public AnimModifier[] AnimationStates;

        [XmlIgnore]
        public List<AnimModifier> AnimationStatesList;


        public string UsesAmmo;

        [XmlIgnore]
        public EntityType UsesAmmoType;

        public int? RoundsToSpend;

       // public bool? AmmoRoundGetsDestroyed;

        public bool IsDownAttack = false;

        /// <summary>
        /// optional skill to use
        /// </summary>
        public string RequiredSkill;

        [XmlIgnore]
        public SkillType RequiredSkillType;

       
        
        public RangeTypes RangeType;

        /// <summary>
        /// 0-1
        /// the chance that the agent may rest for a bit after an attack
        /// 
        /// the default value is set in IntelligenceType
        /// </summary>
        public double? ChanceToRest;

        /// <summary>
        /// the rest time will be computed using a random normal distribution limited by these values
        /// 
        /// the default value is set in IntelligenceType
        /// </summary>
        public float? MaxRestTimeInSeconds;
        public float? MinRestTimeInSeconds;

        [XmlIgnore]
        public float? RestTimeMean;
        [XmlIgnore]
        public float? RestTimeStandardDeviation;


        public float? MinRange = null;
        public float? MaxRange = null;
        
        [XmlIgnore]
        public float? MinRangeSquared;
        [XmlIgnore]
        public float? MaxRangeSquared;


        [XmlIgnore]
        public float? ConditionDamageMean;

        [XmlIgnore]
        public float? ConditionDamageStandardDeviation;
              


        public override string ToString()
        {
            return Name ?? KeyName;
        }

        public void Initialize()
        {
            if (AnimationStates != null)
            {
                AnimationStatesList = AnimationStates.ToList();
            }

            if (MinRange.HasValue)
            {
                MinRangeSquared = MinRange.Value * MinRange.Value;
            }

            if (MaxRange.HasValue)
            {
                MaxRangeSquared = MaxRange.Value * MaxRange.Value;
            }

            if (WeaponConditionDamageMinFraction.HasValue && WeaponConditionDamageMaxFraction.HasValue)
            {
                Common.GetNormalDistributionFromMinMaxValues(WeaponConditionDamageMinFraction.Value, WeaponConditionDamageMaxFraction.Value, out ConditionDamageMean, out ConditionDamageStandardDeviation);
            }

            if (MinRestTimeInSeconds.HasValue && MaxRestTimeInSeconds.HasValue)
            {
                Common.GetNormalDistributionFromMinMaxValues(MinRestTimeInSeconds.Value, MaxRestTimeInSeconds.Value, 
                    out RestTimeMean, out RestTimeStandardDeviation);
            }

            if (AreaAttack != null)
            {
                AreaAttack.Initialize();
            }

            if (RequiredSkill != null)
            {
                RequiredSkillType = GameData.Instance.AllSkillTypes[RequiredSkill];
            }
        }

        public void PostLoadContentInitialize()
        {

            List<AttackTypeActionHook> eventActions;
            if (GameData.Instance.EventHooksByAttackType.TryGetValue(this, out eventActions))
            {
                // group actions by hook type:
                foreach (var item in eventActions)
                {
                    List<ActionSets> actions;
                    if (!this.EventActions.TryGetValue(item.Hook, out actions))
                    {
                        actions = new List<ActionSets>();
                        this.EventActions.Add(item.Hook, actions);
                    }

                    actions.Add(GameData.Instance.AllActionSets[item.ActionSetsKey]);
                }
            }


            DamageFinal = GameData.Instance.AllDamageTypes[Damage];

        }


      /*  public void LoadContent(ContentManager content)
        {
            if (ImpactSound != null)
            {

              //  ImpactSoundEffect =  //content.Load<SoundEffect>("Sounds\\" + ImpactSound);
            }

            if (ActionPointSound != null)
            {
                ActionPointSoundEffect = content.Load<SoundEffect>("Sounds\\" + ActionPointSound);
            }

            if (SoundAtStart != null)
            {
                SoundAtStartEffect = content.Load<SoundEffect>("Sounds\\" + SoundAtStart);
            }
        }*/

        public void PreInitValidate(ref List<string> listOfErrors) 
        {
           // EntityType.ValidateRequiredValue(ref listOfErrors, "Skill", RequiredSkill != null);

            if (RangeType != RangeTypes.Melee)
            {
                EntityType.ValidateRequiredValue(ref listOfErrors, "Max range", MaxRange.HasValue);
            }
        
        }



        public void PostInitValidate(ref List<string> listOfErrors) { }
        public void PostDataCompleteInitialize()
        {
            if (UsesAmmo != null)
            {
                UsesAmmoType = GameData.Instance.AllEntityTypes[UsesAmmo];
            }

            if (EffectsOnVictim != null)
            {
                FinalEffectsOnVictim = new List<EffectProfileType>();
                foreach (var item in EffectsOnVictim)
                {
                    FinalEffectsOnVictim.Add(GameData.Instance.AllEffectProfileTypes[item]);
                }
            }
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors) { }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
        }

        #region Attack code

        /// <summary>
        /// handles collateral damage in area attacks as well.
        /// </summary>
        /// <param name="attackerID"></param>
        /// <param name="job"></param>
        /// <param name="attackType"></param>
        /// <param name="ownerOfCarcass"></param>
        /// <param name="bodyPartToAttackID"></param>
        /// <param name="targetAsEntity"></param>
        /// <returns></returns>
        public bool HitTargets(/*EntityID? attackerID*/ Entity attacker, AttackJob job, OwnerID? ownerOfCarcass, BodyPartID bodyPartToAttackID, Entity targetAsEntity)//Entity targetAsEntity, bool killedTarget)
        {
            if (AreaAttack != null)
            {
                DoAreaAttack(attacker, job, targetAsEntity, ownerOfCarcass);
            }

            bool killedTarget = HitTarget(attacker.ID, job, ownerOfCarcass, bodyPartToAttackID, targetAsEntity);
            return killedTarget;
        }

        /// <summary>
        /// hit other enemies that happen to stand in the area.
        /// </summary>
        /// <param name="targetAsEntity"></param>
        private void DoAreaAttack(Entity entity, AttackJob job, Entity targetAsEntity, OwnerID? ownerOfCarcass)
        {
            List<Entity> agentsInsideArea = AreaAttack.GetEntitiesInArea(entity.PlaySiteLocation, entity.FacingNormal.ToVector2());

            if (agentsInsideArea != null)
            {
                // remove friendlies:
                agentsInsideArea.RemoveAll(e => e == entity
                                                || e == targetAsEntity ||
                                                (AreaAttack.DamageOtherAllegianceMembers == false && e.AllegianceID == entity.AllegianceID));


                foreach (var enemy in agentsInsideArea)
                {
                    // find a body part:
                    BodyPart.AttackDirection attackDirection = GoalDoAttack.GetAttackDirection(entity, enemy.PlaySiteLocation, enemy.Rotation);
                    BodyPart hitBodyPart = enemy.Body.GetRandomBodyPartToHit(attackDirection);

                    HitTarget(entity.EntityID, job, ownerOfCarcass, hitBodyPart.BodyPartID /* bodyPartToAttackID*/, enemy);
                }
            }
        }

        /// <summary>
        /// Returns true if the target entity has died.
        /// </summary>
        /// <param name="attackerID"></param>
        /// <param name="job"></param>
        /// <param name="attackType"></param>
        /// <param name="ownerOfCarcass"></param>
        /// <param name="bodyPartToAttackID"></param>
        /// <param name="targetAsEntity"></param>
        /// <returns></returns>
        private bool HitTarget(EntityID? attackerID, AttackJob job, OwnerID? ownerOfCarcass, BodyPartID bodyPartToAttackID, Entity targetAsEntity)
        {
            float damage;

            float resistance;
            float reductionConstant;
            BodyLayerType armorLayer;

            Entity attacker = null;
            if (attackerID.HasValue)
            {
                attacker = Entity.FindByID(attackerID.Value);
            }

            // only reduce damage for melee attacks!
            float energyLevelFactor = GoalDoAttack.GetEnergyLevelFactorOnDamage(attacker, this);

            BodyPart bodyPartToAttack = targetAsEntity.Body.FindBodyPart(bodyPartToAttackID);

            ComputeDamage(bodyPartToAttack.BodyPartType, out damage, out resistance, out reductionConstant, out armorLayer, true, energyLevelFactor);

            // damage *= ammoUseDamageFactor;

            StartImpactEffects(targetAsEntity, attacker);

            //  damage = 0; // DEbugging only!!!

            string attackKeyName = ""; // for debuggiong
#if DEBUG || PROFILE
            attackKeyName = KeyName;
#endif


            if (damage > 0)
            {
                damage = bodyPartToAttack.DoDamage(damage);

                // hit the target once
                // job.Entity.BiologicalEntity.Hitpoints -= 10;

                if (attacker != null)
                {
                    // Log.Instance.Add(Log.Instance.CombatEvent, job.Entity, "", "was hit by " + entity + " on the " + bodyPartToAttack.BodyPartType.Name.ToLower() + ".", null);

                    The.Client.AddLogEvent(The.Client.Log.CombatEvent, attacker, string.Format("hit {0} on the {1} for {2} damage.{3}",
                    targetAsEntity.ToLink(), bodyPartToAttack.BodyPartType.Name.ToLower(Config.Culture), (int)damage, attackKeyName));

                }

            }
            else
            {
                if (reductionConstant > 0)
                {
                    if (attacker != null)
                    {

                        The.Client.AddLogEvent(The.Client.Log.CombatEvent, attacker, string.Format("hit {0} on the {1} for 0 damage. {2} gave protection.{3}",
                        targetAsEntity.ToLink(), bodyPartToAttack.BodyPartType.Name.ToLower(Config.Culture), armorLayer.Name, attackKeyName));
                    }
                }
            }

            if (FinalEffectsOnVictim != null)
            {
                foreach (var item in FinalEffectsOnVictim)
                {
                    targetAsEntity.SimEffects.Start(item);
                }
            }

            // even unsuccessful attacks can scare opponents
            targetAsEntity.Intelligence.DamageMorale(damage, bodyPartToAttack);

            // see if it's dead
            bool isDead;
            bool isUnconscious;

            CauseOfDeath? causeOfDeath;
            CauseOfUnconsciousness? causeOfUnconsciousness;

            targetAsEntity.GetStatus(out isDead, out isUnconscious, out causeOfDeath, out causeOfUnconsciousness);

            if (isDead || isUnconscious)
            {
                if (isDead)
                {
                    /*if (attacker != null)
                    {*/
                    // NEW: support non-agent attacks, and scripted attacks as well
                    FireEventActionsWhenKillingTarget(job, targetAsEntity, attacker);

                    if (attacker != null)
                    {
                        The.Client.LogKilledTarget(targetAsEntity, attacker);

                    }
                }

                CheckAchievements(attacker, targetAsEntity);

                targetAsEntity.SendMessage(new Message(attacker, Message.MessageTypes.HitAndCollapse, ownerOfCarcass));
            }
            else
            {
                FireEventActionsWhenHitting(job, targetAsEntity, attacker);


                // send message to target to play hit animation (this will possibly interrupt any attack they may be performing!)
                targetAsEntity.SendMessage(new Message(attacker, Message.MessageTypes.Hit, damage));
            }

            // wait for the rest of the animation to play out:

            //  timeToWait = GetAttackTypeDurationOrDefault() - GetAttackTypeActionPointOrDefault();

            if (ImpactSound != null)
            {
                // attach the impact sound to the target:
                targetAsEntity.Renderable.PlayActionSound(ImpactSound);
            }

            if (isDead)
            {
                return true;
            }

            return false;
        }


        void CheckAchievements(Entity attacker, Entity killedTarget)
        {
            if (attacker == null || attacker.Intelligence == null || attacker.Intelligence.Allegiance.AllegianceType != Allegiances.AllegianceType.Player)
            {
                return;
            }

            if (The.Sim.StartGameParams.GetRGScenario() == Scenarios.StartGameParams.RGScenario.TwinklerIsland)
            {
                if (The.Sim.GetDifficultyKey() == "normal" || The.Sim.GetDifficultyKey() == "easy")
                {
                    StatsAndAchievements ach = The.Sim.Controller.StatsAndAchievements;

                    if (!ach.IsAchievementUnlocked(AchievementID.improviser))
                    {
                        if (killedTarget.EntityType.KeyName == "entity:twinkler"
                            && this.KeyName == "shootImprovedFireExtinguisherBushDragonPoison")
                        {
                            ach.UnlockAchievement(AchievementID.improviser);
                        }

                    }
                }
            }
        }


        // we do some pre-computation (once) to map all attack types against different body parts of entity types
        // as an optimization.... cache for use at runtime 
        public void ComputeDamage(BodyPartType bodyPart,
            out float damage, out float resistance, out float reductionConstant, out BodyLayerType armorLayer, bool doAttackRoll, float energyFactor, float? attackDamageToUse = null)
        {
            armorLayer = null;

           // BiologicalBodyPartType bioBodypart = bodyPart as BiologicalBodyPartType;

            resistance = 0f;
            reductionConstant = 0f;


            if (bodyPart.ArmorLayerType != null)
            {
                armorLayer = bodyPart.ArmorLayerType;

                resistance = bodyPart.ArmorLayerType.DamageReductionFactorFinal[DamageFinal];
                reductionConstant = bodyPart.ArmorLayerType.DamageReductionConstantFinal[DamageFinal];
            }

            float attackDamage;
            if (doAttackRoll)
            {
                attackDamage = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(DamageMean, DamageStandardDeviation);
            }
            else
            {
                attackDamage = attackDamageToUse.Value; // attackType.DamageMean;                
            }


            damage = energyFactor * attackDamage * (1f - resistance) - reductionConstant;
            damage = Common.ClampBottom(damage, 0f);
        }

        private void FireEventActionsWhenKillingTarget(AttackJob job, Entity targetAsEntity, Entity attacker)
        {
            FireEventActions(job, targetAsEntity, attacker, AgentActionHooks.KilledEnemy, AgentActionHooks.KilledPrey);

        }

        public void FireEventActions(AttackJob job, Entity targetAsEntity, Entity attacker,
            AgentActionHooks enemyHook, AgentActionHooks preyHook, AgentActionHooks? anyHook = null)
        {

            AgentActionHooks hookToUse;
            if (job != null)
            {
                if (job is ThreatJob)
                {
                    hookToUse = enemyHook;
                }
                else
                {
                    hookToUse = preyHook;
                }
            }
            else if (anyHook.HasValue) // NEW: support event-driven attacks without a job.
            {
                hookToUse = anyHook.Value;
            }
            else return;

            // we support attacks by non-agents now.
            Dictionary<AgentActionHooks, List<ActionSets>> defaultEventActions = null;
            if (attacker != null && attacker.EntityType.IntelligenceType != null)
            {
                defaultEventActions = attacker.EntityType.IntelligenceType.EventActions;
            }

            // fire triggers, events etc:
            Goal.FireEventActions(attacker, targetAsEntity.EntityID,
                hookToUse, defaultEventActions,
                hookToUse, EventActions);
        }

        private void FireEventActionsWhenHitting(AttackJob job, Entity targetAsEntity, Entity attacker)
        {
            FireEventActions(job, targetAsEntity, attacker, AgentActionHooks.HitEnemy, AgentActionHooks.HitPrey);

        }

        #endregion

        #region Effects

        public void StartStartEffects(Entity targetAsEntity, Entity attacker)
        {
            if (StartEffects != null)
            {
                BeginStartEffects(StartEffects.ParticleEmitters, attacker);
            }

            if (SoundAtStart != null && attacker != null)
            {
                attacker.Renderable.PlayActionSound(SoundAtStart);
            }
        }

        public void StartActionPointEffects(Entity targetAsEntity, Entity attacker, bool willHitTarget)
        {
            if (attacker != null)
            {
                MapClient.StartBulletEffect(BulletEffect,
                           MaxRange, targetAsEntity, attacker, willHitTarget);

                if (ActionPointEffects != null)
                {
                    BeginStartEffects(ActionPointEffects.ParticleEmitters, attacker);
                }

                if (ActionPointSound != null)
                {
                    attacker.Renderable.PlayActionSound(ActionPointSound);
                }
            }
        }

        public void StartImpactEffects(Entity targetAsEntity, Entity attacker)
        {
            if (ImpactEffects != null)
            {
                BeginStartEffects(ImpactEffects.ParticleEmitters, attacker);
            }
        }

        private void BeginStartEffects(ParticleEmitterEffect[] effects, Entity entityToAttachTo)
        {
            foreach (var item in effects)
            {
                // set the emitter on the renderable, or at the renderable's location
                if (item.AttachToEntity)
                {
                    The.Client.ParticleManager.AddEmitter(item.ParticleSystemKey, entityToAttachTo.Renderable,
                     null, null, item.EmitParticlesInParentDirection, item.DurationInSeconds, item.Offset);
                }
                else
                {
                    The.Client.ParticleManager.AddEmitter(item.ParticleSystemKey, entityToAttachTo.Renderable.Location.Value.ToVector2(), // targetAsEntity.Renderable, 
                        null, null, item.DurationInSeconds, item.Offset);
                }
            }
        }



        #endregion

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
        }

       
        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(AttackType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true) //.Add(
            
        };

        
        #endregion
    }


    public class Effects
    {
        /// <summary>
        /// particles associated with the anim
        /// </summary>
        public ParticleEmitterEffect[] ParticleEmitters;
    }

    public class BulletEffect
    {
        public float MuzzleDistance = 0f;

        public Color StartColor = Color.Gray;
        public Color EndColor = Color.LightGray;

    }
}
