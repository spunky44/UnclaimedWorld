using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.AI;
using System.Diagnostics;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Jobs
{
    /*
         tasks:
         * create haulingjobs to move items to storage
         * create haulingjobs to transport items between expeditions    
     * 
     *  Make the manager able to work on EntityGroup items instead of owned items. In this way, critters can haul too.
         */

    public class HaulingJobManager : ICyclable
    {
        private Regulator regulator;

      

        // 'constants'
        static float oneOverBaseSpeedOnFoot;
        static float oneOverBaseSpeedByAir;
        static float takeOffAndLandTime;

        double airliftCapacityScore;

        // phases
        private enum Phase { CreateAllItemsList, GatherAllStorageLocations, CreateCombos, ScoreCombos, CreateHaulingJobs }
        private Phase phase = Phase.CreateCombos;


        int itemCounter = 0;

        const int itemsPerCycle = 30;

        // progress variables
        int comboCounter = 0;

        const int combosPerCycle = 30;

        //     int itemStorageComboProgress;


        // a sorted list of best storage locations, delete from the list as they fill up...
        //  Dictionary<Owner, Dictionary<ItemType, List<Storage>>> cachedBestLocations;

        List<StorageLocation> AllStorageLocations = new List<StorageLocation>();

        // we also need a list of all items, cannot 
        List<IKnownEntityData> allItems;

        List<ItemStorageCombo> allCombos = new List<ItemStorageCombo>();
        //   Dictionary<Item, StorageLocation> AssignedStorageLocation;

        private Dictionary<EntityType, Dictionary<StorageLocation, float>> cachedItemStorageScores = new Dictionary<EntityType, Dictionary<StorageLocation, float>>();
        private Dictionary<Point, Dictionary<Point, double>> cachedItemLocationScores = new Dictionary<Point, Dictionary<Point, double>>();

        MethodID notifyWhenRegionSearchIsFinished;

        /// <summary>
        /// the manager belongs to an owner
        /// </summary>
        EntityGroup parent;
        EntityGroupID snapshotParent;

        Point? storageCenterMapPosition;

       
        List<HaulingJob> outdatedJobs = new List<HaulingJob>();

        public double? UpdateInterval
        {
            get
            {
                return 10d;
            }
        }

        public bool IsPaused
        {
            get { return isWaiting; }
        }

        public double StartedOnTimeInSeconds { get; set; }
        public static double totalComputationAllInstancesInSeconds;
        public double TotalComputationAllInstancesInSeconds
        {
            get
            {
                return totalComputationAllInstancesInSeconds;
            }
            set
            {
                totalComputationAllInstancesInSeconds = value;
            }
        }

        public double ComputationTimeSpentInSeconds { get; set; }

        private bool isWaiting = false;

        public HaulingJobManager()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public HaulingJobManager(EntityGroup owner) 
        {
            AddToLookup();

            this.parent = owner;

            notifyWhenRegionSearchIsFinished = ActionLookup.AddWithNewID(NotifyWhenRegionSearchIsFinished);

           
            oneOverBaseSpeedOnFoot = 1f /   
                (GameData.Instance.AllEntityTypes["entity:human"].LocomotorType.LeggedLocomotorType.WalkNormalSpeed * PlainsType.Instance.MovementFactor(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.None));

            EntityType representativeAircraft;

            if (GameData.Instance.AllEntityTypes.TryGetValue("entity:skimmer", out representativeAircraft))
            {
                // TODO: make this better... with constants?

                oneOverBaseSpeedByAir = 1f /
                    representativeAircraft.LocomotorType.LeggedLocomotorType.WalkNormalSpeed;

                if (representativeAircraft.ContainerType == null)
                    takeOffAndLandTime = 17;//MLo fixing crash... I don't know how long a takeoff/land is
                else
                    takeOffAndLandTime = 2 * ((VehicleContainerType)representativeAircraft.ContainerType).Aircraft.EstimatedTakeOffLandingTime;
            }

            // UWGame.SimSide.Instance.AllItems.ListItemRemoved += new ObservableList<Item>.ListItemRemovedHandler(AllItems_ListItemRemoved);

            CreateRegulators();
        }

        void CreateRegulators()
        {
            regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / UpdateInterval.Value, "HaulingJobManager");
        }

        public void Update(GameTime gameTime)
        {
            //return;

            if (!The.Sim.CycleManager.IsRegistered(this))
            {
                double milliSecondsSinceLastReady = 0;
                if (regulator.IsReady(ref milliSecondsSinceLastReady))
                {
                    
                    phase = Phase.CreateAllItemsList;
                    itemCounter = 0;
                    The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
                }
            }

        }

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("HaulingJobManager {0}: {1}", ID, phase));

        }

        public bool CycleOnce()
        {
            switch (phase)
            {
                case Phase.CreateAllItemsList:
                    {
                        if (CreateAllItemsList())
                        {
                            if (allItems.Count > 0)
                            {
                                phase = Phase.GatherAllStorageLocations;
                            }
                            else return true; // no items - we are done!!!
                        }
                        return false;
                    }
                case Phase.GatherAllStorageLocations:
                    {
                        if (GatherAllStorageLocations(/*true,*/ parent, AllStorageLocations, out storageCenterMapPosition))
                        {
                            if (storageCenterMapPosition != null)
                            {
                                phase = Phase.CreateCombos;
                            }
                            else return true; // why haul anything? we have nowhere to put it!
                        }
                        return false;
                    }
                case Phase.CreateCombos:
                    {
                        if (CreateAllCombos())
                        {
                            // we do this now...
                            airliftCapacityScore = CalculateAirliftCapacity(parent);
                            cachedItemLocationScores.Clear();
                            cachedItemStorageScores.Clear();

                            comboCounter = 0;

                            phase = Phase.ScoreCombos;
                        }

                        return false;
                    }
                case Phase.ScoreCombos:
                    {
                        if (ScoreAllCombos())
                        {
                            phase = Phase.CreateHaulingJobs;

                            assignedItems.Clear();

                            comboCounter = 0;
                        }

                        return false;
                    }
                case Phase.CreateHaulingJobs:
                    {
                       
                        if (CreateAllHaulingJobs())
                        {
                            return true;   
                        }

                        return false;
                    }
            }

            return true;
        }


        private static Result ScoreCombo(RegionMap regionMap, Allegiances.Allegiance allegiance, double airliftCapacityScore,
            Dictionary<EntityType, Dictionary<StorageLocation, float>> cachedItemStorageScores, Dictionary<Point, Dictionary<Point, double>> cachedItemLocationScores, MethodID? notifyWhenFinished,
            ItemStorageCombo combo, IKnownEntityData itemData,
            out double score, out float damageScore, bool getPathsNow)
        {
            score = 0f;
            damageScore = 1f;

            float stockpileScore = ScoreStockpileSettings(combo, itemData);

            if (itemData.EntityID == (EntityID)12 && combo.StorageLocation.NormalStorage != null) 
            {

            }

            if (stockpileScore > -1f)
            {                
                // give a score for storing an pitem under these conditions - would it degrade?
                damageScore = ScoreItemStorage(combo, itemData, cachedItemStorageScores, allegiance.SharedKnowledge);

                if (damageScore > -1f)
                {
                    if (itemData.EntityID == (EntityID)3 && stockpileScore > -1f) // && combo.StorageLocation != null && combo.StorageLocation.TradeOfferStorage.HasValue)
                    {

                    }

                    // give a score for storing an item at this location (including the cost of moving it from its present location)
                    double locationScore;
                    if (ScoreItemLocation(regionMap, allegiance, combo, itemData, airliftCapacityScore, cachedItemLocationScores, notifyWhenFinished, out locationScore, getPathsNow) == Result.Wait)
                    {
                        score = 0;
                        return Result.Wait;
                    }

                    if (combo.StorageLocation != null && combo.StorageLocation.TradeOfferStorage.HasValue) 
                    {
                        // give a big bonus to trade offers - should always be higher than all other storage options, only trade stations should vary

                        if (itemData.EntityID == (EntityID) 24633)
                        {

                        }

                        score = 0.5 * damageScore + 0.15 * locationScore + 0.1 * stockpileScore + 10f;  
                    }
                    else
                    {
                        score = 0.7 * damageScore + 0.2 * locationScore + 0.1 * stockpileScore; 
                    }
                }
            }

            return Result.OK;
        }

     
        private static float ScoreStockpileSettings(ItemStorageCombo combo, IKnownEntityData itemData)
        {            
            if (combo.StorageLocation.Stockpile != null)
            {
                int limit;
                if (!combo.StorageLocation.Stockpile.MayStockpile(itemData.EntityType, out limit))
                {                    
                    return -1f;
                }
                else
                {
                    return 1f; // stockpiles are preferred to other areas
                }
            }           

            return 0.1f;
        }

       

        private static float ScoreNearnessToConsumers(IKnownEntityData item)
        {
            return 0f;
        }

        private static bool IsCurrentlyInThisStorage(StorageLocation storageLocation, IKnownEntityData itemData)
        {
            try
            {
                if ((storageLocation.NormalStorage != null
                    && storageLocation.NormalStorage == itemData.StoredPermanentlyIn)
                    ||
                    (storageLocation.TradeOfferStorage != null
                    && storageLocation.TradeOfferStorage == itemData.StoredPermanentlyIn)) // OfferedForTradeIn))
                {

                    return true;

                }
                else if (storageLocation.NormalStorage == null && storageLocation.TradeOfferStorage == null)
                {
                    // the proposed storage is on the ground

                    if (itemData.ContainedBy.HasValue)
                        return false; // the item is currently contained/carried


                    if (storageLocation.Zone != null) // .IsZone)
                    {
                        // consider any location within the zone as valid:
                        TerrainTile tile;

                        tile = The.Map.GetTile(itemData.MapPosition.Value);

                        if (tile.Zones != null)
                        {
                            //List<Zone> listOfZones; // = tile.GetListOfZones(allegiance);
                            foreach (var item in tile.Zones)
                            {
                                if (item.Value.Exists(z => z == storageLocation.Zone))
                                {
                                    return true;
                                }
                            }

                        }
                    }
                    else
                    {
                        if (MapManager.WorldPosToTile(storageLocation.GroundLocation) == MapManager.WorldPosToTile(itemData.PlaySiteLocation))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {

                string exceptionString = e.Message;
                exceptionString += Entity.GetExceptionInformation(itemData);
                throw new Exception(exceptionString);

            }

            return false;
        }


        public enum Result { Wait, OK }
        private static Result ScoreItemLocation(RegionMap regionMap, Allegiances.Allegiance allegiance,
            ItemStorageCombo combo, IKnownEntityData itemData,
            double airliftCapacityScore, Dictionary<Point, Dictionary<Point, double>> cachedItemLocationScores, MethodID? notifyWhenFinished,
            out double result, bool getPathsNow)
        {
            // score nearness from the item to this location.
            // use region maps for this!

            // if airlift is available, we change the weights!      

            // for zones, we evaluate just one tile...

            // max score for wherever the item currently is:
            if (IsCurrentlyInThisStorage(combo.StorageLocation, itemData))
            {
                result = 1.0;
                return Result.OK;
            }

            float distanceOnFoot = 0f;


            Point knownItemMapPosition = itemData.MapPosition.Value;
            Point storagePosition = MapManager.WorldPosToTile(combo.StorageLocation.GroundLocation);

            // see if we have already computed this:
            Dictionary<Point, double> dict;
            if (cachedItemLocationScores.TryGetValue(knownItemMapPosition, out dict))
            {
                double cachedScore;
                if (dict.TryGetValue(storagePosition, out cachedScore))
                {
                    result = cachedScore;
                    return Result.OK;
                }
            }


            EntityID? storageEntity = null;
            Point? toSubtile = null;
            if (combo.StorageLocation.GetStorageTarget != null) //  .StorageEntity != null)
            {
                storageEntity = combo.StorageLocation.GetStorageTarget.Value.StorageEntity; // combo.StorageLocation.StorageEntity.Value; 
            }
            else
            {
                toSubtile = MapManager.WorldPosToSubtile(combo.StorageLocation.GroundLocation);
            }

            EntityResult fromEntityResult, toEntityResult;

            RegionMap.Result result1 = regionMap.GetDistanceToEntityUsingEntityType(
                combo.Item, 
                storageEntity, 
                allegiance.SharedKnowledge,
                ref distanceOnFoot, allegiance, allegiance.RepresentativeEntityType, 
                out fromEntityResult, out toEntityResult,
                null, toSubtile, 
                false, notifyWhenFinished); //, getPathsNow);

            if (fromEntityResult == EntityResult.Destroyed || fromEntityResult == EntityResult.EntityStatusIsNowUnknown
                || toEntityResult == EntityResult.Destroyed || toEntityResult == EntityResult.EntityStatusIsNowUnknown)
            {
                result = ScoreItemLocationFinally(0.0, combo);
                return Result.OK;
            }

            // don't wait for result - get it directly if possible
            if (result1 == RegionMap.Result.Wait)
            {
                // this is only if we don't have a region graph yet
                //result = 0.0;
                result = ScoreItemLocationFinally(0.0, combo);
                return Result.Wait;
            }
            else if (result1 == RegionMap.Result.NoAccess)
            {
                // no foot access. 
                // reason: item is inside inaccessible container (could be an agent that is carrying it)
                // or the path is blocked
                // FIX THIS: Only go on if we have airlift capacity!
                if (airliftCapacityScore == 0)
                {
                    result = ScoreItemLocationFinally(0.0, combo);
                    return Result.OK;
                }
            }

            float timeCostOnFoot = oneOverBaseSpeedOnFoot * distanceOnFoot;


            /*   timeCost = ((double)distance / entity.Locomotor.CurrentMaximumSpeed) *
                           PlainsType.Instance.MovementFactor(TerrainType.TransportType.Foot, TerrainType.TerrainFeatures.None);
            
               GameData.Instance.AllEntityTypes["entity:human"].RenderAsModelType.BaseSpeed
               */

            double travelTimeScoreOnFoot = GoalEvaluator.ScoreTravelTime(timeCostOnFoot);

            if (airliftCapacityScore > 0) // what about item types that can't be airlifted...? ores etc?
            {
                float distanceByAir = Common.DistanceOctile(MapManager.TileToWorldPos(knownItemMapPosition), MapManager.TileToWorldPos(storagePosition));

                //compute the time cost for airlift as well:
                float timeCostByAir = oneOverBaseSpeedByAir * distanceByAir + takeOffAndLandTime;
                double travelTimeScoreByAir = GoalEvaluator.ScoreTravelTime(timeCostByAir);

                // return a weighted sum depending on how much airlift capacity we have:
                double airliftScoreToUse = 0.8 * airliftCapacityScore;
                result = (1.0 - airliftScoreToUse) * travelTimeScoreOnFoot + airliftScoreToUse * travelTimeScoreByAir;
            }
            else
            {
                // no airlift possible.
                result = travelTimeScoreOnFoot;
            }



            result = ScoreItemLocationFinally(result, combo);


            // cache it:
            if (!cachedItemLocationScores.TryGetValue(knownItemMapPosition, out dict))
            {
                dict = new Dictionary<Point, double>();
                cachedItemLocationScores.Add(knownItemMapPosition, dict);
            }
            dict[storagePosition] = result;

            return Result.OK;
        }

        private static double ScoreItemLocationFinally(double score, ItemStorageCombo combo)
        {
            return 0.8 * score + 0.2 * combo.StorageLocation.TravelTimeToCenterScore;
        }


        public static double CalculateAirliftCapacity(EntityGroup owner)
        {
            float totalCapacity = 0f;
            Vehicle vehicleComponent;

            EntityID vehicleID;
            IKnownEntityData vehicleData;
            SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;
            for (int i = owner.Vehicles.Count - 1; i >= 0; i--)
            {
                vehicleID = owner.Vehicles[i];

                if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, vehicleID, owner, out vehicleData))
                {

                    if (vehicleData.Condition > 0.1 && ((VehicleContainerType)vehicleData.EntityType.ContainerType).Aircraft != null)
                    {
                        totalCapacity += vehicleData.TotalItemStorageCapacity.Value; // vehicle.AgentStorage.ItemStorage.TotalCapacity;
                    }
                }
            }

            // let's convert it to a score on the 0-1 scale:

            double score = totalCapacity / 20.0; // capacity of 20 is considered Very Good
            score = Common.ClampTop(score, 1.0);

            score = Math.Pow(score, 2.0);

            return score;
        }


        public static double EstimateAirTime(Entity vehicle, ref Point destination, ref Vector3 knownVehicleLocation, float estimatedLoadedVehicleSpeed, ref Vector3 knownItemLocation)
        {
            double timeByVehicleOverTerrain;

            // distances in pixels!
            timeByVehicleOverTerrain =
                Common.DistanceOctile(knownVehicleLocation, knownItemLocation) / vehicle.Locomotor.CurrentMaximumSpeedForEvaluator //.CurrentMaximumSpeedNoTerrain
                + Common.DistanceOctile(knownItemLocation, MapManager.TileToWorldPos(destination)) / estimatedLoadedVehicleSpeed; // include other items carried?


            // aircraft take some time taking off and landing.             
            // time should be in seconds now:

            timeByVehicleOverTerrain += 2 * ((VehicleContainerType)vehicle.EntityType.ContainerType).Aircraft.EstimatedTakeOffLandingTime;
            return timeByVehicleOverTerrain;
        }


        private static float ScoreItemStorage(ItemStorageCombo combo, IKnownEntityData itemData, Dictionary<EntityType, Dictionary<StorageLocation, float>> cachedItemStorageScores, 
            SharedKnowledge sharedKnowledge)
        {
            float damage, score;

            // see if we have already computed this:
            Dictionary<StorageLocation, float> dict;
            if (cachedItemStorageScores.TryGetValue(itemData.EntityType, out dict))
            {
                float cachedScore;
                if (dict.TryGetValue(combo.StorageLocation, out cachedScore))
                {
                    return cachedScore;
                }
            }

            StorageTarget? storageTarget = combo.StorageLocation.GetStorageTarget;
            
            if (storageTarget != null) // combo.StorageLocation.StorageEntity != null)
            {
                // storing inside

                IKnownEntityData storageEntity;
                EntityResult result = sharedKnowledge.GetKnownData(storageTarget.Value.StorageEntity /* combo.StorageLocation.StorageEntity.Value*/, out storageEntity);
                if (GoalEvaluator.EntityDataResultCausesSkip(result))
                {
                    return -1f;
                }       
                else 
                {
                    if (Entity.IsFunctional(storageEntity))
                    {
                        Storage storage = storageEntity.FindStorage(storageTarget.Value.StorageID);
                       // Storage storage = LookUp<Storage, StorageID>.FindByID(storageTarget.Value.StorageID); // OLD

                        if (storage == null)
                        {
                            return -1f;
                        }
                        ScoreIndoorStorage(combo, itemData, storageEntity, storage, out score);
                    }
                    else
                    {
                        ScoreOutdoorsStorage(combo, itemData, out score);
                    }
                }
              //  }
            }
            else 
            {
                // storing outside
                ScoreOutdoorsStorage(combo, itemData, out score);
            }
                                  

            // cache it:
            if (!cachedItemStorageScores.TryGetValue(itemData.EntityType, out dict))
            {
                dict = new Dictionary<StorageLocation, float>();
                cachedItemStorageScores.Add(itemData.EntityType, dict);
            }
            dict[combo.StorageLocation] = score;

            return score;

        }

        private static void ScoreIndoorStorage(ItemStorageCombo combo, IKnownEntityData itemData, IKnownEntityData storageEntity, Storage storage, out float score)
        {
            if (itemData.EntityType.ItemType.RequiredStorageTypesFinal != null)                
            {
                if (!itemData.EntityType.ItemType.RequiredStorageTypesFinal.Contains(storageEntity.EntityType))
                {
                    score = -1f; // 0f;
                    return;
                }
            }
            
            float damage;

            damage = NonLivingEntity.ComputeDegradeDamage(itemData, storage.StorageConditions, storage.IsPowered, MapManager.WorldPosToTile(combo.StorageLocation.GroundLocation), 1); // one day's damage...

            damage = Common.ClampTop(damage, 1f);

            // reduce the score if the storage is better than necessary, and the storage is close to full...

            // increase score for food and vermin protected structures:
            float verminProtectionScore = ScoreVerminProtection(itemData, storageEntity);

            float conditionScore = (1f - damage);

            score = CombineScore(conditionScore, verminProtectionScore);
        }

        private static void ScoreOutdoorsStorage(ItemStorageCombo combo, IKnownEntityData itemData, out float score)
        {
            ScoreOutdoorsStorage(combo.StorageLocation.GroundLocation, itemData, out score);
        }

        private static void ScoreOutdoorsStorage(Vector3 location, IKnownEntityData itemData, out float score)
        {
            if (itemData.EntityType.ItemType.RequiredStorageTypesFinal != null)
            {
                score = -1f;
                return;
            }

            float damage;

            damage = NonLivingEntity.ComputeDegradeDamage(itemData, null, null, MapManager.WorldPosToTile(location), 1); // one day's damage...

            damage = Common.ClampTop(damage, 1f);

            // reduce the score if the storage is better than necessary, and the storage is close to full...
            float conditionScore = (1f - damage);

            float verminProtectionScore = ScoreVerminProtection(itemData, null);

            score = CombineScore(conditionScore, verminProtectionScore);
        }

        private static float ScoreVerminProtection(IKnownEntityData itemData, IKnownEntityData storageEntity)
        {
            if (itemData.EntityType.ItemType != null && itemData.EntityType.ItemType.FoodType != null)
            {
                if (storageEntity == null || storageEntity.EntityType.ContainerType.VerminCanAccess)
                {
                    return 0f;
                }
            }

            return 1f;
        }


        private static float CombineScore(float conditionScore, float verminProtectionScore)
        {
            return 0.8f * conditionScore + 0.2f * verminProtectionScore;

        }

        /// <summary>
        /// also checks stockpile limits
        /// </summary>
        /// <param name="storage"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool StorageHasCapacity(StorageLocation storageLocation, IKnownEntityData item, ref Pair<int, int> limit) //Storage storage, Item item)
        {          
            if (storageLocation.TotalStored + item.Bulk < storageLocation.TotalCapacity)
            {
                if (storageLocation.StoredItemsAreAtMaximum(item.EntityType, ref limit))
                {
                    return false;
                }

                return true;
            }

            return false;
        }


        private bool CreateAllItemsList()
        {
            if (allItems == null)
            {
                allItems = new List<IKnownEntityData>();
            }

            allItems.Clear();

            if (parent.GetAllegiance() == null)
                return true;

            SharedKnowledge sharedKnowledge = parent.GetAllegiance().SharedKnowledge;

            IKnownEntityData itemData;
            EntityID id;

            bool addToList;

            foreach (KeyValuePair<EntityType, List<EntityID>> kvp in parent.Items)
            {
                addToList = !ItemTypeIsNeededForProcess(parent, kvp.Key); // don't haul items that may be needed for processes!

                // NEW: iterate all items for cleanup purposes.
                for (int i = kvp.Value.Count - 1; i >= 0; i--)
                {
                    id = kvp.Value[i];
                    if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, id, parent, out itemData))
                    {
                        continue;
                    }

                    // NEW: clean up outdated jobs too (they exist... and this is better than doing it in the evaluator)
                    if (itemData.AssignedToJob.HasValue)
                    {
                        if (LookUp<Job, JobID>.FindByID(itemData.AssignedToJob) == null)
                        {
                            itemData.AssignedToJob = null;
                        }
                    }

                    if (addToList)
                    {
                        allItems.Add(itemData);
                    }
                }
                /*
                if (!ItemTypeIsNeededForProcess(parent, kvp.Key)) // don't haul items that may be needed for processes!
                {
                    for (int i = kvp.Value.Count - 1; i >= 0; i--)
                    {
                        id = kvp.Value[i];
                        if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, id, parent, out itemData))
                        {
                            continue;
                        }

                        allItems.Add(itemData);
                    }
                }*/
            }

            // if we own nothing at all, we can stop here!

            return true;

        }


        private static bool GatherAllStorageLocations(/*bool setStorageTotalToZero,*/ EntityGroup storageLocationsGroup, List<StorageLocation> allStorageLocations, out Point? storageCenterMapPosition)
        {
            Allegiances.Allegiance allegiance = storageLocationsGroup.GetAllegiance();
            SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;


            allStorageLocations.Clear();
            storageCenterMapPosition = null;

            storageCenterMapPosition = MapManager.WorldPosToTile(storageLocationsGroup.Parent.Location.Value);

            IKnownEntityData homeData = null;

            //if we have no home, and no buildings, we really have nowhere to store our things. So cancel this.
            Household household = storageLocationsGroup.Parent as Household;
            if (household != null)
            {
                if (household.Home == null)
                {
                    storageCenterMapPosition = null;
                }
            }
            else
            {
                Person person = storageLocationsGroup.Parent as Person;
                
                if (person != null)
                {
                    if (person.Household.Home != null)
                    {
                        if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(household.Home.Value, out homeData)))
                        {
                            person.Household.Home = null;
                        }
                    }

                    if (homeData != null)
                    {   // if we have a home, use that as center. Not the person's current location.
                        storageCenterMapPosition = homeData.MapPosition; // MapManager.WorldPosToTile(owner.OwningBody.Location);
                    }
                    else
                    {
                        storageCenterMapPosition = null;
                    }
                }
            }
           
            IKnownEntityData buildingData;
            foreach (KeyValuePair<EntityType, List<EntityID>> kvp in storageLocationsGroup.Structures)
            {
                for (int i = kvp.Value.Count - 1; i >= 0; i--)
                {
                    if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, kvp.Value[i], storageLocationsGroup, out buildingData))
                    {
                        continue;
                    }

                    HandleStructureForStorage(storageLocationsGroup, /*setStorageTotalToZero,*/ allStorageLocations, storageCenterMapPosition, buildingData);
                }
            }

            // if the household lives in a home that is not owned by it, add that too:
            if (homeData != null)
            {
                bool hasHandledHome = false;
                List<EntityID> ownedStructures;
                if (storageLocationsGroup.Structures.TryGetValue(homeData.EntityType, out ownedStructures))
                {
                    if (ownedStructures.Contains(household.Home.Value))
                    {
                        hasHandledHome = true;
                    }
                }               

                if (!hasHandledHome)
                {
                    HandleStructureForStorage(storageLocationsGroup, /*setStorageTotalToZero,*/ allStorageLocations, storageCenterMapPosition, homeData);
                }
            }


            if (storageCenterMapPosition == null)
            {
                // we have no home. See if we can pick a building instead:
                if (allStorageLocations.Count > 0)
                {
                    storageCenterMapPosition = MapManager.WorldPosToTile(allStorageLocations[0].GroundLocation); //AllStorageLocations[0].GroundTilePosition;
                }
            }


            if (storageCenterMapPosition != null)
            {
                int radius = GameData.Instance.Constants.ExpeditionStorageRadius; // 4; // 4 = 81 squares!!!
                // get the ground locations to put stuff on
               
                GatherGroundStorageLocations(radius, /*setStorageTotalToZero,*/ storageLocationsGroup, allStorageLocations, storageCenterMapPosition);
            }

            // get areas designated by player          
            foreach (var zone in storageLocationsGroup.Zones)
            {
                if (zone.Stockpile != null) // && zone.Stockpile.Setting != Stockpile.StockpileSetting.None)
                {
                    TerrainTile representativeTile = zone.MapArea.GetFirst();
                    Point point = new Point(representativeTile.X, representativeTile.Y);
                    
                    allStorageLocations.Add(new StorageLocation()
                    {
                        GroundLocation = MapManager.TileToWorldPos(point),
                        TotalStored = 0f, // setStorageTotalToZero ? 0f : zone.GetTotalBulkOfItems(),
                        TotalCapacity = zone.GetBulkCapacity(), // GameData.Instance.Constants.BulkCapacityForSingleTile,
                        TravelTimeToCenterScore = ScoreTravelTimeToCenter(point, storageCenterMapPosition),
                                                
                        Stockpile = zone.Stockpile,
                        TotalStoredItems = (zone.Stockpile != null ? CreateItemCounters(zone.Stockpile.GetMaxLimits()) : null),
             
                        Zone = zone
                        //IsZone = true
                    });

                }
            }

            return true;
        }

        private static void HandleStructureForStorage(EntityGroup owner, /*bool setStorageTotalToZero,*/ List<StorageLocation> allStorageLocations, Point? storageCenterMapPosition, IKnownEntityData entityData)
        {
            
            if (entityData.HasItemStorage == true
                && Entity.IsPermanentStorage(entityData.EntityType) // entityData.EntityType.StructureType != null // don't store in vehicles or unfinished buildings - what about vessels?               
                && entityData.IsCompleted()
                && GoalEvaluator.IsOnPlaySite(entityData)
                && Entity.IsFunctional(entityData)) // GoalEvaluator.ScoreIsEntityFunctional(entityData) > 0d) 
            {

                Stockpile stockpile, tradeOffersStockpile;
                owner.StructureStockpiles.TryGetValue(entityData.EntityID, out stockpile);
               
                foreach (var kvpStorageSpace in entityData.StorageSpaces) 
                {
                    Storage storage = kvpStorageSpace.Value;
                    AddStorageSpace(/*setStorageTotalToZero,*/ allStorageLocations, storageCenterMapPosition, entityData, stockpile, storage,
                        new StorageTarget(entityData.EntityID, storage.ID), null/*,
                        false*/);
                }

                var tradeOfferStorageSpaces = entityData.TradeOffersStorageSpaces;
                if (tradeOfferStorageSpaces != null)
                {
                    owner.TerminalTradeOffers.TryGetValue(entityData.EntityID, out tradeOffersStockpile);

                    foreach (var kvpStorageSpace in tradeOfferStorageSpaces)
                    {
                        Storage storage = kvpStorageSpace.Value;
                        AddStorageSpace(/*setStorageTotalToZero,*/ allStorageLocations, storageCenterMapPosition, entityData, tradeOffersStockpile, storage, 
                            null, new StorageTarget(entityData.EntityID, storage.ID)/*,
                            true*/);  
                    }
                }

            }
        }

        private static void AddStorageSpace(/*bool setStorageTotalToZero,*/ List<StorageLocation> allStorageLocations, 
            Point? storageCenterMapPosition, IKnownEntityData entityData, Stockpile stockpile, 
            Storage storage, 
            StorageTarget? normalStorage,
            StorageTarget? tradeOfferStorage/*,
            bool isTradeOffer*/)
        {
            if (entityData.EntityID == (EntityID)3970)
            {

            }

            allStorageLocations.Add(new StorageLocation()
            {                
                NormalStorage = normalStorage,
                TradeOfferStorage = tradeOfferStorage,
             
                TotalStored = 0f, // setStorageTotalToZero ? 0f : storage.TotalStored,
                TotalCapacity = storage.TotalCapacity,
                GroundLocation = entityData.PlaySiteLocation,
                Stockpile = stockpile,
                TotalStoredItems = (stockpile != null ? CreateItemCounters(stockpile.GetMaxLimits()) : null),
             
                TravelTimeToCenterScore = ScoreTravelTimeToCenter(entityData.MapPosition.Value, storageCenterMapPosition)
            });
        }

        private static Dictionary<EntityType, Pair<int, int>> CreateItemCounters(Dictionary<EntityType, int> maxLimits)
        {
            Dictionary<EntityType, Pair<int, int>> counters = null;
            if (maxLimits != null && maxLimits.Count > 0)
            {
                foreach (var item in maxLimits)
                {
                    Common.AddToDictionary(ref counters, item.Key, new Pair<int, int>(0, item.Value));
                }
            }

            return counters;
        }

        private static void GatherGroundStorageLocations(int radius, /*bool setStorageTotalToZero,*/ EntityGroup owner, List<StorageLocation> allStorageLocations, Point? storageCenterMapPosition)
        {
            int minX, maxX, minY, maxY;

            Rectangle dumpingArea = MapManager.GetClampedMapAreaUsingTiles(new TilePos(storageCenterMapPosition.Value.X, storageCenterMapPosition.Value.Y), 
                                                        radius, out minX, out maxX, out minY, out maxY);

            SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot]; 

            Allegiances.Allegiance allegiance = owner.GetAllegiance();

            TerrainTile tile;
            Point point, centerSubtile;
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    point = new Point(x, y);

                    centerSubtile = MapManager.TileCenterToSubTile(point);

                    if (!The.Map.TileIsCompletelyBlocked(map, point)
                        && !The.Map.SubtileIsCompletelyBlocked(map, centerSubtile) // the center subtile must not be blocked...
                        && !The.Map.FlagIsSet(centerSubtile, SurfaceType.TransportType.Foot, MapManager.SubtileValue.Reserved)) // try to avoid areas being built on
                    {
                        tile = The.Map.GetTile(point);

                        if (tile.Owner == null || tile.Owner == owner.ID) // only owned terrain
                        {

                            allStorageLocations.Add(new StorageLocation()
                            {
                                // GroundTilePosition = point, 
                                GroundLocation = MapManager.TileToWorldPos(point),
                                TotalStored = 0f, // setStorageTotalToZero ? 0f : tile.GetTotalBulkOfItems(),
                                TotalCapacity = GameData.Instance.Constants.BulkCapacityForSingleTile,
                                TravelTimeToCenterScore = ScoreTravelTimeToCenter(point, storageCenterMapPosition)
                            });

                        }
                       
                    }
                }
            }
        }

        /// <summary>
        /// returns a score that can give higher weight to locations closer to the center
        /// </summary>
        /// <returns></returns>
        public static double ScoreTravelTimeToCenter(Point mapPos, Point? centerMapPosition)
        {
            float distance = MapManager.tileSize * Common.DistanceOctile(centerMapPosition.Value, mapPos);

            float timeCostOnFoot = oneOverBaseSpeedOnFoot * distance;

            double travelTimeScoreOnFoot = GoalEvaluator.ScoreTravelTime(timeCostOnFoot);

            return travelTimeScoreOnFoot;
        }

        private bool CreateAllCombos()
        {
            int start, end;

            start = itemCounter;
            end = Common.Min(itemCounter + itemsPerCycle, allItems.Count);


            if (itemCounter == 0)
            {
                allCombos.Clear();
            }

            Allegiances.Allegiance allegiance = parent.GetAllegiance();

            IKnownEntityData item;
            for (int i = start; i < end && i < allItems.Count; i++)
            {
                item = allItems[i];

                if (item.EntityID == (EntityID)4555)
                {

                }

                CreateCombosForItem(AllStorageLocations, allCombos, item);
            }

            //CreateCombosForItems(start, end, allegiance, allItems, AllStorageLocations, allCombos);

            itemCounter = end;

            if (end == allItems.Count)
            {
                itemCounter = 0;
                allItems.Clear(); // is it needed still?
                phase = Phase.ScoreCombos;
                return true;
            }

            return false;

        }

        /*  private static void CreateCombosForItems(int start, int end, Intelligence.AllegianceType allegiance, List<Item> allItems, List<StorageLocation> allStorageLocations, List<ItemStorageCombo> allCombos)
          {
              Item item;
              for (int i = start; i < end && i < allItems.Count; i++)
              {
                  item = allItems[i];

                  CreateCombosForItem(allegiance, allStorageLocations, allCombos, item);
              }           
          }*/

        private static bool ItemAssignmentOK(IKnownEntityData item)
        {
            if (item.AssignedToJob != null)
            {
                // leave the item alone unless it is assigned to a haulingjob that is not input materials for a process job...

                Job assignedToJob = null;
                HaulingJob haulingJob = null;

                if (item.AssignedToJob.HasValue)
                {
                    assignedToJob = LookUp<Job, JobID>.FindByID(item.AssignedToJob.Value);
                    if (assignedToJob == null)
                    {
                        // cleanup
                        item.AssignedToJob = null;
                    }
                    else
                    {
                        haulingJob = assignedToJob as HaulingJob;
                    }
                }

                if (haulingJob == null)
                {
                    return false;
                }
                else if (haulingJob.RequiredByProcessJob != null)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ItemIsOK(IKnownEntityData item)
        {
            return item.CanBeHauled()
                && ItemAssignmentOK(item);
        }

        private static void CreateCombosForItem(List<StorageLocation> allStorageLocations, List<ItemStorageCombo> allCombos, IKnownEntityData item)
        {
            if (ItemIsOK(item))
            {
                /*
                if (item.AssignedToJob != null)
                {
                    // leave the item alone unless it is assigned to a haulingjob that is not input materials for a process job...

                    Job assignedToJob = null;
                    HaulingJob haulingJob = null;

                    if (item.AssignedToJob.HasValue)
                    {
                        assignedToJob = LookUp<Job, JobID>.FindByID(item.AssignedToJob.Value);
                        if (assignedToJob == null)
                        {
                            // cleanup
                            item.AssignedToJob = null;
                        }
                        else
                        {
                            haulingJob = assignedToJob as HaulingJob;
                        }
                    }

                    if (haulingJob == null)
                    {
                        return;
                    }
                    else if (haulingJob.RequiredByProcessJob != null)
                    {
                        return;
                    }
                }*/
                              
                foreach (StorageLocation storageLocation in allStorageLocations)
                {
                    allCombos.Add(new ItemStorageCombo() { Item = item.EntityID, StorageLocation = storageLocation });

                }
            }
        }

        /// <summary>
        /// returns the next combo if still valid, else removes it from the list and modifies the index
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        /// <param name="combos"></param>
        /// <param name="index"></param>
        /// <param name="combo"></param>
        /// <param name="itemData"></param>
        /// <returns></returns>
        private static bool IsComboValid(EntityGroup entityGroup, SharedKnowledge sharedKnowledge, List<ItemStorageCombo> combos, ref int index, out ItemStorageCombo combo, out IKnownEntityData itemData)
        {
            combo = combos[index];

            if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(combo.Item, out itemData)))
            {
                combos.RemoveAt(index);

                index--;
                
                return false;
            }

            if (itemData.EntityType.KeyName.ToLower().Contains("vinegar") && itemData.ContainedBy == null)
            {

            }

            if (ItemTypeIsNeededForProcess(entityGroup, itemData.EntityType)) // don't haul items that may be needed for processes!
            {
                combos.RemoveAt(index);
                index--;
                return false;
            }

            // test other properties here..?

            return true;
        }

        private bool ScoreAllCombos()
        {
            ItemStorageCombo combo;
            double score = -1.0;
            float damageScore = 0f;

            int start, end;

            //  RegionMap footRegionMap = UWGame.SimSide.Instance.Map.RegionMapManager.ColonistFootMovementMap; //RegionMaps[TerrainType.TransportType.Foot];
            //RegionMap vehicleRegionMap = UWGame.SimSide.Instance.Map.RegionMapManager.RegionMaps[TerrainType.TransportType.OffRoad]; // ???

            //Intelligence entityIntelligence = entity.Intelligence;

            start = comboCounter;
            end = Common.Min(comboCounter + combosPerCycle, allCombos.Count);

            Allegiances.Allegiance allegiance = parent.GetAllegiance();

            RegionMap regionMap = GetRegionMapToUse(allegiance);         //UWGame.SimSide.Instance.Map.RegionMapManager.HumanFootExposedNormalRegionMap;

            IKnownEntityData itemData;

            for (int i = start; i < end && i < allCombos.Count; i++)
            {               
                if (IsComboValid(parent, allegiance.SharedKnowledge, allCombos, ref i, out combo, 
                    out itemData))
                {
                    Result result = ScoreCombo(regionMap, allegiance, airliftCapacityScore, cachedItemStorageScores, cachedItemLocationScores, notifyWhenRegionSearchIsFinished, combo, itemData,
                        out score, out damageScore, false);

                    if (result == Result.OK)
                    {
                        float currentHaulTargetScore = GetCurrentHaulTargetScore(combo);

                        score += 0.1f * currentHaulTargetScore;

                        combo.Score = score;
                        combo.DamageScore = damageScore;
                    }
                    else
                    {
                        isWaiting = true;
                        return false;
                    }
                }
               
            }

            comboCounter = end;

            if (end == allCombos.Count)
            {
                SortCombos(allCombos);

                return true;
            }
            else
            {
                return false;
            }

        }

        private static RegionMap GetRegionMapToUse(Allegiances.Allegiance allegiance)
        {
            RegionMap regionMap = allegiance.SharedKnowledge.GetMovementMap(ProtectionLevel.Exposed, allegiance.RepresentativeEntityType, ThreatStance.Normal).Layers[SurfaceType.TransportType.Foot].RegionMap;

            return regionMap;
        }

        private static void SortCombos(List<ItemStorageCombo> combos)
        {
            // remove "illegal" storage combos (sand indoors etc.):
            combos.RemoveAll(c => Common.IsZero(c.Score));

            combos.Sort((a, b) => b.Score.CompareTo(a.Score));
        }

        public void NotifyWhenRegionSearchIsFinished()
        {
            // continue...
            isWaiting = false;
        }

        


        private static bool ItemTypeIsNeededForProcess(EntityGroup entityGroup, EntityType entityType)
        {
            List<HaulingJobAnyItemOfType> jobs;
            if (entityGroup.HaulingJobsAnyItemOfType.TryGetValue(entityType, out jobs) && jobs.Count > 0)
            {
                if (jobs.Exists(h => h.Item == null)) // if a job exists where the item has not been assigned yet -> destroy all haul to storage jobs for that type...
                {
                    return true;
                }
            }
            else
            {
                // #PRODCHANGE
                // NEW: test production jobs with immobile inputs too, such as butchering:
                List<ProcessJob> pJobs;
                if (entityGroup.ProductionJobsByInput != null
                    && entityGroup.ProductionJobsByInput.TryGetValue(entityType, out pJobs))
                {
                    for (int i = pJobs.Count - 1; i >= 0; i--)
                    {
                        var job = pJobs[i];
                        Vector3? location;
                        IKnownProcess processData;
                        if (job.GetCurrentJobLocation(out location, out processData))
                        {
                            if (!location.HasValue // is null in the case of processes that require immovable tools and immovable inputs, and which are started from the inventory.
                                && !processData.HasFixedLocation()
                                && job.ProcessType.NeedsImmovableInput())
                            {
                                return true;
                            }
                        }
                    }
                    
                   /* foreach (var job in pJobs)
                    {
                        Vector3? location;
                        IKnownProcess processData;
                        if (job.GetCurrentJobLocation(out location, out processData))
                        {
                            if (!location.HasValue // is null in the case of processes that require immovable tools and immovable inputs, and which are started from the inventory.
                                && !processData.HasFixedLocation()
                                && job.ProcessType.NeedsImmovableInput())
                            {
                                return true;
                            }
                        }
                    }*/

                }
            }

            return false;

        }


        /// <summary>
        /// if there are processes that need certain item types, destroy storage hauling jobs for all those items.
        /// Processes are much more important.
        /// </summary>
      /*  private void CleanupHaulingJobsForItemTypesThatMayBeNeededByProcesses()
        {
            Job job;
            HaulingJobSpecificItem haulingJob;
            Allegiances.Allegiance allegiance = parent.GetAllegiance();

           // Item itemComponent;
            for (int j = 0; j < parent.HaulingJobs.Count; j++)
            {
                job = parent.HaulingJobs[j];

                haulingJob = job as HaulingJobSpecificItem;
                if (haulingJob != null && haulingJob.IsHaulJobToStorage && haulingJob.Item != null)
                {
                    if (parent.HaulingJobsAnyItemOfType.TryGetValue()

                }
            }


        }*/

        /// <summary>
        /// The purpose of this is to remove hauling jobs that refer to items which are no longer available for any reason. A sort of clean up.
        /// Hauling jobs currently underway are not removed here.
        /// 
        /// if there are processes that need certain item types, destroy storage hauling jobs for all those items.
        /// Processes are much more important.
        /// </summary>
        /// <returns></returns>
        private bool CleanupHaulingJobs()
        {
            Job job;
            HaulingJob haulingJob;
            HaulingJobSpecificItem specificItemJob;
            List<HaulingJobAnyItemOfType> list;
            Allegiances.Allegiance allegiance = parent.GetAllegiance();

           // Item itemComponent;
            for (int j = 0; j < parent.HaulingJobs.Count; j++)
            {
                job = parent.HaulingJobs[j];

                haulingJob = job as HaulingJob;
                if (haulingJob != null && haulingJob.Item != null)
                {
                    IKnownEntityData itemData;
                    EntityResult result = allegiance.SharedKnowledge.GetKnownData(haulingJob.Item.Value, out itemData);

                    if (result == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown)
                    {
                        RecordOutdatedJob(ref outdatedJobs, haulingJob);
                        continue;
                    }

                    EntityID? inUseBy = allegiance.SharedKnowledge.GetInUseBy(itemData.EntityID);
                        
                    if (!itemData.CanBeHauled() // check for important state changes... partOf etc.
                        ||
                        (job.TakenBy.Count == 0 // only look at jobs not currently taken.
                            &&
                        ((itemData.AssignedToJob != null && itemData.AssignedToJob != haulingJob.ID) // is the item assigned to a job other than this hauling job?                       
                        || inUseBy != null)))
                    {
                        RecordOutdatedJob(ref outdatedJobs, haulingJob);
                        continue;
                    }

                    if (itemData.OwnedBy == null)
                    {   // clean up discarded item jobs, even if taken:
                        RecordOutdatedJob(ref outdatedJobs, haulingJob);
                        continue;
                    }

                    specificItemJob = job as HaulingJobSpecificItem;
                    if (specificItemJob != null && specificItemJob.IsHaulJobToStorage)
                    {
                        // if there are processes that need certain item types, destroy storage hauling jobs for all those items.
                        if (ItemTypeIsNeededForProcess(parent, itemData.EntityType)
                            || !Entity.StorageHasRoomForItem(allegiance.SharedKnowledge, specificItemJob, itemData)) // NEW: if no longer room... // #PRODCHANGE   
                        {
                            RecordOutdatedJob(ref outdatedJobs, haulingJob);
                            continue;
                        }
                    }
                }
            }
            

            // clean up any outdated jobs that we found:          
           // DestroyJobs();

            return true;
        }

        private void DestroyJobs()
        {
            if (outdatedJobs != null)
            {
                for (int i = 0; i < outdatedJobs.Count; i++)
                {
                    HaulingJob job = outdatedJobs[i];

                    HaulingJobAnyItemOfType anyItemJob = job as HaulingJobAnyItemOfType;
                    if (anyItemJob != null)
                    {
                        ReplaceHaulingJobAnyItem(job, anyItemJob);
                    }
                    else
                    {
                        job.Destroy(true);
                    }
                }
            }
        }

        private static void ReplaceHaulingJobAnyItem(HaulingJob job, HaulingJobAnyItemOfType anyItemJob)
        {
            // this will cancel takers and de-assign the (invalid?) item. but we want to replace the job right after.
            job.Destroy(true);

            ProcessJob processJob = anyItemJob.RequiredByProcessJob;

            bool isStarted;
            if (processJob.IsStarted(out isStarted)
                && !isStarted)
            {
                EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(anyItemJob.EntityGroupID);
                if (entityGroup != null)
                {
                    // copy all the data we need... should this be a method?
                    HaulingJobAnyItemOfType replacingJob = new HaulingJobAnyItemOfType(anyItemJob.ToLocation.Value, entityGroup, anyItemJob.RequiredItemType, anyItemJob.ItemsToHaulGroup, anyItemJob.NewOwner);
                    replacingJob.RequiredByProcessJob = processJob;
                    replacingJob.Priority = anyItemJob.Priority;
                }
            }
        }

        private static void RecordOutdatedJob(ref List<HaulingJob> outdatedJobs, HaulingJob job)
        {
            if (outdatedJobs != null)
            {
                outdatedJobs.Add(job);
            }
            else
            {
                outdatedJobs = new List<HaulingJob>();
                outdatedJobs.Add(job);
            }
        }



       // private Dictionary<IKnownEntityData, ItemStorageCombo> assignedItems = new Dictionary<IKnownEntityData, ItemStorageCombo>();
        private Dictionary<EntityID, ItemStorageCombo> assignedItems = new Dictionary<EntityID, ItemStorageCombo>();

        private bool CreateAllHaulingJobs()
        {
            int start, end;

            start = comboCounter;
            end = Common.Min(comboCounter + combosPerCycle, allCombos.Count);

            ItemStorageCombo currentCombo;
            IKnownEntityData itemData;
                   
            SharedKnowledge sharedKnowledge = parent.GetAllegiance().SharedKnowledge;

            for (int i = start; i < end && i < allCombos.Count; i++)
            {
                currentCombo = allCombos[i];
                                              
                if (IsComboValid(parent, sharedKnowledge, allCombos, ref i, out currentCombo, out itemData))
                {
                    if (itemData.EntityType.KeyName.ToLower().Contains("vinegar") && itemData.ContainedBy == null)
                    {

                    }

                    AssignItemToStorage(currentCombo, itemData, assignedItems);
                }

            }

            comboCounter = end; // save the progress (timesliced)!


            // this code runs once after a certain no. of updates 
            if (comboCounter == allCombos.Count)
            {
                EndCreateAllHaulingJobs();

                return true;
            }
            else
            {
                return false;
            }

        }

        private void EndCreateAllHaulingJobs()
        {
            SharedKnowledge sharedKnowledge = parent.GetAllegiance().SharedKnowledge;

            // clean up any outdated jobs first:
            outdatedJobs.Clear();

            CleanupHaulingJobs();

            //CleanupHaulingJobsForItemTypesThatMayBeNeededByProcesses();

            // clean up any outdated jobs that we found:       
            DestroyJobs();

            // TODO: validate assigned status, haulability etc. again


            CleanListOfItemsAlreadyInTheCorrectPlaceOrHaulingJobExists(sharedKnowledge, assignedItems, parent);

            // if hauling the item to a new place:

            // remove existing hauling jobs
            // remove (cancel) specific hauling jobs for that item

            if (assignedItems.Count > 0)
            {

                foreach (KeyValuePair<EntityID, ItemStorageCombo> kvp in assignedItems)
                {
                    DestroyHaulingJobsForItem(kvp.Key, parent);
                }


                // create jobs finally
                if (assignedItems.Count > 0)
                {
                    // use this to arrange items nicely by category...
                    Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> tileInfluenceMaps = new Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>>();

                    IKnownEntityData itemData;

                    foreach (var kvp in assignedItems)
                    {
                        if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(kvp.Key, out itemData)))
                        {
                            continue;
                        }
                        else
                        {
                            if (itemData.EntityID == (EntityID)5293)
                            {

                            }

                            bool isHaulToTrade = false; 

                            // also specify storage.
                            Vector3? location;
                            if (kvp.Value.StorageLocation.GetStorageTarget == null) //   .StorageEntity == null)
                            {
                                // select a nice spot within the tile to place the item:
                                location = SelectGroundLocation(kvp.Value, itemData,
                                    tileInfluenceMaps);

                                if (location == null)
                                {
                                    continue; // don't create a job, nowhere to place the item
                                }
                            }
                            else
                            {
                                location = null;
                                isHaulToTrade = kvp.Value.StorageLocation.TradeOfferStorage.HasValue; 
                            }

                            HaulingJobSpecificItem job =
                                new HaulingJobSpecificItem(location,
                                    kvp.Value.StorageLocation.GetStorageTarget,                           
                                    parent, //GetPriority(parent), 
                                    itemData,
                                    null, true, isHaulToTrade, parent);
                        }
                    }
                }
            }

        }

        private static void DestroyHaulingJobsForItem(EntityID entityID, EntityGroup owner)//ref KeyValuePair<IKnownEntityData, ItemStorageCombo> kvp)
        {
            List<Job> haulingJobsToCancel;

            // perhaps we need a dictionary...?
           // haulingJobsToCancel = owner.InternalOwner.OwnerContent.HaulingJobs.FindAll((j => ((HaulingJob)j).Item == kvp.Key.EntityID));
            haulingJobsToCancel = owner.HaulingJobs.FindAll((j => ((HaulingJob)j).Item == entityID)); // kvp.Key.EntityID));

            foreach (HaulingJob job in haulingJobsToCancel)
            {
                if (!(job is HaulingJobAnyItemOfType)) // probably should not happen!
                {
                    if (job.IsCompleted) // #HAULMANAGERFIX
                    {
                        continue;
                    }

                    // Cancel jobs...
                    job.Destroy(true);
                }
            }
           
        }


       /* public static Priority GetPriority(EntityGroup ownerOfJobs)
        {*/
            /*Expedition e = ownerOfJobs.GetExpedition();

            if (e != null)
            {
                return e.PlayerSetWorkPriority;
            }
            else*/
            //return Priority.Normal;
       // }

        /// <summary>
        /// tile -> ( list of items on the tile and their sub tile coords + subtile array with bulk amounts)
        /// </summary>
        /// <param name="combo"></param>
        /// <param name="allTileData"></param>
        /// <returns></returns>
        private static Vector3? SelectGroundLocation(ItemStorageCombo combo, IKnownEntityData itemData, Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> allTileData)
        {
            // select a subtile spot with items in the smae item category that is not too full, or select a random spot
            //Point? tilePos;
            Point tilePos;
            MapArea mapArea;

            if (combo.StorageLocation.Zone != null)
            {
                mapArea = combo.StorageLocation.Zone.MapArea;
                // look over multiple tiles to find the best spot

                return mapArea.SelectBestGroundLocationForStorage(itemData, allTileData);

            }
            else
            {
                // look at a single tile
                tilePos = MapManager.WorldPosToTile(combo.StorageLocation.GroundLocation);

                TerrainTile tile = The.Map.GetTile(tilePos);
                return SelectGroundLocationOnTile(itemData, allTileData, tile);

            }            

        }


        public static Vector3? FindSimilarItemToStackWithInTile(IKnownEntityData item, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData, TerrainTile tile)
        {
            Point tilePos = new Point(tile.X, tile.Y);

            float totalBulkOnSubtile;

            Point relativeSubtilePos;
           // Point absoluteSubtilePos;
            Vector3? result;

            float bulkCapacity = GameData.Instance.Constants.BulkCapacityForSubTile;

            if (tile.EntitiesOnTile != null)
            {
                // first see if there is a similar item that we can stack with:
                foreach (var entity in tile.EntitiesOnTile)
                {
                    // get the total bulk on each subtile:
                    relativeSubtilePos = MapManager.WorldPosToRelativeSubtile(entity.PlaySiteLocation);

                    totalBulkOnSubtile = tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y];
                    totalBulkOnSubtile += entity.Bulk;
                    tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y] = totalBulkOnSubtile;

                    result = TestItemToStackWith(item, tilePos, tileData, bulkCapacity, entity, relativeSubtilePos);
                    if (result.HasValue)
                    {
                        return result.Value;
                    }
                }
            }

            // now check the other hauled items to stack with:
            foreach (var entity in tileData.Item1)
            {
                result = TestItemToStackWith(item, tilePos, tileData, bulkCapacity, entity.Item1, entity.Item2);
                if (result.HasValue)
                {
                    return result.Value;
                }
            }

            return null;
        }

        public static Vector3? SelectGroundLocationOnTile(IKnownEntityData item, Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> allTileData, 
            TerrainTile tile)
        {
            Point tilePos = new Point(tile.X, tile.Y);

            Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData = null;

            float bulkCapacity = GameData.Instance.Constants.BulkCapacityForSubTile;

            tileData = GetTileGroundStorageData(allTileData, 
                tilePos);

        //    float totalBulkOnSubtile;
         //   Point relativeSubtilePos;
           
            Vector3? result;

            result = FindSimilarItemToStackWithInTile(item, tileData, tile);
            
            if (result.HasValue)
            {

                return result.Value;
            }
                      

            result = FindEmptySubtileForStorage(tilePos, item, tileData, false);
            if (result.HasValue)
            {

                return result.Value;
            }

            // else choose the first subtile that has room:
            result = FindFirstSubtileWithRoomForStorage(item, tilePos, tileData);
            if (result.HasValue)
            {

                return result.Value;
            }

            return null;
            
            // else dump it in the center...
          /*  relativeSubtilePos = new Point(1, 1);
            totalBulkOnSubtile = tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y];

            tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y] = totalBulkOnSubtile + item.Bulk;
            tileData.Item1.Add(new Tuple<IKnownEntityData, Point>(item, relativeSubtilePos));


            result = MapManager.SubtileAndTilePosToWorldPos(relativeSubtilePos, tilePos);

            return result.Value;*/
        }

        public static Vector3? FindFirstSubtileWithRoomForStorage(IKnownEntityData item, Point tilePos, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData)
        {          
            Point relativeSubtilePos;
            Vector3? result;
            Point absoluteSubtilePos;
            float bulkCapacity = GameData.Instance.Constants.BulkCapacityForSubTile;
           // SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot]; // moveMap.Map[TerrainType.TransportType.Foot];
            Point startSubtile = MapManager.TileEdgeToSubtile(tilePos);

            for (int y = 0; y < MapManager.SubtilesPerTileLength; y++)
            {
                for (int x = 0; x < MapManager.SubtilesPerTileLength; x++)
                {
                    relativeSubtilePos = new Point(x, y);
                    absoluteSubtilePos = new Point(x + startSubtile.X, y + startSubtile.Y);

                    result = TestSubtileAsGroundLocation(item, tilePos, relativeSubtilePos, tileData, bulkCapacity);

                    if (result.HasValue) 
                    {
                        return result.Value;
                    }
                }
            }

            return null;
        }

        public static Vector3? FindEmptySubtileForStorage(Point tilePos, IKnownEntityData item, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData, bool leftMostSubtilesOnly)
        {
            Vector3? result;
          
            Point relativeSubtilePos;
            Point absoluteSubtilePos;
            float totalBulkOnSubtile;

            // now see if there is an empty spot:
            SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot]; // moveMap.Map[TerrainType.TransportType.Foot];
            Point startSubtile = MapManager.TileEdgeToSubtile(tilePos);

            int maxX;
            if (leftMostSubtilesOnly)
            {
                maxX = 1;
            }
            else
            {
                maxX = MapManager.SubtilesPerTileLength;
            }


            for (int y = 0; y < MapManager.SubtilesPerTileLength; y++)
            {
                for (int x = 0; x < MapManager.SubtilesPerTileLength; x++)
                {
                    relativeSubtilePos = new Point(x, y);

                    totalBulkOnSubtile = tileData.Item2[x][y];

                    absoluteSubtilePos = new Point(x + startSubtile.X, y + startSubtile.Y);

                    if (Common.IsZero(totalBulkOnSubtile) &&
                        !The.Map.SubtileIsCompletelyBlocked(map, absoluteSubtilePos) && 
                        !The.Map.FlagIsSet(absoluteSubtilePos, SurfaceType.TransportType.Foot, MapManager.SubtileValue.Reserved))
                    {
                        // if empty and non-blocked, use this location:
                        tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y] = totalBulkOnSubtile + item.Bulk;
                        tileData.Item1.Add(new Tuple<IKnownEntityData, Point>(item, relativeSubtilePos));

                        
                        result = MapManager.SubTileToWorldPos3(absoluteSubtilePos);
                        //  result = MapManager.SubtileAndTilePosToWorldPos(relativeSubtilePos, tilePos);
                        return result.Value;
                    }

                }
            }

            return null;
        }

        public static Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> GetTileGroundStorageData(Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> allTileData, Point tilePos)
        {
            Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData;

            if (!allTileData.TryGetValue(tilePos, out tileData))
            {
                float[][] subtileBulk = null;
                Common.InitJaggedArray(ref subtileBulk, MapManager.SubtilesPerTileLength, MapManager.SubtilesPerTileLength);

                tileData = new Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>(new List<Tuple<IKnownEntityData, Point>>(), subtileBulk);

                allTileData.Add(tilePos, tileData);
            }

            return tileData;
        }



        private static Vector3? TestItemToStackWith(IKnownEntityData item, /*ItemStorageCombo combo*/ Point tilePos, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData, float bulkCapacity, IKnownEntityData entity, Point relativeSubtilePos)
        {
            if (entity.EntityType.ItemType != null) 
            {   
                // group by sprite to make it more visually appealing... // HACK! DECOUPLE!
                if (entity.EntityType.RenderableTypeMode.DefaultClientState != null
                    && item.EntityType.RenderableTypeMode.DefaultClientState != null
                    && entity.EntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType != null
                    && item.EntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType != null
                    && entity.EntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName == item.EntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName) //entity.EntityType.Category == combo.Item.EntityType.Category))
                {
                    
                    return TestSubtileAsGroundLocation(item, tilePos, relativeSubtilePos, tileData, bulkCapacity);
                }
            }

            return null;
        }

        private static Vector3? TestSubtileAsGroundLocation(IKnownEntityData item, 
            Point tilePos, 
            Point relativeSubtilePos, 
            Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]> tileData, 
            float bulkCapacity)
        {
            float totalBulkOnSubtile = tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y];
            SubtileLayers map = The.Map.TerrainCosts[SurfaceType.TransportType.Foot]; // moveMap.Map[TerrainType.TransportType.Foot];

            Point absoluteSubtilePos = MapManager.TileAndRelativeSubtileToAbsoluteSubtile(tilePos.X, tilePos.Y, relativeSubtilePos.X, relativeSubtilePos.Y);

            if (totalBulkOnSubtile + item.Bulk <= bulkCapacity  // free room?
                && !The.Map.SubtileIsCompletelyBlocked(map, absoluteSubtilePos) // not blocked?
                && !The.Map.FlagIsSet(absoluteSubtilePos, SurfaceType.TransportType.Foot, MapManager.SubtileValue.Reserved)) // not reserved either?
            {
                tileData.Item2[relativeSubtilePos.X][relativeSubtilePos.Y] = totalBulkOnSubtile + item.Bulk;
                tileData.Item1.Add(new Tuple<IKnownEntityData, Point>(item, relativeSubtilePos));

                Vector3 result = MapManager.SubTileToWorldPos3(absoluteSubtilePos); // MapManager.SubtileAndTilePosToWorldPos(relativeSubtilePos, tilePos);
                return result;
            }

            return null;
        }


        private static void AssignItemToStorage(ItemStorageCombo combo, IKnownEntityData itemData, Dictionary<EntityID, ItemStorageCombo> assignedItems)
        {
            ItemStorageCombo assignedCombo;
            if (!assignedItems.TryGetValue(itemData.EntityID, out assignedCombo))
            {
                // not already assigned

               // Tuple<int, int> limit = new Tuple<int,int>(0, 0);  // we need to init this

               // Tuple<int, int> limit = null; // tuples are now a class
                Pair<int, int> limit = null; 

                // checks stockpile limits too
                if (StorageHasCapacity(combo.StorageLocation, itemData, ref limit))
                {
                    assignedItems.Add(itemData.EntityID, combo);

                    combo.StorageLocation.TotalStored += itemData.Bulk;

                    if (limit != null) // not the default.. // != default(Tuple<int, int>))
                    {
                       // limit = new Tuple<int, int>(limit.Item1 + 1, limit.Item2);
                        limit.First++;
                        //combo.StorageLocation.TotalStoredItems[itemData.EntityType] = limit;
                    }

                   /* var totalStored = combo.StorageLocation.TotalStoredItems;
                   
                    if (totalStored.TryGetValue(key, out sum))
                    {
                        sum.Item1++;
                        dictWithSums[key] = sum;
                    }*/
                   
                  //  Common.AddToDictWithSums(combo.StorageLocation.TotalStoredItems, itemData.EntityType, false);
            
                }
            }           
        }

        private static void CleanListOfItemsAlreadyInTheCorrectPlaceOrHaulingJobExists(SharedKnowledge sharedKnowledge, Dictionary<EntityID, ItemStorageCombo> assignedItems, EntityGroup owner)
        {
            
            // clean the list of items already in the correct place.
            IKnownEntityData itemData;
            List<EntityID> itemsToClean = null;
            foreach (var kvp in assignedItems)
            {
                if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(kvp.Key, out itemData)))
                {
                    Common.AddToList(ref itemsToClean, kvp.Key);                   
                }
                else
                {
                   
                    if (!ItemIsOK(itemData) // NEW: check assignment status again since we are running timesliced.
                        || IsCurrentlyInThisStorage(kvp.Value.StorageLocation, itemData))
                    {
                        // the item is already in the correct place. Remove from the list.
                        Common.AddToList(ref itemsToClean, kvp.Key);

                       
                        // also destroy any hauling jobs that may exist:
                        DestroyHaulingJobsForItem(kvp.Key, owner);
                    }
                    else if (HaulingJobAlreadyExistsForThisCombo(kvp.Value, itemData, owner) // perhaps we need a dictionary...?
                        || ItemTypeIsNeededForProcess(owner, itemData.EntityType))  
                    {
                        // clean the list of items where a haulingjob already exists for the target storage (no need to cancel and create again):
                        Common.AddToList(ref itemsToClean, kvp.Key);

                    }                   
                }
            }

            if (itemsToClean != null)
            {
                foreach (var item in itemsToClean)
                {
                    assignedItems.Remove(item);
                }
            }
        }

        private float GetCurrentHaulTargetScore(/*EntityGroup owner,*/ ItemStorageCombo combo)
        {
            HaulingJobSpecificItem currentHaulJob;
            if (parent.SpecificHaulingJobs.TryGetValue(combo.Item, out currentHaulJob))
            {
                if (currentHaulJob.ToStorage.HasValue)
                {
                    if (combo.StorageLocation != null
                        && currentHaulJob.ToStorage.Value == combo.StorageLocation.GetStorageTarget)
                    {
                        return 1f;
                    }
                }
                else
                {
                    if (MapManager.WorldPosToTile(currentHaulJob.ToLocation.Value) == MapManager.WorldPosToTile(combo.StorageLocation.GroundLocation))
                    {
                        return 1f;
                    }
                }
            }

            return 0f;
        }


        private static bool HaulingJobAlreadyExistsForThisCombo(ItemStorageCombo combo, IKnownEntityData itemData, EntityGroup owner)
        {
            HaulingJobSpecificItem haulingJob;
            owner.SpecificHaulingJobs.TryGetValue(combo.Item, out haulingJob); // GetHaulingJobForItem(combo, owner);

            /// if hauling to a ground location close to, and with the same damage score as, the location chosen by the manager, consider them the same (to avoid forcing agents to cancel and/or drop the hauled item)
            if (haulingJob != null)
            {
                if (combo.StorageLocation.NormalStorage.HasValue)
                {
                    if (!haulingJob.IsToTradeOfferStorage && haulingJob.ToStorage != null && haulingJob.ToStorage.Value == combo.StorageLocation.NormalStorage.Value)
                    {
                        return true;
                    }

                }
                else if (combo.StorageLocation.TradeOfferStorage.HasValue)
                {
                    if (haulingJob.IsToTradeOfferStorage && haulingJob.ToStorage != null && haulingJob.ToStorage.Value == combo.StorageLocation.TradeOfferStorage.Value)
                    {
                        return true;
                    }
                }                  
                else
                {
                    // compare ground destinations:
                    if (haulingJob.ToLocation.HasValue)
                    {
                        if (haulingJob.ToLocation == combo.StorageLocation.GroundLocation)
                        {
                            return true;
                        }
                        else
                        {
                            // #HAULMANAGERFIX

                            // outdoor damage is the same within tiles
                            bool onSameTile = MapManager.WorldPosToTile(haulingJob.ToLocation.Value) == MapManager.WorldPosToTile(combo.StorageLocation.GroundLocation);

                            if (onSameTile)
                            {
                                return true;
                            }


                            // if not exactly the same, see if the haul destination is good enough:
                            float damageScore;

                            //  ScoreOutdoorsStorage(combo, itemData, out damageScore);
                            ScoreOutdoorsStorage(haulingJob.ToLocation.Value, itemData, out damageScore);


                            // if the combo is better than the current haul destination

                            // if the damage score is nearly the same
                            if (Common.IsEqual(combo.DamageScore, damageScore, 0.01f))  //Common.IsGreaterThanOrEqual(combo.DamageScore, damageScore)) // combo.DamageScore >= damageScore)
                            {
                                // and if they are near each other
                                if (haulingJob.ToLocation.HasValue && Common.DistanceOctile(haulingJob.ToLocation.Value, combo.StorageLocation.GroundLocation) < 200f)
                                {
                                    return true;
                                }
                            }

                        }
                    }
                }

             /*   if (haulingJob.ToLocation == combo.StorageLocation.GroundLocation
                    && haulingJob.ToStorageEntity == combo.StorageLocation.StorageEntity)
                {

                    if (combo.StorageLocation.Storage == null ||
                        haulingJob.ToStorageCondition == combo.StorageLocation.Storage.StorageConditions)
                        return true;
                }*/

            }

            return false;
        }

        /// <summary>
        /// perhaps implement a dictionary...?
        /// </summary>
        /// <param name="combo"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
     /*   private static HaulingJobSpecificItem GetHaulingJobForItem(ItemStorageCombo combo, EntityGroup owner)
        {
            HaulingJobSpecificItem haulingJob = null;

           
            foreach (Job job in owner.HaulingJobs)
            {
                haulingJob = job as HaulingJobSpecificItem;
                if (haulingJob != null)
                {
                    if (haulingJob.Item == combo.Item)
                    {
                        return haulingJob;
                    }                       
                }
            }

            return null;
        }*/


        public static void CreateHaulingJobsForItemOutOfBand(IKnownEntityData item, EntityGroup ownerOfHaulingJobs)
        {
            List<ItemStorageCombo> allCombos = null;

            List<StorageLocation> storageLocations = null;

            Point? storageCenterMapPosition = null;

            CreateCombosForItemOutOfBand(item, ownerOfHaulingJobs, ref allCombos, ref storageLocations, ref storageCenterMapPosition);

            ScoreCombosAndCreateJobsOutOfBand(ownerOfHaulingJobs, allCombos);
        }

        /// <summary>
        /// this call will try to create hauling jobs for a small list of items if needed... no other hauling jobs will be cancelled/no room will be made for the items in various storage locations.
        /// </summary>
        /// <param name="carriedByEntity"></param>
        public static void CreateHaulingJobsForAllCarriedItemsOutOfBand(Entity carriedByEntity, EntityGroup ownerOfHaulingJobs)
        {
            if (carriedByEntity.AgentStorage.ItemStorage.StoredItems.Count > 0)
            {
                List<ItemStorageCombo> allCombos = null;

                List<StorageLocation> storageLocations = null;

                Point? storageCenterMapPosition = null;

                EntityID id;
                Intelligence entityIntelligence = carriedByEntity.Intelligence;
                IKnownEntityData itemData;
                EntityResult result;
                for (int i = carriedByEntity.AgentStorage.ItemStorage.StoredItems.Count - 1; i >= 0; i--)
                {
                    id = carriedByEntity.AgentStorage.ItemStorage.StoredItems[i];

                    result = entityIntelligence.GetKnownData(id, out itemData);

                    if (result != EntityResult.SeenDirectly)
                    {
                        carriedByEntity.AgentStorage.ItemStorage.RemoveOutdatedItem(id);
                        continue;
                    }

                    CreateCombosForItemOutOfBand(itemData, ownerOfHaulingJobs, ref allCombos, ref storageLocations, ref storageCenterMapPosition);

                }                               

                ScoreCombosAndCreateJobsOutOfBand(ownerOfHaulingJobs, allCombos);
            }
        }

        private static void CreateCombosForItemOutOfBand(IKnownEntityData item, EntityGroup storageLocationsGroup, ref List<ItemStorageCombo> allCombos, ref List<StorageLocation> storageLocations, ref Point? storageCenterMapPosition)
        {
            if (item.AssignedToJob == null)
            {
                if (allCombos == null)
                {
                    allCombos = new List<ItemStorageCombo>();
                }

                if (storageLocations == null)
                {
                    // we need the list of storage locations now:
                    storageLocations = new List<StorageLocation>();

                    if (GatherAllStorageLocations(/*true,*/ storageLocationsGroup, storageLocations, out storageCenterMapPosition))
                    {
                        if (storageCenterMapPosition == null)
                        {
                            return; // why haul anything? we have nowhere to put it!
                        }
                    }
                    else
                    {
                        return; // why haul anything? we have nowhere to put it!
                    }
                }

             //   Allegiance.Allegiance allegiance = ((IHasAllegiance)item.Owner.InternalOwner).Allegiance;
                HaulingJobManager.CreateCombosForItem(storageLocations, allCombos, item);
               
            }

        }

        /// <summary>
        /// will not change ownership status on hauled items
        /// </summary>
        /// <param name="ownerOfHaulingJobs"></param>
        /// <param name="allCombos"></param>
        private static void ScoreCombosAndCreateJobsOutOfBand(EntityGroup ownerOfHaulingJobs, List<ItemStorageCombo> allCombos)
        {
            if (allCombos != null && allCombos.Count > 0)
            {
                // score combos:
                ItemStorageCombo combo;
                double score;
                float damageScore = 0f;

                Allegiances.Allegiance allegiance = ownerOfHaulingJobs.GetAllegiance();
                SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;

                RegionMap regionMap = GetRegionMapToUse(allegiance);         //UWGame.SimSide.Instance.Map.RegionMapManager.HumanFootExposedNormalRegionMap;

                Dictionary<EntityType, Dictionary<StorageLocation, float>> cachedItemStorageScores = new Dictionary<EntityType, Dictionary<StorageLocation, float>>();
                Dictionary<Point, Dictionary<Point, double>> cachedItemLocationScores = new Dictionary<Point, Dictionary<Point, double>>();

                double airliftCapacityScore = CalculateAirliftCapacity(ownerOfHaulingJobs);

                IKnownEntityData itemData;

                for (int i = 0; i < allCombos.Count; i++)
                {
                    if (IsComboValid(ownerOfHaulingJobs, sharedKnowledge, allCombos, ref i, out combo, out itemData))
                    {
                        Result result = ScoreCombo(regionMap, allegiance, airliftCapacityScore, cachedItemStorageScores, cachedItemLocationScores, null, combo, itemData,
                            out score, out damageScore, true);

                        if (result == Result.OK)
                        {
                            combo.Score = score;
                            combo.DamageScore = damageScore;
                        }
                        else
                        {
                            // skip this... we don't have time to wait for Region map results.
                            combo.Score = 0;
                            // isWaiting = true;
                            //  return false;
                        }
                    }                    
                }

                SortCombos(allCombos);

                Dictionary<EntityID, ItemStorageCombo> assignedItems = new Dictionary<EntityID, ItemStorageCombo>();

                ItemStorageCombo currentCombo;
             
                for (int i = 0; i < allCombos.Count; i++)
                {
                    if (IsComboValid(ownerOfHaulingJobs, sharedKnowledge, allCombos, ref i, out currentCombo, out itemData))
                    {   
                        AssignItemToStorage(currentCombo, itemData, assignedItems);
                    }
                }


                CleanListOfItemsAlreadyInTheCorrectPlaceOrHaulingJobExists(sharedKnowledge, assignedItems, ownerOfHaulingJobs);

                // use this to arrange items nicely by category...
                Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>> tileInfluenceMaps = new Dictionary<Point, Tuple<List<Tuple<IKnownEntityData, Point>>, float[][]>>();
                        
                // create jobs finally
                foreach (var kvp in assignedItems)
                {
                    if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(kvp.Key, out itemData)))
                    {
                        continue;
                    }
                    else
                    {
                        bool isHaulToTrade = false;

                        Vector3? location;
                        if (kvp.Value.StorageLocation.GetStorageTarget == null) // .StorageEntity == null)
                        {
                            // select a nice spot within the tile to place the item:
                            location = SelectGroundLocation(kvp.Value, itemData,
                                tileInfluenceMaps);

                            if (location == null)
                            {
                                continue; // don't create a job, nowhere to place the item 
                            }
                        }
                        else
                        {
                            location = null; 
                            isHaulToTrade = kvp.Value.StorageLocation.TradeOfferStorage.HasValue; // .IsTradeOffer;
                        }

                        // also specify storage.
                        HaulingJobSpecificItem job =
                            new HaulingJobSpecificItem(location,
                                kvp.Value.StorageLocation.GetStorageTarget,                            
                                ownerOfHaulingJobs, /*HaulingJobManager.GetPriority(ownerOfHaulingJobs),*/ itemData, null, true, isHaulToTrade, ownerOfHaulingJobs);
                    }
                }
            }
        }

        #region ILookup

        private CyclableID id = CyclableID.Invalid;
     
        //=================== ILookup Methods =====================
        public CyclableID ID
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

        public CyclableID GetUniqueID()
        {
            return Cyclable.GetUniqueID();
        }

        public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
        {
            return (CyclableID)sn.DoEnum(id);
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
            if (ID != CyclableID.Invalid)
                LookUp<ICyclable, CyclableID>.Add(ID, this);
        }

        public void RemoveIDEntry()
        {
            LookUp<ICyclable, CyclableID>.Remove(this);
        }

        public void SetInvalid()
        {
            id = CyclableID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing... Sim will call Cyclable.ResetIDCounter.
        {
        }

        void ILookUp<ICyclable, CyclableID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ICyclable, CyclableID>.Create();
        }

        #endregion


        /*
        private Storage FindBestItemStorage(Item item)
        {
            List<Storage> storageLocations;
                       

            if (cachedBestLocations[item.Owner].TryGetValue(item.ItemType, out storageLocations))
            {
                foreach (Storage storage in storageLocations)
                {
                    if (StorageHasCapacity(storage, item))
                    {
                        return storage;
                    }
                }
            }


           
            // step through the best options first:
            foreach (DegradeType.StorageDamageEstimation storageDamage in item.ItemType.DegradeType.ConditionDamages)
            {
                if (StorageSpaces.TryGetValue(storageDamage.Condition, out storage))
                {
                    if (storage.HasCapacityForItem(item))
                    {
                       
                        return true;
                    }
                }
            }


        }*/

        [DebuggerDisplay("{NormalStorage}, {GroundLocation}, Zone: {Zone}")]
        private class StorageLocation
        {
            public StorageTarget? NormalStorage;
            public StorageTarget? TradeOfferStorage;


            public StorageTarget? GetStorageTarget
            {
                get
                {
                    return NormalStorage ?? TradeOfferStorage;
                }
            }

            /*
            public EntityID? StorageEntity;

            /// <summary>
            /// this is a copy of the original Entity storage, not a pointer to it.
            /// </summary>
            public Storage Storage;
            //  public Storage.Conditions? StorageCondition;
             
            public bool IsTradeOffer;
            */


            /// <summary>
            /// 
            /// user/AI settings that apply to a storage entity or the ground location
            /// </summary>
            public Stockpile Stockpile;

           
            public Zone Zone; // TODO: make it a ZoneID
                      

            // counters
            public float TotalCapacity;
            public float TotalStored;

            /// <summary>
            /// only needed to check when Stockpile has limits less than unlimited
            /// </summary>
            public Dictionary<EntityType, Pair<int, int>> TotalStoredItems;
           // public Dictionary<EntityType, int> TotalStoredItems;


            /// <summary>
            /// gets assigned a value even when using storage?
            /// </summary>
            public Vector3 GroundLocation;

            /// <summary>
            /// cached distance (by air) of storage location to the home/expedition/person
            /// </summary>
            public double TravelTimeToCenterScore;


            public bool StoredItemsAreAtMaximum(EntityType itemType, ref Pair<int, int> limit)
            {
                 if (TotalStoredItems != null)
                 {
                     //int totalStored;
                     //Tuple<int, int> value;
                     if (TotalStoredItems.TryGetValue(itemType, out limit))
                     {
                         return limit.First >= limit.Second;

                       /*  int maxLimit;
                         if (Stockpile.TryGetItem(itemType, out maxLimit))
                         {

                         }*/
                     }
                 }

                
                 return false;

            }

        }

        [DebuggerDisplay("{Item} {StorageLocation}")]
        private class ItemStorageCombo
        {
            public EntityID Item; 
            private StorageLocation location;
            public StorageLocation StorageLocation
            {
                get{ return location;}
                set {  location = value; }
            }

            public double Score;

            public float DamageScore;
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {            
            // when snapshotting, we scrap the current progress.

            this.id = SnapshotID(sn, id);
            this.snapshotParent = (EntityGroupID)sn.SnapshotID<EntityGroup, EntityGroupID>(parent);
            this.notifyWhenRegionSearchIsFinished = (MethodID)sn.DoEnum(notifyWhenRegionSearchIsFinished);

            takeOffAndLandTime = sn.DoFloat(takeOffAndLandTime);
            oneOverBaseSpeedByAir = sn.DoFloat(oneOverBaseSpeedByAir);
            oneOverBaseSpeedOnFoot = sn.DoFloat(oneOverBaseSpeedOnFoot);

            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

            sn.Ignore(cachedItemLocationScores);
            sn.Ignore(cachedItemStorageScores);
            sn.Ignore(this.airliftCapacityScore);
            sn.Ignore(this.allCombos);
            sn.Ignore(this.allItems);
            sn.Ignore(this.AllStorageLocations);
            sn.Ignore(this.assignedItems);
            sn.Ignore(this.comboCounter);
            sn.Ignore(this.itemCounter);
            sn.Ignore(this.outdatedJobs);
            sn.Ignore(this.storageCenterMapPosition);
            sn.Ignore(phase);
            sn.Ignore(regulator);
            sn.Ignore(isWaiting);

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

            parent = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotParent);

            ActionLookup.Add(notifyWhenRegionSearchIsFinished, NotifyWhenRegionSearchIsFinished);

            CreateRegulators();
        }
        
        #endregion


        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return true; // scrap progress & start over
            }
        }

        public void Destroy()
        {
            The.Sim.CycleManager.UnRegister(this);

            ActionLookup.Remove(notifyWhenRegionSearchIsFinished);

            RemoveIDEntry();
        }
    }
}
