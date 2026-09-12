using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities.Body;
using System.Xml.Serialization;
using System.Collections;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Combat;
using UWGame.SimSide.XmlCollections;
using UWGame.ClientSide.Renderables;
using Xclna.Xna.Animation;
using UWGame.SimSide.Communication;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// a ranking of strength of the mature entity of a species
    /// perhaps make this a class instead of enum.
    /// </summary>
    public enum StrengthRating { None, VeryWeak, WeakerThanHumans, LikeHumans, StrongerThanHumans, VeryStrong}

   
   // public enum Boldness { Cautious, Normal, Careless }

   

    public class IntelligenceType: IXmlSerializable
    {       
        public bool IsMobile;
        
        public string[] Attacks;

        public string[] IntrinsicTools;
        public string[] IntrinsicWeapons;


        /// <summary>
        /// we can communicate for short distances beyond the site...
        /// </summary>
      //  public CommunicatorType[] IntrinsicCommunicators;
      

        /// <summary>
        /// tools as Entity? would make the combos easier in EvaluateJob. Tools should be parts also.
        /// 
        /// what about bios..? 
        /// bioweapons with ammo? made as entities, but not parts...
        /// </summary>
        [XmlIgnore]
        public List<EntityType> IntrinsicToolTypes;

        [XmlIgnore]
        public List<EntityType> IntrinsicWeaponTypes;


        [XmlIgnore]
        public List<AttackType> AttackTypes;
        public DefendActionType[] DefendActionTypes;
        public ScareActionType[] ScareActionTypes;

      
        /// <summary>
        /// this tag can group together similar entity types to handle their ability to transact with containers
        /// </summary>
        public string ContainerTransactTag;

        /// <summary>
        /// for efficiency(?), the string tags are replaced with an int...
        /// </summary>
        [XmlIgnore]
        public int? ContainerTransactValue;


       
        public float IdleChanceToTalk;

        /// <summary>
        /// placeholder - delete this
        /// 
        /// seems too simplistic because it does not account for the place in the food chain
        /// </summary>
        public bool IsPredator = false;

       

        /// <summary>
        /// will attack nearby vermin on its own (like the dog)
        /// </summary>
        public bool HuntsVermin = false;

        /// <summary>
        /// some critters are ornery and will attack other critters that get too close, even if they pose no threat to them. 
        /// Needed when we have IsTerritorial????
        /// </summary>
        public bool WillAttackNonThreatsNearby = false;

        public bool OtherAgentsNearExpeditionCenterAreConsideredThreats = false;

        /// <summary>
        /// How far away from expeditions do we generate vermin threat jobs.
        /// Patrol zones can override this.
        /// Only the Allegiance's RepresentativeEntity property is used
        /// </summary>
        public float? MaxDistanceFromExpeditionsToHuntVermin;

        public StrengthRating StrengthRating = StrengthRating.LikeHumans;

        /// <summary>
        /// 0-1 - is the species bold (= 1) or does it try to avoid enemies (= 0)
        /// 
        /// This can not become a bio property, since the threat map is shared...
        /// </summary>
        public float Boldness = 0.5f;

        /// <summary>
        /// 0 - 1: courage = 1 means the species is never affected by morale.
        /// </summary>
        public float Courage = 0.5f;

        /// <summary>
        /// how likely are we to attack entities that come near us?
        /// </summary>
      //  public float Aggressiveness = 0f;

        /// <summary>
        /// Make sure you want to access this one directly and not use the
        /// function to get this variable in Allegiance.
        /// </summary>
        public int ForageAndHuntingRadius = 500;
        public float MembersScoutingFraction = 1f;


      

        #region Pickup Anim durations
        // from file AnimDurations.txt

        /*
man_pickupHeavy 18 (17*0,04)   =0,68    ......actionpoint(with bindpose=0): 9 (8 = 0,32)
man_pickupLight 31 = 1,2		......actionpoint(with bindpose=0): 15 (14 = 0,56)
man_pickupLightSame 32 = 1,24		......actionpoint(with bindpose=0): 15 (0,56)
man_pickupEquipped 44 1,72		......actionpoint(with bindpose=0): 13 (12 = 0,48)
man_pickupMounted 25 = 0,96		...actionpoint(with bindpose=0): 13 (12 = 0,48)


man_dropHeavy 24 (23*0,04)= 0,92     	...actionpoint(with bindpose=0): 12 (11 = 0,44)
man_dropEquipped 34 = 1,32		...actionpoint(with bindpose=0): 21 (20 = 0,8)
man_dropLight 31 = 1,2			...actionpoint(with bindpose=0): 17 (16 = 0,64)
man_dropMounted 25  (24*0,04) = 0,96	...actionpoint(with bindpose=0): 10 (9  = 0,36)
*/

        public float DropLightDuration = 1.2f;
        public float DropLightActionPointDuration = 0.64f;

        public float DropHeavyDuration = 0.92f;
        public float DropHeavyActionPointDuration = 0.44f;

        public float pickupMountedEquippedDuration = 1.72f;
        public float pickupMountedEquippedActionPointDuration = 0.48f;

        public float pickupEquipDuration = 1.72f;
        public float pickupEquipActionPointDuration = 0.48f;


        public float PickupLightDuration = 1.2f;
        public float PickupLightActionPointDuration = 0.56f;

        public float PickupHeavyDuration = 0.68f;
        public float PickupHeavyActionPointDuration = 0.32f;

        public float pickupMountDuration = 0.96f;
        public float pickupMountActionPointDuration = 0.48f;

        public float SwitchLightToLightDuration = 2.4f; // 1.24f;
        public float SwitchLightToLightActionPointDuration = 1.2f; // 0.56f;

        #endregion

        #region Drop Anim durations

        public float dropEquippedDuration = 1.32f;
        public float dropEquippedActionPointDuration = 0.8f;

        public float dropMountedDuration = 0.96f;
        public float dropMountedActionPointDuration = 0.36f;

        public float DropLightToLightActionPointDuration = 0.6f;
        public float DropLightToLightDuration = 1.24f; // 

        public float dropMountToMountActionPointDuration = 0.5f; // TODO
        public float dropMountToMountDuration = 1f; // TODO

        #endregion

        /// <summary>
        /// how likely will we flee when attacked/injured
        /// 1 = never flee
        /// 0 = always flee
        /// </summary>
        public float FightOverFleeProbability = 0.5f;
                
        /// <summary>
        /// how quickly do we recover from fleeing in panic
        /// </summary>
        public float MoraleIncreasePerDay = 60f; //MP 2014 oct 10. was: 60f but they still did not recover for more than half a day and stayed in panic!!...because of coupled to hitpoints..     MP 2014 sep.  was: 8f 
        
      

        /// <summary>
        /// only from the representative entity type
        /// </summary>
        public string[] ExpeditionPolledEvents; 

        #region Event hooks


        [XmlIgnore]
        public Dictionary<AgentActionHooks, List<ActionSets>> EventActions = new Dictionary<AgentActionHooks,List<ActionSets>>();

        [XmlIgnore]
        public Dictionary<EntityType, List<ActionSets>> DetectEntityTypeEvents = new Dictionary<EntityType,List<ActionSets>>();

        [XmlIgnore]
        public Dictionary<ResourceType, List<ActionSets>> DetectResourceTypeEvents = new Dictionary<ResourceType,List<ActionSets>>();


        /// <summary>
        /// actions to fire when this entity triggers a trigger
        /// key is TriggerType keyname
        /// 
        /// TODO: move to EntityType and refactor to use the same pattern as the event hooks
        /// </summary>
      //  public SerializableDictionary<string, ActionSets> TriggerEventActions;

        #endregion

        /// <summary>
        /// 0-1
        /// the chance that the agent may rest for a bit after an attack
        /// can be overridden in AttackType
        /// </summary>
        public double? ChanceToRestAfterMeleeAttack;
        public double? ChanceToRestAfterRangedAttack;


        /// <summary>
        /// the rest time will be computed using a random normal distribution limited by these values
        /// can be overridden in AttackType
        /// </summary>
        public float? MaxRestTimeAfterAttackingInSeconds;
        public float? MinRestTimeAfterAttackingInSeconds;

        [XmlIgnore]
        public float? RestTimeAfterAttackingMean;
        [XmlIgnore]
        public float? RestTimeAfterAttackingStandardDeviation;

        
        /// <summary>
        /// the number of days that memory facts are stored
        /// </summary>
        public float MemoryInDays = 6f;

        /// <summary>
        /// only affects mobile entities, because
        /// only mobile entities can panic per default.
        /// </summary>
        public bool CanPanic = true;

        /// <summary>
        /// the range within which other entities are seen as a threat and triggers attack
        /// 
        /// can also be speicifed as a bio property
        /// </summary>
        public float? AggroRange;

        /// <summary>
        /// the max range that allegiance members will move towards a common threat.
        /// It makes sense for assistance range to be much greater than aggro range and sensor range.
        /// </summary>
        public float? AssistanceRange;


        public float? ChanceToIdleWalkShortDistanceAway;
        public float? ShortIdleWalkMaxDistance;
        public float? ShortIdleWalkMinDistance;


        public string[] InterestInTriggerTypes;

        [XmlIgnore]
        public Dictionary<TriggerType, bool> HasInterestInTriggers = new Dictionary<TriggerType,bool>();

        public bool? AllowEscapeFromTinyAreas;
     
        public bool? CanSpeak;

        public bool? CanTradeAndCommunicate;


        /// <summary>
        /// if true, will add job evaluators to non-persons
        /// </summary>
        public bool? CanScout;


        public bool? CanExamine;


        /// <summary>
        /// also needs attacks
        /// </summary>
        public bool? CanPatrol;

        /// <summary>
        /// also needs a locomotor
        /// </summary>
        public bool? CanDoJobs;


        public bool? CanProduce;        
       
        /// <summary>
        /// also needs itemstorage
        /// </summary>
        public bool? CanHaul;


        public bool? CanCheckProgress;
       
        /// <summary>
        /// also needs attacks
        /// </summary>
        public bool? CanHunt;
        
        public bool? CanAttack;

        /// <summary>
        /// if false, can only use intrinsic attacks.
        /// if true, AgentStorage must also be defined.
        /// </summary>
        public bool? CanUseWeapons;


        public bool? CanUseGadgets;


        public bool? CanEmigrate;

        /// <summary>
        /// if false, can only use intrinsic tools.
        /// if true, AgentStorage must also be defined.
        /// </summary>
        public bool? CanMountTools;

        /// <summary>
        /// true if the agent can replenish weapons, tools etc. Requires AgentStorage.
        /// </summary>
        public bool? CanReplenish;

        /// <summary>
        /// true for people, false for others..?
        /// The representative entity type for the whole allegiance controls this.
        /// so for robots and dogs in the human allegiance, this setting does not matter..
        /// </summary>
        public bool RespectsOwnership = false;

        /// <summary>
        /// if false, the entity does not react to ratings and never migrates on its own
        /// </summary>
      //  public bool IsIndependent = true;

        public string[] Prey;

        [XmlIgnore]
        public HashSet<EntityType> PreyTypes;
      //  public List<EntityType> PreyTypes;


        /// <summary>
        /// if member of an allegiance as a servant, the entity does not react to ratings and never migrates on its own
        /// </summary>
        public string ServantForEntityTypeTag;

        /// <summary>
        /// any entity with this tag that becomes a member of an allegiance which has this entity type as representative type, will be a servant...
        /// </summary>
        public string[] HasServantsTags;

        [XmlIgnore]
        public List<EntityType> HasServants;

        /// <summary>
        /// skills with default values
        /// </summary>
        public SerializableDictionary<string, float> Skills;


        public void PostLoadContentInitialize(EntityType parent)
        {
            
          //  canEnter = GameData.CreateBitArrayFromTags(GameData.Instance.EnterBuildingByTag, BuildingEntranceDesignerTags);
            int enter;
            if (ContainerTransactTag != null)
            {
                if (GameData.Instance.ContainerTags.TryGetValue(ContainerTransactTag, out enter)) // #TutorialBug!!!
                {
                    ContainerTransactValue = enter;
                }
            }

            if (HasServantsTags != null)
            {
                HasServants = new List<EntityType>();
                foreach (var item in HasServantsTags)
                {
                    List<EntityType> list;
                    if (GameData.Instance.ServantEntityTypeByTag.TryGetValue(item, out list))
                    {
                        HasServants.AddRange(list);
                    }
                }
            }


            if (parent.IntelligenceType != null)
            {
                if (parent.IntelligenceType.IsPredator)
                {
                    HasInterestInTriggers.Add(GameData.Instance.AllTriggerTypes["prey"], true);
                }
            }

            List<AgentActionHook> agentActionHooks;
            if (GameData.Instance.AgentActionHooksByEntityType.TryGetValue(parent, out agentActionHooks))
            {
                // group actions by hook type:
                foreach (var item in agentActionHooks)
                {
                   /* List<ActionSets> actions;
                    if (!this.EventActions.TryGetValue(item.Hook, out actions))
                    {
                        actions = new List<ActionSets>();
                        this.EventActions.Add(item.Hook, actions);
                    }
                    
                    actions.Add(GameData.Instance.AllActionSets[item.ActionSetsKey]);*/

                    if (item.Hook == AgentActionHooks.DecidedToLeaveAllegiance)
                    {

                    }


                    Common.AddToMultiList(this.EventActions, item.Hook, GameData.Instance.AllActionSets[item.ActionSetsKey]);
                }               
            }


            List<DetectEntityTypeHook> detectEntityTypeActions;
            if (GameData.Instance.DetectedEntityHooksByEntityType.TryGetValue(parent, out detectEntityTypeActions))
            {
                // group actions by detected entity type: (I tried generalizing this code, but stumbled on the 'trigger' type)
                foreach (var item in detectEntityTypeActions)
                {
                    EntityType detectedEntityType = GameData.Instance.AllEntityTypes[item.DetectedEntityKey];

                  /*  List<ActionSets> actions;
                    
                    if (!this.DetectEntityTypeEvents.TryGetValue(detectedEntityType, out actions))
                    {
                        actions = new List<ActionSets>();
                        this.DetectEntityTypeEvents.Add(detectedEntityType, actions);
                    }

                    actions.Add(GameData.Instance.AllActionSets[item.ActionSetsKey]);
                    */

                    Common.AddToMultiList(this.DetectEntityTypeEvents, detectedEntityType, GameData.Instance.AllActionSets[item.ActionSetsKey]);
                }
            }

            List<DetectResourceTypeHook> detectResourceTypeActions;
            if (GameData.Instance.DetectedResourceHooksByEntityType.TryGetValue(parent, out detectResourceTypeActions))
            {
                // group actions by detected resource type
                foreach (var item in detectResourceTypeActions)
                {
                    List<ActionSets> actions;
                    ResourceType detectedEntityType = GameData.Instance.AllResourceTypes[item.DetectedResourceKey];
                    if (!this.DetectResourceTypeEvents.TryGetValue(detectedEntityType, out actions))
                    {
                        actions = new List<ActionSets>();
                        this.DetectResourceTypeEvents.Add(detectedEntityType, actions);
                    }

                    actions.Add(GameData.Instance.AllActionSets[item.ActionSetsKey]);
                }
            }

            if (IntrinsicTools != null)
            {
                IntrinsicToolTypes = new List<EntityType>();
                foreach (var item in IntrinsicTools)
                {
                    IntrinsicToolTypes.Add(GameData.Instance.AllEntityTypes[item]);
                }
            }

            if (IntrinsicWeapons != null)
            {
                IntrinsicWeaponTypes = new List<EntityType>();
                foreach (var item in IntrinsicWeapons)
                {
                    IntrinsicWeaponTypes.Add(GameData.Instance.AllEntityTypes[item]);
                }
            }

            if (Prey != null)
            {
                PreyTypes = new HashSet<EntityType>();
                foreach (var item in Prey)
                {
                    PreyTypes.Add(GameData.Instance.AllEntityTypes[item]);
                }

            }

            /*
            if (parent.Person != null)
            {
                HasInterestInTriggers.Add(GameData.Instance.AllTriggerTypes["entityDied"], true);
            }*/
        }


     



        /// <summary>
        /// see if all the body parts that we depend on for this attack are there.
        /// </summary>
        /// <param name="attackType"></param>
        /// <returns></returns>
        public static bool AttackTypeIsFunctional(/*Entity entity,*/ Body.Body entityBody, AttackType attackType)
        {
            if (attackType.DependsOn == null || attackType.DependsOn.Length == 0)
            {
                return true;
            }

            return AreDependentBodyPartsFunctional(attackType.DependsOn, entityBody);
        }

        public static bool DefendActionTypeIsFunctional(Body.Body entityBody, DefendActionType defendType)
        {
            if (defendType.DependsOn == null || defendType.DependsOn.Length == 0)
            {
                return true;
            }

            return AreDependentBodyPartsFunctional(defendType.DependsOn, entityBody);
        }


        public bool CanMountToolsOrWeapons()
        {
            return CanUseWeapons == true || CanMountTools == true;

        }

        private static bool AreDependentBodyPartsFunctional(BodyPartType[] dependentBodyParts, Body.Body entityBody) //, AttackType attackType)
        {
            BodyPart foundBodyPart;
            foreach (BodyPartType bodyPartType in dependentBodyParts)
            {
                foundBodyPart = entityBody.FindBodyPartOfType(bodyPartType);
                if (foundBodyPart != null)
                {
                    if (!foundBodyPart.IsFunctional())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

       /* public bool CanEnterBuilding(EntityType buildingType)
        {
            if (CanAccess.HasValue)
            {
                if (buildingType.ContainerType != null)
                {
                    return buildingType.ContainerType.ContentsCanBeAccessedBy[CanAccess.Value];
                }
                else return true; // ??
            }
            else return false;
        }*/
       

        public IntelligenceType()
        {
            
        }

        public void Initialize(EntityType parent) 
        {
            if (MinRestTimeAfterAttackingInSeconds.HasValue && MaxRestTimeAfterAttackingInSeconds.HasValue)
            {
                Common.GetNormalDistributionFromMinMaxValues(MinRestTimeAfterAttackingInSeconds.Value, MaxRestTimeAfterAttackingInSeconds.Value,
                    out RestTimeAfterAttackingMean, out RestTimeAfterAttackingStandardDeviation);
            }


            if (InterestInTriggerTypes != null)
            {
                foreach (var item in InterestInTriggerTypes)
                {
                    HasInterestInTriggers.Add(GameData.Instance.AllTriggerTypes[item], true);
                }
            }

            if (Attacks != null)
            {
                AttackTypes = new List<AttackType>(); // new AttackType[Attacks.Length];

                foreach (var item in Attacks)
                {
                    AttackTypes.Add(GameData.Instance.AllAttackTypes[item]);
                }
            }

            if (!string.IsNullOrEmpty(ServantForEntityTypeTag))
            {
                BaseDataLoader.AddToTagCollection(parent,
                    ServantForEntityTypeTag, GameData.Instance.ServantEntityTypeByTag);
            }

           
        }


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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(IntelligenceType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()
        };
       


        #endregion
    }
}
