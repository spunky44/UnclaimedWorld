using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.Resources;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.AI.Planners;
using UWGame.SimSide.Overland;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Trade;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Jobs.JobTypes;
using System.Diagnostics;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Collisions;

namespace UWGame.SimSide.Expeditions
{
    public enum ExpeditionID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }


    /// <summary>
    /// marks a center on the map with its own economy...
    /// Important! We will never merge/split expeditions. Only create, delete and update.
    /// </summary>     
    [DebuggerDisplay("{KeyName}")]
    public class Expedition : IHasEntityGroup, IOwner, IHasExposedProperties, ICanIterateEntities, ILookUp<Expedition, ExpeditionID>, ISnapshot 
    {       
        
        public string Name;
        public string KeyName {get; set; }

        private Vector3? center;

        /// <summary>
        /// should be nullable
        /// </summary>
        public Vector3? Center
        {
            get
            {
                return center;
            }
            set
            {               
                
                center = value;

                if (center.HasValue && Collidable != null)
                {
                    Collidable.Center = center.Value.ToVector2();
                }
                   
            }

        }


        /// <summary>
        /// move this to EntityGroup..?
        /// </summary>
      //  public ProductionOrders Stocks;


        public Collidable<Expedition> Collidable
        {
            get;
            set;
        }

        public ExpeditionPolicy Policy;
     

        private FoodExtraction foodExtraction;

        /// <summary>
        /// used in the ledger, does not include dogs
        /// </summary>
        private FoodExtraction independentMemberFoodExtraction;


      //  private Regulator replenishAlertRegulator;


        public Population Population;

        /// <summary>
        /// people with the skills matching these will migrate/work here.
        /// </summary>
      //  public SkillList SkillList = new SkillList();

        /// <summary>
        /// Includes everything: robots, structures, animals...      
        /// </summary>
        public List<EntityID> Members = new List<EntityID>();

        /// <summary>
        /// filtered members, by independent status - these are the ones that can leave
        /// </summary>
        public List<EntityID> IndependentMembers = new List<EntityID>();
        
        public List<EntityID> Workers = new List<EntityID>();


        /// <summary>
        /// households currently having a claim on food and housing
        /// </summary>
        public List<Household> Households = new List<Household>();
        List<HouseholdID> snapshotHouseholds;

      //  public Owner ExpeditionOwner; 


        public JobManager JobManager;
        CyclableID? snapshotJobManager;


        Dictionary<SkillType, int> membersWithSkill = new Dictionary<SkillType,int>();
        HashSet<EntityID> membersWithUniqueSkill = new HashSet<EntityID>();


        private Regulator skillRegulator;

        /// <summary>
        /// don't use yet - is null! TODO!!!
        /// </summary>
        public GroupStatistics Statistics; 

#region IOwner
        private EntityGroup ownerContent;
        private EntityGroupID snapshotOwnerContent;
        public EntityGroup OwnedEntities
        {
            get
            {
                return ownerContent;
            }
        }

        public int NoOfWorkers
        {
            get
            {
                return Workers.Count; // Members.Count;
            }
        }

     
        public bool IsEatable(EntityType entityType)
        {
            return foodExtraction.IsEatable(entityType);            
        }

        public bool IsEatableByIndependentMembers(EntityType entityType)
        {
            return independentMemberFoodExtraction.IsEatable(entityType);
        }

        public Allegiance GetAllegiance
        {
            get
            {
                return this.Allegiance;
            }
        }

      

        public void IterateMembers(Action<Entity> iterateFunction)
        {
            EntityID entityID;
            Entity entity;
            for (int i = Members.Count - 1; i >= 0; i--)
            {
                entityID = Members[i];

                entity = Entity.FindByID(entityID);

                if (entity != null)
                {
                    iterateFunction(entity);
                }
                else
                {
                    Members.RemoveAt(i); // entityID);
                }
            }            
        }

        public void IterateOwnedItems(Action<EntityGroup> iterateFunction)
        {
            iterateFunction(ownerContent);
        }


        public Vector3? Location
        {
            get
            {
                return Center;
            }
        }


     /*   public ExpeditionOwner(Expedition expedition) 
        {
           
            this.Expedition = expedition;

            haulingJobManager = new HaulingJobManager(this);
        }*/


       

        private Allegiances.Allegiance allegiance;
        AllegianceID snapshotAllegiance;
        public Allegiances.Allegiance Allegiance
        {
            get
            {
               
                return allegiance; 
            }
            set { allegiance = value; }
        }

#endregion
        // here we want a Planner reference for non-player allegiances
        private PhysicalNeedsPlanner physicalNeedsPlanner = null;


       // public HumanExpeditionActivities HumanExpeditionActivities;

       
        /// <summary>
        /// uses the Allegiance!
        /// </summary>
        public decimal? TradeCredits
        {
            get
            {
                return allegiance.TradeCredits;
            }

            set
            {
                allegiance.TradeCredits = value;
            }
        }


        public void RemoveGatheringSite()
        {
            gatheringSite = null;
        }

        GatheringSite gatheringSite = null;
        GatheringSiteID? snapshotGatheringSite = null;
        public GatheringSite GatheringSite
        {
            get
            {
                if (gatheringSite == null)
                {
                    GatheringSiteType siteType;

                    siteType = new GatheringSiteType()
                    {
                        arc = new Arc()
                        {
                            Radius = 100,
                            MinAngle = -180,
                            MaxAngle = 180
                        },
                        SeatSize = 20,
                        MaxVisitors = 20,

                    };

                    gatheringSite = new GatheringSite(siteType, Location.Value);

                }
                return gatheringSite;
            }

        }

        /// <summary>
        /// these kinds of hauling jobs between expeditions must be handled separately...
        /// </summary>
      //  public Dictionary<EntityType, List<Job>> ImportJobs = new Dictionary<EntityType, List<Job>>();



     /*   #region Client feedback

        /// <summary>
        /// only used in player feedback     
        /// </summary>
        Dictionary<EntityType, bool> toolReplenishAvailableStates = new Dictionary<EntityType, bool>();
      
        #endregion
        */
        public Expedition()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public Expedition(Allegiance allegiance, string keyName, string name, Vector3? center)
        {
           /* if (!Snapshotter.IsSnapshotting)
            {*/
                AddToLookup();
                ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).AddToLookup();
                ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).AddToLookup();
                ((ILookUp<IOwner, OwnerID>)this).AddToLookup();
            //}

            this.allegiance = allegiance;
            this.Name = name;
            this.KeyName = keyName;
            Center = center;


            ownerContent = new EntityGroup(this, true, true); //, foodExtraction); 

            // this is used in food classification, which we'll use off-site as well
            foodExtraction = new FoodExtraction(this, ownerContent.ID);
            independentMemberFoodExtraction = new FoodExtraction(this, ownerContent.ID, false);

            Policy = new ExpeditionPolicy();

            if (allegiance.Site.IsPlaySite)
            {
                InitPlaySite();
            }
            else
            {
                InitOtherSite();
            }
          
            
            allegiance.Expeditions.Add(this);

            if (allegiance.RepresentativeEntityType.Person != null)
            {
               // HumanExpeditionActivities = new HumanExpeditionActivities();
            }
            else //TMP PlannerStuff just added this to do some testing.
            {
                physicalNeedsPlanner = new PhysicalNeedsPlanner(allegiance, this);
            }


            // play site only? no, othersite expeditions may do spawns too...
            if (allegiance.RepresentativeEntityType.PolledEvents != null)
            {
                List<PolledEventType> list;
                if (allegiance.RepresentativeEntityType.PolledEvents.TryGetValue(Scope.Expedition, out list))
                {
                    foreach (var item in list)
                    {
                        if (Allegiance.Site.IsPlaySite || item.PlaySiteOnly == false)
                        {
                            PolledEvent polledEvent = allegiance.Site.EventManager.AddPolledEvent(item.KeyName, null, this.ID);
                        }
                    }
                }
            }

            CreateRegulators();

            CreateCollidable();

            if (Collidable != null)
            {
                allegiance.Site.PlaySite.ExpeditionRadiusQuadTree.AddCollidable(Collidable);
            }
        }

        public OwnerID GetOwnerID()
        {
            return ((IOwner)this).ID;
        }

        private void CreateRegulators()
        {
            skillRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.3, "Expedition");

           // replenishAlertRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / GameData.Instance.GUIConstants.TimeBetweenReplenishAlerts, "Expedition replenish");
        }

        public static bool CreateFromExpeditionData(ExpeditionData data, Allegiances.Allegiance allegiance, EventAction action, float? sizeFactor, string expeditionKeyName, out string failReason)
        {
            failReason = "";

            Vector3? spawnLocation = null;

            // othersite expeditions do not need to define a location.

            float sizeToUse = data.SizeFactor ?? sizeFactor ?? 1f;

            if (data.Location != null)
            {
                // get the location:
                PropertyResult? result = data.Location.Evaluate(action);

                if (result != null
                    && result.Value.LocationResult.HasValue)
                {
                    spawnLocation = result.Value.LocationResult.Value.ToVector3();
                }
                else
                {
                    failReason = "Location did not evaluate to a result";
                    return false;
                }
            }


            PricesProfile pricesProfile = null;
            if (data.PricesProfile != null)
            {
                pricesProfile = GameData.Instance.AllPricesProfiles[data.PricesProfile];
            }

            Expedition expedition = new Expedition(allegiance, 
                expeditionKeyName ?? data.KeyName, 
                data.Name, spawnLocation);

            if (data.TradeProfile != null)
            {
                if (allegiance.Site.IsPlaySite)
                {
                    throw new Exception("Cannot create a trade manager for a playsite allegiance.");
                }

                CreateTradeManager(expedition);

                expedition.OwnedEntities.TradeManager.FillFromTradeProfile(GameData.Instance.AllTradeProfiles[data.TradeProfile], sizeToUse, pricesProfile);
                
            }
         

            if (data.AvailableForTrade != null)
            {
                if (allegiance.Site.IsPlaySite)
                {
                    throw new Exception("Cannot create a trade manager for a playsite allegiance.");
                }

                CreateTradeManager(expedition);

                // allow override:
                if (data.AvailableForTrade != null)
                {
                    expedition.OwnedEntities.TradeManager.SetTradeProperties(data.AvailableForTrade, pricesProfile);
                }
            }

            if (data.VehiclesProfile != null)
            {
                CreateTradeManager(expedition);

                expedition.OwnedEntities.TradeManager.FillFromVehiclesProfile(GameData.Instance.AllVehiclesProfiles[data.VehiclesProfile], sizeToUse);
            }

            if (data.VehiclesForHire != null)
            {
                CreateTradeManager(expedition);

                expedition.OwnedEntities.TradeManager.SetVehiclesForHire(data.VehiclesForHire); 
            }

            expedition.Policy = ExpeditionPolicy.CreateFromPolicyData(data.PolicyData);

            if (data.PopulationData != null)
            {
                expedition.Population = Population.CreateFromPopulationData(expedition, data.PopulationData);
               // expedition.Population.SpawnStartingPopulation();
            }

            // spawn the starting entities:

            if (expedition.Population != null)
            {              
                expedition.Population.SpawnStartingPopulation();                
            }
            
            if (data.StructuresProfile != null)
            {
                expedition.OwnedEntities.SpawnOthersiteStartingStructures(data.StructuresProfile);
            }
                      
            if (expedition.OwnedEntities.TradeManager != null)
            {
                expedition.OwnedEntities.TradeManager.SpawnStartingTradeItems(); 
                expedition.OwnedEntities.TradeManager.SpawnStartingVehicles(); //data.VehiclesProfile);
               // expedition.OwnedEntities.TradeManager.SetRandomStartDemand();
            }
           

            return true;

        }

        private static void CreateTradeManager(Expedition expedition)
        {
            if (expedition.OwnedEntities.TradeManager == null)
            {
                expedition.OwnedEntities.TradeManager = new TradeManager(expedition.OwnedEntities);
            }
        }

        public void AdoptTierPolicy(TierType tier, RatingTypes rating)
        {
            Policy.AdoptTierPolicy(tier, rating);

            float minimum = Policy.GetPolicyMinimum(rating);

            foreach (var item in IndependentMembers)
            {
                Entity entity = Entity.FindByID(item);
                if (entity != null)
                {
                    if (entity.PersonEntity != null)
                    {
                        entity.PersonEntity.Personality.SetPrinciplesToMinimum(rating, minimum);

                    }
                }
                
            }
        }

       /// <summary>
       /// clamp the number at half the number of agents...
       /// </summary>
       /// <returns></returns>
        public int NoOfIndependentMembersAllowedToSleep()
        {
          /*  if (Policy.IndependentsToStayAwake.HasValue)
            {
                return Common.ClampBottom(IndependentMembers.Count - Policy.IndependentsToStayAwake.Value, 0);                
            }*/

            int noAllowedToSleep;

            if (Policy.FractionIndependentsAllowedToSleep.HasValue)
            {
                noAllowedToSleep = (int)((float)IndependentMembers.Count * Policy.FractionIndependentsAllowedToSleep.Value);
            }
            else if (Policy.IndependentsAllowedToSleep.HasValue)
            {
                noAllowedToSleep = Policy.IndependentsAllowedToSleep.Value;
                //return Math.Min(IndependentMembers.Count, Policy.IndependentsAllowedToSleep.Value);
            }
            else
            {
                noAllowedToSleep = IndependentMembers.Count;
            }

            // clamp:
            int minimumAllowedToSleep = Common.ClampBottom(IndependentMembers.Count / 2, 1);

            return Math.Max(minimumAllowedToSleep, noAllowedToSleep);
        }

        public int GetNumberOfSleepingIndependents()
        {
            int numberOfAgentsSleeping = 0;
           // float numberOfAgents =  The.Sim.PlaySite.Persons.Count;

            EntityID entityID;
            for (int i = IndependentMembers.Count - 1; i >= 0; i--)
            {
                entityID = IndependentMembers[i];
                Entity entity = Entity.FindByID(entityID);
                if (entity != null)
                {
                    if (entity.Intelligence.IsSleeping())
                    {
                        numberOfAgentsSleeping++;
                    }
                }
                else
                {
                    RemoveMemberID(entityID);
                }
            }

            /*
            for (int i = 0; i < numberOfAgents; i++)
            {
                if (The.Sim.PlaySite.Persons[i].Intelligence.IsSleeping() == true)
                {
                    numberOfAgentsSleeping++;
                }
            }*/

            return numberOfAgentsSleeping;
        }

        /// <summary>
        /// should expeditions be able to move betweeen othersite and playsite..?
        /// </summary>
        public void InitPlaySite() //bool computeAuxiliaryMaps)
        {
            JobManager = new JobManager(this.OwnedEntities);

            if (allegiance.RepresentativeEntityType.IntelligenceType.CanHaul == true)
            {
                ownerContent.HaulingJobManager = new HaulingJobManager(ownerContent);
            }

            ownerContent.OtherJobManager = new OtherJobManager(ownerContent);

            if (allegiance.RepresentativeEntityType.IntelligenceType.CanHunt == true)
            {
                ownerContent.HuntingJobManager = new HuntingJobManager(ownerContent);
            }
            
            UpdateOperatingAreas();

            

        }


        public void AddHousehold(Household household)
        {
            if (!Households.Contains(household))
            {
                Households.Add(household);
            }
        }

        public void RemoveHousehold(Household household)
        {
            Households.Remove(household);

            Residence.RemoveHousehold(allegiance.SharedKnowledge, household); //, household.Home);

        }

        private void InitOtherSite()
        {
           // TradeAmounts = new TradeAmounts

        }

        public void GetAvailableVehicles(RouteType? routeType, bool airRoute, double distance, ref Dictionary<EntityType, List<Entity>> vehicles, int? noOfVehicles = 1)
        {
            //List<Entity> vehicles  = null;
            int vehiclesFound = 0;
            foreach (var vehicle in OwnedEntities.Vehicles)
            {
                Entity vehicleEntity = Entity.FindByID(vehicle);

                if (VehicleIsAvailable(vehicleEntity)                  
                    && (((VehicleContainerType)vehicleEntity.EntityType.ContainerType).CanUseRoute(routeType, airRoute, distance))) // ??
                {
                    Common.AddToMultiList(vehicles,
                        vehicleEntity.EntityType, vehicleEntity);

                    vehiclesFound++;

                    if (noOfVehicles.HasValue && vehiclesFound == noOfVehicles)
                        return;
                }
            }
        }

        public bool HasAavailableVehicle()
        {
            Dictionary<EntityType, List<Entity>> vehicles = null;
            GetAvailableVehicles(null, ref vehicles, 1);

            return vehicles != null;
        }


        /// <summary>
        /// can either supply a route type or a vehicle entity type
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="vehicles"></param>
        /// <param name="type"></param>
        /// <param name="noOfVehicles"></param>
        public void GetAvailableVehicles(EntityType type, ref Dictionary<EntityType, List<Entity>> vehicles, int? noOfVehicles = 1)
        {          
            int vehiclesFound = 0;
            foreach (var vehicle in OwnedEntities.Vehicles)
            {
                Entity vehicleEntity = Entity.FindByID(vehicle);

                if (VehicleIsAvailable(vehicleEntity)
                    && (type == null || vehicleEntity.EntityType == type))
                {
                    Common.AddToMultiList(ref vehicles, 
                        vehicleEntity.EntityType, vehicleEntity);

                    vehiclesFound++;

                    if (noOfVehicles.HasValue && vehiclesFound == noOfVehicles)
                        return;
                }
            }
           
        }

        private bool VehicleIsAvailable(Entity vehicleEntity)
        {
            if (vehicleEntity != null
                    && vehicleEntity.IsCompleted()
                    && Entity.IsFunctional(vehicleEntity))
            {
                if (vehicleEntity.AssignedToJob == null)
                {
                    return true;
                }
                else
                {
                    // this was once set to an outdated job:
                    Job job = LookUp<Job, JobID>.FindByID(vehicleEntity.AssignedToJob);
                    if (job == null)
                    {
                        vehicleEntity.AssignedToJob = null;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }



        public List<IKnownEntityData> GetWorkingTerminals(SharedKnowledge sharedKnowledge, TerminalType.TypesOfTerminal? typeOfTerminal)
        {
            List<IKnownEntityData> terminals = null;

            List<EntityID> terminalsOfType;
            if (typeOfTerminal.HasValue)
            {
                if (OwnedEntities.Terminals.TryGetValue(typeOfTerminal.Value, out terminalsOfType))
                {
                    foreach (var structure in terminalsOfType) //Structures)
                    {
                        GetWorkingTerminal(sharedKnowledge, ref terminals, structure);
                    }
                }
            }
            else
            {
                foreach (var list in OwnedEntities.Terminals)
                {
                    foreach (var structure in list.Value)
                    {
                        GetWorkingTerminal(sharedKnowledge, ref terminals, structure);                  
                    }
                }
            }

            return terminals;
        }

        private static void GetWorkingTerminal(SharedKnowledge sharedKnowledge, ref List<IKnownEntityData> terminals, EntityID structure)
        {
            IKnownEntityData data;
            if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(structure, out data)))
            {

                if (data != null
                    && data.IsCompleted()
                    && Entity.IsFunctional(data))
                //&& structureEntity.EntityType.ContainerType is TerminalContainerType) // StructureType.IsHelipad) 
                {
                    Common.AddToList(ref terminals, data);
                }
            }
           
        }

        /// <summary>
        /// not used...
        /// </summary>
        /// <returns></returns>
        public List<EntityID> GetMembersReadyToEmigrate()
        {
            List<EntityID> result = null;

            foreach (var item in Members)
            {
                Entity entity = Entity.FindByID(item);
                if (entity != null)
                {
                    if (entity.Intelligence.HasDesireToEmigrate(The.InGameUI.UIAllegiance))
                    {
                        Common.AddToList(ref result, item);
                    }
                }
            }

            return result;

        }

      

        public void Destroy()
        {
            JobManager.Destroy();

            ownerContent.Destroy();

            RemoveIDEntry();
            ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).RemoveIDEntry();
            ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).RemoveIDEntry();
            ((ILookUp<IOwner, OwnerID>)this).RemoveIDEntry();


            if (allegiance != null && allegiance.RepresentativeEntityType.PolledEvents != null)
            {
                List<PolledEventType> list;
                if (allegiance.RepresentativeEntityType.PolledEvents.TryGetValue(Scope.Expedition, out list))
                {
                    foreach (var eventType in list)
                    {
                        allegiance.Site.EventManager.RemovePolledEvent(eventType, null, this.ID, null);
                    }
                }

            }

            if (Collidable != null)
            {
                Collidable.Delete(); // removes from coll manager
                //The.Sim.PlaySite.PlaySite.ExpeditionRadiusQuadTree.RemoveCollidable(Collidable);
            }
        }


       /* void foodExtraction_FoodProcessesChanged()
        {
            ownerContent.SetFoodDirty();
            
        }*/

        
/*
        /// <summary>
        /// call this from AI evaluators to give player feedback without extra overhead
        /// </summary>
        /// <param name="toolType"></param>
        /// <param name="status"></param>
        public void SetToolReplenishStatus(EntityType toolType, bool ownsItem) // ReplenishStatus status)
        {          
            if (!ownsItem && replenishAlertRegulator.IsReady()) //(!toolReplenishAvailableStates.TryGetValue(toolType, out currentValue) || currentValue == true))
            {
                The.Client.LogOutOfFuel(toolType); // show an alert as well
            }

            toolReplenishAvailableStates[toolType] = ownsItem;
           
        }

        public bool GetToolReplenishStatus(EntityType toolType)
        {
            bool ownsItem; // 
            if (toolReplenishAvailableStates.TryGetValue(toolType, out ownsItem))
            {
                return ownsItem;
            }
            else return true; // true is default...
        }
        */


        private void UpdateOperatingAreas()
        {
            // TODO! iterate over all expeditions and divide the map...
            // make area depend on size???

            bool isOnMap = allegiance.Site.IsPlaySite;

            if (allegiance.AllegianceType == AllegianceType.Other)//TMP Workaround. Now the player controls the map.
            {
                return;
            }

            TerrainTile[] column;
            TerrainTile tile;
            for (int x = 0; x < The.Map.mapTileWidth; x++)
            {
                column = The.Map.TileMap[x];

                for (int y = 0; y < The.Map.mapTileHeight; y++)
                {
                    tile = column[y];

                    if (isOnMap)
                    {
                        tile.OperatingAreaOf = this;
                    }
                    else
                    {
                        // no longer on map - reset:
                        if (tile.OperatingAreaOf == this)
                        {
                            tile.OperatingAreaOf = null;
                        }
                    }
                }
            }
        }

        public void MergeExpeditions(Expedition expeditionToDissappear)
        {
            //TODO: 1. move all item ownership

            // 2. cancel all jobs in the deleted expedition
            
        }

        
        public void AddMember(Entity member)
        {
            if (member.EntityType.IntelligenceType != null) //member.EntityType.Person != null)
            {
                //member.PersonEntity.CurrentExpedition = this;
                member.Intelligence.CurrentExpedition = this;

                if (member.Intelligence.IsIndependent())
                {
                    IndependentMembers.Add(member.ID);
                }

                if (member.EntityType.IntelligenceType.CanDoJobs == true)
                {
                    Workers.Add(member.ID);
                }
            }
                    
            Members.Add(member.EntityID);

            UpdateWhenMembersChange();


        }

        private void UpdateWhenMembersChange()
        {
            UpdateSkills();

            Allegiance.HandleGroupMembersChanged(foodExtraction);
            Allegiance.HandleGroupMembersChanged(independentMemberFoodExtraction);

            OwnedEntities.RecomputeFoodConsumeRate();
        }
        
        public void RemoveMember(Entity member)
        {
            if (member.EntityType.IntelligenceType != null) 
            {
                if (member.Intelligence.CurrentExpedition == this)
                {
                    member.Intelligence.CurrentExpedition = null;
                }
            }           

            RemoveMemberID(member.EntityID);

            UpdateWhenMembersChange();

            //Statistics.NotifyPopulationChanged(Members.Count); // TODO

        }

        /// <summary>
        /// only for cleanup!
        /// </summary>
        /// <param name="member"></param>
        public void RemoveMemberID(EntityID member)
        {
            IndependentMembers.Remove(member);
            Members.Remove(member);
            Workers.Remove(member);

            UpdateSkills();

            Allegiance.HandleGroupMembersChanged(foodExtraction);
            Allegiance.HandleGroupMembersChanged(independentMemberFoodExtraction);
        }

        /// <summary>
        /// is false for single-member expeditions
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public bool HasUniqueSkill(Entity agent)
        {
            if (Members.Count == 1)
            {
                return false;
            }
            else
            {
                return membersWithUniqueSkill.Contains(agent.ID);
            }
        }


        public bool SkillIsUnique(SkillType skillType)
        {
            if (skillType == null)
                return false;

            int members;
            if (membersWithSkill.TryGetValue(skillType, out members) && members == 1)
            {
                return true;
            }

            return false;
        }
       

        public bool HasSkill(SkillType skillType)
        {
            if (skillType == null)
                return true;

            int members;
            if (membersWithSkill.TryGetValue(skillType, out members) && members > 0)
            {
                return true;
            }

            return false;

            /*
            bool expeditionHasSkill = false;
            Entity entity;
            foreach (var member in Members) 
            {
                entity = Entity.FindByID(member);

                if (entity != null && entity.Intelligence.HasSkill(skillType))
                {
                    expeditionHasSkill = true;
                    break;
                }
            }

            return expeditionHasSkill;*/
        }

       

        /*
        public void UpdateFoodEntitlement()
        {
            // calculate from food stocks:
            int entitledPersons = 0;
            for (int i = 0; i < Households.Count; i++)
            {
                if (Households[i].IsEntitled())
                {
                    entitledPersons += Households[i].Members.Count;
                }
            }

            if (entitledPersons > 0)
            {
                int entitledMeat = ExpeditionOwner.OwningBody.Items[GameData.Instance.MeatType].Count / entitledPersons;
                int entitledGrains = ExpeditionOwner.OwningBody.Items[GameData.Instance.StaplesType].Count / entitledPersons;
                int entitledVegs = ExpeditionOwner.OwningBody.Items[GameData.Instance.VegetablesType].Count / entitledPersons;

                if (FoodEntitlementPerPerson.Count == 0)
                {
                    FoodEntitlementPerPerson.Add(GameData.Instance.MeatType, entitledMeat);
                    FoodEntitlementPerPerson.Add(GameData.Instance.StaplesType, entitledGrains);
                    FoodEntitlementPerPerson.Add(GameData.Instance.VegetablesType, entitledVegs);
                }
                else
                {
                    FoodEntitlementPerPerson[GameData.Instance.MeatType] = entitledMeat;
                    FoodEntitlementPerPerson[GameData.Instance.StaplesType] = entitledGrains;
                    FoodEntitlementPerPerson[GameData.Instance.VegetablesType] = entitledVegs;
                }

                TotalEntitledPerPerson = entitledMeat + entitledGrains + entitledVegs;

                for (int i = 0; i < Households.Count; i++)
                {
                    Households[i].ResetEntitledFoodHaulingJobs();
                }
            }
        }
        */

        /// <summary>
        /// update the represented skills
        /// </summary>
        private void UpdateSkills()
        {
           // reset
            foreach (var item in membersWithSkill.Keys.ToList())
            {
                membersWithSkill[item] = 0;
            }

            IterateMembers(e => AddSkills(e));

            membersWithUniqueSkill.Clear();
            IterateMembers(e => AddToMembersWithUniqueSkills(e));    

        }


        private void AddSkills(Entity entity)
        {
            foreach (var item in entity.Intelligence.Skills)
	        {
		        if (item.Value.Value > GameData.Instance.Constants.MinimumSkillValueToUse)
                {
                    Common.AddToDictWithSums(membersWithSkill, item.Key);
                }
            }
        }

        private void AddToMembersWithUniqueSkills(Entity entity)
        {
            foreach (var item in entity.Intelligence.Skills)
            {
                if (item.Value.Value > GameData.Instance.Constants.MinimumSkillValueToUse
                    && SkillIsUnique(item.Key))
                {
                    membersWithUniqueSkill.Add(entity.ID);
                    return;
                }
            }

        }

        /// <summary>
        /// no sleepy updates...
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (JobManager != null)
            {
                JobManager.Update(gameTime);
            }

            if (physicalNeedsPlanner != null)
            {
                physicalNeedsPlanner.Update(gameTime);
            }

          /*  if (HumanExpeditionActivities != null)
                HumanExpeditionActivities.Update(gameTime);
            */

            if (Statistics != null)
                Statistics.Update(gameTime);

            if (skillRegulator.IsReady())
            {
                UpdateSkills();
            }

            if (Population != null)
            {
                Population.Update(gameTime);
            }

            UpdateReplenishAvailableStates();
           
        }


        private void UpdateReplenishAvailableStates()
        {


        }

        private bool GiveFoodToHousehold(EntityType foodType, Household household)
        {
            // select a food item to give
            List<EntityID> items = ownerContent.Items[foodType];

            return GiveFreeItemInList(household, items);          

        }

        /// <summary>
        /// use this later???
        /// </summary>
        /// <param name="household"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private static bool GiveFreeItemInList(Household household, List<EntityID> items)
        {
          /*  foreach (EntityID item in items)
            {
                if (item.IsUnassigned())
                {
                    item.ChangeOwnership(household.Ownership);

                    return true;
                }
            }*/

            return false;
        }

      

        /*

        public decimal? GetTradePrice(EntityType itemType)
        {
            if (HumanExpeditionActivities != null)
            {
                return HumanExpeditionActivities.TradeManager.GetTradePrice(itemType);
            }

            return null;
        }

        public decimal? GetVehicleForHirePrice(EntityType itemType)
        {
            if (HumanExpeditionActivities != null)
            {
                return HumanExpeditionActivities.TradeManager.GetPriceToHire(itemType);
            }

            return null;
        }
        */
        private void CreateCollidable()
        {
            Vector2? location = null;
            if (Location.HasValue) 
            {
                location = Location.Value.ToVector2();

                float size = allegiance.GetForageAndHuntingRadius();

                Collidable = new Collidable<Expedition>(this, location, new Vector2(size));
                Collidable.BeCircle();   

                //The.Sim.PlaySite.PlaySite.ExpeditionRadiusQuadTree.AddCollidable(Collidable);
            }                   
        }

        public static Expedition FindByID(ExpeditionID id)
        {
            return LookUp<Expedition, ExpeditionID>.FindByID(id);
        }


        #region IHasExposedProperties

        public void GetChildren(string key, ref List<IHasExposedProperties> listOfChildren, FilterCondition filter,
            EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, 
            SharedKnowledge getterKnowledge = null)
        {
            // first get the list of items:
            //maybe put these keys in a dictionary, it would make validation possible
            bool wasFiltered = false;
            switch (key)
            {
                case "OwnedEntities":
                    {
                        ownerContent.GetEntities(filter, ref listOfChildren, out wasFiltered, 
                            triggeringEntity, targetEntity, polledEventSource);

                        break;
                    }

            }

            // then apply filter:
            if (wasFiltered == false && filter != null)
            {
                Site.FilterChildren(listOfChildren, filter, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
            }

        }

        private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();

        public string GetCaption(string captionKey)
        {
            return null;
        }

        public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
        {
            PropertyResult? result = null;
            if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
            {
                result = exposedPropertyValueFunctions[propertyKey].Invoke(this, getterKnowledge, parent);
            }
            else
            {
                PropertyResult customResult;
                if (customFields != null && customFields.TryGetValue(propertyKey, out customResult))
                {
                    result = customResult;
                }
            }

            return result;
        }

        public string GetDefaultCaption(string propertyKey)
        {
            return Name;
        }

        public void GetDefaultKey(out string PropertyKey)
        {
            PropertyKey = null;
        }

       
        public EntityID? GetEntityID()
        {
            return null;
        }

        public bool GetIsSeenDirectly() //SharedKnowledge sharedKnowledge)
        {
            return true; // check if in FOW via SharedKnowlege param, if needed...
        }

        public void SetPropertyValue(string propertyKey, PropertyResult? value)
        {
            Entity.SetPropertyValue(ref customFields, propertyKey, value);
        }

        private Dictionary<string, PropertyResult> customFields;

        #endregion

        #region Exposed properties

       

      
       

        #endregion


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // IDs
            IDCounter = sn.DoEnum(IDCounter);
            id = SnapshotID(sn, id);
            hasEntityGroupID = sn.DoEnum(hasEntityGroupID);
            canIterateEntitiesID = sn.DoEnum(canIterateEntitiesID);
            ownerID = sn.DoEnum(ownerID);
            this.customFields = sn.DoDictionary(customFields);
            this.snapshotAllegiance = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(allegiance);
            this.Center = sn.DoVector3Nullable(Center);
            this.snapshotGatheringSite = sn.SnapshotID<GatheringSite, GatheringSiteID>(gatheringSite);
            this.foodExtraction = (FoodExtraction)sn.DoISnapshot(foodExtraction);
            this.independentMemberFoodExtraction = (FoodExtraction)sn.DoISnapshot(independentMemberFoodExtraction);

            if (sn.mode != Snapshotter.Mode.Load)
            {
                // missing households caused a crash in testing..
#if DEBUG
                foreach (var item in Households)
                {
                    if (item.ID == HouseholdID.Invalid)
                    {
                        System.Diagnostics.Debug.Assert(false, "Household is invalid");
                    }
                }
#endif
                this.snapshotHouseholds = Households.Select(h => h.ID).ToList(); 
            }
            this.snapshotHouseholds = sn.DoList(snapshotHouseholds); 

            //this.ImportJobs =  not implemented yet
            this.snapshotOwnerContent = (EntityGroupID)sn.SnapshotID<EntityGroup, EntityGroupID>(ownerContent);
            this.snapshotJobManager = sn.SnapshotID<ICyclable, CyclableID>(JobManager);
            this.Members = sn.DoList(Members);
            this.IndependentMembers = sn.DoList(IndependentMembers);
            this.KeyName = sn.DoString(KeyName);
            this.Name = sn.DoString(Name);
          //  this.PlayerSetWorkPriority = sn.DoEnum(PlayerSetWorkPriority);          
            //this.SkillList - no implemented yet
          
         //   this.toolReplenishAvailableStates = sn.DoDictionary(toolReplenishAvailableStates);

            this.physicalNeedsPlanner = (PhysicalNeedsPlanner)sn.DoISnapshot(physicalNeedsPlanner);          
            this.Policy = (ExpeditionPolicy)sn.DoISnapshot(Policy);
            this.membersWithSkill = sn.DoDictionary(membersWithSkill);
            this.membersWithUniqueSkill = sn.DoHashSet(membersWithUniqueSkill);
            this.Workers = sn.DoList(Workers);
            this.Population = (Population)sn.DoISnapshot(Population);

            sn.Postpone(Statistics);

            sn.Ignore(JobManager);
            sn.Ignore(Households);
            sn.Ignore(exposedPropertyValueFunctions);
    
            return this;
        }


      
        public void LoadPostProcess(Snapshotter sn)
        {

            sn.RegisterLoadPostProcessCall(this);

            allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
            gatheringSite = LookUp<GatheringSite, GatheringSiteID>.FindByID(snapshotGatheringSite);
            Households = snapshotHouseholds.Select(h => LookUp<Household, HouseholdID>.FindByID(h)).ToList();
            // missing households caused a crash in testing..
            Households.RemoveAll(h => h == null);

            ownerContent = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnerContent);
            JobManager = (JobManager)LookUp<ICyclable, CyclableID>.FindByID(snapshotJobManager);

            
            if (physicalNeedsPlanner != null)
            {
                physicalNeedsPlanner.LoadPostProcess(sn);
            }

            if (foodExtraction != null)
            {
                foodExtraction.LoadPostProcess(sn);
            }

            if (independentMemberFoodExtraction != null)
            {
                independentMemberFoodExtraction.LoadPostProcess(sn);
            }

            if (Policy != null)
            {
                Policy.LoadPostProcess(sn);
            }

            if (Population != null)
            {
                Population.LoadPostProcess(sn);
                Population.Expedition = this;
            }

            CreateRegulators();

          /*  if (HumanExpeditionActivities != null)
            {
                HumanExpeditionActivities.LoadPostProcess(sn);
            }*/

            CreateCollidable();
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

        #endregion

        #region ILookup

        private ExpeditionID id = ExpeditionID.Invalid;
        static ExpeditionID IDCounter = ExpeditionID.First;

        public ExpeditionID ID
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

        public ExpeditionID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ExpeditionID.Max)
            {
                throw new Exception("Astounding, ExpeditionID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public ExpeditionID SnapshotID(Snapshotter sn, ExpeditionID id)
        {
            return (ExpeditionID)sn.DoEnum(id);
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
            if (ID != ExpeditionID.Invalid)
                LookUp<Expedition, ExpeditionID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = ExpeditionID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<Expedition, ExpeditionID>.Remove(this);
        }

        void ILookUp<Expedition, ExpeditionID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ExpeditionID.First;
        }

        void ILookUp<Expedition, ExpeditionID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<Expedition, ExpeditionID>.Create();
        }

        #endregion

        #region CanIterateEntitiesID ILookup

        public CanIterateEntitiesID canIterateEntitiesID;
        CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ID
        {
            get
            {
                return canIterateEntitiesID;
            }
        }

        CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.GetUniqueID()
        {
            return HasMembers.GetUniqueID();
        }

      

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.AddToLookup()
        {
            canIterateEntitiesID = ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).GetUniqueID();

            if (canIterateEntitiesID != CanIterateEntitiesID.Invalid)
            {
                LookUpICanIterateEntities.Add(canIterateEntitiesID, this); // uses special class!
            }
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.RemoveIDEntry()
        {
            LookUpICanIterateEntities.Remove(this);  // uses special class!
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.SetInvalid()
        {
            canIterateEntitiesID = CanIterateEntitiesID.Invalid;
        }

        void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

       
        #endregion


        #region HasEntityGroupID ILookup

        HasEntityGroupID hasEntityGroupID;
        HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.ID
        {
            get
            {
                return hasEntityGroupID;
            }
        }

        HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.GetUniqueID()
        {
            return HasEntityGroup.GetUniqueID();
        }

       
        void ILookUp<IHasEntityGroup, HasEntityGroupID>.AddToLookup()
        {
            hasEntityGroupID = ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).GetUniqueID();

            if (hasEntityGroupID != HasEntityGroupID.Invalid)
            {
                LookUpHasEntityGroup.Add(hasEntityGroupID, this); // uses special class!
            }
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.RemoveIDEntry()
        {
            LookUpHasEntityGroup.Remove(this);  // uses special class!
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.SetInvalid()
        {
            hasEntityGroupID = HasEntityGroupID.Invalid;
        }

        void ILookUp<IHasEntityGroup, HasEntityGroupID>.CreateLookupCollection() // interface method - does nothing...
        {

        }


        #endregion

        #region OwnerID ILookup

        OwnerID ownerID;
        OwnerID ILookUp<IOwner, OwnerID>.ID
        {
            get
            {
                return ownerID;
            }
        }

        OwnerID ILookUp<IOwner, OwnerID>.GetUniqueID()
        {
            return Owner.GetUniqueID();
        }

      
        void ILookUp<IOwner, OwnerID>.AddToLookup()
        {
            ownerID = ((ILookUp<IOwner, OwnerID>)this).GetUniqueID();

            if (ownerID != OwnerID.Invalid)
            {
                LookUpOwners.Add(ownerID, this); // uses special class!
            }
        }

        void ILookUp<IOwner, OwnerID>.RemoveIDEntry()
        {
            LookUpOwners.Remove(this);  // uses special class!
        }

        void ILookUp<IOwner, OwnerID>.ResetIDCounter() // interface method - does nothing... Sim will call ResetIDCounter.
        {

        }

        void ILookUp<IOwner, OwnerID>.SetInvalid()
        {
            ownerID = OwnerID.Invalid;
        }

        void ILookUp<IOwner, OwnerID>.CreateLookupCollection() // interface method - does nothing...
        {
        }
               

        #endregion





    }
}
