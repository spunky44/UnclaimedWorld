using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Items;
using System.IO;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Soil;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Buildings;
using System.Reflection;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Resources;
using System.Collections;
using UWGame.SimSide.Processes;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Jobs;
using UWGame.ClientSide.Renderables;
using UWGame;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.IngameEvents;
using UWGame.Client.Particles;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.Triggers;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide;

using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.Client.Audio;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.AllGameData.EventHooks;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.SimSide.Entities.Biological;
using UWGame.ClientSide.Particles;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities.Substances;
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
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland.Templates;
using UWGame.SimSide.Trade;
using UWGame.SimSide.Overland;
using UWGame.ClientSide.Hints;
using UWGame.SimSide.Scenarios;

namespace UWGame.SimSide.AllGameData
{
    /// <summary>
    /// should use pascal case - remove the capitalization, please
    /// </summary>
    public enum DataLoaderQueueState
    {
        Begin,
        AIConstants,
        Constants,
        GUIConstants,
        Sounds,
        SkillCategories,
        SkillTypes,
        ProfessionTypes,
        FoodNutrientTypes,
        DefaultStorageSettings,
        EntityCategories,
        StorageConditions,
        DegradeProfiles,
        FoodNutrientProfiles,       
        SoilTypes,  
        LowVegetationTypes,  
        BodyLayerTypes, 
        BodyTypes,       
        StanceTypes,
        StancesTypes,
        ActionSets,
        EventActionTypes,
        EntityData,
        SiteData,
        AllegianceData,
        AllegianceTemplates,
        SiteTemplates,
        ExpeditionData,
        TradeProfiles,
        TradeGroups,
        PricesProfiles,
        OfferDemandProfiles,
        VehicleProfiles,
        StructureProfiles,
        EventHooks,
        AttackTypes,
        DamageTypes,
        HelpTopics,
        ResourceCategories,
        Substances,
        IconInfo,
        ResourceTypes,
        TriggerTypes,
        Personalities,
        Traits,
        Cultures,
        Tiers,
        TierAreas,
        UpgradeCategories,
        UpgradeProfiles,
        RepairProfiles,
        BioOrders,
        AllegianceEvents,
        EntityTypes,
        EntityTypeDescriptions,
        DetectionTypes,
        FilterSettingTypes,     
        JobTypes,
        PresentationTypes,
        PresentationTypeCategories,
        PolledEventTypes,
        ProcessToolSets,
        ProcessTypes,
        EffectTypes,
        EffectProfileTypes,
        Particles,
        GUI,
        AttachableRenderableTypes,
             

        // place in sequence!

        DONE     
    }


    /// <summary>
    /// base class for the loader classes that are used to generate xml for all RG scenarios and game data types. 
    /// 
    /// there will be an instance for vanilla base data, one instance for each vanilla scenario, (vanilla mods?)...
    /// 
    /// Also, in release, it will deserialize its data from xml files in the correct location.
    /// </summary>
    public abstract class DataLoader
    {
        private DataLoaderQueueState queueState = DataLoaderQueueState.Begin;

       
        private Dictionary<string, List<string>> allPreInitValidationErrors;
        private Dictionary<string, List<string>> allPostInitValidationErrors;


        private Config.DataType dataSource;


        string folderName = "";
        /// <summary>
        /// should be set to the same value as the parent ScenarioLoader, if applicable
        /// </summary>
        public string FolderName
        {
            get
            {
                return folderName;
            }
            set { folderName = value; }
        }

        /// <summary>
        /// the percentage factor that the fixed progress indicator numbers should be multiplied with... a bit hackish...
        /// </summary>
        private float progressFactor;

       


        public DataLoader(Config.DataType dataSource, float progressFactor)
        {
            this.dataSource = dataSource;
            this.progressFactor = progressFactor;
        }


        /// <summary>
        /// we call this function to create/load a full data set and add/update/delete to previous data.
        /// gets called several times in a row, once for base data, then scenario, map, mods...
        /// 
        /// NEW: scenario settings can omit (base) data
        /// </summary>
        /// <returns></returns>
        public bool QueueInitGameData(Scenario scenario)
        {
            
            bool doSerialize = false;

            if (Sim.CurrentSerializeMode != Sim.SerializeMode.NoSerialize)
            {
                doSerialize = true;
            }

            bool loadMissionData = scenario != null ? scenario.ScenarioData.EnableMissions : true;


            switch (queueState)
            {
                case DataLoaderQueueState.Begin:
                    {
                        if (allPreInitValidationErrors == null)
                        {
                            allPreInitValidationErrors = new Dictionary<string, List<string>>();
                        }
                        if (allPostInitValidationErrors == null)
                        {
                            allPostInitValidationErrors = new Dictionary<string, List<string>>();
                        }

                        UpdateProgress(DataLoaderQueueState.AIConstants, 16);
                        break;
                    }
                case DataLoaderQueueState.AIConstants:
                    {                     
                        HandleDataTypeObject(InitAIConstants, 
                            ref GameData.Instance.AIConstants, 
                            "AIConstants.xml");
                        

                        //The.LoadScreen.Progress("Init Constants " + (doSerialize ? "Serializing..." : "..."), 203);
                        UpdateProgress(DataLoaderQueueState.Constants, 16);
                        break;
                    }
                  
                case DataLoaderQueueState.Constants:
                    {
                        HandleDataTypeObject(InitGameConstants,
                           ref GameData.Instance.Constants,
                           "constants.xml");

                        UpdateProgress(DataLoaderQueueState.GUIConstants, 15);
                        break;
                    }

                case DataLoaderQueueState.GUIConstants:
                    {
                        HandleDataTypeObject(InitGUIConstants,
                           ref GameData.Instance.GUIConstants,
                           "GUIConstants.xml");

                        UpdateProgress(DataLoaderQueueState.Sounds, 16);
                        break;
                    }
                case DataLoaderQueueState.Sounds:
                    {
                        HandleDataTypeList(InitSounds, 
                            GameData.Instance.AllSoundData, 
                            "sounds.xml");


                        UpdateProgress(DataLoaderQueueState.SkillCategories, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.SkillCategories:
                    {
                        HandleDataTypeList(InitSkillCategories,
                           GameData.Instance.AllSkillCategories,
                           "skillCategories.xml");

                      

                        UpdateProgress(DataLoaderQueueState.SkillTypes, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.SkillTypes:
                    {
                        HandleDataTypeList(InitSkillTypes,
                          GameData.Instance.AllSkillTypes,
                          "skillTypes.xml");

                        UpdateProgress(DataLoaderQueueState.ProfessionTypes, 15);
                     
                        break;
                    }
                case DataLoaderQueueState.ProfessionTypes:
                    {
                        HandleDataTypeList(InitProfessionTypes,
                          GameData.Instance.AllProfessionTypes,
                          "professionTypes.xml");

                        UpdateProgress(DataLoaderQueueState.FoodNutrientTypes, 0);
                        break;
                    }
                case DataLoaderQueueState.FoodNutrientTypes:
                    {
                        HandleDataTypeList(InitNutrientTypes,
                         GameData.Instance.AllFoodNutrientTypes,
                         "foodNutrientTypes.xml");

                        UpdateProgress(DataLoaderQueueState.DefaultStorageSettings, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.DefaultStorageSettings:
                    {
                        HandleDataTypeList(InitDefaultStorageSettings,
                            GameData.Instance.AllDefaultStorageSettings,
                            "defaultStorageSettings.xml");

                        UpdateProgress(DataLoaderQueueState.EntityCategories, 0);

                        break;
                    }
                case DataLoaderQueueState.EntityCategories:
                    {
                        HandleDataTypeList(InitEntityCategories, 
                           GameData.Instance.AllEntityCategories,
                           "itemCategories.xml");

                        UpdateProgress(DataLoaderQueueState.StorageConditions, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.StorageConditions:
                    {
                        HandleDataTypeList(InitStorageConditions,
                          GameData.Instance.AllStorageConditions,
                          "storageConditions.xml");

                        UpdateProgress(DataLoaderQueueState.DegradeProfiles, 32);

                        break;
                    }
                case DataLoaderQueueState.DegradeProfiles:
                    {
                        HandleDataTypeList(InitDegradeTypes, 
                          GameData.Instance.AllDegradeTypes,
                          "degradeTypes.xml");
                       
                       
                        UpdateProgress(DataLoaderQueueState.FoodNutrientProfiles, 15);
                     
                        break;
                    }
                case DataLoaderQueueState.FoodNutrientProfiles:
                    {
                        HandleDataTypeList(InitFoodNutrientProfiles,
                          GameData.Instance.AllFoodNutrientProfiles,
                          "foodNutrientProfiles.xml");


                        UpdateProgress(DataLoaderQueueState.SoilTypes, 0);
                     
                        break;
                    }
              
                case DataLoaderQueueState.SoilTypes:
                    {
                        HandleDataTypeList(InitSoilComponentTypes,
                            GameData.Instance.AllSoilComponentTypes,
                            "soilTypes.xml");

                        UpdateProgress(DataLoaderQueueState.LowVegetationTypes, 16);
                     
                        break;
                    }
                case DataLoaderQueueState.LowVegetationTypes:
                    {
                        
                        HandleDataTypeList(InitLowVegetationTypes,
                           GameData.Instance.AllLowVegetationTypes,
                           "lowVegetationTypes.xml");

                        UpdateProgress(DataLoaderQueueState.BodyLayerTypes, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.BodyLayerTypes:
                    {
                        HandleDataTypeList(InitBodyLayerTypes,
                          GameData.Instance.AllBodyLayerTypes,
                          "bodyLayerTypes.xml");

                        UpdateProgress(DataLoaderQueueState.BodyTypes, 31);

                        break;
                    }
                case DataLoaderQueueState.BodyTypes:
                    {
                        HandleDataTypeList(InitBodyTypes,
                          GameData.Instance.AllBodyTypes,
                          "bodyTypes.xml");

                        UpdateProgress(DataLoaderQueueState.StanceTypes, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.StanceTypes:
                    {
                        HandleDataTypeList(InitStanceTypes,
                          GameData.Instance.AllStanceTypes,
                          "stanceTypes.xml");

                        UpdateProgress(DataLoaderQueueState.StancesTypes, 16);

                        break;
                    }
                case DataLoaderQueueState.StancesTypes:
                    {
                        HandleDataTypeList(InitStancesTypes,
                          GameData.Instance.AllStancesTypes,
                          "stancesTypes.xml"); // note the 's'

                        UpdateProgress(DataLoaderQueueState.ActionSets, 109);

                        break;
                    }               
                case DataLoaderQueueState.ActionSets:
                    {
                        HandleDataTypeList(InitActionSets,
                             GameData.Instance.AllActionSets,
                             "actionSets.xml");

                        UpdateProgress(DataLoaderQueueState.EventActionTypes, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.EventActionTypes:
                    {
                        HandleDataTypeList(InitEventActionTypes,
                            GameData.Instance.AllEventActionTypes,
                            "eventActionTypes.xml");

                        UpdateProgress(DataLoaderQueueState.EntityData, 16);
                     
                        break;
                    }
                case DataLoaderQueueState.EntityData:
                    {
                        HandleDataTypeList(InitEntityData,
                            GameData.Instance.AllEntityData,
                            "entityData.xml");

                        UpdateProgress(DataLoaderQueueState.SiteData, 16);

                        break;
                    }
                case DataLoaderQueueState.SiteData:
                    {
                        HandleDataTypeList(InitSiteData,
                            GameData.Instance.AllSiteData,
                            "siteData.xml");

                        UpdateProgress(DataLoaderQueueState.AllegianceData, 16);

                        break;
                    }
                case DataLoaderQueueState.AllegianceData:
                    {
                        HandleDataTypeList(InitAllegianceData,
                            GameData.Instance.AllAllegianceData,
                            "allegianceData.xml");

                        UpdateProgress(DataLoaderQueueState.AllegianceTemplates, 16);

                        break;
                    }
                case DataLoaderQueueState.AllegianceTemplates:
                    {
                        HandleDataTypeList(InitAllegianceTemplates,
                            GameData.Instance.AllAllegianceTemplates,
                            "allegianceTemplates.xml");

                        UpdateProgress(DataLoaderQueueState.SiteTemplates, 16);

                        break;
                    }
                case DataLoaderQueueState.SiteTemplates:
                    {
                        HandleDataTypeList(InitSiteTemplates,
                            GameData.Instance.AllSiteTemplates,
                            "siteTemplates.xml");

                        UpdateProgress(DataLoaderQueueState.ExpeditionData, 16);

                        break;
                    }
                case DataLoaderQueueState.ExpeditionData:
                    {
                        if (loadMissionData)
                        {
                            HandleDataTypeList(InitExpeditionData,
                            GameData.Instance.AllExpeditionData,
                            "expeditionData.xml");
                        }

                        UpdateProgress(DataLoaderQueueState.TradeProfiles, 16);

                        break;
                    }
                case DataLoaderQueueState.TradeProfiles:
                    {
                        if (loadMissionData)
                        {
                            HandleDataTypeList(InitTradeProfiles,
                                GameData.Instance.AllTradeProfiles,
                                "tradeProfiles.xml");
                        }

                        UpdateProgress(DataLoaderQueueState.TradeGroups, 16);

                        break;
                    }

                case DataLoaderQueueState.TradeGroups:
                    {
                        if (loadMissionData)
                        {
                            HandleDataTypeList(InitTradeGroups,
                            GameData.Instance.AllTradeGroups,
                            "tradeGroups.xml");
                        }

                        UpdateProgress(DataLoaderQueueState.PricesProfiles, 16);

                        break;
                    }
                case DataLoaderQueueState.PricesProfiles:
                    {
                        if (loadMissionData)
                        {
                            HandleDataTypeList(InitPricesProfiles,
                            GameData.Instance.AllPricesProfiles,
                            "pricesProfiles.xml");
                        }

                        UpdateProgress(DataLoaderQueueState.OfferDemandProfiles, 16);

                        break;
                    }
                case DataLoaderQueueState.OfferDemandProfiles:
                    {
                        if (loadMissionData)
                        {
                            HandleDataTypeList(InitOfferDemandProfiles,
                            GameData.Instance.AllOfferDemandProfiles,
                            "offerDemandProfiles.xml");
                        }

                        UpdateProgress(DataLoaderQueueState.VehicleProfiles, 16);

                        break;
                    }
                case DataLoaderQueueState.VehicleProfiles:
                    {
                        HandleDataTypeList(InitVehicleProfiles,
                            GameData.Instance.AllVehiclesProfiles,
                            "vehicleProfiles.xml");

                        UpdateProgress(DataLoaderQueueState.StructureProfiles, 16);

                        break;
                    }
                case DataLoaderQueueState.StructureProfiles:
                    {
                        HandleDataTypeList(InitStructureProfiles,
                            GameData.Instance.AllStructuresProfiles,
                            "structureProfiles.xml");

                        UpdateProgress(DataLoaderQueueState.EventHooks, 16);

                        break;
                    }
                /*
                  AllAllegianceData = new GameDataCollection<AllegianceData>(AllGameDataCollections, DataLoaderQueueState.AllegianceData);
        AllAllegianceTemplates = new GameDataCollection<AllegianceTemplate>(AllGameDataCollections, DataLoaderQueueState.AllegianceTemplates);           
        AllSiteTemplates = new GameDataCollection<SiteTemplate>(AllGameDataCollections, DataLoaderQueueState.SiteTemplates);
        AllExpeditionData = new GameDataCollection<ExpeditionData>(AllGameDataCollections, DataLoaderQueueState.ExpeditionData);
*/
                case DataLoaderQueueState.EventHooks:
                    {
                       // InitEventHooks();
                        HandleDataTypeList(InitEntityEventHooks,
                           GameData.Instance.AllEntityEventHooks,
                           "entityEventHooks.xml");

                        HandleDataTypeList(InitAgentActionHooks,
                           GameData.Instance.AllAgentActionHooks,
                           "agentActionHooks.xml");

                        HandleDataTypeList(InitAttackTypeEventHooks, //bso
                           GameData.Instance.AllAttackTypeEventHooks,
                           "attackTypeEventHooks.xml");

                        HandleDataTypeList(InitProcessTypeEventHooks,
                          GameData.Instance.AllProcessTypeEventHooks,
                          "processTypeEventHooks.xml");

                        HandleDataTypeList(InitEffectTypeEventHooks,
                         GameData.Instance.AllEffectTypeEventHooks,
                         "effectTypeEventHooks.xml");

                        HandleDataTypeList(this.InitAttackTypeEventHooks,
                           GameData.Instance.AllAttackTypeEventHooks,
                           "attackTypeEventHooks.xml");

                        HandleDataTypeList(this.InitDetectEntityTypeHooks,
                            GameData.Instance.AllDetectedEntityEventHooks,
                            "detectedEntityEventHooks.xml");

                        HandleDataTypeList(this.InitDetectResourceTypeHooks,
                           GameData.Instance.AllDetectedResourceEventHooks,
                           "detectedResourceEventHooks.xml");


                        HandleDataTypeList(InitEntityPolledEvents,
                           GameData.Instance.AllEntityPolledEvents,
                           "entityPolledEvents.xml");

                        UpdateProgress(DataLoaderQueueState.AttackTypes, 31);
                     
                        break;
                    }
                case DataLoaderQueueState.AttackTypes:
                    {                      
                        HandleDataTypeList(InitAttackTypes,
                                GameData.Instance.AllAttackTypes,
                                "attackTypes.xml");

                        UpdateProgress(DataLoaderQueueState.DamageTypes, 16);
                     
                        break;
                    }
                case DataLoaderQueueState.DamageTypes:
                    {
                        HandleDataTypeList(InitDamageTypes,
                                GameData.Instance.AllDamageTypes,
                                "damageTypes.xml");

                        UpdateProgress(DataLoaderQueueState.HelpTopics, 0);

                        break;
                    }
                case DataLoaderQueueState.HelpTopics:
                    {
                        HandleDataTypeList(InitHelpTopics,
                               GameData.Instance.AllHelpTopics,
                               "helpTopics.xml");

                        HandleDataTypeList(InitTutorialTopics,
                               GameData.Instance.AllTutorialTopics,
                               "tutorialTopics.xml");

                        UpdateProgress(DataLoaderQueueState.ResourceCategories, 0);
                     
                        break;
                    }              
                case DataLoaderQueueState.ResourceCategories:
                    {
                        HandleDataTypeList(InitResourceCategories,
                            GameData.Instance.AllResourceCategories,
                            "resourceCategories.xml");

                        UpdateProgress(DataLoaderQueueState.Substances, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.Substances:
                    {
                        HandleDataTypeList(InitSubstanceTypes,
                           GameData.Instance.AllSubstanceTypes,
                           "substances.xml");

                        UpdateProgress(DataLoaderQueueState.IconInfo, 15);

                        break;
                    }
                case DataLoaderQueueState.IconInfo:
                    {
                        HandleDataTypeList(InitIconInfo,
                           GameData.Instance.AllIconInfo,
                           "iconInfo.xml");

                        UpdateProgress(DataLoaderQueueState.ResourceTypes, 32);

                        break;
                    }
                case DataLoaderQueueState.ResourceTypes:
                    {
                        HandleDataTypeList(InitResourceTypes,
                           GameData.Instance.AllResourceTypes,
                           "resourceTypes.xml");
                                                                    

                        UpdateProgress(DataLoaderQueueState.TriggerTypes, 0);
                     
                        break;
                    }
                /*     case QUEUESTATE.DETECTIONTYPES:
                         {
                             InitDetectionTypes();

                             The.LoadScreen.Progress("Init trigger types " + (doSerialize ? "Serializing..." : "..."), 16);
                             QueueState = QUEUESTATE.TRIGGERS;
                             break;
                         }*/
                case DataLoaderQueueState.TriggerTypes:
                    {                        
                        HandleDataTypeList(InitTriggerTypes,
                              GameData.Instance.AllTriggerTypes,
                              "triggerTypes.xml");

                        UpdateProgress(DataLoaderQueueState.Personalities, 15);
                     
                        break;
                    }
                case DataLoaderQueueState.Personalities:
                    {
                        HandleDataTypeList(InitPersonalityTypes,
                            GameData.Instance.AllPersonalityTypes,
                            "personalityTypes.xml");

                        UpdateProgress(DataLoaderQueueState.Traits, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.Traits:
                    {
                        HandleDataTypeList(InitTraitTemplates,
                            GameData.Instance.AllTraitTemplates,
                            "traitTemplates.xml");

                        UpdateProgress(DataLoaderQueueState.Cultures, 0);

                        break;
                    }
                case DataLoaderQueueState.Cultures:
                    {
                        HandleDataTypeList(InitCultureTemplates,
                            GameData.Instance.AllCultureTemplates,
                            "cultureTemplates.xml");

                        UpdateProgress(DataLoaderQueueState.Tiers, 0);

                        break;
                    }
                case DataLoaderQueueState.Tiers:
                    {
                        HandleDataTypeList(InitTiers,
                            GameData.Instance.AllTierTypes,
                            "tiers.xml");

                        UpdateProgress(DataLoaderQueueState.TierAreas, 0);

                        break;
                    }
                case DataLoaderQueueState.TierAreas:
                    {
                        HandleDataTypeList(InitTierAreas,
                            GameData.Instance.AllTierAreas,
                            "tierAreas.xml");

                        UpdateProgress(DataLoaderQueueState.UpgradeCategories, 0);

                        break;
                    }
                case DataLoaderQueueState.UpgradeCategories:
                    {
                        HandleDataTypeList(InitUpgradeCategories,
                            GameData.Instance.AllUpgradeCategories,
                            "upgradeCategories.xml");

                        UpdateProgress(DataLoaderQueueState.UpgradeProfiles, 0);

                        break;
                    }
                case DataLoaderQueueState.UpgradeProfiles:
                    {
                        HandleDataTypeList(InitUpgradeProfiles,
                            GameData.Instance.AllUpgradeProfiles,
                            "upgradeProfiles.xml");

                        UpdateProgress(DataLoaderQueueState.RepairProfiles, 0);

                        break;
                    }
                case DataLoaderQueueState.RepairProfiles:
                    {
                        HandleDataTypeList(InitRepairProfiles,
                            GameData.Instance.AllRepairProfiles,
                            "repairProfiles.xml");

                        UpdateProgress(DataLoaderQueueState.BioOrders, 0);

                        break;
                    }
                case DataLoaderQueueState.BioOrders:
                    {
                        HandleDataTypeList(InitBioOrderTypes,
                          GameData.Instance.AllBioOrderTypes,
                            "bioOrderTypes.xml");


                        UpdateProgress(DataLoaderQueueState.AllegianceEvents, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.AllegianceEvents:
                    {
                        HandleDataTypeList(InitAllegianceEvents,
                          GameData.Instance.AllAllegianceEventTypes,
                            "allegianceEventTypes.xml");

                        UpdateProgress(DataLoaderQueueState.EntityTypes, 813);
                        break;
                    }
                case DataLoaderQueueState.EntityTypes:
                    {
                        List<EntityType> newEntityTypes = HandleDataTypeList(InitEntityTypes,
                                                            GameData.Instance.AllEntityTypes,
                                                            "entityTypes.xml");
                        
                        // because EntityType can have references to EntityType, we must do a second pass to replace placeholders (i.e. EntityType instances with only KeyName filled in)

                        // replace placeholder entities (Parts...) here - they are needed to do ProcessType validation for instance.
                       /* if (newEntityTypes != null)
                        {
                            ReplaceEntityTypePlaceholders(newEntityTypes);
                        }*/

                        UpdateProgress(DataLoaderQueueState.EntityTypeDescriptions, 0);
                     

                        break;
                    }
                case DataLoaderQueueState.EntityTypeDescriptions:
                    {
                        HandleDataTypeList(InitEntityTypeDescriptions,
                           GameData.Instance.AllEntityTypeDescriptions,
                           "entityTypeDescriptions.xml");

                        UpdateProgress(DataLoaderQueueState.DetectionTypes, 15);
                     
                        break;
                    }
                case DataLoaderQueueState.DetectionTypes:
                    {                        
                        HandleDataTypeList(InitDetectionTypes,
                            GameData.Instance.AllDetectionTypes,
                            "detectionTypes.xml");

                        UpdateProgress(DataLoaderQueueState.FilterSettingTypes, 0);
                     
                        break;
                    }
                case DataLoaderQueueState.FilterSettingTypes:
                    {
                        HandleDataTypeList(InitFilterSettingTypes,
                           GameData.Instance.AllFilterSettingTypes,
                           "filterSettings.xml");

                        UpdateProgress(DataLoaderQueueState.JobTypes, 47);

                        break;
                    }
                case DataLoaderQueueState.JobTypes:
                    {
                        HandleDataTypeList(InitJobTypes,
                           GameData.Instance.AllJobTypes,
                           "jobTypes.xml");

                        UpdateProgress(DataLoaderQueueState.PresentationTypes, 47);

                        break;
                    }
             /*   case DataLoaderQueueState.Hints:
                    {
                        HandleDataTypeList(InitHints,
                           GameData.Instance.AllHints,
                           "hints.xml");

                        UpdateProgress(DataLoaderQueueState.PresentationTypes, 47);

                        break;
                    }*/
                case DataLoaderQueueState.PresentationTypes:
                    {
                        HandleDataTypeList(InitPresentationTypes,
                           GameData.Instance.AllPresentationTypes,
                           "presentationTypes.xml");

                        UpdateProgress(DataLoaderQueueState.PresentationTypeCategories, 47);
                     
                        break;
                    }
                case DataLoaderQueueState.PresentationTypeCategories:
                    {
                        HandleDataTypeList(InitPresentationTypeCategories,
                           GameData.Instance.AllPresentationTypeCategories,
                           "presentationTypeCategories.xml");

                        UpdateProgress(DataLoaderQueueState.PolledEventTypes, 47);

                        break;
                    } 
                case DataLoaderQueueState.PolledEventTypes:
                    {

                        HandleDataTypeList(InitGlobalConditionalEvents,
                          GameData.Instance.AllPolledEvents,
                          "globalEvents.xml");

                        UpdateProgress(DataLoaderQueueState.ProcessToolSets, 47);
                     
                        break;
                    }
                case DataLoaderQueueState.ProcessToolSets:
                    { 
                        // after entities but before processes
                        HandleDataTypeList(InitProcessToolSets,
                           GameData.Instance.AllProcessToolSets,
                           "processToolSets.xml");

                        UpdateProgress(DataLoaderQueueState.ProcessTypes, 406);                    
                       

                        break;
                    }
                case DataLoaderQueueState.ProcessTypes:
                    {
                        HandleDataTypeList(InitProcessTypes,
                            GameData.Instance.AllProcessTypes,
                            "processTypes.xml");

                        UpdateProgress(DataLoaderQueueState.EffectTypes, 16);                    
                       
                        break;
                    }
                case DataLoaderQueueState.EffectTypes:
                    {
                        HandleDataTypeList(InitEffectTypes,
                            GameData.Instance.AllEffectTypes,
                            "effectTypes.xml");

                        UpdateProgress(DataLoaderQueueState.EffectProfileTypes, 16);

                        break;
                    }
                case DataLoaderQueueState.EffectProfileTypes:
                    {
                        HandleDataTypeList(InitEffectProfileTypes,
                            GameData.Instance.AllEffectProfileTypes,
                            "effectProfiles.xml");

                        UpdateProgress(DataLoaderQueueState.Particles, 16);

                        break;
                    }
                case DataLoaderQueueState.Particles:
                    {
                        HandleDataTypeList(InitParticleSystems,
                           GameData.Instance.AllParticleSystems,
                           "particleSystems.xml");

                        UpdateProgress(DataLoaderQueueState.GUI, 15);                    
                      
                        break;
                    }

                case DataLoaderQueueState.GUI:
                    {
                        HandleDataTypeObject(InitStatusIconPresentation,
                          ref GameData.Instance.CustomStatusIconData,
                            "entityStatusIcons.xml");
                        GameData.Instance.CustomStatusIconData.Initialize(); // delete!!


                        HandleDataTypeObject(InitSidePanelPresentation,
                             ref GameData.Instance.CustomSidePanelData,
                                "sidePanelPresentation.xml");
                        GameData.Instance.CustomSidePanelData.Initialize();
                                               
                         HandleDataTypeObject(InitOtherSiteSidePanelPresentation,
                             ref GameData.Instance.CustomOtherSiteSidePanelData,
                                "otherSiteSidePanelPresentation.xml");
                        GameData.Instance.CustomOtherSiteSidePanelData.Initialize();
                        
                        HandleDataTypeObject(InitActivityPresentation,
                            ref GameData.Instance.CustomEntityActivityData,
                               "entityActivityIcons.xml");
                        GameData.Instance.CustomEntityActivityData.Initialize();

                   /*      HandleDataTypeObject(InitEntityTypeTooltipPresentation,
                            ref GameData.Instance.CustomEntityTypeTooltipData,
                               "entityTypeTooltip.xml");*/


                        UpdateProgress(DataLoaderQueueState.AttachableRenderableTypes, 0);     
                       
                        break;
                    }
                /*case QUEUESTATE.ImmigrationType:
                    {

                        UpdateProgress(QUEUESTATE.AttachableRenderableTypes, 50);

                        break;
                    }*/

                case DataLoaderQueueState.AttachableRenderableTypes:
                    {
                      /*  InitOtherLists();
                        InitMetaConstants();
                        // skip this during dev??
                        InitDamageScoreTables();
                        */

                        HandleDataTypeList(InitAttachableRenderableTypes,
                          GameData.Instance.AttachableRenderableTypes,
                          "attachableRenderableTypes.xml");

                        
                        //   SerializeAndDeserializeTypeList(listOfRenderableTypes, GameData.Instance.AttachableRenderableTypes, "", "attachableObjects.xml", Config.DataType.BaseData);

                        queueState = DataLoaderQueueState.DONE;
                        break;
                    }
                case DataLoaderQueueState.DONE:
                    {
                        DisplayAllValidationErrors(allPostInitValidationErrors);

                        //reset the progress variable:
                        queueState = DataLoaderQueueState.Begin;
                        allPreInitValidationErrors.Clear();
                        allPostInitValidationErrors.Clear();

                        return true;
                    }

            }

            return false;

        }

        private void UpdateProgress(DataLoaderQueueState nextStep, int lengthOfNextStepInMilliseconds) 
        {
            if ((int)nextStep < (int)queueState)
            {
                throw new Exception("Sequence error!");
            }

            The.LoadScreen.Progress(nextStep.ToString(), (int)(lengthOfNextStepInMilliseconds * progressFactor));
         //   The.LoadScreen.Progress("Init AI Constants " + (doSerialize ? "Serializing..." : "..."), 203);
            queueState = nextStep;
        }

        #region Helpers


        /// <summary>
        /// for data lists like Hint that does not implement IGameData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Init"></param>
        /// <param name="finalDictionary"></param>
        /// <param name="xmlFileName"></param>
        public static List<T> HandleOtherDataList<T>(Func<List<T>> Init, /*Dictionary<string, T> finalDictionary,*/ string xmlFileName)
        {
            List<T> list = null;
            if (Sim.CurrentSerializeMode != Sim.SerializeMode.Read)
            {
                list = Init();
            }

            // use FolderName here
            SerializeAndDeserializeOtherTypeList(list, /*finalDictionary,*/ "" /*FolderName*/, xmlFileName);

            return list;
        }

        private List<T> HandleDataTypeList<T>(Func<List<T>> Init, Dictionary<string, T> finalDictionary, string xmlFileName) where T : IGameData
        {
            List<T> list = null;
            if (Sim.CurrentSerializeMode != Sim.SerializeMode.Read)
            {
                list = Init();
            }

            // use FolderName here
            SerializeAndDeserializeTypeList(list, finalDictionary, FolderName /* ""*/, xmlFileName);

            return list;
        }

        private void HandleDataTypeObject<T>(Func<T> Init, ref T finalObject, string xmlFileName) where T: IGameDataObject
        {
            T item = default(T);
            if (Sim.CurrentSerializeMode != Sim.SerializeMode.Read)
            {
                item = Init();
            }

            SerializeAndDeserializeGameDataObject(item, ref finalObject, FolderName, xmlFileName, dataSource);

        }
      

        public static void DeserializeTypeList<T>(string folderPath, string filePath, out List<T> listToSerialize, Config.DataType dataType) // where T : IGameData
        {
            listToSerialize = null;

            // I changed this so we use a single xml file instead of a folder for each data type
            // like: entities.xml, attackTypes.xml etc.
            if (System.IO.Directory.Exists(folderPath))
            {
                // Deserialization
                XmlSerializer s = new XmlSerializer(typeof(List<T>));

                using (TextReader r = new StreamReader(filePath))
                {
                    listToSerialize = (List<T>)s.Deserialize(r);//this takes forever
                }
            }
            
        }




        /// <summary>
        /// add, update or delete items in the final dictionary from the list of new additions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listOfNewItems"></param>
        /// <param name="finalTypeDictionary"></param>
        /// <param name="fileDescriptor"></param>
        private void InitTypeList<T>(List<T> listOfNewItems, Dictionary<string, T> finalTypeDictionary, string fileDescriptor) where T : IGameData
        {
           
            foreach (T it in listOfNewItems)
            {
               
                if (it.DeleteRecord == true)
                {
                    // delete:
                    finalTypeDictionary.Remove(it.KeyName);
                }
                else
                {
                    if (!finalTypeDictionary.ContainsKey(it.KeyName))
                    {
                        // add:
                        finalTypeDictionary.Add(it.KeyName, it);
                    }
                    else
                    {
                        // update:
                        finalTypeDictionary[it.KeyName] = it;
                    }
                }
            }

            List<string> validationErrors;

            HashSet<string> uniqueKeys = new HashSet<string>();

            // Validate the loaded data!
            foreach (var item in listOfNewItems) // finalTypeDictionary)
            {
              /*  validationErrors = new List<string>();
                allPreInitValidationErrors.Add(fileDescriptor + "/" + item.KeyName, validationErrors);
                */
                if (item.DeleteRecord == false)
                {
                    validationErrors = null;

                    // NEW: check for duplicate keys:
                    if (uniqueKeys.Contains(item.KeyName))
                    {                    
                        allPreInitValidationErrors.Remove(fileDescriptor + "/" + item.KeyName); // only store one error for each set of duplicates

                        EntityType.CreateValidationError(ref validationErrors, item + " is a duplicate key.");

                    }
                    else
                    {
                        uniqueKeys.Add(item.KeyName);

                        item.PreInitValidate(ref validationErrors);
                    }

                    if (validationErrors != null)
                    {
                        allPreInitValidationErrors.Add(fileDescriptor + "/" + item.KeyName, validationErrors);              
                    }

                  
                }
            }

            // break if any errors so far:
            DisplayAllValidationErrors(allPreInitValidationErrors);

            // otherwise init:
            foreach (var item in listOfNewItems) // finalTypeDictionary)
            {
                if (item.DeleteRecord == false)
                {
                    item.Initialize();
                }
            }

            // Validate the initialized data!
            foreach (var item in listOfNewItems) // finalTypeDictionary)
            {
               /* validationErrors = new List<string>();
                allPostInitValidationErrors.Add(fileDescriptor + "/" + item.KeyName, validationErrors);
                */
                validationErrors = null;

                if (item.DeleteRecord == false)
                {
                    item.PostInitValidate(ref validationErrors);

                    if (validationErrors != null)
                    {
                        allPostInitValidationErrors.Add(fileDescriptor + "/" + item.KeyName, validationErrors);
                    }
               
                }
            }
        }

       /* public static void Serialize(Type typeToSerialize, object dataToSerialize, string path)
        {
            XmlSerializerNamespaces ns;
            XmlSerializer s;

            //Create our own namespaces for the output
            ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");
            s = new XmlSerializer(typeToSerialize);

            using (TextWriter w = new StreamWriter(path))
            {
                s.Serialize(w, dataToSerialize, ns);
            }

        }*/

        /*
        public static void ReplaceEntityTypePlaceholders()
        {
            ReplaceEntityTypePlaceholders(GameData.Instance.AllResourceTypes);

         //   ReplaceEntityTypePlaceholders(GameData.Instance.AllEntityTypes); // was done after each entity type batch
           
            ReplaceEntityTypePlaceholders(GameData.Instance.AllAttackTypes);
        }*/

        /// <summary>
        /// only call this after all data has loaded
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityType"></param>
        /// <param name="tags"></param>
        /// <param name="tagCollection"></param>
        public static void AddToTagCollection<T>(T entityType, string[] tags, Dictionary<string, List<T>> tagCollection)
        {
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    List<T> listOfTypes;
                    if (!tagCollection.TryGetValue(tag,
                        out listOfTypes))
                    {
                        listOfTypes = new List<T>();
                        tagCollection.Add(tag, listOfTypes);
                    }
                    listOfTypes.Add(entityType);
                }
            }

        }

        public static void AddToTagCollection<T>(T entityType, string tag, Dictionary<string, List<T>> tagCollection)
        {
            if (tag != null)
            {
                List<T> listOfTypes;
                if (!tagCollection.TryGetValue(tag,
                    out listOfTypes))
                {
                    listOfTypes = new List<T>();
                    tagCollection.Add(tag, listOfTypes);
                }

                listOfTypes.Add(entityType);
            }
        }

        public static void DisplayAllValidationErrors(Dictionary<string, List<string>> allErrors)
        {
            StringBuilder errors = new StringBuilder();
            foreach (var kvp in allErrors)
            {
                if (kvp.Value.Count > 0)
                {
                    errors.Append(kvp.Key);
                    errors.AppendLine(":");
                    foreach (string error in kvp.Value)
                    {
                        errors.AppendLine(error);
                    }
                }
            }

            DisplayErrors(errors);
        }

        public static void DisplayValidationErrors(List<string> errorList)
        {
            if (errorList != null)
            {
                StringBuilder errors = new StringBuilder();
                foreach (string error in errorList)
                {
                    errors.AppendLine(error);
                }

                DisplayErrors(errors);
            }
        }

        private static void DisplayErrors(StringBuilder errors)
        {
            // perhaps display this in a better way... with a dialog or something... without crashing the game
            if (errors.ToString() != "")
            {
                throw (new Exception("Errors were found during validation: " + errors.ToString()));
            }
        }

       
      
      /*  public static EntityType CreatePlaceholder(string keyName)
        {
            return new EntityType(keyName);
        }*/

        /// <summary>
        /// for other data lists (Hint etc.)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listToSerialize"></param>
        /// <param name="finalTypeDictionary"></param>
        /// <param name="folderName"></param>
        /// <param name="xmlFileName"></param>
        private static void SerializeAndDeserializeOtherTypeList<T>(List<T> listToSerialize, /*Dictionary<string, T> finalTypeDictionary,*/ string folderName, string xmlFileName) 
        {
            Config.DataType dataSource = Config.DataType.BaseData;
                        
            if (Sim.CurrentSerializeMode != Sim.SerializeMode.NoSerialize)
            {
                string folderPath = Config.GetDataFolderPath(dataSource, folderName, "");                               
                string filePath = Config.GetDataFolderPath(dataSource, folderName, xmlFileName);

                // write if requested
                if (Sim.CurrentSerializeMode == Sim.SerializeMode.WriteAndRead)
                {
                    XmlSerializer s = new XmlSerializer(typeof(List<T>));


                    // create, in case we forgot...
                    if (!System.IO.Directory.Exists(folderPath))
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                    }

                    using (TextWriter w = new StreamWriter(filePath))
                    {
                        s.Serialize(w, listToSerialize);
                    }
                }

                // now read...
                DeserializeTypeList<T>(folderPath, filePath, out listToSerialize, dataSource);

            }          
        }


        /// <summary>
        /// for IGameData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listToSerialize"></param>
        /// <param name="finalTypeDictionary"></param>
        /// <param name="folderName"></param>
        /// <param name="xmlFileName"></param>
        private void SerializeAndDeserializeTypeList<T>(List<T> listToSerialize, Dictionary<string, T> finalTypeDictionary, string folderName, string xmlFileName) where T : IGameData 
        {
           // Config.DataType dataSource = dataSource;

            if (Sim.CurrentSerializeMode != Sim.SerializeMode.NoSerialize)
            {
                string folderPath = Config.GetDataFolderPath(dataSource, folderName, "");

                //    string oldPath = DataFolder + folderName + "/" + xmlFileName;
                string filePath = Config.GetDataFolderPath(dataSource, folderName, xmlFileName);

                // write if requested
                if (Sim.CurrentSerializeMode == Sim.SerializeMode.WriteAndRead)
                {
                    XmlSerializer s = new XmlSerializer(typeof(List<T>));


                    // create, in case we forgot...
                    if (!System.IO.Directory.Exists(folderPath))
                    {
                        System.IO.Directory.CreateDirectory(folderPath);
                    }

                    using (TextWriter w = new StreamWriter(filePath))
                    {
                        s.Serialize(w, listToSerialize);
                    }
                }

                // now read...
                DeserializeTypeList<T>(folderPath, filePath, out listToSerialize, dataSource);

            }
                      

            // init and validate, update, add to or delete from current lists
            if (finalTypeDictionary != null 
                && listToSerialize != null)
            {      
                InitTypeList<T>(listToSerialize, finalTypeDictionary, dataSource.ToString() + "/" + xmlFileName);
            }
        }

        /// <summary>
        /// for game data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize"></param>
        /// <param name="finalDataObject"></param>
        /// <param name="folderName"></param>
        /// <param name="xmlFileName"></param>
        /// <param name="dataType"></param>
        public static void SerializeAndDeserializeGameDataObject<T>(T objectToSerialize, ref T finalDataObject, string folderName, string xmlFileName, Config.DataType dataType) where T: IGameDataObject
        {
            if (Sim.CurrentSerializeMode != Sim.SerializeMode.NoSerialize)
            {
                // write if requested
                if (Sim.CurrentSerializeMode == Sim.SerializeMode.WriteAndRead)
                {
                    SerializeObject<T>(objectToSerialize, folderName, xmlFileName, dataType);
                }

                // now read...
                DeserializeObject<T>(folderName, xmlFileName, out objectToSerialize, dataType);

            }


            // scenario data should only overwrite Constants and AIConstants if they have replaced those files:
            if (objectToSerialize != null)
            {
                finalDataObject = objectToSerialize;

                finalDataObject.Initialize();
            }

        }

        /// <summary>
        /// for non-game data use... really needed?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize"></param>
        /// <param name="finalDataObject"></param>
        /// <param name="folderName"></param>
        /// <param name="xmlFileName"></param>
        /// <param name="dataType"></param>
        public static void SerializeAndDeserializeObject<T>(T objectToSerialize, ref T finalDataObject, string folderName, string xmlFileName, Config.DataType dataType)
        {

            if (Sim.CurrentSerializeMode != Sim.SerializeMode.NoSerialize)
            {
                // write if requested
                if (Sim.CurrentSerializeMode == Sim.SerializeMode.WriteAndRead)
                {
                    SerializeObject<T>(objectToSerialize, folderName, xmlFileName, dataType);
                }

                // now read...
                DeserializeObject<T>(folderName, xmlFileName, out objectToSerialize, dataType);

            }

 
            // scenario data should only overwrite Constants and AIConstants if they have replaced those files:
            if (objectToSerialize != null)
            {
               
                finalDataObject = objectToSerialize;
            }

        }

        public static void SerializeObject<T>(T objectToSerialize, string folderName, string xmlFileName, Config.DataType dataType)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));

            string folderPath = Config.GetDataFolderPath(dataType, folderName, "");

            // create, in case we forgot...
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            //    string oldPath = DataFolder + folderName + "/" + xmlFileName;
            string filePath = Config.GetDataFolderPath(dataType, folderName, xmlFileName);
            using (TextWriter w = new StreamWriter(filePath))
            {
                s.Serialize(w, objectToSerialize);
            }
        }


        /// <summary>
        /// Let's use this method for all deserializing of maps, scenarios, mods etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folderName"></param>
        /// <param name="xmlFileName"></param>
        /// <param name="objectToSerialize"></param>
        /// <param name="dataType"></param>
        public static void DeserializeObject<T>(string folderName, string xmlFileName, out T objectToSerialize, Config.DataType dataType)
        {

            string file;

            if (!string.IsNullOrEmpty(xmlFileName))
            {
                file = Config.GetDataFolderPath(dataType, folderName, xmlFileName);
            }
            else
            {
                // pick the first file in folder if no filename was supplied...
                string folderPath = Config.GetDataFolderPath(dataType, folderName, "");

                string[] files = System.IO.Directory.GetFiles(folderPath);

                objectToSerialize = default(T);

                file = files[0];
            }

            DeserializeObject<T>(file, out objectToSerialize);
        }

        public static void DeserializeObject<T>(string filePath, out T objectToSerialize)
        {
            // objectToSerialize = null;

            // Deserialization
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (TextReader r = new StreamReader(filePath))
            {
                objectToSerialize = (T)s.Deserialize(r);
            }

        }

        private void MoveListToDictionary(List<IGameData> list, Dictionary<string, IGameData> dictionary)
        {
            foreach (IGameData item in list)
            {
                dictionary.Add(item.KeyName, item);
            }

        }
        private static bool DoReplaceOnType(Type t)
        {
            return !(t == typeof(string))
                && t.IsClass  // skip value types
                && !t.IsAbstract // 'Type' only?
                && !(t == typeof(CustomXmlSerializer.XmlProxyData)); // skip other garbage.
        }


        public static void ReplaceEntityTypePlaceholdersOnObjectCollection<T>() where T : IEnumerable
        {

        }

        private static bool IsCollection(object o)
        {
            return typeof(ICollection).IsAssignableFrom(o.GetType())
                || typeof(ICollection<>).IsAssignableFrom(o.GetType());
        }

        /// <summary>
        /// is public to allow reflection call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
     /*   public static void ReplaceEntityTypePlaceholdersOnObject<T>(T obj)
        {
            Type typeOfT = typeof(T);

           
            if (IsCollection(obj)) // typeOfT.IsGenericType) //obj is IEnumerable<S>) //  typeof(T).IsGenericType)
            {
                // if typeOfT is List<CasteType>, then this gives us CasteType: (exception on arrays.)
                // Type typeOfListMember = typeOfT.GetGenericArguments()[0];

                //  MethodInfo method = typeof(GameDataLoader).GetMethod("ReplaceEntityTypePlaceholdersOnObject");
                //   MethodInfo genericMethod = method.MakeGenericMethod(typeOfListMember);
                MethodInfo method = typeof(DataLoader).GetMethod("ReplaceEntityTypePlaceholdersOnObject");

                IDictionary dictionary = obj as IDictionary;
                if (dictionary != null)
                {
                    //Type dictionaryType = dictionary.GetType().GetGenericTypeDefinition();
                    Type[] argTypes = dictionary.Keys.GetType().GetGenericArguments(); // dictionaryType.GetGenericArguments();


                    // new dictionary? can't update while iterating?
                    //Dictionary<EntityType, argTypes[1]> newDictionary = new Dictionary<EntityType, argTypes[1]>();
                    //List<EntityType> keysToReplace = new List<EntityType>();

                    if (argTypes[0] == typeof(EntityType))
                    {
                        // replace all keys:
                        EntityType[] keysToReplace = new EntityType[dictionary.Count];
                        dictionary.Keys.CopyTo(keysToReplace, 0);

                        foreach (EntityType entityType in keysToReplace)
                        {
                            if (string.IsNullOrEmpty(entityType.Name))
                            {
                                object value = dictionary[entityType];
                                dictionary.Remove(entityType);
                                dictionary.Add(GameData.Instance.AllEntityTypes[entityType.KeyName], value); //do the replacement!                          
                            }
                        }
                    }

                    // treat values:
                    foreach (DictionaryEntry de in dictionary)
                    {
                        object kvpKey = de.Key;

                      

                        object kvpValue = de.Value;

                        MethodInfo genericMethod = method.MakeGenericMethod(kvpValue.GetType());
                        genericMethod.Invoke(null, new object[] { kvpValue });
                    }
                }
                else
                {

                    foreach (object collectionMember in (obj as IEnumerable))
                    {
                        if (collectionMember != null)
                        {
                            // call again on each list member:
                            Type collectionMemberType = collectionMember.GetType(); // typeof(collectionMember);

                            // do recursion:
                            // we need to use reflection to call the generic method with the correct type, because the type is not known at compile time:

                            MethodInfo genericMethod = method.MakeGenericMethod(collectionMemberType);
                            genericMethod.Invoke(null, new object[] { collectionMember });
                         
                        }
                    }
                }

                // if typeOfT is List<CasteType>, then this gives us CasteType:
                //   Type typeOfListMember = typeOfT.GetGenericArguments()[0];

                return; // done with the collection.
            }

            FieldInfo[] fieldInfos = typeof(T).GetFields();
            List<FieldInfo> fieldsToCheck = new List<FieldInfo>();
            foreach (FieldInfo field in fieldInfos)
            {
                if (field.FieldType == typeof(EntityType))
                {
                    fieldsToCheck.Add(field);
                }
                else if (DoReplaceOnType(field.FieldType))
                {
                    object o = field.GetValue(obj);
                    if (o != null)
                    {
                        Type fieldType = field.FieldType;

                        // Convert.ChangeType(o, typeof(fieldType));
                        // do recursion:
                        // we need to use reflection to call the generic method witth the correct type, because the type is not known at compile time:
                        MethodInfo method = typeof(DataLoader).GetMethod("ReplaceEntityTypePlaceholdersOnObject");
                        MethodInfo genericMethod = method.MakeGenericMethod(fieldType);
                        genericMethod.Invoke(null, new object[] { o });

                        // ReplaceEntityTypePlaceholdersOnObject(o);
                    }
                }
            }

            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            List<PropertyInfo> propertiesToCheck = new List<PropertyInfo>();
            foreach (PropertyInfo field in propertyInfos)
            {
                if (field.PropertyType == typeof(EntityType))
                {
                    propertiesToCheck.Add(field);
                }
                if (field.PropertyType == typeof(Dictionary<EntityType, int>)) //?!?!? add more?
                {
                    propertiesToCheck.Add(field);
                }
                else if (DoReplaceOnType(field.PropertyType))
                {
                    object o = field.GetValue(obj, null);
                    if (o != null)
                    {
                        Type fieldType = field.PropertyType;

                        // do recursion:
                        MethodInfo method = typeof(DataLoader).GetMethod("ReplaceEntityTypePlaceholdersOnObject");
                        MethodInfo genericMethod = method.MakeGenericMethod(fieldType);
                        genericMethod.Invoke(null, new object[] { o });
                    }
                }
            }

            // loop through fields:
            foreach (FieldInfo field in fieldsToCheck)
            {
                ReplaceEntityTypePlaceholderOnObjectField(field, obj);
            }

            // do the same for properties:
            foreach (PropertyInfo property in propertiesToCheck)
            {
                ReplaceEntityTypePlaceholderOnObjectProperty(property, obj);
            }         

        }*/

        /// <summary>
        /// do a second pass to replace all the dummy type instances with real ones. We identify dummies by Name = "".
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allTypes"></param>
     /*   protected static void ReplaceEntityTypePlaceholders(List<EntityType> allTypes) //Dictionary<string, EntityType> allTypes) //List<EntityType> allTypes) //Dictionary<string, EntityType> allTypes)
        {
            foreach (var item in allTypes) 
            {
                ReplaceEntityTypePlaceholdersOnObject(item);
            }

        }*/

     /*   protected static void ReplaceEntityTypePlaceholders<T>( Dictionary<string, T> allTypes) //List<T> allTypes) //  Dictionary<string, T> allTypes)
        {
            foreach (var item in allTypes)
            {
                ReplaceEntityTypePlaceholdersOnObject(item.Value);
            }

        }*/

     /*   private static void ReplaceEntityTypePlaceholderOnObjectProperty(PropertyInfo property, object obj)
        {
            object o = property.GetValue(obj, null);
            if (o != null)
            {
                EntityType valueToReplace = (EntityType)o;
                if (string.IsNullOrEmpty(valueToReplace.Name))
                {
                    property.SetValue(obj, GameData.Instance.AllEntityTypes[valueToReplace.KeyName], null); //do the replacement!
                }
            }
        }

        private static void ReplaceEntityTypePlaceholderOnObjectField(FieldInfo field, object obj)
        {
            object o = field.GetValue(obj);
            if (o != null)
            {
                EntityType valueToReplace = (EntityType)o;
                if (string.IsNullOrEmpty(valueToReplace.Name))
                {
                    field.SetValue(obj, GameData.Instance.AllEntityTypes[valueToReplace.KeyName]); //do the replacement!
                }
            }
        }

        /// <summary>
        /// do a second pass to replace all the dummy type instances with real ones. We identify dummies by Name = "".
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allTypes"></param>
        private static void ResolveTypePlaceholders<T>(Dictionary<string, T> allTypes) where T : class, IGameData
        {

            // get the fields/properties on the type which are relevant:
            FieldInfo[] fieldInfos = typeof(T).GetFields();
            List<FieldInfo> fieldsToCheck = new List<FieldInfo>();
            foreach (FieldInfo field in fieldInfos)
            {
                if (field.FieldType == typeof(T))
                {
                    fieldsToCheck.Add(field);
                }


               
            }

            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
           

            List<PropertyInfo> propertiesToCheck = new List<PropertyInfo>();
            foreach (PropertyInfo field in propertyInfos)
            {
                if (field.PropertyType == typeof(T))
                {
                    propertiesToCheck.Add(field);
                }
                if (field.PropertyType == typeof(Dictionary<T, int>))
                {
                    propertiesToCheck.Add(field);
                }
            }


            // resolve cyclic references:
            foreach (KeyValuePair<string, T> kvp in allTypes)
            {
                // loop through fields:
                foreach (FieldInfo field in fieldsToCheck)
                {
                    object o = field.GetValue(kvp.Value);
                    if (o != null)
                    {
                        T valueToReplace = o as T;
                        if (valueToReplace != null) // is it an ItemType?
                        {
                            if (string.IsNullOrEmpty(valueToReplace.Name))
                            {
                                field.SetValue(kvp.Value, GameData.Instance.AllEntityTypes[valueToReplace.KeyName]); // we can do this!
                            }
                            continue;
                        }

                       

                       

                    }
                }

                // do the same for properties:
                foreach (PropertyInfo property in propertiesToCheck)
                {
                    object o = property.GetValue(kvp.Value, null);
                    if (o != null)
                    {
                        T valueToReplace = (T)o;
                        if (string.IsNullOrEmpty(valueToReplace.Name))
                        {
                            property.SetValue(kvp.Value, GameData.Instance.AllEntityTypes[valueToReplace.KeyName], null); //do the replacement!
                        }
                    }
                }

            }
        }*/


        /// <summary>
        /// Deprecate this!
        /// 
        /// the full list of type mappings/formatters that we use when serializing.
        /// 
        /// when placeholders are used, a second pass is needed to replace them on deserialization. This must be added in code.
        /// </summary>
        /// <returns></returns>
        public static List<CustomXmlSerializer.XmlTypeMappingBase> GetListOfTypeMappings(bool useEntityTypePlaceholders = false)
        {
            return new List<CustomXmlSerializer.XmlTypeMappingBase>()
            {
                EntityType.GetEntityTypePropertySerializer(useEntityTypePlaceholders),
                new CustomXmlSerializer.XmlTypeMapping<EntityCategory, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllEntityCategories[s]
                },
                new CustomXmlSerializer.XmlTypeMapping<ResourceCategory, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllResourceCategories[s]
                },
                new CustomXmlSerializer.XmlTypeMapping<ResourceType, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllResourceTypes[s]
                },
                new CustomXmlSerializer.XmlTypeMapping<ResourceType[], string[]> ()
                {
                    GetterMethod = SerializeResourceTypeArray,
                    SetterMethod = DeserializeResourceTypeArray
                },
                new CustomXmlSerializer.XmlTypeMapping<AttackType[], string[]> ()
                {
                    GetterMethod = SerializeAttackTypeArray, // SerializeResourceTypeArray,
                    SetterMethod = DeserializeAttackTypeArray
                },
                new CustomXmlSerializer.XmlTypeMapping<BodyType, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllBodyTypes[s]
                },
                new CustomXmlSerializer.XmlTypeMapping<DetectionType, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllDetectionTypes[s]
                },
                new CustomXmlSerializer.XmlTypeMapping<Vector3?, string> ()
                {
                    GetterMethod = t => t == null ? null : PersonType.Vector3ToHexString(t.Value),
                    SetterMethod = s => s == null ? null : new Vector3?(PersonType.HexStringToVector3(s))
                },  
              /*  new CustomXmlSerializer.XmlTypeMapping<float?, string> ()
                {
                    GetterMethod = t => t == null ? null : t.Value.ToString(),
                    SetterMethod = s => string.IsNullOrEmpty(s) ? null : new float?(float.Parse(s))
                },*/
                new CustomXmlSerializer.XmlTypeMapping<Type, string> ()
                {
                    GetterMethod = t => t == null ? null : ConvertTypeToString(t), // not sure which is best... the short or the detailed type names
                    SetterMethod = s => s == null ? null : ConvertStringToType(s)
                  /*  GetterMethod = t => t == null ? null : t.AssemblyQualifiedName,
                    SetterMethod = s => s == null ? null : Type.GetType(s)*/
                },  
                // PARTS:
                new CustomXmlSerializer.XmlTypeMapping<Dictionary<EntityType, int>, KVP<string, int>[]>()  
                {
                    GetterMethod = SerializeEntityTypeIntDictionary,
                    SetterMethod = DeserializeEntityTypeIntDictionary                      
                },                                                                    
                new CustomXmlSerializer.XmlTypeMapping<ProcessType, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllProcessTypes[s]
                },
              /*  new CustomXmlSerializer.XmlTypeMapping<ActionSets, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllActionSets[s]
                },*/
                new CustomXmlSerializer.XmlTypeMapping<SoundData, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllSoundData[s]
                },              
                new CustomXmlSerializer.XmlTypeMapping<FoodNutrientProfile, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllFoodNutrientProfiles[s]
                },
                new CustomXmlSerializer.XmlTypeMapping<FoodNutrientType, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllFoodNutrientTypes[s]
                },
                new CustomXmlSerializer.XmlTypeMapping<ProcessToolSet, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllProcessToolSets[s]
                },
                new CustomXmlSerializer.XmlTypeMapping<PresentationType, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllPresentationTypes[s]
                },
                new CustomXmlSerializer.XmlTypeMapping<EntityType[], string[]>()  
                {
                    GetterMethod = SerializeEntityTypeArray, // NB! no placeholders implemented!
                    SetterMethod = DeserializeEntityTypeArray                      
                },
                new CustomXmlSerializer.XmlTypeMapping<BodyPartType[], string[]>()
                    {                       
                        GetterMethod = SerializeBodyPartTypes,                           
                        SetterMethod = DeserializeBodyPartTypes                                           
                    },
                new CustomXmlSerializer.XmlTypeMapping<SkillType, string> ()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                    SetterMethod = s => s == null ? null : GameData.Instance.AllSkillTypes[s]
                },
                   GetMaterialInputSerializer(useEntityTypePlaceholders)     

            };
        }

        /// <summary>
        /// converts a dictionary to an array when serializing.
        /// </summary>
        /// <param name="useEntityPlaceholders"></param>
        /// <returns></returns>
        public static CustomXmlSerializer.XmlTypeMapping<Dictionary<EntityType, Input>, Input[]> GetMaterialInputSerializer(bool useEntityPlaceholders)
        {
            if (useEntityPlaceholders)
            {
                return new CustomXmlSerializer.XmlTypeMapping<Dictionary<EntityType, Input>, Input[]>()
                {
                    GetterMethod = t =>
                    {
                        if (t == null) return null;
                        var d = new Input[t.Count];
                        int index = 0;
                        foreach (KeyValuePair<EntityType, Input> kvp in t)
                        {
                            d[index] = kvp.Value;
                            index++;
                        }
                        return d;
                    },

                    SetterMethod = s =>
                    {
                        if (s == null) return null;
                        var d = new Dictionary<EntityType, Input>();
                        foreach (var el in s)
                        {

                            d[new EntityType(el.EntityType.KeyName)] = el; // PLACEHOLDER!
                        }
                        return d;
                    }
                };
            }
            else
            {
                return new CustomXmlSerializer.XmlTypeMapping<Dictionary<EntityType, Input>, Input[]>()
                {
                    GetterMethod = t =>
                    {
                        if (t == null) return null;
                        var d = new Input[t.Count];
                        int index = 0;
                        foreach (KeyValuePair<EntityType, Input> kvp in t)
                        {
                            d[index] = kvp.Value;
                            index++;
                        }
                        return d;
                    },

                    SetterMethod = s =>
                    {
                        if (s == null) return null;
                        var d = new Dictionary<EntityType, Input>();
                        foreach (var el in s)
                        {
                            d[el.EntityType] = el;
                        }
                        return d;
                    }
                };


            }
        }

        /// <summary>
        /// XXXX HACK
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string ConvertTypeToString(Type type)
        {
            if (type == typeof(AnimModifier))
            {
                return "AnimModifier";
            }
            else if (type == typeof(StateModifier))
            {
                return "SpriteModifier";
            }

            return null;
        }

        private static Type ConvertStringToType(string s)
        {

            if (s == "AnimModifier")
            {
                return typeof(AnimModifier);
            }
            else if (s == "SpriteModifier")
            {
                return typeof(StateModifier);
            }

            return null;
        }

        public static KVP<string, int>[] SerializeEntityTypeIntDictionary(Dictionary<EntityType, int> t)
        {
            if (t == null) return null;
            var d = new KVP<string, int>[t.Count];
            int index = 0;
            foreach (KeyValuePair<EntityType, int> kvp in t)
            {
                d[index] = new KVP<string, int>(kvp.Key.KeyName, kvp.Value);
                index++;
            }
            return d;
        }

        public static Dictionary<EntityType, int> DeserializeEntityTypeIntDictionary(KVP<string, int>[] s)
        {
            if (s == null) return null;
            var d = new Dictionary<EntityType, int>();
            foreach (var el in s)
            { // when deserializing: watch out for cycles, or EntityType instances not created yet! 
                //- create a placeholder EntityType with empty Name to identify and replace in a second pass.
                d[new EntityType() { KeyName = el.Key }] = el.Value;
            }
            return d;
        }

        public static string[] SerializeEntityTypeArray(EntityType[] t)
        {
            if (t == null) return null;
            var d = new string[t.Length];
            int index = 0;
            foreach (EntityType e in t)
            {
                d[index] = e.KeyName;
                index++;
            }
            return d;
        }

        public static EntityType[] DeserializeEntityTypeArray(string[] s)
        {
            if (s == null) return null;
            var d = new EntityType[s.Length];
            int index = 0;
            foreach (var el in s)
            {
                d[index] = GameData.Instance.AllEntityTypes[el];
                index++;
            }
            return d;
        }


        public static string[] SerializeResourceTypeArray(ResourceType[] t)
        {
            if (t == null) return null;
            var d = new string[t.Length];
            int index = 0;
            foreach (ResourceType e in t)
            {
                d[index] = e.KeyName;
                index++;
            }
            return d;
        }

        public static ResourceType[] DeserializeResourceTypeArray(string[] s)
        {
            if (s == null) return null;
            var d = new ResourceType[s.Length];
            int index = 0;
            foreach (var el in s)
            {
                d[index] = GameData.Instance.AllResourceTypes[el];
                index++;
            }
            return d;
        }


        public static string[] SerializeAttackTypeArray(AttackType[] t)
        {
            if (t == null) return null;
            var d = new string[t.Length];
            int index = 0;
            foreach (AttackType e in t)
            {
                d[index] = e.KeyName;
                index++;
            }
            return d;
        }

        public static AttackType[] DeserializeAttackTypeArray(string[] s)
        {
            if (s == null) return null;
            var d = new AttackType[s.Length];
            int index = 0;
            foreach (var el in s)
            {
                d[index] = GameData.Instance.AllAttackTypes[el];
                index++;
            }
            return d;
        }


        public static string[] SerializeBodyPartTypes(BodyPartType[] bodyPartTypes)
        {
            if (bodyPartTypes == null) return null;
            var names = new string[bodyPartTypes.Length];
            int index = 0;
            foreach (BodyPartType bodyPartType in bodyPartTypes)
            {
                // encode the body key name as well as the body part name when serializing:
                names[index] = bodyPartType.BodyKeyName + "_" + bodyPartType.Name;

                index++;
            }

            return names;

        }

        public static BodyPartType[] DeserializeBodyPartTypes(string[] names)
        {
            if (names == null) return null;

            // can contain null elements???
            /* int count = 0;
             foreach (string name in names)
             {
                 if (name != null)
                     count++;
             }*/

            var bodyPartTypes = new BodyPartType[names.Length];
            int index = 0;
            foreach (string name in names)
            {

                int underscoreIndex = name.IndexOf("_");
                string bodyKeyName = name.Substring(0, underscoreIndex);
                string bodyPartName = name.Substring(underscoreIndex + 1);
                BodyType bodyType = GameData.Instance.AllBodyTypes[bodyKeyName];

                bodyPartTypes[index] = bodyType.FindBodyPart(bodyPartName);

                index++;
            }
            return bodyPartTypes;


        }
 
        #endregion

        #region Data type init methods
        
        protected virtual List<SkillCategory> InitSkillCategories()
        {
            return null;           
        }

        protected virtual List<SoundData> InitSounds()
        {
            return null;
        }

        #region single objects

        protected virtual AI.Constants.AIConstants InitAIConstants()
        {
            return null;
        }


        protected virtual UWGame.SimSide.Constants InitGameConstants()
        {
            return null;
        }

        protected virtual GUIConstants InitGUIConstants()
        {
            return null;
        }


        protected virtual CustomDataPresentation InitStatusIconPresentation()
        {
            return null;
        }

        protected virtual CustomDataPresentation InitActivityPresentation()
        {
            return null;
        }        

        protected virtual CustomDataPresentation InitSidePanelPresentation()
        {
            return null;
        }


        protected virtual CustomDataPresentation InitOtherSiteSidePanelPresentation()
        {
            return null;
        }

       
       

        #endregion


        /*
          protected override void InitEventHooks()
        {
            base.InitEventHooks();

            EventHooksLoader.InitAgentActionHooks();
            EventHooksLoader.InitAttackTypeHooks();
            EventHooksLoader.InitProcessTypeHooks();

            DetectionEventHooksLoader.InitDetectEntityTypeHooks();
            DetectionEventHooksLoader.InitDetectResourceTypeHooks();
        }*/



        #region hooks


        protected virtual List<EntityEventHook> InitEntityEventHooks()
        {
            return EventHooksLoader.InitEntityEventHooks();
        }

        protected virtual List<AgentActionHook> InitAgentActionHooks()
        {
            return EventHooksLoader.InitAgentActionHooks();
        }

      
        protected virtual List<AttackTypeActionHook> InitAttackTypeEventHooks()
        {
            return EventHooksLoader.InitAttackTypeHooks();
        }

        protected virtual List<ProcessTypeActionHook> InitProcessTypeEventHooks()
        {
            return EventHooksLoader.InitProcessTypeHooks();
        }

        protected virtual List<EffectTypeActionHook> InitEffectTypeEventHooks()
        {
            return EventHooksLoader.InitEffectTypeHooks();
        }
        

        protected virtual List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
        {
            return null;
        }

        protected virtual List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
        {
            return null;
        }

      

        #endregion


        protected virtual List<EntityTypePolledEvent> InitEntityPolledEvents()
        {
            return EntityPolledEventsLoader.Init();
        }


        protected virtual List<EventActionType> InitEventActionTypes()
        {
            return null;
        }

        protected virtual List<EntityData> InitEntityData()
        {
            return null;
        }

        protected virtual List<SiteData> InitSiteData()
        {
            return null;
        }

        protected virtual List<AllegianceData> InitAllegianceData()
        {
            return null;
        }

        protected virtual List<ExpeditionData> InitExpeditionData()
        {
            return null;
        }

        protected virtual List<SiteTemplate> InitSiteTemplates()
        {
            return null;
        }

        protected virtual List<AllegianceTemplate> InitAllegianceTemplates()
        {
            return null;
        }

        protected virtual List<TradeProfile> InitTradeProfiles()
        {
            return null;
        }

        protected virtual List<PricesProfile> InitPricesProfiles()
        {
            return null;
        }

        protected virtual List<TradeGroup> InitTradeGroups()
        {
            return null;
        }

        protected virtual List<OfferDemandProfile> InitOfferDemandProfiles()
        {
            return null;
        }
        

        protected virtual List<VehiclesProfile> InitVehicleProfiles()
        {
            return null;
        }

        protected virtual List<StructuresProfile> InitStructureProfiles()
        {
            return null;
        }

        protected virtual List<ActionSets> InitActionSets()
        {
            return null;
        }

       

        protected virtual List<StancesType> InitStancesTypes()
        {
            return null;
        }

        protected virtual List<StanceType> InitStanceTypes()
        {
            return null;
        }

        protected virtual List<BodyLayerType> InitBodyLayerTypes()
        {
            return null;
        }

        protected virtual List<BodyType> InitBodyTypes()
        {
            return null;
        }

        protected virtual List<Vegetation.LowVegetationType> InitLowVegetationTypes()
        {
            return null;
        }

        protected virtual List<SoilComponentType> InitSoilComponentTypes()
        {
            return null;
        }

      /*  protected virtual List<StructureCategory> InitStructureCategories()
        {
            return null;
        }*/

        protected virtual List<FoodNutrientProfile> InitFoodNutrientProfiles()
        {
            return null;
        }

        protected virtual List<StorageCondition> InitStorageConditions()
        {
            return null;
        }

        protected virtual List<DegradeType> InitDegradeTypes()
        {
            return null;
        }

        protected virtual List<EntityCategory> InitEntityCategories()
        {
            return null;
        }

        protected virtual List<DefaultStorageSettings> InitDefaultStorageSettings()
        {
            return null;
        }        

        protected virtual List<SkillType> InitSkillTypes()
        {
            return null;
        }

        protected virtual List<ProfessionType> InitProfessionTypes()
        {
            return null;
        }
        

        protected virtual List<FoodNutrientType> InitNutrientTypes()
        {
            return null;
        }

        protected virtual List<HelpTopic> InitTutorialTopics()
        {
            return null;
        }

        protected virtual List<HelpTopic> InitHelpTopics()
        {
            return null;
        }

        protected virtual List<ResourceCategory> InitResourceCategories()
        {
            return null;
        }

       
        protected virtual List<DamageType> InitDamageTypes()
        {
            return null;
        }


        protected virtual List<AttackType> InitAttackTypes()
        {
            return null;
        }

        protected virtual List<ResourceType> InitResourceTypes()
        {
            return null;
        }

        protected virtual List<TriggerType> InitTriggerTypes()
        {
            return null;
        }

        protected virtual List<PersonalityType> InitPersonalityTypes()
        {
            return null;
        }

        protected virtual List<RepairProfile> InitRepairProfiles() { return null; }

        protected virtual List<TraitTemplate> InitTraitTemplates() { return null; }
        protected virtual List<CultureTemplate> InitCultureTemplates() { return null; }

        protected virtual List<TierType> InitTiers() { return null; }
        protected virtual List<TierArea> InitTierAreas() { return null; }

        protected virtual List<UpgradeCategory> InitUpgradeCategories() { return null; }

        protected virtual List<UpgradeProfile> InitUpgradeProfiles() { return null; }
       
        protected virtual List<BioOrderType> InitBioOrderTypes()
        {
            return null;
        }

        protected virtual List<AllegianceEventType> InitAllegianceEvents() { return null; }

        protected virtual List<RenderableType> InitAttachableRenderableTypes() { return null; }

        protected virtual List<EntityType> InitEntityTypes() { return null; }

        protected virtual List<EntityTypeDescription> InitEntityTypeDescriptions() { return null; }

        protected virtual List<FilterSettingType> InitFilterSettingTypes() { return null; }

        protected virtual List<JobType> InitJobTypes() { return null; }

       // protected virtual List<Hint> InitHints() { return null; }
       
        

        protected virtual List<PresentationType> InitPresentationTypes(){ return null; }

        protected virtual List<PresentationTypeCategory> InitPresentationTypeCategories() { return null; }

        protected virtual List<PolledEventType> InitGlobalConditionalEvents() { return null; }

        protected virtual List<ProcessToolSet> InitProcessToolSets() { return null; }

        protected virtual List<ProcessType> InitProcessTypes() { return null; }

        protected virtual List<EffectType> InitEffectTypes() { return null; }        
 
        protected virtual List<EffectProfileType> InitEffectProfileTypes() { return null; }         
        
        protected virtual List<DetectionType> InitDetectionTypes() { return null; }

        protected virtual List<ParticleSystemType> InitParticleSystems() { return null; }

        protected virtual List<SubstanceType> InitSubstanceTypes() { return null; }

        protected virtual List<IconInfo> InitIconInfo() { return null; }


        #endregion
    }
}
