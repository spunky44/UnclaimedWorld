using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Items;
using UWGame.SimSide.Trees;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Containers;
using System.Diagnostics;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.Client.Interface;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Containers.Components;


namespace UWGame.SimSide.AI
{

    public enum MemoryFactID : ulong
    {
        First = 0L,
        Invalid = uint.MaxValue,
        Max = Invalid
    }


    /// <summary>
    /// some basic info about items/entities that the evaluators use. 
    /// We only keep memory facts about entities that are NOT currently seen (= in the fog of war, or not detected yet)!
    /// 
    /// This class will only grow bigger when we add new features and evaluators...
    /// </summary>
    [DebuggerDisplay("{EntityType.Name}{MapPosition}")]
    public class MemoryFact : GameObject, IKnownEntityData, IHasExposedProperties, ILookUp<MemoryFact, MemoryFactID>, ISnapshot
    {
        public EntityID EntityID { get; set; }

        private SharedKnowledge sharedKnowledge;
        private AllegianceID snapshotAllegianceID;


        public DebugLog DebugLog = new DebugLog();

        /// <summary>
        /// set true when this memory fact can no longer be trusted. we have no knowledge of the entity anymore.
        /// Set true in these cases:
        /// case 1: the corresponding entity is not seen when exploring the tile referencing the memory fact. 
        ///     this works for contained/part (leaf) entities too.
        /// case 2: the memory fact has been kept for a long time and refers to an entity that is not owned by us.
        /// 
        /// this will result in EntityResult.EntityStatusIsNowUnknown being returned by lookup. A skip result for the evaluators that will cause jobs etc. to be removed as well.
        /// 
        /// However, the memory fact will remain in the collection for a long while after.
        /// </summary>
        public bool IsDeprecated
        {
            get
            {
                return isDeprecated;
            }
            set
            {
                if (value != isDeprecated)
                {
                    isDeprecated = value;

                    if (isDeprecated)
                    {
                        // processes do not have a deprecated flag. We remove them immediately:
                        if (sharedKnowledge.PlaySiteKnowledge != null)
                        {
                            sharedKnowledge.PlaySiteKnowledge.DeleteMemoryOfProcesses(this);
                        }

                        if (sharedKnowledge.Allegiance.IsOwnedByAllegiance(this)) // new: only log owned items
                        {
                            // NEW: store a stat as well for "disappeared" items:
                            if (Replenishes.HasValue)
                            {
                                IKnownEntityData container;
                                if (sharedKnowledge.GetKnownData(Replenishes.Value, out container) == EntityResult.SeenDirectly)
                                {
                                    Entity containerEntity = container as Entity;
                                    //if (containerEntity.)

                                    // consider disappeared replenish items used as inputs/fuel... test if they were burning also? could have degraded...
                                    Entity.LogProductionEvent(this, Allegiances.Statistics.ProductionStatistics.StatTypes.UsedAsInput, false);
                                    //sharedKnowledge.Allegiance.Statistics.AddProductionEvent(EntityType, Allegiances.Statistics.ProductionStatistics.StatTypes.UsedAsInput, 1);

                                    return;
                                }
                            }

                            sharedKnowledge.Allegiance.Statistics.AddProductionEvent(EntityType, Allegiances.Statistics.ProductionStatistics.StatTypes.Disappeared, 1);
                        }
                    }
                }
            }
        }

        private bool isDeprecated = false;

        /// <summary>
        /// the time point for when this memoryfact should be deleted - only for not-owned entities!!
        /// </summary>
        public double? ToBeDeletedOnTimeStampInSecondsOfGameTime;


        /// <summary>
        /// used in cleanup
        /// </summary>
        public double TimeStampInSecondsOfGameTime;


        //  public AnimConditions AnimConditions { get; set; }


        public Vector3 PlaySiteLocation
        {
            get
            {
                return Location.Value;
            }

        }

        // these are the most commonly used, so we don't treat them as objects in the Properties collection to avoid boxing performance issues...
        public Point? MapPosition { get; set; }

        public SiteID? Site { get; set; }

        /// <summary>
        /// for structures ..
        /// </summary>
        public Point? TopLeftMapPosition { get; set; }

        public float Rotation { get; set; }

        public Vector3 FacingNormal { get; set; }

        public bool? IsMoving { get; set; }

        public EntityType EntityType { get; set; }

        public float Bulk { get; set; }

        public double? Condition { get; set; }

        public float? ConditionChangeSpeed { get; set; }

        public CasteType CasteType { get; set; }
        private string casteTypeKey;

        public float? Integrity { get; set; }

        public Dictionary<EntityType, EntityID> IntrinsicWeapons { get; set; }

        public List<ProcessType> AvailableSharedSpecialActions { get; set; }


        public List<SimProcessID> Processes { get; set; }

        /// <summary>
        /// a full copy of all bodyparts...
        /// </summary>
        public Body Body { get; set; }

        public Dictionary<SubstanceType, SubstanceAmount> SubstanceBulkAmounts { get; set; }

        public string Name { get; set; }


        public GatheringSite GatheringSite { get; set; }
        private GatheringSiteID? snapshotGatheringSite;


        public bool IsAlwaysShown() //= false; // !!! trees, rocks, buildings are always shown...
        {
            return EntityType.GetIsNeverInFogOfWar();
        }

        public bool PartIsBroken { get; set; }

        public double? FunctionalScore { get; set; }

        //public AgentStorage AgentStorage { get; set;  }
        //  public Container Container { get; set; }


        /* public Dictionary<Entity, Vector3> MeleeAttackers
         {
             get; // { return sharedData.MeleeAttackers; }
             set;
         }*/

        public Dictionary<string, PropertyResult> CustomFields;

        public StanceType Stance { get; set; }
        //  public LeggedLocomotor.Stance? Stance { get; set; }

        public float? StrengthRating
        {
            get;
            private set;
        }

        private RepairPackage repairPackage;

       /* private bool areaIsCleared;

        /// <summary>
        /// applies to ordered structures??
        /// </summary>
        public bool AreaIsCleared
        {
            get
            {
                return areaIsCleared;
            }
        }*/

      //  private Vector3? accessPoint;
        public Vector3? AccessPoint { get; set; }
      /*  {
            get
            {
                return accessPoint;
            }
            set
            {
                accessPoint = value;
            }
        }*/


        public OwnerID? OwnedBy { get; set; }



        private JobID? assignedToJob;

        /// <summary>
        /// note that we can assign destroyed entities this way!
        /// </summary>
        public JobID? AssignedToJob
        {
            get
            {
                return assignedToJob;
            }
            set
            {
                if (assignedToJob != value)
                {
                    if (value.HasValue)
                    {
                        DebugLog.Add(string.Format("[MF] AssignedToJob set: JobID {0}", value.Value.ToString()));
                    }
                    else
                    {
                        DebugLog.Add(string.Format("[MF] AssignedToJob cleared. Old JobID: {0}", assignedToJob.Value.ToString()));
                    }

                    assignedToJob = value;
                }
            }
        }

        /// <summary>
        /// No class should directly interact with inuseby. They should use the 2 methods  SetInUseBy and GetInUseBy
        /// </summary>
      /*  private Dictionary<EntityGroupID, EntityID> inUseBy;
        private Dictionary<EntityGroupID, EntityID> InUseBy
        {
            get
            {
                if (inUseBy == null)
                {
                    inUseBy = new Dictionary<EntityGroupID, EntityID>();
                }
                return inUseBy;
            }
        }

       

        public void ClearInUseBy(EntityGroup entityGroup, EntityID userID)
        {
            Entity.ClearInUseBy(InUseBy, entityGroup, userID);


            DebugLog.Add(string.Format("[MF] ClearInUseBy: EntityGroup {0}, Entity {1}", entityGroup.ID, userID));
        }


        public void SetInUseBy(EntityGroup entityGroup, EntityID id)
        {
            // InUse[entityGroup.ID] = new SimProcess(id);
            if (id == (EntityID)4546)
            {

            }

            EntityID idToSet;
            if (InUseBy.TryGetValue(entityGroup.ID, out idToSet))
            {
                InUseBy.Remove(entityGroup.ID);
            }

            InUseBy.Add(entityGroup.ID, id);

            DebugLog.Add(string.Format("[MF] SetInUseBy: EntityGroup {0}, Entity {1}", entityGroup.ID, id));
        }

        public EntityID? GetInUseBy(EntityGroup entityGroup)
        {

            EntityID entityInUseByID;

            if (!InUseBy.TryGetValue(entityGroup.ID, out entityInUseByID))
                return null;

            return entityInUseByID;
        }
        */

        public float BoundingRadius3D { get; set; }


        private bool flipHorizontally = false;
        public bool FlipHorizontally
        {
            get
            {
                return flipHorizontally;
            }
            set
            {
                flipHorizontally = value;

                if (Renderable != null)
                {
                    Renderable.FlipHorizontally = value;
                }
            }
        }


        #region Storage

        public bool HasItemStorage { get; set; }


        private Dictionary<StorageCondition, Storage> storageSpaces;
      //  Dictionary<StorageCondition, StorageID> snapshotStorageSpaces;

        /// <summary>
        /// Equipment storage is not included here...
        /// </summary>
        public Dictionary<StorageCondition, Storage> StorageSpaces
        {
            get { return storageSpaces; }
        }

        public Storage FindStorage(StorageID storageID)
        {
            if (storageSpaces != null)
            {
                foreach (var item in storageSpaces)
                {
                    if (item.Value.ID == storageID)
                    {
                        return item.Value;
                    }
                }
            }

            if (tradeOfferStorageSpaces != null)
            {
                foreach (var item in tradeOfferStorageSpaces)
                {
                    if (item.Value.ID == storageID)
                    {
                        return item.Value;
                    }
                }
            }

            return null;
        }

        public bool IsTradeOfferStorage(StorageID storageID)
        {
            if (tradeOfferStorageSpaces != null)
            {
                return tradeOfferStorageSpaces.Any(s => s.Value.ID == storageID);
            }

            return false;
        }

      /*  public StorageCompartment? FindCompartment(StorageID storageID)
        {
            if (storageSpaces != null)
            {
                foreach (var item in storageSpaces)
                {
                    if (item.Value.ID == storageID)
                    {
                        return item.Value.;
                    }
                }
            }

            return null;
        }*/


        private Dictionary<StorageCondition, Storage> tradeOfferStorageSpaces;
     //   Dictionary<StorageCondition, StorageID> snapshotTradeOfferStorageSpaces;


        public Dictionary<StorageCondition, Storage> TradeOffersStorageSpaces
        {
            get { return tradeOfferStorageSpaces; }
        }


        public float? TotalItemStorageCapacity { get; set; }
        public float? TotalStored { get; set; }

        private Dictionary<EntityID, EntityID> contains;
        public bool ContainsEntity(EntityID entity)
        {
            return contains != null && contains.ContainsKey(entity);
        }


        #endregion



        #region Containers

        /// <summary>
        /// still needed??
        /// </summary>
        //  public Dictionary<EntityType, int> ContainedEntityTotals { get; set; } //= new Dictionary<EntityType, int>();

        public Dictionary<EntityType, List<EntityID>> ContainedEntitiesByType { get; set; }

        public List<EntityID> ContainedEntities
        {
            get
            {
                if (contains != null)
                {
                    return contains.Keys.ToList();
                }

                return null;
            }
        }

        public Dictionary<UpgradeCategory, EntityID> ContainedUpgrades { get; set; }
            
        public Dictionary<EntityType, List<EntityID>> OfferedEntitiesByType { get; set; }


        public EntityID? ContainedBy { get; set; }

        public EntityID? Replenishes { get; set; }

        public EntityID? UpgradeFor { get; set; }


        public StorageTarget? StoredPermanentlyIn { get; set; }

      //  public bool IsOfferedForTrade { get; set; }

        //public StorageTarget? OfferedForTradeIn { get; set; }

        /*
        public EntityID? StoredPermanentlyIn
        {
            get;
            set;
        }
        public StorageID? StoredPermanentlyStorageID
        {
            get;
            set;
        }
        public StorageCondition StoredPermanentlyCondition
        {
            get;
            set;
        }

        public StorageID? OfferedForTradeStorageID
        {
            get;
            set;
        }*/



        /// <summary>
        /// TODO: rework this...
        /// </summary>
        private bool notOnboardDrivenVehicle;

        public bool NotOnboardDrivenVehicle
        {
            get
            {
                return notOnboardDrivenVehicle;
            }
        }

        /* public Storage StoredIn
         {
             get
             {
                 Entity entity = Entity.FindByID(EntityID);
                 if (entity != null)
                 {
                     return entity.StoredIn;
                 }
                 else
                 {

                 }
                                

                 return null;
             }
         }*/

        #endregion

        public float? CurrentMaximumSpeed { get; set; }

        // public float? LoadedVehicleSpeed { get; set;  }

        #region Vehicles

        private float loadedVehicleSpeed;
        public float CalculateSpeed(float bulk)
        {
            return loadedVehicleSpeed;
        }

        private PassengerOrCargoSlot freeDriversSlot;
        /// <summary>
        /// if the vehicle entity exists, we can give the real driver slot.
        /// if the vehicle has been detroyed, we just give a fake one.
        /// The agent will find the destroyed vehicle before he gets there.
        /// </summary>
        /// <param name="bulkToLoad"></param>
        /// <returns></returns>
        public PassengerOrCargoSlot GetFreeDriversSlot()
        {
            Entity vehicle = Entity.FindByID(EntityID);
            if (vehicle != null)
            {
                return vehicle.GetFreeDriversSlot();
            }
            else
            {
                return freeDriversSlot;
            }
        }


        private List<PassengerOrCargoSlot> listOfCargoSlotsForLoading;
        /// <summary>
        /// if the vehicle entity exists, we can give the real list of cargo slots (nobody but our agents will have interfered with them)
        /// if the vehicle has been detroyed, we just give a fake list.
        /// The agent will find the destroyed vehicle before he gets there.
        /// </summary>
        /// <param name="bulkToLoad"></param>
        /// <returns></returns>
        public List<PassengerOrCargoSlot> GetCargoSlotsForLoading(float bulkToLoad)
        {
            Entity vehicle = Entity.FindByID(EntityID);
            if (vehicle != null)
            {
                return vehicle.GetCargoSlotsForLoading(bulkToLoad);
            }
            else
            {
                return listOfCargoSlotsForLoading;
            }
        }


        private List<PassengerOrCargoSlot> listOfCargoSlotsForUnloading;
        /// <summary>
        /// if the vehicle entity exists, we can give the real list of cargo slots.
        /// if the vehicle has been detroyed, we just give a fake list.
        /// The agent will find the destroyed vehicle before he gets there.
        /// </summary>
        /// <param name="bulkToLoad"></param>
        /// <returns></returns>
        public List<PassengerOrCargoSlot> GetCargoSlotsForUnloading(float bulkToLoad)
        {
            Entity vehicle = Entity.FindByID(EntityID);
            if (vehicle != null)
            {
                return vehicle.GetCargoSlotsForUnloading(bulkToLoad);
            }
            else
            {
                return listOfCargoSlotsForUnloading;
            }
        }
        #endregion


        #region Ammunition

        public int? NoOfRounds { get; set; }

        #endregion

        #region Replenish

        private float? fuel;
        public bool HasEnoughFuel(float neededFuel)
        {
            if (fuel.HasValue)
            {
                return fuel.Value >= neededFuel;
            }

            return true;
        }


        Dictionary<EntityType, int> ammoItems = new Dictionary<EntityType, int>();
        public bool HasEnoughAmmo(EntityType ammoType, int noOfRounds)
        {
            if (ammoItems != null)
            {
                int ammo;
                if (ammoItems.TryGetValue(ammoType, out ammo))
                {
                    return ammo > noOfRounds;
                }

            }

            return false;
        }

        public bool NeedsReload(SharedKnowledge sharedKnowledge, out IKnownEntityData itemToReload)
        {
            return Entity.NeedsReload(sharedKnowledge, this, out itemToReload);
        }

        
        public bool NeedsRepair() //SharedKnowledge sharedKnowledge) //, out RepairAction? action, out EntityID? partToFix)
        {
            // eval during Init, save the info.
          
            return repairPackage != null; // needsRepair;
        }

        
        public RepairPackage ComputeBestRepairPackage() //SharedKnowledge sharedKnowledge, out RepairAction? action, out EntityID? partToFix)
        {
            return repairPackage;
        }


        public float GetRepairProgress(RepairAction repairAction) //, EntityID? partToFix)
        {
            if (repairAction == RepairAction.Integrity)
            {
                return Integrity.Value;
            }
            else if (repairAction == RepairAction.Condition ||
                repairAction == RepairAction.PartsCondition)
            {
                return (float)Condition.Value;
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        public int? GetTotalAmmo()
        {
            if (ammoItems != null)
            {
                return ammoItems.Sum(a => a.Value);
            }

            return null;
        }

        public bool HasEnergyForDuration(float jobDuration)
        {
            bool hasFuel = true;
            if (fuel.HasValue)
            {
                hasFuel = RequiresFuel.HasFuelForDuration(jobDuration, EntityType, fuel.Value);
            }

            //TODO:
            /*  bool hasPower = true;
              if (RequiresPower != null)
              {
                  hasPower = RequiresPower.HasPowerForDuration(duration);
              }*/

            //  return hasFuel && hasPower;     
            return hasFuel;
        }

        #endregion


        #region Food

        public Dictionary<FoodNutrientType, float> NutrientBulkAmounts { get; set; }

        #endregion

        #region structures

        public int? Residents { get; set; }

        public float? ComfortLevel
        {
            get; set;
        }

        public List<HouseholdID> Households { get; set; }

        #endregion

        public bool? IsPrepared { get; set; }


        /*  public IComposite GetRoot()
          {
              if (PartOf == null)
              {
                  return this;
              }
              else
              {
                  return PartOf.GetRoot();
              }
          }*/

        /// <summary>
        /// TODO: i don't think this works for getting the root. Should MemoryFact implement IComposite?
        /// </summary>
        public CompositeID? /*IComposite*/ PartOfID { get; set; }

        public List<EntityID> PartIDs { get; set; }


        /// <summary>
        /// NOT the root.
        /// </summary>
        public EntityID? ParentEntityID { get; private set; }

        /// <summary>
        /// the root entity we are part of.
        /// otherwise itself (similar to Entity.GetRoot())
        /// </summary>
        public EntityID RootEntityID { get; private set; }

        public EntityAndRoot GetAsEntityAndRoot()
        {
            EntityAndRoot partAndRoot = new EntityAndRoot(this.EntityID, RootEntityID);
            return partAndRoot;
        }


        private bool isCompleted;
        public bool IsCompleted()
        {
            return isCompleted;
        }

        private bool? isStarted;
        public bool? IsStarted()
        {
            return isStarted;
        }

       /* private bool isEnclosed;
        public bool IsEnclosed()
        {
            return isEnclosed;
        }*/

        private float? progress;
        public float? Progress
        {
            get
            {
                return progress;
            }
        }

        private bool isWeatherProof;
        public bool IsWeatherProof()
        {
            return isWeatherProof;
        }

        private Regulator showStatusRegulator;

        public bool IsTimeToShowStatusMarkerWindow()
        {
            return showStatusRegulator.IsReady();
        }

        public bool IsUnassigned(SharedKnowledge sharedKnowledge)
        {
            return IsUnassigned(this, sharedKnowledge);

        }

       /* public static bool IsUnassigned(IKnownEntityData entity, EntityGroup groupToCheck)
        {
            return entity.PartOfID == null && entity.AssignedToJob == null && entity.GetInUseBy(groupToCheck) == null;

        }*/

        public static bool IsUnassigned(IKnownEntityData entity, SharedKnowledge sharedKnowledge)
        {
            return entity.PartOfID == null && entity.AssignedToJob == null && sharedKnowledge.GetInUseBy(entity.EntityID) == null;

        }

        public bool IsUnassignedToAnythingButThisJob(Job job, SharedKnowledge sharedKnowledge)
        {
            return IsUnassignedToAnythingButThisJob(this, job, sharedKnowledge);

        }

       /* public static bool IsUnassignedToAnythingButThisJob(IKnownEntityData entity, Job job, EntityGroup groupToCheck)
        {
            return entity.PartOfID == null && (entity.AssignedToJob == null || entity.AssignedToJob == job.ID) && entity.GetInUseBy(groupToCheck) == null;

        }*/

        public static bool IsUnassignedToAnythingButThisJob(IKnownEntityData entity, Job job, SharedKnowledge sharedKnowledge)
        {
            return entity.PartOfID == null && (entity.AssignedToJob == null || entity.AssignedToJob == job.ID) && sharedKnowledge.GetInUseBy(entity.EntityID) == null;

        }

        public bool CanBeHauled()
        {
            return Entity.CanBeHauled(this);
        }

        public bool IsItemValidForHauling(Entity haulingEntity, Intelligence entityIntelligence, HaulingJob job)
        {
            return Entity.IsItemValidForHauling(haulingEntity, entityIntelligence, job, this);

            /*
            // Vector3 knownItemLocation = The.Sim.GetKnownLocation(entityIntelligence.Allegiance, item);
            Job assignedToJob = EvaluateJob.ResolveAssignedToJob(this);

            if (
                (notOnboardDrivenVehicle)
                && CanBeHauled() 
                && OwnedBy != null
                && Entity.StorageHasRoomForItem(entityIntelligence, job, this)
                && (assignedToJob == null || assignedToJob is HaulingJob)
                // && (haulingEntity.AgentStorage != null && haulingEntity.AgentStorage.ItemStorage != null 
                && haulingEntity.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(this.Bulk)
                && (job.ToStorageEntity == null || (job.ToStorageEntity != StoredPermanentlyIn && job.ToStorageCondition != StoredPermanentlyCondition))
                )
            {
                return true;
            }

            return false;*/

        }

        public bool CanBeHunted(Allegiances.Allegiance byAllegiance)
        {
            return Entity.CanBeHunted(this, byAllegiance);
        }

        public bool IsVehicleValidForHauling(Entity entity, IKnownEntityData item)
        {
            // Vehicle vehicleComponent;
            // Find(out vehicleComponent);

            if (//((vehicleComponent.DrivenBy == null) || vehicleComponent.DrivenBy == entity) // the vehicle is not driven by anyone since it is in the fog of war
                HasItemStorage
                && TotalItemStorageCapacity >= item.Bulk
                && IsCompleted()
                && Entity.IsFunctional(this)) // GoalEvaluator.ScoreIsEntityFunctional(this) > 0.0) // omit broken down vehicles
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ChangeOwnership(IOwner newOwner, Entity.GiveNewOwnerKnowledge giveNewOwnerKnowledge = Entity.GiveNewOwnerKnowledge.Yes)
        {

#if !RELEASE
            if (newOwner != null)
            {
                throw new Exception("Can't set a new owner on a MemoryFact");
            }
#endif

            Entity.ChangeOwnership(this, newOwner, giveNewOwnerKnowledge);

        }
      

        public AllegianceID? AllegianceID { get; set; }

        public ThreatGroup ThreatGroup { get; set; }
        private ThreatGroupID? snapshotThreatGroup;

        public Renderable Renderable;

        #region "Client" fields

        Renderable.SnapshotRenderable snapshotRenderable;

        #endregion


        public enum StatusProperty { Condition, AllegianceId, Stance }

        // public Dictionary<StatusProperty, object> Properties;

        // too risky to use memory pool i feel... some fields may not be cleared/reset, which causes subtle bugs...
        //  private static Pool<MemoryFact> memoryFactPool = new Pool<MemoryFact>(60);

        private EntityTypeTooltipInstanceData tooltipEntityData = new EntityTypeTooltipInstanceData(); // should be decoupled from memory fact?
        public EntityTypeTooltipInstanceData TooltipEntityData
        {
            get
            {
                return tooltipEntityData;
            }
            set
            {
                tooltipEntityData = value;
            }
        }

        public MemoryFact() //IGameEntity entity)
        {
            //Init(entity);
        }


        /// <summary>
        /// TODO: resource containers
        /// </summary>
        /// <param name="gameEntity"></param>
        /// <returns></returns>
        public static MemoryFact GetNew(Entity gameEntity, Allegiances.Allegiance allegiance)
        {
            // MemoryFact memoryFact = memoryFactPool.Get();
            MemoryFact memoryFact = new MemoryFact(); // too risky to use memory pool i feel... some fields may not be cleared/reset, which causes subtle bugs...

            memoryFact.AddToLookup();

            if (!memoryFact.Init(gameEntity, allegiance))
            {
                // the entity is destroyed...
                memoryFact.Destroy();
                return null;
            }

            return memoryFact;
        }


        /// <summary>
        /// returns false if the Entity was destroyed while examining it (could happen because of non-existing containers)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Init(Entity entity, Allegiances.Allegiance allegiance) // TODO: instead of Entity, create an interface for resource containers too
        {

            DebugLog.Add("[MF] MemoryFact created");

            if (entity.EntityID == (Entities.EntityID)4991)
            {

            }

            isDeprecated = false;

            EntityID = entity.EntityID;

            EntityType = entity.EntityType;

            //AnimConditions = new ClientSide.Renderables.AnimConditions();


            Renderable = null;

            // only create the renderable if it is needed to by the client:
            The.Client.SetRenderableOnMemoryFact(this, entity, allegiance);


            sharedKnowledge = allegiance.SharedKnowledge;

            TimeStampInSecondsOfGameTime = The.Sim.TotalUnPausedGameTimeInSeconds;

            //  IsDestroyed = gameEntity.IsDestroyed;

            Location = entity.Location;
            MapPosition = entity.MapPosition; // MapManager.WorldPosToTile(Location);
            Site = ((IKnownEntityData)entity).Site;
            TopLeftMapPosition = entity.TopLeftMapPosition;

           /* if (Location != null)
            {*/
                AccessPoint = entity.AccessPoint;
           // }

            FlipHorizontally = entity.FlipHorizontally;

            IsMoving = entity.IsMoving;

            PartIsBroken = entity.PartIsBroken;

            showStatusRegulator = Entity.CreateShowStatusIconRegulator("MemoryFact", entity.EntityType);

            //areaIsCleared = entity.AreaIsCleared;

            Bulk = entity.Bulk;

            AssignedToJob = entity.AssignedToJob;
           

           // inUseBy = entity.GetInUseByListForSync(); // make a copy.
            

            ContainedBy = entity.ContainedBy;

            ContainedUpgrades = entity.ContainedUpgrades;


            BoundingRadius3D = entity.BoundingRadius3D;

            // this points to an object that we would like to survive after the entity is destroyed
            GatheringSite = entity.GatheringSite;

            // is this safe...?
            OwnedBy = entity.OwnedBy;

            Rotation = entity.Rotation;
            FacingNormal = entity.FacingNormal;

            Locomotor locomotor;
            if (entity.Find(out locomotor))
            {
                //  Rotation = locomotor.Rotation;
                //  FacingNormal = locomotor.FacingNormal;

                CurrentMaximumSpeed = locomotor.CurrentMaximumSpeed;
                // use an estimate...
                loadedVehicleSpeed = locomotor.CalculateSpeed(1f);
            }
            else
            {
                //   Rotation = null;
                //    FacingNormal = null;
                CurrentMaximumSpeed = null;
                loadedVehicleSpeed = 0f;
            }

            NonLivingEntity nonLiving;
            if (entity.EntityType.NonLivingType != null &&
                entity.Find(out nonLiving))
            {
                Condition = nonLiving.Condition;
                ConditionChangeSpeed = nonLiving.ConditionChangeSpeed;
                Integrity = nonLiving.Integrity;

                progress = nonLiving.Progress;

                if (entity.EntityType.IsRepairable()
                    && nonLiving.NeedsRepair())
                {
                    repairPackage = nonLiving.ComputeBestRepairPackage();
                }
                else
                {
                    repairPackage = null;
                }
            }
            else
            {
                Condition = null;
                Integrity = null;

                progress = null;
            }

            UWGame.SimSide.Entities.Containers.ReplenishItems replenishItems;
            fuel = null;
            IsPrepared = null;
            if (entity.EntityType.ContainerType != null && entity.EntityType.ContainerType.GetRequiresReplenishType() != null)
            //gameEntity.Find(out energy))
            {
                replenishItems = ((IHasReplenishItems)entity.Contains).ReplenishItems;
                if (replenishItems != null)
                {
                    if (replenishItems.RequiresFuel != null)
                    {
                        fuel = replenishItems.RequiresFuel.Fuel;
                    }
                }
            }

            UWGame.SimSide.Items.Tool tool;
            if (entity.Find(out tool))
            {

                IsPrepared = tool.IsPrepared;
            }

            // if the entity has a magazine:
            ammoItems = null;
            if (entity.EntityType.ContainerType != null && entity.EntityType.ContainerType is MagazineContainerType)
            {
                MagazineContainer mag = entity.Contains as MagazineContainer;
                if (mag != null)
                {
                    mag.GetAmmoStatus(ref ammoItems);
                }
            }

            // if the entity is an ammo item:
            NoOfRounds = null;
            if (entity.EntityType.ItemType != null && entity.EntityType.ItemType.AmmunitionType != null)
            {
                NoOfRounds = entity.Item.Ammunition.NoOfRounds;
            }


            StrengthRating = entity.StrengthRating;

            FunctionalScore = entity.FunctionalScore;

            StoredPermanentlyIn = entity.StoredPermanentlyIn;
          //  IsOfferedForTrade = entity.IsOfferedForTrade;

            /*
            StoredPermanentlyIn = entity.StoredPermanentlyIn;
            StoredPermanentlyCondition = entity.StoredPermanentlyCondition;
            StoredPermanentlyStorageID = entity.StoredPermanentlyStorageID;
            OfferedForTradeStorageID = entity.OfferedForTradeStorageID;*/


            if (entity.Processes != null)
            {
                // copy the list
                Processes = new List<SimProcessID>(entity.Processes);
            }

            // COPY STORAGE
            HasItemStorage = false;

            if (storageSpaces != null)
                storageSpaces.Clear();

            TotalItemStorageCapacity = null;
            TotalStored = null;

            contains = null;

            // get storage info - we don't want to copy the Container component!!!
            Container container = entity.Contains;
            if (container != null)
            {

                IStorage storage = container as IStorage;
                if (storage != null)
                {
                    HasItemStorage = true;
                    TotalItemStorageCapacity = storage.TotalItemStorageCapacity;
                    TotalStored = storage.TotalStored;

                    if (storageSpaces == null)
                    {
                        storageSpaces = new Dictionary<StorageCondition, Storage>();
                    }

                    var originalStorageSpaces = storage.GetStorageSpaces();
                    foreach (var storageSpace in originalStorageSpaces)
                    {
                        // copy the objects (and the lists of stored items):
                        this.storageSpaces.Add(storageSpace.Key, new Storage(storageSpace.Value));
                    }
                }

                TerminalContainer terminalContainer = container as TerminalContainer;
                if (terminalContainer != null)
                {
                    if (tradeOfferStorageSpaces == null)
                    {
                        tradeOfferStorageSpaces = new Dictionary<StorageCondition, Storage>();
                    }

                    var originalStorageSpaces = terminalContainer.GetTradeOffersStorageSpaces();
                    foreach (var storageSpace in originalStorageSpaces)
                    {
                        // copy the objects (and the lists of stored items):
                        this.tradeOfferStorageSpaces.Add(storageSpace.Key, new Storage(storageSpace.Value));
                    }

                }


                // get a list of the entity ids currently stored!
                container.IterateContained(GetContainedStatus);


                // this is used to populate the side panel:
                ContainedEntitiesByType = new Dictionary<Entities.EntityType, List<Entities.EntityID>>();
                container.IterateContained(e =>
                {
                    Common.AddToMultiList(ContainedEntitiesByType, e.EntityType, e.ID);
                });


                TerminalContainer terminal = entity.Contains as TerminalContainer;
                if (terminal != null)
                {
                    OfferedEntitiesByType = terminal.GetOfferedItems();
                }

            }


            PartOfID = entity.PartOfID;

            if (PartOfID.HasValue)
            {
                IComposite parent = LookUpIComposites.FindByID(PartOfID.Value);

                Entity parentAsEntity = parent as Entity;
                if (parentAsEntity != null)
                {
                    ParentEntityID = parentAsEntity.EntityID;
                }
            }

            PartIDs = entity.PartIDs;

            IComposite root = entity.GetRoot();
            Entity rootEntity = root as Entity;
            if (rootEntity != null) //root != entity)
            {
                RootEntityID = rootEntity.ID;
            }

            isCompleted = entity.IsCompleted();

            isStarted = entity.IsStarted();
            
           // isEnclosed = entity.IsEnclosed();

            isWeatherProof = entity.IsWeatherProof();

            Residents = entity.Residents;
            ComfortLevel = entity.ComfortLevel;

            List<HouseholdID> households = entity.Households;
            if (households != null)
            {
                Households = new List<HouseholdID>();
                Households.AddRange(households);
            }


            NutrientBulkAmounts = null;
            if (entity.EntityType.ItemType != null)
            {
                //Item itemComponent = gameEntity.Item;
                notOnboardDrivenVehicle = entity.NotOnboardDrivenVehicle; // gameEntity.OnBoard == null || gameEntity.OnBoard.Vehicle.DrivenBy == null;

                if (entity.EntityType.ItemType.FoodType != null)
                {
                    NutrientBulkAmounts = entity.Item.Food.NutrientBulkAmounts.ToDictionary(k => k.Key, v => v.Value); // it is necessary to copy these values if the original gets destroyed...
                }
            }
            else
            {
                notOnboardDrivenVehicle = true;
            }

           /* Person person;
            if (entity.Find(out person))
            {*/
                Name = entity.Name;
          /*  }
            else
            {
                Name = null;
            }*/

            if (entity.EntityType.ContainerType != null && entity.EntityType.ContainerType is VehicleContainerType)
            {
                freeDriversSlot = entity.GetFreeDriversSlot();
                listOfCargoSlotsForLoading = entity.GetCargoSlotsForLoading(1f);
                listOfCargoSlotsForUnloading = entity.GetCargoSlotsForUnloading(1f);
            }
            else
            {
                freeDriversSlot = null;
                listOfCargoSlotsForLoading = null;
                listOfCargoSlotsForUnloading = null;
            }


            Entity replenishes;
            if (!entity.GetReplenishes(out replenishes))
            {
                // destroyed...
                return false;
            }
            else
            {
                if (replenishes != null)
                {
                    Replenishes = replenishes.EntityID;
                }
                else Replenishes = null;
            }

            Entity upgrades;
            if (!entity.GetUpgradesFor(out upgrades))
            {
                // destroyed...
                return false;
            }
            else
            {
                if (upgrades != null)
                {
                    UpgradeFor = upgrades.EntityID;
                }
                else UpgradeFor = null;
            }

            AvailableSharedSpecialActions = new List<ProcessType>(); // new List<string>();
            foreach (var item in entity.AvailableSharedSpecialActions)
            {
                AvailableSharedSpecialActions.Add(item);
            }

            /*  if (Properties != null)
              {
                  Properties.Clear();
              }*/

            AllegianceID = null;
            //MeleeAttackers = null;


            Body = null;

            BodyComponent body;
            if (entity.Find(out body))
            {
                Body = new Body(body.Body); // copy the body class, not the wrapping component...
                Body.ParentMemoryFact = this;
            }



            // AddProperty(StatusProperty.FunctionalScore, gameEntity.FunctionalScore);

            Intelligence intelligence;
            if (entity.Find(out intelligence))
            {
                AllegianceID = intelligence.Allegiance.ID;

                // point to the same collection as other factions!
                //MeleeAttackers = intelligence.CombatInfo.MeleeAttackers;

                //AddProperty(StatusProperty.AllegianceId, intelligence.Allegiance.ID);

                IntrinsicWeapons = intelligence.IntrinsicWeapons;
            }

            if (entity.Locomotor != null)
            {
                // entity.Renderable./*TODO DECOUPLE*/RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.All, entity.Renderable./*TODO DECOUPLE*/RenderAsModel.FinalModelScale);

                float? vehicleRoll = null, vehiclePitch = null;
                Vehicles.Vehicle vehicle;
                if (entity.Find(out vehicle))
                {
                    vehicleRoll = vehicle.Roll;
                    vehiclePitch = vehicle.Pitch;
                }


                //  MemoryModelRenderData = MemoryModelRenderData.Get(entity.GetFacingNormal(), vehicleRoll, vehiclePitch);

                Stance = entity.Stance;

                /*
                if (entity.Locomotor.LeggedLocomotor != null)
                {
                    Stance = entity.Locomotor.LeggedLocomotor.CurrentStance;                        
                }*/
            }

            /*  else if (entity.Renderable.RenderAsBillboard != null)
              {
                  // copy render info, which will be static:
                  List<FeatureQuad> quads = new List<FeatureQuad>();
                  foreach (RenderAsBillboard r in entity.Renderable.RenderAsBillboard) //TODO DECOUPLE
                  {
                      quads.Add(r.GetQuad());
                  }

                  MemoryItemRenderData = MemoryItemRenderData.Get(quads);
              }*/

            if (entity.EntityType.BiologicalType != null)
            {
                BiologicalEntity bioComponent;
                entity.Find(out bioComponent);
                CasteType = bioComponent.CasteType;                
            }

            SubstanceComponent substances;
            if (entity.EntityType.SubstancesType != null &&
                entity.Find(out substances))
            {
                SubstanceBulkAmounts = new Dictionary<SubstanceType, SubstanceAmount>();

                foreach (var item in substances.BulkAmounts)
                {
                    SubstanceBulkAmounts.Add(item.Key, new SubstanceAmount(item.Value)); // item.Value);
                }
            }

            //IsAlwaysShown = EntityType.NeverInFogOfWar();

            if (entity.TooltipEntityData != null)
            {
                // copy the data:
                TooltipEntityData = new EntityTypeTooltipInstanceData(entity.TooltipEntityData);
            }


            if (entity.CustomFields != null)
            {
                CustomFields = new Dictionary<string, PropertyResult>();
                foreach (var item in entity.CustomFields)
                {
                    CustomFields.Add(item.Key, item.Value); // we want a copy, and since PropertyResult is a struct, we get it this way
                }
            }

            /*  }
              catch (Exception e)
              {
                
                  string exceptionString = e.Message;
                  exceptionString += Entity.GetExceptionInformation(entity);
                  throw new Exception(exceptionString);
                                               
              }*/

            return true;

        }

        /*  public string GetName()
          {
              if (Name != null)
              {
                  return Name;
              }
              else return EntityType.Name;

          }*/


        public bool CanSetStockpileSettings(EntityGroupID byOwner)
        {
            return Entity.CanSetStockpileSettings(this, byOwner);
        }

        public bool CanSetTradeOfferSettings(EntityGroupID byOwner)
        {
            return Entity.CanSetTradeOfferSettings(this, byOwner);
        }

        public bool CanBeUpgraded(EntityGroupID byOwner)
        {
            return Entity.CanBeUpgraded(this, byOwner);
        }

        /*  private void AddProperty(StatusProperty property, object value)
          {
              if (Properties == null)
              {
                  Properties = new Dictionary<StatusProperty, object>();
              }

              Properties.Add(property, value);
          }*/

        public void GetContainedStatus(Entity entity)
        {
            if (contains == null)
            {
                contains = new Dictionary<EntityID, EntityID>();
            }
            contains.Add(entity.EntityID, entity.EntityID);
        }

        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();

        static MemoryFact()
        {
            exposedPropertyValueFunctions.Add("bulkForPresentation", GetBulkForPresentation);
            exposedPropertyValueFunctions.Add("hitpointLevel", GetHitpointsFraction);
            exposedPropertyValueFunctions.Add("itemPartCondition", GetCondition);
            exposedPropertyValueFunctions.Add("itemPartConditionTooltip", GetConditionTooltip);        
            exposedPropertyValueFunctions.Add("integrity", GetIntegrity);
            exposedPropertyValueFunctions.Add("progress", GetProgress);
            exposedPropertyValueFunctions.Add("inAccessible", GetInaccessible); // also in Entity!
            exposedPropertyValueFunctions.Add("homeComfortLevel", GetHomeComfortLevel);
            exposedPropertyValueFunctions.Add("replenishStatus", GetReplenishStatus);
            exposedPropertyValueFunctions.Add("replenishStatusTooltip", GetReplenishStatusTooltip);
           
        }

        // LPE: don't know what this means:
        //All IHasExposedProperties are casted to be able to call the class specific function for a certain property, 
        //this is safe because GetPropertyValue uses the global functions defined for the class in which it is used
        // However if we by mistake added a global function from another class to our dictionary then it might cause problems, but that should never happen
        public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            PropertyResult? result = null;
            PropertyResult customResult;

            if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
            {
                result = exposedPropertyValueFunctions[propertyKey].Invoke(this, getterKnowledge, parent);
            }
            else if (CustomFields != null && CustomFields.TryGetValue(propertyKey, out customResult))
            {
                result = customResult;
            }

            return result;
        }


        public void GetChildren(string key, ref List<IHasExposedProperties> listOfChildren, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null) // string filterKey, string filterPropertyValue)
        {
            switch (key)
            {
                case "bodyParts":
                    {
                        if (Body != null)
                        {
                            Body.GetBodyParts<IHasExposedProperties>(ref listOfChildren);
                        }
                        break;
                    }
                case "residents":
                    {
                        Entity.GetResidents(this, getterKnowledge, ref listOfChildren);
                        break;
                    }

                case "itemParts":
                    {

                        //   GetParts<IHasExposedProperties>(ref listOfChildren); //, filterKey);
                        break;
                    }
            }
        }

        public void SetPropertyValue(string propertyKey, PropertyResult? value)
        {
            /* don't set properties on memoryfacts. only on real entities.
             * 
            if (CustomFields == null)
            {
                CustomFields = new Dictionary<string, PropertyResult>();
            }

            CustomFields.Add(propertyKey, value);
            */
        }

       /* public string GetName()
        {
            if (Name != null)
            {
                return Name;
            }
            else
            {
                return EntityType.Name;
            }
        }*/

        public string GetDisplayName()
        {
            return Entity.GetDisplayName(this);
        }

        public Vector3 RenderedLocation
        {
            get
            {
                return Renderable.Location.Value;
            }
        }
        public string GetDefaultCaption(string propertyKey)
        {
            return EntityType.Name;
        }
        public void GetDefaultKey(out string PropertyKey)
        {
            PropertyKey = "" + EntityID;
        }

        public string KeyName
        {
            get { return EntityType.KeyName; }
        }

        public EntityID? GetEntityID()
        {
            return EntityID;
        }

        public bool GetIsSeenDirectly() //SharedKnowledge sharedKnowledge)
        {
            return false;
        }

        public string GetCaption(string captionKey)
        {
            return null;
        }



        public void Destroy()
        {
            RemoveIDEntry();

            if (storageSpaces != null)
            {
                foreach (var item in storageSpaces)
                {
                    // destroy the IDs we created:
                    item.Value.Destroy();
                }
            }

            if (MapPosition.HasValue)
            {
                The.Map.GetTile(MapPosition.Value).RemoveRememberedRootEntity(sharedKnowledge, this);
            }

            //if the entity is destroyed, we can do extra cleanup:
            if (Entity.FindByID(EntityID) == null)
            {
                // remove accessibility data from Client:
                The.Client.DestroyAccessibility(EntityID);

                // destroy the shared gathering site:
                if (GatheringSite != null)
                    GatheringSite.Destroy();
            }

        }

        #region Exposed properties

        public static PropertyResult? GetHitpointsFraction(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return Entity.GetHitpointsFractionValue(((MemoryFact)anObjectToGetValueFrom).Body);
        }


        public static PropertyResult? GetBulkForPresentation(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return Entity.GetItemBulkForPresentation((MemoryFact)anObjectToGetValueFrom);
        }

        public static PropertyResult? GetCondition(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return Entity.GetCondition((MemoryFact)anoObjectToGetValueFrom);

        }

        public static PropertyResult? GetConditionTooltip(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return Entity.GetConditionTooltip((MemoryFact)anoObjectToGetValueFrom);

        }

        public static PropertyResult? GetReplenishStatus(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return Entity.GetReplenishStatus(getterKnowledge, (IKnownEntityData)anObjectToGetValueFrom);          
        }

        public static PropertyResult? GetReplenishStatusTooltip(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
        {
            return Entity.GetReplenishStatusTooltip(getterKnowledge, (IKnownEntityData)anObjectToGetValueFrom);
        }

        public static PropertyResult? GetHomeComfortLevel(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return Entity.GetHomeComfortLevel((MemoryFact)anoObjectToGetValueFrom);

        }

        public static PropertyResult? GetProgress(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return ((MemoryFact)anObjectToGetValueFrom).GetProgress();
        }

        private PropertyResult? GetProgress()
        {
            if (Progress.HasValue)
            {
                return new PropertyResult() { NumberResult = Progress.Value };
            }
            else return null;
        }

        public static PropertyResult? GetIntegrity(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return Entity.GetIntegrity((MemoryFact)anObjectToGetValueFrom);
        }

        public static PropertyResult? GetInaccessible(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            return ((MemoryFact)anObjectToGetValueFrom).GetInaccessible();
        }

        public PropertyResult? GetInaccessible()
        {

            PropertyResult result = GetInaccessibleStatus(EntityID);

            return result;
        }

        public static PropertyResult GetInaccessibleStatus(EntityID entityID)
        {
            bool inaccessible, blockedByThreat, blockedByStance;

            The.Client.GetFeedback(entityID, out inaccessible, out blockedByThreat, out blockedByStance);

            PropertyResult result = new PropertyResult();


            // encode the result as a number:
            if (blockedByThreat || blockedByStance)//TODO: blockedByStance??
            {
                result.NumberResult = 0.25f;
            }
            else if (inaccessible)
            {
                result.NumberResult = 0.75f;
            }
            else
            {
                result.NumberResult = 0f;
            }

            return result;
        }


        #endregion


        public bool DeprecateIfNeeded(Entity detectingEntity, TerrainTile.DeprecateDistance? deprecateDistance)
        {
            // many items require the agent to get reasonably near until he will accept that the item is truly gone... (it might still be in the vicinity, theoretically)
            if (isDeprecated == false &&
                (deprecateDistance == null ||
                deprecateDistance == TerrainTile.DeprecateDistance.Near ||
                EntityType.MemoryFactIsDeprecatedInstantly()))
            {
                IsDeprecated = true;

               
                // don't log this for parts... too much clutter.
                if (PartOfID == null && detectingEntity != null
                    && (EntityType.ItemType == null || EntityType.Category.IsWaste == false))
                {
                    string text;
                    if (EntityType.IntelligenceType != null)
                    {
                        // for critters, use this phrase:
                        text = "no longer has {0} in view";
                    }
                    else
                    {
                        // for immobile entities, phrase it a bit differently:
                        text = "cannot see {0} where it used to be";
                    }


                    The.Client.AddLogEvent(detectingEntity.Intelligence.Allegiance, The.Client.Log.GeneralEvent, detectingEntity,
                        string.Format(text, EntityType.Name.ToLower(Config.Culture)));
                }
                

                return true;
            }

            return false;
        }

        #region ILookup

        private MemoryFactID id = MemoryFactID.Invalid;
        static MemoryFactID IDCounter = MemoryFactID.First;

        public MemoryFactID ID
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

        public MemoryFactID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= MemoryFactID.Max)
            {
                throw new Exception("Astounding, MemoryFactID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public MemoryFactID SnapshotID(Snapshotter sn, MemoryFactID id)
        {
            return (MemoryFactID)sn.DoEnum(id);
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
            if (ID != MemoryFactID.Invalid)
                LookUp<MemoryFact, MemoryFactID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = MemoryFactID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<MemoryFact, MemoryFactID>.Remove(this);
        }

        void ILookUp<MemoryFact, MemoryFactID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = MemoryFactID.First;
        }


        void ILookUp<MemoryFact, MemoryFactID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<MemoryFact, MemoryFactID>.Create();
        }


        #endregion


        #region ISnapshot

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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            IDCounter = sn.DoEnum(IDCounter);
            id = SnapshotID(sn, id);

            this.AccessPoint = sn.DoVector3Nullable(AccessPoint);
            this.ammoItems = sn.DoDictionary(ammoItems);
            this.AllegianceID = sn.DoEnumNullable(AllegianceID);

            this.DebugLog = (DebugLog)sn.DoISnapshot(DebugLog);

            this.Body = (Body)sn.DoISnapshot(Body);
            this.BoundingRadius3D = sn.DoFloat(BoundingRadius3D);
            this.Bulk = sn.DoFloat(Bulk);
            this.Condition = sn.DoDoubleNullable(Condition);
            this.ConditionChangeSpeed = sn.DoFloatNullable(ConditionChangeSpeed);
            this.Integrity = sn.DoFloatNullable(Integrity);
            this.ContainedBy = sn.DoEntityIDNullable(ContainedBy);
            //  this.ContainedEntityTotals = sn.DoDictionary(ContainedEntityTotals);
            this.ContainedEntitiesByType = sn.DoMultiMap(ContainedEntitiesByType);
            this.ContainedUpgrades = sn.DoDictionary(ContainedUpgrades);
            this.contains = sn.DoDictionary(contains);
            this.OfferedEntitiesByType = sn.DoMultiMap(OfferedEntitiesByType);
            this.CurrentMaximumSpeed = sn.DoFloatNullable(CurrentMaximumSpeed);
            this.EntityID = sn.DoEntityID(EntityID);
            this.EntityType = sn.DoGameData(EntityType);
            this.FacingNormal = sn.DoVector3(FacingNormal);
            this.flipHorizontally = sn.DoBool(flipHorizontally);
            //  this.freeDriversSlot =  // TODO??
            //this.listOfCargoSlotsForLoading // TODO??
            //this.listOfCargoSlotsForUnloading // TODO??

            this.CustomFields = sn.DoDictionary(CustomFields);
            this.fuel = sn.DoFloatNullable(fuel);
            this.FunctionalScore = sn.DoDoubleNullable(FunctionalScore);
            this.snapshotGatheringSite = sn.SnapshotID<GatheringSite, GatheringSiteID>(GatheringSite);
            this.HasItemStorage = sn.DoBool(HasItemStorage);
            this.OwnedBy = sn.DoEnumNullable(OwnedBy);
           // this.inUseBy = sn.DoDictionary(inUseBy);         
            this.isCompleted = sn.DoBool(isCompleted);
            this.isStarted = sn.DoBoolNullable(isStarted);
            this.isDeprecated = sn.DoBool(isDeprecated);
         //   this.isEnclosed = sn.DoBool(isEnclosed);
            this.IsMoving = sn.DoBoolNullable(IsMoving);
            this.IsPrepared = sn.DoBoolNullable(IsPrepared);
            this.isWeatherProof = sn.DoBool(isWeatherProof);
            this.loadedVehicleSpeed = sn.DoFloat(loadedVehicleSpeed);
            this.location = sn.DoVector3Nullable(location);
            this.MapPosition = sn.DoPointNullable(MapPosition);
            this.Name = sn.DoString(Name);
            this.NoOfRounds = sn.DoInt32Nullable(NoOfRounds);
            this.notOnboardDrivenVehicle = sn.DoBool(notOnboardDrivenVehicle);
            this.NutrientBulkAmounts = sn.DoDictionary(NutrientBulkAmounts);
            this.Processes = sn.DoList(Processes);
            this.AvailableSharedSpecialActions = sn.DoList(AvailableSharedSpecialActions);
            this.IntrinsicWeapons = sn.DoDictionary(IntrinsicWeapons);
           
            this.Site = sn.DoEnumNullable(Site);
            this.PartIsBroken = sn.DoBool(PartIsBroken);
                    
            this.PartOfID = sn.DoEnumNullable(PartOfID);
            this.PartIDs = sn.DoList(PartIDs);
            this.ParentEntityID = sn.DoEnumNullable(ParentEntityID);
            this.RootEntityID = sn.DoEnum(RootEntityID);

            this.progress = sn.DoFloatNullable(progress);
            this.Replenishes = sn.DoEntityIDNullable(Replenishes);
            this.UpgradeFor = sn.DoEntityIDNullable(UpgradeFor);
            this.Residents = sn.DoInt32Nullable(Residents);
            this.ComfortLevel = sn.DoFloatNullable(ComfortLevel);
            this.Households = sn.DoList(Households);
            this.Rotation = sn.DoFloat(Rotation);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                this.snapshotAllegianceID = sharedKnowledge.Allegiance.ID;
            }
            this.snapshotAllegianceID = sn.DoEnum(snapshotAllegianceID);


            this.assignedToJob = sn.DoEnumNullable(assignedToJob);


            this.Stance = sn.DoGameData(Stance); // sn.DoEnumNullable(Stance);

            if (sn.mode != Snapshotter.Mode.Load)
            {
               /* if (storageSpaces != null)
                {
                    snapshotStorageSpaces = storageSpaces.ToDictionary(s => s.Key, s => s.Value.ID);
                }

                if (tradeOfferStorageSpaces != null)
                {
                    snapshotTradeOfferStorageSpaces = tradeOfferStorageSpaces.ToDictionary(s => s.Key, s => s.Value.ID);
                }*/

                if (CasteType != null)
                {
                    casteTypeKey = CasteType.KeyName;
                }
            }

            casteTypeKey = sn.DoString(casteTypeKey);

            storageSpaces = sn.DoDictionary(storageSpaces);
            tradeOfferStorageSpaces = sn.DoDictionary(tradeOfferStorageSpaces);
            /*
            snapshotStorageSpaces = sn.DoDictionary(snapshotStorageSpaces);
            snapshotTradeOfferStorageSpaces = sn.DoDictionary(snapshotTradeOfferStorageSpaces);*/

            if (sn.mode != Snapshotter.Mode.Load
               && The.Client != null
               && Renderable != null)
            {
                snapshotRenderable = Renderable.GetFieldsToSnapshot(); // ugly, but necessary..
            }
            snapshotRenderable = (Renderable.SnapshotRenderable)sn.DoISnapshot(snapshotRenderable);

            this.StoredPermanentlyIn = sn.DoStorageTargetNullable(StoredPermanentlyIn);
           // this.IsOfferedForTrade = sn.DoBool(IsOfferedForTrade);
            /*
            this.StoredPermanentlyCondition = sn.DoGameData(StoredPermanentlyCondition);
            this.StoredPermanentlyIn = sn.DoEntityIDNullable(StoredPermanentlyIn);
            this.StoredPermanentlyStorageID = sn.DoEnumNullable(StoredPermanentlyStorageID);
            this.OfferedForTradeStorageID = sn.DoEnumNullable(OfferedForTradeStorageID);
            */

            this.StrengthRating = sn.DoFloatNullable(StrengthRating);
            this.SubstanceBulkAmounts = sn.DoDictionary(SubstanceBulkAmounts);
            // this.snapshotThreatGroup = Snapshotter.GetID<ThreatGroup, ThreatGroupID>(ThreatGroup);
            this.snapshotThreatGroup = sn.SnapshotID<ThreatGroup, ThreatGroupID>(ThreatGroup); // works???
            this.TimeStampInSecondsOfGameTime = sn.DoDouble(TimeStampInSecondsOfGameTime);
            this.ToBeDeletedOnTimeStampInSecondsOfGameTime = sn.DoDoubleNullable(ToBeDeletedOnTimeStampInSecondsOfGameTime);
            this.tooltipEntityData = (EntityTypeTooltipInstanceData)sn.DoISnapshot(tooltipEntityData);
            this.TopLeftMapPosition = sn.DoPointNullable(TopLeftMapPosition);
            this.TotalItemStorageCapacity = sn.DoFloatNullable(TotalItemStorageCapacity);
            this.TotalStored = sn.DoFloatNullable(TotalStored);

            this.repairPackage = (RepairPackage)sn.DoISnapshot(repairPackage);

            sn.Postpone(listOfCargoSlotsForUnloading);
            sn.Postpone(listOfCargoSlotsForLoading);
            sn.Postpone(freeDriversSlot);

            sn.Ignore(CasteType);
            sn.Ignore(exposedPropertyValueFunctions);
            sn.Ignore(sharedKnowledge);
           /* sn.Ignore(storageSpaces);
            sn.Ignore(tradeOfferStorageSpaces);*/

            return this;
        }




        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            // AssignedToJob = LookUp<Job, JobID>.FindByID(snapshotAssignedToJob);
            ThreatGroup = LookUp<ThreatGroup, ThreatGroupID>.FindByID(snapshotThreatGroup);

            sharedKnowledge = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegianceID).SharedKnowledge;

            if (snapshotGatheringSite != null && snapshotGatheringSite.HasValue)
            {
                GatheringSite = LookUp<GatheringSite, GatheringSiteID>.FindByID(snapshotGatheringSite);
            }
            if (Body != null)
            {
                Body.LoadPostProcess(sn);
            }
                     

            GatheringSite = LookUp<GatheringSite, GatheringSiteID>.FindByID(snapshotGatheringSite);

            if (tradeOfferStorageSpaces != null)
            {
                foreach (var item in tradeOfferStorageSpaces)
                {
                    item.Value.LoadPostProcess(sn);
                }
            }

            if (storageSpaces != null)
            {
                foreach (var item in storageSpaces)
                {
                    item.Value.LoadPostProcess(sn);
                }
            }            

            if (EntityType.BiologicalType != null)
            {
                CasteType = EntityType.BiologicalType.Castes.FirstOrDefault(c => c.KeyName.Equals(casteTypeKey));
            }

            #region RE-INIT - recreate the stuff that was not snapshotted:

            showStatusRegulator = Entity.CreateShowStatusIconRegulator("MemoryFact", EntityType);


            if (snapshotRenderable != null) //EntityType.RenderableType != null)
            {
                // The.Client.SetRenderableOnMemoryFact(this, entity, sharedKnowledge.Allegiance);

                Renderable = RenderableFactory.Produce(snapshotRenderable, this);
                //   Renderable = RenderableFactory.Produce(this, EntityType.RenderableType, snapshotRenderable);

                // Renderable.Initialize(); // this creates a new RenderAsModel.AnimatedModel!

                Renderable.UpdateAnimationConditionState(); // make sure the correct animation tracks are running - probably needed since the animator is fresh?       

                // make sure that matrices, locations etc have proper values:
                Renderable.ComputeMatricesForDrawing();
            }

            

            #endregion
        }

        #endregion
    }
}
