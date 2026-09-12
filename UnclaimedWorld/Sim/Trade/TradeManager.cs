using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Allegiances;

namespace UWGame.SimSide.Trade
{

    /// <summary>
    /// Manages items for trade in human othersite expeditions. Placeholder...
    /// 
    /// Enough to set prices once for all terminals in the expedition
    /// 
    /// Making this cyclable means that updates may be missed/queued...
    /// </summary>
    public class TradeManager: ISnapshot //, ICyclable
    {
       

        EntityGroup owner;
        EntityGroupID snapshotOwnerID;

        private Regulator linearProductionRegulator;

        /// <summary>
        /// has lower frequency
        /// </summary>
        private Regulator variableProductionRegulator;



        /// <summary>
        /// all entities, sorted by type. This should be in the Terminal class...
        /// </summary>
       // public Dictionary<EntityType, List<EntityID>> AllEntitiesAvailableForTrade = new Dictionary<EntityType, List<EntityID>>();


        #region Planner placeholders

        /// <summary>
        /// the vehicle types, and their price, that are offered for hire
        /// </summary>
        private Dictionary<EntityType, VehiclesForHireAmount> VehiclesForHire = new Dictionary<EntityType, VehiclesForHireAmount>();


        /// <summary>
        /// is used to control the available amounts
        /// </summary>
        private Dictionary<EntityType, TradeAmount> TradeAmounts = new Dictionary<EntityType, TradeAmount>();

      
        #endregion

        public TradeManager()
        {

        }

        public TradeManager(EntityGroup owner)
        {
            this.owner = owner;

            CreateRegulators();

           /* if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();

                CreateRegulators();
            }*/
        }


        public static TradeManager CreateFromTradeAmounts(EntityGroup entityGroup, 
            SerializableDictionary<string, TradeAmountType> AvailableForTrade, // TradeAmountTypes tradeAmounts)
            SerializableDictionary<string, VehiclesForHireType> VehiclesForHire,
            PricesProfile pricesProfile)
        {
            TradeManager manager = new TradeManager(entityGroup);
            
            // init goods for trading:
         
            if (AvailableForTrade != null)
            {
                manager.SetTradeProperties(AvailableForTrade, pricesProfile);
            }

            // setup vehicles that can be hired:
            manager.SetVehiclesForHire(VehiclesForHire);

            return manager;
        }

        public void SetVehiclesForHire(SerializableDictionary<string, VehiclesForHireType> VehiclesForHire)
        {
            if (VehiclesForHire != null)
            {
                EntityType entityType;
                foreach (var item in VehiclesForHire)
                {
                    if (GameData.Instance.AllEntityTypes.TryGetValue(item.Key, out entityType))
                    {
                        //  manager.VehiclesAvailableForHire.Add(entityType, item.Value.StartAmount.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator));

                        SetVehiclesOfferedForHireProperties(entityType, item.Value, 1f);
                    }
                }
            }
        }

        public void SetTradeProperties(SerializableDictionary<string, TradeAmountType> AvailableForTrade, PricesProfile pricesProfile)
        {
            EntityType entityType;
            foreach (var item in AvailableForTrade)
            {
                if (GameData.Instance.AllEntityTypes.TryGetValue(item.Key, out entityType))
                {                    
                    SetTradeProperties(entityType, item.Value, 1f, null, pricesProfile, true, true);
                }
            }
        }

        public void SpawnStartingVehicles() 
        {       
            foreach (var item in VehiclesForHire) // profile.VehiclesForHire)
            {
               // VehiclesForHireType type = item.Value;
                VehiclesForHireAmount amount = item.Value;

               // int amountToStart = Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0, item.Value.StartAmount + 1);

                for (int i = 0; i < amount.StartAmount; i++)
                {
                    Site site = owner.Parent.Allegiance.Site;
                    Entity entity = Entity.CreateAndInitEntity(item.Key, site, allegiance: owner.Parent.Allegiance);

                    /*
                        !outputEntity.PlaceEntityOnPlaySite(null, container, null,
                            new Entity.SetOwnerInfo((IOwner)owner.Parent), placeProductsInCompartment: StorageCompartment.OfferedForTrade)) 
                     */

                    entity.PlaceEntityOnOtherSite(site, null, null, new Entity.SetOwnerInfo((IOwner)owner.Parent), null);
                    entity.ComeOnline(); // New
                }
            }
        }

        private void CreateRandomTradeAmountsFromGroup(TradeProfile profile, RandomTrade randomTrade, NormalDistribution scaleDistribution, float sizeFactor, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
        {
            if (randomTrade != null)
            {
                List<string> options = randomTrade.Options.ToList();
                for (int i = 0; i < randomTrade.NoOfGroups; i++)
                {
                    string key = Common.GetRandomListMember(options, The.Sim.GameplayRandomGenerator);

                    options.Remove(key);

                    TradeGroup tradeGroup = GameData.Instance.AllTradeGroups[key];

                    CreateTradeAmounts(profile, scaleDistribution, sizeFactor, tradeGroup, pricesProfile, allowDemand, allowProduction);

                    if (options.Count == 0)
                    {
                        return;
                    }
                }
            }

        }

        private void CreateTradeAmountsFromGroup(TradeProfile tradeProfile, string[] tradeGroups, NormalDistribution scaleDistribution, float sizeFactor, PricesProfile pricesProfile, bool allowDemand, bool allowProduction) 
        {
            if (tradeGroups != null)
            {
                foreach (var item in tradeGroups)
                {
                    TradeGroup tradeGroup = GameData.Instance.AllTradeGroups[item];

                    CreateTradeAmounts(tradeProfile, scaleDistribution, sizeFactor, tradeGroup, pricesProfile, allowDemand, allowProduction);
                }
            }
        }

        private void CreateTradeAmounts(TradeProfile tradeProfile, NormalDistribution scaleDistribution, float sizeFactor, TradeGroup tradeGroup, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
        {
            int priority;
            if (tradeProfile.TradeGroupPriority != null && tradeProfile.TradeGroupPriority.TryGetValue(tradeGroup.KeyName, out priority))
            {
                // create these last, to override the previously set
                TradeGroupParams parms = new TradeGroupParams()
                {
                    AllowDemand = allowDemand,
                    AllowProduction = allowProduction,
                    Priority = priority,
                    SizeFactor = sizeFactor,
                    NormalDistribution = scaleDistribution,
                    PricesProfile = pricesProfile
                };

                Common.AddToDictionary(ref overridingTradeGroups, tradeGroup, parms);               
            }
            else
            {
                CreateTradeAmountsNow(scaleDistribution, sizeFactor, tradeGroup, pricesProfile, allowDemand, allowProduction);
            }
        }

        private void CreateTradeAmountsNow(NormalDistribution scaleDistribution, float sizeFactor, TradeGroup tradeGroup, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
        {
            OfferDemandProfile profile = null;
            if (tradeGroup.OfferDemandProfile != null)
            {
                profile = GameData.Instance.AllOfferDemandProfiles[tradeGroup.OfferDemandProfile];
            }

            foreach (var ware in tradeGroup.AvailableForTrade)
            {
                EntityType entityType = GameData.Instance.AllEntityTypes[ware.GetEntityTypeKey()]; //.Key];

                SetTradeProperties(entityType, ware, sizeFactor * (float)scaleDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator, true), profile, pricesProfile, allowDemand, allowProduction);
            }
        }

        private class TradeGroupParams
        {
            public int Priority;
            public NormalDistribution NormalDistribution;
            public bool AllowDemand;
            public bool AllowProduction;
            public float SizeFactor;
            public PricesProfile PricesProfile;
        }

        private Dictionary<TradeGroup, TradeGroupParams> overridingTradeGroups;

        public void FillFromTradeProfile(TradeProfile tradeProfile, float sizeFactor, PricesProfile pricesProfile)
        {           
            // init goods for trading:
          
           // tradeProfile.TradeGroupPriority

            CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.HighTrade, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, true, true);
            CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.MediumTrade, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, true, true);
            CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.LowTrade, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile, true, true);

            CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.HighImport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, true, false);
            CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.MediumImport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, true, false);
            CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.LowImport, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile, true, false);

            CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.HighExport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, false, true);
            CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.MediumExport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, false, true);
            CreateTradeAmountsFromGroup(tradeProfile, tradeProfile.LowExport, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile, false, true);


            CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomHighTrade, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, true, true);
            CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomMediumTrade, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, true, true);
            CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomLowTrade, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile, true, true);

            CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomHighImport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, true, false);
            CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomMediumImport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, true, false);
            CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomLowImport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, true, false);

            CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomHighExport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile, false, true);
            CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomMediumExport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile, false, true);
            CreateRandomTradeAmountsFromGroup(tradeProfile, tradeProfile.RandomLowExport, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile, false, true);
           
            /*

            CreateTradeAmountsFromGroup(tradeProfile.HighExport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile);
            CreateTradeAmountsFromGroup(tradeProfile.MediumExport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile);
            CreateTradeAmountsFromGroup(tradeProfile.LowExport, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile);
            CreateTradeAmountsFromGroup(tradeProfile.HighImport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile);
            CreateTradeAmountsFromGroup(tradeProfile.MediumImport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile);
            CreateTradeAmountsFromGroup(tradeProfile.LowImport, GameData.Instance.Constants.LowTradeAmountDistribution, sizeFactor, pricesProfile);

            CreateRandomTradeAmountsFromGroup(tradeProfile.RandomHighExport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile);
            CreateRandomTradeAmountsFromGroup(tradeProfile.RandomHighImport, GameData.Instance.Constants.HighTradeAmountDistribution, sizeFactor, pricesProfile);
            CreateRandomTradeAmountsFromGroup(tradeProfile.RandomMediumExport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile);
            CreateRandomTradeAmountsFromGroup(tradeProfile.RandomMediumImport, GameData.Instance.Constants.MediumTradeAmountDistribution, sizeFactor, pricesProfile);
          */

            if (overridingTradeGroups != null && overridingTradeGroups.Count > 0)
            {
                var sortedGroups = overridingTradeGroups.OrderByDescending(k => k.Value.Priority);

                foreach (var item in sortedGroups)
                {
                    CreateTradeAmountsNow(item.Value.NormalDistribution, item.Value.SizeFactor, item.Key, item.Value.PricesProfile, item.Value.AllowDemand, item.Value.AllowProduction);
                }
            }
        }

        /// <summary>
        /// setup types of vehicles that can be hired, but don't spawn them
        /// </summary>
        /// <param name="tradeProfile"></param>
        /// <param name="sizeFactor"></param>
        public void FillFromVehiclesProfile(VehiclesProfile tradeProfile, float sizeFactor)
        {           
            if (tradeProfile.VehiclesForHire != null)
            {
                foreach (var item in tradeProfile.VehiclesForHire)
                {
                    EntityType entityType;
                    if (GameData.Instance.AllEntityTypes.TryGetValue(item.Key, out entityType))
                    {
                        // VehiclesAvailableForHire.Add(entityType, item.Value.StartAmount.GetRandomIntegerValue(The.Sim.GameplayRandomGenerator));

                        SetVehiclesOfferedForHireProperties(entityType, item.Value, sizeFactor);
                    }
                }
            }
        }

        public float? GetSellPrice(EntityType itemType)
        {
            TradeAmount amount;
            if (TradeAmounts.TryGetValue(itemType, out amount))
            {
                return amount.SellPrice;
            }

            return null;
        }

        public float? GetBuyPrice(EntityType itemType)
        {
            TradeAmount amount;
            if (TradeAmounts.TryGetValue(itemType, out amount))
            {
                return amount.BuyPrice;
            }

            return null;
        }

        public int GetBuyAmount(EntityType itemType)
        {
            TradeAmount amount;
            if (TradeAmounts.TryGetValue(itemType, out amount))
            {
                return amount.AmountToBuy;
            }

            return 0;
        }


        public decimal? GetPriceToHire(EntityType vehicleType, out decimal? pricePerKilometer)
        {
            VehiclesForHireAmount amount;
            if (VehiclesForHire.TryGetValue(vehicleType, out amount))
            {
                pricePerKilometer = amount.PricePerKilometer;
                return amount.Price;
            }

            pricePerKilometer = null;
            return null;
        }

        private void SetTradeProperties(EntityType entityType, TradeAmountType amountType, float scaleAmounts, OfferDemandProfile profile, PricesProfile pricesProfile, bool allowDemand, bool allowProduction)
        {
          
            TradeAmount amount = new TradeAmount(entityType.KeyName, amountType, scaleAmounts, profile, pricesProfile, allowDemand, allowProduction);          

            TradeAmounts[entityType] = amount;
        }

        public void SetVehiclesOfferedForHireProperties(EntityType entityType, VehiclesForHireType amountType, float sizeFactor)
        {
            VehiclesForHireAmount amount = new VehiclesForHireAmount(amountType, sizeFactor);
      
            VehiclesForHire.Add(entityType, amount);
        }

        public void Update(GameTime gameTime)
        {
            double milliSecondsSinceLastReady = 0;
            if (linearProductionRegulator.IsReady(ref milliSecondsSinceLastReady))
            {
                double daysPassed = The.Sim.DateAndTime.MillisecondsToDays(milliSecondsSinceLastReady);
                
                ProduceTradeItems(daysPassed);

                SimulateConsumingTradeItems(daysPassed);

            }

            // not sure if this low-freq regulator is needed... unsure if the edge probabilities can be converted like 
            // does a higher frequency affect the result???
            if (variableProductionRegulator.IsReady(ref milliSecondsSinceLastReady))
            {
                double daysPassed = The.Sim.DateAndTime.MillisecondsToDays(milliSecondsSinceLastReady);
                
                ProduceVariableTradeItems(daysPassed);
            }
        }

        void CreateRegulators()
        {
           // regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / UpdateInterval.Value, "TradeManager");
            linearProductionRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.1d /* 1d / UpdateInterval.Value*/, "TradeManager1");

            variableProductionRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.03d, "TradeManager2");
        }

        public void Destroy()
        {
           
            /*The.Sim.CycleManager.UnRegister(this);

            RemoveIDEntry();*/
        }

       /* public void SetRandomStartDemand()
        {
            foreach (var item in TradeAmounts)
            {
                if (item.Key.KeyName.Contains("driedBeef"))
                {

                }

                TradeAmount tradeAmount = item.Value;
                if (tradeAmount.MaxAmountToBuy > 0)
                {
                    tradeAmount.AmountToBuy = Common.RandomBetween(The.Sim.GameplayRandomGenerator, 1, tradeAmount.MaxAmountToBuy.Value + 1);
                }
            }
        }*/


        /// <summary>
        /// call this after spawning the terminals.
        /// </summary>
        public void SpawnStartingTradeItems()
        {
            List<Entity> terminals;
            if (GetTerminals(out terminals))
            {
                List<Tuple<EntityType, EntityData, int>> spawnAmounts = null; // collect first, then spawn one at a time until we're full, to fill the space with all the various types

                foreach (var item in TradeAmounts)
                {       
                    if (item.Key.KeyName.Contains("hauling"))
                    {

                    }

                    TradeAmount tradeAmount = item.Value;
                    int? amountToStart = null;

                    if (tradeAmount.StartAmount != null)
                    {
                        amountToStart = tradeAmount.StartAmount.Value;
                    }
                    else if (tradeAmount.MaxAmountForSale > 0)
                    {
                        amountToStart = Common.RandomBetween(The.Sim.GameplayRandomGenerator, 0, tradeAmount.MaxAmountForSale.Value + 1);
                    }

                    if (amountToStart > 0)
                    {
                        Common.AddToList(ref spawnAmounts, new Tuple<EntityType, EntityData, int>(item.Key, tradeAmount.EntityData, amountToStart.Value));
                    }
                }

                DistributeProducedItems(spawnAmounts, terminals);
            }          
        }

        private bool GetTerminals(out List<Entity> terminals)
        {
            terminals = GetTradeTerminals();

            if (terminals == null) 
                return false;
            
            // shuffle:
            terminals = Common.Randomize(terminals, The.Sim.GameplayRandomGenerator);

            return true;
        }

        /// <summary>
        /// count items in all terminals
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private int GetItemsForSaleByOtherSite(EntityType entityType)
        {
            int total = 0;

            List<EntityID> list;
            if (owner.AllEntities.TryGetValue(entityType, out list))
            {
                foreach (var entityID in list)
                {
                    if (ItemIsValidForSale(entityID))
                    {
                        total++;
                        /* Common.AddToList(ref listOfEntities,
                             entityID);*/
                    }
                }
            }

            return total;
        }

        private bool ItemIsValidForSale(EntityID entityID)
        {
            Entity item = Entity.FindByID(entityID);

            if (item != null)
            {
                if (item.ContainedBy == null
                    || item.OwnedBy != ((IOwner)owner.Parent).ID)
                {
                    return false;
                }

                return true;
            }

            return false;
        }



        List<Entity> GetTradeTerminals()
        {
            List<Entity> terminals = null;
            foreach (var item in owner.Terminals)
            {
                foreach (var entityID in item.Value)
                {
                    Entity terminal = Entity.FindByID(entityID);
                    if (terminal != null)
                    {
                        if (terminal.OfferedEntitiesByType != null)
                        {
                            Common.AddToList(ref terminals, terminal);
                        }
                    }
                }       
            }

            return terminals;
        }

        private void ProduceVariableTradeItems(double daysElapsed)
        {
            List<Entity> terminals;

            if (GetTerminals(out terminals))
            {
                List<Entity> currentTerminals = new List<Entity>();

               // Dictionary<EntityType, int> spawnAmounts = null; // collect first, then spawn one at a time until we're full
                List<Tuple<EntityType, EntityData, int>> spawnAmounts = null; // collect first, then spawn one at a time until we're full

                foreach (var item in TradeAmounts)
                {
                    currentTerminals.Clear();
                    currentTerminals.AddRange(terminals);

                    TradeAmount tradeAmount = item.Value;

                    if (tradeAmount.OfferDemandProfile != null)
                    {
                        int currentItems = GetItemsForSaleByOtherSite(item.Key);
                        int itemsToSpawn;
                        if (tradeAmount.OfferDemandProfile.States != null)
                        {
                            int index;
                            OfferDemandState state = Common.GetStairStepIndex(tradeAmount.OfferDemandProfile.States, out index, The.Sim.GameplayRandomGenerator);

                            // get the growth for the state we're in (can be negative):
                            float growth = (float)state.OfferDemandChange.GetRandomValue(The.Sim.GameplayRandomGenerator);

                            itemsToSpawn = Population.GetStepsFromProgress(growth,
                                    daysElapsed, ref tradeAmount.Progress, currentItems, 0, tradeAmount.MaxAmountForSale.Value);
                        }
                        else if (tradeAmount.OfferedForTradeNoise != null)
                        {
                            NoiseParams noise = tradeAmount.OfferDemandProfile.NonlinearAmountForSale;
                       
                            // the noise should give the increase instead of the current amount... otherwise the player can never buy up everything

                            float increase = tradeAmount.OfferedForTradeNoise.Generate1D((float)The.Sim.TotalUnPausedGameTimeInSeconds, 
                                noise.NoiseFrequency,
                                tradeAmount.OfferDemandProfileScaleFactor * (noise.NoiseAmplitude ?? 1f),
                                tradeAmount.OfferDemandProfileScaleFactor * (noise.NoiseAddend ?? 0f));


                            itemsToSpawn = (int)(Math.Abs(increase));
                            itemsToSpawn *= Math.Sign(increase);

                            int beforeClamp = itemsToSpawn;
                           

                            // clamp:
                            itemsToSpawn = Common.ClampTop(itemsToSpawn, tradeAmount.MaxAmountForSale.Value - currentItems);
                            itemsToSpawn = Common.ClampBottom(itemsToSpawn, -currentItems);

#if DEBUG
                            if (item.Key.KeyName == "item:cotton")
                            {
                                Console.WriteLine(string.Format("Cotton amount: {0}, increase: {1}, items to spawn: {2}, clamped: {3}", currentItems, increase, beforeClamp, itemsToSpawn));
                            }
#endif

                            /*int newWholeAmount = (int)newAmount;
                            itemsToSpawn = newWholeAmount - currentItems;*/
                        }
                        else
                        {
                            itemsToSpawn = 0;
                        }

                        if (itemsToSpawn > 0)
                        {
                           // Common.AddToDictionary(ref spawnAmounts, itemsToSpawn);
                            Common.AddToList(ref spawnAmounts, new Tuple<EntityType, EntityData, int>(item.Key, tradeAmount.EntityData, itemsToSpawn));

                          /*  if (!ProduceItems(item.Key, itemsToSpawn, currentTerminals))
                            {
                                return;
                            }*/
                        }
                        else if (itemsToSpawn < 0)
                        {
                            // destroy some items:
                            DestroyItems(item.Key, Math.Abs(itemsToSpawn), currentTerminals);
                        }
                    }
                }

                // distribute item types over terminals:
                DistributeProducedItems(spawnAmounts, terminals);

            }
        }

        private void DistributeProducedItems(List<Tuple<EntityType, EntityData, int>> spawnAmounts, List<Entity> terminals)
        {
            if (spawnAmounts != null)
            {
                List<Entity> currentTerminals = new List<Entity>();
                                

                do
                {
                    for (int i = spawnAmounts.Count - 1; i >= 0; i--)
                    {
                        currentTerminals.Clear();
                        currentTerminals.AddRange(terminals);

                        var amount = spawnAmounts[i];

                        if (amount.Item3 > 0)
                        {
                            if (!ProduceItems(amount.Item1, amount.Item2, 1, currentTerminals))
                            {
                                return;
                            }

                            int leftAmount = amount.Item3 - 1;
                            if (leftAmount == 0)
                            {
                                spawnAmounts.RemoveAt(i);
                            }
                            else
                            {
                                spawnAmounts[i] = new Tuple<EntityType, EntityData, int>(amount.Item1, amount.Item2, leftAmount);
                            }
                        }
                        else
                        {
                            spawnAmounts.RemoveAt(i);
                        }
                    }
                }
                while (spawnAmounts.Count > 0);
            }
        }


        /// <summary>
        /// divide the items between the terminals
        /// </summary>
        /// <param name="daysPassed"></param>
        /// <returns></returns>
        private void ProduceTradeItems(double daysElapsed)
        {           
             List<Entity> terminals;
             if (GetTerminals(out terminals))
             {
                 List<Tuple<EntityType, EntityData, int>> spawnAmounts = null; // collect first, then spawn one at a time until we're full

                 foreach (var item in TradeAmounts)
                 {
                     TradeAmount tradeAmount = item.Value;

                     if (tradeAmount.IncreasePerDay > 0)
                     {
                         int currentItems = GetItemsForSaleByOtherSite(item.Key);

                         if (tradeAmount.MaxAmountForSale.HasValue)
                         {

                             int itemsToSpawn = Population.GetStepsFromProgress(tradeAmount.IncreasePerDay,
                                 daysElapsed, ref tradeAmount.Progress, currentItems, 0, tradeAmount.MaxAmountForSale.Value); //membersToFill);
                            

                             if (itemsToSpawn > 0)
                             {
                                 Common.AddToList(ref spawnAmounts, new Tuple<EntityType, EntityData, int>(item.Key, tradeAmount.EntityData, itemsToSpawn));                                
                             }

                         }

                     }
                 }

                 // distribute item types over terminals:
                 DistributeProducedItems(spawnAmounts, terminals);

             }
        }

        private void DestroyItems(EntityType entityType, int itemsToDestroy, List<Entity> terminals)
        {
            Entity container;

            int itemsDestroyed = 0;

            int i = 0;

            // cycle between terminals
            do
            {
                container = terminals[i % terminals.Count];

                if (container != null)
                {
                    //TerminalContainer terminal = container.Contains as TerminalContainer; 

                    List<EntityID> list;
                    if (container.OfferedEntitiesByType.TryGetValue(entityType, out list) && list.Count > 0)
                    {
                        Entity item = Entity.FindByID(list[0]);
                        if (item != null)
                        {
                            item.Destroy();

                            itemsDestroyed++;
                        }
                    }
                    else
                    {
                        terminals.Remove(container);
                    }
                }

                i++;
            }
            while (itemsDestroyed < itemsToDestroy && terminals.Count > 0);
        }

        public static float? GetBulkOfTradeItem(EntityType entityType)
        {
            if (entityType.ItemType != null)
            {
                return entityType.ItemType.MaximumBulk; // cannot trade carcasses... // ?? 0f;
            }
            else if (entityType.BiologicalType != null)
            {
                return entityType.BiologicalType.GetMaxBulk(); // .MeanBulkOfAdultMember;
            }
            else if (entityType.BodyType != null)
            {
                return entityType.BodyType.Bulk; // NEW - for robots
            }
            else
            {
                throw new Exception("Cannot trade this type: " + entityType.KeyName);
            }
        }

        /// <summary>
        /// calling this by type means the storage can be filled by just a few item types.
        /// Let's spawn one item of each type at a time instead
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="itemsToSpawn"></param>
        /// <param name="terminals"></param>
        /// <returns></returns>
        private bool ProduceItems(EntityType entityType, EntityData entityData, int itemsToSpawn, List<Entity> terminals)
        {           
            // put an equal no. of items in each terminal, total = MaxForSale
            Entity container;

            if (entityType.KeyName.Contains("hauling"))
            {

            }

            float bulk = GetBulkOfTradeItem(entityType).Value;

            Allegiance allegiance = owner.GetAllegiance();
            Expedition expedition = owner.GetExpedition();


            for (int i = 0; i < itemsToSpawn; i++)
            {
                // find a terminal with room
                container = GetTerminalWithRoom(terminals, bulk, i % terminals.Count);

                if (container != null)
                {
                    Site site = owner.Parent.Allegiance.Site;

                    bool placementSucceeded;

                   

                    if (entityData != null)
                    {                      
                      
                        bool placementFailed;
                        Entity spawnedEntity = MapLoader.CreateAndPlaceEntityFromEntityData(entityData, out placementFailed, 
                            container, 
                            placeInStorage: null, offerForSale: true, 
                            owningAllegianceKey: allegiance.KeyName, owningExpeditionKey: expedition.KeyName,
                            memberOfAllegianceKey: allegiance.KeyName, memberOfExpeditionKey: expedition.KeyName, 
                            siteKey: site.KeyName,
                            assertContainment: false, // ??                       
                            suppressSpawningEvents: true);

                        placementSucceeded = !placementFailed;
                    }
                    else
                    {
                        Entity entity = Entity.CreateAndInitEntity(entityType, site, allegiance: allegiance);

                        placementSucceeded = entity.PlaceEntityOnOtherSite(site, container, null, new Entity.SetOwnerInfo((IOwner)owner.Parent), null, compartment: StorageCompartment.OfferedForTrade);
                        
                    }

                    if (!placementSucceeded)
                    {
                        terminals.Remove(container); // no more room..?

                        if (terminals.Count == 0)
                            return false;
                    }

                }
                else return false;
            }

            return true;
        }

        private Entity GetTerminalWithRoom(List<Entity> terminals, float bulk, int index)
        {
            do
            {
                Entity container = terminals[index];

                TerminalContainer terminal = container.Contains as TerminalContainer;

                if (terminal.TotalTradeItemStorageCapacity - terminal.TotalTradeItemsStored < bulk)
                {
                    terminals.RemoveAt(index);
                }
                else
                {
                    return container;
                }

            }
            while (terminals.Count > 0);

            return null;
        }


        /// <summary>
        /// adjusts the counters that control how much the player can sell here. No actual items are consumed.
        /// </summary>
        /// <param name="daysPassed"></param>
        /// <returns></returns>
        private void SimulateConsumingTradeItems(double daysElapsed)
        {
            foreach (var item in TradeAmounts)
            {
                 TradeAmount tradeAmount = item.Value;
                 if (tradeAmount.ConsumptionPerDay > 0)
                 {
                     if (tradeAmount.MaxAmountToBuy.HasValue
                         && tradeAmount.AmountToBuy < tradeAmount.MaxAmountToBuy.Value)
                     {
                         int currentItems = tradeAmount.AmountToBuy;
                         int freeSpace = tradeAmount.MaxAmountToBuy.Value - currentItems;

                         int itemsToConsume = Population.GetNoToSpawn(tradeAmount.ConsumptionPerDay,
                              daysElapsed, ref tradeAmount.TimeInDaysElapsedSinceItemConsumed, freeSpace);

                         if (itemsToConsume > 0)
                         {
                             tradeAmount.AmountToBuy += itemsToConsume;
                             tradeAmount.AmountToBuy = Common.ClampTop(tradeAmount.AmountToBuy, tradeAmount.MaxAmountToBuy.Value);
                         }
                     }
                 }
            }
        }

        /// <summary>
        /// adjusts the amount we are willing to buy, if this is a simulated trading partner.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        public void Buy(EntityType item, int amount)
        {
            TradeAmount tradeAmount;
            if (TradeAmounts.TryGetValue(item, out tradeAmount))
            {
                tradeAmount.AmountToBuy -= amount;

                tradeAmount.AmountToBuy = Common.ClampBottom(tradeAmount.AmountToBuy, 0);
                if (tradeAmount.MaxAmountToBuy.HasValue)
                {
                    tradeAmount.AmountToBuy = Common.ClampTop(tradeAmount.AmountToBuy, tradeAmount.MaxAmountToBuy.Value);
                }
                
            }

        }

      /*  #region ICyclable Members

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("TradeManager {0}:", ID));

        }

        public bool CycleOnce()
        {
            switch (phase)
            {
                case Phase.ProduceTradeItems:

                    if (ProduceTradeItems())
                    {

                        phase = Phase.ConsumeTradeItems;
                    }

                    break;

                case Phase.ConsumeTradeItems:

                    if (ConsumeTradeItems())
                    {

                        phase = Phase.ProduceTradeItems;

                        return true;
                    }

                    break; 
            }

            return false;
        }

        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return true;
            }
        }

        #endregion*/


     /*   #region ILookup

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
            ID = CyclableID.Invalid;
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

        #endregion*/


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            //AvailableForTrade = sn.DoDictionary(AvailableForTrade);
            //VehiclesAvailableForHire = sn.DoDictionary(VehiclesAvailableForHire);

            TradeAmounts = sn.DoDictionary(TradeAmounts);
            VehiclesForHire = sn.DoDictionary(VehiclesForHire);

            this.snapshotOwnerID = (EntityGroupID)sn.SnapshotID<EntityGroup, EntityGroupID>(owner);

            //sn.Ignore(owner);

            sn.Ignore(overridingTradeGroups);


            return this;
        }



        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (VehiclesForHire != null)
            {
                foreach (var item in VehiclesForHire)
                {
                    item.Value.LoadPostProcess(sn);
                }
            }

            owner = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnerID);

            if (TradeAmounts != null)
            {
                foreach (var item in TradeAmounts)
                {
                    item.Value.LoadPostProcess(sn);
                }
            }

            CreateRegulators();
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
    }
}
