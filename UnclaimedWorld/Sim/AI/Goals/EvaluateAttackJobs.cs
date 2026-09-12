using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;//MLo required for Distinct method
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.Body;
using GameStateManagement;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    

    // we have multiple levels of caching when scoring the combos - and a class for each level.
    /// <summary>
    /// this class is the full combo of weapon instance, attack type, body part target
    /// </summary>
    public class WeaponInstanceCombo : IEdge, IScore
    {
        // public AttackJob Job;

        public float Score { get; set; }
        public float Edge { get; set; }


        public IKnownEntityData Weapon;

        /// <summary>
        /// max one weapon entity will be a key
        /// </summary>
        public Dictionary<IKnownEntityData, List<Tuple<ProcessType, List<IKnownEntityData>>>> ReplenishItemsForWeapon;
       // public Dictionary<IKnownEntityData, List<Tuple<GoalReplenish.ReplenishAction, List<IKnownEntityData>>>> ReplenishItemsForWeapon;

     //   public Tuple<GoalReplenish.ReplenishAction, List<Entity>> ReplenishItemsForWeapon;

      
        public WeaponInstanceComboAttackData AttackData;

        /* public override string ToString()
         {
             return AttackType.ToString() + " " + BodyPart.ToString() + " Score: " + Score + ", Edge: " + Edge;
         }*/
    }

    /// <summary>
    /// the attack vs body part score, including chance to hit (skill). Shared bteween multiple Weapon combos.
    /// </summary>
    public class WeaponInstanceComboAttackData
    {

        public AttackType AttackType;

        //public BodyPart BodyPart;
        public BodyPartID bodyPartID;
        
        /// <summary>
        /// score excluding To Hit chance
        /// </summary>
        public float EstimatedDamageScore;


        public WeaponInstanceComboJobData JobData;

        /// <summary>
        /// score for the job including ToHit chance, but excluding the weapon score
        /// </summary>
        public double? AttackScore;
    }

    /// <summary>
    /// these data are common for the combos for attacks against the same entity (job). Shared between multiple Attack type scoring.
    /// </summary>
    public class WeaponInstanceComboJobData
    {
        public AttackJob Job;

        public BodyPart.AttackDirection? AttackDirection;

        //public double? TravelTimeToTargetScore;
       
    }

    public struct ReplenishWeaponStatus
    {
        // we can add more state variables here, such as for power cells etc.


        /// <summary>
        /// do we have the replenish item in the inventory? (does not consider accessibility!)
        /// </summary>
        public bool? OwnsAmmo;

        /// <summary>
        /// are we carrying the item?
        /// </summary>
      //  public bool? CarriesAmmo;
               
        public int? RoundsAvailable;
    }


    class EvaluateAttackJobs : GoalEvaluator, IScoreJob
    {
        private float priority;
        public override float Priority
        {
            get
            {
                return priority;
            }
        }

        //*************
        // progress variables for timeslicing/interrupt and continue:
        private enum Progress { NotStarted, GetCombos, ScoreCombos, FindFinalCombo }
        private Progress progress = Progress.NotStarted;

      
       // private WeaponInstanceCombo bestCombo;

        private Dictionary<Job, List<WeaponInstanceCombo>> currentAttackCombos = new Dictionary<Job, List<WeaponInstanceCombo>>();


        private Allegiances.Allegiance allegiance;

        private EntityGroup huntingJobs;
        private List<EntityGroup> ownersOfWeapons;
        private List<EntityGroup> ownersOfVehicles;

        /// <summary>
        /// is null for critters..
        /// </summary>
        private OwnerID? ownerOfCarcass;


        private List<AttackType> AvailableAttackTypes;

        // Dictionary<Job, WeaponInstanceCombo> allCombos = new Dictionary<Job, WeaponInstanceCombo>();
        List<WeaponInstanceCombo> allCombos = new List<WeaponInstanceCombo>();

        // progress variables
        private int scoreComboIndex = 0;
        private int finalComboSelectionIndex = 0;

      //  public Dictionary<EntityType, ReplenishStatus> cachedToolEnergyAvailableStates = new Dictionary<EntityType, ReplenishStatus>();
        public Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates = new Dictionary<AttackType, ReplenishWeaponStatus>();


        /// <summary>
        /// a list of agents in our allegiance. We can send messages to command them to stop any job
        /// </summary>
        private List<Entity> needsToBeCancelled = new List<Entity>();


        /// <summary>
        /// a list of items carried by members of our allegiance.  We can send them messages to commandeer items
        /// </summary>
        private List<Entity> itemsToBeDropped = new List<Entity>();


        /// <summary>
        /// store the tools that we have found succesful replenish results for, with this job duration and job score
        /// </summary>
        //  private Dictionary<Entity, List<Entity>> cachedReplenishResults = new Dictionary<Entity, List<Entity>>();

        private Dictionary<EntityType, List<ItemDistance>> replenishItemsSortedByDistanceToEntity = new Dictionary<EntityType, List<ItemDistance>>();

        /// <summary>
        /// choose randomly weighted from the top scoring combos for the best job to create more varied combat:
        /// </summary>
        private List<WeaponInstanceCombo> jobAttackCombosToSelectFrom = new List<WeaponInstanceCombo>();
        //private AttackJob bestJob = null;

        private int noOfCombosToSelectFrom = 10;


        private JobTypes jobTypes;

        public enum JobTypes { Hunt, AssetThreat, Threat }


     /*   public struct ReplenishStatus
        {
            public bool? OwnsEnergy;
            //  public bool? EnergyIsAccessible;

            /// <summary>
            /// for how long time the above properties hold.
            /// </summary>
            public float? Duration;
        }*/


        /// <summary>
        /// threat jobs have higher priority than hunt jobs.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="jobTypes"></param>
        /// <param name="allegiance"></param>
        /// <param name="ownerOfCarcass"></param>
        /// <param name="weaponsGroups"></param>
        /// <param name="vehiclesGroups"></param>
        public EvaluateAttackJobs(Entity entity, JobTypes jobTypes, Allegiances.Allegiance allegiance, EntityGroup huntingJobs, OwnerID? ownerOfCarcass,
                                                                            List<EntityGroup> weaponsGroups = null, List<EntityGroup> vehiclesGroups = null)
            : base(entity)
        {
            this.allegiance = allegiance;
            this.huntingJobs = huntingJobs;
            this.jobTypes = jobTypes;

            if (this.jobTypes == JobTypes.Threat)
            {
                this.priority = GameData.Instance.AIConstants.PriorityOfThreatJobs; // 100f;

                noOfCombosToSelectFrom = 10;
            }
            else if (this.jobTypes == JobTypes.AssetThreat)
            {
                this.priority = GameData.Instance.AIConstants.PriorityOfAssetThreatJobs; 

                noOfCombosToSelectFrom = 10;
            }
            else
            {

                this.priority = GameData.Instance.AIConstants.PriorityOfWorkJobs; //1f;

                // when hunting, always choose the best combo (also to prevent excessive job switching with other agents)
                noOfCombosToSelectFrom = 1;

                if (this.huntingJobs == null)
                {
                    throw new Exception();
                }
            }

            this.ownerOfCarcass = ownerOfCarcass;

            this.ownersOfWeapons = weaponsGroups;
            this.ownersOfVehicles = vehiclesGroups;
        }

        public static void SetHuntingJobNotFeasible(Job job, bool value)
        {
            EntityGroup owner;
            job.ResolveOwner(out owner);

            if (owner != null)
            {
                The.Client.SetHuntingJobNotFeasible(job, owner.Parent, value);
            }
        }

        public static void SetJobTooFarFromExpedition(Job job, bool value)
        {
            EntityGroup owner;
            job.ResolveOwner(out owner);

            if (owner != null)
            {
                The.Client.SetJobTooFarFromExpedition(job, owner.Parent, value);
            }
        }

      

        public static void SetJobInaccessibleDueToBoldStance(Job job, bool IsBlocked)
        {
            EntityGroup owner;
            job.ResolveOwner(out owner);

            if (owner != null)
            {
                The.Client.SetJobBlockedByBoldStance(job, owner.Parent, IsBlocked);
            }
        }


        private void SetHuntingJobsAccessibility(bool isNotAccessible)
        {

            if (huntingJobs != null)
            {
                foreach (var kvp in huntingJobs.ProductionJobs) // also gather and hunting jobs
                {
                    //We cannot do any of the available hunting jobs.
                    foreach (var job in kvp.Value)
                    {
                        SetJobInaccessibleDueToBoldStance(job, isNotAccessible);
                    }
                }
            }
        }

        //This overrides abstract GoalEvaluator
        WeaponInstanceCombo bestCombo = null;
        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            bestCombo = null;

            if (progress == Progress.NotStarted)
            {
                bestCarriedCombo = null;
                carriedCombos.Clear();
                //bestCombo = null;

                if (!entityIntelligence.StanceCanBeBold()) // we are not in a state to fight...
                {
                    SetHuntingJobsAccessibility(true);
                    result = 0;
                    return CalculateResult.Done;
                }
                SetHuntingJobsAccessibility(false);
                bestScore = minimumRatingToConsider;

                // keep all the attack combos
                currentAttackCombos.Clear();

                BiologicalEntity bioEntity;
                if (entity.Find(out bioEntity))
                {
                    double ageContribution = 1;

                    ageContribution = ScoreAge(bioEntity);

                    if (ageContribution == 0)
                    {
                        result = 0;
                        return CalculateResult.Done;
                    }
                }

                double timeOfDayContribution = ScoreTimeOfDay();

                progress = Progress.GetCombos; // start!

            }

            if (progress == Progress.GetCombos)
            {
                GetAllJobCombos();
            }


            if (progress == Progress.ScoreCombos)
            {
                if (ScoreAllCombos(minimumRatingToConsider) == CalculateResult.Processing)
                {
                    return CalculateResult.Processing;
                }
            }

           
            if (progress == Progress.FindFinalCombo)
            {
                
                // use weighted random to get more interesting melee combat

                // if (ScoreAllCombosAndReturnBest(minimumRatingToConsider, ref bestCombo) == CalculateResult.Done)
                if (FindBestCombo(minimumRatingToConsider) == CalculateResult.Done)
                {

                    if (jobAttackCombosToSelectFrom.Count > 0) // bestCombo != null) //bestCombo.HasValue)
                    {

                        bestScore = jobAttackCombosToSelectFrom[0].Score; // bestCombo.Score;
                        result = bestScore; // bestCombo.Score;
                        bestCombo = jobAttackCombosToSelectFrom[0];

                        //progress = Progress.GetCombos; // start from the top next time!

                        // for debugging feedback. 
                        entityIntelligence.TopScoringJobs.Add(new GoalAndScore() { Score = bestScore, Goal = jobAttackCombosToSelectFrom[0].AttackData.JobData.Job.ToString() });

                        return CalculateResult.Done;
                    }
                }
                else return CalculateResult.Processing;

            }


            // no vacant jobs...
            result = 0;
            return CalculateResult.Done;
        }

        private void GetAllJobCombos()
        {
            scoreComboIndex = 0;
            allCombos.Clear();

            if (jobTypes == JobTypes.Threat
                || jobTypes == JobTypes.AssetThreat)
            {

                List<Job> threatJobs = null;
                if (jobTypes == JobTypes.Threat)
                {
                    threatJobs = allegiance.SharedKnowledge.AllKnownEntities.ThreatJobs;
                }
                else if (jobTypes == JobTypes.AssetThreat)
                {
                    threatJobs = allegiance.SharedKnowledge.AllKnownEntities.AssetThreatJobs;
                }

                for (int i = threatJobs.Count - 1; i >= 0; i--)
                {

                    AttackJob attackJob = threatJobs[i] as AttackJob;

                    if (!entityIntelligence.Brain.IsSame(attackJob)) // don't consider a job we are already doing!                    
                    {
                        IKnownEntityData targetData;

                        if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(attackJob.Target.Value, out targetData)))
                        {
                            // remove the outdated job:
                            attackJob.Destroy(true);
                            continue;
                        }
                        WeaponInstanceComboJobData jobData = new WeaponInstanceComboJobData();
                        jobData.Job = attackJob;
                        GetAllWeaponInstanceCombos(attackJob, jobData, targetData, allCombos);
                    }
                }
            }
            else
            {
                HuntingJob huntingJob;
                for (int i = huntingJobs.OtherJobs.Count - 1; i >= 0; i--)
                {
                    huntingJob = huntingJobs.OtherJobs[i] as HuntingJob;

                    if (huntingJob != null)
                    {
                        if (!entityIntelligence.Brain.IsSame(huntingJob)) // don't consider a job we are already doing!                    
                        {

                            IKnownEntityData targetData = null;
                            if (!huntingJob.Target.HasValue // remove the job if no target has been set!
                                || EntityDataResultCausesSkip(entityIntelligence.GetKnownData(huntingJob.Target.Value, out targetData)))
                            {
                                // remove the outdated job:
                                huntingJob.Destroy(true);
                                continue;
                            }

                            WeaponInstanceComboJobData jobData = new WeaponInstanceComboJobData();
                            jobData.Job = huntingJob;

                            GetAllWeaponInstanceCombos(huntingJob, jobData, targetData, allCombos);
                        }
                    }
                }

               /*              
                foreach (KeyValuePair<EntityType, List<Job>> kvp in huntingJobs.ProductionJobs) // also gather and hunting jobs
                {
                    for (int i = kvp.Value.Count - 1; i >= 0; i--) // we may remove/clean jobs while iterating!
                    {
                        huntingJob = kvp.Value[i] as HuntingJob;

                        if (!entityIntelligence.Brain.IsSame(huntingJob)) // don't consider a job we are already doing!                    
                        {
                            if (huntingJob == null)
                            {
                                // look at next EntityType
                                break;
                            }

                            IKnownEntityData targetData = null;
                            if (!huntingJob.Target.HasValue // remove the job if no target has been set!
                                || EntityDataResultCausesSkip(entityIntelligence.GetKnownData(huntingJob.Target.Value, out targetData)))
                            {
                                // remove the outdated job:
                                huntingJob.Destroy(true);
                                continue;
                            }

                            WeaponInstanceComboJobData jobData = new WeaponInstanceComboJobData();
                            jobData.Job = huntingJob;

                            GetAllWeaponInstanceCombos(huntingJob, jobData, targetData, allCombos);
                        }
                    }
                }*/
            }


            progress = Progress.ScoreCombos;
        }

        List<EntityID> tempIntrinsicWeaponsList = new List<EntityID>();

        /// <summary>
        ///  get all possible weapon combos for this attack job with the weapons available
        /// </summary>
        public void GetAllWeaponInstanceCombos(AttackJob job, WeaponInstanceComboJobData jobData, IKnownEntityData targetData, List<WeaponInstanceCombo> allCombos)
        {
            BodyComponent attackerBody;
            entity.Find(out attackerBody);
            Body targetBody = targetData.Body;          

            // get weapon attacks:
            if (entity.EntityType.IntelligenceType.CanUseWeapons != false)
            {
                if (ownersOfWeapons != null)
                {
                    foreach (var owner in ownersOfWeapons)
                    {
                        foreach (var weaponGroup in owner.WeaponsByAttackType)
                        {
                            if (weaponGroup.Value.Count > 0)
                            {
                                GetCombosForAttackType(entity, jobData, attackerBody.Body, targetBody, weaponGroup.Key, weaponGroup.Value, allCombos, ownersOfWeapons, cachedWeaponReplenishStates);
                            }
                        }
                    }
                }
            }

            if (entity.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
            {
                // add intrinsic weapons
                foreach (var item in entityIntelligence.IntrinsicWeapons)
                {                    
                    foreach (var attackType in item.Key.ItemType.WeaponType.AttackTypes)
                    {
                        tempIntrinsicWeaponsList.Add(item.Value);

                        GetCombosForAttackType(entity, jobData, attackerBody.Body, targetBody, attackType, tempIntrinsicWeaponsList,
                            allCombos, ownersOfWeapons, cachedWeaponReplenishStates);

                        tempIntrinsicWeaponsList.Clear();
                    }
                }
            }

            // now add intrinsic attacks       
            if (entity.EntityType.IntelligenceType.AttackTypes != null)
            {
                foreach (var intrinsicAttackType in entity.EntityType.IntelligenceType.AttackTypes)
                {
                    GetCombosForAttackType(entity, jobData, attackerBody.Body, targetBody, intrinsicAttackType, null, allCombos, ownersOfWeapons, cachedWeaponReplenishStates);

                }
            }

            tempIntrinsicWeaponsList.Clear();
        }

        /// <summary>
        /// get all the attack combos we can do with the carried weapons and ammunition
        /// 
        /// we can consider carried items as Entities instead of IKnownEntityData
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="job"></param>
        /// <param name="jobData"></param>
        /// <param name="targetData"></param>
        /// <param name="allCombos"></param>
        public static void GetAllWeaponInstanceCombosWithCarriedWeapons(Entity attacker, AttackJob job, WeaponInstanceComboJobData jobData, IKnownEntityData targetData, List<WeaponInstanceCombo> allCombos)
        {
            BodyComponent attackerBody;
            attacker.Find(out attackerBody);

            Body targetBody = targetData.Body;
          
            if (attacker.Contains != null)
            {
                WeaponInstanceCombo combo;
                List<Entity> listOfWeapons = attacker.Contains.GetContainedItemsList(e => e.EntityType.ItemType != null && e.EntityType.ItemType.WeaponType != null); //.IterateContained(GetCarriedWeapon);

                // group weapons by attack types:

                List<EntityID> weapons;
                Dictionary<AttackType, List<EntityID>> weaponsByAttackType = new Dictionary<AttackType, List<EntityID>>();
                foreach (var weapon in listOfWeapons)
                {
                    foreach (var attackType in weapon.EntityType.ItemType.WeaponType.AttackTypes)
                    {
                        if (!weaponsByAttackType.TryGetValue(attackType, out weapons))
                        {
                            weapons = new List<EntityID>();
                            weaponsByAttackType.Add(attackType, weapons);
                        }

                        weapons.Add(weapon.EntityID);
                    }
                }

                // get carried ammo:
                List<Entity> listOfAmmoItems = attacker.Contains.GetContainedItemsList(e => e.EntityType.ItemType != null && e.EntityType.ItemType.AmmunitionType != null);
                //attacker.Contains.IterateContained(GetCarriedAmmunition);

                Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates = new Dictionary<AttackType, ReplenishWeaponStatus>();

                List<Entity> ammoItemsForAttack;

                // get the weapon replenish statuses:  
                ReplenishWeaponStatus ammoStatus;
                int roundsAvailable;
                foreach (var weapon in listOfWeapons)
                {
                    foreach (var attackType in weapon.EntityType.ItemType.WeaponType.AttackTypes)
                    {
                        if (attackType.UsesAmmo != null)
                        {
                            if (!cachedWeaponReplenishStates.TryGetValue(attackType, out ammoStatus))
                            {
                                ammoStatus = new ReplenishWeaponStatus();

                                ammoItemsForAttack = listOfAmmoItems.FindAll(i => i.EntityType == attackType.UsesAmmoType);
                                roundsAvailable = RoundsAvailableForAttack(ammoItemsForAttack);

                               // roundsAvailable = ammoItemsForAttack.Sum(i => i.Item.Ammunition.NoOfRounds);

                                ammoStatus.RoundsAvailable = roundsAvailable; // (!ammoStatus.RoundsAvailable.HasValue ? ammoItem.Item.Ammunition.NoOfRounds : ammoStatus.RoundsAvailable.Value + ammoItem.Item.Ammunition.NoOfRounds);
                                ammoStatus.OwnsAmmo = roundsAvailable > 0;
                                cachedWeaponReplenishStates[attackType] = ammoStatus;	
                            }                            	 
                        }
                    }

                }

              
                //List<Entity> ammo;


                foreach (var item in weaponsByAttackType)
                {
                    GetCombosForAttackType(attacker, jobData, attackerBody.Body, targetBody, item.Key, item.Value, allCombos, null, cachedWeaponReplenishStates);    
                }                
            }


            // now add intrinsic attacks
            foreach (var intrinsicAttackType in attacker.EntityType.IntelligenceType.AttackTypes)
            {
                GetCombosForAttackType(attacker, jobData, attackerBody.Body, targetBody, intrinsicAttackType, null, allCombos, null, null);

            }
        }
      
        // TODO: put NoOfRounds In IKnownEntityData
        public static int RoundsAvailableForAttack(List<Entity> ammoItemsForAttack)
        {
            int roundsAvailable = ammoItemsForAttack.Sum(i => i.Item.Ammunition.NoOfRounds);
            return roundsAvailable;
        }
        /// <summary>
        /// we could use bit patterns and filters here...?
        /// </summary>
    /*    private static List<Entity> listOfWeapons = new List<Entity>();
        private static void GetCarriedWeapon(Entity e)
        {
            listOfWeapons.Clear();
            if (e.EntityType.ItemType != null && e.EntityType.ItemType.WeaponType != null)
            {
                listOfWeapons.Add(e);
            }
        }
        */
/*
        public static List<Entity> listOfAmmoItems = new List<Entity>();
        public static void GetCarriedAmmunition(Entity e)
        {
            listOfAmmoItems.Clear();
            if (e.EntityType.ItemType != null && e.EntityType.ItemType.AmmunitionType != null)
            {
                listOfAmmoItems.Add(e);
            }
        }
        */

        private static void GetCombosForAttackType(Entity attacker, WeaponInstanceComboJobData jobData, Body attackerBody, Body targetBody, AttackType attackType,
            List<EntityID> weapons, List<WeaponInstanceCombo> allCombos, List<EntityGroup> ownersOfWeapons,
            Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates)
        {
            Dictionary<BodyPartType, float> attackScoresAgainstBodyParts;
            if (IntelligenceType.AttackTypeIsFunctional(attackerBody, attackType))
            {
                // Get the precomputed damge scores
                attackScoresAgainstBodyParts = GameData.Instance.AttackScoresAgainstBodyParts[attackType];

                foreach (var bodyPart in targetBody.BodyParts)
                {
                    GetAllWeaponInstanceCombosAgainstBodyParts(attacker, jobData, attackType, bodyPart, weapons, attackScoresAgainstBodyParts, allCombos, ownersOfWeapons, cachedWeaponReplenishStates);
                }              
            }

        }

        private static void GetAllWeaponInstanceCombosAgainstThisBodyPart(Entity attacker, WeaponInstanceComboJobData jobData, AttackType attackType, 
            BodyPart bodyPart, WeaponInstanceComboAttackData attackData /*BodyPartType bodyPartType*/,
            List<EntityID> weapons, List<WeaponInstanceCombo> allCombos, List<EntityGroup> ownersOfWeapons,
            Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates) //, Dictionary<BodyPartType, float> estimatedDamageOfAttackType)
        {
            if (bodyPart.IsFunctional())
            {
               
                // get all weapon instances:

                WeaponInstanceCombo weaponInstanceCombo;
                //   WeaponInstanceComboAttackData attackData;

                Intelligence intelligence = attacker.Intelligence;

                if (weapons != null)
                {
                    IKnownEntityData weaponData;
                    foreach (var weapon in weapons)
                    {
                        if(!EntityDataResultCausesSkip(intelligence.GetKnownData(weapon, out weaponData)))
                        {
                            if (IsValidPlaysiteWeapon(attacker, weaponData, attackType, ownersOfWeapons, cachedWeaponReplenishStates)) 
                            {
                            
                                weaponInstanceCombo = new WeaponInstanceCombo() { Weapon = weaponData, AttackData = attackData };
                                allCombos.Add(weaponInstanceCombo);
                            }
                        }
                    }
                }
                else
                {
                    
                    weaponInstanceCombo = new WeaponInstanceCombo() { Weapon = null, AttackData = attackData };
                    allCombos.Add(weaponInstanceCombo);
                }
            }
           
        }

        private static void GetAllWeaponInstanceCombosAgainstBodyParts(Entity attacker, WeaponInstanceComboJobData jobData, AttackType attackType, BodyPart bodyPart, 
            List<EntityID> weapons, Dictionary<BodyPartType, float> estimatedDamageOfAttackType,
            List<WeaponInstanceCombo> allCombos, List<EntityGroup> ownersOfWeapons,
            Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates)
        {
            // recursive method

            // base case: 
            // compute the attack data that all weapon instance combos will share:

            float estimatedDamage;
            if (!IsRangedAttackAgainsNonVitalBodyPart(attackType, bodyPart.BodyPartType) // we don't want aimed shots at non-vital bodyparts.
                && estimatedDamageOfAttackType.TryGetValue(bodyPart.BodyPartType, out estimatedDamage)) // this attack type/weapon group can damage this body part type.           
            {
                   
                WeaponInstanceComboAttackData attackData = new WeaponInstanceComboAttackData();
                //attackData.BodyPart = bodyPart;
                attackData.bodyPartID = bodyPart.BodyPartID;
                attackData.AttackType = attackType;
                attackData.JobData = jobData;
                attackData.EstimatedDamageScore = estimatedDamage;

                GetAllWeaponInstanceCombosAgainstThisBodyPart(attacker, jobData, attackType, bodyPart, attackData, weapons, allCombos, ownersOfWeapons, cachedWeaponReplenishStates); //, estimatedDamageOfAttackType);
            }

            // recursive case:
            if (bodyPart.BodyParts != null)
            {
                foreach (var childBodyPart in bodyPart.BodyParts)
                {
                    GetAllWeaponInstanceCombosAgainstBodyParts(attacker, jobData, attackType, childBodyPart, weapons, estimatedDamageOfAttackType, allCombos, ownersOfWeapons, cachedWeaponReplenishStates);
                }
            }

        }

        /// <summary>
        /// we don't want aimed shots at non-vital bodyparts.
        /// </summary>
        /// <returns></returns>
        private static bool IsRangedAttackAgainsNonVitalBodyPart(AttackType attackType, BodyPartType bodyPartType)
        {
            if (attackType.RangeType != AttackType.RangeTypes.Melee
                && !bodyPartType.IsVital())
            {
                return true;
            }

            return false;
        }


        private CalculateResult ScoreAllCombos(double minimumRatingToConsider) //, ref ToolInstanceCombo bestCombo)
        {
            WeaponInstanceCombo combo;
            double score = -1f;

            // always bold when attacking:
            RegionMap footRegionMap = null;

            if (entity.EntityType.IntelligenceType.IsMobile)
            {
                footRegionMap =
                    entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(
                    entity.Intelligence.ProtectionLevel,
                    entity.EntityType, // entity.Intelligence.Allegiance.RepresentativeEntityType.ThreatCategory, 
                    ThreatStance.Bold).Layers[SurfaceType.TransportType.Foot].RegionMap;
            }

            double ageContribution = GetAgeContribution();
            double timeOfDayContribution = ScoreTimeOfDay();

            float fitnessScore = ScoreFitness(entity);
            float energyLevelFactor = GoalDoAttack.GetEnergyLevelFactorOnDamageForMelee(entity);

            if (allCombos.Count > 0 && entity.Name != null && entity.Name.Contains("coyd"))
            {

            }
            //float estimatedJobDuration = 

            for (; scoreComboIndex < allCombos.Count; scoreComboIndex++)
            {

                combo = allCombos[scoreComboIndex];

                //Before scoring, check that the job and tools are still valid!
                if (!scoringWasInterrupted || IsComboValid(entity, entityIntelligence, combo, cachedWeaponReplenishStates)) // only need to check for validity if we are resuming.
                {
                    int proposedNumberOfWorkers = Common.Clamp(combo.AttackData.JobData.Job.TakenBy.Count + 1, 0, combo.AttackData.JobData.Job.MaxJobPositions);

                   
                  /*  ToolParams toolParams = new ToolParams()
                    {
                        Tools = combo.Tools,
                        ToolProductivity = toolsetProductivity,
                        ReplenishStatus = cachedToolEnergyAvailableStates,
                        JobDuration = null
                    };*/


                    if (ScoreThisAttackJob(footRegionMap, entity, proposedNumberOfWorkers, ageContribution, timeOfDayContribution, fitnessScore, energyLevelFactor,
                        combo, // combo.AttackData, combo.Weapon, combo.AttackData.AttackType, 
                        out score)
                        == CalculateResult.Done)
                    {
                        combo.Score = (float)score;

                        if (combo.Weapon != null)
                        {
                            if (entity.AgentStorage != null && entity.AgentStorage.Contains(combo.Weapon.EntityID))
                            {
                                
                                if (bestCarriedCombo == null ||combo.AttackData.AttackScore > bestCarriedCombo.AttackData.AttackScore)
                                {
                                    bestCarriedCombo = combo;
                                }

                                carriedCombos.Add(combo);
                            }
                           // carriedCombos.Add(combo);
                        }                      

                    }
                    else
                    {
                        scoringWasInterrupted = true; // now we need to check all the combos again for availability...
                        return CalculateResult.Processing; // come back later...
                    }
                }
                else
                {
                    combo.Score = 0;
                }
            }


            // reset variables and progress to next phase:
            scoreComboIndex = 0;
            jobAttackCombosToSelectFrom.Clear();
            cachedWeaponReplenishStates.Clear(); // did we forget this..? adding it now.

            progress = Progress.FindFinalCombo;

            return CalculateResult.Done;

        }


        private Dictionary<Job, WeaponInstanceComboJobData> jobData = new Dictionary<Job, WeaponInstanceComboJobData>();
        private Dictionary<Job, WeaponInstanceComboAttackData> attackData = new Dictionary<Job, WeaponInstanceComboAttackData>();
        WeaponInstanceCombo combo = null;
        private CalculateResult FindBestCombo(double minimumRatingToConsider) //, ref WeaponInstanceCombo bestCombo)
        {

            combo = null;
            // Important! Remove the combos with zero score:
            allCombos.RemoveAll(c => c.Score == 0);

            allCombos.Sort((a, b) => b.Score.CompareTo(a.Score));

            if (allCombos.Count > 0 && entity.Name != null && entity.Name.Contains("coyd"))
            {

            }

            // final combo selection.
            // find the replenish items that we need for the weapons.

            // use weighted random to select one attack...
            // only choose between combos for the same job, and using either melee or the same weapon (targeting different bodyparts)

            AttackJob bestJob = null;
          
            IKnownEntityData effectiveWeaponInHand = null; // use this to filter stupid choices...

            for (; finalComboSelectionIndex < allCombos.Count; finalComboSelectionIndex++)
            {
                
                combo = allCombos[finalComboSelectionIndex];

                // only gather attack combos against the best job:
                if (bestJob != null && combo.AttackData.JobData.Job != bestJob)
                {
                    continue;
                }

                if (!scoringWasInterrupted || IsComboValid(entity, entityIntelligence, combo, cachedWeaponReplenishStates)) // only need to check for validity if we are resuming.
                {
                    if (combo.Score < minimumRatingToConsider)
                    {
                        break; // the score is not good enough. we have reached the low scores and are now finished
                    }
                }
                else
                {
                    continue; // this combo is no longer available, skip it.
                    //break; 
                }
                
                // we have the best job now. See if we need to cancel anyone:
                if (bestJob == null)
                {
                    // collect the top X combos against this job and select from them randomly
                    bestJob = combo.AttackData.JobData.Job;
                }


                // TODO: set a score on equipped items!

                // fills needsToBeCancelled as well as itemsToBeDropped:
                GetJobsToCancel(combo);

                double ourScore = combo.Score; // -GameData.Instance.AIConstants.AmountNewGoalMustBeBetterThanOtherEntityToCancel;

                // if there are people doing this job, only take it from them if our score is better than theirs by a certain margin.
                // and if we need their items, they must be permitted to give them up too
                if (CanCancelAndForceDropItems(ourScore)) //  IsScoreBetterThanAllInvolveds(ourScore, needsToBeCancelled, entity))
                {

                    if (SkipThisCombo(combo))
                    {
                        continue;
                    }

                    if (combo.Weapon != null)
                    {
                        // the randomized attack selection means we need to filter weird choices here...
                        // first: disregard weapon combos that use a weapon which is same or worse than what we are already carrying:
                        
                        // if the weapon is not carried, we want to compare it to the carried weapons. if the score is not good enough, it should not be considered.
                        // see if we have a weapon of the same type:
                            
                        

                        // COPIED FROM EvaluateJob!

                        // Time to see if the selected weapon needs replenishment.
                        // This step may fail. In which case we go to the next combo.

                        // The reason I decided not build this into the combo/scoring system was to avoid an explosion in item combinations.
                        // Doing it this way, makes the code a bit more complex, and the results less optimal, but should cut down on total computations..

                        // Important: the replenishment items are not scored as such, only rated for distance to the agent.
                        // There is also a distance cutoff, that disregards items more than one screen away.

                       

                        SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;

                        RegionMap footRegionMap = null;

                        if (entity.EntityType.IntelligenceType.IsMobile)
                        {
                            footRegionMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel,
                                entity.EntityType, ThreatStance.Bold).Layers[SurfaceType.TransportType.Foot].RegionMap;
                        }

                        bool replenishResult;
                        // see if we can find the items that we need:
                        CalculateResult result = FindReplenishItemsForToolOrWeapon(entity, null, ownersOfWeapons, ref combo.ReplenishItemsForWeapon,
                            ourScore, sharedKnowledge, footRegionMap, combo.Weapon, replenishItemsSortedByDistanceToEntity, out replenishResult,
                            combo.AttackData.AttackType.UsesAmmoType, combo.AttackData.AttackType.RoundsToSpend);

                        if (result == CalculateResult.Processing)
                        {
                            return CalculateResult.Processing;
                        }
                        else if (replenishResult == false)
                        {
                            // couldn't replenish the needed tools. Look at next combo.
                            continue;
                        }
                        else
                        {
                            // got it.
                            //bestCombo = combo;

                            GetReplenishItemsToCancel(combo.ReplenishItemsForWeapon, ref needsToBeCancelled, ref itemsToBeDropped);


                            jobAttackCombosToSelectFrom.Add(combo);

                            if (entity.AgentStorage != null && entity.AgentStorage.MountedToolOrWeapon == combo.Weapon.EntityID)
                            {   // save this...
                                effectiveWeaponInHand = combo.Weapon;
                            }

                           // break;
                        }
                    }
                    else
                    {
                        // disregard unarmed attacks using limbs that already hold a weapon that can perform a valid attack:                      
                        // we would have to drop the weapon first to perform the attack, which would look stupid
                        if (IsUnarmedAttackUsingBodypartWithEffectiveWeapon(effectiveWeaponInHand, combo))
                        {
                            continue;
                        }

                        jobAttackCombosToSelectFrom.Add(combo);

                       // break;
                    }
                }

                // the job was taken and we didn't score high enough to take it from them. continue looking.
                // also continue until we have enough combos to select from:
                if (jobAttackCombosToSelectFrom.Count >= noOfCombosToSelectFrom)
                {
                    break;
                }
            }

            // return bestCombo;

            // reset progress variables for next time
            finalComboSelectionIndex = 0;
            replenishItemsSortedByDistanceToEntity.Clear();
           
            
            progress = Progress.NotStarted; // all done. Start from the top next time!

            return CalculateResult.Done;

        }

        private bool SkipThisCombo(WeaponInstanceCombo combo)
        {
            //If this is a weapon combo that is not carried, then we want to see if it is worth picking it up.
            //We do not want to go and get a downgrade.
            if (UncarriedWeaponIsBetterThanCarriedWeapons(combo) == false)
            {
                return true;
            }
                

            //Now we want to go trough our equipped combos
            //If this combo is not within range and we got a higher scoring combo equipped we should not consider using this one.
            // Lars: this seems to be a melee/vs range weapon filter
            if (ComboNotInRangeAndGotComboInRangeEquipped(combo))
            {
                return true;
            }

            return false;
        }


        private bool ComboNotInRangeAndGotComboInRangeEquipped(WeaponInstanceCombo combo)
        {           
            IKnownEntityData target = null;
            EntityResult result = entity.Intelligence.GetKnownData(combo.AttackData.JobData.Job.Target.Value, out target);
            if (result == EntityResult.Destroyed || target == null)
            {
                return false;
                //Target has been killed.
            }
 
            //Early bailout. We are already in melee range. All attacks are within range.
            float distanceToTarget = Common.DistanceOctile(entity.PlaySiteLocation, target.PlaySiteLocation);
            if (distanceToTarget > 48f) //Hack in ActivateMeleeAttack? Seems to look at this magic number
            {
                return false;
            }

            //Early bailout, the current combo we are evaluating is within range.
            float? maxRange = combo.AttackData.AttackType.MaxRange;
            if (maxRange.HasValue && maxRange > distanceToTarget)
            {
                return false;
            } 
            
            //Lets not consider using this weapon if it is not in range unless it has a better damage score. (Lightsaber vs slingshot?)
            foreach (var carriedCombo in carriedCombos)
            {
                if (carriedCombo.AttackData.AttackScore > combo.AttackData.AttackScore)
                {
                    float? carriedComboMaxRange = carriedCombo.AttackData.AttackType.MaxRange;
                    if (carriedComboMaxRange.HasValue)
                    {
                        if (carriedComboMaxRange.Value > distanceToTarget)
                        {
                            return true;
                        }
                    }

                }
            }

            return false;
        }

        private bool IsUnarmedAttackUsingBodypartWithEffectiveWeapon(IKnownEntityData effectiveWeaponInHand, WeaponInstanceCombo combo)
        {
            if (effectiveWeaponInHand != null &&
                            combo.AttackData.AttackType.DependsOn.Any(p => p.Name == effectiveWeaponInHand.EntityType.ItemType.AttachesToBodyPart))
            {
                return true;
            }

            return false;
        }


        private bool WeaponIsCarried(IKnownEntityData weapon)
        {
            //TODO: Rename or split? IfWeaponISCarried Look at type after that
            //TODO: Check inventory for weapon
            // check that the weapon is different:

            return entity.AgentStorage.Contains(weapon.EntityID);                     

        }

        /// <summary>
        /// returns true if the weapon we are carring is a different weapon, but of the same or better type
        /// the idea is to avoid a situation where for example an agent has a spear, but then wants to get another spear that is lying somewhere else
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        private bool WeCarrySameOrBetterWeaponType(IKnownEntityData weapon)
        {
           

            // see if we have a weapon of the same type:
            if (WeaponIsSameOrBetterType(entity.AgentStorage.MountedToolOrWeapon, weapon))
            {
                return true;
            }

            return false;

            // TODO: look in agent storage also...
       /*     if (entity.AgentStorage.ItemStorage.IterateContained(WeaponIsSameOrBetterType))
            {

            }*/
        }
        private WeaponInstanceCombo bestCarriedCombo;
        private List<WeaponInstanceCombo> carriedCombos = new List<WeaponInstanceCombo>();
        public bool UncarriedWeaponIsBetterThanCarriedWeapons(WeaponInstanceCombo weaponCombo)
        {
            if (combo.Weapon != null && entity.AgentStorage != null)
            {
                if (!WeaponIsCarried(combo.Weapon))
                {
                    // first loop over the list of carried weapons - check their type
                    // then compare damage score with the best combo of the carried weapons

                    bool carriesSameType = false;
                    entity.AgentStorage.IterateContainedBreakOnTrue(e =>
                        {
                            if (e.EntityType == weaponCombo.Weapon.EntityType)
                            {
                                carriesSameType = true;
                                return true;
                            }

                            return false;
                        });

                    //We do not have to go and get a new weapon when we already carry one of this type.
                    if (carriesSameType)
                    {
                        return false;
                    }

                    // now compare to the best DAMAGE score of carried weapons
                    if (bestCarriedCombo != null &&
                        weaponCombo.AttackData.AttackScore < bestCarriedCombo.AttackData.AttackScore)
                    {
                        return false;
                    }

                }
            }
            return true;
        }

        public bool WeaponIsSameOrBetterType(EntityID? carriedWeapon, IKnownEntityData otherWeapon)
        {
            if (carriedWeapon == null)
                return false;

            Entity carriedWeaponEntity = Entity.FindByID(carriedWeapon.Value);
            if (carriedWeaponEntity.EntityType == otherWeapon.EntityType)
            {
                return true;
            }
         
            // TODO: look at condition, damage rating etc...

            return false;
        }


        /// <summary>
        /// find the combo we are going to use, register the items we need in memory
        /// </summary>
        public override void PreSetGoal()
        {
            base.PreSetGoal();

            // weighted random selection!!!

            float totalScore;


            if (jobAttackCombosToSelectFrom.Count > 1)
            {
                // pick attack type and body part weighted randomly:
                Common.BuildEdgesFromBucketSizes(jobAttackCombosToSelectFrom, true, out totalScore);

                int index;
                Common.GetStairStepIndex(jobAttackCombosToSelectFrom, out index, The.Sim.GameplayRandomGenerator, totalScore);

                selectedCombo = jobAttackCombosToSelectFrom[index];
            }
            else
            {
                selectedCombo = jobAttackCombosToSelectFrom[0];
            }
           
            //  List<Tuple<GoalReplenish.ReplenishAction, List<IKnownEntityData>>> replenishItemDataForWeapon = null;
            List<Tuple<ProcessType, List<IKnownEntityData>>> replenishItemDataForWeapon = null;

            // create entity id lists of replenish items:
            if (selectedCombo.Weapon != null && selectedCombo.ReplenishItemsForWeapon != null)
            {
                if (selectedCombo.ReplenishItemsForWeapon.TryGetValue(selectedCombo.Weapon, out replenishItemDataForWeapon))
                {
                    replenishItemsForWeapon = new List<ReplenishItemsForAction>();

                    List<EntityID> listOfItemIDs;
                    foreach (var action in replenishItemDataForWeapon)
                    {
                        listOfItemIDs = new List<EntityID>();

                        foreach (var item in action.Item2)
                        {
                            listOfItemIDs.Add(item.EntityID);
                        }

                        replenishItemsForWeapon.Add(new ReplenishItemsForAction(action.Item1, listOfItemIDs));
                    }
                }
            }


            // prevent dropping hauled items that we need:
            if (selectedCombo.Weapon != null)
            {
                entityIntelligence.Memory.SetNeededItemForSwitchedGoal(selectedCombo.Weapon.EntityID);
            }

            if (replenishItemsForWeapon != null)
            {
                foreach (var item in replenishItemsForWeapon)
                {
                    foreach (var item2 in item.Items)
                    {
                        entityIntelligence.Memory.SetNeededItemForSwitchedGoal(item2);       
                    }                     
                }
            }

        }

        WeaponInstanceCombo selectedCombo;
        List<ReplenishItemsForAction> replenishItemsForWeapon = null;

        public override bool SetGoal()
        {
            base.SetGoal();
                        
            

            bool goalWasSet;
            if (selectedCombo.AttackData.JobData.Job is HuntingJob)
            {
                goalWasSet = entityIntelligence.SetTopLevelGoal(
                            new GoalHunt(entity, ((HuntingJob)selectedCombo.AttackData.JobData.Job), GetOwnerIDs(ownersOfVehicles),
                                ownerOfCarcass,
                                selectedCombo.AttackData.AttackType, selectedCombo.AttackData.bodyPartID,
                                selectedCombo.Weapon != null ? (EntityAndRoot?)selectedCombo.Weapon.GetAsEntityAndRoot() : null,
                                replenishItemsForWeapon) { GoalEvaluator = this }, selectedCombo.Score);
            }
            else
            {
                goalWasSet = entityIntelligence.SetTopLevelGoal(
                            new GoalAttack(entity, selectedCombo.AttackData.JobData.Job, GetOwnerIDs(ownersOfVehicles), null, // don't claim the carcass when not hunting  ownerOfCarcass,
                                selectedCombo.AttackData.AttackType, selectedCombo.AttackData.bodyPartID,
                                selectedCombo.Weapon != null ? (EntityAndRoot?)selectedCombo.Weapon.GetAsEntityAndRoot() : null,
                                replenishItemsForWeapon) { GoalEvaluator = this }, selectedCombo.Score);
            }


            selectedCombo = null; // was local before
            replenishItemsForWeapon = null; // was local before

            return true;
        }

       /* public override bool SetGoal()
        {
            base.SetGoal();

            // weighted random selection!!!

            float totalScore;
            WeaponInstanceCombo selectedCombo;
           
            if (jobAttackCombosToSelectFrom.Count > 1)
            {
                // pick attack type and body part weighted randomly:
                Common.BuildEdgesFromBucketSizes(jobAttackCombosToSelectFrom, true, out totalScore);
                  
                int index;
                Common.GetStairStepIndex(jobAttackCombosToSelectFrom, out index, The.Sim.GameplayRandomGenerator,totalScore);

                selectedCombo = jobAttackCombosToSelectFrom[index];
            }
            else
            {
                selectedCombo = jobAttackCombosToSelectFrom[0];
            }



            List<ReplenishItemsForAction> replenishItemsForWeapon = null;
            
          //  List<Tuple<GoalReplenish.ReplenishAction, List<IKnownEntityData>>> replenishItemDataForWeapon = null;
            List<Tuple<ProcessType, List<IKnownEntityData>>> replenishItemDataForWeapon = null;

            // create entity id lists of replenish items:
            if (selectedCombo.Weapon != null && selectedCombo.ReplenishItemsForWeapon != null)
            {                
                if (selectedCombo.ReplenishItemsForWeapon.TryGetValue(selectedCombo.Weapon, out replenishItemDataForWeapon))
                {
                    replenishItemsForWeapon = new List<ReplenishItemsForAction>();

                    List<EntityID> listOfItemIDs;
                    foreach (var action in replenishItemDataForWeapon)
                    {
                        listOfItemIDs = new List<EntityID>();

                        foreach (var item in action.Item2)
                        {
                            listOfItemIDs.Add(item.EntityID);
                        }

                        replenishItemsForWeapon.Add(new ReplenishItemsForAction(action.Item1, listOfItemIDs));
                    }
                }
            }



            bool goalWasSet;
            if (selectedCombo.AttackData.JobData.Job is HuntingJob)
            {
                goalWasSet = entityIntelligence.SetTopLevelGoal(
                            new GoalHunt(entity, ((HuntingJob)selectedCombo.AttackData.JobData.Job), GetOwnerIDs(ownersOfVehicles), 
                                ownerOfCarcass, 
                                selectedCombo.AttackData.AttackType, selectedCombo.AttackData.bodyPartID, 
                                selectedCombo.Weapon != null? (EntityAndRoot?)selectedCombo.Weapon.GetAsEntityAndRoot(): null,
                                replenishItemsForWeapon) { GoalEvaluator = this }, selectedCombo.Score);
            }
            else
            {
                goalWasSet = entityIntelligence.SetTopLevelGoal(
                            new GoalAttack(entity, selectedCombo.AttackData.JobData.Job, GetOwnerIDs(ownersOfVehicles), null, // don't claim the carcass when not hunting  ownerOfCarcass,
                                selectedCombo.AttackData.AttackType, selectedCombo.AttackData.bodyPartID,
                                selectedCombo.Weapon != null ? (EntityAndRoot?)selectedCombo.Weapon.GetAsEntityAndRoot() : null,
                                replenishItemsForWeapon) { GoalEvaluator = this }, selectedCombo.Score);
            }

           
            return true;
        }*/


        private bool IsComboValid(Entity entity, Intelligence entityIntelligence, WeaponInstanceCombo combo, Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates)
        {

            if (combo.AttackData.JobData.Job.ID == JobID.Invalid // threat job manager can delete the job...
                || (combo.Weapon != null && !IsValidPlaysiteWeapon(entity, combo.Weapon, combo.AttackData.AttackType, ownersOfWeapons, cachedWeaponReplenishStates)))
            {
                return false;
            }


            return true;
        }

        /// <summary>
        /// see: EvaluateJob.IsValidTool
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="weapon"></param>
        /// <param name="attackType"></param>
        /// <param name="ownersOfWeapons"></param>
        /// <param name="cachedWeaponReplenishStates"></param>
        /// <param name="doNotCheckAmmunitionAndBulk"></param>
        /// <returns></returns>
        public static bool IsValidPlaysiteWeapon(Entity entity, IKnownEntityData weapon, AttackType attackType, 
            List<EntityGroup> ownersOfWeapons, Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates, bool doNotCheckAmmunitionAndBulk = false) // float? jobDuration)
        {

          //  Item itemComponent;
            bool itemIsOK = true;
          //  weapon.Find(out itemComponent);

            bool itemIsDestroyed = false;
            bool isCarriedByOccupiedAgent = false;

            bool isIntrinsic = weapon.EntityType.IsIntrinsic();
            bool isIntrinsicAndCarried = false;
            if (isIntrinsic)
            {
                if (entity.IntrinsicWeapons == null || !entity.IntrinsicWeapons.ContainsValue(weapon.EntityID))
                {
                    // this was needed because optional weapons don't check attack types, which have the Intrinsic flag which otherwise prevents other agents from considering that weapon
                    return false;
                }
                else
                {
                    isIntrinsicAndCarried = true;
                }            
            }    
     
            // NEW: check for part as well:
            if (!isIntrinsicAndCarried
                && weapon.PartOfID.HasValue)
            {
                return false;
            }

            bool isMountable = weapon.EntityType.IsMountable();

            if (isMountable) 
            {               
                if (entity.IsCarrying(weapon.EntityID) == false)
                {
                    isCarriedByOccupiedAgent = IsCarriedByEntityWhoIsOccupied(weapon, out itemIsDestroyed); // we don't want agents who are busy dropping their gear.  
                }
            }

            if (itemIsDestroyed)
            {
                return false;
            }

            bool isInUseByNonWorkerProcess = false;
            if (!EvaluateJob.IsInUseByNonWorkerProcess(weapon, out isInUseByNonWorkerProcess))
            {
                return false;
            }

            if (isInUseByNonWorkerProcess)
                return false;


            bool hasAmmoForAttack = true;
            if (doNotCheckAmmunitionAndBulk == false)
            {
                float bulk = 0;
                if (attackType.UsesAmmo != null)
                {
                    bulk = weapon.Bulk + attackType.UsesAmmoType.ItemType.MaximumBulk.Value;// GetBulkFromCombo(weapon, attackType.UsesAmmo.ItemType.Bulk);
                }
                else
                {
                    bulk = weapon.Bulk;
                }

                itemIsOK =
                    isIntrinsicAndCarried ||
                    (!isCarriedByOccupiedAgent
                        && weapon.NotOnboardDrivenVehicle // ((itemComponent.OnBoard == null || itemComponent.OnBoard.Vehicle.DrivenBy == null)
                        && (entity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(bulk)));


                if (attackType.UsesAmmo != null)
                {
                    hasAmmoForAttack = HasAmmoForAttack(entity, weapon, attackType, null, ownersOfWeapons, cachedWeaponReplenishStates);
                }
                else hasAmmoForAttack = true;

            }
            else
            {
                itemIsOK = isIntrinsicAndCarried || (!isCarriedByOccupiedAgent && weapon.NotOnboardDrivenVehicle);
            }

            return itemIsOK
                  && hasAmmoForAttack
                  && IsOnPlaySite(weapon)
                  && Entity.IsFunctional(weapon) //  ScoreIsEntityFunctional(weapon) > 0d 
                  && weapon.IsCompleted();

        }

        /// <summary>
        /// in order to cut down on tool combinations, check to see if our inventory supports replenishment of this tool.
        /// Also checks if the agent may be able to replenish the weapon
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="jobDuration"></param>
        /// <returns></returns>
        public static bool HasAmmoForAttack(Entity agent, IKnownEntityData weapon, AttackType attackType, EntityGroup ownerOfItems, List<EntityGroup> ownersOfItems,
            Dictionary<AttackType, ReplenishWeaponStatus> cachedWeaponReplenishStates)
        {
          //  Magazine magazine;
            bool hasEnoughAmmo = true;
           // if (weapon.Find(out magazine))
            if (weapon.EntityType.ContainerType != null && weapon.EntityType.ContainerType is MagazineContainerType) // .Find(out magazine))
            {
                hasEnoughAmmo = weapon.HasEnoughAmmo(attackType.UsesAmmoType, attackType.RoundsToSpend.Value); // magazine.HasAmmo(ammoType, noOfRounds);

                if (!hasEnoughAmmo)
                {
                    // see if we can reload the weapon:

                    ReplenishWeaponStatus replenishStatus;
                    if (!cachedWeaponReplenishStates.TryGetValue(attackType, out replenishStatus)) 
                    {
                        replenishStatus = new ReplenishWeaponStatus() { /*EnergyIsAccessible = null, Duration = jobDuration*/ };

                        if (agent.EntityType.IntelligenceType.CanReplenish == true)
                        {
                            int availableRounds;
                            if (ownerOfItems != null)
                            {
                                // handle single owner
                                replenishStatus.OwnsAmmo = HasAmmoForAttack(attackType, ownerOfItems, out availableRounds);
                                replenishStatus.RoundsAvailable = availableRounds;
                            }
                            else if (ownersOfItems != null)
                            {  //multiple ownership lists
                                replenishStatus.OwnsAmmo = HasAmmoForAttack(attackType, ownersOfItems, out availableRounds);
                                replenishStatus.RoundsAvailable = availableRounds;
                            }
                        }
                        else
                        {
                            replenishStatus.OwnsAmmo = false;
                            replenishStatus.RoundsAvailable = 0;
                        }

                        hasEnoughAmmo = replenishStatus.OwnsAmmo.Value;

                        cachedWeaponReplenishStates.Add(attackType, replenishStatus); 
                    }
                    else
                    {
                        hasEnoughAmmo = attackType.RoundsToSpend.Value <= replenishStatus.RoundsAvailable.Value;
                       
                    }
                    // hasEnergy = availableFuel.OwnsFuel.Value; // hasEnergyForToolType;

                }
            }

            return hasEnoughAmmo;
        }

        private static bool HasAmmoForAttack(AttackType attackType /* EntityType ammoType, int neededAmmo*/, List<EntityGroup> ownersOfItems, out int availableRounds)
        {
            int totalRounds = 0;
            foreach (var owner in ownersOfItems)
            {
                int currentRounds;
                bool hasAmmo = HasAmmoForAttack(attackType, owner, out currentRounds);

                totalRounds += currentRounds;
            }

            availableRounds = totalRounds;

            return availableRounds >= attackType.RoundsToSpend.Value;          
        }

        public static bool HasAmmoForAttack(AttackType attackType /* EntityType ammoType, int neededAmmo,*/, EntityGroup ownerOfItems, out int availableRounds)
        {                                
            //List<Entity> items;
            
            // find ammo items:

            OwnerAmmoOfType ownerAmmoOfType;

            if (ownerOfItems.AmmoItems.TryGetValue(attackType.UsesAmmoType, out ownerAmmoOfType))
            {
                availableRounds = ownerAmmoOfType.TotalRounds;
                return ownerAmmoOfType.TotalRounds >= attackType.RoundsToSpend.Value;
                   
            }

            availableRounds = 0;
            return false;          
          
        }

        /// <summary>
        /// is the item being carried by an entity who is currently attacking, fleeing or incapacitated? (or any action where it would look stupid if he dropped commandeered items)
        /// </summary>
        /// <returns></returns>
        public static bool IsCarriedByEntityWhoIsOccupied(IKnownEntityData item, out bool itemIsInvalidOrDestroyed)
        {
            itemIsInvalidOrDestroyed = false;

            Entity itemEntity = item as Entity; // will be non-null if carried by our allegiance
            if (itemEntity != null)
            {
                Entity carrier;
                if (!itemEntity.CarriedByAgent(out carrier))
                {
                    itemIsInvalidOrDestroyed = true;
                    return false;
                }

                if (carrier != null)
                {
                    return carrier.IsAttacking() || carrier.IsFleeing(); // perhaps add more here                
                }
            }

            return false;
        }



       /* private CalculateResult FindReplenishItems(WeaponInstanceCombo combo, out bool success) // out double replenishResult)
        {
            // find energy items:

            //replenishResult = 0d;

            double ourScore = combo.Score - GameData.Instance.AIConstants.AmountNewGoalMustBeBetterThanOtherEntityToCancel;

            SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;

            RegionMap footRegionMap = sharedKnowledge.GetMovementMap(entity).RegionMap[TerrainType.TransportType.Foot]; // UWGame.SimSide.Instance.Map.RegionMapManager.HumanFootExposedNormalRegionMap;



           
            CalculateResult result = EvaluateJob.FindReplenishItemsForTool(combo, jobDuration, ourScore, sharedKnowledge, footRegionMap, combo.Weapon, out success);

            if (result == CalculateResult.Processing)
            {
                return CalculateResult.Processing;
            }
            else if (success == false)
            {   // failed to replenish this tool in the set...
                return CalculateResult.Done;
            }
           

            success = true;
            return CalculateResult.Done;
        }*/

   /*     private CalculateResult FindReplenishItemsForTool(WeaponInstanceCombo combo, float jobDuration, double ourScore, SharedKnowledge sharedKnowledge, RegionMap footRegionMap, Entity entityToReplenish, out bool success)
        {
            bool hasEnoughEnergy;
            RequiresEnergy energy;

            success = false;

            hasEnoughEnergy = true;

            if (entityToReplenish.Find(out energy))
            {
                hasEnoughEnergy = energy.HasEnergyForDuration(jobDuration);
            }

            if (hasEnoughEnergy)
            {
                // the tool already has enough energy - replenishment is not needed
                success = true;
                return CalculateResult.Done;
                // continue;
            }

            float maxDistance = GameData.Instance.AIConstants.MaxDistanceForReplenishItems;

            RequiresFuelType requiresFuelType = entityToReplenish.EntityType.RequiresEnergyType.RequiresFuelType;
            List<Entity> foundReplenishItems = null;

            List<Entity> energyItemTakersNeedToBeCancelled = null;
            List<Entity> energyItemsNeedToBeDropped = null;

            if (requiresFuelType != null) //action == ReplenishAction.Refuel)
            {

                float neededFuelBulk = (float)(requiresFuelType.BurnRatePerDay * jobDuration * DateAndTime.DaysPerSeconds);

                float availableFuelBulk = 0f;


                List<ItemDistance> distanceSortedList;

                foreach (var fuelEntityType in requiresFuelType.FuelEntityTypes)
                {
                    // sort the items by distance, excluding the ones far away...
                    if (!energyItemsSortedByDistanceToEntity.TryGetValue(fuelEntityType, out distanceSortedList))
                    {
                        distanceSortedList = new List<ItemDistance>();
                        CalculateResult result = GetAllEntitiesSortedByDistance(fuelEntityType, sharedKnowledge, footRegionMap, distanceSortedList, maxDistance);

                        if (result == CalculateResult.Processing)
                        {
                            return CalculateResult.Processing;
                        }

                        energyItemsSortedByDistanceToEntity.Add(fuelEntityType, distanceSortedList);
                    }


                    // we have the items sorted by distance. start creating the set of valid items:



                    foreach (var item in distanceSortedList)
                    {
                        //first see that we did not take this item already:
                        if (IsItemAlreadyAdded(combo, item))
                        {
                            continue;
                        }

                        // now check our score compared to anyone else currently using the item:
                        GetItemUsersToCancel(item.Entity, ref energyItemTakersNeedToBeCancelled, ref energyItemsNeedToBeDropped, true);

                        if (IsScoreBetterThanAllInvolveds(ourScore, energyItemTakersNeedToBeCancelled))
                        {
                            // good - we found a useable item:
                            if (foundReplenishItems == null)
                            {
                                foundReplenishItems = new List<Entity>();
                            }

                            foundReplenishItems.Add(item.Entity);

                            availableFuelBulk += item.Entity.Bulk;

                            if (availableFuelBulk >= neededFuelBulk)
                            {
                                // we have found all the items we need. store them:
                                if (combo.ReplenishItemsForWeapon == null)
                                {
                                    combo.ReplenishItemsForWeapon = new Dictionary<Entity, Tuple<GoalReplenish.ReplenishAction, List<Entity>>>(); // new Dictionary<Entity, List<Entity>>();
                                }
                                combo.ReplenishItemsForWeapon[entityToReplenish] = new Tuple<GoalReplenish.ReplenishAction, List<Entity>>(GoalReplenish.ReplenishAction.Refuel, foundReplenishItems);

                                //done. handle next tool...
                                success = true;
                                return CalculateResult.Done;
                            }
                        }
                    }

                }
            }

            success = false;
            return CalculateResult.Done;

           
        }*/

      /*  private static bool IsItemAlreadyAdded(WeaponInstanceCombo combo, ItemDistance item)
        {
            if (combo.ReplenishItemsForWeapon != null)
            {
                foreach (var itemAlreadyAdded in combo.ReplenishItemsForWeapon.Item2)
                {
                    if (itemAlreadyAdded == item.Entity)
                    {
                        // was already taken by us. go to the next item
                        return true;
                    }
                    
                }
            }

            return false;
        }*/







       // private Owner ownerOfJobs;


     /*   private bool GetOwnedWeapons(EntityType entityType, out List<Entity> allItems)
        {
            allItems = null;
            List<Entity> currentItems;

            if (entityType.ItemType != null)
            {
                foreach (var owner in ownersOfWeapons)
                {
                    if (owner.InternalOwner.OwnerContent.Items.TryGetValue(entityType, out currentItems))
                    {
                        if (currentItems.Count > 0)
                        {
                            if (allItems == null)
                            {
                                allItems = currentItems;
                            }
                            else
                            {
                                allItems.AddRange(currentItems);
                            }
                        }
                        //return allItems.Count > 0;
                    }
                }                
            }

            return allItems != null && allItems.Count > 0;
        }*/



        private int comboProgress = 0; // TODO move these up top
        private bool scoringWasInterrupted = false; // this keeps track of us getting interrupted and then having to resume in a later frame.


        /*  private CalculateResult ScoreAllCombosAndReturnBest(double minimumRatingToConsider, ref WeaponInstanceCombo bestCombo)
          {
              WeaponInstanceCombo combo;
              double score = -1.0;

              RegionMap footRegionMap =
                  entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel,
                                                                              entity.Intelligence.Allegiance.RepresentativeEntityType.ThreatCategory,
                                                                              entity.Intelligence.ThreatStance).RegionMap[TerrainType.TransportType.Foot];

              double ageContribution = GetAgeContribution();
              double timeOfDayContribution = ScoreTimeOfDay();


              for (; comboProgress < allCombos.Count; comboProgress++)
              {
                  combo = allCombos[comboProgress];

                  //Before scoring, check that the job and weapons are still valid!
                  if (!scoringWasInterrupted || IsComboValid(entity, entityIntelligence, combo)) // only need to check for validity if we are resuming.
                  {
                      int proposedNumberOfWorkers = Common.Clamp(combo.Job.TakenBy.Count + 1, 0, combo.Job.MaxJobPositions);

                    
                      CalculateResult result = ScoreThisJob(footRegionMap, entity, combo.Job, combo.Weapon, weaponSetLethality, proposedNumberOfWorkers,
                                                                                  ageContribution, timeOfDayContribution, ref combo.JobData, out score);
                      if (result == CalculateResult.Done)
                      {
                          combo.Score = score;
                      }
                      else
                      {
                          scoringWasInterrupted = true; // now we need to check all the combos again for availability...
                          return CalculateResult.Processing; // come back later...
                      }
                  }
                  else
                  {
                      combo.Score = 0;
                  }
              }

              // Important! Remove the combos with zero score:
              allCombos.RemoveAll(c => c.Score == 0);

              allCombos.Sort((a, b) => b.Score.CompareTo(a.Score));

              bestCombo = null;


              for (int i = 0; i < allCombos.Count; i++)
              {
                  combo = allCombos[i];
                  if (!scoringWasInterrupted || IsComboValid(entity, entityIntelligence, combo)) // only need to check for validity if we are resuming.
                  {
                      if (combo.Score < minimumRatingToConsider)
                      {
                          break; // the score is not good enough. we have reached the low scores and are now finished
                      }
                  }
                  else
                  {
                      continue; // this combo is no longer available, skip it.
                  }

                  // we have the best job now. See if we need to cancel anyone:

                  // TODO: set a score on equipped weapon items!
                  GetJobsToCancel(combo);//MLo add a condition to favor the current owner of a weapon already in use in attack, not being commandeered


                  if (needsToBeCancelled.Count == 0 && itemsToBeDropped.Count == 0)
                  {   // WINAR!!! No one to cancel
                      bestCombo = combo;
                      break;
                  }
                  else if (IsScoreBetterThanAllInvolveds(combo.Score - GameData.Instance.AIConstants.AmountNewGoalMustBeBetterThanOtherEntityToCancel, needsToBeCancelled))
                  {
                      // there are people doing this job. Only take it from them if our score is better than theirs by a certain margin.
                      bestCombo = combo;
                      break;
                  }

                  // the job was taken and we didn't score high enough to take it from them. continue looking.
              }

              return CalculateResult.Done;
          }*/

        /// <summary>
        /// this method signature matches IScoreJob!
        /// Only gets called from outside the evaluator. (when an updated score is needed)
        /// </summary>  
        public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Jobs.Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution,
            out double rating, ToolParams? toolParams, AttackParams? attackParams, HaulingParams? haulingParams) // List<Entity> tools, float? toolsetProductivity, Dictionary<EntityType, ReplenishStatus> availableFuel)
        {

            WeaponInstanceCombo combo = new WeaponInstanceCombo();
            combo.AttackData = new WeaponInstanceComboAttackData();


            combo.AttackData.JobData = new WeaponInstanceComboJobData();

            combo.AttackData.JobData.Job = (AttackJob)job;


            if (attackParams.HasValue) // always the case..?
            {
                combo.AttackData.bodyPartID = attackParams.Value.BodyPartToAttackID;
                combo.AttackData.AttackType = attackParams.Value.AttackType;

                if (attackParams.Value.Weapon.HasValue)
                {
                    IKnownEntityData weaponData;
                    EntityResult result = entityIntelligence.GetKnownData(attackParams.Value.Weapon.Value.Entity, out weaponData);
                    if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown)
                    {
                        rating = 0;
                        return CalculateResult.Done; 
                    }

                    combo.Weapon = weaponData; 
                }

                IKnownEntityData targetData;
                if (EntityDataResultCausesSkip(entityIntelligence.GetKnownData(combo.AttackData.JobData.Job.Target.Value, out targetData)))
                {
                    rating = 0;
                    return CalculateResult.Done;
                }

                BodyPart BodyPart = targetData.Body.FindBodyPart(combo.AttackData.bodyPartID);
                combo.AttackData.EstimatedDamageScore = GameData.Instance.AttackScoresAgainstBodyParts[combo.AttackData.AttackType][BodyPart.BodyPartType];
                //combo.AttackData.EstimatedDamageScore = attackParams.Value.
            }

            float energyLevelFactor = GoalDoAttack.GetEnergyLevelFactorOnDamage(entity, combo.AttackData.AttackType);
            float fitnessScore = ScoreFitness(entity);

            return ScoreThisAttackJob(regionMap, entity, //job,
                proposedNumberOfWorkers, ageContribution, timeContribution, fitnessScore, energyLevelFactor,
                combo, //attackParams.Value.AttackType,              
                out rating);


        }

        private static float ScoreFitness(Entity entity)
        {
            BodyComponent body;
            entity.Find(out body);

            return body.Body.GlobalHitpoints / body.Body.MaxHitpoints;

        }


        /// <summary>
        /// this method is called from ScoreJobs and from the IScoreJob method implementation.
        /// </summary>      
        public CalculateResult ScoreThisAttackJob(RegionMap regionMap, Entity entity, //Jobs.Job job, 
                                                            int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, float? fitnessScore, float? energyFactor, //float? estimatedDamageScore,
                                                            WeaponInstanceCombo combo, /*Entity weapon, AttackType attackType, BodyPart bodyPart,*/ out double score)
        {
            score = 0f;

            if (!ageContribution.HasValue)
                ageContribution = GetAgeContribution();

            if (!timeContribution.HasValue)
                timeContribution = ScoreTimeOfDay();

            
            AttackJob attackJob = combo.AttackData.JobData.Job;

            Intelligence entityIntelligence = entity.Intelligence;

            // see if we already scored this job (not the weapons, just the job itself)
            if (combo.AttackData.AttackScore == null)
            {

                double jobScore = 0d;
                
                // score the attack type vs body part and the distance to the target, among other things
                CalculateResult jobResult = attackJob.ScoreThisJobWithoutWeapon(entity, entityIntelligence, combo, proposedNumberOfWorkers, ageContribution, timeContribution,
                                                            ref jobScore, fitnessScore, Priority, energyFactor);

                if (jobResult != CalculateResult.Done)
                    return CalculateResult.Processing;

                combo.AttackData.AttackScore = jobScore; // we can reuse this for the other weapons
                
            }


            // create new function ScoreDistance() : return a score for distance
            // unarmed: entity -> target
            // weapon: entity -> weapon -> target
            // for ranged attacks, subtract max attack range
            // cache value? for unarmed: cache in JobData (UnarmedDistanceScore) - is this useful? research.. see if there is more than one combo per unarmed attack type against a target
            // for weapons - there may be only one combo for each weapon, so caching is not needed

            
            double? weaponScore = null;
            double distanceScore;

            if(combo.AttackData.AttackScore > 0d)
            {
                
                CalculateResult distanceScoreResult = ScoreDistance(entity, combo, attackJob.Target.Value, ageContribution, timeContribution,  //jobData.AgentAssignedLocation,
                                                entityIntelligence, regionMap, Priority, out distanceScore);
                
                if (distanceScoreResult == CalculateResult.Processing)
                    return CalculateResult.Processing;


            
                if (combo.Weapon != null) // continue scoring the weapon if the job is valid.
                {

                   /* if (combo.Weapon.EntityID == (EntityID)9)
                    {

                    }*/

                    // now score the weapon (distance, condition etc.):
                    double weaponScored;
                    CalculateResult weaponResult = attackJob.ScoreWeapon(entity, combo.Weapon, distanceScore, ageContribution, timeContribution,  //jobData.AgentAssignedLocation,
                                                    Priority, combo.AttackData.AttackType, out weaponScored);
                    weaponScore = weaponScored;
                    if (weaponResult == CalculateResult.Processing)
                        return CalculateResult.Processing;

                    if (!Common.IsZero(weaponScore))
                    {
                        if (combo.AttackData.AttackType.UsesAmmo != null)
                        {
                            bool hasEnoughAmmo = combo.Weapon.HasEnoughAmmo(combo.AttackData.AttackType.UsesAmmoType, combo.AttackData.AttackType.RoundsToSpend.Value);

                            // score lower if the weapon is not loaded                    
                            if (!hasEnoughAmmo)
                            {
                                weaponScore *= 0.65f;
                            }
                        }
                    }
                }
                else
                {
                    weaponScore = distanceScore;
                }
           
            
            }
            
            score = attackJob.CombineJobAndWeaponScore(combo.AttackData.AttackScore.Value, weaponScore);

            EvaluateJob.ApplyJobPriorityModifier(attackJob.Priority, ref score); // apply the user priority

            return CalculateResult.Done;
           
        }

        private GoalEvaluator.CalculateResult ScoreDistance(Entity entity, WeaponInstanceCombo combo, EntityID target,
                   double? ageContribution, double? timeContribution,
                   Intelligence entityIntelligence, RegionMap regionMap, float priority, out double score)
        {
            score = 0d;
            double locationScore;

            SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;

            IKnownEntityData targetData;
            if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(target, out targetData)))
            {
                return GoalEvaluator.CalculateResult.Done;
            }
            GoalEvaluator.CalculateResult locationResult;

            float maxRange = 0;
            if (combo.AttackData.AttackType.MaxRange.HasValue)
            {
                maxRange = combo.AttackData.AttackType.MaxRange.Value;
            }


            if (entity.EntityType.IntelligenceType.IsMobile)
            {
                if (combo.Weapon != null)
                {
                    float distanceToTool;

                    locationResult = ProcessJob.ScoreToolLocation(entity, combo.Weapon, entity.PlaySiteLocation, targetData.Location.Value, sharedKnowledge,
                                                                                        regionMap, out distanceToTool, out locationScore, maxRange);
                }
                else
                {
                    locationResult = ProcessJob.ScoreToLocation(entity, entity.PlaySiteLocation, targetData.Location.Value, sharedKnowledge,
                                                                                        regionMap, out locationScore, maxRange);
                }
            }
            else
            {
                // non-mobiles score high for everything in their range
                if (Common.DistanceOctile(entity.PlaySiteLocation, targetData.PlaySiteLocation) <= maxRange)
                {
                    locationScore = 1;
                }
                else
                {
                    locationScore = 0;
                }

               
                locationResult = CalculateResult.Done;
                
            }
            

            if (locationResult == GoalEvaluator.CalculateResult.Processing)
            {
                return GoalEvaluator.CalculateResult.Processing;
            }

            score = locationScore * 0.7f;
            return CalculateResult.Done;
        }

        
       


        private bool GetJobsToCancel(WeaponInstanceCombo combo)
        {
           
            needsToBeCancelled.Clear();
            itemsToBeDropped.Clear();

            //if (combo.AttackData.JobData.Job.TakenBy.Count >= combo.AttackData.JobData.Job.MaxJobPositions) //  0)
            //{
            //    needsToBeCancelled.Add(combo.AttackData.JobData.Job.TakenBy.GetLowestScorer());
            //}

            List<Entity> takenBy = new List<Entity>();
            Job job = combo.AttackData.JobData.Job;
            if (job.TakenBy.Count > 0)
            {
                job.TakenBy.GetLowestScorer();
                for (int i = 0; i < job.TakenBy.Count; i++)
                {
                    takenBy.Add(job.TakenBy.Get(i));
                }

                while (takenBy.Count >= job.MaxJobPositions)
                {
                    needsToBeCancelled.Add(takenBy[0]);
                    takenBy.RemoveAt(0);
                }
            }

            if (combo.Weapon != null)
            {
                if (combo.Weapon.EntityType.ItemType != null)
                {
                    // considers AssignedToJob as well
                    GetItemUsersToCancel(combo.Weapon, ref needsToBeCancelled, ref itemsToBeDropped, false);
                }
                
            }

            // remove duplicates:
            needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
            itemsToBeDropped = itemsToBeDropped.Distinct().ToList();

            return true;
        }

        public override bool CancelCurrentTakers()
        {
            return CancelEntities(needsToBeCancelled, itemsToBeDropped);
        }

        public override bool CanTakeGoal()
        {
           
            if (bestCombo != null)
            {
                if (EvaluateJob.IsJobValid(bestCombo.AttackData.JobData.Job) == false)
                {
                    return false;
                }
                needsToBeCancelled.Clear();
                itemsToBeDropped.Clear();

                if (bestCombo.Weapon == null || !entity.Contains.Contains(bestCombo.Weapon.EntityID))
                {

                }

                if (GetJobsToCancel(bestCombo))
                {
                    // these checks should also be made in FindBestCombo, right before assigning bestCombo:
                    if (CanCancelAndForceDropItems(bestCombo.Score))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private bool CanCancelAndForceDropItems(double ourScore)
        {
            if (CanForceDropItems(itemsToBeDropped)
                && IsScoreBetterThanAllInvolveds(ourScore, needsToBeCancelled))
            {
                return true;
            }

            return false;
        }

    }
}