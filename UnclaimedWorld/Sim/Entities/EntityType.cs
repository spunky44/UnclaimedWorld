using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Xclna.Xna.Animation;
using System.Xml.Serialization;
using UWGame.SimSide.Items;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Buildings;
using SpriteSheetRuntime;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Processes;
using UWGame.ClientSide.Renderables;
using UWGame;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities.Locomotors; 
using UWGame.ClientSide;
using UWGame.Client.Audio;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Communication;
using UWGame.SimSide.InGameEvents;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Allegiances;

namespace UWGame.SimSide.Entities
{
    
    public class EntityType : IXmlSerializable, IGameData, IHasCategory<EntityCategory>, IDetectableType
    {
     
        /// <summary>
        /// used to override the default fog of war behaviour. 
        /// for terrain and trees, they are always rendered by default. If this is set to false, they will disappear when in the fog of war.
        /// </summary>
        public bool? IsNeverInFogOfWar;

        /// <summary>
        /// used to override the default memory behaviour.
        /// terrain and trees normally are not represented as memory facts. If this is set true, they will appear as memory facts
        /// </summary>
        public bool? UsesMemory;

        /// <summary>
        /// used to override the default detection behaviour.
        /// Some terrain types which are hidden in the FOW (like sound emitters) should be detected immediately.
        /// trees and items by default do not require detection.
        /// 
        /// If the entity is in the area where ExploreShroud is invoked, then it will always have been detected. 
        /// If in FOW it then exists as a emoryfact. When a character then approaches it, 
        /// and it requires RollToDetect (like for terrain types) if he fails his RollsToDetect, the memory fact will become deprecated.
        /// This will cause any jobs on that entity to be cancelled also. 
        /// This happened with missing crew member entity and Rescure crew member job.
        /// Fix this by overriding requires RollToDetect by setting this flag to true. That way, the entity will not disappear when 
        /// approached.
        /// </summary>
        public bool? RequiresRollToDetect;


        public string KeyName { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }
        public string Name { get; set; }
        public string PluralName { get; set; }

        public string FormalName;

       
        public string CategoryKey { get; set; }

        [XmlIgnore]
        public EntityCategory Category { get; set; }


        public string ThumbnailBig;

        /// <summary>
        /// if false, Name will be displayed in the UI instead of the usual EntityType.Name
        /// </summary>
        public bool UseTypeNameForDisplay = true;

        /// <summary>
        /// used in entity type info
        /// </summary>
        public string ThumbnailSmall;


        public string[] Tags;


        /// <summary>
        /// can be filled to display an icon on inventory lists if no default billboard is specified, like for the spear
        /// </summary>
        public string Icon;


        public string WorldMapIcon;

       
        public string Description = "";

        /// <summary>
        ///  
        /// This description should not be longer then 89 characters, if it contains longer words then is should be less then 89 character because of the line breaks
        /// </summary>
        public string SummaryDescription;

        public SerializableDictionary<string, PropertyResult> CustomFields;

        public PersonType Person;

        public BiologicalType BiologicalType;

        
        //public VehicleType VehicleType;

        public IntelligenceType IntelligenceType;

    //    public ResidenceType ResidenceType;

        public TerrainFeatureType TerrainType;

        public HeatingType HeatingType;

        public Trees.TreeType TreeType;

        public BodyType BodyType;

        public RockType RockType;

        public ItemType ItemType;

        public ThreatType ThreatType;

        public GatheringSiteType GatheringSiteType;

        public ContainerType ContainerType;

        public NonLivingType NonLivingType;

        public SubstancesType SubstancesType;

        public CommunicatorType CommunicatorType;

        /// <summary>
        /// items and structures can both be tools
        /// </summary>
        public ToolType ToolType;

        /// <summary>
        /// Only intelligent entities require detection. 
        /// This tag is used by DetectionType.
        /// </summary>
        public string DetectionTag;

        public string[] CanEatDesignerTags;

        public Upgrader Upgrader;

       

        /// <summary>
        /// triggers only become active after the entity is completed!
        /// </summary>
        public TriggerType[] Triggers;

        /// <summary>
        /// also in ProcessType
        /// </summary>
        public TierOrArea TierOrArea;
      //  public string TierArea;

        [XmlIgnore]
        public TierOrAreaType TierOrAreaType;
      //  public TierArea TierAreaType;


        [XmlIgnore]
        public Dictionary<Scope, List<PolledEventType>> PolledEvents = new Dictionary<Scope, List<PolledEventType>>();
        

        [XmlIgnore]
        public Dictionary<EntityEventHooks, List<ActionSets>> EventActions = new Dictionary<EntityEventHooks, List<ActionSets>>();


        /// <summary>
        /// any actions defined here will be available to the player in the entity menu
        /// NOTE: new unique process types will be created, with new key names and a correct ActingOnType property.
        /// 
        ///   
        /// separate availability of actions into PHYSICAL (for all, all the time) and LOCKS (per allegiance)
        /// PHYSICAL availability can be toggled automatically with Anchor.
        /// 
        /// Grow crop: this is a lock (not physical. does not affect other allegiances.) - Is invoked by a process - EnablesSpecialActionProcessTypes in ProcessProductionFinished
        /// Use fertilizer: also a lock.
        /// 
        /// Locks: stored in SharedKnowledge for the allegiance, no syncing required. Are set on IKnownEntityData.
        /// 
        /// Physical: are only set on the entity. Require syncing:
        /// Use smoke bomb. Enable attack vermin. Build farm plot. Build fish trap. Build turnip hut.
        /// 
        /// Both types have a corresponding job, so the UI will disable the button from the job, disabling the action is not needed!
        /// </summary>
        public Pair<string, bool>[] SharedSpecialActions;

        /// <summary>
        /// use this for non-physical actions!
        /// </summary>
        public Pair<string, bool>[] SpecialActionLocks;
       

        public bool? IsSelectable;

        public bool IsFlyer = false;

        /// <summary>
        /// The direction to scan for an accesspoint. The default is South (0f, 1f, 0f). 
        /// Overridden by Doors.
        /// </summary>
        public Vector3? AccessPointDirection;
       
        /// <summary>
        /// uniquely generated processes for each entity type. Only compare them using OriginalProcess!
        ///      
        /// </summary>      
        [XmlIgnore]
        public List<ProcessType> SharedSpecialActionTypes
        {
            get;
            private set;
        }


        [XmlIgnore]
        public List<ProcessType> SpecialActionLockTypes
        {
            get;
            private set;
        }

        public LocomotorType LocomotorType
        {
            get;
            set;
        }

        public Dictionary<EntityType, int> Parts
        {
            get
            {
                if (NonLivingType != null)
                {
                    return NonLivingType.Parts;
                }

                return null;
            }
        }

        
        public RenderableType RenderableType;

        public RenderableType EditorRenderableType;


        public StructureType StructureType;
        public SensorType SensorType;
        public DirectionalLayoutType DirectionalLayoutType;

        public TerminalType TerminalType;
       
        /// <summary>
        /// deprecate this!!!
        /// </summary>
        public PointLayoutType PointLayoutType;
               

        /// <summary>
        /// passive collision info.
        /// is overridden by geolayout!!!
        /// </summary>
        public CollidableType CollidableType;

      //  public ThreatCategory ThreatCategory; 



        /// <summary>
        /// used for selecting geolayouts for non-animating entities
        /// </summary>
        public SimStateInfo[] SimStateConditions;

        public SimStateInfo DefaultSimState;


        /// <summary>
        /// TODO: delete this, look up in Entity.CurrentSimState instead
        /// 
        /// this should be the default geolayout to use
        /// </summary>
      //  public GeometryLayoutType GeometryLayoutType;



        public enum ShowMarkerWindowMode { OnlyWhenSelected, Always, ByStatus }

        public ShowMarkerWindowMode? ShowMarkerWindowSetting; // = ShowMarkerWindowMode.OnlyWhenSelected;
         

        
        public EntityType() 
        {
           
        }

      
        public EntityType(string keyName) //, string modelName)
        {
            this.KeyName = keyName;

        }

        public EntityType ShallowCopy()
        {
            return (EntityType)this.MemberwiseClone();
        }


        public void PreInitValidate(ref List<string> listOfErrors) 
        {
           
            if (ToolType != null)
            {
                ToolType.PreInitValidate(ref listOfErrors);

                if (StructureType != null && ToolType.IsPseudoTool == false && ToolType.ToolHandling != ToolHandlingType.Stationary)
                {
                    CreateValidationError(ref listOfErrors, "Structures can only be stationary tools.");       
                }

                if (Upgrader != null && ToolType.IsPseudoTool == false && ToolType.ToolHandling != ToolHandlingType.Stationary)
                {
                    CreateValidationError(ref listOfErrors, "Upgrades can only be stationary tools.");
                }
            }

            if (CommunicatorType != null && SensorType == null)
            {
                CreateValidationError(ref listOfErrors, "Communicators require a SensorType to be defined also. They must not be in FOW.");       
            }


            if (ContainerType != null)
            {
                TerminalContainerType terminal = ContainerType as TerminalContainerType;
                if (terminal != null && terminal.OfferedForTradeStorageType != null
                    && SensorType == null)
                {
                    CreateValidationError(ref listOfErrors, "Buy/Sell Terminals require a SensorType to be defined also. They must not be in FOW.");
                }
            }

            if (SensorType != null && IntelligenceType == null)
            {
                CreateValidationError(ref listOfErrors, "SensorType without IntelligenceType is not supported.");
            }
        
        }

        public void PostInitValidate(ref List<string> listOfErrors) 
        {
            if (RenderableType != null) // RenderAsModelType != null && RenderAsBillboardType != null)
            {
                RenderableType.PostInitValidate(ref listOfErrors);
            }

            if (EditorRenderableType != null) // RenderAsModelType != null && RenderAsBillboardType != null)
            {
                EditorRenderableType.PostInitValidate(ref listOfErrors);
            }


            int noOfLayouts = (DirectionalLayoutType != null ? 1 : 0) + (PointLayoutType != null ? 1 : 0);

            //if (EdgeLayoutType != null && TileLayoutType != null)
            if (noOfLayouts > 1)
            {
                CreateValidationError(ref listOfErrors, "Only one of the EdgeLayout and PointLayout may be specified.");
            }

           
            if (BiologicalType != null)
            {
                BiologicalType.Validate(ref listOfErrors);
               
            }

            if (ItemType != null)
            {
                ItemType.Validate(ref listOfErrors);
            }
          
            if (SensorType != null)
            {
                SensorType.PostInitValidate(ref listOfErrors);
            }


            if (ContainerType != null)
            {
                ContainerType.PostInitValidate(this, ref listOfErrors);
            }

            ValidateRequiredValue(ref listOfErrors, "Name", Name != null);
        }


        public void PostLoadContentValidate(ref List<string> listOfErrors)
        {
           // Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle(entityType.ThumbnailSmall);

            Rectangle? rect;
            if (!string.IsNullOrEmpty(ThumbnailSmall)
                && The.InGameUI != null  // Decouple..?
                && !The.InGameUI.gui.GUISpriteSheet.TryGetSourceRectangle(ThumbnailSmall, out rect))
            {
                EntityType.CreateValidationError(ref listOfErrors, "ThumbnailSmall asset " + ThumbnailSmall + " not found.");
                
            }

            if (RenderableType != null) 
            {
                RenderableType.PostLoadContentValidate(ref listOfErrors, this);
            }

            if (EditorRenderableType != null)
            {
                EditorRenderableType.PostLoadContentValidate(ref listOfErrors, this);
            }

        }

        public void PostDataCompleteInitialize()
        {

            #region Handle item tags. This must be done after all data has loaded. Why not all data lookups too?
            // uses separate collections to avoid tag name collisions... but what if the designer gets confused..?

            BaseDataLoader.AddToTagCollection(this, this.Tags, GameData.Instance.GeneralTags);

           // BaseDataLoader.AddToTagCollection(this, this.UpgradeTag, GameData.Instance.UpgradesByTag);
           

            if (this.ToolType != null)
            {
                BaseDataLoader.AddToTagCollection(this, this.ToolType.ToolTag, GameData.Instance.ToolsByTag);
            }

            if (this.ItemType != null)
            {
                ItemType.PostDataCompleteInitialize();

                if (this.ItemType.FuelType != null)
                {
                    BaseDataLoader.AddToTagCollection(this, this.ItemType.FuelType.FuelTags, GameData.Instance.FuelByTag);
                }

                if (this.ItemType.AmmunitionType != null)
                {
                    BaseDataLoader.AddToTagCollection(this, this.ItemType.AmmunitionType.AmmoTags, GameData.Instance.AmmoByTag);
                }

                if (this.ItemType.FoodType != null)
                {
                    BaseDataLoader.AddToTagCollection(this, this.ItemType.FoodType.FoodTags, GameData.Instance.FoodByTag);
                }
            }

            if (NonLivingType != null)
            {
                NonLivingType.PostDataCompleteInitialize(this);
            }

            if (Upgrader != null)
            {
                Upgrader.PostDataCompleteInitialize(this);
            }

            if (this.ContainerType != null)
            {
                BaseDataLoader.AddToTagCollection(this, this.ContainerType.StorageTags, GameData.Instance.ContainersByTag); // NEW - we should use this instead of the bits/ints...


                if (this.ContainerType.CanTransactWithTags != null)
                {
                    foreach (var tag in this.ContainerType.CanTransactWithTags)
                    {
                        if (!GameData.Instance.ContainerTags.ContainsKey(tag))
                        {
                            // save all tags with a number
                            GameData.Instance.ContainerTags.Add(tag, GameData.Instance.ContainerTags.Count);
                        }

                    }
                }
                //and just in case a tag was defined for CanBeContained that was not included in canTransactWith
                //then include these also
                if (this.ContainerType.CanBeEnteredByTags != null)
                {
                    foreach (var tag in this.ContainerType.CanBeEnteredByTags)
                    {
                        if (!GameData.Instance.ContainerTags.ContainsKey(tag))
                        {
                            // save all tags with a number
                            GameData.Instance.ContainerTags.Add(tag, GameData.Instance.ContainerTags.Count);
                        }

                    }
                }

                ContainerType.PostDataCompleteInitialize(this);


            }

    #endregion

            if (TierOrArea != null)
            {
                TierOrAreaType = new TierOrAreaType(TierOrArea);
               // TierAreaType = GameData.Instance.AllTierAreas[TierArea];               
            }                
                      

            if (BiologicalType != null)
            {
                BiologicalType.PostDataCompleteInitialize();
            }

            InitSpecialActions(SharedSpecialActions);
            InitSpecialActions(SpecialActionLocks);


          /*  if (BiologicalType != null && IntelligenceType != null)
            {
                GameData.Instance.AllBiologicals.Add(this);
            }*/


            if (StructureType != null)
            {
                GameData.Instance.AllStructureTypes.Add(KeyName, this);
            }

            if (TerrainType != null)
            {
                GameData.Instance.AllTerrainFeatureTypes.Add(KeyName, this);
            }

            // new....
            if (TreeType != null)
            {
                GameData.Instance.AllTreeTypes.Add(KeyName, this);
            }

            if (ItemType != null)
            {
                GameData.Instance.AllItemTypes.Add(KeyName, this);
            }

            if (BiologicalType != null)
            {
                GameData.Instance.AllCreatureTypes.Add(KeyName, this);

                if (BiologicalType.IsVermin)
                {
                    GameData.Instance.AllVerminTypes.Add(KeyName, this);
                }
            }

            if (CategoryKey != null)
            {
                Category = GameData.Instance.AllEntityCategories[CategoryKey];
            }
        }

        private static void InitSpecialActions(Pair<string, bool>[] specialActions)
        {
            if (specialActions != null)
            {
                foreach (var item in specialActions)
                {
                    ProcessType process;
                    if (GameData.Instance.AllProcessTypes.TryGetValue(item.First, out process))
                    {
                        process.IsOriginalSpecialAction = true;  // set a mark so this process won't appear in the production graph...
                    }
                }
            }
        }

        public bool CanBeHuntedBy(Allegiance allegiance)
        {
            if (this != allegiance.RepresentativeEntityType
                && allegiance.RepresentativeEntityType.IntelligenceType.HasServants == null || !allegiance.RepresentativeEntityType.IntelligenceType.HasServants.Contains(this)
                && BiologicalType != null
                && IntelligenceType != null)
            {
                return true;
            }

            return false;
        }

        public void MarkAnchorStructures()
        {
            if (StructureType != null)
            {
                StructureType.MarkAnchorStructures(this);
            }
        }

        public bool IsInOriginalBlueprint(EntityType entityTypeToCheck)
        {
            // examine the part types and see if there is a matching type
            foreach (var item in Parts)
            {
                if (entityTypeToCheck == item.Key)
                {
                    return true;
                }
            }
            return false;
        }

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (NonLivingType != null)
            {
                NonLivingType.PreDataCompleteValidate(ref listOfErrors);
            }

            if (Upgrader != null)
            {
                Upgrader.PreDataCompleteValidate(ref listOfErrors);
            }
        }

        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
           /* if (Upgrader != null)
            {
                Upgrader.PostDataCompleteValidate(this, ref listOfErrors);
            }*/
        }

        public EntityType.ShowMarkerWindowMode GetShowMarkerWindowMode()
        {
            if (ShowMarkerWindowSetting.HasValue)
            {
                return ShowMarkerWindowSetting.Value;
            }
            else if (ItemType != null || StructureType != null // items - and structures! show accessible status by default
                || (BiologicalType != null && BiologicalType.IsTerritorial) || (IntelligenceType != null && IntelligenceType.IsPredator)) // critters need to show the threat job icon
            {
                return ShowMarkerWindowMode.ByStatus; 
            }
            else
            {
                return ShowMarkerWindowMode.OnlyWhenSelected; // default
            }
        }

        /// <summary>
        /// returns true if, for the given sets of processes, the item can be consumed directly or converted into an item that can be consumed
        /// </summary>
        /// <param name="ExtractionProcesses"></param>
        /// <param name="consumeProcesses"></param>
        /// <returns></returns>
        public bool IsEatable(Dictionary<EntityType, HashSet<ProcessType>> extractionResultsInConsumable,
                             Dictionary<EntityType, ProcessType> consumeProcesses)
        {
            return consumeProcesses.ContainsKey(this)
                || extractionResultsInConsumable.ContainsKey(this);
        }

      

        public bool CanBeSalvagedDirectly()
        {
            if (NonLivingType != null
                && NonLivingType.SalvageProcessType != null
                && Upgrader == null) // should be salvaged by using the upgrade panel
            {
                return true;
            }

            return false;

        }

        
        public bool CanBeUpgradedBy(EntityType upgrader)
        {
            if (ContainerType != null)
            {
                var categories = ContainerType.GetUpgradeOptions();

                foreach (var item in upgrader.Upgrader.UpgradeCategoryFinal)
                {
                    if (categories.Contains(item))
                    {
                        return true;
                    }
                }
                

               /* if (categories.Contains(upgrader.Upgrader.UpgradeCategoryFinal))
                {
                    return true;
                }*/
            }

            return false;
        }


        public bool CanBeMounted(Entity agent)
        {
            if (ItemType != null)
            {
                if (ItemType.WeaponType != null && agent.EntityType.IntelligenceType.CanUseWeapons == true)
                {
                    return true;
                }
                else if (ToolType != null 
                    && ToolType.ToolHandling == ToolHandlingType.HandTool // .IsHandTool == true 
                    && agent.EntityType.IntelligenceType.CanMountTools == true)
                {
                    return true;
                }
            }

            return false;
        }

        /*
        public bool CausesCollisions()
        {
          
            if (GeometryLayoutType != null && GeometryLayoutType.CausesCollisions == true) //CollidableType != null && CollidableType.CausesCollisions == true)
            {
                // can be specified on the type level
                return true;
            }
            else return TerrainType == null && TreeType == null && StructureType == null;  // the default case
        }
        */

        public bool MemoryFactIsDeprecatedInstantly()
        {
            if (StructureType != null)
            {
                return true;
            }

            return false;
        }


        public bool RenderWithOverlayWhenMemoryFact()
        {
            if (StructureType == null)
            {
                return true;
            }

            return false;
        }

      
       
        public static void CreateValidationError(ref List<string> listOfErrors, string errorMessage)
        {
            Common.AddToList(ref listOfErrors, errorMessage);           
        }

        public static bool ValidateGameDataTypeExists<T>(ref List<string> listOfErrors, string key, GameDataCollection<T> collection) where T : IGameData
        {
            T dataType;
            return ValidateGameDataTypeExists(ref listOfErrors, key, collection, out dataType);
        }

        public static bool ValidateGameDataTypeExists<T>(ref List<string> listOfErrors, string key, GameDataCollection<T> collection, out T dataType) where T: IGameData
        {
            if (!collection.TryGetValue(key, out dataType))
            {
                CreateValidationError(ref listOfErrors, typeof(T).Name + " key value '" + key + "' not found.");

                return false;
            }

            return true;
        }


        public static void ValidateEntityTypeKeyExists(ref List<string> listOfErrors, string key)
        {
            if (!GameData.Instance.AllEntityTypes.ContainsKey(key))
            {
                CreateValidationError(ref listOfErrors, "Entity type: " + key + " not found.");
            }

        }


        public static void ValidateRequiredValue(ref List<string> listOfErrors, string fieldName, bool hasValue)
        {
            if (hasValue == false)
            {
                Common.AddToList(ref listOfErrors, fieldName + " is a required value.");                 
            }
        }

        public void Initialize() //Dictionary<string, ModelData> AllModels)
        {
           

            if (!string.IsNullOrEmpty(DetectionTag))
            {
                if (DetectionTag == "inDeeperWaterFishingSpot")
                {

                }

                BaseDataLoader.AddToTagCollection(this,
                    DetectionTag, GameData.Instance.DetectableTypeByTag); // EntityTypeDetectionByTag);
            }


            if (Person != null)
            {
                Person.Initialize();               
            }

            if (BiologicalType != null)
            {
                BiologicalType.Initialize(this);               
            }

            if (RenderableType != null) 
            {
                RenderableType.Initialize();                
            }

            if (EditorRenderableType != null) 
            {
                EditorRenderableType.Initialize();
            }

           // InitRenderableTypeMode();

           /* if (RenderableType == null && EditorRenderableType == null) //RenderableTypeMode == null)
            {
                RenderableType = new RenderableType(); // necessary. all entity types have a Renderable, and a RenderableType
               // InitRenderableTypeMode();
            }*/


            if (StructureType != null)
            {
                StructureType.Initialize(this);
               

            }

            if (ContainerType != null)
            {
                ContainerType.Initialize();


            }

            if (TreeType != null)
            {
                TreeType.Initialize();

            }
                        
            
            /*
            if (BodyType != null)
            {
                BodyType.Initialize();            // delete this??   
            }*/

            if (SubstancesType != null)
            {
                SubstancesType.Initialize();                
            }

            if (ItemType != null)
            {
                ItemType.Initialize();

                // set a default icon sprite:
               /* if (string.IsNullOrEmpty(ItemType.IconSpriteName))
                {
                    if (RenderAsBillboardType != null && RenderAsBillboardType.Length > 0)
                    {
                      //  ItemType.IconSpriteName = RenderAsBillboardType[0].SpriteName + "_icon";
                    }
                }*/

                //TODO isItem kindof?
            }

            if (string.IsNullOrEmpty(PluralName))
            {
                PluralName = Name;
            }

            if (LocomotorType != null)
            {
                LocomotorType.Initialize();
            }
                       

            if (IntelligenceType != null)
            {
                IntelligenceType.Initialize(this);
            }
            
         

            if (SensorType != null)
            {
                SensorType.Initialize();
            }


        }

        public void InitRenderableTypeMode()
        {
            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                RenderableTypeMode = RenderableType;
            }
            else
            {
                RenderableTypeMode = EditorRenderableType ?? RenderableType;
            }

            if (RenderableTypeMode == null) // RenderableType == null && EditorRenderableType == null) //RenderableTypeMode == null)
            {
                RenderableType = new RenderableType(); // necessary. all entity types have a Renderable, and a RenderableType
                InitRenderableTypeMode();
            }
        }


        /// <summary>
        /// will this make it possible only to load the content in the scenario..?
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            if (RenderableType != null)
            {
                RenderableType.LoadContent(content);
            }

            if (EditorRenderableType != null)
            {
                EditorRenderableType.LoadContent(content);
            }
        }

       

        /// <summary>
        /// AFTER PostDataCompleteInitialize
        /// some init depend on the sprite maps/ utility maps having been loaded, or just need to happen after all type data has loaded. 
        /// 
        /// It would be better to place it earlier, in the loading thread...
        /// 
        /// put the code here:
        /// </summary>
        /// <param name="parent"></param>
        public void PostLoadContentInitialize()
        {
            if (IntelligenceType != null)
            {
                if (Person != null)
                {

                }

                IntelligenceType.PostLoadContentInitialize(this);
            }

            if (StructureType != null)
            {
                StructureType.PostLoadContentInitialize(this);
            }
            /*
            if (GeometryLayoutType != null)
            {
                GeometryLayoutType.PostLoadContentInitialize();// does nothing
            }*/

            if (BiologicalType != null)
            {
                BiologicalType.PostLoadContentInitialize(this);
            }

            if (ContainerType != null)
            {
                ContainerType.PostLoadContentInitialize(this);
            }

            if (ToolType != null)
            {
                ToolType.PostLoadContentInitialize();
            }

         /*   if (RequiresEnergyType != null)
            {
                // we need all other entity types to have been deserialized first:
                RequiresEnergyType.PostLoadContentInitialize();
               
            }*/

            if (NonLivingType != null)
            {
                NonLivingType.PostLoadContentInitialize();
            }

           


            if (Parts != null)
            {
                foreach (var item in Parts)
                {
                    if (item.Value > 0)
                    {
                        item.Key.ItemType.CanBeAPart = true;
                    }                    
                }
            }

           


            if (SensorType != null)
            {
                SensorType.PostLoadContentInitialize();
            }

        

            if (TreeType != null)
            {
                TreeType.PostLoadContentInitialize();
            }

            if (ItemType != null)
            {
                ItemType.PostLoadContentInitialize();
            }



            List<EntityEventHook> entityEventHooks;
            if (GameData.Instance.EntityEventHooksByEntityType.TryGetValue(this, out entityEventHooks))
            {
                // group actions by hook type:
                foreach (var item in entityEventHooks)
                {                    
                    Common.AddToMultiList(this.EventActions, item.Hook, GameData.Instance.AllActionSets[item.ActionSetsKey]);

                }
            }


            List<EntityTypePolledEvent> entityPolledEvent;
            if (GameData.Instance.EntityPolledEventByEntityType.TryGetValue(this, out entityPolledEvent))
            {
                // group actions by hook type:
                foreach (var item in entityPolledEvent)
                {
                    Common.AddToMultiList(this.PolledEvents, item.Scope, GameData.Instance.AllPolledEvents[item.PolledEventKey]);

                }
            }
        }

       

        public void CreateSpecialProcesses()
        {
            // creates unique processes for every EntityType. This is so process types can be shared by multiple entity types and their ActingOnEntityType will be correct.
            // but what about the production graph? mining sites especially
            if (SharedSpecialActions != null)
            {
                SharedSpecialActionTypes = new List<ProcessType>();

                foreach (var item in SharedSpecialActions)
                {
                    // just skip unavailable process types - they may have been excluded in the scenario data. (is this a good idea???)
                    ProcessType originalProcessType;
                    if (GameData.Instance.AllProcessTypes.TryGetValue(item.First, out originalProcessType))
                    {
                        bool isAvailable = item.Second;

                        ProcessType process = CreateUniqueSpecialProcess(originalProcessType, isAvailable);
                        SharedSpecialActionTypes.Add(process);

                        // we have to exclude the original process from the production graph, it has no ActingOnType set, so will screw things up.
                        // but we also need to keep the process in the gamedata collection, otherwise enable/disable won't work?
                        // why is this so... hmm..
                       // GameData.Instance.RemoveProcessFromProductionGraph(originalProcessType);
                    }

                }
            }

            if (SpecialActionLocks != null)
            {
                SpecialActionLockTypes = new List<ProcessType>();

                foreach (var item in SpecialActionLocks)
                {                  
                    ProcessType originalProcessType;
                    if (GameData.Instance.AllProcessTypes.TryGetValue(item.First, out originalProcessType))
                    {
                        bool isAvailable = item.Second;

                        ProcessType process = CreateUniqueSpecialProcess(originalProcessType, isAvailable);
                        SpecialActionLockTypes.Add(process);

                        // we have to exclude the original process from the production graph, it has no ActingOnType set, so will screw things up.
                        // but we also need to keep the process in the gamedata collection, otherwise enable/disable won't work?
                        // why is this so... hmm..
                        // GameData.Instance.RemoveProcessFromProductionGraph(originalProcessType);
                    }
                }
            }
        }

        private ProcessType CreateUniqueSpecialProcess(ProcessType originalProcessType, bool isAvailable)
        {
            string newKeyName = originalProcessType.KeyName + "_" + KeyName;

            ProcessType process = new ProcessType(originalProcessType, newKeyName, isAvailable);

            process.ActingOnType = this;// override

            process.InitDynamicProcess();
            return process;
        }

        public bool IsRepairable()
        {
            if (((IntelligenceType == null || IntelligenceType.IsMobile == false) // can't coop with agents yet
                    && (ItemType == null || Upgrader != null)) // don't repair items yet... we probably need special code to haul them to smithies etc.
                    && (NonLivingType != null && NonLivingType.RepairProfile != null))
            {
                return true;
            }

            return false;
        }


        public bool RequiresOutsideReplenishment()
        {
            if (IntelligenceType != null 
                && IntelligenceType.CanReplenish != true
                && IntelligenceType.IsMobile == false) // can't coop yet... // LocomotorType == null || LocomotorType.)
            {
                if (IntelligenceType.IntrinsicWeaponTypes != null)
                {
                    foreach (var item in IntelligenceType.IntrinsicWeaponTypes)
                    {
                        if (item.ContainerType != null && item.ContainerType is MagazineContainerType)
                        {
                            return true;
                        }
                    }
                }
            }

            // TODO: add more cases here...

            return false;
        }


        /// <summary>
        /// returns true if the given structure is the output of a special action on this entity type
        /// </summary>
        /// <param name="structureType"></param>
        /// <returns></returns>
        public bool IsSpecialActionOutput(EntityType structureType)
        {
            if (SharedSpecialActionTypes != null)
            {
                List<ProcessType> processTypes;
                if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(structureType, out processTypes)
                    && SharedSpecialActionTypes.Exists(p => processTypes.Contains(p)))
                {
                    return true;
                }
            }

            return false;
        }

      //  private RenderableType renderableTypeMode;

        public RenderableType RenderableTypeMode
        {
            get;
            private set;
            /* get 
             {
                 if (The.Sim.Mode == Sim.EngineMode.Game)
                 {
                     return RenderableType;
                 }
                 else
                 {
                     return EditorRenderableType ?? RenderableType;
                 }
             }*/
        }

        public Rectangle GetIconSprite(out IconInfo iconInfo)
        {
            Rectangle rect;
            Rectangle? rect2;

            string spriteName;

            if (ItemType != null)
            {
                if (!string.IsNullOrEmpty(Icon))
                {
                    spriteName = Icon;

                }
                else if (RenderableTypeMode != null
                    && RenderableTypeMode.DefaultClientState != null
                    && RenderableTypeMode.DefaultClientState.RenderAsBillboardType != null
                    && RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName != null
                    && The.InGameUI.gui.GUISpriteSheet.TryGetSourceRectangle(RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName, out rect2))
                {
                    spriteName = RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName;
                    // rect = rect2.Value;
                }
                /*   else if (!string.IsNullOrEmpty(entityType.Icon))
                   {
                       rect = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle(entityType.Icon); 

                   }*/
                else
                {
                    spriteName = "boxes";
                }
            }
            else if (StructureType != null)
            {
                spriteName = "HUD_icon_structure";
            }
            else if (TreeType != null)
            {
                spriteName = "HUD_icon_plant";
            }
            else if (BiologicalType != null)
            {

                if (Person != null)
                {
                    spriteName = "HUD_icon_person";
                }
                else
                {
                    spriteName = "HUD_icon_animal";
                }

            }
            else if (TerrainType != null)
            {
                if (TerrainType.IsSpecialInterestFeature)
                {
                    spriteName = "HUD_icon_star";
                }
                else
                {
                    spriteName = "boxes";
                }
            }
            else
            {
                spriteName = "boxes";
            }


            rect = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle(spriteName);
            GameData.Instance.AllIconInfo.TryGetValue(spriteName, out iconInfo);

            return rect;
        }


        public float? GetMeanHitpoints()
        {
            if (BiologicalType != null)
            {
                return BiologicalType.MeanHitpointsOfAdultMember;
            }
            else if (BodyType != null)
            {
                return BodyType.Hitpoints;                
            }

            return null;
        }

        public bool IsMountableWeapon()
        {
            if (ItemType != null && ItemType.WeaponType != null)
            {
                return IsMountable();
            }

            return false;
        }
        

        public bool IsMountable()
        {
            
            if (ToolType != null)
            {
                if (ToolType.ToolHandling == ToolHandlingType.HandTool) // IsHandTool == true) // ToolType.IsIntrinsic == true || ToolType.IsImmovable)
                    return true;
            }
            else if (ItemType != null && ItemType.WeaponType != null)
            {
                return ItemType.WeaponType.IsIntrinsic != true;
            }

            return false;

        }


        public bool IsImmovable()
        {
            if (ItemType != null)
            {
                return ItemType.HasNoMaximumBulk || (ItemType.MaximumBulk.HasValue && Items.Item.IsImmovable(ItemType.MaximumBulk.Value));
            }
            else if (Upgrader != null) // salvage upgrades?
            {
                return true;
            }
            else if (StructureType != null) // salvaging?
            {
                return true;
            }
            else return false;
        }
        
        public bool IsIntrinsic()
        {
            if (ToolType != null)
            {
                return ToolType.ToolHandling == ToolHandlingType.Intrinsic; // .IsIntrinsic == true;
            }
            else if (ItemType != null && ItemType.WeaponType != null)
            {
                return ItemType.WeaponType.IsIntrinsic == true;
            }

            return false;
        }
       
        public bool GetIsNeverInFogOfWar()
        {
            if (IsNeverInFogOfWar.HasValue)
            {
                return IsNeverInFogOfWar.Value;
            }
            else
            {
                return TreeType != null || TerrainType != null || RockType != null;
            }

        }


        public List<EffectProfileType> GetEffectProfiles()
        {
            List<EffectProfileType> list = null;
            if (Upgrader != null)
            {
                Common.AddRangeToList(ref list, Upgrader.EffectsFinal);
            }

            if (ItemType != null)
            {
                Common.AddRangeToList(ref list, ItemType.FinalEffectsWhenEquipped);

                if (ItemType.FoodType != null)
                {
                    Common.AddRangeToList(ref list, ItemType.FoodType.EffectTypes);
                }

                if (ItemType.WeaponType != null)
                {
                    foreach (var attack in ItemType.WeaponType.AttackTypes)
                    {
                        Common.AddRangeToList(ref list, attack.FinalEffectsOnVictim);    
                    }                    
                }
            }

            if (IntelligenceType != null)
            {
                if (IntelligenceType.IntrinsicWeaponTypes != null)
                {
                    foreach (var item in IntelligenceType.IntrinsicWeaponTypes)
                    {
                        foreach (var attack in item.ItemType.WeaponType.AttackTypes)
                        {
                            Common.AddRangeToList(ref list, attack.FinalEffectsOnVictim);
                        } 
                    }
                }
            }
            
            return list;
        }

        public bool GetUsesMemory()
        {
            if (UsesMemory.HasValue)
            {
                return UsesMemory.Value;
            }
            else
            {
                return TreeType == null && TerrainType == null && RockType == null;
            }

        }
        

       
        public bool HasStance()
        {
            return LocomotorType != null && LocomotorType.StancesType != null;
        }


        public override string ToString()
        {
            return Name + "(" + KeyName + ")";
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

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(EntityType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings(true)
        };

        #endregion

      

 
        /// <summary>
        /// serializes an EntityType property as a string, and deserializes it as a new placeholder instance (the real one may not have been created yet).
        /// the placeholder must then be replaced in a second pass...
        /// </summary>
        /// <returns></returns>
        public static CustomXmlSerializer.XmlTypeMapping<EntityType, string> GetEntityTypePropertySerializer(bool useEntityPlaceholder)
        {
            if (useEntityPlaceholder)
            {
                return new CustomXmlSerializer.XmlTypeMapping<EntityType, string>()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,

                    // when deserializing: watch out for cycles, or EntityType instances not created yet! 
                    //- create a placeholder EntityType with empty Name to identify and replace in a second pass.
                    SetterMethod = s => s == null ? null : new EntityType(s)

                 };
            }
            else
            {
                return new CustomXmlSerializer.XmlTypeMapping<EntityType, string>()
                {
                    GetterMethod = t => t == null ? null : t.KeyName,
                                        
                    SetterMethod = s => s == null ? null : GameData.Instance.AllEntityTypes[s]
                };
            }
        }



    }


   

    public class XmlDictionary<T, V> : Dictionary<T, V>, IXmlSerializable
    {
        [XmlType("Entry")]
        public struct Entry
        {
            public Entry(T key, V value) : this() { Key = key; Value = value; }
            [XmlElement("Key")]
            public T Key { get; set; }
            [XmlElement("Value")]
            public V Value { get; set; }
        }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            this.Clear();
                       

            var serializer = new XmlSerializer(typeof(List<Entry>));
            reader.Read();  // Why is this necessary?
            var list = (List<Entry>)serializer.Deserialize(reader);

            foreach (var entry in list) 
                this.Add(entry.Key, entry.Value);

            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            var list = new List<Entry>(this.Count);

            foreach (var entry in this) 
                list.Add(new Entry(entry.Key, entry.Value));

            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");

            XmlSerializer serializer = new XmlSerializer(list.GetType());
            serializer.Serialize(writer, list, ns);
        }
    }
}
