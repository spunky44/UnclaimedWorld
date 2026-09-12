using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Trade;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Combat;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.Overland.Missions.Templates;
using System.Collections.Specialized;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Overland;

namespace UWGame.SimSide.Entities
{
   
    public enum EntityGroupID : long // ulong
    {
        Invalid = long.MaxValue, // uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// God class..?!? Perhaps split this class into composition subclasses, not all owners will require all data
    /// this loosely defined class contains the superset of all collections/objects that entities want to keep track of, as well as jobs that may work with those objects.
    /// Also policy/settings.
    /// has auxiliary lists to increase iteration performance.
    /// 
    /// The entity lists contain ids for destroyed entities! the ids are used for looking up memory facts.
    ///   
    /// Referenced in: SharedKnowledge, Expedition, Household, Person
    /// 
    /// </summary>
    public class EntityGroup : ISnapshot, ILookUp<EntityGroup, EntityGroupID>
    {        
       
        public IHasEntityGroup Parent;
        private HasEntityGroupID snapshotParent;

      
        #region Entities
        // Entities ********
      
        /// <summary>
        /// The main collection, contains all tracked entities
        /// The extra collections below contain subsets of these.     
        /// 
        /// </summary>
        private Dictionary<EntityID, EntityID> allEntities = new Dictionary<EntityID, EntityID>();

        /// <summary>
        /// all entities, sorted by type.
        /// </summary>
        public Dictionary<EntityType, List<EntityID>> AllEntities = new Dictionary<EntityType,List<EntityID>>();


        /// <summary>
        /// only ItemType entities are in this collection.
        /// </summary>
        private Dictionary<EntityType, List<EntityID>> items = new Dictionary<EntityType, List<EntityID>>();      
        public Dictionary<EntityType, List<EntityID>> Items { get { return items; } }

        private List<EntityID> vehicles = new List<EntityID>();

        public Dictionary<EntityType, OwnerAmmoOfType> AmmoItems = new Dictionary<EntityType, OwnerAmmoOfType>();

        private bool foodIsDirty = false;

     //   private bool totalDirectOrderJobsDirty = true;

      //  private int totalJobs = 0;

        /// <summary>
        /// to snapshot this callback, we need a dictionary of method ids mapping to delegate functions. 
        /// The owner of the function should post it in a dictionary in its ctor.
        /// </summary>
      //  Predicate<EntityType> isEatable;

       // public delegate void UpdateFoodItemsDelegate(Dictionary<EntityType, List<EntityID>> food);

      //  private UpdateFoodItemsDelegate UpdateFoodItems;
      //  Action<Dictionary<EntityType, List<EntityID>>> UpdateFoodItems;

        /// <summary>
        /// this list contains items that can be cosumed or from which a consumable item can be extracted by at least one of the parent's members
        /// </summary>
        private Dictionary<EntityType, List<EntityID>> food = new Dictionary<EntityType, List<EntityID>>();
        public Dictionary<EntityType, List<EntityID>> Food
        {
            get
            {
                if (foodIsDirty)
                {         
                    UpdateFoodItems(); 
                    foodIsDirty = false;
                }

                return food;
            }
        }

        
        public Dictionary<EntityType, List<EntityID>> Structures = new Dictionary<EntityType, List<EntityID>>();

        public Dictionary<EntityType, List<EntityID>> Communicators = new Dictionary<EntityType, List<EntityID>>();

        /// <summary>
        /// NEW: These items are tools with a process that can fulfill a need... to be used in a new evaluator
        /// </summary>
        public Dictionary<EntityType, List<EntityID>> FulfillsNeeds = new Dictionary<EntityType, List<EntityID>>();
      

        public List<EntityID> Vehicles { get { return vehicles; } }

        /// <summary>
        /// iteration list for EvaluateAttackJobs
        /// </summary>
        public Dictionary<AttackType, List<EntityID>> WeaponsByAttackType = new Dictionary<AttackType, List<EntityID>>();

        #region Orders for managers

        public Dictionary<EntityID, Stockpile> StructureStockpiles = new Dictionary<EntityID, Stockpile>();

        public Dictionary<EntityID, Stockpile> TerminalTradeOffers = new Dictionary<EntityID, Stockpile>();

        /// <summary>
        /// Upgrade orders.
        /// an entry exists for every entity that can be upgraded.
        /// contains null for empty slots!
        /// </summary>
        public Dictionary<EntityID, Dictionary<UpgradeCategory, EntityType>> Upgrades = new Dictionary<EntityID,Dictionary<UpgradeCategory,EntityType>>();

        #endregion

        /// <summary>
        /// only some of these may allow buy/sell
        /// </summary>
        public Dictionary<TerminalType.TypesOfTerminal, List<EntityID>> Terminals = new Dictionary<TerminalType.TypesOfTerminal, List<EntityID>>();


        #endregion

        #region Managers and policies

        /// <summary>
        /// moved this to EntityGroup
        /// </summary>
         public ProductionOrders ProductionOrders;



        /// <summary>
        /// sets prices on items for sale. 
        /// 
        /// Person, Household, Expedition defines this - Allegiance does not.
        /// </summary>
        public TradeManager TradeManager;
       

        public EntityGroupPolicy Policy;

        /// <summary>
        /// this should also work on items we do not own (for critters)
        /// </summary>
        public HaulingJobManager HaulingJobManager;
        private CyclableID? snapshotHaulingJobManager;

        public OtherJobManager OtherJobManager;
        private CyclableID? snapshotOtherJobManager;

        public HuntingJobManager HuntingJobManager;
        private CyclableID? snapshotHuntingJobManager;

        #endregion

        // Jobs - are null for SharedKnowledge, except for threat jobs ******
        #region Jobs
                
        /// <summary>
        /// master list - has all jobs
        /// </summary>
        private List<JobID> allJobs = new List<JobID>();

        // special lists - only have some jobs
        // why not use the derived classes..?
        public List<Job> HaulingJobs = new List<Job>(); 
        private List<JobID> snapshotHaulingJobs;

        /// <summary>
        /// key is RequiredItemType. Are also in HaulingJobs
        /// </summary>
        public Dictionary<EntityType, List<HaulingJobAnyItemOfType>> HaulingJobsAnyItemOfType = new Dictionary<EntityType,List<HaulingJobAnyItemOfType>>();
        private Dictionary<EntityType, List<JobID>> snapshotHaulingJobsAnyItemOfType;


        /// <summary>
        /// Are also in HaulingJobs
        /// used for inertia. add a number to the score of the current haul target.
        /// keep a permanent collection? will avoid 500^2 lookups
        /// </summary>      
        public Dictionary<EntityID, HaulingJobSpecificItem> SpecificHaulingJobs = new Dictionary<EntityID, HaulingJobSpecificItem>(); // #HAULMANAGERFIX
        private Dictionary<EntityID, JobID> snapshotSpecificHaulingJobs;



        /// <summary>
        /// jobs for processes without outputs, an also non-process jobs like hunting
        /// salvage jobs, establisgh farm plots, replenish, hunting, upgrades etc. NOT repair!
        /// 
        /// Currently, there is a link from an entity -> Owner -> Jobs
        /// If we move this, how do we link them together..?
        /// </summary>
        private List<Job> otherJobs = new List<Job>();
        public List<Job> OtherJobs { get { return otherJobs; } }
        private List<JobID> snapshotOtherJobs;
       

        /// <summary>
        /// this is production (process) jobs listed by output - also gather jobs!!!
        /// </summary>
      //  public Dictionary<EntityType, List<ProcessJob>> ProductionJobs { get { return productionJobs; } }
        public Dictionary<EntityType, List<ProcessJob>> ProductionJobs = new Dictionary<EntityType, List<ProcessJob>>();
        private Dictionary<EntityType, List<JobID>> snapshotProductionJobs;

        /// <summary>
        /// a filtered subset of the above, used by JobManager to manage standing orders etc.
        /// Filtered by process type.      
        /// 
        /// Contains: 
        /// 1. Standing order inventry jobs
        /// 2. Directly ordered inventry jobs
        /// 
        /// 3. Standing order gather jobs
        /// 4. Directly ordered gather jobs
        /// 
        /// The JobManager should only remove 1 + 2 + 3 when balancing.
        /// Directly ordered gather jobs are in this list because they should still count against the total, but the JobManager should not touch them!
        /// </summary>
        public Dictionary<EntityType, List<ProcessJob>> ManagedProductionJobs = new Dictionary<EntityType, List<ProcessJob>>();
        private Dictionary<EntityType, List<JobID>> snapshotManagedProductionJobs;


        /// <summary>       
        /// and by input
        /// </summary>
        public Dictionary<EntityType, List<ProcessJob>> ProductionJobsByInput = new Dictionary<EntityType, List<ProcessJob>>();
        private Dictionary<EntityType, List<JobID>> snapshotProductionJobsByInput;

        /// <summary>
        /// should be sorted by completion timepoint - don't use SleepyUpdater since we want to group them also
        /// </summary>
        public List<Job> UnattendedProcessJobs = new List<Job>();
        private List<JobID> snapshotUnattendedProcessJobs;
      
        /// <summary>
        /// grouped by ActingOn
        /// </summary>
        public Dictionary<EntityID, List<ProcessJob>> RepairJobs = new Dictionary<EntityID, List<ProcessJob>>();
        private Dictionary<EntityID, List<JobID>> snapshotRepairJobs;


        private List<Job> scoutingJobs = new List<Job>();
        private List<JobID> snapshotScoutingJobs;
       
        private List<Job> findPreyJobs = new List<Job>();
        private List<JobID> snapshotFindPreyJobs;
       
        private List<Job> patrolJobs = new List<Job>();
        private List<JobID> snapshotPatrolJobs;

        private List<Job> attackAreaJobs = new List<Job>();
        private List<JobID> snapshotAttackAreaJobs;

        /// <summary>
        /// a grouping of the other jobs lists, for iterating
        /// </summary>
        private List<Job> variableMaxTakerJobs = new List<Job>();
        private List<JobID> snapshotVariableMaxTakerJobs;


        public List<Job> ScoutingJobs
        {
            get { return scoutingJobs; }   
        }

        public List<Job> FindPreyJobs
        {
            get { return findPreyJobs; }
        }

        public List<Job> PatrolJobs
        {
            get { return patrolJobs; }
        }

        public List<Job> AttackAreaJobs
        {
            get { return attackAreaJobs; }
        }

        public List<Job> VariableMaxTakerJobs
        {
            get { return variableMaxTakerJobs; }
        }

        public List<Job> CheckProcessJobs = new List<Job>();
        private List<JobID> snapshotCheckProcessJobs;

        // public Dictionary<JobID, JobID> ProcessJobsToCheckingJobs = new Dictionary<JobID, JobID>();


        /// <summary>
        /// used for looking up checking jobs in range
        /// </summary>
        public PointQuadTree<JobID> CheckProcessJobsQuadTree;
        List<Pair<JobID, Vector2>> snapshotCheckProcessJobsQuadTree;

               
        /// <summary>      
        /// only allegiance will use these. Theay are placed here in order to handle jobs add/removal in a uniform way.       
        /// </summary>
        public List<Job> ThreatJobs = new List<Job>();
        public Dictionary<EntityID, Job> ThreatJobsByTarget = new Dictionary<EntityID,Job>();
        private List<JobID> snapshotThreatJobs;

        /// <summary>
        /// vermins...
        /// </summary>
        public List<Job> AssetThreatJobs = new List<Job>();
        public Dictionary<EntityID, Job> AssetThreatJobsByTarget = new Dictionary<EntityID, Job>();       
        private List<JobID> snapshotAssetThreatJobs;
        
        #endregion

        /// <summary>
        /// Zones and Jobs are linked...
        /// </summary>
        public List<Zone> Zones = new List<Zone>();
        private List<ZoneID> snapshotZones;

        // Other **********

       
       

        /// <summary>
        /// placed in this class, because even SharedKnowledge (Allegiance) uses it...
        /// 
        /// Allegiance, Person, Household defines this - Expedition does not.
        /// </summary>
     //   public decimal? TradeCredits { get; set; }

      
        private List<Activity> activities = new List<Activity>();
        public List<Activity> Activities { get { return activities; } set { activities = value; } }

        #region Production

        /// <summary>
        /// services the Jobs in the EntityGroup
        /// shared by all process jobs with this output
        /// 
        /// recomputed periodically OR when direct orders are placed for new output types
        /// </summary>
        public Dictionary<EntityType, float> ProductionImportance = new Dictionary<EntityType, float>();

        /// <summary>
        /// Use same number for all food types
        /// 
        /// recomputed periodically OR when direct orders are placed for food
        /// </summary>
        public float? FoodProductionImportance;

        /// <summary>
        /// generic food items needed per day
        /// 
        /// Recomputed when members change
        /// </summary>
        private float foodConsumeRate;

        #endregion


        public EntityGroup(IHasEntityGroup parent, bool manageTrade, bool manageProduction) 
        {
            AddToLookup();

            Parent = parent;

            Allegiance allegiance = GetAllegiance();
            if (allegiance != null) 
            {
                if (manageTrade 
                    && allegiance.RepresentativeEntityType.Person != null
                    && !allegiance.Site.IsPlaySite)
                {
                    TradeManager = new Trade.TradeManager(this);
                    //HumanExpeditionActivities = new HumanExpeditionActivities();
                }

                if (manageProduction &&
                    allegiance.RepresentativeEntityType.IntelligenceType.CanProduce == true)
                {
                    ProductionOrders = new ProductionOrders();
                }
            }

            Policy = new EntityGroupPolicy();


            CheckProcessJobsQuadTree = new PointQuadTree<JobID>(new Vector2(The.Map.MapWorldWidth, The.Map.MapWorldHeight), 10, 7);
         
  
        }

        public EntityGroup()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       

        }

       


        /// <summary>
        /// set this dirty when the items in the food list no longer corresponds to the food process types/eating habits in the owning group
        /// </summary>
        public void SetFoodDirty()
        {
            foodIsDirty = true;
        }

        /// <summary>
        /// here we should unregister any managers..
        /// </summary>
        public void Destroy()
        {
            if (HaulingJobManager != null)
            {
                HaulingJobManager.Destroy();
            }

            if (OtherJobManager != null)
            {
                OtherJobManager.Destroy();
            }

            if (HuntingJobManager != null)
            {
                HuntingJobManager.Destroy();
            }

            if (TradeManager != null)
            {
                TradeManager.Destroy();
            }

            DestroyJobs();

            LookUp<EntityGroup, EntityGroupID>.Remove(this);
        }


        public float GetImportance(EntityType mainOutput)
        {
            float importance;
            if (GetAllegiance().IsEatable(mainOutput))
            {
                return FoodProductionImportance ?? EntityGroup.NeutralImportance;
            }
            else if (ProductionImportance.TryGetValue(mainOutput, out importance))
            {
                return importance;
            }
            else return EntityGroup.NeutralImportance;


        }

        private void DestroyJobs()
        {
            // destroy all jobs to remove their ID entries:
            // with a master list this is much easier...
            for (int i = allJobs.Count - 1; i >= 0; i--) // iterate backwards because Destroy will be removing from this list
            {
                JobID jobID = allJobs[i];
                Job job = LookUp<Job, JobID>.FindByID(jobID);

                job.Destroy(true);
            }
        }


        public void RecomputeFoodProductionImportance(/*HashSet<EntityType> foodUnderProduction,*/ Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
        {
              /// score all food, give all food types the same importance
              // consumtion rate cannot use consume stats alone if stockpile is empty...
              // for food, consumption rate is based on needs since it is constant (based on members)


           
           // int totalStockpiledAndProduced = 0;
            int totalStockpiled = 0;

            float totalProductionRate = 0f;

           // float totalStockpiledAndProduced = 0f;
           /* foreach (var entityType in foodUnderProduction)
            {
                int currentStockAmount;
                float productionRate;

                ComputeProductionRate(entityType, out currentStockAmount, out totalStockpiledAndProduced, out productionRate, allAvailableItems);

                totalProductionRate += productionRate;
                totalStockpiled += currentStockAmount;
                totalStockpiledAndProduced += totalStockpiledAndProduced;
            }

            // add stockpiled food not considered:
            foreach (var item in food)
            {
                if (!foodUnderProduction.Contains(item.Key))
                {
                    totalStockpiled += item.Value.Count; 
                    totalStockpiledAndProduced += item.Value.Count;
                }
            }*/

            float interval = GameData.Instance.AIConstants.IntervalInDaysForComputingProductImportance;

            DateAndTime.TimeDateYear to = The.Sim.DateAndTime.CurrentTimeDateYear;
            DateAndTime.TimeDateYear from = to;
            from.AddTime(-interval);

            foreach (var item in food)
            {

                totalStockpiled += GetCurrentStockAmount(item.Key, allAvailableItems);

               /* totalStockpiled += item.Value.Count;
                totalStockpiledAndProduced += item.Value.Count;
                */
                totalProductionRate += ComputeEventRate(item.Key, from, to, interval, ProductionStatistics.StatTypes.Produced);
               
            }

            double timeLeftScore = ScoreHowLongStocksWillLast(totalStockpiled, totalProductionRate, foodConsumeRate);
            double urgencyScore = 1d - timeLeftScore;

            double survivalScore = 1d;
            double score = CalculateImportance(urgencyScore, survivalScore);
            

            FoodProductionImportance = (float)score;

        }



        /// <summary>
        /// call this when members change
        /// </summary>
        public void RecomputeFoodConsumeRate()
        {
            // iterate members, get their daily needs.
            // convert to an amount of standardized food items
            // we can cache this for constant members.


            ICanIterateEntities canIterate = Parent as ICanIterateEntities;

            Dictionary<NeedType, float> totalNeeeds = new Dictionary<NeedType, float>();
            canIterate.IterateMembers(e => GetFoodNeeds(e, totalNeeeds));


            int maxItemsNeeded = 0;
            EntityType genericFoodItem = GameData.Instance.AIConstants.GenericFoodItem;
            foreach (var item in totalNeeeds)
            {
                if (item.Key.FoodNeedType.IsEssential)
                {
                    FoodNutrientAmount amount = genericFoodItem.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes.FirstOrDefault(n => n.Nutrient == item.Key.FoodNeedType.FoodNutrientType);
                    if (amount != null)
                    {
                        float amountPerItem = amount.Amount * genericFoodItem.ItemType.MaximumBulk.Value;

                        int itemsNeeded = (int)(item.Value / amountPerItem);

                        if (itemsNeeded > maxItemsNeeded)
                        {
                            maxItemsNeeded = itemsNeeded;
                        }
                    }
                }
            }

            foodConsumeRate = maxItemsNeeded;
        }

        private void GetFoodNeeds(Entity entity, Dictionary<NeedType, float> totalNeeeds)
        { 
            if (entity.Intelligence.IsIndependent()
                && entity.EntityType.BiologicalType != null)
            {
                foreach (var item in entity.BiologicalEntity.Needs.NeedsList)
                {
                    if (item.Value.FoodNeed != null)
                    {
                        float currentTotal = 0f;
                        totalNeeeds.TryGetValue(item.Value.NeedType, out currentTotal);

                        currentTotal += item.Value.FoodNeed.TotalNeededNutrientBulk; //  .NeedType.DecreasePerDay 
                        totalNeeeds[item.Value.NeedType] = currentTotal;
                    }
                }                
            }
        }


        /// <summary>
        /// should only count direct order jobs, not standing order jobs (which the JobManager is responsible for)
        /// </summary>
       /* public int TotalDirectOrderProductionJobs
        {
            get
            {
                if (totalDirectOrderJobsDirty)
                {
                    totalJobs = 0;

                    EntityType entityType;

                    foreach (var item in ProductionJobs)
	                {
                        entityType = item.Key;
                        ProductionOrder order;
                        if (ProductionOrders.Orders.TryGetValue(entityType, out order) && order.ProductionJobsToComplete > 0)
                        {
                            totalJobs += item.Value.Count;
                        }              
                    }

                    //totalJobs += ProductionJobs.Sum(p => p.Value.Count);

                    totalDirectOrderJobsDirty = false;
                }

                return totalJobs;
            }
        }*/

       /* public int TotalProductionJobs
        {
            get
            {
                if (totalJobsDirty)
                {
                    totalJobs = 0;
                    totalJobs += ProductionJobs.Sum(p => p.Value.Count);

                    totalJobsDirty = false;
                }

                return totalJobs;
            }
        }*/

        public static int GetMaximumJobsBeforeWarning(IHasEntityGroup /* Expedition*/ expedition)
        {
            return expedition.NoOfWorkers * GameData.Instance.AIConstants.JobsPerWorkerCap; // .JobsPerWorkerToTriggerWarning;
        }

        public static Vector3 GetFreeGroundLocation(IHasEntityGroup hasEntityGroup)
        {
            // TODO: do the influence map thing... avoid other people and jobs...
            SubtileLayers terrain = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];

            if (!The.Map.SubtileIsCompletelyBlocked(terrain, MapManager.WorldPosToSubtile(hasEntityGroup.Location.Value))) // Center)))
            {
                return hasEntityGroup.Location.Value; // Center;
            }
            else
            {
                List<Vector2> locations = UniformPoissonDiskSampler.SampleCircle(hasEntityGroup.Location.Value.ToVector2(), 60f, 16f);

                Vector2 location;
                for (int i = 0; i < locations.Count; i++)
                {
                    location = locations[i];
                    if (!The.Map.SubtileIsCompletelyBlocked(terrain, MapManager.WorldPosToSubtile(location)))
                    {
                        return new Vector3(location, 0f);
                    }
                }

                return new Vector3(locations[0], 0f); // a necessary default - a blocked location is returned.
            }
        }

        /// <summary>
        /// if the parent can own, returns its ID
        /// </summary>
        /// <returns></returns>
        public OwnerID? GetOwnerID()
        {
            OwnerID? ownerID = null;
            IOwner iOwner = Parent as IOwner;
            if (iOwner != null)
            {
                ownerID = iOwner.ID;
            }

            return ownerID;
        }

        /// <summary>
        /// when the group members' food abilities change, we have to re-assign all the items to re-categorize what is food and what is not - happily, they are already grouped by type.
        /// </summary>
        private void UpdateFoodItems() //Dictionary<EntityType, List<EntityID>> Items, Dictionary<EntityType, List<EntityID>> food, Predicate<EntityType> isEatable)
        {
            
            EntityType entityType;
            List<EntityID> listOfItems;
            foreach (var list in Items) // iterate over the general collection of items
            {
                entityType = list.Key;
                listOfItems = list.Value;

                List<EntityID> existingFoodItems;
                food.TryGetValue(entityType, out existingFoodItems);

                // the Food collection is organized by EntityType
                // determine valid food types:

                if (Parent.IsEatable(entityType)) 
                {
                    // make sure that these items are in the Food list:                    
                    if (existingFoodItems == null || existingFoodItems.Count != listOfItems.Count)
                    {
                        // they are not in the list..
                      
                        if (existingFoodItems == null)
                        {
                            existingFoodItems = new List<EntityID>();
                            food.Add(entityType, existingFoodItems);
                        }
                        existingFoodItems.Clear();

                        // let's copy the items instead of just the pointer to the list, since it's safer:
                        existingFoodItems.AddRange(listOfItems);
                    }
                }
                else
                {
                    // not eatable.
                    // we want to remove these items from the Food list:
                    if (existingFoodItems != null && existingFoodItems.Count > 0)
                    {
                        // the food list contains items. just remove them:                        
                        existingFoodItems.Clear();
                    }
                }
            }

            //foodIsDirty = false;
        }

        public bool StandingOrderJobIsNeeded(EntityType entityType)
        {
            ProductionOrder orders;
            if (ProductionOrders.Orders.TryGetValue(entityType, out orders))
            {
                int amountInStore = CountAvailableItems(entityType);

                if (orders.AmountToKeepInStore.HasValue && orders.AmountToKeepInStore.Value > amountInStore)
                {
                    return true;
                }
            }

            return false;
        }

        public void SpawnOthersiteStartingStructures(string structureProfileKey)
        {
            StructuresProfile profile = GameData.Instance.AllStructuresProfiles[structureProfileKey];

            foreach (var item in profile.StartingStructures)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    Site site = Parent.Allegiance.Site;
                    Entity entity = Entity.CreateAndInitEntity(GameData.Instance.AllEntityTypes[item.Key], site, allegiance: Parent.Allegiance);
                    entity.PlaceEntityOnOtherSite(site, null, null, new Entity.SetOwnerInfo((IOwner)Parent), null);
                    entity.ComeOnline(); // start the AI we need - activates terminals.
          
                }
            }
        }

       
       
        public void DeleteEntity(EntityID entityID, EntityType entityType)
        {
            allEntities.Remove(entityID);
                        
            StructureStockpiles.Remove(entityID);
            TerminalTradeOffers.Remove(entityID);
            Upgrades.Remove(entityID);

            if (entityType != null)
            {
                // if the EntityType is known we can delete the instance quicker! otherwise we have to iterate the multilists.
                Common.RemoveFromMultiList(AllEntities, entityType, entityID);

                if (IsVehicle(entityType))
                {
                    if (Vehicles != null)
                    {
                        Vehicles.Remove(entityID);
                    }
                }

                if (entityType.StructureType != null)
                {
                    Common.RemoveFromMultiList(Structures, entityType, entityID);
                }

                if (entityType.CommunicatorType != null)
                {
                    Common.RemoveFromMultiList(Communicators, entityType, entityID);
                }

                if (entityType.TerminalType != null)
                {
                    Common.RemoveFromMultiList(Terminals, entityType.TerminalType.TypeOfTerminal, entityID);

                    // Terminals.Remove(entityID);         
                }

                if (entityType.ItemType != null) // Items!
                {
                    Common.RemoveFromMultiList(Items, entityType, entityID);

                    if (entityType.IsMountableWeapon())
                    {                        
                        List<EntityID> weapons;
                        foreach (var attackType in entityType.ItemType.WeaponType.AttackTypes)
                        {
                            if (WeaponsByAttackType.TryGetValue(attackType, out weapons))
                            {
                                weapons.Remove(entityID);
                            }

                        }
                    }

                    if (entityType.ItemType.AmmunitionType != null)
                    {
                        OwnerAmmoOfType ammoOfType;

                        if (AmmoItems.TryGetValue(entityType, out ammoOfType))
                        {
                            ammoOfType.Items.Remove(entityID);
                            ammoOfType.TotalIsDirty = true;
                        }
                    }

                    if (entityType.ItemType.FoodType != null)
                    {
                        Common.RemoveFromMultiList(Food, entityType, entityID);
                        //InternalOwner.OwnerContent.Food.Remove(entityID);
                    }
                }

                // add more entity type lists for iteration here. animals... robots... tools...

            }
            else
            {
                // EntityType is unknown - (cleanup of missing entities)!
                // we don't know the entity type, we have to look through all lists (more costly...):

                if (Vehicles != null)
                {
                    Vehicles.Remove(entityID);
                }

                foreach (var list in AllEntities)
                {
                    if (list.Value.Remove(entityID))
                    {
                        break;
                    }
                }

                if (Items != null)
                {
                    foreach (var list in Items)
                    {
                        if (list.Value.Remove(entityID))
                        {
                            break;
                        }
                    }
                }

                foreach (var weapons in WeaponsByAttackType)
                {
                    if (weapons.Value.Remove(entityID))
                    {
                        break;
                    }
                }

                foreach (var structure in Structures)
                {
                    if (structure.Value.Remove(entityID))
                    {
                        break;
                    }
                }

                foreach (var entity in Communicators)
                {
                    if (entity.Value.Remove(entityID))
                    {
                        break;
                    }
                }

                foreach (var ammo in AmmoItems)
                {
                    if (ammo.Value.Items.Remove(entityID))
                    {
                        ammo.Value.TotalIsDirty = true;
                        break;
                    }
                }

                foreach (var list in Food)
                {
                    if (list.Value.Remove(entityID))
                    {
                        break;
                    }
                }

                foreach (var list in Terminals)
                {
                    if (list.Value.Remove(entityID))
                    {
                        break;
                    }
                }
               // Terminals.Remove(entityID);
             
            }
        }


       
        public void DeleteEntity(Entity entity)
        {           
            DeleteEntity(entity.EntityID, entity.EntityType);
        }

      
        private bool IsVehicle(EntityType entityType)
        {
            return entityType.ContainerType != null && entityType.ContainerType is VehicleContainerType;
        }



        public void AddJob(Job job)
        {           
            AddOrRemoveJob(job, true);
        }

        public void RemoveJob(Job job)
        {
            AddOrRemoveJob(job, false);
        }


        public void Update(GameTime gameTime)
        {
            if (HaulingJobManager != null)
            {
                HaulingJobManager.Update(gameTime);
            }

            if (OtherJobManager != null)
            {
                OtherJobManager.Update(gameTime);
            }

            if (HuntingJobManager != null)
            {
                HuntingJobManager.Update(gameTime);
            }

            if (TradeManager != null)
            {
                TradeManager.Update(gameTime);
            }



        }

        private void AddOrRemoveJob(Job job, bool add)
        {
            
            // if calling from Job, we have to cast, overloads won't work

            if (add)
            {
                allJobs.Add(job.ID);
            }
            else
            {
                allJobs.Remove(job.ID);
            }

            if (job is IVariableMaxTakerJob maxTakerJob)
            {
                if (add)
                {
                    variableMaxTakerJobs.Add(job);
                }
                else
                {
                    variableMaxTakerJobs.Remove(job);
                }
            }

            HaulingJob haulingJob = job as HaulingJob;
            if (haulingJob != null)
            {
                
                if (add)
                {
                    HaulingJobs.Add(haulingJob);                    
                }
                else
                {
                    HaulingJobs.Remove(haulingJob);                    
                }

                HaulingJobAnyItemOfType haulingJobAnyItem = job as HaulingJobAnyItemOfType;
                if (haulingJobAnyItem != null)
                {
                    if (add)
                    {
                        Common.AddToMultiList(HaulingJobsAnyItemOfType, haulingJobAnyItem.RequiredItemType, haulingJobAnyItem);
                       
                    }
                    else
                    {
                        Common.RemoveFromMultiList(HaulingJobsAnyItemOfType, haulingJobAnyItem.RequiredItemType, haulingJobAnyItem);                     
                    }
                }

                HaulingJobSpecificItem haulingJobSpecific = job as HaulingJobSpecificItem;
                if (haulingJobSpecific != null)
                {
                    if (add)
                    {
                       // Common.AddToMultiList(SpecificHaulingJobs, haulingJobSpecific.Item.Value, haulingJobSpecific);
                        SpecificHaulingJobs[haulingJobSpecific.Item.Value] = haulingJobSpecific;

                    }
                    else
                    {
                        SpecificHaulingJobs.Remove(haulingJobSpecific.Item.Value);                      
                    }
                }

                return;
            }


            ProcessJob processJob = job as ProcessJob;
            if (processJob != null)
            {
                if (processJob.RepairJob != null)
                {
                    // save the job under the parent (root) ID, not the parts.
                   /* EntityID? actingOnEntity;
                    if (processJob.GetActingOnEntity(out actingOnEntity))
                    {*/
                        if (add)
                        {
                            Common.AddToMultiList(RepairJobs, processJob.RepairJob.EntityToRepair /* actingOnEntity.Value*/, processJob);
                        }
                        else
                        {
                            Common.RemoveFromMultiList(RepairJobs, processJob.RepairJob.EntityToRepair /*actingOnEntity.Value*/, processJob, removeEmptyList: true);
                        }
                    //}
                }
                else
                {
                    //totalDirectOrderJobsDirty = true;

                    if (processJob.OutputEntityType != null) // production, gather, construction
                    {
                        if (add)
                        {
                            Common.AddToMultiList(ProductionJobs, processJob.OutputEntityType, processJob);

                            if (JobManager.IsManagedProductionJob(processJob))
                            {
                                Common.AddToMultiList(ManagedProductionJobs, processJob.OutputEntityType, processJob);
                            }

                            if (processJob.IsUnattended())
                            {
                                Common.AddToList(ref UnattendedProcessJobs, processJob);

                               // UnattendedProcessJobs.Sort(ProcessJob.CompareByEstimatedCompletion); // could also use isDirty pattern...
                            }

                            UpdateImportance(processJob.OutputEntityType); // make sure that importance is calculated right away, if we have not done it before for this output

                        }
                        else
                        {
                            Common.RemoveFromMultiList(ProductionJobs, processJob.OutputEntityType, processJob);

                            if (JobManager.IsManagedProductionJob(processJob))
                            {
                                Common.RemoveFromMultiList(ManagedProductionJobs, processJob.OutputEntityType, processJob);
                            }

                            if (processJob.IsUnattended())
                            {
                                UnattendedProcessJobs.Remove(processJob);
                            }

                        }
                    }
                    else
                    {
                        // salvage, fumigate...
                        if (add) // WHY is this necessary...
                        {
                            otherJobs.Add(processJob);
                        }
                        else
                        {
                            otherJobs.Remove(processJob);
                        }
                    }

                    if (processJob.ProcessType.InputsByType != null)
                    {
                        foreach (var input in processJob.ProcessType.InputsByType)
                        {
                            if (add)
                            {
                                Common.AddToMultiList(ProductionJobsByInput, input.Key, processJob);
                            }
                            else
                            {
                                Common.RemoveFromMultiList(ProductionJobsByInput, input.Key, processJob);
                            }
                        }
                    }
                }

                return;
            }

            HuntingJob huntingJob = job as HuntingJob;
            if (huntingJob != null)
            {
                // gets placed in production jobs:
               /* if (huntingJob.EntityTypeToHunt != null)
                {
                    if (add)
                    {
                        Common.AddToMultiList(productionJobs, huntingJob.EntityTypeToHunt, huntingJob);
                    }
                    else
                    {
                        Common.RemoveFromMultiList(productionJobs, huntingJob.EntityTypeToHunt, huntingJob);
                    }
                }
                else
                {*/
                    if (add)
                    {
                        otherJobs.Add(huntingJob);
                    }
                    else
                    {
                        otherJobs.Remove(huntingJob);
                    }
              //  }

                return;
            }


            FindPreyJob findPreyJob = job as FindPreyJob;
            if (findPreyJob != null)
            {
                if (add)
                {
                    findPreyJobs.Add(findPreyJob);

                    if (findPreyJob.Zone.ZoneHunt.CreaturesToHunt != null)
                    {
                        foreach (var item in findPreyJob.Zone.ZoneHunt.CreaturesToHunt)
                        {
                            if (item.Value > 0)
                            {
                                UpdateImportance(item.Key.BiologicalType.CarcassType); // make sure that importance is calculated right away, if we have not done it before for this output
                            }
                        }                        
                    }

                }
                else
                {
                    findPreyJobs.Remove(findPreyJob);
                }

                return;
            }

            ScoutingJob scoutingJob = job as ScoutingJob;
            if (scoutingJob != null)
            {
                if (add)
                {
                    scoutingJobs.Add(scoutingJob);
                }
                else
                {
                    scoutingJobs.Remove(scoutingJob);
                }

                return;
            }

            PatrolJob patrolJob = job as PatrolJob;
            if (patrolJob != null)
            {
                if (add)
                {
                    patrolJobs.Add(patrolJob);
                }
                else
                {
                    patrolJobs.Remove(patrolJob);
                }

                return;
            }

            AttackAreaJob attackAreaJob = job as AttackAreaJob;
            if (attackAreaJob != null)
            {
                if (add)
                {
                    attackAreaJobs.Add(attackAreaJob);
                }
                else
                {
                    attackAreaJobs.Remove(attackAreaJob);
                }

                return;
            }

            CheckProcessJob checkJob = job as CheckProcessJob;
            if (checkJob != null)
            {
                if (add)
                {
                    CheckProcessJobs.Add(checkJob);
                    CheckProcessJobsQuadTree.AddObject(checkJob.ID, checkJob.Location.ToVector2());
                }
                else
                {
                    CheckProcessJobs.Remove(checkJob);
                    CheckProcessJobsQuadTree.RemoveObject(checkJob.ID);
                }

                return;
            }

            ThreatJob threatJob = job as ThreatJob;
            if (threatJob != null)
            {
                if (add)
                {
                    if (threatJob.IsVermin)
                    {
                        AssetThreatJobs.Add(threatJob);
                        AssetThreatJobsByTarget.Add(threatJob.Target.Value, threatJob);
                    }
                    else
                    {
                        ThreatJobs.Add(threatJob);
                        ThreatJobsByTarget.Add(threatJob.Target.Value, threatJob);
                    }
                }
                else
                {
                    if (threatJob.IsVermin)
                    {
                        AssetThreatJobs.Remove(threatJob);
                        AssetThreatJobsByTarget.Remove(threatJob.Target.Value);
                    }
                    else
                    {
                        ThreatJobs.Remove(threatJob);
                        ThreatJobsByTarget.Remove(threatJob.Target.Value);
                    }
                }

                return;
            }
         
        }

        /// <summary>
        /// only updates if we have no data entry yet.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="processJob"></param>
        private void UpdateImportance(EntityType entityType)
        {           
            bool isEatable = GetAllegiance().FoodExtraction.IsEatable(entityType);

            if (!isEatable)
            {
                if (!ProductionImportance.ContainsKey(entityType))
                {
                    RecomputeImportance(entityType, null);
                }
            }
            else
            {
                if (FoodProductionImportance == null)
                {
                    RecomputeFoodProductionImportance(null);                

                  /*  HashSet<EntityType> foodUnderProduction = new HashSet<EntityType>(){ entityType };
                    RecomputeFoodProductionImportance(foodUnderProduction, null);*/
                }
               
            }     
        }
       
        public void AddEntity(Entity entity) 
        {
            if (Contains(entity))
                return;

            // seen entities are added to these collections even if the agent/critter has no interest in it...
            
            EntityType entityType = entity.EntityType;

            // add to main list:
            allEntities.Add(entity.ID, entity.ID);
            //Common.AddToMultiList(allEntities, entityType, entity.ID);

            Common.AddToMultiList(AllEntities, entityType, entity.ID);

            // add to new collection:
            if (IsVehicle(entityType))
            {
                Vehicles.Add(entity.EntityID);
            }

            if (entityType.ItemType != null)
            {
                List<EntityID> entities;
                if (!Items.TryGetValue(entityType, out entities))
                {
                    entities = new List<EntityID>();
                    Items.Add(entityType, entities);
                }
                entities.Add(entity.EntityID);

                // iteration list for EvaluateAttackJobs
                if (entityType.IsMountableWeapon()) // .ItemType.WeaponType != null)
                {
                   
                    foreach (var attackType in entityType.ItemType.WeaponType.AttackTypes)
                    {
                        Common.AddToMultiList(WeaponsByAttackType, attackType, entity.EntityID);

                        /*if (!InternalOwner.OwnerContent.Weapons.TryGetValue(attackType, out weapons))
                        {
                            weapons = new List<EntityID>();
                            InternalOwner.OwnerContent.Weapons.Add(attackType, weapons);
                        }
                        weapons.Add(entity.ID);*/
                    }
                }

                if (entityType.ItemType.AmmunitionType != null)
                {
                    OwnerAmmoOfType ammoOfType;

                    if (!AmmoItems.TryGetValue(entityType, out ammoOfType))
                    {
                        ammoOfType = new OwnerAmmoOfType() { Items = new List<EntityID>() };
                        AmmoItems.Add(entityType, ammoOfType);
                    }
                    ammoOfType.Items.Add(entity.EntityID);
                    ammoOfType.TotalIsDirty = true;
                }

                // Food
              
                // see if we can determine valid food items:
                if (this.Parent.Allegiance.Site.IsPlaySite)
                {
                    if (Parent.IsEatable(entity.EntityType))
                    {
                        Common.AddToMultiList(food, entity.EntityType, entity.EntityID); // we don't use the property so we don't trigger an update
                    }
                }

            }

            if (entityType.StructureType != null)
            {
                Common.AddToMultiList(Structures, entityType, entity.EntityID);

                if (entity.Contains != null && entity.Contains is IStorage)
                {
                    // create a stockpile setting for the structure immediately:
                    Stockpile stockpile = new Stockpile(Stockpile.TypesOfStockpiles.Normal, entity.EntityType.ContainerType.GetDefaultStorageSettings());

                    StructureStockpiles.Add(entity.EntityID, stockpile);
                }
            }

            if (entityType.ContainerType != null && entityType.ContainerType.CanBeUpgraded)
            {
                // create a setting for the entity immediately:
                Dictionary<UpgradeCategory, EntityType> slots = new Dictionary<UpgradeCategory, EntityType>();
                foreach (var item in entityType.ContainerType.GetUpgradeOptions())
                {
                    slots.Add(item, null); // start with empty slot orders
                }                

                Upgrades.Add(entity.ID, slots);

                /*
                Common.AddToNestedDictionary(Upgrades, entity.ID,
                   GameData.Instance.AllUpgradeCategories[UpgradeCategory], GameData.Instance.AllEntityTypes[UpgradeEntityType]);*/
            }

            if (entityType.CommunicatorType != null)
            {
                Common.AddToMultiList(Communicators, entityType, entity.EntityID);
            }

            if (entityType.TerminalType != null) //ContainerType != null && entityType.ContainerType is TerminalContainerType)
            {
                // Parent
              /*  IOwner owner = LookUpOwners.FindByID(entity.OwnedBy);
                if (owner != null)
                {*/

                Common.AddToMultiList(Terminals, entityType.TerminalType.TypeOfTerminal, entity.ID);

                if (entityType.ContainerType != null && entityType.ContainerType is TerminalContainerType)
                {
                    Stockpile stockpile = new Stockpile(Stockpile.TypesOfStockpiles.OfferedForTrade, null);
                    TerminalTradeOffers.Add(entity.EntityID, stockpile);
                }
            }
            // add more entity type lists here... gadgets... animals... robots...


        }


        public void SetAmmoDirty(EntityType ammoType)
        {
            OwnerAmmoOfType ammo;
            if (AmmoItems.TryGetValue(ammoType, out ammo))
            {
                ammo.TotalIsDirty = true;
            }
        }

        /// <summary>
        /// get the type of upgrade that is ordered for this entity and slot, if any
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public EntityType GetOrderedUpgrade(EntityID entityID, UpgradeCategory category)
        {
            EntityType entityType = null;

            Dictionary<UpgradeCategory, EntityType> dict;
            if (Upgrades.TryGetValue(entityID, out dict))
            {
                EntityType currentUpgrade;
                if (dict.TryGetValue(category, out currentUpgrade))
                {
                    entityType = currentUpgrade;
                }

                /*   var ordered = dict.Values.FirstOrDefault(u => u != null);
                   if (ordered != null)
                   {
                       entityType = ordered;
                   }*/
            }

            return entityType;
        }

        public bool GetIsUpgrade(EntityID entityID, UpgradeCategory category, EntityType entityType)
        {
            Dictionary<UpgradeCategory, EntityType> dict;
            if (Upgrades.TryGetValue(entityID, out dict))
            {
                EntityType currentUpgrade;
                if (dict.TryGetValue(category, out currentUpgrade)
                    && currentUpgrade == entityType)
                {
                    return true;
                }

            }

            return false;
        }

        public void SetUpgrade(EntityID entityID, UpgradeCategory upgradeCategory, EntityType upgradeType)
        {
            Common.AddToNestedDictionary(Upgrades, entityID, 
                    upgradeCategory, upgradeType);

        }

        public bool Contains(IKnownEntityData entity)
        {
            return allEntities.ContainsKey(entity.EntityID); // Common.MultiListContains(allEntities, entity.EntityType, entity.EntityID);
        }

        public bool Contains(Entity entity)
        {
            return allEntities.ContainsKey(entity.ID); // Common.MultiListContains(allEntities, entity.EntityType, entity.ID);
        }


        public void IterateEntities(Action<Entity> iterateMethod)
        {
            foreach (var entityID in allEntities)
            {
                Entity entity = Entity.FindByID(entityID.Value);
                if (entity != null)
                {
                    iterateMethod(entity);
                }
            }
        }


        /// <summary>
        /// transfers money and ownership of items in one transaction.
        /// Only makes sense for ownership collections.
        /// </summary>
        /// <param name="buyer"></param>
        /// <param name="order"></param>
        /// <param name="buyReducedAmountsIfNeeded"></param>
        /// <param name="maxAmountToSpend"></param>
        /// <param name="spentAmount"></param>
        /// <param name="boughtItems"></param>
        /// <returns></returns>
        public bool Buy(IOwner buyer,
            Dictionary<EntityType, List<EntityID>> order,  //Dictionary<EntityType, int> order, 
            bool buyReducedAmountsIfNeeded, decimal? maxAmountToSpend, out decimal spentAmount, out List<Entity> boughtItems)
        {
            spentAmount = 0;
            boughtItems = new List<Entity>();

           
           // List<EntityID> entityIDsOfType;
            List<Entity> entitiesOfType;
            Dictionary<EntityType, List<Entity>> entitiesFulfillingOrder = new Dictionary<EntityType, List<Entity>>();

            Entity entityToBuy;

            foreach (var list in order)
            {
                for (int i = 0; i < list.Value.Count; i++)
                {
                    // must exist before a trade can take place:
                    entityToBuy = Entity.FindByID(list.Value[i]);

                    if (entityToBuy != null)
                    {
                        // copy to new collection:
                        Common.AddToMultiList(entitiesFulfillingOrder, entityToBuy.EntityType, entityToBuy);
                      /*  itemsAdded++;

                        if (itemsAdded == item.Value)
                        {
                            break;
                        }*/
                    }
                }

            }
            
            if (buyReducedAmountsIfNeeded == false && maxAmountToSpend.HasValue) // for remote buying
            {
                decimal maxAmount = maxAmountToSpend.Value; // ?? buyer.OwnedEntities.Parent.TradeCredits.Value;

                // check that the order can be fulfilled, otherwise abort.
                // also check that we have enough money (credits).
                decimal? price;
                decimal totalPrice = 0;
                foreach (var item in order)
                {
                    if (entitiesFulfillingOrder.TryGetValue(item.Key, out entitiesOfType))
                    {
                        if (item.Value.Count != entitiesOfType.Count)
                        {
                            return false;
                        }
                        else
                        {
                          
                           // price = (decimal)GetBuyPrice(item.Key);
                            price = BuySellActionTemplate.GetTradePrice(item.Key, buyer.OwnedEntities, this);

                            totalPrice += item.Value.Count * price.Value;

                            if (totalPrice > maxAmount)
                            {
                                return false;
                            }

                        }
                    }
                    else return false;
                }
            }

          // not needed for now, maybe when buying without comm link?
            Entity.GiveNewOwnerKnowledge detectBoughtItemsIfOffSite = Entity.GiveNewOwnerKnowledge.Yes;
          /*  if (!buyer.Allegiance.Site.IsPlaySite)
            {    
                // detection on othersite: only if in Comm range (we never add memory facts)
                CommunicationMethod? method;
                if(Parent.Allegiance.IsInCommunicationRange(buyer.Allegiance, out method))
                {
                    detectBoughtItemsIfOffSite = Entity.GiveNewOwnerKnowledge.Yes;
                }
                else 
                {
                    detectBoughtItemsIfOffSite =  Entity.GiveNewOwnerKnowledge.No; // hacked for now...
                }
            }*/

            // make the transaction:
            foreach (var item in order)
            {
                if (entitiesFulfillingOrder.TryGetValue(item.Key, out entitiesOfType))
                {
                   
                    decimal? price = BuySellActionTemplate.GetTradePrice(item.Key, buyer.OwnedEntities, this);

                   // decimal? price = (decimal)GetBuyPrice(item.Key);

                    //int entitiesToBuy = item.Value;
                    for (int i = 0; i < entitiesOfType.Count; i++)
                    {
                        entityToBuy = entitiesOfType[i];                   

                       /* if (spentAmount + price > maxAmount)
                        {
                            break; // stop when we reach an item we cannot afford
                        }*/
                         
                        // TODO:
                        // When buying/changing ownership at othersite, the items will show up in the stock manager.
                        // Code needs to be added to ensure the interface and AI can handle such items, 
                        // even if they are only MemoryFacts, or have a new, as yet undesigned state (State: NotYetDiscovered could be used to filter them away?).                       
                        // for now, let's cheat and change ownership when they arrive at PlaySite.
                        entityToBuy.ChangeOwnership(buyer, detectBoughtItemsIfOffSite); 
                        boughtItems.Add(entityToBuy);

                        spentAmount += price.Value;                       
                    }

                    if (buyer.OwnedEntities.TradeManager != null)
                    {
                        buyer.OwnedEntities.TradeManager.Buy(item.Key, boughtItems.Count);
                    }
                }
            }

            MakeTradeCreditsTransaction(buyer.OwnedEntities.Parent, this.Parent, spentAmount);

           
            /*
            buyer.OwnedEntities.Parent.TradeCredits -= spentAmount;
            this.Parent.TradeCredits += spentAmount;
            */

            return true;
        }

        /// <summary>
        /// count harvest output too? yes, then we can adjust production if there is both gathering and regular production going on
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="currentJobs"></param>
        /// <returns></returns>
        public int CountOutstandingJobOutput(EntityType entityType, bool countUnstarted, out int currentJobs, out float averageSpeed)
        {
            List<ProcessJob> existingProductionJobs;
            ProductionJobs.TryGetValue(entityType, out existingProductionJobs); //get the current production jobs
            //  expedition.OwnedEntities.ManagedProductionJobs.TryGetValue(entityType, out existingProductionJobs); 

            if (existingProductionJobs != null)
            {
                currentJobs = existingProductionJobs.Count;
            }
            else
            {
                currentJobs = 0;
            }

            int totalOutstandingOutput = CountJobOutput(entityType, countUnstarted, existingProductionJobs, out averageSpeed);
            return totalOutstandingOutput;
        }


        private static int CountJobOutput(EntityType entityType, bool countUnstarted, List<ProcessJob> existingProductionJobs, out float averageSpeed)// productionInterval) //, out int totalOutput)
        {
            int totalOutput = 0;
           // productionInterval = 0f;

            averageSpeed = 0f;

            float totalSpeed = 0f;
            int count = 0;

            if (existingProductionJobs != null)
            {
                ProcessJob processJob;
                for (int i = existingProductionJobs.Count - 1; i >= 0; i--)
                {
                    processJob = existingProductionJobs[i];

                    bool isStarted;
                    if (countUnstarted == true ||
                        processJob.IsStarted(out isStarted) && isStarted)
                    {

                        int? output = processJob.ProcessType.GetOutputAmount(entityType);

                        totalOutput += output ?? 0;

                        // find the total interval for this process
                      /*  float normalTimeNeeded = processJob.ProcessType.GetTimeNeeded();
                        float estimatedTimeNeeded = normalTimeNeeded * processJob.GetProgressSpeed();
                        */

                        totalSpeed += processJob.GetProgressSpeed();
                        count++;
                       /* if (estimatedTimeNeeded > productionInterval)
                        {
                            productionInterval = estimatedTimeNeeded;
                        }*/
                    }
                }
            }

            if (count > 0)
            {
                averageSpeed = totalSpeed / count;
            }
          

            return totalOutput;
        }

        public int CountAvailableItems(EntityType entityType)
        {
            int itemsInStock = 0;

            int noOfIncompleteItems = 0, noOfAvailableItems = 0, noOfAvailableItemsIncludingIntrinsic = 0, noOfItemsUsedAsParts = 0, noOfItemsOffSite = 0, noOfItemsOwnedByOthers = 0;
            List<EntityID> availableEntities = null;
            List<EntityID> unavailableEntities = null;

            List<EntityID> itemsOfType;
            if (AllEntities.TryGetValue(entityType, out itemsOfType))
            {
                foreach (var entityID in itemsOfType)
                {
                    Entity entity = Entity.FindByID(entityID);
                    if (entity != null)
                    {
                        EntityGroup.CountEntity(GetOwnerID(), entity, ref noOfIncompleteItems, ref noOfItemsUsedAsParts, ref noOfItemsOffSite, ref noOfItemsOwnedByOthers, ref noOfAvailableItems, ref noOfAvailableItemsIncludingIntrinsic,
                            ref availableEntities, ref unavailableEntities);
                    }
                }
                itemsInStock = noOfAvailableItems;
            }

            return itemsInStock;
        }


        public static void MakeTradeCreditsTransaction(IOwner buyer, IOwner seller, decimal amount)
        {
            buyer.OwnedEntities.Parent.TradeCredits -= amount;
            seller.OwnedEntities.Parent.TradeCredits += amount;
        }

        public static void MakeTradeCreditsTransaction(IHasEntityGroup buyer, IHasEntityGroup seller, decimal amount)
        {
            buyer.TradeCredits -= amount;
            seller.TradeCredits += amount;
        }
        
        public void GetEntities(FilterCondition filter, ref List<IHasExposedProperties> listToFillWithProperties, out bool wasFiltered, 
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource)
        {

            PropertyCondition propertyCondition = filter as PropertyCondition;

            if (propertyCondition != null)
            {
                if (propertyCondition.PropertyKey == "type")
                {
                    List<EntityID> list;
                    if (AllEntities.TryGetValue(GameData.Instance.AllEntityTypes[propertyCondition.ConstantStringEqual], out list))
                    {
                        foreach (var item in list)
                        {
                            IKnownEntityData entityData;
                            if (!GoalEvaluator.EntityDataResultCausesSkip(Parent.Allegiance.SharedKnowledge.GetKnownData(item, out entityData)))
                            {
                                listToFillWithProperties.Add((IHasExposedProperties)entityData);
                            }
                        }

                    }

                    wasFiltered = true;
                    return;
                }               
            }


            // return the full list:
            foreach (var item in allEntities)
            {
                IKnownEntityData entityData;
                if (!GoalEvaluator.EntityDataResultCausesSkip(Parent.Allegiance.SharedKnowledge.GetKnownData(item.Key, out entityData)))
                {
                    listToFillWithProperties.Add((IHasExposedProperties)entityData);
                }
            }

            wasFiltered = false;

        }

        public int GetBuyAmount(EntityType itemType)
        {
            if (TradeManager != null)
            {
                return TradeManager.GetBuyAmount(itemType);
            }

            return 0;
        }

        /// <summary>
        /// prices for goods this owner is willing to BUY
        /// 
        /// Not currently used by players
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public float? GetBuyPrice(EntityType itemType)
        {
            if (TradeManager != null)
            {
                return TradeManager.GetBuyPrice(itemType);
            }

            return null;
        }

        /// <summary>
        /// prices for goods this owner has FOR SALE
        /// 
        /// Not currently used by players
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public float? GetSellPrice(EntityType itemType)
        {
            if (TradeManager != null)
            {
                return TradeManager.GetSellPrice(itemType);
            }

            return null;
        }

       /* public decimal? GetTradePrice(EntityType itemType)
        {           
            if (TradeManager != null)
            {
                return TradeManager.GetTradePrice(itemType);
            }

            return null;
        }*/


      
        public static void CountEntity(OwnerID? thisOwner, IKnownEntityData itemData, ref int noOfIncompleteEntities, ref int noOfEntitiesUsedAsParts, ref int noOfItemsOnOtherSite, ref int noOfItemsOwnedByOthers,
            ref int noOfAvailableEntities, ref int noOfAvailableEntitiesIncludingIntrinsic,
            ref List<EntityID> listOfAvailableEntities, ref List<EntityID> listOfUnavailableEntities)
        {
            bool isOwnedByOthers = false;
            bool isOnOtherSite = false;
            bool isPart = false;
            bool isCompleted = true;

            // only count the item once.

            if (itemData.PartOfID != null)
            {
                isPart = true;
                noOfEntitiesUsedAsParts++;
            }
            else if (itemData.Location == null)
            {
                isOnOtherSite = true;
                noOfItemsOnOtherSite++;
            }
            else if (thisOwner != null && itemData.OwnedBy != thisOwner)
            {
                isOwnedByOthers = true;
                noOfItemsOwnedByOthers++;
            }
            else if (!itemData.IsCompleted())
            {
                isCompleted = false;
                noOfIncompleteEntities++;
            }

        
            if (isCompleted && !isOnOtherSite && !isOwnedByOthers && !isPart) // (!isPart || (itemData.EntityType.IsIntrinsic())))
            {                
                noOfAvailableEntities++;
                noOfAvailableEntitiesIncludingIntrinsic++;

                if (listOfAvailableEntities != null)
                {
                    listOfAvailableEntities.Add(itemData.EntityID);
                }
            }
            else
            {
                if (isPart && itemData.EntityType.IsIntrinsic())    // If an intrinsic tool is a part, count it as available (mounted), but only if the context is tools, not if inputs... so let the client decide.
                {
                    noOfAvailableEntitiesIncludingIntrinsic++;
                }

                if (listOfUnavailableEntities != null)
                {
                    listOfUnavailableEntities.Add(itemData.EntityID);
                }
            }
        }

       

        public decimal? GetVehicleForHirePrice(EntityType itemType, out decimal? pricePerKilometer)
        {
            if (TradeManager != null)
            {
                return TradeManager.GetPriceToHire(itemType, out pricePerKilometer);
            }

            pricePerKilometer = null;
            return null;
        }

        /// <summary>    
        /// </summary>
        /// <param name="allegiance"></param>
        /// <returns></returns>
        public bool IsOwnedByAllegiance(Allegiances.Allegiance allegiance) 
        {
            return GetAllegiance() == allegiance;
        }

        /// <summary>     
        /// gets the allegiance that this owner belongs to.
        /// </summary>
        /// <returns></returns>
        public Allegiances.Allegiance GetAllegiance()
        {
            return Parent.Allegiance; // ((IHasAllegiance)InternalOwner).Allegiance;
        }

        /// <summary>     
        /// </summary>
        /// <returns></returns>
        public Expedition GetExpedition()
        {
            Expedition expedition = Parent as Expedition;
            return expedition;
        }


       

       

        /// <summary>
        /// can be used when doing overland lookups
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public EntityResult GetKnownData(EntityID entityID, out IKnownEntityData data)
        {
            if(Parent.Allegiance.SharedKnowledge != null)
            {
                return Parent.Allegiance.SharedKnowledge.GetKnownData(entityID, out data);

            }
            else
            {
                data = Entity.FindByID(entityID);
                if (data != null)
                {
                    return EntityResult.SeenDirectly;
                }
                else
                {
                    return EntityResult.Destroyed;
                }    
            }
        }

        public const float NeutralImportance = 0.5f;

        public void SetNeutralImportance(EntityType entityType)
        {
            ProductionImportance[entityType] = NeutralImportance; // netrual
        }

        /// <summary>
        /// Not food!
        /// 
        /// neutral importance is 0.5
        /// 0 is low importance (big stockpiles (how big???))
        /// 1 is high importance (low stockpiles, or high consumption rate)
        /// 
        /// consider production currently underway (unknown progress?)
        /// 
        /// cache this score in EntityGroup // SharedKnowledge
        /// </summary>
        /// <returns></returns>
        public void RecomputeImportance(EntityType entityType, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
        {
            /*  if (ProcessType.HasOutput)
              {
                  // scale consumption and production rates so thay can be compared
                  // score "how long stores will last"
                  // 0 time gives highest score.
                  // for 0 time: score surbival importance (food over tools)

                  var output = ProcessType.Outputs.FirstOrDefault(o => o.IsWasteProduct == false);
                  if (output != null)
                  {
                      EntityType mainOutput = output.FinalEntityTypeToCreate;
              */
            // bool isEatable = GetAllegiance().FoodExtraction.IsEatable(entityType);

            double timeLeftScore = ScoreHowLongStocksWillLast(entityType, allAvailableItems);

            double urgencyScore = 1d - timeLeftScore;
            double survivalScore = 0d; // ScoreSurvivalImportance(/*owner,*/ isEatable);

            // fuel is not used in process inputs, but is still important...

            double score = CalculateImportance(urgencyScore, survivalScore);

            ProductionImportance[entityType] = (float)score;

            //  Importance = score;
            // return;

            // }

            // Importance = 0.5;
        }

        private static double CalculateImportance(double urgencyScore, double survivalScore)
        {
            double score = 0.75f * urgencyScore + 0.25f * survivalScore;
            return score;
        }

        private double ScoreSurvivalImportance(/*EntityGroup owner,*/ bool isEatable) // EntityType mainOutput)
        {
            if (isEatable) // owner.GetAllegiance().FoodExtraction.IsEatable(mainOutput))
            {
                return 1.0;
            }

            return 0d;
        }


        private double ScoreHowLongStocksWillLast(EntityType entityType, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
        {

            int currentStockAmount;
          /* 
           * OLD: only count items under production
           * int stockpiledAndProduced;
            float productionRate;
            ComputeProductionRate(entityType, out currentStockAmount, out stockpiledAndProduced, out productionRate, allAvailableItems);

            if (stockpiledAndProduced == 0)
            {
                return 0d;
            }

            */
           
           // double interval = 0.5d;


            // currently, starting production of an item with a zero stockpile and zero historical consumption will immediately drop its importance to 0.

            // consumtion rate cannot use consume stats alone if stockpile is empty...
            // for food, consumption rate is based on needs

            // not food:
            // count current processes using this as input - what about fuel??
            // basing future consumption on current consumption...
            // use production stats (one day back)!

            // score other "need", like tools???
            // or score the desire to have a big stockpile? Almost impossible... must be left to player priority.
            // TODO: fuel and tools (not inputs) - create stats for them as well

            // Warning! stats are in allegiance, but we need them in EntityGroup! Will make a difference when more expeditions get added.

            float interval = GameData.Instance.AIConstants.IntervalInDaysForComputingProductImportance;

            DateAndTime.TimeDateYear to = The.Sim.DateAndTime.CurrentTimeDateYear;
            DateAndTime.TimeDateYear from = to;
            from.AddTime(-interval);

            float consumedRate = ComputeEventRate(entityType, from, to, interval, ProductionStatistics.StatTypes.UsedAsInput);
            float productionRate = ComputeEventRate(entityType, from, to, interval, ProductionStatistics.StatTypes.Produced);

            currentStockAmount = GetCurrentStockAmount(entityType, allAvailableItems);

            return ScoreHowLongStocksWillLast(currentStockAmount, productionRate, consumedRate);

        }


        private float ComputeEventRate(EntityType entityType, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to, double interval, ProductionStatistics.StatTypes statType)
        {
            float consumedRate;
            var usedAsInput = GetAllegiance().Statistics.ProductionStatistics.Stats[statType]; // ProductionStatistics.StatTypes.UsedAsInput];

           /* DateAndTime.TimeDateYear to = The.Sim.DateAndTime.CurrentTimeDateYear;
            DateAndTime.TimeDateYear from = to;

            from.AddTime(-interval);*/

            int sumUsedAsInput = Statistic.SumDataPoints(usedAsInput, entityType, from, to);

            consumedRate = (float)(sumUsedAsInput / interval);
            return consumedRate;
        }


        private void ComputeProductionRate(EntityType entityType, out int currentStockAmount, out int stockpiledAndProduced, out float productionRate, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
        {
            currentStockAmount = GetCurrentStockAmount(entityType, allAvailableItems);


            // count items being produced now:

            // for prepared food items, we want to combine all food stocks.
            int currentJobs;
            float averageSpeed;
            int totalOutstandingOutput = CountOutstandingJobOutput(entityType, false, out currentJobs, out averageSpeed); // don't count unstarted...


            stockpiledAndProduced = currentStockAmount + totalOutstandingOutput;


            productionRate = totalOutstandingOutput * averageSpeed;
        }

        private int GetCurrentStockAmount(EntityType entityType, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
        {
            int currentStockAmount;
            InventoryPanel.Availability available;
            if (allAvailableItems == null || !allAvailableItems.TryGetValue(entityType, out available))
            {
                currentStockAmount = CountAvailableItems(entityType);
            }
            else
            {
                currentStockAmount = available.NoOfAvailableItems;
            }
            return currentStockAmount;
        }

        private static double ScoreHowLongStocksWillLast(int currentStockAmount, float productionRate, float consumedRate)
        {
            if (productionRate > consumedRate)
            {
                return 1d; // will never run out
            }
            else
            {
                float stockConsumptionRate = consumedRate - productionRate;

                float daysStockWillLast = currentStockAmount / stockConsumptionRate;

                float daysClamped = Common.Clamp(daysStockWillLast, 0f, 1f);

                return daysClamped;

            }
        }


       
           


        #region ILookup

        private static EntityGroupID IDCounter;

        private EntityGroupID id = EntityGroupID.Invalid;
       
        public EntityGroupID ID
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

        public EntityGroupID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= EntityGroupID.Max)
            {
                throw new Exception("Astounding, OwnerID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public EntityGroupID SnapshotID(Snapshotter sn, EntityGroupID id)
        {
            return (EntityGroupID)sn.DoEnum(id);
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
            if (ID != EntityGroupID.Invalid)
                LookUp<EntityGroup, EntityGroupID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = EntityGroupID.Invalid;
        }

        public void RemoveIDEntry()
        {           
            LookUp<EntityGroup, EntityGroupID>.Remove(this);
        }

        void UWGame.SimSide.Snapshots.ILookUp<EntityGroup, EntityGroupID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = EntityGroupID.First;
        }

        void ILookUp<EntityGroup, EntityGroupID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<EntityGroup, EntityGroupID>.Create();
            LookUp<EntityGroup, EntityGroupID>.SetLoadPostProcessOrder(10);        // before Job
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
            IDCounter = (EntityGroupID)sn.DoEnum(IDCounter);
            id = SnapshotID(sn, id);

            this.allEntities = sn.DoDictionary(allEntities);
            this.AllEntities = sn.DoMultiMap(AllEntities);
            this.activities = sn.DoList(activities);
            this.AmmoItems = sn.DoDictionary(AmmoItems);        
            this.food = sn.DoMultiMap(food);
            this.foodIsDirty = sn.DoBool(foodIsDirty);
            this.items = sn.DoMultiMap(items);
            this.Structures = sn.DoMultiMap(Structures);
            this.StructureStockpiles = sn.DoDictionary(StructureStockpiles);
            this.TerminalTradeOffers = sn.DoDictionary(TerminalTradeOffers);
            this.Upgrades = sn.DoNestedDictionary(Upgrades);
            this.Communicators = sn.DoMultiMap(Communicators);
            this.vehicles = sn.DoList(vehicles);
            this.WeaponsByAttackType = sn.DoMultiMap(WeaponsByAttackType);
            this.FulfillsNeeds = sn.DoMultiMap(FulfillsNeeds);
            this.Terminals = sn.DoMultiMap(Terminals);
            this.Policy = (EntityGroupPolicy)sn.DoISnapshot(Policy);
            this.ProductionOrders = (ProductionOrders)sn.DoISnapshot(ProductionOrders);
            this.foodConsumeRate = sn.DoFloat(foodConsumeRate);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotZones = Zones.Select(z => z.ID).ToList();
            }
            snapshotZones = sn.DoList(snapshotZones);

            snapshotHaulingJobManager = sn.SnapshotID<ICyclable, CyclableID>(HaulingJobManager);
            snapshotOtherJobManager = sn.SnapshotID<ICyclable, CyclableID>(OtherJobManager);
            snapshotHuntingJobManager = sn.SnapshotID<ICyclable, CyclableID>(HuntingJobManager);

            allJobs = sn.DoList(allJobs);
            SnapshotJobList(sn, ref snapshotThreatJobs, ThreatJobs);
            SnapshotJobList(sn, ref snapshotAssetThreatJobs, AssetThreatJobs);           
            SnapshotJobList(sn, ref snapshotHaulingJobs, HaulingJobs);
            SnapshotJobList(sn, ref snapshotOtherJobs, otherJobs);
            SnapshotJobList(sn, ref snapshotPatrolJobs, patrolJobs);
            SnapshotJobList(sn, ref snapshotAttackAreaJobs, attackAreaJobs);
            SnapshotJobList(sn, ref snapshotScoutingJobs, scoutingJobs);
            SnapshotJobList(sn, ref snapshotUnattendedProcessJobs, UnattendedProcessJobs);
            SnapshotJobList(sn, ref snapshotFindPreyJobs, findPreyJobs);
            SnapshotJobList(sn, ref snapshotCheckProcessJobs, CheckProcessJobs);

           // snapshotVariableMaxTakerJobs = new List<JobID>();
            SnapshotJobList(sn, ref snapshotVariableMaxTakerJobs, VariableMaxTakerJobs); // #LOAD35


            this.TradeManager = (TradeManager)sn.DoISnapshot(TradeManager);

            if (sn.mode != Snapshotter.Mode.Load)
            {                
              
                this.snapshotCheckProcessJobsQuadTree = CheckProcessJobsQuadTree.GetAllObjectsAndPositions().Select(p => new Pair<JobID, Vector2>(p.First, p.Second)).ToList();
                                

                snapshotProductionJobs = new Dictionary<EntityType, List<JobID>>();
                foreach (var item in ProductionJobs)
                {
                    snapshotProductionJobs.Add(item.Key, item.Value.Select(i => i.ID).ToList());
                }

                snapshotManagedProductionJobs = new Dictionary<EntityType, List<JobID>>();
                foreach (var item in ManagedProductionJobs)
                {
                    snapshotManagedProductionJobs.Add(item.Key, item.Value.Select(i => i.ID).ToList());
                }

                snapshotProductionJobsByInput = new Dictionary<EntityType, List<JobID>>();
                foreach (var item in ProductionJobsByInput)
                {
                    snapshotProductionJobsByInput.Add(item.Key, item.Value.Select(i => i.ID).ToList());
                }

                snapshotHaulingJobsAnyItemOfType = new Dictionary<EntityType, List<JobID>>();
                foreach (var item in HaulingJobsAnyItemOfType)
                {
                    snapshotHaulingJobsAnyItemOfType.Add(item.Key, item.Value.Select(i => i.ID).ToList());
                }

                snapshotRepairJobs = new Dictionary<EntityID, List<JobID>>();
                foreach (var item in RepairJobs)
                {
                    snapshotRepairJobs.Add(item.Key, item.Value.Select(i => i.ID).ToList());
                }

                snapshotSpecificHaulingJobs = new Dictionary<EntityID, JobID>();
                foreach (var item in SpecificHaulingJobs)
                {
                    snapshotSpecificHaulingJobs.Add(item.Key, item.Value.ID);
                } 
            }

            snapshotProductionJobs = sn.DoMultiMap(snapshotProductionJobs);
            snapshotManagedProductionJobs = sn.DoMultiMap(snapshotManagedProductionJobs);
            snapshotProductionJobsByInput = sn.DoMultiMap(snapshotProductionJobsByInput);
            snapshotHaulingJobsAnyItemOfType = sn.DoMultiMap(snapshotHaulingJobsAnyItemOfType);
            snapshotRepairJobs = sn.DoMultiMap(snapshotRepairJobs);
            snapshotSpecificHaulingJobs = sn.DoDictionary(snapshotSpecificHaulingJobs);
           


            snapshotParent = (HasEntityGroupID)sn.SnapshotID<IHasEntityGroup, HasEntityGroupID>(Parent);


            CheckProcessJobsQuadTree = (PointQuadTree<JobID>)sn.DoISnapshot(CheckProcessJobsQuadTree);
            snapshotCheckProcessJobsQuadTree = sn.DoList(snapshotCheckProcessJobsQuadTree);

            FoodProductionImportance = sn.DoFloatNullable(FoodProductionImportance);
            ProductionImportance = sn.DoDictionary(ProductionImportance);

            sn.Ignore(ThreatJobsByTarget);
            sn.Ignore(AssetThreatJobsByTarget);
            sn.Ignore(HaulingJobManager);
            sn.Ignore(OtherJobManager);
            sn.Ignore(HuntingJobManager);
            sn.Ignore(Zones);
            sn.Ignore(ProductionJobs);
            sn.Ignore(ProductionJobsByInput);
            sn.Ignore(ManagedProductionJobs);
            sn.Ignore(HaulingJobsAnyItemOfType);
            sn.Ignore(SpecificHaulingJobs);
            sn.Ignore(RepairJobs);
          
            return this;
        }

        private static void SnapshotJobList(Snapshotter sn, ref List<JobID>snapshotList, List<Job> jobsList)
        {
          /*  foreach (var item in jobsList)
            {
                if (item.ID == JobID.Invalid
                    || LookUp<Job, JobID>.FindByID(item.ID) == null)
                {

                }
            }*/

            snapshotList = jobsList.Select(j => j.ID).ToList();
            snapshotList = sn.DoList(snapshotList);

            sn.Ignore(jobsList);
        }

        private static void PostLoadJobsList(Snapshotter sn, List<JobID>snapshotList, ref List<Job> jobsList)
        {
            jobsList = snapshotList.Select(j => LookUp<Job, JobID>.FindByID(j)).ToList();

        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            ThreatJobs = snapshotThreatJobs.Select(j => LookUp<Job, JobID>.FindByID(j)).ToList(); 
                     

            AssetThreatJobs = snapshotAssetThreatJobs.Select(j => LookUp<Job, JobID>.FindByID(j)).ToList();

            PostLoadJobsList(sn, snapshotThreatJobs, ref ThreatJobs);
            PostLoadJobsList(sn, snapshotAssetThreatJobs, ref AssetThreatJobs);
            PostLoadJobsList(sn, snapshotScoutingJobs, ref scoutingJobs);
            PostLoadJobsList(sn, snapshotPatrolJobs, ref patrolJobs);
            PostLoadJobsList(sn, snapshotAttackAreaJobs, ref attackAreaJobs);           
            PostLoadJobsList(sn, snapshotOtherJobs, ref otherJobs);
            PostLoadJobsList(sn, snapshotUnattendedProcessJobs, ref UnattendedProcessJobs);
            PostLoadJobsList(sn, snapshotFindPreyJobs, ref findPreyJobs);
            PostLoadJobsList(sn, snapshotHaulingJobs, ref HaulingJobs);
            PostLoadJobsList(sn, snapshotVariableMaxTakerJobs, ref variableMaxTakerJobs);

            foreach (ThreatJob item in ThreatJobs)
            {
                ThreatJobsByTarget.Add(item.Target.Value, item);
            }

            foreach (ThreatJob item in AssetThreatJobs)
            {
                AssetThreatJobsByTarget.Add(item.Target.Value, item);
            }           

            foreach (var item in snapshotProductionJobs)
            {
                ProductionJobs.Add(item.Key, item.Value.Select(j => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
            }

            foreach (var item in snapshotManagedProductionJobs)
            {
                ManagedProductionJobs.Add(item.Key, item.Value.Select(j => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
            }

            foreach (var item in snapshotProductionJobsByInput)
            {
                ProductionJobsByInput.Add(item.Key, item.Value.Select(j => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
            }

            foreach (var item in snapshotHaulingJobsAnyItemOfType)
            {
                HaulingJobsAnyItemOfType.Add(item.Key, item.Value.Select(j => (HaulingJobAnyItemOfType)LookUp<Job, JobID>.FindByID(j)).ToList());
            }

            foreach (var item in snapshotSpecificHaulingJobs)
            {
                SpecificHaulingJobs.Add(item.Key, (HaulingJobSpecificItem)LookUp<Job, JobID>.FindByID(item.Value));
            }

            foreach (var item in snapshotRepairJobs)
            {
                RepairJobs.Add(item.Key, item.Value.Select(j => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
            }

            Zones = snapshotZones.Select(z => LookUp<Zone, ZoneID>.FindByID(z)).ToList();

            Parent = LookUpHasEntityGroup.FindByID(snapshotParent);

            if (snapshotHaulingJobManager.HasValue)
            {
                HaulingJobManager = (HaulingJobManager)LookUp<ICyclable, CyclableID>.FindByID(snapshotHaulingJobManager.Value);
            }

            if (snapshotOtherJobManager.HasValue)
            {
                OtherJobManager = (OtherJobManager)LookUp<ICyclable, CyclableID>.FindByID(snapshotOtherJobManager.Value);
            }

            if (snapshotHuntingJobManager.HasValue)
            {
                HuntingJobManager = (HuntingJobManager)LookUp<ICyclable, CyclableID>.FindByID(snapshotHuntingJobManager.Value);
            }

            if (TradeManager != null)
            {
                TradeManager.LoadPostProcess(sn);
            }

            if (StructureStockpiles != null)
            {
                foreach (var item in StructureStockpiles)
                {
                    item.Value.LoadPostProcess(sn);
                }
            }

            if (TerminalTradeOffers != null)
            {
                foreach (var item in TerminalTradeOffers)
                {
                    item.Value.LoadPostProcess(sn);
                }
            }

            if (Policy != null)
            {
                Policy.LoadPostProcess(sn);
            }

            foreach (var item in  AmmoItems)
            {
                item.Value.LoadPostProcess(sn);
            }

            if (ProductionOrders != null)
            {
                ProductionOrders.LoadPostProcess(sn);
            }


            CheckProcessJobsQuadTree.SetPreLoadPostProcess(snapshotCheckProcessJobsQuadTree);
            CheckProcessJobsQuadTree.LoadPostProcess(sn); // this will fix the quad tree


        }

       

       
        #endregion

        
    }


    public class OwnerAmmoOfType: ISnapshot
    {
        public List<EntityID> Items;


        private int totalRounds = 0;

        /// <summary>
        /// a convenience sum of the total rounds contained in the list of items
        /// </summary>
        public int TotalRounds
        {
            get
            {
                if (TotalIsDirty)
                {
                    RecomputeTotalRounds();
                }

                return totalRounds;
            }
        }

        /// <summary>
        /// tells whether the total needs recomputing - may also be set from outside...
        /// </summary>
        public bool TotalIsDirty = true;

        public void RecomputeTotalRounds()
        {
            int total = 0;
            for (int i = Items.Count - 1; i >= 0; i--)
            {
               // Entity ammoEntity = Items[i];

                Entity ammoEntity = Entity.FindByID(Items[i]);

                if (ammoEntity != null)
                {
                    total += ammoEntity.Item.Ammunition.NoOfRounds;
                }
                else
                {
                    Items.RemoveAt(i);
                }
            }

            totalRounds = total;
            TotalIsDirty = false;
        }



        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Items = sn.DoList(Items);
            this.TotalIsDirty = sn.DoBool(TotalIsDirty);
            this.totalRounds = sn.DoInt32(totalRounds);
           

            return this;
        }

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

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion

    }
}
