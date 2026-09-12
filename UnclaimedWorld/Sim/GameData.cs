using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Soil;
using SpriteSheetRuntime;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework.Graphics;
using Xclna.Xna.Animation;
using Microsoft.Xna.Framework;
using GameStateManagement;
using Microsoft.Xna.Framework.Content;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Processes;
using System.Collections;
using UWGame.ClientSide;
using WindowSystem;
using System.Diagnostics;
using UWGame.SimSide.InGameEvents;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.Triggers;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.Client.Audio;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.ClientSide.Particles;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Skills;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Entities.Templates;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Trade;
using UWGame.SimSide.Overland;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using UWGame.ClientSide.Map;
using UWGame.ClientSide.Hints;
using UWGame.Steam;


namespace UWGame.SimSide
{
    /// <summary>
    ///this class will live for eternity... it contains all static data loaded from .xnb and xml files.
    /// it should not be saved with savegames, because this would mean bloating them unnecessarily. Question: what happens when the user changes the data files between saves?
    /// probably a crash...
    /// </summary>
    public class GameData
    {
        private static GameData instance;
        public static GameData Instance
        {
            get
            {
                // return instance;
                if (instance != null)
                {
                    return instance;
                }
                else
                {
                    instance = new GameData();
                    return instance;
                }
            }
            /*  set
              {
                  instance = value;
              }*/
        }

        private GameData()
        {
            AllEntityCategories = new GameDataCollection<EntityCategory>(AllGameDataCollections, DataLoaderQueueState.EntityCategories);

            AllResourceCategories = new GameDataCollection<ResourceCategory>(AllGameDataCollections, DataLoaderQueueState.ResourceCategories);
            //StructureTypesInCategory = new Dictionary<StructureCategory, List<EntityType>>();

            AllAttackTypes = new GameDataCollection<AttackType>(AllGameDataCollections, DataLoaderQueueState.AttackTypes);
            AllDamageTypes = new GameDataCollection<DamageType>(AllGameDataCollections, DataLoaderQueueState.DamageTypes);

            AllStancesTypes = new GameDataCollection<StancesType>(AllGameDataCollections, DataLoaderQueueState.StancesTypes);
            AllStanceTypes = new GameDataCollection<StanceType>(AllGameDataCollections, DataLoaderQueueState.StanceTypes);


            AllDegradeTypes = new GameDataCollection<DegradeType>(AllGameDataCollections, DataLoaderQueueState.DegradeProfiles);
            AllStorageConditions = new GameDataCollection<StorageCondition>(AllGameDataCollections, DataLoaderQueueState.StorageConditions);
            AllFoodNutrientProfiles = new GameDataCollection<FoodNutrientProfile>(AllGameDataCollections, DataLoaderQueueState.FoodNutrientProfiles);
            AllProcessToolSets = new GameDataCollection<ProcessToolSet>(AllGameDataCollections, DataLoaderQueueState.ProcessToolSets);
            AllSkillTypes = new GameDataCollection<SkillType>(AllGameDataCollections, DataLoaderQueueState.SkillTypes);
            AllSkillCategories = new GameDataCollection<SkillCategory>(AllGameDataCollections, DataLoaderQueueState.SkillCategories);
            AllProfessionTypes = new GameDataCollection<ProfessionType>(AllGameDataCollections, DataLoaderQueueState.ProfessionTypes);

            AllEntityTypes = new GameDataCollection<EntityType>(AllGameDataCollections, DataLoaderQueueState.EntityTypes);
            AllDetectionTypes = new GameDataCollection<DetectionType>(AllGameDataCollections, DataLoaderQueueState.DetectionTypes);
            AllTriggerTypes = new GameDataCollection<TriggerType>(AllGameDataCollections, DataLoaderQueueState.TriggerTypes);
            AttachableRenderableTypes = new GameDataCollection<RenderableType>(AllGameDataCollections, DataLoaderQueueState.AttachableRenderableTypes);
            AllEntityTypeDescriptions = new GameDataCollection<EntityTypeDescription>(AllGameDataCollections, DataLoaderQueueState.EntityTypeDescriptions);

            //   public Dictionary<string, ResourceDetectionFactor> AllResourceDetectionFactors = new Dictionary<string, ResourceDetectionFactor>();
            //   public Dictionary<string, EntityTypeDetectionFactor> AllEntityTypeDetectionFactors = new Dictionary<string, EntityTypeDetectionFactor>();
            //   public Dictionary<string, DetectEntityType> AllDetectEntityTypes = new Dictionary<string, DetectEntityType>();
            //  public Dictionary<string, ProcessType> AllHarvestTypes = new Dictionary<string, ProcessType>();       

            AllBodyTypes = new GameDataCollection<BodyType>(AllGameDataCollections, DataLoaderQueueState.BodyTypes);
            AllBodyLayerTypes = new GameDataCollection<BodyLayerType>(AllGameDataCollections, DataLoaderQueueState.BodyLayerTypes);

            AllLowVegetationTypes = new GameDataCollection<LowVegetationType>(AllGameDataCollections, DataLoaderQueueState.LowVegetationTypes);
            AllSoilComponentTypes = new GameDataCollection<SoilComponentType>(AllGameDataCollections, DataLoaderQueueState.SoilTypes);
            //   public GameDataCollection<LightSourceType> AllLightSourceTypes = new GameDataCollection<LightSourceType>();
            //AllStructureCategories = new GameDataCollection<StructureCategory>(AllGameDataCollections, DataLoaderQueueState.StructureCategories);
            AllFoodNutrientTypes = new GameDataCollection<FoodNutrientType>(AllGameDataCollections, DataLoaderQueueState.FoodNutrientTypes);
            // public Dictionary<string, AgentAction> AllAgentActions = new Dictionary<string, AgentAction>();
            AllDefaultStorageSettings = new GameDataCollection<DefaultStorageSettings>(AllGameDataCollections, DataLoaderQueueState.DefaultStorageSettings);

            AllPresentationTypes = new GameDataCollection<PresentationType>(AllGameDataCollections, DataLoaderQueueState.PresentationTypes);
            AllPresentationTypeCategories = new GameDataCollection<PresentationTypeCategory>(AllGameDataCollections, DataLoaderQueueState.PresentationTypeCategories);
            //  public Dictionary<string, TileResourceType> AllTileResourceTypes = new Dictionary<string, TileResourceType>();

            AllResourceTypes = new GameDataCollection<ResourceType>(AllGameDataCollections, DataLoaderQueueState.ResourceTypes);

            //  public Dictionary<string, ResourceType> AllTileResourceTypes = new Dictionary<string, ResourceType>();

            //  public Dictionary<string, EdgeFeatureType> AllEdgeFeatureTypes = new Dictionary<string, EdgeFeatureType>();

            AllSoundData = new GameDataCollection<SoundData>(AllGameDataCollections, DataLoaderQueueState.Sounds);
            AllPersonalityTypes = new GameDataCollection<PersonalityType>(AllGameDataCollections, DataLoaderQueueState.Personalities);
            AllTraitTemplates = new GameDataCollection<TraitTemplate>(AllGameDataCollections, DataLoaderQueueState.Traits);
            AllCultureTemplates = new GameDataCollection<CultureTemplate>(AllGameDataCollections, DataLoaderQueueState.Cultures);
            AllTierTypes = new GameDataCollection<TierType>(AllGameDataCollections, DataLoaderQueueState.Tiers);
            AllTierAreas = new GameDataCollection<TierArea>(AllGameDataCollections, DataLoaderQueueState.TierAreas);
         
            AllUpgradeCategories = new GameDataCollection<UpgradeCategory>(AllGameDataCollections, DataLoaderQueueState.UpgradeCategories);
            AllUpgradeProfiles = new GameDataCollection<UpgradeProfile>(AllGameDataCollections, DataLoaderQueueState.UpgradeProfiles);
            AllBioOrderTypes = new GameDataCollection<BioOrderType>(AllGameDataCollections, DataLoaderQueueState.BioOrders);
            AllPolledEvents = new GameDataCollection<PolledEventType>(AllGameDataCollections, DataLoaderQueueState.PolledEventTypes);

            AllRepairProfiles = new GameDataCollection<RepairProfile>(AllGameDataCollections, DataLoaderQueueState.RepairProfiles);
            AllSubstanceTypes = new GameDataCollection<SubstanceType>(AllGameDataCollections, DataLoaderQueueState.Substances);
            AllProcessTypes = new GameDataCollection<ProcessType>(AllGameDataCollections, DataLoaderQueueState.ProcessTypes);
            AllEffectProfileTypes = new GameDataCollection<EffectProfileType>(AllGameDataCollections, DataLoaderQueueState.EffectProfileTypes);
            AllEffectTypes = new GameDataCollection<EffectType>(AllGameDataCollections, DataLoaderQueueState.EffectTypes);
          
            AllActionSets = new GameDataCollection<ActionSets>(AllGameDataCollections, DataLoaderQueueState.ActionSets);
            AllActionSetTypes = new GameDataCollection<ActionSetType>(AllGameDataCollections, DataLoaderQueueState.ActionSets);  // this collection is only for snapshotting references to the nested ActionSetType instances
            AllEventActionTypes = new GameDataCollection<EventActionType>(AllGameDataCollections, DataLoaderQueueState.EventActionTypes);

            AllIconInfo = new GameDataCollection<IconInfo>(AllGameDataCollections, DataLoaderQueueState.IconInfo);
            AllEntityData = new GameDataCollection<EntityData>(AllGameDataCollections, DataLoaderQueueState.EntityData);

            AllSiteData = new GameDataCollection<SiteData>(AllGameDataCollections, DataLoaderQueueState.SiteData);
            AllAllegianceData = new GameDataCollection<AllegianceData>(AllGameDataCollections, DataLoaderQueueState.AllegianceData);
            AllAllegianceTemplates = new GameDataCollection<AllegianceTemplate>(AllGameDataCollections, DataLoaderQueueState.AllegianceTemplates);           
            AllSiteTemplates = new GameDataCollection<SiteTemplate>(AllGameDataCollections, DataLoaderQueueState.SiteTemplates);
            AllExpeditionData = new GameDataCollection<ExpeditionData>(AllGameDataCollections, DataLoaderQueueState.ExpeditionData);

            AllTradeProfiles = new GameDataCollection<TradeProfile>(AllGameDataCollections, DataLoaderQueueState.TradeProfiles);
            AllStructuresProfiles = new GameDataCollection<StructuresProfile>(AllGameDataCollections, DataLoaderQueueState.StructureProfiles);
            AllVehiclesProfiles = new GameDataCollection<VehiclesProfile>(AllGameDataCollections, DataLoaderQueueState.VehicleProfiles);

            AllTradeGroups = new GameDataCollection<TradeGroup>(AllGameDataCollections, DataLoaderQueueState.TradeGroups);
            AllOfferDemandProfiles = new GameDataCollection<OfferDemandProfile>(AllGameDataCollections, DataLoaderQueueState.OfferDemandProfiles);
            AllPricesProfiles = new GameDataCollection<PricesProfile>(AllGameDataCollections, DataLoaderQueueState.PricesProfiles);
          
            AllFilterSettingTypes = new GameDataCollection<FilterSettingType>(AllGameDataCollections, DataLoaderQueueState.FilterSettingTypes);
            AllJobTypes = new GameDataCollection<JobType>(AllGameDataCollections, DataLoaderQueueState.JobTypes);

           // AllHints = new GameDataCollection<Hint>(AllGameDataCollections, DataLoaderQueueState.Hints);
            
        }

      //  public Dictionary<StructureCategory, List<EntityType>> StructureTypesInCategory;

        #region extra entity type collections, for iterating

        public Dictionary<string, EntityType> AllItemTypes = new Dictionary<string, EntityType>();
        public Dictionary<string, EntityType> AllTreeTypes = new Dictionary<string, EntityType>();
        public Dictionary<string, EntityType> AllTerrainFeatureTypes = new Dictionary<string, EntityType>();
        public Dictionary<string, EntityType> AllStructureTypes = new Dictionary<string, EntityType>();

        public Dictionary<string, EntityType> AllCreatureTypes = new Dictionary<string, EntityType>();
        public Dictionary<string, EntityType> AllVerminTypes = new Dictionary<string, EntityType>();


        #endregion


        /// <summary>
        /// how does this work when serializing..?
        /// </summary>
        public Dictionary<Type, IGameDataCollection> AllGameDataCollections = new Dictionary<Type, IGameDataCollection>();

      //  public Dictionary<Type, IGameDataObject> AllGameDataObjects = new Dictionary<Type, IGameDataObject>();
      
        // split them into Home / Away types... for staggered load?
        // public GameDataCollection<EntityType> AllItemTypes = new GameDataCollection<EntityType>();

        #region GameDataCollection

        public GameDataCollection<EntityCategory> AllEntityCategories;

        public GameDataCollection<ResourceCategory> AllResourceCategories;
        
        //   public Dictionary<string, EntityCategory> AllEntityCategories = new Dictionary<string, EntityCategory>(); // needed?

      //  public GameDataCollection<LocomotorType> AllLocomotorTypes = new GameDataCollection<LocomotorType>();

        public GameDataCollection<AttackType> AllAttackTypes;
        public GameDataCollection<DamageType> AllDamageTypes;

        public GameDataCollection<StancesType> AllStancesTypes;
        public GameDataCollection<StanceType> AllStanceTypes;


        public GameDataCollection<DegradeType> AllDegradeTypes;
        public GameDataCollection<StorageCondition> AllStorageConditions;
        public GameDataCollection<FoodNutrientProfile> AllFoodNutrientProfiles;
        public GameDataCollection<ProcessToolSet> AllProcessToolSets;
        public GameDataCollection<SkillType> AllSkillTypes;
        public GameDataCollection<SkillCategory> AllSkillCategories;
        public GameDataCollection<ProfessionType> AllProfessionTypes;

        public GameDataCollection<EntityType> AllEntityTypes;
        public GameDataCollection<DetectionType> AllDetectionTypes;
        public GameDataCollection<TriggerType> AllTriggerTypes;
        public GameDataCollection<RenderableType> AttachableRenderableTypes;
        public GameDataCollection<EntityTypeDescription> AllEntityTypeDescriptions;
        
        //   public Dictionary<string, ResourceDetectionFactor> AllResourceDetectionFactors = new Dictionary<string, ResourceDetectionFactor>();
        //   public Dictionary<string, EntityTypeDetectionFactor> AllEntityTypeDetectionFactors = new Dictionary<string, EntityTypeDetectionFactor>();
        //   public Dictionary<string, DetectEntityType> AllDetectEntityTypes = new Dictionary<string, DetectEntityType>();
        //  public Dictionary<string, ProcessType> AllHarvestTypes = new Dictionary<string, ProcessType>();       

        public GameDataCollection<BodyType> AllBodyTypes;
        public GameDataCollection<BodyLayerType> AllBodyLayerTypes;

        public GameDataCollection<LowVegetationType> AllLowVegetationTypes;
        public GameDataCollection<SoilComponentType> AllSoilComponentTypes;
     //   public GameDataCollection<LightSourceType> AllLightSourceTypes = new GameDataCollection<LightSourceType>();
      //  public GameDataCollection<StructureCategory> AllStructureCategories;
        public GameDataCollection<FoodNutrientType> AllFoodNutrientTypes;
       // public Dictionary<string, AgentAction> AllAgentActions = new Dictionary<string, AgentAction>();
        public GameDataCollection<DefaultStorageSettings> AllDefaultStorageSettings;

        public GameDataCollection<PresentationTypeCategory> AllPresentationTypeCategories;

        public GameDataCollection<PresentationType> AllPresentationTypes;

        //  public Dictionary<string, TileResourceType> AllTileResourceTypes = new Dictionary<string, TileResourceType>();

        public GameDataCollection<ResourceType> AllResourceTypes;

        //  public Dictionary<string, ResourceType> AllTileResourceTypes = new Dictionary<string, ResourceType>();

        //  public Dictionary<string, EdgeFeatureType> AllEdgeFeatureTypes = new Dictionary<string, EdgeFeatureType>();
      
        public GameDataCollection<SoundData> AllSoundData;

        public GameDataCollection<TierType> AllTierTypes;
        public GameDataCollection<TierArea> AllTierAreas;
        
        public GameDataCollection<UpgradeCategory> AllUpgradeCategories;
        public GameDataCollection<UpgradeProfile> AllUpgradeProfiles;
       
        public GameDataCollection<PersonalityType> AllPersonalityTypes;
        public GameDataCollection<TraitTemplate> AllTraitTemplates;
        public GameDataCollection<CultureTemplate> AllCultureTemplates;

        public GameDataCollection<BioOrderType> AllBioOrderTypes;
        public GameDataCollection<PolledEventType> AllPolledEvents;

        public GameDataCollection<RepairProfile> AllRepairProfiles;

        public GameDataCollection<SubstanceType> AllSubstanceTypes;
        public GameDataCollection<ProcessType> AllProcessTypes;

        public GameDataCollection<EffectProfileType> AllEffectProfileTypes;
        public GameDataCollection<EffectType> AllEffectTypes;
       
        
        public GameDataCollection<ActionSets> AllActionSets;
        public GameDataCollection<ActionSetType> AllActionSetTypes;  // this collection is only for snapshotting references to the nested ActionSetType instances
        public GameDataCollection<EventActionType> AllEventActionTypes;

        public GameDataCollection<EntityData> AllEntityData;

        public GameDataCollection<SiteTemplate> AllSiteTemplates;
        public GameDataCollection<AllegianceTemplate> AllAllegianceTemplates;

        public GameDataCollection<SiteData> AllSiteData;
        public GameDataCollection<AllegianceData> AllAllegianceData;
        public GameDataCollection<ExpeditionData> AllExpeditionData;

        public GameDataCollection<TradeProfile> AllTradeProfiles;
        public GameDataCollection<VehiclesProfile> AllVehiclesProfiles;
        public GameDataCollection<StructuresProfile> AllStructuresProfiles;
      
        public GameDataCollection<TradeGroup> AllTradeGroups;
        public GameDataCollection<PricesProfile> AllPricesProfiles;


        public GameDataCollection<OfferDemandProfile> AllOfferDemandProfiles;

        public GameDataCollection<IconInfo> AllIconInfo;

        public GameDataCollection<JobType> AllJobTypes;

      //  public GameDataCollection<Hint> AllHints;

        /// <summary>
        /// GUI data...
        /// </summary>
        public GameDataCollection<FilterSettingType> AllFilterSettingTypes;
      

        #region event hooks

        /// <summary>
        /// these big multilists make it possible for mods and scenarios to add, update and delete actions, without replacing whole entity types...
        /// </summary>
        public Dictionary<string, AgentActionHook> AllAgentActionHooks = new Dictionary<string, AgentActionHook>();
        public Dictionary<string, EntityEventHook> AllEntityEventHooks = new Dictionary<string, EntityEventHook>();

        public Dictionary<string, AttackTypeActionHook> AllAttackTypeEventHooks = new Dictionary<string, AttackTypeActionHook>();
        public Dictionary<string, ProcessTypeActionHook> AllProcessTypeEventHooks = new Dictionary<string, ProcessTypeActionHook>();
        public Dictionary<string, EffectTypeActionHook> AllEffectTypeEventHooks = new Dictionary<string, EffectTypeActionHook>();
       
        public Dictionary<string, DetectEntityTypeHook> AllDetectedEntityEventHooks = new Dictionary<string, DetectEntityTypeHook>();
        public Dictionary<string, DetectResourceTypeHook> AllDetectedResourceEventHooks = new Dictionary<string, DetectResourceTypeHook>();

        public Dictionary<string, EntityTypePolledEvent> AllEntityPolledEvents = new Dictionary<string, EntityTypePolledEvent>();



        public Dictionary<EntityType, List<AgentActionHook>> AgentActionHooksByEntityType = new Dictionary<EntityType, List<AgentActionHook>>();
        public Dictionary<EntityType, List<EntityEventHook>> EntityEventHooksByEntityType = new Dictionary<EntityType, List<EntityEventHook>>();

        public Dictionary<EntityType, List<DetectEntityTypeHook>> DetectedEntityHooksByEntityType = new Dictionary<EntityType, List<DetectEntityTypeHook>>();
        public Dictionary<EntityType, List<DetectResourceTypeHook>> DetectedResourceHooksByEntityType = new Dictionary<EntityType, List<DetectResourceTypeHook>>();

        public Dictionary<AttackType, List<AttackTypeActionHook>> EventHooksByAttackType = new Dictionary<AttackType, List<AttackTypeActionHook>>();
        public Dictionary<ProcessType, List<ProcessTypeActionHook>> EventHooksByProcessType = new Dictionary<ProcessType, List<ProcessTypeActionHook>>();
        public Dictionary<EffectProfileType, List<EffectTypeActionHook>> EventHooksByEffectType = new Dictionary<EffectProfileType, List<EffectTypeActionHook>>();

        public Dictionary<EntityType, List<EntityTypePolledEvent>> EntityPolledEventByEntityType = new Dictionary<EntityType, List<EntityTypePolledEvent>>();



       

        #endregion

        #endregion

        /// <summary>
        /// here we define allegiance events. Since there are no "Allegiance Types", there is no use for a special connection, or Hook class to link them...
        /// Perhaps we 
        /// </summary>
        public Dictionary<string, AllegianceEventType> AllAllegianceEventTypes = new Dictionary<string, AllegianceEventType>();
        /// <summary>
        /// a computed multilist extracted from the above data.
        /// </summary>
        public Dictionary<AllegianceEvents, List<ActionSets>> AllegianceEvents = new Dictionary<AllegianceEvents, List<ActionSets>>();




        public Dictionary<string, HelpTopic> AllHelpTopics = new Dictionary<string, HelpTopic>();

        public Dictionary<string, HelpTopic> AllTutorialTopics = new Dictionary<string, HelpTopic>();


        #region Tag collections

        /// <summary>
        /// these collections are not to be used during gameplay... only during init. (the list references are then held in each entity type. alright I guess.)
        /// </summary>
        public Dictionary<string, List<EntityType>> ToolsByTag = new Dictionary<string, List<EntityType>>();
        public Dictionary<string, List<EntityType>> FuelByTag = new Dictionary<string, List<EntityType>>();
        public Dictionary<string, List<EntityType>> AmmoByTag = new Dictionary<string, List<EntityType>>();

        public Dictionary<string, List<IDetectableType>> DetectableTypeByTag = new Dictionary<string, List<IDetectableType>>();
      
        public Dictionary<string, List<EntityType>> FoodByTag = new Dictionary<string, List<EntityType>>();
        public Dictionary<string, List<EntityType>> ServantEntityTypeByTag = new Dictionary<string, List<EntityType>>();

      //  public Dictionary<string, List<EntityType>> UpgradesByTag = new Dictionary<string, List<EntityType>>();

        /// <summary>
        /// delete this..? too confusing to use?
        /// </summary>
        public Dictionary<string, List<EntityType>> GeneralTags = new Dictionary<string, List<EntityType>>();

        /// <summary>
        /// this collection is used to build bit arrays for quick lookup
        /// TODO: this gets confusing. let's use a list of types instead, like for the other tags
        /// </summary>
        public Dictionary<string, int> ContainerTags = new Dictionary<string, int>();

        public Dictionary<string, List<EntityType>> ContainersByTag = new Dictionary<string, List<EntityType>>();

    //    public Dictionary<string, List<ProcessType>> ProcessTypesByJobType = new Dictionary<string, List<ProcessType>>();

        #endregion

      //  public Dictionary<FoodNutrientType, bool> SatisfiesComfort = new Dictionary<FoodNutrientType, bool>();


        public Dictionary<EntityCategory, List<EntityType>> ItemTypesInCategory = new Dictionary<EntityCategory, List<EntityType>>();

        /// <summary>
        /// returns the items that can fit in an upgrade slot/category
        /// </summary>
        public Dictionary<UpgradeCategory, List<EntityType>> UpgraderEntityTypesByUpgradeCategory = new Dictionary<UpgradeCategory, List<EntityType>>();

        /// <summary>
        /// returns the structures that have an upgrade slot for the category key
        /// </summary>
        public Dictionary<UpgradeCategory, List<EntityType>> EntityTypesToUpgradeByUpgradeCategory = new Dictionary<UpgradeCategory, List<EntityType>>();


     //   public Dictionary<EntityType, ProcessType> ItemProductionChain = new Dictionary<EntityType, ProcessType>();
        // public Dictionary<ItemType, CropType> ItemHarvest = new Dictionary<ItemType,CropType>();
        // public Dictionary<ItemType, List<CropType>> ItemHarvestSource = new Dictionary<ItemType, List<CropType>>();
        //  public Dictionary<EntityType, CropType> ItemHarvestSource = new Dictionary<EntityType, CropType>();

        /// <summary>
        /// derived collection that has the inverse relationship between resource type and entity type
        /// </summary>
        public Dictionary<EntityType, ResourceType> ItemHarvestSource = new Dictionary<EntityType, ResourceType>();



        /*
        public Dictionary<ThreatCategory, List<ThreatCategory>> GlobalThreatenedBy;
        public Dictionary<ThreatCategory, Dictionary<ThreatCategory, byte>> GlobalThreatLevels;
        */
      
     

        /// <summary>
        /// does not include salvage processes
        /// </summary>
        public Dictionary<EntityType, List<ProcessType>> ProcessYieldsThisOutput = new Dictionary<EntityType, List<ProcessType>>();

        public Dictionary<EntityType, List<ProcessType>> SalvageProcessYieldsThisOutput = new Dictionary<EntityType, List<ProcessType>>();

        /// <summary>
        /// any production process, salvage and other
        /// </summary>
        public List<ProcessType> AllProductionProcesses = new List<ProcessType>();
        public List<ProcessType> NonSalvageProductionProcesses = new List<ProcessType>();

      //  public Dictionary<EntityType, List<ProcessType>> AllProcessesYieldsThisOutput = new Dictionary<EntityType, List<ProcessType>>();



        /// <summary>
        /// does not include salvage processes
        /// </summary>
        public Dictionary<EntityType, List<ProcessType>> ProcessesUsingThisInput = new Dictionary<EntityType, List<ProcessType>>();

     //   public Dictionary<EntityType, List<ProcessType>> SalvageProcessUsingThisInput = new Dictionary<EntityType, List<ProcessType>>();


        /// <summary>
        /// only used in GUI for now
        /// both entity types (process outputs) and processes
        /// </summary>
        public Dictionary<EntityType, HashSet<ProcessType>> ToolsUsedFor = new Dictionary<EntityType, HashSet<ProcessType>>();
       // public Dictionary<EntityType, List<EntityType>> ToolsUsedFor = new Dictionary<EntityType, List<EntityType>>();

        /// <summary>
        /// why not GameDataCollection..?
        /// </summary>
        public Dictionary<string, ParticleSystemType> AllParticleSystems = new Dictionary<string, ParticleSystemType>();

       // public Dictionary<string, Hint> AllHints;
        public List<Hint> AllHints;


        #region meta data
        public float OneOverMeanDamageFromHumanPunch;

        /// <summary>
        /// estimated damage score for attack types and body part types. 
        /// if a combination is not in the table, it means that the attack would have no effect.
        /// 
        /// these scores do not consider hitpoints.
        /// </summary>
        public Dictionary<AttackType, Dictionary<BodyPartType, float>> AttackScoresAgainstBodyParts =
                                        new Dictionary<AttackType, Dictionary<BodyPartType, float>>();

        /// <summary>
        /// estimates hitpoint damage using the mean hitpoints of an adult entity
        /// </summary>
        public Dictionary<AttackType, Dictionary<BodyType, float>> MeanAttackDamageAgainstEnemyTypes =
                                        new Dictionary<AttackType, Dictionary<BodyType, float>>();

        public Dictionary<AttackType, Dictionary<BodyType, float>> HighAttackDamageAgainstEnemyTypes =
                                       new Dictionary<AttackType, Dictionary<BodyType, float>>();

        #endregion



        /// <summary>
        /// TODO: make it data driven
        /// </summary>
        public Dictionary<string, ModelData> AllModels;

        public Dictionary<string, Texture> ExtraModelTextures;

       
        /// <summary>
        /// has no sim purpose.
        /// TODO: move to GameWorldRenderer next to GhostedSpriteSheet
        /// </summary>
        public ExtendedSpriteSheet BillboardSpriteSheet;

        /// <summary>     
        /// same
        /// </summary>
        public SpriteSheet LightSourcesSpriteSheet;
       // public LightSourceSpriteSheet LightSourcesSpriteSheet;

        public Dictionary<string, Animation2D> Animation2Ds = new Dictionary<string, Animation2D>();

        public Constants Constants;
        public AI.Constants.AIConstants AIConstants;
        public GUIConstants GUIConstants;

        public CustomDataPresentation CustomEntityActivityData;
        public CustomDataPresentation CustomStatusIconData;
        public CustomDataPresentation CustomSidePanelData;
        public CustomDataPresentation CustomOtherSiteSidePanelData;

        /// <summary>
        /// TODO: appears on the Data part of the Entity type tooltips
        /// </summary>
        public CustomDataPresentation CustomEntityTypeTooltipData;


        public TierType[] Tiers;


        #region non-data constants. We don't want this serialized. We are going to hardcode GameData keys here...

        // uses scenario names/folder names as unique key. This should make it tamper proof...
      //  public Dictionary<string, AchievementID> ScenarioWinAchievements = new Dictionary<string, AchievementID>();
           /* {
                { "TUTORIAL - Castaways", AchievementID.tutorialCompleted  }
            };*/


        #endregion


        private bool Advance()
        {
            The.LoadScreen.Progress("LoadGameData stage " + (state + 1), 100);
            ++state;
            return false;
        }

        private int state = 0;

        public bool LoadContent(UnclaimedWorld game, ContentManager content)
        {

            // TODO: pull content based on the entity types in data... progress will be hard to predict though..
            switch (state)
            {
                case 0://-----------------------------------------------------------------------------------
                    // race textures...
                    GameData.Instance.ExtraModelTextures = new Dictionary<string, Texture>();
                    GameData.Instance.ExtraModelTextures.Add("PatricianPurpleTexture", content.Load<Texture>("Models\\patrician_texture7"));
                    GameData.Instance.ExtraModelTextures.Add("PatricianWhiteTexture", content.Load<Texture>("Models\\patrician_texture6"));   
                    GameData.Instance.ExtraModelTextures.Add("PatricianZebraTexture", content.Load<Texture>("Models\\patrician_texture5"));  
                    GameData.Instance.ExtraModelTextures.Add("PatricianWaspTexture", content.Load<Texture>("Models\\patrician_texture4"));  
                    GameData.Instance.ExtraModelTextures.Add("PatricianBrownTexture", content.Load<Texture>("Models\\patrician_texture3"));  
                    return Advance();



                case 1://--------------------------------------------------------------------------------------------

                    GameData.Instance.ExtraModelTextures.Add("PatricianPaleTexture", content.Load<Texture>("Models\\patrician_texture2"));
                    GameData.Instance.ExtraModelTextures.Add("PatricianBlackTexture", content.Load<Texture>("Models\\patrician_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("QuaditeYellowTexture", content.Load<Texture>("Models\\twinkler_texture1")); 
                    GameData.Instance.ExtraModelTextures.Add("QuaditeRedTexture", content.Load<Texture>("Models\\twinkler_texture2"));  
                    GameData.Instance.ExtraModelTextures.Add("QuaditeTurquoiseTexture", content.Load<Texture>("Models\\twinkler_texture3"));
                    GameData.Instance.ExtraModelTextures.Add("QuaditeThinTexture", content.Load<Texture>("Models\\twinklerThin_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("QuaditeThinSpikyTexture", content.Load<Texture>("Models\\twinklerThinSpiky_texture1")); 

                    return Advance();



                case 2://--------------------------------------------------------------------------------------------


                    GameData.Instance.ExtraModelTextures.Add("QuaditeStripedTexture", content.Load<Texture>("Models\\twinkler_texture4"));  
                    GameData.Instance.ExtraModelTextures.Add("BirdPaleTexture", content.Load<Texture>("Models\\bird_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("BirdDarkTexture", content.Load<Texture>("Models\\bird_texture2"));
                    GameData.Instance.ExtraModelTextures.Add("BirdBlackTexture", content.Load<Texture>("Models\\bird_texture3"));
                    GameData.Instance.ExtraModelTextures.Add("BirdPurpleTexture", content.Load<Texture>("Models\\bird_texture3b"));
                    GameData.Instance.ExtraModelTextures.Add("BirdYellowTexture", content.Load<Texture>("Models\\bird_texture4"));
                    GameData.Instance.ExtraModelTextures.Add("BirdRedTexture", content.Load<Texture>("Models\\bird_texture5"));
                    GameData.Instance.ExtraModelTextures.Add("BirdVeryDarkTurqoiseTexture", content.Load<Texture>("Models\\bird_texture5b"));
                    GameData.Instance.ExtraModelTextures.Add("BirdVeryDarkGreenTexture", content.Load<Texture>("Models\\bird_texture5c"));
                    return Advance();



                case 3://--------------------------------------------------------------------------------------------


                    GameData.Instance.ExtraModelTextures.Add("BushdragonPaleTexture", content.Load<Texture>("Models\\scarecrow_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("BushdragonDarkTexture", content.Load<Texture>("Models\\scarecrow_texture2"));
                    
                    GameData.Instance.ExtraModelTextures.Add("ThunderchickenPaleTexture", content.Load<Texture>("Models\\thunderChicken_texture2"));
                    GameData.Instance.ExtraModelTextures.Add("ThunderchickenDarkTexture", content.Load<Texture>("Models\\thunderChicken_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("ThunderchickenBulkyTexture1", content.Load<Texture>("Models\\thunderChickenBulky_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("ThunderchickenBulkyTexture2", content.Load<Texture>("Models\\thunderChickenBulky_texture2"));
                    GameData.Instance.ExtraModelTextures.Add("ThunderchickenBulkyTexture3", content.Load<Texture>("Models\\thunderChickenBulky_texture3"));
                    GameData.Instance.ExtraModelTextures.Add("ThunderchickenThinTexture1", content.Load<Texture>("Models\\thunderChickenThin_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("ThunderchickenThinTexture2", content.Load<Texture>("Models\\thunderChickenThin_texture2"));
                    GameData.Instance.ExtraModelTextures.Add("ThunderchickenThinTexture3", content.Load<Texture>("Models\\thunderChickenThin_texture3"));
                    GameData.Instance.ExtraModelTextures.Add("SkinnedTestTexture", content.Load<Texture>("Models\\dogShepherd_texture1")); // for the skinned test model
                    GameData.Instance.ExtraModelTextures.Add("DemonTreeTexture", content.Load<Texture>("Models\\demonTree_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("DemonTreeMossTexture1", content.Load<Texture>("Models\\demonTreeMoss_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("DemonTreeMossTexture2", content.Load<Texture>("Models\\demonTreeMoss_texture2"));
                    GameData.Instance.ExtraModelTextures.Add("DemonTreeMossTexture3", content.Load<Texture>("Models\\demonTreeMoss_texture3"));
                    GameData.Instance.ExtraModelTextures.Add("SpikePlantTexture", content.Load<Texture>("Models\\spikePlant_texture1"));
                    return Advance();



                case 4://--------------------------------------------------------------------------------------------

                    GameData.Instance.ExtraModelTextures.Add("DogGermanShepherdTexture", content.Load<Texture>("Models\\dogShepherd_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherVariantTexture7", content.Load<Texture>("Models\\snatcher_texture7"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherVariantTexture6", content.Load<Texture>("Models\\snatcher_texture6"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherVariantTexture5", content.Load<Texture>("Models\\snatcher_texture5"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherVariantTexture4", content.Load<Texture>("Models\\snatcher_texture4"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherVariantTexture3", content.Load<Texture>("Models\\snatcher_texture3"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherVariantTexture2", content.Load<Texture>("Models\\snatcher_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherFemaleTexture", content.Load<Texture>("Models\\snatcher_texture2"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherArmoredTexture4", content.Load<Texture>("Models\\snatcherArmored_texture4"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherArmoredTexture3", content.Load<Texture>("Models\\snatcherArmored_texture3"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherArmoredTexture2", content.Load<Texture>("Models\\snatcherArmored_texture2"));
                    GameData.Instance.ExtraModelTextures.Add("SnatcherArmoredTexture1", content.Load<Texture>("Models\\snatcherArmored_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("WormTexture", content.Load<Texture>("Models\\worm_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("WormThinTexture", content.Load<Texture>("Models\\wormThin_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("WormSimpleTexture", content.Load<Texture>("Models\\wormSimple_texture2"));
                    GameData.Instance.ExtraModelTextures.Add("BushbackPaleTexture", content.Load<Texture>("Models\\bushback_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("BushbackDarkTexture", content.Load<Texture>("Models\\bushback_texture2"));
                    GameData.Instance.ExtraModelTextures.Add("TurnipPaleTexture", content.Load<Texture>("Models\\turnip_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("TurnipDarkTexture", content.Load<Texture>("Models\\turnip_texture2"));
                    return Advance();



                case 5://--------------------------------------------------------------------------------------------
                    GameData.Instance.ExtraModelTextures.Add("ManColorReplaceTexture", content.Load<Texture>("Models\\man_texture"));  //color replace.
                    GameData.Instance.ExtraModelTextures.Add("ManBlue1Texture", content.Load<Texture>("Models\\man_texture1"));  //blue stripes. white hair.
                    GameData.Instance.ExtraModelTextures.Add("ManBlue2Texture", content.Load<Texture>("Models\\man_texture2"));  // blue stripes. black skin black hair
                    GameData.Instance.ExtraModelTextures.Add("ManRed1Texture", content.Load<Texture>("Models\\man_texture3"));   // red stripes. black hair light skin
                    GameData.Instance.ExtraModelTextures.Add("ManRed2Texture", content.Load<Texture>("Models\\man_texture4"));   // red stripes. blond hair.
                    GameData.Instance.ExtraModelTextures.Add("ManGreen1Texture", content.Load<Texture>("Models\\man_texture5"));  // blue stripes.YES blue, even tho it says green. black hair light skin
                    return Advance();



                case 6://--------------------------------------------------------------------------------------------

                    GameData.Instance.ExtraModelTextures.Add("ManGreen2Texture", content.Load<Texture>("Models\\man_texture6"));  //green stripes. brown hair
                    GameData.Instance.ExtraModelTextures.Add("ManGrey1Texture", content.Load<Texture>("Models\\man_texture7"));  // black stripes. blond hair.
                    GameData.Instance.ExtraModelTextures.Add("ManGrey2Texture", content.Load<Texture>("Models\\man_texture8"));  //black stripes. brown hair
                    GameData.Instance.ExtraModelTextures.Add("ManGrey3Texture", content.Load<Texture>("Models\\man_texture9"));   //blue stripes. brown hair.                 
                    GameData.Instance.ExtraModelTextures.Add("ManBlueSolid1Texture", content.Load<Texture>("Models\\man_texture10"));   //blue suit. white hair.
                    GameData.Instance.ExtraModelTextures.Add("ManGreenSolid1Texture", content.Load<Texture>("Models\\man_texture15"));   //green suit. brown hair.
                    GameData.Instance.ExtraModelTextures.Add("ManGreySolid1Texture", content.Load<Texture>("Models\\man_texture16"));   //grey suit. blond hair.
                    GameData.Instance.ExtraModelTextures.Add("ManGreySolid2Texture", content.Load<Texture>("Models\\man_texture18"));   //grey suit, black hair
                    GameData.Instance.ExtraModelTextures.Add("ManBlueBrownClothes1Texture", content.Load<Texture>("Models\\man_textureClothes1"));
                    GameData.Instance.ExtraModelTextures.Add("ManGreenGreyClothes1Texture", content.Load<Texture>("Models\\man_textureClothes2"));
                    GameData.Instance.ExtraModelTextures.Add("ManGreyClothes1Texture", content.Load<Texture>("Models\\man_textureClothes3"));
                    GameData.Instance.ExtraModelTextures.Add("ManWhitePantsClothes1Texture", content.Load<Texture>("Models\\man_textureClothes4"));
                    GameData.Instance.ExtraModelTextures.Add("ManGreenBlueClothes1Texture", content.Load<Texture>("Models\\man_textureClothes5"));
                    GameData.Instance.ExtraModelTextures.Add("ManWhiteBlueClothes1Texture", content.Load<Texture>("Models\\man_textureClothes6"));
///////////////////////////////////
                    GameData.Instance.ExtraModelTextures.Add("ManBlueBrownClothesBrownHairTexture", content.Load<Texture>("Models\\man_textureClothes7"));
                    GameData.Instance.ExtraModelTextures.Add("ManOrangeGreyClothesYellowHairTexture", content.Load<Texture>("Models\\man_textureClothes8"));
                    GameData.Instance.ExtraModelTextures.Add("ManCurryClothesRedHairTexture", content.Load<Texture>("Models\\man_textureClothes9"));
                    GameData.Instance.ExtraModelTextures.Add("ManDarkRedClothesDarkSkinBrownHairTexture", content.Load<Texture>("Models\\man_textureClothes10"));
                    GameData.Instance.ExtraModelTextures.Add("ManBrownGreyClothesDarkSkinTexture", content.Load<Texture>("Models\\man_textureClothes11"));
                    GameData.Instance.ExtraModelTextures.Add("ManTurquoiseDarkClothesBlondHairTexture", content.Load<Texture>("Models\\man_textureClothes12"));
                    GameData.Instance.ExtraModelTextures.Add("ManSandyClothesBlackHairTexture", content.Load<Texture>("Models\\man_textureClothes13"));
                    GameData.Instance.ExtraModelTextures.Add("ManBurgundyClothesWhiteHairTexture", content.Load<Texture>("Models\\man_textureClothes14"));
                    GameData.Instance.ExtraModelTextures.Add("ManDarkBlueBeigeClothesBrownHairTexture", content.Load<Texture>("Models\\man_textureClothes15"));
                    GameData.Instance.ExtraModelTextures.Add("ManOrangeDarkGreyClothesYellowHairTexture", content.Load<Texture>("Models\\man_textureClothes16"));
                    GameData.Instance.ExtraModelTextures.Add("ManDarkBrownClothesRedHairTexture", content.Load<Texture>("Models\\man_textureClothes17"));
                    GameData.Instance.ExtraModelTextures.Add("ManBrownBeigeClothesDarkSkinTexture", content.Load<Texture>("Models\\man_textureClothes18"));
                    GameData.Instance.ExtraModelTextures.Add("ManSandyClothesDarkSkinTexture", content.Load<Texture>("Models\\man_textureClothes19"));
                    GameData.Instance.ExtraModelTextures.Add("ManBlueGreyClothesYellowHairTexture", content.Load<Texture>("Models\\man_textureClothes20"));
                    GameData.Instance.ExtraModelTextures.Add("ManOrangeGreyClothesRedHairTexture", content.Load<Texture>("Models\\man_textureClothes21"));
                    GameData.Instance.ExtraModelTextures.Add("ManCurryGreyClothesBrownSkinTexture", content.Load<Texture>("Models\\man_textureClothes22"));
                    GameData.Instance.ExtraModelTextures.Add("ManOchreClothesBlackHairTexture", content.Load<Texture>("Models\\man_textureClothes23"));
                    GameData.Instance.ExtraModelTextures.Add("ManBrownGreyClothesBlondHairTexture", content.Load<Texture>("Models\\man_textureClothes24"));
                    GameData.Instance.ExtraModelTextures.Add("ManTurquoiseDarkClothesBlackHairTexture", content.Load<Texture>("Models\\man_textureClothes25"));
                    GameData.Instance.ExtraModelTextures.Add("ManSandyDarkClothesWhiteHairTexture", content.Load<Texture>("Models\\man_textureClothes26"));

//////////////////////////////////////
                    GameData.Instance.ExtraModelTextures.Add("BinalRatBrownTexture", content.Load<Texture>("Models\\binalRat_texture1"));
                    GameData.Instance.ExtraModelTextures.Add("RobotTexture1", content.Load<Texture>("Models\\robot_texture1")); //grey
                    GameData.Instance.ExtraModelTextures.Add("RobotTexture2", content.Load<Texture>("Models\\robot_texture2")); //yellow/brown
                    return Advance();



                case 7://--------------------------------------------------------------------------------------------


                    GameData.Instance.AllModels = new Dictionary<string, ModelData>(); //MP: DO NOT copy paste this! it is the first entry of its kind so if you copy paste it you will get an error.

                   
                    GameData.Instance.AllModels.Add("sentry", new ModelData() 
                    {
                        Model = content.Load<Model>("Models\\sentry_idle"), // if this fails, check the InnerException! It is probably an embedded texture that is missing in the folder
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 6f, //5f, 
                        CRTDisplayLightIntensity = 5f, //3f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });
                    return Advance();



                case 8://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("skimmer", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\skimmer"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 0.8f,
                        ModelOffset = new Vector3(0f, 0f, 0f),
                        /*DriversSeatTranslation = new Vector3(5f, 10f, 0f),*/
                        // Matrix.CreateTranslation(5f, 10f, 0f)
                        /*DriversSeatScaling = 1.5f,*/
                        // DriverAttachBoneName = "" // "HULL"

                        DriverAttachor = new AttachPoint()
                        {
                            KeyName = "DriversSeat",
                            Translation = new Vector3(0f, 0f, 0f),
                            BoneName = "HULL",
                            AttachedAnimationName = "driving"
                        },

                        PassengerAttachors = new List<AttachPoint>(){ 
                                                                new AttachPoint() { KeyName = "FrontSeat", BoneName = "HULL", AttachedAnimationName = "passenger_1_seated", Translation = new Vector3(-5f, 10f, 0f) /*new Vector3(-5f, 10f, 0f)*/}
                }

                    });

                    return Advance();



                case 9://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("utilityvehicle", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\utility_vehicle"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2.4f,
                        ModelOffset = new Vector3(0f, 0f, 0f),

                        DriverAttachor = new AttachPoint()
                        {
                            KeyName = "DriversSeat",
                            Translation = new Vector3(5f, 10f, 0f),
                            BoneName = "hull",
                            AttachedAnimationName = "driving"
                        },

                        PassengerAttachors = new List<AttachPoint>(){ 
                                                                new AttachPoint() { KeyName = "FrontSeat", BoneName = "hull", AttachedAnimationName = "passenger_1_seated", Translation = new Vector3(-5f, 10f, 0f) /*new Vector3(-5f, 10f, 0f)*/}, 
                                                                new AttachPoint() { KeyName="CargoLeft", BoneName = "hull", AttachedAnimationName = "passenger_2_lying_left", Translation = new Vector3(5f, 10f, -10f)},
                                                                new AttachPoint() { KeyName="CargoRight", BoneName = "hull", AttachedAnimationName = "passenger_3_lying_right", Translation = new Vector3(-5f, 10f, -10f)}}

                    });

                    return Advance();



                case 10://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("patrician", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\patrician_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();



                case 11://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("turnip", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\turnip_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });
                    return Advance();



                case 12://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("twinkler", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\twinkler_idle"), //"Models\\twinkler_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });
                    return Advance();



                case 13://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("bird", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\bird_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });


                    return Advance();







                case 14://-----------------------------------------------------------------------------------------



                    GameData.Instance.AllModels.Add("thunderchicken", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\thunderChicken_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)                      
                    });
                    return Advance();



                case 15://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("bushdragon", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\scarecrow_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });


                    return Advance();



                case 16://--------------------------------------------------------------------------------------------


                    GameData.Instance.AllModels.Add("forestguardian", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\bushback_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f),
                        BackAttachor = new AttachPoint()
                        {
                            BoneName = "Root",
                            Translation = new Vector3(0f, 0f, 0f),
                            Rotation = new Vector3(0f, 0f, 0f)
                        }, //rifle trans {X:0 Y:-2,3 Z:0} rot {X:110 Y:0 Z:70}

                    });

                    return Advance();



                case 17://--------------------------------------------------------------------------------------------



                    GameData.Instance.AllModels.Add("man", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\man_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f), //new Vector3(-158f, 0f, 0f),
                        LeftHandAttachor = new AttachPoint()
                        {
                            Tag = "leftHand",
                            IsHand = true,
                            BoneName = "FingerLeftA",
                            // BodyPartName = "Left arm",
                            Translation = new Vector3(0.25f, -0.6f, 0f)
                        },
                        RightHandAttachor = new AttachPoint()
                        {
                            Tag = "rightHand",
                            IsHand = true,
                            BoneName = "FingerRightA",
                            //BodyPartName = "Right arm",
                            Translation = new Vector3(0.25f, -0.6f, 0f)
                        },
                        BackAttachor = new AttachPoint()
                        {
                            Tag = "back",
                            BoneName = "SpineD",
                            // BodyPartName = "Torso",
                            Translation = new Vector3(0f, -2.3f, 0f),
                            Rotation = new Vector3(110f, 0f, 70f)
                        }, //rifle trans {X:0 Y:-2,3 Z:0} rot {X:110 Y:0 Z:70}

                        HelmetAttachor = new AttachPoint()
                        {
                            Tag = "head",
                            BoneName = "Head",
                            Translation = new Vector3(0f, 0f, 0f),
                            Rotation = new Vector3(0f, 0f, 0f)
                        },
                        BottomAttachee = new AttachPoint()
                        {
                            Tag = "bottom",
                            BoneName = "SpineD",
                            Translation = new Vector3(0f, 0f, 0f),
                            Rotation = new Vector3(0f, 0f, 0f)
                        }/*, // the seat...
                        HasRunAnimation = true*/
                    });

                    return Advance();



                case 18://--------------------------------------------------------------------------------------------


                    GameData.Instance.AllModels.Add("woman", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\woman_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f),
                        LeftHandAttachor = new AttachPoint()
                        {
                            Tag = "leftHand",
                            IsHand = true,
                            BoneName = "FingerLeftA",
                            //BodyPartName = "Left arm",
                            Translation = new Vector3(0.25f, -0.6f, 0f)
                        },
                        RightHandAttachor = new AttachPoint()
                        {
                            Tag = "rightHand",
                            IsHand = true,
                            BoneName = "FingerRightA",
                            //BodyPartName = "Right arm",
                            Translation = new Vector3(0.25f, -0.6f, 0f)
                        },
                        BackAttachor = new AttachPoint()
                        {
                            Tag = "back",
                            BoneName = "SpineD",
                            //  BodyPartName = "Torso",
                            Translation = new Vector3(0f, -2.3f, 0f),
                            Rotation = new Vector3(110f, 0f, 70f)
                        }, //rifle trans {X:0 Y:-2,3 Z:0} rot {X:110 Y:0 Z:70}
                        HelmetAttachor = new AttachPoint()
                        {
                            Tag = "head",
                            BoneName = "Head",
                            //  BodyPartName = "Head",
                            Translation = new Vector3(0f, 0f, 0f),
                            Rotation = new Vector3(0f, 0f, 0f)
                        },
                        BottomAttachee = new AttachPoint()
                        {
                            Tag = "bottom",
                            BoneName = "SpineD",
                            Translation = new Vector3(0f, 0f, 0f),
                            Rotation = new Vector3(0f, 0f, 0f)// the seat...
                        }/*, 
                        HasRunAnimation = true*/
                    });
                    return Advance();



                case 19://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("box", new ModelData() // TODO: make two copies for robot/people so the two don't conflict
                    {
                        Model = content.Load<Model>("Models\\box"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,/*, ModelOffset = new Vector3(158f, 0f, 0f)*/
                        RightHandAttachee = new AttachPoint() // used by man!
                        {
                            BoneName = "AttacheeFingerRightA",
                            Rotation = new Vector3(178.11f, 103.465f, -147.874f),
                            Translation = new Vector3(2.966f, -4.173f, -3.281f)
                        },
                        LeftHandAttachee = new AttachPoint() // used by hauling robot!
                        {
                            BoneName = "AttacheeFingerLeftA" ,
                            Rotation = new Vector3(-180f, 180f, -180f), 
                            Translation = new Vector3(0.367f, -3.832f, -5.617f)                         
                        },
                        BottomAttachee = new AttachPoint() // used by hauling robot!
                        {
                            BoneName = "AttacheeBottom",
                            Rotation = new Vector3(180f, -1.417f, -180f),
                            Translation = new Vector3(0.079f, 0.236f, -11f)
                        }
                    });


                    GameData.Instance.AllModels.Add("backpackHeavy", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\backpackHeavy"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,//, ModelOffset = new Vector3(158f, 0f, 0f)
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeSpineD",
                            Rotation = new Vector3(-25.984f, 91.18101f, -133.701f),
                            Translation = new Vector3(-3.491f, -6.01f, -0.131f)
                        }

                    });



                    return Advance();



                case 20://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("backpack", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\backpack"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f/*, ModelOffset = new Vector3(158f, 0f, 0f)*/
                    });


                    return Advance();



                case 21://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("hammer", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\hammer"), //modeled similar as axe
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "HammerJoint",
                            Translation = new Vector3(1.129f, -3.438f, -0.60f),
                            Rotation = new Vector3(-154.5f, 60.95f, 114.8f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeFingerRightA",
                            Rotation = new Vector3(180f, -177.165f, -103.465f), //holding at end of handle
                            Translation = new Vector3(-1.942f, 0.157f, 0.052f)
                            //second try now wrong also:
               //             Rotation = new Vector3(180f, -177.165f, -103.465f), //holding at end of handle
              //              Translation = new Vector3(0.522f, 0.157f, 0.052f)
                            //first try all wrong?!?:
                        //    Rotation = new Vector3(180f, -177.165f, -103.465f), //holding at end of handle
                        //    Translation = new Vector3(0.627f, 0.472f, 0.052f)
                        //////////////
                            //was Rotation = new Vector3(180f, -175.3f, -115.7f) based on machete but all wrong here
                            //was Translation = new Vector3(-2.677f, -4.042f, -0.367f) 
                        }
                    });

                    return Advance();



                case 22://--------------------------------------------------------------------------------------------


                

                    GameData.Instance.AllModels.Add("spear", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\spear"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeSpineD",   //program looks for translation/rotation. it uses this priority: 1. animstate.  2. attachee (item. Here).   3. attachor (creature, top of this doc). (=first it looks in animstate, if nothing defined, it looks in attachee (Here), if nothing here it looks in attachor, further up this document, under "man".)
                            Translation = new Vector3(0f, -2.3f, 0f),
                            Rotation = new Vector3(110f, 0f, 70f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "SpearJoint",
                            Translation = new Vector3(0.656f, 0.079f, 0.079f),
                            Rotation = new Vector3(-83.622f, -0.472f, -17.48f)
                        }
                    });

             


                    GameData.Instance.AllModels.Add("machete", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\machete"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeSpineD",
                            Translation = new Vector3(1.129f, -3.438f, -0.60f),
                            Rotation = new Vector3(-154.5f, 60.95f, 114.8f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "MacheteJoint", //" "AttacheeFingerRightA"
                            Rotation = new Vector3(180f, -175.3f, -115.7f),
                            Translation = new Vector3(-0.341f, -0.236f, -0.026f)
                        }
                    });


                    GameData.Instance.AllModels.Add("pickaxe", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\pickaxe"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "PickaxeJoint",
                            Translation = new Vector3(1.129f, -3.438f, -0.60f),
                            Rotation = new Vector3(-154.5f, 60.95f, 114.8f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeFingerRightA", //
                                Rotation = new Vector3(180f, -177.165f, -103.465f), //holding in middle of handle
                                Translation = new Vector3(-2.467f, -4.462f, -0.262f)
                       //     Rotation = new Vector3(180f, -177.165f, -103.465f), //from hammer
                       //     Translation = new Vector3(-1.942f, 0.157f, 0.052f)
                         //   Rotation = new Vector3(180f, -175.3f, -115.7f),
                         //   Translation = new Vector3(-2.677f, -4.042f, -0.367f)
                            // mp these numbers are suited for a 2-handed hacking anim, where the right hand grips the end of the handle:
                      //      Rotation = new Vector3(180f, -175.3f, -115.7f),
                      //      Translation = new Vector3(-0.341f, -0.236f, -0.026f)
                        }
                    });


                    GameData.Instance.AllModels.Add("axe", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\axe"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AxeJoint",
                            Translation = new Vector3(1.129f, -3.438f, -0.60f),
                            Rotation = new Vector3(-154.5f, 60.95f, 114.8f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeFingerRightA",
                            Rotation = new Vector3(180f, -177.165f, -103.465f), //holding at end of handle
                            Translation = new Vector3(-1.942f, 0.157f, 0.052f)
                            //wrong:
                 //           Rotation = new Vector3(180f, -177.165f, -103.465f), //holding at end of handle
                 //           Translation = new Vector3(0.627f, 0.472f, 0.052f)

                        }
                    });



                    GameData.Instance.AllModels.Add("knife", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\knife"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeSpineD",
                            Translation = new Vector3(0f, -2.3f, 0f),
                            Rotation = new Vector3(110f, 0f, 70f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "KnifeJoint", //" "AttacheeFingerRightA"
                            Rotation = new Vector3(180f, -175.3f, -115.7f),
                            Translation = new Vector3(-0.341f, -0.236f, -0.026f)
                        }
                    });

                    GameData.Instance.AllModels.Add("watergun", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\watergun"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeSpineD",
                            Translation = new Vector3(0f, -2.3f, 0f),
                            Rotation = new Vector3(110f, 0f, 70f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "WatergunJoint", //" "AttacheeFingerRightA"
                            Translation = new Vector3(1.60f, 0.236f, -0.184f),
                            Rotation = new Vector3(-90.24f, 84.57f, -6.142f)
                        }
                    });

                    GameData.Instance.AllModels.Add("watergunTank", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\watergunTank"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeSpineD",
                            Translation = new Vector3(-0.656f, -2.598f, 1.076f),
                            Rotation = new Vector3(-96.85f, -92.126f, -8.031f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "WatergunTankJoint",
                            Translation = new Vector3(1.60f, 0.236f, -0.184f),
                            Rotation = new Vector3(-90.24f, 84.57f, -6.142f)
                        }
                    });


                    GameData.Instance.AllModels.Add("bow", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\bow"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeSpineD",
                            Translation = new Vector3(0.236f, -2.3f, -3.176f),
                            Rotation = new Vector3(65.669f, 62.835f, 24.094f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "BowJoint",
                            Translation = new Vector3(0.341f, 0.184f, -0.026f),
                            Rotation = new Vector3(-55.276f, 180f, 75.118f)
                        }
                    });


                    GameData.Instance.AllModels.Add("tablet", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\tablet"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeSpineD",
                            Translation = new Vector3(0.236f, -2.3f, -3.176f),
                            Rotation = new Vector3(65.669f, 62.835f, 24.094f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "TabletJoint",
                            Translation = new Vector3(-29.764f, 15.591f, 8.031f),
                            Rotation = new Vector3(1.601f, 0.709f, 0.026f)
                        }
                    });



                    return Advance();



                case 23://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("armsling", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\armsling"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeSpineD",
                            Translation = new Vector3(0f, 0f, 0f),
                            Rotation = new Vector3(180f, 0f, 90f)
                        },
                    });



                    return Advance();


                case 24://-----------------------------------------------------------------------------------------


                    GameData.Instance.AllModels.Add("rifle", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\rifle"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeSpineD",
                            Translation = new Vector3(0f, -2.3f, 0f),
                            Rotation = new Vector3(110f, 0f, 70f)
                        },
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "RifleJoint", //" "AttacheeFingerRightA"
                            Translation = new Vector3(1.60f, 0.236f, -0.184f),
                            Rotation = new Vector3(-90.24f, 84.57f, -6.142f)
                        }
                    });
                    return Advance();



                case 25://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("bush", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\bushbackPlant"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,/*, ModelOffset = new Vector3(158f, 0f, 0f)*/

                        // why not Bottom..?
                        BackAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeRoot",
                            //  Rotation = new Vector3(50.0f, 80f, 95f),
                            //    Translation = new Vector3(3f, -4.5f, -3f)
                        }

                        /* RightHandAttachee = new AttachPoint()
                         {
                             AttachBoneName = "AttacheeRoot",
                           //  Rotation = new Vector3(50.0f, 80f, 95f),
                         //    Translation = new Vector3(3f, -4.5f, -3f)
                         }*/

                    });

                    return Advance();



                case 26://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("meshtest", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\meshtest"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });
                    return Advance();



                case 27://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("skinnedtest", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\skinnedtest_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    //   GameData.Instance.AllModels.Add("manRerig_attachmentTest", new ModelData() { Model = content.Load<Model>("Models\\manRerig_attachmentTest"), ModelType = ModelType.Skinned, CRTDisplayScale = 2f, ModelOffset = new Vector3(-158f, 0f, 0f) });
                    return Advance();
                    

                case 28://--------------------------------------------------------------------------------------------

                 //   Effect myEffect = content.Load<Effect>("Content\\skinFX");

                    ExamineModelsAndSetProperties();

                    return Advance();

                case 29://--------------------------------------------------------------------------------------------
                    
                    GameData.Instance.BillboardSpriteSheet = content.Load<ExtendedSpriteSheet>("BuildingsAndTrees");

                    //GameWorldRenderer.SaveTextureToFile("BuildingsAndTrees", GameData.Instance.BillboardSpriteSheet.Texture);

                    return Advance();


                case 30://--------------------------------------------------------------------------------------------
                    GameData.Instance.LightSourcesSpriteSheet = game.Content.Load<SpriteSheet>("LightSources");
                    // GameData.Instance.LightSourcesSpriteSheet = game.Content.Load<LightSourceSpriteSheet>("LightSources");
                    //  GameData.Instance.AllLightSourceTypes = GameData.Instance.LightSourcesSpriteSheet.AllLightSourceData; // TODO

                    return Advance();


                case 31://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("demonTree", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\demonTree_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();


                case 32://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("spikePlant", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\spikePlant_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();

//        -----------------------------


                case 33://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("worm", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\worm_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();

                //        -----------------------------



                case 34://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("snatcher", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\snatcher_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();

                //        -----------------------------



                case 35://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("farmingHoe", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\farmingHoe"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,/*, ModelOffset = new Vector3(158f, 0f, 0f)*/
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeFingerRightA",
                            Translation = new Vector3(0.394f, -2.861f, -0.079f), 
                            Rotation = new Vector3(66.61401f, 61.89f, 157.323f)
                        }

                    });

                    GameData.Instance.AllModels.Add("shovel", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\shovel"),
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 2f,//, ModelOffset = new Vector3(158f, 0f, 0f)
                        RightHandAttachee = new AttachPoint()
                        {
                            BoneName = "AttacheeFingerRightA", //mp shovel was made on basis of hoe, so use those coords.
                            Translation = new Vector3(0.394f, -2.861f, -0.079f),
                            Rotation = new Vector3(66.61401f, 61.89f, 157.323f)
                        }

                    });
                    return Advance();

                case 36://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("demonTreeMoss", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\demonTreeMoss_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();


                case 37://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("snatcherArmored", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\snatcherArmored_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();


                case 38://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("thunderChickenBulky", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\thunderChickenBulky_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();

                case 39://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("thunderChickenThin", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\thunderChickenThin_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();

                case 40://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("twinklerThin", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\twinklerThin_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();


                case 41://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("twinklerThinSpiky", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\twinklerThinSpiky_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();

                //        -----------------------------

                case 42://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("dog", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\dog_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();



                case 43://--------------------------------------------------------------------------------------------
                    
                    // MP I got an error here because I copypasted case 7 ..instead make sure to take one which is further down the list.

                    GameData.Instance.AllModels.Add("robotLight", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\robotLight_idle"), //the light robot only has one arm, the right one: right hand: robotArm_R_end_jnt
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 6f, //5f,
                        CRTDisplayLightIntensity = 5f, //3f,
                        ModelOffset = new Vector3(0f, 0f, 0f),
                       RightHandAttachor = new AttachPoint()
                        {
                            Tag = "rightHand",
                            IsHand = true,
                            BoneName = "robotArm_R_end_jnt",
                       //     Translation = new Vector3(0.25f, -0.6f, 0f)//mp numbers copypasted from man
                        },
                    });
                    return Advance();

                case 44://--------------------------------------------------------------------------------------------


                    GameData.Instance.AllModels.Add("robotHeavy", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\robotHeavy_idle"), //heavy robot has 2 arms. the left hand is for grabbing, the right hand is for tools. has cargo area.
                        ModelType = ModelType.Stiff,
                        CRTDisplayScale = 6f, //5f,
                        CRTDisplayLightIntensity = 5f, //3f,
                        ModelOffset = new Vector3(0f, 0f, 0f),
                        LeftHandAttachor = new AttachPoint()
                        {
                            Tag = "leftHand",
                            IsHand = true,
                            BoneName = "robotArm_L_end_jnt",
                    //        Translation = new Vector3(0.25f, -0.6f, 0f)//mp numbers copypasted from man
                        },
                        RightHandAttachor = new AttachPoint()
                        {
                            Tag = "rightHand",
                            IsHand = true,
                            BoneName = "robotArm_R_end_jnt",
                     //       Translation = new Vector3(0.25f, -0.6f, 0f)//mp numbers copypasted from man
                        },
                        BackAttachor = new AttachPoint()
                        {
                            Tag = "back",
                            BoneName = "robotBody_jnt",
                  //          Translation = new Vector3(0f, -2.3f, 0f),//mp numbers copypasted from man
                 //           Rotation = new Vector3(110f, 0f, 70f)//mp numbers copypasted from man
                        }, 
                        
                    });
                    return Advance();


                case 45://--------------------------------------------------------------------------------------------

                    GameData.Instance.AllModels.Add("wormThin", new ModelData()
                    {
                        Model = content.Load<Model>("Models\\wormThin_idle"),
                        ModelType = ModelType.Skinned,
                        CRTDisplayScale = 2f,
                        ModelOffset = new Vector3(0f, 0f, 0f)
                    });

                    return Advance();

                //        -----------------------------

// Add new entry here and renumber the following case.

                case 46://--------------------------------------------------------------------------------------------
                    // progress???
                    foreach (var item in AllSoundData)
                    {
                        item.Value.LoadContent(content);
                    }

                    foreach (var item in AllEntityTypes)
                    {
                        item.Value.LoadContent(content);
                    }

                   /* foreach (var item in AllAttackTypes)
                    {
                        item.Value.LoadContent(content);
                    }*/

                    foreach (var item in AllActionSets)
                    {
                        item.Value.LoadContent(content);
                    }
                    foreach (var item in AllPolledEvents)
                    {
                        item.Value.LoadContent(content);
                    }
                    foreach (var item in AllParticleSystems)
                    {
                        item.Value.LoadContent(content);
                    }

                    return true;

            }//end switch


            //if control reaches this part of code, then there is a gap in the case constant sequence. this will catch it up eventually, but that should be fixed
            Debug.Assert(false, "if control reaches this part of code, then there is a gap in the case constant sequence. this will catch it up eventually, depending on the size of the gap but that should be fixed");
            state++;
            return false;
        }

        public static void ResolveEntityTypeTags(ref List<EntityType> resultList, string[] tags, string[] types, Dictionary<string, List<EntityType>> tagCollection)
        {
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Common.AddRangeToList(ref resultList, tagCollection[tag]);
                }
            }

            if (types != null)
            {
                foreach (var type in types)
                {
                    EntityType toolType = GameData.Instance.AllEntityTypes[type];

                    Common.AddToList(ref resultList, toolType);
                }
            }

            if (resultList != null)
            {
                resultList = resultList.Distinct().ToList();
            }
        }

        public static void ResolveEntityTypeTags(ref HashSet<EntityType> resultList, string[] tags, string[] types, Dictionary<string, List<EntityType>> tagCollection)
        {
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Common.AddRangeToSet(ref resultList, tagCollection[tag]);
                }
            }

            if (types != null)
            {
                foreach (var type in types)
                {
                    EntityType toolType = GameData.Instance.AllEntityTypes[type];

                    Common.AddToSet(ref resultList, toolType);
                }
            }
        }

        Dictionary<string, List<string>> allPostLoadContentValidationErrors;

        public void Initialize()
        {
          
            allPostLoadContentValidationErrors = new Dictionary<string, List<string>>();


            Init2DAnims();

            //--------------------------------------------------------------------------------------------

   
            SetupEventHooks();

            SetupOtherEvents();

            PostLoadContentInitialize();

            // process graph depends on Salvage processses being marked in EntityType.PostLoadContentInit:
            CreateProcessGraph();
            ValidateProcessTypeGraph();

            CreateRepairProcesses(allPostLoadContentValidationErrors); // depends on Process Graph

            CreateSpecialActionProcesses(); // depends on Process Graph

            MarkAnchorStructures(); // no purpose??

            List<string> duplicateKeyErrors = null; // can't use allPostLoadContentValidationErrors dict
            // some nested classes have keys for use in snapshotting. they need to be placed in the AllGameData dictionaries:
            ExtractNestedGameData(ref duplicateKeyErrors);
            // they may not have duplicate keys, breaks save
            DataLoader.DisplayValidationErrors(duplicateKeyErrors);


            OverrideEntityTypeDescriptions();

            InitContainerTransactHints(); // depends on both cretures and containers having fully inited
            InitOtherLists();
            InitMetaConstants();
            InitDamageScoreTables();
            InitWeaponEffectiveness();

            DataLoader.DisplayAllValidationErrors(allPostLoadContentValidationErrors); // there can be errors to display now, but more errors are gathered later also in PostLoadContetValidate..


            //--------------------------------------------------------------------------------------------
            /*
            GlobalThreatenedBy = new Dictionary<ThreatCategory, List<ThreatCategory>>();
            GlobalThreatLevels = new Dictionary<ThreatCategory, Dictionary<ThreatCategory, byte>>();

            GlobalThreatenedBy.Add(ThreatCategory.Human, new List<ThreatCategory>());
            GlobalThreatLevels.Add(ThreatCategory.Human, new Dictionary<ThreatCategory, byte>());
            GlobalThreatenedBy.Add(ThreatCategory.IndigHerbivore, new List<ThreatCategory>());
            GlobalThreatLevels.Add(ThreatCategory.IndigHerbivore, new Dictionary<ThreatCategory, byte>());
            GlobalThreatenedBy.Add(ThreatCategory.Predator, new List<ThreatCategory>());
            GlobalThreatLevels.Add(ThreatCategory.Predator, new Dictionary<ThreatCategory, byte>());
            GlobalThreatenedBy.Add(ThreatCategory.TameAnimal, new List<ThreatCategory>());
            GlobalThreatLevels.Add(ThreatCategory.TameAnimal, new Dictionary<ThreatCategory, byte>());

          
            // must be multiplicable by 1.5!!! And added to terrain etc. 0-100 are valid values
            GlobalThreatenedBy[ThreatCategory.Human].Add(ThreatCategory.Predator); //, 200));
            GlobalThreatLevels[ThreatCategory.Human].Add(ThreatCategory.Predator, 100);
            GlobalThreatenedBy[ThreatCategory.Human].Add(ThreatCategory.IndigHerbivore);//, 50));
            GlobalThreatLevels[ThreatCategory.Human].Add(ThreatCategory.IndigHerbivore, 30);
            //
         
            ///
            GlobalThreatenedBy[ThreatCategory.IndigHerbivore].Add(ThreatCategory.Human); //, 80));
            GlobalThreatLevels[ThreatCategory.IndigHerbivore].Add(ThreatCategory.Human, 60);
            GlobalThreatenedBy[ThreatCategory.IndigHerbivore].Add(ThreatCategory.Predator); //, 180));
            GlobalThreatLevels[ThreatCategory.IndigHerbivore].Add(ThreatCategory.Predator, 90);
                      
            //This should be redundant

            GlobalThreatenedBy[ThreatCategory.Predator].Add(ThreatCategory.Human);
            GlobalThreatLevels[ThreatCategory.Predator].Add(ThreatCategory.Human, 60);
            GlobalThreatenedBy[ThreatCategory.Predator].Add(ThreatCategory.Predator);
            GlobalThreatLevels[ThreatCategory.Predator].Add(ThreatCategory.Predator, 100);

            GlobalThreatenedBy[ThreatCategory.TameAnimal].Add(ThreatCategory.Predator);
            GlobalThreatLevels[ThreatCategory.TameAnimal].Add(ThreatCategory.Predator, 100);
            */

             
            /*
            // add more as needed...
            typeToCollectionMappings.Add(typeof(EntityType), k => AllEntityTypes[k]);
            typeToCollectionMappings.Add(typeof(AttackType), k => AllAttackTypes[k]);
            typeToCollectionMappings.Add(typeof(ProcessType), k => AllProcessTypes[k]);
            typeToCollectionMappings.Add(typeof(EventActionType), k => AllEventActionTypes[k]);
            typeToCollectionMappings.Add(typeof(SkillType), k => AllSkillTypes[k]);
            typeToCollectionMappings.Add(typeof(TriggerType), k => AllTriggerTypes[k]);
            typeToCollectionMappings.Add(typeof(ResourceType), k => AllResourceTypes[k]);
            typeToCollectionMappings.Add(typeof(SoilComponentType), k => AllSoilComponentTypes[k]);
            typeToCollectionMappings.Add(typeof(LowVegetationType), k => AllLowVegetationTypes[k]);
            typeToCollectionMappings.Add(typeof(SubstanceType), k => AllSubstanceTypes[k]);           
            typeToCollectionMappings.Add(typeof(FoodNutrientType), k => AllFoodNutrientTypes[k]);
            typeToCollectionMappings.Add(typeof(PolledEventType), k => AllPolledEvents[k]);
            typeToCollectionMappings.Add(typeof(PersonalityType), k => AllPersonalityTypes[k]);
            typeToCollectionMappings.Add(typeof(ActionSetType), k => AllActionSetTypes[k]);
            typeToCollectionMappings.Add(typeof(EntityCategory), k => AllItemCategories[k]);
            typeToCollectionMappings.Add(typeof(DefaultStorageSettings), k => AllDefaultStorageSettings[k]);
            typeToCollectionMappings.Add(typeof(StanceType), k => AllStanceTypes[k]);
            typeToCollectionMappings.Add(typeof(StorageCondition), k => AllStorageConditions[k]);
            typeToCollectionMappings.Add(typeof(ResourceCategory), k => AllResourceCategories[k]);
            typeToCollectionMappings.Add(typeof(FilterSettingType), k => AllFilterSettingTypes[k]);*/

        }


        private void CreateSpecialActionProcesses()
        {
            foreach (var item in AllEntityTypes)
            {
               /* if (item.Value.SpecialActions != null)
                {
                    List<string> list = new List<string>();
                    allPostLoadContentValidationErrors.Add("EntityTypes" + "/" + item.Key, list);
                */
                    item.Value.CreateSpecialProcesses();

               // }
            }
        }

       

        private void CreateRepairProcesses(Dictionary<string, List<string>> errors)
        {
            foreach (var item in AllEntityTypes)
            {
                if (item.Value.NonLivingType != null && item.Value.NonLivingType.EntityRepairProfile != null)
                {
                    List<string> list = new List<string>();
                    allPostLoadContentValidationErrors.Add("EntityTypes" + "/" + item.Key, list);


                    item.Value.NonLivingType.EntityRepairProfile.GenerateProcesses(item.Value, list);

                }
            }
        }

        /*private void ValidateKeysInNestedGameData(ref List<string> errors)
        {
            // get the nested event action types which have not been added to the dictionary. 
            foreach (var item in AllActionSets)
            {
                item.Value.ValidateKeysInNestedGameData(ref errors);
            }

            foreach (var item in AllPolledEvents)
            {
                if (item.Value.ActionSetsKey == null) // global events can refer to actions by key, or can define them inline (nested). Extract the nested ones here.
                {
                    item.Value.ActionSets.ValidateKeysInNestedGameData();
                }
            }

            foreach (var item in AllAllegianceEventTypes)
            {
                item.Value.ActionSets.ValidateKeysInNestedGameData();
            }
        }*/


        private void ExtractNestedGameData(ref List<string> duplicateKeyErrors)
        {
            // get the nested event action types which have not been added to the dictionary. 
            foreach (var item in AllActionSets)
            {
                item.Value.ExtractNestedGameData(ref duplicateKeyErrors);
            }
                   
            foreach (var item in AllPolledEvents) 
            {
                if (item.Value.ActionSetsKey == null) // global events can refer to actions by key, or can define them inline (nested). Extract the nested ones here.
                {
                    item.Value.ActionSets.ExtractNestedGameData(ref duplicateKeyErrors);
                }
            }

            foreach (var item in AllAllegianceEventTypes)
            {
                item.Value.ActionSets.ExtractNestedGameData(ref duplicateKeyErrors);
            }
        }


        /// <summary>
        /// overrides descriptions, names and texts on EntityType instances with custom ones, most likely defined in scenarios or mods
        /// </summary>
        private void OverrideEntityTypeDescriptions()
        {
            EntityType entityType;
            foreach (var item in AllEntityTypeDescriptions)
            {
                if (AllEntityTypes.TryGetValue(item.Value.EntityType, out entityType))
                {
                    item.Value.ApplyDescriptions(entityType);
                }
            }
        }

        public static string CreateKeyName()
        {
            return Guid.NewGuid().ToString();
        }

        private void Init2DAnims()
        {
            Animation2Ds.Add("campfireFast", new Animation2D(BillboardSpriteSheet.Texture, 0.1f, true)  //big bonfire flames
            {
                Origin = new Vector2(16f, 43f),
                Cells = new List<Cell>{ 
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_01")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_02")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_03")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_04")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_05")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_06")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_07")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_08")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_09")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfire_10"))}
            });

            //--------------------------------------------------------------------------------------------


            Animation2Ds.Add("campfireSmall", new Animation2D(BillboardSpriteSheet.Texture, 0.1f, true)  //smaller campfire flames
            {
                Origin = new Vector2(16f, 48f), // moved the anim 5 px up compared to campfireFast.  WHY does the anim wiggle so much!!! my pngs don't do that!? MP because PS screws them up with its anim function..; make sure you don't drag the folders into a small png and save from there, instead you need to crop the work file and then save from that. double check that layers don't move around...
                Cells = new List<Cell>{ 
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_01")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_02")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_03")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_04")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_05")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_06")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_07")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_08")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_09")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_campfireSmall_10"))}
            });

            //--------------------------------------------------------------------------------------------


            Animation2Ds.Add("fishCircling", new Animation2D(BillboardSpriteSheet.Texture, 0.075f, true)
            {
                Cells = new List<Cell>{ 
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_01")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_02")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_03")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_04")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_05")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_06")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_07")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_08")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_09")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_10")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_11")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_12")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_13")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_14")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_15")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_16")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_17")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_18")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_19")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_20")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_21")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_22")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_23")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_24")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_25")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_26")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_27")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_28")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_29")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_30")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_31")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_32")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_33")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_34")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_35")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_36")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_37")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_38")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fish_39"))}
            });

            //--------------------------------------------------------------------------------------------


            Animation2Ds.Add("fishSwarming", new Animation2D(BillboardSpriteSheet.Texture, 0.075f, true)
            {
                Cells = new List<Cell>{ 
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_01")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_02")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_03")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_04")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_05")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_06")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_07")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_08")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_09")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_10")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_11")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_12")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_13")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_14")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_15")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_16")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_17")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_18")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_19")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_20")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_21")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_22")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_23")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_24")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_25")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_26")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_27")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_28")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_29")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_30")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_31")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_32")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_33")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_34")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_35")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_36")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_37")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_38")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_39")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_fishSwarm_40"))}
            });


            //--------------------------------------------------------------------------------------------


            Animation2Ds.Add("butterfliesSwarm", new Animation2D(BillboardSpriteSheet.Texture, 0.04f, true)
            {
                Cells = new List<Cell>{ 
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_01")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_02")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_03")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_04")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_05")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_06")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_07")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_08")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_09")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_10")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_11")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_12")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_13")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_14")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_15")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_16")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_17")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_18")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_19")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_20")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_21")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_22")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_23")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_24")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_25")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_26")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_27")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_28")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_29")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_30")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_31")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_32")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_33")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_34")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_35")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_36")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_37")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_38")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_39")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_40")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_41")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_42")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_43")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_44")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_45")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_46")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_47")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_48")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_49")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_50")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_51")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_52")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_53")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_54")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_55")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_56")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_57")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_butterfly_58"))}
            });


            //--------------------------------------------------------------------------------------------


            Animation2Ds.Add("mosquitoSwarming", new Animation2D(BillboardSpriteSheet.Texture, 0.04f, true)
            {
                Cells = new List<Cell>{ 
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_01")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_02")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_03")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_04")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_05")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_06")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_07")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_08")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_09")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_10")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_11")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_12")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_13")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_14")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_15")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_16")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_17")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_18")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_19")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_20")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_21")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_22")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_23")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_24")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_25")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_26")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_27")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_28")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_29")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_30")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_31")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_32")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_33")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_34")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_35")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_36")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_37")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_38")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_39")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_40")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_41")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_42")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_43")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_44")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_45")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_46")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_47")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_48")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_49")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_50")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_51")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_52")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_53")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_54")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_55")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_56")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_57")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_mosquitoSwarm_58"))}
            });


            //--------------------------------------------------------------------------------------------


             Animation2Ds.Add("groundBugsSwarm", new Animation2D(BillboardSpriteSheet.Texture, 0.05f, true)
            {
                Cells = new List<Cell>{ 
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_1")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_2")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_3")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_4")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_5")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_6")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_7")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_8")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_9")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_10")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_11")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_12")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_13")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_14")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_15")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_16")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_17")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_18")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_19")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_20")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_21")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_22")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_23")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_24")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_25")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_26")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_26")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_27")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_28")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_29")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_30")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_31")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_32")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_33")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_34")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_35")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_36")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_37")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_38")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_39")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_40")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_41")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_42")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_43")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_44")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_45")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_46")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_47")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_48")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_49")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_groundBugs_50"))}
            });



             //--------------------------------------------------------------------------------------------


             Animation2Ds.Add("dragonflies", new Animation2D(BillboardSpriteSheet.Texture, 0.03f, true)
             {
                 Cells = new List<Cell>{ 
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_1")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_2")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_3")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_4")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_5")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_6")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_7")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_8")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_9")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_10")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_11")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_12")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_13")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_14")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_15")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_16")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_17")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_18")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_19")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_20")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_21")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_22")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_23")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_24")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_25")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_26")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_27")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_28")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_29")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_30")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_31")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_32")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_33")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_34")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_35")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_36")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_37")),
                    new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_38")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_39")),
                     new Cell(BillboardSpriteSheet.GetSourceRectangle("x_dragonfly_40"))}
             });



        }


        public static void InitializeComputerGeneratedData(IGameData data)
        {
            data.Initialize();
            data.PostDataCompleteInitialize();
        }

        private void PostLoadContentInitialize()
        {
            // this is the safest place to access tag collections. 

           // perhaps move these operations to the PostLoadAllDataInitialize method, there it will be performed by the loading thread instead of the main thread.


            List<string> errors = null;
            foreach (var item in AllDefaultStorageSettings)
            {
                item.Value.PostLoadContentInitialize();
            }

            foreach (var type in AllEntityTypes)
            {
                type.Value.PostLoadContentInitialize();
            }

            foreach (var type in AllParticleSystems)
            {
                type.Value.PostLoadContentInitialize();
            }


            foreach (var type in AllBodyTypes)
            {
                type.Value.PostLoadContentInitialize();
            }

            foreach (var type in AllAttackTypes)
            {
                type.Value.PostLoadContentInitialize();
            }

            foreach (var type in AllProcessTypes)
            {
                type.Value.PostLoadContentInitialize();
            }

            foreach (var type in AllEffectProfileTypes)
            {
                type.Value.PostLoadContentInitialize();
            }

            foreach (var type in AllSoilComponentTypes)
            {
                type.Value.PostLoadContentInitialize();
            }

            foreach (var type in AllLowVegetationTypes)
            {
                type.Value.PostLoadContentInitialize();
            }

            // New:
            foreach (var type in AllDetectionTypes)
            {
                errors = new List<string>();
                allPostLoadContentValidationErrors.Add("DetectionTypes" + "/" + type.Key, errors);


                type.Value.PostLoadContentInitialize(ref errors);
            }

            foreach (var item in AllResourceTypes)
            {
                item.Value.PostLoadContentInitialize();
            }
        }

        private void SetupOtherEvents()
        {
            AllegianceEvents.Clear();
            foreach (var item in AllAllegianceEventTypes)
            {
                Common.AddToMultiList(AllegianceEvents, item.Value.Event, item.Value.ActionSets);
            }

        }

        /// <summary>
        /// perform grouping of these lists
        /// </summary>
        private void SetupEventHooks()
        {            
            SetupEventHooksList(AllAgentActionHooks, ref AgentActionHooksByEntityType, AllEntityTypes);
            SetupEventHooksList(AllEntityEventHooks, ref EntityEventHooksByEntityType, AllEntityTypes);   
            SetupEventHooksList(AllAttackTypeEventHooks, ref EventHooksByAttackType, AllAttackTypes);
            SetupEventHooksList(AllProcessTypeEventHooks, ref EventHooksByProcessType, AllProcessTypes);
            SetupEventHooksList(AllEffectTypeEventHooks, ref EventHooksByEffectType, AllEffectProfileTypes);
            SetupEventHooksList(AllDetectedEntityEventHooks, ref DetectedEntityHooksByEntityType, AllEntityTypes);
            SetupEventHooksList(AllDetectedResourceEventHooks, ref DetectedResourceHooksByEntityType, AllEntityTypes);
            SetupEventHooksList(AllEntityPolledEvents, ref EntityPolledEventByEntityType, AllEntityTypes);   
           
        }


        /// <summary>
        /// groups the list of event hooks by the type they are related to (Attack type, Process type, Entity type)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="allEventHooks"></param>
        /// <param name="eventHooksByType"></param>
        /// <param name="allTypes"></param>
        private void SetupEventHooksList<T, U>(
            Dictionary<string, U> allEventHooks, 
            ref Dictionary<T, List<U>> eventHooksByType, 
            Dictionary<string, T> allTypes) where U: IHook
        {
            eventHooksByType = new Dictionary<T, List<U>>();
            foreach (var item in allEventHooks)
            {
                List<U> list;
                T mainType = allTypes[item.Value.TypeKey]; // Attack type/Process type/Entity type

                if (!eventHooksByType.TryGetValue(mainType, out list))
                {
                    list = new List<U>();
                    eventHooksByType.Add(mainType, list);
                }

                list.Add(item.Value);

                list = list.OrderBy(h => h.ExecutionOrder).ToList();
            }

        }

        private void InitContainerTransactHints()
        {
            foreach (var item in AllEntityTypes)
            {
                if (item.Value.ContainerType != null)
                {
                    item.Value.ContainerType.SetVerminCanAccess();
                }
            }

        }

        /// <summary>
        /// populate any other lists we need
        /// </summary>
        private void InitOtherLists()
        {
           /* foreach (var item in AllEntityTypes)
            {
                // good idea with extra lists...?
                if (item.Value.StructureType != null)
                {
                    GameData.Instance.AllStructureTypes.Add(item.Key, item.Value);
                }

                if (item.Value.TerrainType != null)
                {
                    GameData.Instance.AllTerrainFeatureTypes.Add(item.Key, item.Value);
                }

                // new....
                if (item.Value.TreeType != null)
                {
                    GameData.Instance.AllTreeTypes.Add(item.Key, item.Value);
                }

                if (item.Value.ItemType != null)
                {
                    GameData.Instance.AllItemTypes.Add(item.Key, item.Value);                   
                    
                }

                if (item.Value.BiologicalType != null)
                {
                    GameData.Instance.AllCreatureTypes.Add(item.Key, item.Value);
                }
            }*/


            // used on the Stocks panel etc:
            // only add the categories that are represented as items
            foreach (KeyValuePair<string, EntityCategory> kvp in GameData.Instance.AllEntityCategories)
            {
                List<EntityType> listOfItemTypes2 = new List<EntityType>();
                GameData.Instance.ItemTypesInCategory.Add(kvp.Value, listOfItemTypes2);
                foreach (KeyValuePair<string, EntityType> itemPair in GameData.Instance.AllItemTypes)
                {
                    if (itemPair.Value.Category == kvp.Value)
                    {
                        listOfItemTypes2.Add(itemPair.Value);
                    }
                }
            }

                       

            // used on the Build panel:
           /* foreach (KeyValuePair<string, StructureCategory> kvp in GameData.Instance.AllStructureCategories)
            {
                List<EntityType> listOfItemTypes2 = new List<EntityType>();
                GameData.Instance.StructureTypesInCategory.Add(kvp.Value, listOfItemTypes2);
                foreach (KeyValuePair<string, EntityType> itemPair in GameData.Instance.AllStructureTypes)
                {
                    if (itemPair.Value.StructureType.BuildByPlayer && itemPair.Value.StructureType.Category == kvp.Value)
                    {
                        listOfItemTypes2.Add(itemPair.Value);
                    }
                }
            }*/

        }

        /// <summary>
        /// init various constants derived from the data we have read in.
        /// </summary>
        private void InitMetaConstants()
        {
            try
            {
                AttackType personPunchAttack = AllEntityTypes["entity:human"].IntelligenceType.AttackTypes.First(
                        a => a.DamageFinal == GameData.instance.AllDamageTypes["blunt"] && a.KeyName.ToLowerInvariant().Contains("punch"));

               /* AttackType personPunchAttack = AllEntityTypes["entity:human"].IntelligenceType.AttackTypes.First(
                        a => a.Damage == AttackType.DamageTypes.Blunt && a.KeyName.ToLowerInvariant().Contains("punch"));
                */
                OneOverMeanDamageFromHumanPunch = 1f / personPunchAttack.DamageMean;
            }
            catch (Exception)
            {
                throw new Exception("Missing punch attack for person.");
            }

        }

        Dictionary<BodyType, HashSet<EntityType>> bodyToEntityMappings = new Dictionary<BodyType, HashSet<EntityType>>();

        private void InitDamageScoreTables()
        {
            foreach (var attackType in AllAttackTypes)
            {
                if (attackType.Key == "shootImprovedFireExtinguisherBushDragonPoison")
                {

                }

                Dictionary<BodyPartType, float> scores = new Dictionary<BodyPartType, float>();
                AttackScoresAgainstBodyParts.Add(attackType.Value, scores);

                Dictionary<BodyType, float> meanDamage = new Dictionary<BodyType, float>();
                MeanAttackDamageAgainstEnemyTypes.Add(attackType.Value, meanDamage);

                //Dictionary<BodyType, float> Damage = new Dictionary<BodyType, float>();
                //MeanAttackDamageAgainstEnemyTypes.Add(attackType.Value, meanDamage);

                float bodyHitpoints;
                
                foreach (var entityType in AllEntityTypes)
                {
                    BodyType bodyType = entityType.Value.BodyType;
                    if (bodyType != null) // only Bodies can be targeted
                    {
                        Common.AddToMultiList(bodyToEntityMappings, bodyType, entityType.Value);

                        if (!meanDamage.ContainsKey(bodyType))
                        {
                            bodyHitpoints = entityType.Value.GetMeanHitpoints().Value;

                            foreach (var bodypart in bodyType.BodyPartTypes)
                            {
                                SaveDamageScore(bodyType, bodyHitpoints, attackType.Value, scores, bodypart, meanDamage);
                            }
                        }
                    }
                }
                

              /*  foreach (var body in AllBodyTypes)
                {
                    foreach (var bodypart in body.Value.BodyPartTypes)
                    {
                        SaveDamageScore(body.Value, attackType.Value, scores, bodypart, meanDamage);
                    }

                }*/
            }
        }

        private void InitWeaponEffectiveness()
        {
            Dictionary<BodyType, float> bodyDamage = null;

            
            Dictionary<EntityType, EntityType> highlyEffectiveAgainst = new Dictionary<EntityType, EntityType>();

            foreach (var item in AllItemTypes)
            {
                
                if (item.Value.ItemType.WeaponType != null)
                {
                    highlyEffectiveAgainst.Clear();
                    if (item.Value.ItemType.WeaponType.AttackTypes != null)
                    {            
                        foreach (var attackType in item.Value.ItemType.WeaponType.AttackTypes)
                        {                          
                            if (MeanAttackDamageAgainstEnemyTypes.TryGetValue(attackType, out bodyDamage))
                            {
                                foreach (var damageKVP in bodyDamage)
                                {
                                   
                                    HashSet<EntityType> targetEntityTypes = bodyToEntityMappings[damageKVP.Key];

                                    foreach (var target in targetEntityTypes)
                                    {                                       
                                        if (!highlyEffectiveAgainst.ContainsKey(target))
                                        {
                                            float bodyHitpoints = target.GetMeanHitpoints().Value;

                                            if (damageKVP.Value > 0.3f * bodyHitpoints) // kills in 3 shots or less
                                            {
                                                highlyEffectiveAgainst.Add(target, target);
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }

                    // create a string from the result:
                    List<EntityType> orderedList = highlyEffectiveAgainst.Keys.OrderBy(k => k.Name).ToList();
                    StringBuilder highlyEffectiveAsString = new StringBuilder();
                    string delimiter = "";
                    foreach (var i in orderedList)
                    {
                        highlyEffectiveAsString.Append(delimiter);
                        highlyEffectiveAsString.Append(i.Name);

                        delimiter = ", ";
                    }
                    item.Value.ItemType.WeaponType.HighlyEffectiveAgainst = highlyEffectiveAsString.ToString();


                }                
            }

            bodyToEntityMappings.Clear();
        }



        private void SaveDamageScore(BodyType bodyType, float bodyHitpoints, AttackType attackType, Dictionary<BodyPartType, float> scores, BodyPartType bodypart, Dictionary<BodyType, float> meanDamages)
        {
            float meanDamage;
            float score = AttackJob.ComputeEstimatedDamageScore(attackType, bodypart, bodyHitpoints, out meanDamage);


            float oldMeanDamage;
            if (meanDamages.TryGetValue(bodyType, out oldMeanDamage))
            {
                // overwrite if higher:
                if (meanDamage > oldMeanDamage)
                {
                    meanDamages[bodyType] = meanDamage; 
                }
            }
            else
            {
                meanDamages[bodyType] = meanDamage;
            }

            if (score > 0f)
            {
                scores.Add(bodypart, score);
            }

            if (bodypart.BodyPartTypes != null)
            {
                foreach (var childPart in bodypart.BodyPartTypes)
                {
                    SaveDamageScore(bodyType, bodyHitpoints, attackType, scores, childPart, meanDamages);
                }
            }

        }

        private void CreateProcessGraph()
        {
            foreach (var process in AllProcessTypes)
            {
                if (!process.Value.IsOriginalSpecialAction)
                {
                    AddProcessToProductionGraph(process.Value);
                }
            }
        }

        public void AddProcessToProductionGraph(ProcessType process)
        {
            if (process.IsPartOfProductionChain())
            {
                // output:
                if (process.Outputs != null)
                {
                    foreach (var output in process.Outputs)
                    {
                        if (!output.IsWasteProduct)
                        {
                            int count = Common.AddToMultiList(ProcessYieldsThisOutput, output.FinalEntityTypeToCreate, process);

                        }
                    }

                }

                // input:
                if (process.InputsByType != null)
                {
                    foreach (var item in process.InputsByType)
                    {
                        Common.AddToMultiList(ProcessesUsingThisInput, item.Key, process);

                    }
                }

                //tools
                if (process.ProcessToolSet != null)
                {
                    foreach (var item in process.ProcessToolSet.Tools)
                    {
                        foreach (var tool in item.ToolsAndProductivity)
                        {
                            Common.AddToMultiList(ToolsUsedFor, 
                                tool.Item1, process);
                        }
                    }
                }

            }
            else if (process.IsSalvageProcess)
            {
                // salvage:
                if (process.Outputs != null)
                {
                    foreach (var output in process.Outputs)
                    {
                        if (!output.IsWasteProduct)
                        {
                            Common.AddToMultiList(SalvageProcessYieldsThisOutput, output.FinalEntityTypeToCreate, process);

                        }
                    }
                }

            }

            if (process.IsPartOfAttainableCalculation())
            {
                AllProductionProcesses.Add(process);

                if (!process.IsSalvageProcess)
                {
                    NonSalvageProductionProcesses.Add(process);
                }

            }
          
        }

        private void MarkAnchorStructures()
        {
            foreach (var item in AllEntityTypes)
            {
                item.Value.MarkAnchorStructures();
            }

        }

        private void ValidateProcessTypeGraph()
        {
            List<string> validationErrors;
           // Dictionary<string, List<string>> allPostInitValidationErrors = new Dictionary<string, List<string>>();

            foreach (var item in GameData.Instance.AllProcessTypes)
            {
                validationErrors = new List<string>();
                allPostLoadContentValidationErrors.Add("ValidateProcessTypeGraph" + "/" + item.Value.KeyName, validationErrors);
               // allPostInitValidationErrors.Add("ValidateProcessTypeGraph" + "/" + item.Value.KeyName, validationErrors);

                item.Value.PostProcessGraphValidate(validationErrors);
            }

         //   DataLoader.DisplayAllValidationErrors(allPostLoadContentValidationErrors);
        }

        public static void UnloadAllData()
        {
            instance = null;

        }


        public void PostDataCompleteInitialize()
        {
            Dictionary<string, List<string>> validationErrors = new Dictionary<string, List<string>>();

            // make these calls using the same ordering as when loading:

            var orderedCollections = AllGameDataCollections.OrderBy(k => k.Value.Order).ToList();

            foreach (var item in orderedCollections)
            {
                item.Value.PreDataCompleteValidate(validationErrors);
            }

            // break if any errors so far:
            DataLoader.DisplayAllValidationErrors(validationErrors);

            validationErrors.Clear();


            foreach (var item in orderedCollections)
            {
                item.Value.PostDataCompleteInitialize();
            }

            GUIConstants.PostDataCompleteInitialize();
            Constants.PostDataCompleteInitialize();
            AIConstants.PostDataCompleteInitialize();
           // CustomDataPresentation.post

            foreach (var item in orderedCollections)
            {
                item.Value.PostDataCompleteValidate(validationErrors);
            }

            // move/join these calls..?           
            DataLoader.DisplayAllValidationErrors(validationErrors);

            List<TierType> tiers = AllTierTypes.Values.ToList();
            this.Tiers = tiers.OrderBy(t => t.UpperEdge).ToArray();

            int index = 0;
            foreach (var item in Tiers)
            {
                item.Index = index;
                index++;
            }


        }

        private static void ExamineModelsAndSetProperties()
        {
            foreach (KeyValuePair<string, ModelData> modelData in GameData.Instance.AllModels)
            {
                // examine the model...
                foreach (ModelBone bone in modelData.Value.Model.Bones)
                {
                    if (bone.Name == "jet_engine_joint")
                    {
                        modelData.Value.HasAircraftDucts = true;
                        //break;
                    }
                    else if (bone.Name == "wheel_front_right_joint")
                    {
                        modelData.Value.HasSteerableFrontWheels = true;
                        //break;
                    }
                }

                foreach (ModelMesh mesh in modelData.Value.Model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        Vector3 emissiveColor = meshPart.Effect.Parameters["EmissiveColor"].GetValueVector3();

                        if (emissiveColor != Vector3.Zero)
                        {
                            modelData.Value.HasEmittingParts = true;
                            break;
                        }
                    }
                }

                //modelData.Value.

                // Look up our custom collision data from the Tag property of the model.
                Dictionary<string, object> tagData = (Dictionary<string, object>)modelData.Value.Model.Tag;

                if (tagData == null)
                {
                    throw new InvalidOperationException(
                        "Model.Tag is not set correctly. Make sure your model " +
                        "was built using the custom TrianglePickingProcessor.");
                }

                // Start off with a fast bounding sphere test.
                BoundingSphere boundingSphere = (BoundingSphere)tagData["BoundingSphere"];
                modelData.Value.BoundingSphereRadius = boundingSphere.Radius;


                // Replace the old effects with your custom shader
                /*   if (modelData.Value.ModelType == ModelType.Skinned)
                   {
                       foreach (ModelMesh mesh in modelData.Value.Model.Meshes)//PersonEntityType.Instance.Model.Meshes)
                       {
                           foreach (ModelMeshPart part in mesh.MeshParts)
                           {
                               if (part.Effect is BasicPaletteEffect)
                               {
                                   BasicPaletteEffect oldEffect = (BasicPaletteEffect)part.Effect;
                                   //  BasicEffect oldEffect = (BasicEffect)part.Effect;
                                   Effect newEffect = myEffect.Clone(); //game.ScreenManager.GraphicsDevice);
                                   //   Effect newEffect = oldEffect.Clone(ScreenManager.GraphicsDevice);                      
                                   newEffect.Parameters["BasicTexture"].SetValue(oldEffect.Texture);

                                   newEffect.Parameters["View"].SetValue(skinnedViewMatrix); //Map.renderer.view);
                                   newEffect.Parameters["Projection"].SetValue(skinnedProjMatrix); //Projection);
                                   part.Effect = newEffect;
                                   oldEffect.Dispose();
                               }
                               else if (part.Effect is BasicEffect)
                               {
                                   BasicEffect oldEffect = (BasicEffect)part.Effect;
                                   //  BasicEffect oldEffect = (BasicEffect)part.Effect;
                                   Effect newEffect = myEffect.Clone(); //game.ScreenManager.GraphicsDevice);
                                   //   Effect newEffect = oldEffect.Clone(ScreenManager.GraphicsDevice);                      
                                   newEffect.Parameters["BasicTexture"].SetValue(oldEffect.Texture);

                                   newEffect.Parameters["View"].SetValue(skinnedViewMatrix); //Map.renderer.view);
                                   newEffect.Parameters["Projection"].SetValue(skinnedProjMatrix); //Projection);
                                   part.Effect = newEffect;
                                   oldEffect.Dispose();
                               }
                           }
                       }
                   }*/
            }
        }

        public void PostLoadContentValidate()
        {
            //Dictionary<string, List<string>> allPostLoadContentValidationErrors = new Dictionary<string, List<string>>();

            allPostLoadContentValidationErrors = new Dictionary<string, List<string>>();


            List<string> validationErrors;

            foreach (var item in AllEntityTypes)
            {
                validationErrors = new List<string>();
                allPostLoadContentValidationErrors.Add(/*folderName + "/" +*/ item.Value.KeyName, validationErrors);

                item.Value.PostLoadContentValidate(ref validationErrors);


            }

            foreach (var item in AllActionSets)
            {
                validationErrors = new List<string>();
                allPostLoadContentValidationErrors.Add(item.Value.KeyName, validationErrors);

                item.Value.PostLoadContentValidate(validationErrors);
            }

            foreach (var item in AllEventActionTypes)
            {
                validationErrors = new List<string>();
                allPostLoadContentValidationErrors.Add(item.Value.KeyName, validationErrors);

                item.Value.PostLoadContentValidate(ref validationErrors);
            }


            BaseDataLoader.DisplayAllValidationErrors(allPostLoadContentValidationErrors);

        }

        /*   public void AddToProductionChain(EntityType carcassType, EntityType entityType)
           {
               //ItemHuntingSource.Add(carcassType, entityType);
           }*/

        /// <summary>
        /// call this after all types have been loaded! (PostLoadContentIntialize)
        /// </summary>
        /// <param name="resourceType"></param>
      /*  public void AddToProductionChain(ResourceType resourceType) 
        {
          
            if (!ItemHarvestSource.ContainsKey(resourceType.ResourceItemType))
            {
                ItemHarvestSource.Add(resourceType.ResourceItemType, resourceType);
            }
           
        }*/


        public static BitArray CreateBitArrayFromTags(Dictionary<string, int> allTags, string[] theseTags)
        {
            // create a bit array from the tags for possibly quicker lookup...
            BitArray bitArray = new BitArray(allTags.Count);
            if (theseTags != null)
            {
                foreach (var tag in theseTags)
                {
                    bitArray[allTags[tag]] = true;
                }
            }

            return bitArray;
        }


        /// <summary>
        /// remove these after a while...
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
       /* private string ReplaceRenamedEntityTypeKeys(string key)
        {
            if (key == "item:rareMetal") // v. 1.0.1.0
            {
                return "item:scandium";
            }
            else if (key == "item:rareMetal2")
            {
                return "item:terbium";
            }
            else if (key == "item:rareMetalOre1")
            {
                return "item:scandiumOre";
            }
            else if (key == "item:rareMetalOre2")
            {
                return "item:terbiumOre";
            }
            

            return key;
        }*/

        
        /// <summary>
        /// used in snapshotting IGameData
        /// </summary>
        public IGameData GetGameData(Type type, string keyName)
        {            
            IGameDataCollection collection;
            if (AllGameDataCollections.TryGetValue(type, out collection))
            {
                /*if (type == typeof(EntityType))
                {
                    keyName = ReplaceRenamedEntityTypeKeys(keyName);
                }*/

                return collection.Get(keyName);
            }
            else
            {
                throw new Exception("Missing GameData type in mappings");              
            }

        }

        /*
        public IGameData GetGameData(Type type, string keyName)
        {
            Func<string, IGameData> func;
            if (typeToCollectionMappings.TryGetValue(type, out func))
            {
                return func(keyName);
            }
            else
            {
                throw new Exception("Missing GameData type in mappings");
                return null;
            }
                        
        }*/
    }
}
