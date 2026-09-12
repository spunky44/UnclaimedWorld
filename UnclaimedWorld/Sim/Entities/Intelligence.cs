using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using System.Collections;
using UWGame.SimSide.AI;
using UWGame.SimSide.Jobs;
using GameStateManagement;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Entities.Body;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Processes;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI.StrategicDecisions;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities.Skills;
namespace UWGame.SimSide.Entities
{
    public class Intelligence: Component
    {
        public string FirstName
        {
            get;
            private set;
        }

        public string LastName
        {
            get;
            private set;
        }

        /// <summary>
        /// never used..?
        /// </summary>
     /*   public string ShortName
        {
            get;
            private set;
        }*/

        /// <summary>
        /// the entities we share a hive-mind with... 
        /// don't save references to this object (or SharedKnowledge) permanently. Perhaps, at some point, it will become possible for entites to switch allegiances.
        /// </summary>
        public Allegiances.Allegiance Allegiance; // = AllegianceType.Player; 
        private AllegianceID snapshotAllegianceID; // used in snapshot

        /// <summary>
        /// robots, persons (and animals?) should belong to an expedition
        /// </summary>
        public Expedition CurrentExpedition;
        ExpeditionID? snapshotExpedition;
      

        /// <summary>
        /// only filled for entities in the player allegiance
        /// </summary>
        //    public Expeditions.Expedition PlayerExpedition;

        public AI.Memory Memory = new AI.Memory();

        public CombatInfo CombatInfo;

        public ConversationID? CurrentConversationID;

        public event Action TalkActionEnded;

       

        /// <summary>
        /// should be parts as well...
        /// </summary>
        public Dictionary<EntityType, EntityID> IntrinsicTools;

        /// <summary>
        /// should be parts as well...
        /// </summary>
        public Dictionary<EntityType, EntityID> IntrinsicWeapons;



        // had to move this here since the body may also turn based on interest
        #region head turns and interest
        /// <summary>
        /// how interested in this entity/spot am I
        /// </summary>
        public float interestLevel = 0f;


        /// <summary>
        /// we can look at either an entity or a spot
        /// </summary>
        private EntityID entityIDToLookAt = EntityID.Invalid;
        private Vector3? locationToLookAt = null;

        public EntityID EntityIDToLookAt
        {
            get { return entityIDToLookAt; }
        }
        public Vector3? LocationToLookAt
        {
            get { return locationToLookAt; }
        }

        /// <summary>
        /// tracks the head's facing angle, relative to the body. 0 is straight forward.
        /// Also, is set to 0 when not in head turn state
        /// </summary>     
        private float lookAngle = 0;

        /*

        */
        /// <summary>
        /// how interested in this entity/spot am I
        /// </summary>
        //  private float interestLevel = 0f;

        private float maxAngleToTurnHead;

        #endregion

        public PathPlanner PathPlanner;
        private CyclableID? pathPlannerID;

        public ResourceMapForAgent CropsMapForAgent;
        CyclableID? snapshotCropsMap;

      
        public GroupStatistics Statistics;

     

        public float Comfort
        {
            get
            {                
                return Parent.GetEffect(SimEffects.AffectsNumbers.AgentComfort, 0f);                
            }
        }


        public Intelligence()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
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

            this.FirstName = sn.DoString(FirstName);
            this.LastName = sn.DoString(LastName);
           // this.ShortName = sn.DoString(ShortName);

            DisableAI = sn.DoBool(this.DisableAI);
            DPSmoraleDamageFactor = sn.DoFloat(this.DPSmoraleDamageFactor);
            entityIDToLookAt = sn.DoEntityID(this.entityIDToLookAt);
            evaluatorInfluenceMap = sn.DoJaggedArray(this.evaluatorInfluenceMap);
            GroupMoveAssignedWaypointRay = sn.DoVector3Nullable(this.GroupMoveAssignedWaypointRay);
            interestLevel = sn.DoFloat(this.interestLevel);
            IsAtGroupMoveDestination = sn.DoBoolNullable(IsAtGroupMoveDestination);
            IsAwakeAndActive = sn.DoBool(this.IsAwakeAndActive);
            isStealthy = sn.DoBool(this.isStealthy);
            locationToLookAt = sn.DoVector3Nullable(this.locationToLookAt);
            lookAngle = sn.DoFloat(this.lookAngle);
            maxAngleToTurnHead = sn.DoFloat(this.maxAngleToTurnHead);
            morale = sn.DoFloat(this.morale);
            panicLevel = sn.DoInt32(this.panicLevel);
            panicLevelIsDirty = sn.DoBool(this.panicLevelIsDirty);          
            Skills = sn.DoDictionary(this.Skills);
            SpokenLine = sn.DoString(this.SpokenLine);
            spokenLineDuration = sn.DoDouble(this.spokenLineDuration);
            spokenLineElapsedTime = sn.DoDouble(this.spokenLineElapsedTime);

            AttacksVermin = sn.DoBool(AttacksVermin);
            triggersOnCooldown = sn.DoDictionary(this.triggersOnCooldown);           
           
           
            ProtectionLevel = sn.DoEnum(ProtectionLevel);
            threatStance = sn.DoEnum(threatStance);
            FollowerStatus = sn.DoEnum(FollowerStatus);
            hitsTakenLastFewSeconds = sn.DoQueue(hitsTakenLastFewSeconds); 
           
            snapshotBrain = sn.SnapshotID<Goal, GoalID>(Brain);
            CombatInfo = (CombatInfo)sn.DoISnapshot(this.CombatInfo);
            currentGoalUtility = sn.DoDoubleNullable(currentGoalUtility);
            Memory = (Memory)sn.DoISnapshot(Memory);
            CurrentConversationID = sn.DoEnumNullable(CurrentConversationID);  
          
            snapshotAllegianceID = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(Allegiance);
            snapshotCropsMap = sn.SnapshotID<ICyclable, CyclableID>(CropsMapForAgent);
            pathPlannerID = sn.SnapshotID<ICyclable, CyclableID>(PathPlanner);  // sn.DoEnumNullable(pathPlannerID);


            EmigrateDecider = (EmigrateDecider)sn.DoISnapshot(EmigrateDecider);
            Statistics = (GroupStatistics)sn.DoISnapshot(Statistics);
            snapshotExpedition = sn.SnapshotID<Expedition, ExpeditionID>(CurrentExpedition);
            IntrinsicTools = sn.DoDictionary(IntrinsicTools);
            IntrinsicWeapons = sn.DoDictionary(IntrinsicWeapons);
            profession = sn.DoGameData(profession);
            professionIsDirty = sn.DoBool(professionIsDirty);

            sn.Ignore(CropsMapForAgent);
            sn.Ignore(PathPlanner);
            sn.Ignore(triggersToActivate);
            sn.Ignore(TopScoringJobs);
            sn.Ignore(triggerCleanupRegulator);
            sn.Ignore(combatStatRegulator);
            sn.Ignore(CurrentActionScoreRegulator);
            sn.Ignore(Brain);
            sn.Ignore(TalkActionEnded); // only used from client
            //sn.Ignore(profession);
            //sn.Ignore(professionIsDirty)

            sn.Postpone(groupMoveAssignedWaypoint);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);
                      

            CombatInfo.LoadPostProcess(sn);
            Memory.LoadPostProcess(sn);

            if (snapshotExpedition.HasValue)
            {
                CurrentExpedition = Expedition.FindByID(snapshotExpedition.Value);
            }

            if (pathPlannerID.HasValue)
            {
                PathPlanner = (PathPlanner)LookUp<ICyclable, CyclableID>.FindByID(pathPlannerID.Value);
            }

            if (snapshotBrain.HasValue)
            {
                Brain = (GoalThink)LookUpGoals.FindByID(snapshotBrain.Value);
            }

            Allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegianceID);

            if (snapshotCropsMap.HasValue)
            {
                CropsMapForAgent = (ResourceMapForAgent)LookUp<ICyclable, CyclableID>.FindByID(snapshotCropsMap.Value);
            }

            if (EmigrateDecider != null)
            {
                EmigrateDecider.LoadPostProcess(sn);
            }

            foreach (var item in Skills)
            {
                item.Value.LoadPostProcess(sn);
            }

            Statistics.LoadPostProcess(sn);

            CreateRegulators();
        }

     

      
        /*
        public void ResetCurrentProductivity()
        {
            currentTotalProductivity = null;
            skillProductionFactorIsDirty = false;
            currentSkillInUse = null;
            currentEnergyLevel = null;
            currentToolProductivity = null;
            ToolCombinationNameForDisplay = null;

            productivityIsDirty = false;           
        }*/

        private ThreatStance threatStance = ThreatStance.Normal;
        public ThreatStance ThreatStance
        {
            get { return threatStance; }
        }


        public bool CanAttackVermin(out CombatAreaJob patrolJob)
        {
            patrolJob = null;

            if (Parent.EntityType.IntelligenceType.HuntsVermin)
            {
                return true;
            }
            else
            {
                patrolJob = Memory.GetLastCombatAreaJob(); //GetPatrolJob();                
                if (patrolJob != null)
                {
                    if (patrolJob.CanAttackVermin)
                    {
                        return true;
                    }
                }              

                return AttacksVermin; 
            }
        }

        /// <summary>
        /// No longer used by sentry, the ammo policy is sufficient
        /// used by sentry... special action can turn on/off
        /// </summary>
        public bool AttacksVermin = false;

       
        private bool isStealthy;
        public bool IsStealthy
        {
            get
            {
                return isStealthy;
            }
            set
            {
                if (value != isStealthy)
                {
                    isStealthy = value;
                    if (isStealthy)
                    {
                        Parent.Renderable.SetAnimationStateFlag(ClientSide.Renderables.AnimModifier.Stealthy);
                    }
                    else
                    {
                        Parent.Renderable.ClearAnimationStateFlag(ClientSide.Renderables.AnimModifier.Stealthy);
                    }
                }
            }
        }

        /// <summary>
        /// a flag that tells the state of the agent - is it awake and capable?
        /// </summary>
        public bool IsAwakeAndActive = true;

        public ProtectionLevel ProtectionLevel = ProtectionLevel.Exposed;

        private float morale = 1f;
        /// <summary>
        /// 0 - 1
        /// </summary>
        public float Morale
        {
            get
            {
                return morale;
            }
            set
            {
                if (value != morale)
                {
                    morale = value;
                    panicLevelIsDirty = true;
                }
            }
        }



       
        /// <summary>
        /// is null before ComeOnline()!!!
        /// </summary>
        public GoalThink Brain;
        GoalID? snapshotBrain;

        //private StrategyDecider StrategyDecider;
        public EmigrateDecider EmigrateDecider;


        /// <summary>
        /// Signal if the group movement member has reached its destination and is waiting for orders.
        /// </summary>
        public bool? IsAtGroupMoveDestination;

        public FollowerStatus FollowerStatus = FollowerStatus.Normal;

        public Regulator CurrentActionScoreRegulator;

     

        /// <summary>
        /// shuts off trigger events for specific triggers for a while
        /// value is next tick where we will receive message
        /// </summary>
        private Dictionary<TriggerID, long> triggersOnCooldown = new Dictionary<TriggerID, long>();

     //   private SortedList<long, Trigger> triggersOnCooldownByNextTimePoint = new SortedList<long, Trigger>();
       

        /// <summary>
        /// for debugging only
        /// </summary>
        public bool DisableAI = false;

       

        public Dictionary<SkillType, Skill> Skills = new Dictionary<SkillType, Skill>();

        bool professionIsDirty = true;

        private ProfessionType profession;
        /// <summary>
        /// can be null for low-skilled individuals...
        /// </summary>
        public ProfessionType Profession
        {
            get
            {
                if (professionIsDirty)
                {
                    profession = null;
                    var orderedSkills = Skills.OrderByDescending(kvp => kvp.Value.Value); //.FirstOrDefault();

                    foreach (var item in orderedSkills)
                    {
                        if (item.Value.Value < GameData.Instance.Constants.ExpertSkillLevel)
                        {
                            break;
                        }
                        else if (item.Key.ProfessionType != null)
                        {
                            profession = item.Key.ProfessionType; // GameData.Instance.AllProfessionTypes[item.Key.ProfessionKey];
                            break;
                        }
                    }

                    professionIsDirty = false;
                }

                return profession;
            }
        }

        //public GetEnergyLevel()


        public string SpokenLine;
        /// <summary>
        /// we need to keep track of elapsed time for when we want to signal the conversation that is has ended...
        /// </summary>
        private double spokenLineElapsedTime;
        private double spokenLineDuration;

        //public float SpokenLineDisplaySecondsLeft; //SpokenLineTimestamp;

      
        public float GetSkillValue(SkillType skillType)
        {
            if (skillType == null)
                return 1f;

            Skill skill;
            if (Skills.TryGetValue(skillType, out skill) == false)
            {
                return 0.0f;
            }

            float value = skill.Value;
            if (skillType.GiveExpertSkillBonus)
            {
                float expertSegment = 1f - GameData.Instance.Constants.ExpertSkillBonusThreshold;
                float bonusLevel = (value - GameData.Instance.Constants.ExpertSkillBonusThreshold) / expertSegment;
                if (bonusLevel > 0f)
                {
                    value = MathHelper.Lerp(GameData.Instance.Constants.ExpertSkillBonusThreshold, 1f + GameData.Instance.Constants.ExpertSkillBonus, bonusLevel);
                }
            }

            return value;
        }

        public void SetSkillValue(SkillType skillType, float value)
        {
            Skill skill;
            if (Skills.TryGetValue(skillType, out skill))
            {
                skill.Value = value;
            }

            professionIsDirty = true;
        }

       


        /// <summary>
        /// TODO: make property in Entity to expose this value for scripts and panels
        /// </summary>
        /// <param name="skillType"></param>
        /// <returns></returns>
        public float GetSkillProductionFactor(SkillType skillType)
        {
            if (skillType == null)
                return 1f;

            Skill skill;
            if (Skills.TryGetValue(skillType, out skill) == false)
            {
                return 0.0f;
            }
            return skill.ProductionFactor;
        }

        private bool panicLevelIsDirty = true; // false;
        private int panicLevel;
        public int PanicLevel
        {
            get
            {
                if (panicLevelIsDirty)
                {
                    ComputePanicLevel();
                    panicLevelIsDirty = false;
                }
                return panicLevel;
            }
        }


        private Waypoint groupMoveAssignedWaypoint;

        /// <summary>
        /// store the ray as well, so we can test dependably if the member is at the waypoint. TODO: Move into Waypoint struct?
        /// </summary>
        public Vector3? GroupMoveAssignedWaypointRay;

       
        private double? currentGoalUtility;

        /// <summary>
        /// a saved score to fall back on by TopLevelGoals
        /// </summary>
        /// <returns></returns>
        public double? GetCurrentGoalUtility()
        {
            return currentGoalUtility;
        }

        public List<GoalAndScore> TopScoringJobs = new List<GoalAndScore>();


        /// <summary>
        /// probably only the evaluators should call this.
        /// NEW: calls Activate immediately to set all locks
        /// </summary>
        /// <param name="g"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public bool SetTopLevelGoal(ITopLevelGoal g, double score)
        {
            Goal goal = g as Goal;
            if (Brain.NotPresent(g.GetType()))
            {
                Brain.AddSubgoal(goal);

                goal.ActivateIfInactive(); // NEW!

                currentGoalUtility = score;

                Parent.DebugLog.Add(string.Format("SetTopLevelGoal: {0}", goal.ToString()));

                return true;
            }
            else
            {
                goal.RemoveIDEntry(); //If we do not plan to use this new goal. We should remove it from the collection.
            }
            
            return false;
        }

        /// <summary>
        /// helper method
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public EntityResult GetKnownData(EntityID entityID, out IKnownEntityData data)
        {          
            return Allegiance.SharedKnowledge.GetKnownData(entityID, out data);
        }

        /// <summary>
        /// helper method
        /// </summary>
        /// <param name="processID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ProcessResult GetKnownProcessData(SimProcessID processID, out IKnownProcess data)
        {
            return Allegiance.SharedKnowledge.PlaySiteKnowledge.GetKnownProcessData(processID, out data);
        }

       /// <summary>
       /// returns true if the entity is seen directly
       /// </summary>
       /// <param name="entityID"></param>
       /// <param name="entity"></param>
       /// <returns></returns>
        public bool GetEntitySeenDirectly(EntityID entityID, out Entity entity)
        {
            IKnownEntityData data;
            EntityResult result = GetKnownData(entityID, out data);
            if (result != EntityResult.SeenDirectly)
            {
                entity = null;
                return false;
            }

            entity = (Entity)data;
            return true;
        }

        private void ComputePanicLevel()
        {
            if (Parent.EntityType.IntelligenceType.CanPanic == false
                || Parent.EntityType.IntelligenceType.IsMobile == false)
            {
                // never panic/flee:
                panicLevel = (int)ThreatStance.Bold;
            }
            else if (!CanDefend() && !CanAttack())
            {
                // set the level as low as possible (always panic/flee):
                panicLevel = (int)ThreatStance.Cautious;
            }
            else
            {

                BodyComponent body;
                Parent.Find(out body);

                float hitpointsFractionLeft = body.Body.GetHitpointsFractionUntilUnconsciousness();//body.MaxHitpoints;
                float hurtVitalPartsMultiplier = body.Body.GetHurtVitalBodyPartFactorForMorale();
               
                float panicScore = hitpointsFractionLeft * Morale * hurtVitalPartsMultiplier; // *Parent.EntityType.IntelligenceType.FightOverFleeProbability;

                panicLevel = (int)MathHelper.Lerp((float)ThreatStance.Cautious, 100f, panicScore);

            }

            panicLevelIsDirty = false;


            if (threatStance != ThreatStance.Bold)
            {
                SetNormalOrCautiousThreatStance();
            }
            
        }


        public void SetSkill(string skillKey, float value)
        {
            SkillType skillType = GameData.Instance.AllSkillTypes[skillKey];
            Skill skill;
            if (!Skills.TryGetValue(skillType, out skill))
            {
                skill = new Skill(value, skillType);
                Skills.Add(skillType, skill);
            }

            skill.Value = value;
        }

        private void SetNormalOrCautiousThreatStance()
        {
            if (StanceShouldBeCautious()) // PanicLevel < (int)ThreatStance.Normal)
            {
                // set cautious stance
                threatStance = ThreatStance.Cautious;

                // the blocking level is the lowest valid value:
                panicLevel = Common.ClampBottom(panicLevel, (int)ThreatStance.Cautious);
            }
            else
            {
                // set normal stance

                threatStance = ThreatStance.Normal;
            }
        }


        public void SetBoldStance()
        {
            threatStance = ThreatStance.Bold;
        }


        public void SetLastAgentOnPost(CombatAreaJob job)
        {
            Memory.SetRecentlyOnPatrol(job.ID); // make sure we can pursue vermin targets for a while...
            job.SetLastAgentOnPost(Parent.ID);  // hopefully, we can pick up this job again...?
        }

        public void ResetLastAgentOnPost(CombatAreaJob job)
        {
            Memory.SetRecentlyOnPatrol(null); // reset
            job.RemoveLastAgentOnPost(Parent.ID);
        }


        #region AI Status functions

        /// <summary>
        /// no, not really...
        /// </summary>
        /// <returns></returns>
        public bool IsSleeping()
        {
            if (Brain != null && Brain.Subgoals.Count > 0)
            {
                if (Brain.Subgoals.Peek() is GoalSleep)
                {
                    if ((Brain.Subgoals.Peek() as GoalSleep).Status != Status.Completed)
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        public bool IsIdle()
        {
            if (Brain.Subgoals.Count > 0)
                return Brain.Subgoals.Peek() is GoalTakeFive;

            return false;
        }

        public bool IsEmigrating()
        {
            if (Brain.Subgoals.Count > 0)
                return Brain.Subgoals.Peek() is GoalEmigrate;

            return false;
        }

        public bool IsAttacking()
        {
            return CombatInfo.Target != null;
        }

        public bool IsFleeing()
        {
            // hmm..  
            return Parent.Locomotor.LeggedLocomotor.TargetSpeed == Goal.MovementSpeeds.Run;
        }

        #endregion

        /// <summary>      
        /// Scratchpad used by all evaluators. Dimensions = map dimensions
        /// </summary>     
        public /*byte*/ ushort[][] EvaluatorInfluenceMap
        {
            get
            {
                if (evaluatorInfluenceMap == null)
                {
                    Common.InitJaggedArray(ref evaluatorInfluenceMap, The.Map.mapTileWidth, The.Map.mapTileHeight);
                }

                return evaluatorInfluenceMap;
            }
        }


        private /*byte*/ ushort[][] evaluatorInfluenceMap;

       
        /// <summary>
        /// for optimization reasons, we store the assigned group movement waypoint here.
        /// </summary>
        public Waypoint GroupMoveAssignedWaypoint
        {
            get { return groupMoveAssignedWaypoint; }
            set
            {
                groupMoveAssignedWaypoint = value;
                if (groupMoveAssignedWaypoint != null)
                {
                    GroupMoveAssignedWaypointRay = GoalTraverseEdgeBetweenWaypoints.ComputeWaypointRay(groupMoveAssignedWaypoint.Location, Parent);
                }
                else
                {
                    GroupMoveAssignedWaypointRay = null;
                }
            }
        }



        public Intelligence(Entity parent)
            : base(parent)
        {
           
            CombatInfo = new CombatInfo();

            if (parent.EntityType.IntelligenceType.IntrinsicToolTypes != null)
            {
                IntrinsicTools = new Dictionary<EntityType, EntityID>();
            }

            if (parent.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
            {
                IntrinsicWeapons = new Dictionary<EntityType, EntityID>();
            }

            CreateRegulators();
        }

        public void Initialize(Allegiances.Allegiance allegiance)
        {
           /* if (FirstName != null && LastName != null)
            {
                ShortName = FirstName[0] + ". " + LastName;
            }
            else
            {
                ShortName = FirstName ?? LastName;
            }*/

            if (allegiance == null)
            {
                if (The.Sim.Mode == Sim.EngineMode.Game)
                {
                    throw new Exception("Allegiance has not been set on the Entity");
                }
            }
            else
            {
                Allegiance = allegiance;
                Allegiance.AddMember(Parent);
            }


            Statistics = new GroupStatistics(Parent.CanIterateEntitiesID, allegiance); // requires the ID to have been set
           
        }


        public void NotifyFoodProcessesChanged()
        {
            if (Allegiance != null)
            {
                Allegiance.FoodExtraction.SetIsDirty();
            }
        }


        public void SpeakLine(string lineKey, string defaultText, float durationInSeconds, bool turnTowardsListeners, Conversation conversation)
        {
            // only seen entities can be heard by the player:
           /* IKnownEntityData data;
            if (The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(Parent.EntityID, out data) != EntityResult.SeenDirectly) // DECOUPLE
            {
                return;
            }*/

            Person person;

            // see if there is a key for a personality infused line:
            string personalityInfusedLine = null;
            if (!string.IsNullOrEmpty(lineKey) && Parent.Find(out person))
            {
                personalityInfusedLine = person.Personality.PersonalityType.GetSpokenLine(lineKey, defaultText);
            }
            else 
            {
                personalityInfusedLine = defaultText;
            }


            // only seen entities can be heard by the player:
            The.Client.SpeakLine(Parent, durationInSeconds, conversation, personalityInfusedLine);

            SpokenLine = personalityInfusedLine;
            spokenLineDuration = durationInSeconds;  // we need to keep track of elapsed time for when we want to signal the conversation that is has ended...
            spokenLineElapsedTime = 0f;

            // save this so we won't interrupt ourselves:
            CurrentConversationID = conversation.ID;

            if (turnTowardsListeners &&
                Brain != null) // brain is null for immigrants joining the expedition, right before ComeOnline gets called...
            {
                Brain.SendMessage(new Message(Message.MessageTypes.SpeakLine));
            }
           
           // SpokenLineTimestamp = The.Sim.GameTime.TotalGameTime.Ticks;
        }

        
                

        private Queue<Tuple<TimeSpan, float>> hitsTakenLastFewSeconds = new Queue<Tuple<TimeSpan, float>>();

        private Regulator combatStatRegulator;

        /// <summary>
        /// is at least 1
        /// </summary>
        private float DPSmoraleDamageFactor;
        private void CalculateDamagePerSecondTaken()
        {
            DPSmoraleDamageFactor = hitsTakenLastFewSeconds.Sum(t => t.Item2);

            DPSmoraleDamageFactor *= 0.5f;

          //  DPSmoraleDamageFactor = Common.ClampBottom(DPSmoraleDamageFactor, 1f);

            //return total;

            /*foreach (Tuple<TimeSpan, double> item in hitsTakenLastFewSeconds)
            {
                
            }
             * */
        }
        public void DamageMorale(float physicalDamage, BodyPart hitBodyPart)
        {
            
            float damageToMorale = 0f;

            float relativeDamageAmount = 0;
            BodyComponent body;
            if (Parent.Find(out body))
            {                
                if (body.Body.GlobalHitpoints > 0f)
                {
                    relativeDamageAmount = physicalDamage / body.Body.GlobalHitpoints;
                   // damageToMorale = 0.6f * relativeDamageAmount;
                }               
            }

            if (hitBodyPart.IsVital())
            {
                float vitalBodyPartDamageAmountBoost = GameData.Instance.Constants.vitalBodyPartDamageEvaluationBoost * hitBodyPart.GetMoraleDecreaseFactor(physicalDamage);
                if (vitalBodyPartDamageAmountBoost != 0.0f)
                {
                    //If vital body parts are being hit we become more and more likely to flee depending on how endagered the body part is
                    relativeDamageAmount *= vitalBodyPartDamageAmountBoost;
                }
            }
                       
            relativeDamageAmount = Common.ClampBottom(relativeDamageAmount, GameData.Instance.Constants.MoraleDamageForZeroDamageAttacks); 

             // log the hit with an expiration time
            hitsTakenLastFewSeconds.Enqueue(new Tuple<TimeSpan, float>(The.Sim.TotalUnPausedGameTime.Add(new TimeSpan(0, 0, GameData.Instance.Constants.TimeIntervalForDPSMoraleFactor)), relativeDamageAmount));

            CalculateDamagePerSecondTaken();

            // we take a bigger morale hit if we have been receivving a lot of damage/hits over a short period of time!
           // damageToMorale *= DPSmoraleDamageFactor; // hitsLastFewSeconds;
            damageToMorale = DPSmoraleDamageFactor; // hitsLastFewSeconds;

           // damageToMorale = Common.ClampBottom(damageToMorale, 0.06f);

            damageToMorale *= (1f -Parent.EntityType.IntelligenceType.Courage);

            Morale = Common.ClampBottom(Morale - damageToMorale, 0f);
        }

       /* public void body_HitpointsChanged(object sender, EventArgs e)
        {
            panicLevelIsDirty = true;
        }*/


        public void SetPanicLevelDirty()
        {
            panicLevelIsDirty = true;
        }

        public void SetTriggerCooldown(Trigger trigger, long? periodInTicks)
        {
            if (periodInTicks.HasValue)
            {
                if (!triggersOnCooldown.ContainsKey(trigger.ID))
                {
                    long activeTimepoint = The.Sim.TotalUnPausedGameTime.Ticks + periodInTicks.Value;

                    triggersOnCooldown.Add(trigger.ID, activeTimepoint);
                    //triggersOnCooldownByNextTimePoint.Add(activeTimepoint, trigger);

                }
            }
        }

        public bool IsReadyToHandleTrigger(Trigger triggerToCheck)
        {
            long nextTickToAcceptMsgs;
            if (!triggersOnCooldown.TryGetValue(triggerToCheck.ID, out nextTickToAcceptMsgs))
            {
                return true;
            }


            if (The.Sim.TotalUnPausedGameTime.Ticks > nextTickToAcceptMsgs)
            {
                // remove from both lists...
                triggersOnCooldown.Remove(triggerToCheck.ID);

              /*  for (int i = 0; i < triggersOnCooldownByNextTimePoint.Count; i++)
                {
                    if (triggersOnCooldownByNextTimePoint[i] == triggerToCheck)
                    {
                        triggersOnCooldownByNextTimePoint.RemoveAt(i);
                    }
                }*/

                return true;
            }

            return false;

          //  Regulator triggerRegulator = triggerCooldownTimers[triggerToCheck];
          //  return triggerRegulator.IsReady();
        }

        public bool HasSkill(SkillType skillType)
        {
            if (skillType == null)
            {
                return true;
            }

            Skill skill;
            if (Skills.TryGetValue(skillType, out skill))
            {
                return skill.Value > GameData.Instance.Constants.MinimumSkillValueToUse; // 0.1;
            }
                       
            return false;
        }

       
        public Skill GetSkill(SkillType skillType)
        {
            Skill skill;
            if (Skills.TryGetValue(skillType, out skill))
            {
                return skill;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// starts the AI running for a new entity
        /// 
        /// do branching so only the modules that are needed for othersite are created
        /// make sure the method can be called twice and do nothing with modules are already existing/running
        /// </summary>
        public void ComeOnline()
        {
            if (Parent.IsOnPlaySite())
            {
                if (Parent.EntityType.IntelligenceType.IsMobile)
                {
                    if (PathPlanner == null)
                    {
                        PathPlanner = new PathPlanner(Parent);
                    }

                    if (CropsMapForAgent == null)
                    {
                        CropsMapForAgent = new ResourceMapForAgent(Parent);
                    }
                }

                if (Brain == null)
                {
                    Brain = new GoalThink(Parent);
                }
            }


            if (CanEmigrate())
            {
                // creatures too..?
                if (EmigrateDecider == null)
                {
                    EmigrateDecider = new EmigrateDecider(this.Parent);
                }
            }


            if (!GatherPolledStatistics && IsIndependent())
            {
                // generate personal ratings once. for othersite entitites, these will consist of shared ratings.
                Statistics.UpdateOnce();
            }
        }

        public bool CanEmigrate()
        {
            return IsIndependent() && Parent.EntityType.IntelligenceType.CanEmigrate == true;
        }


        public AllegianceRatings GetRatingsForAllegiance(AllegianceID allegiance)
        {
            AllegianceRatings rating = null;
            if (EmigrateDecider != null)
            {
                return EmigrateDecider.GetRatings(allegiance);
            }

            return rating;
        }


        public bool HasDesireToEmigrate(Allegiance toAllegiance)
        {
            if (EmigrateDecider != null)
            {
                return EmigrateDecider.HasDesireToEmigrate(toAllegiance);
            }

            return false;
        }

        /// <summary>
        /// sets threat stance to cautious if we are injured and have propensity to flee
        /// </summary>
        public void ResetThreatStance()
        {
            SetNormalOrCautiousThreatStance();

            /*if (ThreatStance != ThreatStance.Cautious)
            {
                if (StanceShouldBeCautious()) 
                {**
                    threatStance = ThreatStance.Cautious;
                }                
            }
            else if (ThreatStance == ThreatStance.Cautious)
            {
                if (!StanceShouldBeCautious())
                {
                    threatStance = ThreatStance.Normal;
                }  
            }*/
        }

        /// <summary>
        /// do we have functioning modes to defend ourselves
        /// </summary>
        /// <returns></returns>
        private bool CanDefend()
        {
            if (Parent.EntityType.IntelligenceType.DefendActionTypes != null)
            {
                BodyComponent entityBody;

                Parent.Find(out entityBody);

                foreach (DefendActionType defendType in Parent.EntityType.IntelligenceType.DefendActionTypes)
                {
                    if (IntelligenceType.DefendActionTypeIsFunctional(entityBody.Body, defendType))
                    {
                        return true;
                    }
                }

            }

            return false;

        }

        private bool CanAttack()
        {
            if (Parent.EntityType.IntelligenceType.AttackTypes != null)
            {
                BodyComponent entityBody;
                Parent.Find(out entityBody);

                foreach (AttackType attackType in Parent.EntityType.IntelligenceType.AttackTypes)
                {
                    if (IntelligenceType.AttackTypeIsFunctional(entityBody.Body, attackType))
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        public bool StanceCanBeBold()
        {
            return PanicLevel >= (int)ThreatStance.Normal;
        }

        public bool StanceShouldBeCautious()
        {
            return PanicLevel < (int)ThreatStance.Normal;

           /* Body.Body body;
            if (Parent.Find(out body))
            {
                float hitpointsFractionLeft = body.GlobalHitpoints / body.MaxHitpoints;

                if (hitpointsFractionLeft * Morale * Parent.EntityType.IntelligenceType.FightOverFleeProbability < 0.2f) // 0.2f)
                {
                    return true;                    
                }
            }

            return false;*/
        }

        public void SetName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;

            string totalName;
            if (lastName != null)
            {
                totalName = FirstName + " " + LastName;
            }
            else
            {
                totalName = FirstName;
            }

            totalName = totalName.Trim();
            Parent.Name = totalName;
        }

        public double? GetScore()
        {
            if (CurrentActionScoreRegulator.IsReady())
            {
                // recalc score now:

                currentGoalUtility = Brain.ScoreTopLevelGoal();
            }

            return currentGoalUtility;
        }

        public void UpdateOtherSite(GameTime time)
        {
            


        }

        /// <summary>
        /// the process calls this to extract the work that the agent has assigned for
        /// </summary>
        /// <param name="workedTimeInSeconds"></param>
        /// <param name="progressDelta"></param>
        /// <param name="toolTypeCombination"></param>
        /// <param name="processType"></param>
        public void PerformWorkAndAffectHandTools(SimProcess process, float workedTimeInSeconds, float progressDelta)
        {

            Brain.PerformWork(process, workedTimeInSeconds, progressDelta);

        }

        private bool GatherPolledStatistics
        {
            get
            {
                return IsIndependent() && Parent.IsOnPlaySite();
            }
        }

        private void UpdateCommonSystems(GameTime time)
        {
            if (Parent.Name != null && Parent.Name.Contains("Tamara"))
            {

            }

            if (GatherPolledStatistics)
            {
                Statistics.Update(time);
            }

            if (Parent.PersonEntity != null)
            {
                EmigrateDecider.Update(time);
            }

        }

        /// <summary>
        /// UPDATE PLAYSITE STATS
        /// </summary>
        /// <param name="time"></param>
        private void UpdatePlaySiteSystemsOnly(GameTime time)
        {
            Brain.ProcessThink(time);

            // entity kan die here.
            if (Parent.EntityID == EntityID.Invalid)
                return;

            if (Allegiance != null)
            {
                Allegiance.SharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.UpdateObject(Parent.EntityID, Parent.PlaySiteLocation.ToVector2());
            }


            if (CropsMapForAgent != null)
            {
                CropsMapForAgent.Update(time);
            }

            UpdateCombatStats();

            UpdateInterest(time);

            UpdateSpokenLine(time);


           
            if (triggerCleanupRegulator.IsReady())
            {
                CleanupTriggersOnCooldown();
            }
        }

        private void UpdateCombatStats()
        {
            double deltaTimeInSeconds;
            if (combatStatRegulator.IsReadyGetTimeElapsedInSeconds(out deltaTimeInSeconds))
            {

                if (hitsTakenLastFewSeconds.Count > 0)
                {
                    int previousCount = hitsTakenLastFewSeconds.Count;
                    while (hitsTakenLastFewSeconds.Count > 0 && hitsTakenLastFewSeconds.Peek().Item1 < The.Sim.TotalUnPausedGameTime)
                    {
                        hitsTakenLastFewSeconds.Dequeue();
                    }

                    if (previousCount != hitsTakenLastFewSeconds.Count)
                    {
                        CalculateDamagePerSecondTaken();
                    }
                }

                Morale = Common.IncreaseValueBetweenZeroAndOne(Morale, Parent.EntityType.IntelligenceType.MoraleIncreasePerDay, deltaTimeInSeconds);

            }
        }


        public override void Update(GameTime time)
        {
            UpdateCommonSystems(time);
            
            if (Parent.IsOnPlaySite())
            {
                UpdatePlaySiteSystemsOnly(time);         
            }
            else
            {
                UpdateOtherSite(time);                   
            }
        }

        private void UpdateSpokenLine(GameTime time)
        {
            if (!string.IsNullOrEmpty(SpokenLine))
            {
                spokenLineElapsedTime += time.ElapsedGameTime.TotalSeconds;

                if (spokenLineElapsedTime >= spokenLineDuration)
                {
                    // the line is over and will not be displayed anymore. The agent can now speak a different line
                    SpokenLine = null;
                    spokenLineElapsedTime = 0f;

                    if (TalkActionEnded != null)
                    { 
                        // notify the GUI window   // see if the whole conversation is over
                        TalkActionEnded.Invoke();
                    }

                    if (CurrentConversationID.HasValue)
                    {
                        Conversation currentConversation = LookUp<Conversation, ConversationID>.FindByID(CurrentConversationID.Value);
                        if (currentConversation != null)
                        {
                            // conversation does not use the event because it is a pain to snapshot
                            currentConversation.TalkActionEnded();
                        }
                    }
                   
                }
            }
        }

        private void UpdateInterest(GameTime time)
        {

            if (interestLevel <= 0f)
            {
                interestLevel = 0f;

                // head turn not active, OR turning back to neutral
                HandleNoInterest(time);
            }
            else
            {
                // update head turn towards the target of interest

                //having this in Draw instead of Update makes the counter decrease even when paused, hmmm:
              //  interestLevel -= GameData.Instance.Constants.InterestLevelDropOffPerFrame; //become less interesting // Lars: why is it ok to tie this to the frame rate...? perhaps it looks smoother / doesn't matter?

                interestLevel -= (float)time.ElapsedGameTime.TotalSeconds * GameData.Instance.Constants.InterestLevelDropOffPerSecond; //become less interesting

                Vector2? lookAt = GetPointToLookAt();

                // avoid looking at self, causes model rendering problems
                if (lookAt.HasValue == false || Common.DistanceOctile(lookAt.Value, GetLocationForHeadAndBodyTurn()) < 2)
                {
                    interestLevel = 0f;
                    lookAt = null;
                }

                if (lookAt.HasValue)
                {
                    HandleTurnToInterest(lookAt);
                }
            }
        }

        /// <summary>
        /// principles and happiness are stored in Personality
        /// </summary>
        /// <returns></returns>
        public bool HasHappiness()
        {
            return IsIndependent() && Parent.PersonEntity != null;

        }

        /// <summary>
        /// pets and robots are not...
        /// </summary>
        /// <returns></returns>
        public bool IsIndependent()
        {
            if (Parent.EntityType.IntelligenceType.ServantForEntityTypeTag != null)
            {
                if (Allegiance.RepresentativeEntityType.IntelligenceType.HasServants != null)
                {
                    if (Allegiance.RepresentativeEntityType.IntelligenceType.HasServants.Contains(Parent.EntityType))
                    {
                        return false;
                    }
                }
            }

            return true;
        }


      
        public void ClearCenterOfAttention()
        {
            interestLevel = 0f; // this will turn back the head to neutral
        }

        public void HandleNoInterest(GameTime gameTime)
        {
            entityIDToLookAt = EntityID.Invalid;
            locationToLookAt = null;

            // interestLevel = 0f;
            

            if (!Common.IsZero(lookAngle))  //returnToNormalProgress < 1f)
            {
                // turn the head back to neutral

                //get a point straight ahead to look at:
                Vector2 lookAt = (Parent.FacingNormal + Parent.PlaySiteLocation).ToVector2();

                bool isOutOfView;
                bool isLookingAtTarget;

          
                float lookTargetAngle;

                UpdateHeadTurnAngles(lookAt, interestLevel, out lookTargetAngle, out isLookingAtTarget, out isOutOfView);
                
                // animate the head to turn back to neutral:
                //TurnToLook(lookAt, null, out isLookingAtTarget, out isOutOfView);
                
                Parent.Renderable.TurnToLook(lookAt, lookAngle, lookTargetAngle);
             

                if (isLookingAtTarget) // !TurnToLook(lookAt, null))
                {
                    // done - looking straight ahead
                    lookAngle = 0f;

                    // restore the spine bone DefaultTransforms:
                    Parent.Renderable.StartLerpingBackSpine();
                }
            }
            else 
            {
                // restore the spine bone DefaultTransforms:
                Parent.Renderable.UpdateLerpingBackSpine(gameTime);                
            }
        }

        /// <summary>
        /// Better to send a message to the agent!
        /// </summary>
        /// <param name="location"></param>
        /// <param name="interestLevel"></param>
    /*    public bool SetNewCenterOfAttention(Vector3 location, float interestLevel)
        {
            if (!InterestIsHighEnough(null, location, interestLevel))
            {
                return false;
            }


            if (location == Parent.Location) // can't look at same spot as standing on...
                return;


            if (interestLevel > 0f)
            {
                if ((entityIDToLookAt != EntityID.Invalid || locationToLookAt != location)
                    && this.interestLevel > interestLevel)
                {
                    // the thing we are looking at is more interesting.
                    return;
                }
            }

            maxAngleToTurnHead = GetMaxAngleFromInterest(interestLevel);
            //  if (angl)

            entityIDToLookAt = EntityID.Invalid;
            locationToLookAt = location;

            this.interestLevel = interestLevel;

        }*/

        public bool InterestIsHighEnough(EntityID? id, Vector3? location, float interestLevel)
        {
            if (id == Parent.EntityID) // not myself
                return false;

            if (location.HasValue &&
                 location.Value == Parent.Location) // can't look at same spot as standing on...
                return false;

            if (interestLevel > 0f)
            {
                bool hasHighterInterest = GameData.Instance.Constants.InterestInertiaFactor * this.interestLevel > interestLevel; // set a margin to avoid hysteresis
                bool alreadyLookingAtThisEntity = entityIDToLookAt == id;
                bool hasValidEntityToLookAt = (entityIDToLookAt != EntityID.Invalid);
                bool hasValidLocationToLookAt = (locationToLookAt != null);

                if (alreadyLookingAtThisEntity ||
                    ((hasValidEntityToLookAt == true || hasValidLocationToLookAt == true) && hasHighterInterest))
                {
                    // the thing we are looking at already is more interesting.
                    return false;
                }
            }
            else
            {
                //If the interestLevel is 0 then we would not want to consider looking at this target.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Better to send a message to the agent!
        /// will only switch if the new target is more interesting than the old one - returns false if no switch was made.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="interestLevel"></param>
        public bool SetNewCenterOfAttention(EntityID? id, Vector3? location, float interestLevel)
        {
            if (!InterestIsHighEnough(id, location, interestLevel))
            {
                return false;
            }


            maxAngleToTurnHead = GetMaxAngleFromInterest(interestLevel);

            if (id.HasValue)
            {
                entityIDToLookAt = id.Value;
            }
            else
            {
                entityIDToLookAt = EntityID.Invalid;
            }

            locationToLookAt = location;

            this.interestLevel = interestLevel;

            return true;
        }

        /// <summary>
        /// the max angle that the agent will turn his head to look at something this interesting
        /// </summary>
        /// <param name="interestLevel"></param>
        /// <returns></returns>
        private float GetMaxAngleFromInterest(float interestLevel)
        {
            return MathHelper.Lerp(GameData.Instance.Constants.MaxHeadTurnAngleForZeroInterest,
                                   GameData.Instance.Constants.MaxHeadTurnAngleForMaxInterest,
                                   Common.Clamp(interestLevel, 0f, 1f));

        }

        private void HandleTurnToInterest(Vector2? lookAt)
        {
            bool isOutOfView;
            bool isLookingAtTarget;
            float lookTargetAngle;

            
            if (Parent.EntityType.RenderableTypeMode.AnimatedHeadType != null)
            {
                // animate the head to track the interesting target
                UpdateHeadTurnAngles(lookAt.Value, interestLevel, out lookTargetAngle, out isLookingAtTarget, out isOutOfView);

                if (isOutOfView)
                {
                    // the target has moved beyond our turning range, so set it as not interesting anymore:                        
                    interestLevel = 0f;
                }
                else
                {
                    // keep tracking the target as it moves about
                    Parent.Renderable.TurnToLook(lookAt.Value, lookAngle, lookTargetAngle);
                }
            } // TODO: handle body turns here:

            
        }

        private void UpdateHeadTurnAngles(Vector2 lookTarget, float? interestLevel, out float lookTargetAngle, out bool isLookingAtTarget, out bool isOutOfView)
        {
            isLookingAtTarget = false;
            isOutOfView = false;

            /* if (interestLevel.HasValue && !IsInterestHighEnough(interestLevel.Value))
                 return; // false;
             */

            // use entity location/direction here??? what about the zero case? where renderable is on top of look target
            float bodyFacingRotation = Common.VectorToAngle(Parent.FacingNormal.ToVector2()); // .RenderAsModel.FacingNormal.ToVector2()); // can't use renderable heading...

            float absoluteHeadRotation = bodyFacingRotation + lookAngle;

            Vector2 lookTargetDirection = lookTarget - GetLocationForHeadAndBodyTurn();
            lookTargetDirection.Normalize();

            lookTargetAngle = Common.VectorToAngle(lookTargetDirection);
            // compare to lookAngle

            float angleDifference = Math.Abs(MathHelper.WrapAngle(absoluteHeadRotation - lookTargetAngle));
            float bodyAndTargetAngleDifference = Math.Abs(MathHelper.WrapAngle(bodyFacingRotation - lookTargetAngle));
            if (angleDifference < 0.05f) //dotProduct > 0.95f) // perpDot > 0.95f) //almost facing now
            {
                isLookingAtTarget = true;
                return; // false;
            }
            else if (bodyAndTargetAngleDifference > maxAngleToTurnHead)
            {
                isOutOfView = true;
                return;
            }


           // Vector2 headLookingDirection = parent.RenderAsModel.FacingNormal.ToVector2();

           
            float desiredHeadAngle = MathHelper.WrapAngle(lookTargetAngle - bodyFacingRotation); //bodyFacingRotation - lookTargetAngle; // -(float)Math.Atan2(perpDot, dotProduct);

            float lerpFactor = Parent.EntityType.RenderableTypeMode.AnimatedHeadType.TurnToLookLerpFactor;
            

            //add a portion of the difference between the current head orientation and a perfect straight-on angle
            //lookAngle = lookAngle * (1f - lerpFactor) + targetAngle * lerpFactor;
            lookAngle = Common.EaseInValueTowardsTarget(lookAngle, desiredHeadAngle, lerpFactor);
        }


        private Vector2 GetLocationForHeadAndBodyTurn()
        {
            return Parent.PlaySiteLocation.ToVector2();
        }


        private Vector2? GetPointToLookAt()
        {
            if (entityIDToLookAt != EntityID.Invalid)
            {

                IKnownEntityData entityData;
                GetKnownData(entityIDToLookAt, out entityData);

                if (entityData != null
                    && entityData.Location.HasValue)
                {
                    return entityData.PlaySiteLocation.ToVector2();

                }
                else
                {
                    entityIDToLookAt = EntityID.Invalid;
                    interestLevel = 0f;

                    return null;
                }
            }
            else if (locationToLookAt.HasValue)
            {
                return locationToLookAt.Value.ToVector2();
            }

            return null;
        }


        Regulator triggerCleanupRegulator;

        void CreateRegulators()
        {
             triggerCleanupRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.5, "IntelligenceTriggerCleanup");
             CurrentActionScoreRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 3, "IntelligenceCurrentActionsScoreRegulator");
             combatStatRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1, "IntelligenceDamagePerSecond");

        }

        public override double? GetUpdateInterval()
        {
            if (!Parent.IsDead
                && Parent.IsCompleted())
            {
                if (Parent.IsOnPlaySite())
                {                    
                    return 0;
                }
                else
                {
                    // other site
                    double? tempInterval = null, currentInterval = null;

                    if (EmigrateDecider != null)
                    {
                        tempInterval = EmigrateDecider.GetUpdateInterval();
                        UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);                                  
                    }


                    return currentInterval;
                }
            }

            return null;
        }

        /*
        public override double? GetUpdateInterval()
        {
            if (!Parent.IsDead 
                && Parent.IsCompleted()
                && Parent.IsOnPlaySite())
            {
                return GameData.Instance.Constants.UpdateIntervalForEntityComponents; // 5;
            }

            return null;
        }*/


        /*
        public void UpdateSimulation(double deltaTimeInSeconds)
        {
         
            Morale = Common.IncreaseValueBetweenZeroAndOne(Morale, Parent.EntityType.IntelligenceType.MoraleIncreasePerDay, deltaTimeInSeconds);


            if (triggerCleanupRegulator.IsReady())
            {
                CleanupTriggersOnCooldown();
            }
        }*/

        private List<TriggerID> triggersToActivate = new List<TriggerID>();
        private void CleanupTriggersOnCooldown()
        {
            
            if (triggersOnCooldown.Count > 0)            
            {
                long currentTime = The.Sim.TotalUnPausedGameTime.Ticks;

                triggersToActivate.Clear();
                
                foreach (var item in triggersOnCooldown)
                {
                    if (item.Value < currentTime)
                    {
                        triggersToActivate.Add(item.Key);
                    }
                }

                foreach (var item in triggersToActivate)
                {
                    triggersOnCooldown.Remove(item);
                }

            }


        }

        public void Destroy()
        {
            if (Brain != null)
            {
                Brain.RemoveAllSubgoals();

                Brain.RemoveIDEntry();
                Brain.Terminate();
            }

            if (Allegiance != null) // only in Edit mode do we not have an allegiance.
            {
                
                Allegiance.RemoveMember(Parent, true); 
                Allegiance.LetOtherAllegiancesSeeEntity(Parent, false); // destroys memory fact

                if (Allegiance.Members.Count == 0 && Allegiance.AllegianceType != global::UWGame.SimSide.Allegiances.AllegianceType.Player
                    && !Allegiance.Expeditions.Any(e => e.Population != null
                        && (e.Population.GrowthInMembersPerDay.HasValue || e.Population.GrowthInPercentagePerDay.HasValue))) // don't destroy if respawn capable
                {
                    // no more members? What about memory facts???
                    Allegiance.Destroy();
                }

                Allegiance = null;
            }

            if (PathPlanner != null)
                PathPlanner.Destroy();

            if (CropsMapForAgent != null)
                CropsMapForAgent.Destroy();

            if (CurrentExpedition != null)
            {
                CurrentExpedition.RemoveMember(Parent);
            } 

           /* if (PlayerExpedition != null)
            {
                PlayerExpedition.CurrentMembers.Remove(Parent);
            }*/

            //if (Allegiance.SharedKnowledge.CropHarvesters.ContainsKey()
            //Allegiance.SharedKnowledge.CropHarvesters.

        }

       /* public static bool CanBeSelectedForEmbark(AllegianceRatings rating) //Entity entity)
        {
           

        }*/

        public bool IsReadyForEmbark(Allegiance toAllegiance)
        {
            if (this.Allegiance == toAllegiance)
            {
                return false;
            }
            else
            {
                if (EmigrateDecider.CanEmigrateToAnyTarget())
                {
                    AllegianceRatings rating;
                    rating = GetRatingsForAllegiance(toAllegiance.ID);
                    if (rating != null)
                    {
                        if (rating.Desirability > 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool CanDropRequestedItem(Entity item)
        {
            if (Brain != null)
            {
                return Brain.CanDropRequestedItem(item);
            }
            else return false;
        }
    }
}
