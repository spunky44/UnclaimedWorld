using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Expeditions;
using WindowSystem;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Resources;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Policies;

namespace UWGame.ClientSide.Interface.Inventory
{
    /// <summary>
    /// programmer defined
    /// </summary>
    /// 
    public enum StaticFilterSettings
    {
        Containers, Storage, Fuel/*Fertilizer*/, BetterTools, Structures, Items, UsableAsWeapon, AffectsFoodRating, AffectsSecurityRating, AffectsComfortRating
    }

  //  public enum ProductionAvailability { CanBuildNow, MissingOneInputType, FurtherAway }

    public class InventorySettings : ISnapshot
    {
        public enum SortColumns { Name, InStock, CanProduce, Tracking }

        public SortingSettings<SortColumns> SortingSettings;
        /*
        public SortColumns SortedBy = DefaultSortColumn;

        public Grid.Sorting SortOrder = DefaultSortOrder;
        */
        /*
        public const SortColumns DefaultSortColumn = SortColumns.Name;
        public const Grid.Sorting DefaultSortOrder = Grid.Sorting.Ascending;
        */

        public FilterPropertySettings FilterPropertySettings;

        //private int attainableProgress = 0;

        /// <summary>
        /// in progress list
        /// </summary>
        private List<ProcessType> processesToDo;

        /// <summary>
        /// we keep and manage our own copy because of timeslicing
        /// why public???
        /// </summary>
        public Dictionary<EntityType, InventoryPanel.Availability> AllAvailableItems = new Dictionary<EntityType,InventoryPanel.Availability>();

        //private bool attainableIsDirty = true;
        private Regulator attainabilityRegulator = new Regulator(The.Client.ClientRandomGenerator, 0.3, "InventorySettings");

        bool settingsAreDirty = true;

        public enum Availability { AvailableNow, Attainable, AllKnownBlueprints }

        private Availability availabilitySettings = Availability.AvailableNow;
        public Availability AvailabilitySettings
        {
            get
            {
                return availabilitySettings;
            }
            set
            {
                if (availabilitySettings != value)
                {
                    availabilitySettings = value;
                  //  settingsAreDirty = true;
                }
            }
        }

        public enum AndOr { Or, And }

        private AndOr andOrSetting = AndOr.Or;//default settings for production manager... is Or. overwritten when saved..
        public AndOr AndOrSetting
        {
            get
            {
                return andOrSetting;
            }
            set
            {
                if (andOrSetting != value)
                {
                    andOrSetting = value;
                    settingsAreDirty = true;
                }

            }
        }

        public bool IsExpanded = true; 

        private List<Color> AvailableColors;

        public const string barTooltipBeingTracked = "Being tracked";

        
       
        public Dictionary<EntityType, TrackTarget> TrackedTargets = new Dictionary<EntityType, TrackTarget>();
        
        /// <summary>
        /// supersets for all tracked entity types. recompute when adding/removing trackers
        /// </summary>
        private Dictionary<EntityType, List<TrackTarget>> InputForTrackTargets = new Dictionary<EntityType, List<TrackTarget>>();
        private Dictionary<EntityType, List<TrackTarget>> OutputFromTrackTargets = new Dictionary<EntityType, List<TrackTarget>>();
        private Dictionary<EntityType, List<TrackTarget>> ToolForTrackTargets = new Dictionary<EntityType, List<TrackTarget>>();

        private bool includeSalvageProcesses;
        public bool IncludeSalvageProcesses
        {
            get
            {
                return includeSalvageProcesses;
            }
            set
            {
                if (value != includeSalvageProcesses)
                {
                    includeSalvageProcesses = value;

                    // update tracking results:
                    foreach (var item in TrackedTargets)
                    {
                        item.Value.RecomputeRelatedEntityTypes();
                    }

                    RecomputeTrackedSuperSets();
                }
            }
        }

        #region cached sets

        /// <summary>
        /// all blueprints
        /// </summary>
        HashSet<EntityType> baseData;

        /// <summary>
        /// changes with filters
        /// </summary>
        HashSet<EntityType> staticData;

        /// <summary>
        /// changes with game state (inventory)
        /// </summary>
        //Dictionary<string, EntityType> gameStateDependentData = new Dictionary<string,EntityType>();
        HashSet<EntityType> gameStateDependentData = new HashSet<EntityType>();

        #endregion


      /*  Dictionary<EntityType, AttainableInfo> attainableInfo = new Dictionary<EntityType, AttainableInfo>();
        Dictionary<EntityType, AttainableInfo> attainableInfoInProgress = new Dictionary<EntityType, AttainableInfo>();
        */

        HashSet<EntityType> isOwnedOrTradable = new HashSet<EntityType>();
        Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfo = new Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>>();
        Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>> attainableInfoInProgress = new Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>>();


        /// <summary>
        /// this is used to break infinite recursion loops when evaluating the production of items
        /// </summary>
        HashSet<EntityType> beingEvaluated = new HashSet<EntityType>();

       // Dictionary<SkillType, bool> availableSkillsInOwnerExpedition = new Dictionary<SkillType, bool>();


        public event Action TrackTargetsChanged;


        public static bool FilterProductionManagerItems(EntityType entityType)
        {
           
            if (entityType.TerrainType == null
                        && entityType.TreeType == null
                        && entityType.BiologicalType == null
                        && (/*(entityType.StructureType != null && entityType.StructureType.UsesAnchor == false) */
                           entityType.StructureType != null || entityType.ItemType != null))
            {

                return true;
            }

            return false;
        }
         
        public static HashSet<EntityType> GetBaseData(ref HashSet<EntityType> cachedBaseData)
        {
            if (cachedBaseData == null)
            {
                cachedBaseData = new HashSet<EntityType>(); // new Dictionary<string, EntityType>();
                foreach (var item in GameData.Instance.AllEntityTypes)
                {
                    // only show production related types?
                    if (FilterProductionManagerItems(item.Value))
                    {
                        cachedBaseData.Add(item.Value);
                    }
                }

            }

            return cachedBaseData;
        }

        public HashSet<EntityType> GetData(Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
        {
            // only recompute data when the filters change
            // except if there are active filters which depend on the curent state, like for instance the items the player has
            // the data sets that don't depend on the curent state such as current inventory can be cached. 
            // apply the availability filter on every refresh.

            settingsAreDirty = settingsAreDirty || FilterPropertySettings.SettingsAreDirty;

            if (settingsAreDirty)
            {              
                HashSet<EntityType> listOfTracked;
                HashSet<EntityType> listOfFiltered;

                listOfFiltered = GetFilteredEntities();

                listOfTracked = GetTrackedEntities();

                //union or interect the two collections(tracked and filtered) according to the selected radiobutton(intersect or union)
                if (AndOrSetting == AndOr.Or)
                {
                    staticData = listOfTracked.Union(listOfFiltered).ToHashSet();
                }
                else
                {
                    if (listOfTracked.Count == 0)
                    {
                        staticData = listOfFiltered;
                    }
                    else
                    {
                        staticData = listOfTracked.Intersect(listOfFiltered).ToHashSet();
                    }
                }
                                
                settingsAreDirty = false;
                FilterPropertySettings.SetSettingsNotDirty();
            }

            // this step depends on the current state of the game, so needs to be performed every time:
            return GetGameStateDependentData(allAvailableItems);

        }


       

        public TrackTarget GetFirstTrackedTarget()
        {
            if (TrackedTargets.Count > 0)
            {
                var e = TrackedTargets.GetEnumerator();
                e.MoveNext();
                return e.Current.Value;
            }

            return null;
        }

        private HashSet<EntityType> GetTrackedEntities()
        {
            List<HashSet<EntityType>> results = new List<HashSet<EntityType>>();
            HashSet<EntityType> listOfTracked = new HashSet<EntityType>();


            //get the entity-collection that fit for the tracking(s)
           // results = new List<HashSet<EntityType>>();
            if (TrackedTargets != null)
            {
                // Lars: why not use the supersets InputForTrackTargets etc. here? probably because the result depends on and/or.
                foreach (var item in TrackedTargets)
                {
                    listOfTracked = new HashSet<EntityType>();

                    //add entites that are in the tracked entity's input - output- tool list
                    if (item.Value.ShowInputs)
                    {
                      /*  var helper = item.Value.InputForTrackTarget.ToDictionary(x => x.KeyName, x => x);
                        listOfTracked = helper.Union(listOfTracked).ToDictionary(x => x.Key, x => x.Value);*/

                        listOfTracked = item.Value.InputForTrackTarget.Union(listOfTracked).ToHashSet();
                    }
                    if (item.Value.ShowOutputs)
                    {
                     /*   var helper = item.Value.OutputForTrackTarget.ToDictionary(x => x.KeyName, x => x);
                        listOfTracked = helper.Union(listOfTracked).ToDictionary(x => x.Key, x => x.Value);*/

                        listOfTracked = item.Value.OutputForTrackTarget.Union(listOfTracked).ToHashSet();
                    }
                    if (item.Value.ShowTools)
                    {
                       /* var helper = item.Value.ToolsForTrackTarget.ToDictionary(x => x.KeyName, x => x);
                        listOfTracked = helper.Union(listOfTracked).ToDictionary(x => x.Key, x => x.Value);*/

                        listOfTracked = item.Value.ToolsForTrackTarget.Union(listOfTracked).ToHashSet();
                    }

                    //add the tracked entity itself
                    if (!listOfTracked.Contains(item.Value.EntityType))
                    {
                        listOfTracked.Add(item.Value.EntityType);
                    }

                   /* EntityType placeholder;
                    if (!listOfTracked.TryGetValue(item.Value.EntityType.KeyName, out placeholder))
                    {
                        listOfTracked.Add(item.Value.EntityType.KeyName, item.Value.EntityType);
                    }*/


                    results.Add(listOfTracked);
                }

                listOfTracked = new HashSet<EntityType>();
                if (results.Count > 0)
                {
                    if (AndOrSetting == AndOr.Or)
                    {
                        listOfTracked = results.Aggregate((previousList, nextList) => previousList.Union(nextList).ToHashSet());
                    }
                    else
                    {
                        listOfTracked = results.Aggregate((previousList, nextList) => previousList.Intersect(nextList).ToHashSet());
                    }
                }
            }

            return listOfTracked;
        }

        private void RecomputeTrackedSuperSets()
        {
            InputForTrackTargets.Clear();
            OutputFromTrackTargets.Clear();
            ToolForTrackTargets.Clear();

            foreach (var item in TrackedTargets)
            {
                AddTrackedTargetToSupersets(item.Value);
            }

        }

        private HashSet<EntityType> GetFilteredEntities()
        {
            
            HashSet<EntityType> listOfFiltered = new HashSet<EntityType>();

            //get the entity-collection that fit for the filter(s)
            if (FilterPropertySettings.HasActiveFilters())
            {
                staticData = new HashSet<EntityType>(); // new Dictionary<string, EntityType>();

                List<HashSet<EntityType>> results = FilterPropertySettings.GetFilteredEntities(FilterProductionManagerItems);


                //create one dictionary from the result dictionary list 
                //if OR we union them if AND we intersect them
                if (results.Count > 0)
                {
                    if (AndOrSetting == AndOr.Or)
                    {
                        listOfFiltered = results.Aggregate((previousList, nextList) => previousList.Union(nextList).ToHashSet());
                    }
                    else
                    {
                        listOfFiltered = results.Aggregate((previousList, nextList) => previousList.Intersect(nextList).ToHashSet());
                    }
                }
            }
            else
            {
                listOfFiltered = GetBaseData(ref baseData);
            }

            return listOfFiltered;
        }

     /*   private Dictionary<string, EntityType> GetFilteredEntities()
        {
            List<Dictionary<string, EntityType>> results = new List<Dictionary<string, EntityType>>();
            Dictionary<string, EntityType> listOfFiltered = new Dictionary<string, EntityType>();

            //get the entity-collection that fit for the filter(s)
            if (ActiveFilterSettings.Count > 0)
            {
                staticData = new HashSet<EntityType>(); // new Dictionary<string, EntityType>();

                // add results from other filters, make a method for each.
                foreach (var item in ActiveFilterSettings)
                {
                    results.Add(item.Value.GetData());
                }

                //create one dictionary from the result dictionary list 
                //if OR we union them if AND we intersect them
                if (results.Count > 0)
                {
                    if (AndOrSetting == AndOr.Or)
                    {
                        listOfFiltered = results.Aggregate((previousList, nextList) => previousList.Union(nextList).ToDictionary(x => x.Key, x => x.Value));
                    }
                    else
                    {
                        listOfFiltered = results.Aggregate((previousList, nextList) => previousList.Intersect(nextList).ToDictionary(x => x.Key, x => x.Value));
                    }
                }
            }
            else
            {
                listOfFiltered = GetBaseData(); 
            }

            return listOfFiltered;
        }*/

        private HashSet<EntityType> GetGameStateDependentData(Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
        {

            EntityGroup owner = UWGame.SimSide.Snapshots.LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
            Expedition expedition = The.InGameUI.GetExpedition();


            if (expedition == null)
                return staticData;

            // copy the data:        
            gameStateDependentData.Clear();

           // attainableInfo.Clear();
            //attainableFromSalvageInfo.Clear();

            //availableSkillsInOwnerExpedition.Clear();


            foreach (var entityType in staticData)
            {
             
               // int currentOrder = 0;
                int noOfIncompleteItems, noOfEntitiesUsedAsParts, noOfItemsOffSite, noOfItemsOwnedByOthers, noOfAvailableItemsIncludingIntrinsic;

                int noOfAvailableItems = InventoryPanel.GetNoOfAvailableEntities(owner.AllEntities, owner, entityType, out noOfIncompleteItems,
                                                                                 out noOfEntitiesUsedAsParts, out noOfItemsOffSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic,
                                                                                 allAvailableItems);
                ProductionOrder sTarget;
                bool ordersExist = false;
                if (entityType.ItemType != null)
                {
                    ordersExist = owner.ProductionOrders.OrdersExist(entityType);
                }
                
                bool canBuildNow = InventoryPanel.CanBuildNow(entityType, owner, includeSalvageProcesses);

                bool isAvailableNow = false;

                if (noOfAvailableItems > 0
                 || noOfEntitiesUsedAsParts > 0 // not really available if there is no salvage process...
                 || noOfIncompleteItems > 0
                 || noOfItemsOffSite > 0
                 || ordersExist // currentOrder > 0 
                 || canBuildNow) 
                {
                    isAvailableNow = true;
                }

               /* AttainableInfo info;
                attainableInfo.TryGetValue(entityType, out info);
                */
                Dictionary<ProcessType, AttainableInfo> info;
                attainableInfo.TryGetValue(entityType, out info);
               

                bool keepEntityType = false;
                
                switch (AvailabilitySettings)
                {
                    case Availability.AvailableNow:

                       // GetDistanceToRoot(entityType, allAvailableItems); 

                        keepEntityType = isAvailableNow;

                        break;

                    case Availability.Attainable:
                        // all the above
                     
                        if (isAvailableNow || (info != null && info.Any(a => a.Value.IsAttainable))) // info.IsAttainable)) 
                        {
                            keepEntityType = true;
                        }


                        break;
                    case Availability.AllKnownBlueprints:
                       // GetDistanceToRoot(entityType, allAvailableItems);
                        keepEntityType = true;

                        break;
                }

                if (keepEntityType)
                {
                    gameStateDependentData.Add(entityType);
                }
            }

            return gameStateDependentData;
        }


        /*

        private HashSet<EntityType> GetGameStateDependentData(Dictionary<EntityType, int> allAvailableItems)
        {
                      
            EntityGroup owner = UWGame.SimSide.Snapshots.LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
            Expedition expedition = The.InGameUI.GetExpedition();


            if (expedition == null)
                return staticData;

            // copy the data:        
            gameStateDependentData.Clear();

            attainableInfo.Clear();
            //attainableFromSalvageInfo.Clear();

            availableSkillsInOwnerExpedition.Clear();
           

            foreach (var entityType in staticData)
	        {
                if (entityType.Name.Contains("Firebricks"))
                { 
                }
                int currentOrder = 0;
                int noOfIncompleteItems, noOfEntitiesUsedAsParts;

                int noOfAvailableItems = InventoryPanel.GetNoOfAvailableEntities(owner, entityType, out noOfIncompleteItems,
                                                                                 out noOfEntitiesUsedAsParts, allAvailableItems);
                StocksTarget sTarget;
                if (entityType.ItemType != null)
                {
                    if (expedition.Stocks.Targets.TryGetValue(entityType,out sTarget))
                    {
                        currentOrder = expedition.Stocks.Targets[entityType].ProductionTarget;
                    }
                }



                bool canBuildNow = InventoryPanel.CanBuildNow(entityType, owner);
               
                bool isAvailableNow = false; 

                if (noOfAvailableItems > 0
                 || noOfEntitiesUsedAsParts > 0 // not really available if there is no salvage process...
                 || noOfIncompleteItems > 0
                 || currentOrder > 0
                 || canBuildNow) // buildAvailability == ProductionAvailability.CanBuildNow)
                {
                    isAvailableNow = true;
                }


                bool keepEntityType = false;
                bool isAttainable = false;

                switch (AvailabilitySettings)
                {
                    case Availability.AvailableNow:

                        GetDistanceToRoot(entityType, allAvailableItems); // if we don't want attainability info on this setting, this line should be commented out.

                        keepEntityType = isAvailableNow;
                      
                        break;

                    case Availability.Attainable:
                        // all the above
                        isAttainable = GetDistanceToRoot(entityType, allAvailableItems) >= 0;
                        if (isAvailableNow || isAttainable)
                        {
                            keepEntityType = true;
                        }

                       
                        break;
                    case Availability.AllKnownBlueprints:
                        GetDistanceToRoot(entityType, allAvailableItems);
                        keepEntityType = true;                      

                        break;
                }

                if (keepEntityType)
                {
                    gameStateDependentData.Add(entityType);
                }
            }

            return gameStateDependentData;
        }
        */

       /* private bool HasSkill(SkillType skillType, EntityGroup owner)
        {
            bool hasSkill;
            if (!availableSkillsInOwnerExpedition.TryGetValue(skillType, out hasSkill))
            {
                hasSkill = owner.GetExpedition().HasSkill(skillType);
                availableSkillsInOwnerExpedition.Add(skillType, hasSkill);
            }

            return hasSkill;
        }*/

        public void Update(GameTime gameTime)
        {
            if (processesToDo == null
                && attainabilityRegulator.IsReady())
            {
                StartRecomputeAttainability();
            }

            if (processesToDo != null)
            {
                if (CycleRecomputeAttainability())
                {
                    EndRecomputeAttainability();
                }
            }
        }

       

        const int noOfProcessesToComputePerFrame = 50;


        private bool CycleRecomputeAttainability()
        {
            // NEW: compute attainability of owned items too

            /*
            * Brute force algo:
            * 
          Foreach entity you own
             Mark it attainable

           todoList = all processes;

           bool needToLoop = true;
           While (needToLoop)
             needToLoop = false;
             Foreach process in todoList
               If the process is valid (all inputs and tools are attainable, or any other condition checks you want)
                 Remove process from todoList
                 Foreach output in process
                   Mark it producable
                   If it is not yet marked attainable
                     needToLoop = true;
                     Mark it attainable
            */

            int noOfProcessesHandled = 0;
            EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);

            bool needToLoop = true;
            while (needToLoop)
            {
                if (noOfProcessesHandled > noOfProcessesToComputePerFrame)
                {
                    // continue next frame...
                    return false;
                }

                needToLoop = false;

                for (int i = processesToDo.Count - 1; i >= 0; i--)
                {
                    ProcessType process = processesToDo[i];

                  
                    if (ProcessCanProduce(process, AllAvailableItems, owner, false))
                    {
                        processesToDo.RemoveAt(i);

                        foreach (var item in process.Outputs)
                        {
                            // mark outputs as attainable.

                            if (!item.IsWasteProduct)
                            {

                               Dictionary<ProcessType, AttainableInfo> attainableInfos;
                               if (!attainableInfoInProgress.TryGetValue(item.FinalEntityTypeToCreate, out attainableInfos))
                               {
                                   attainableInfos = new Dictionary<ProcessType, AttainableInfo>();
                                   attainableInfoInProgress.Add(item.FinalEntityTypeToCreate, attainableInfos);
                                   needToLoop = true;
                               }

                                AttainableInfo info;
                                if (!attainableInfos.TryGetValue(process, out info))
                                {
                                    info = new AttainableInfo(1);
                                    attainableInfos.Add(process, info);
                                   // needToLoop = true;
                                }



                                info.IsProducable = true;
                               // info.NoProcess = false;  


                                /*
                                AttainableInfo info;
                                if (!IsAttainable(item.FinalEntityTypeToCreate, out info)) 
                                {                                    
                                    needToLoop = true;
                                    attainableInfoInProgress[item.FinalEntityTypeToCreate] = new AttainableInfo(1) { IsProducable = true };
                                }
                                else if (info.IsProducable == false)
                                { 
                                    // if it is owned, mark it as attainable/producable also                                  
                                    info.IsProducable = true;
                                    info.NoProcess = false;  
                                    
                                    // no need to loop again.
                                }*/
                            }
                            
                        }
                    }

                    noOfProcessesHandled++;

                }
            }

            return true; // done
        }


        //private bool CycleRecomputeAttainability()
        //{
        //    /*
        //    * Brute force algo:
        //    * 
        //  Foreach entity you own
        //     Mark it attainable

        //   todoList = all processes;

        //   bool needToLoop = true;
        //   While (needToLoop)
        //     needToLoop = false;
        //     Foreach process in todoList
        //       If the process is valid (all inputs and tools are attainable, or any other condition checks you want)
        //         Remove process from todoList
        //         Foreach output in process
        //           If it is not yet marked attainable
        //             needToLoop = true;
        //             Mark it attainable
        //    */

        //    int noOfProcessesHandled = 0;
        //    EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);

        //    bool needToLoop = true;
        //    while (needToLoop)
        //    {
        //        if (noOfProcessesHandled > noOfProcessesToComputePerFrame)
        //        {
        //            // continue next frame...
        //            return false;
        //        }

        //        needToLoop = false;               

        //        for (int i = processesToDo.Count - 1; i >= 0; i--)
        //        {
        //            ProcessType process = processesToDo[i];

        //            if (ProcessCanProduce(process, allAvailableItems, owner, false))
        //            {
        //                processesToDo.RemoveAt(i);

        //                foreach (var item in process.Outputs)
        //                {
        //                    if (!item.IsWasteProduct 
        //                        && !IsAttainable(item.FinalEntityTypeToCreate))
        //                    {
        //                        needToLoop = true;
        //                        attainableInfoInProgress[item.FinalEntityTypeToCreate] = new AttainableInfo(1) { };
        //                    }
        //                }
        //            }

        //            noOfProcessesHandled++;
                    
        //        }
        //    }

        //    return true; // done
        //}

        private void EndRecomputeAttainability()
        {
            EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);

            // If you want to fill out hint information with missing inputs, you can do this at the end by using whatever's left in the todoList 
            // - those are the processes that the player can't make. 
            foreach (var process in processesToDo)
            {               
                ProcessCanProduce(process, AllAvailableItems, owner, true);
            }

            // make a shallow copy: (??)
           // attainableInfo = new Dictionary<EntityType, AttainableInfo>(attainableInfoInProgress);

            //attainableInfo = new Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>>(attainableInfoInProgress);
            attainableInfo = new Dictionary<EntityType, Dictionary<ProcessType, AttainableInfo>>();
            foreach (var item in attainableInfoInProgress)
            {
                attainableInfo.Add(item.Key, new Dictionary<ProcessType, AttainableInfo>(item.Value));
            }

            attainableInfoInProgress.Clear();
            isOwnedOrTradable.Clear();
 
            processesToDo = null;
        }

        private void StartRecomputeAttainability()
        {            
               /*
            * Brute force algo:
            * 
              Foreach entity you own
                 Mark it attainable
                * */

            // init all entity types from FilterProductionManagerItems?

            EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);

            // we always do a full recompute. it does not matter what the panel filter settings are. If the salvage button is clicked while computing... not sure it matters that much.
            AllAvailableItems.Clear();
            InventoryPanel.CountAllEntities(owner, this.AllAvailableItems, true);

            attainableInfoInProgress.Clear();
            //availableSkillsInOwnerExpedition.Clear();
            isOwnedOrTradable.Clear();

            foreach (var item in owner.AllEntities)
            {
                InventoryPanel.Availability availability = AllAvailableItems[item.Key];

              
                if (availability.NoOfAvailableItems > 0 || availability.AvailableToBuy > 0)
                {
                   // bool isOwned = availability.NoOfAvailableItems > 0;
                    this.isOwnedOrTradable.Add(item.Key);

                    /*
                    attainableInfoInProgress[item.Key] = new AttainableInfo(0) 
                    { 
                        IsOwned = isOwnedOrTradable, 
                        NoProcess = true // may get updated later
                    }; */
                }
            }


            // copy lists:
            if (includeSalvageProcesses)
            {
                processesToDo = new List<ProcessType>(GameData.Instance.AllProductionProcesses);
            }
            else
            {
                processesToDo = new List<ProcessType>(GameData.Instance.NonSalvageProductionProcesses);
            }
        }

      /*  private bool IsAttainable(EntityType entityType, out AttainableInfo info) 
        {          

            if (attainableInfoInProgress.TryGetValue(entityType, out info))
            {             
                return info.IsAttainable;    
            }

            return false;
        }*/

        private bool IsAttainable(EntityType entityType)
        {
            // new - split into two collections
            if (isOwnedOrTradable.Contains(entityType))
                return true;

            Dictionary<ProcessType, AttainableInfo> infos;
            if (attainableInfoInProgress.TryGetValue(entityType, out infos))
            {
                return infos.Any(a => a.Value.IsAttainable);
            }

            return false;


            /*
            AttainableInfo info;
            if (attainableInfoInProgress.TryGetValue(entityType, out info))
            {
                return info.IsAttainable; 
            }

            return false;*/
        }

        private bool ProcessCanProduce(ProcessType process, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems, EntityGroup owner, bool gatherMissingInfo)
        {
            ResourceType missingResource = null;
            TierOrAreaType missingPolicy = null;
            SkillType missingSkill = null;
            EntityType missingSpecialSite = null;
            List<EntityType> missingInputs = null;
            List<EntityType> missingTools = null;
           

            SkillType skillType = process.RequiredSkillType;
            bool hasSkills = owner.GetExpedition().HasSkill(skillType); // HasSkill(skillType, owner);
            if (!hasSkills)
            {
                if (gatherMissingInfo)
                {
                    // here, we store feedback like "skill XXX needed"
                    missingSkill = process.RequiredSkillType;
                }
                else return false;
                
            }

            if (process.ResourceTypeInput != null)
            {
                if (!HasResources(process))
                {
                    if (gatherMissingInfo)
                    {
                        missingResource = process.ResourceTypeInput;
                    }
                    else return false;
                }
            }

            Expedition expedition = owner.Parent as Expedition;
            TierOrAreaType policy;
            if (expedition != null && !expedition.Policy.CanUseProcess(process, out policy))
            {
                if (gatherMissingInfo)
                {
                    missingPolicy = policy;
                }
                else return false;
            }

            if (process.ActingOnType != null)
            {
                if (!HasSpecialSite(process))
                {
                    if (gatherMissingInfo)
                    {
                        missingSpecialSite = process.ActingOnType;
                    }
                    else return false;
                }
            }

            // trace inputs:
            if (process.InputsByType != null)
            {
                foreach (var input in process.InputsByType)
                {
                    if (input.Key.TreeType != null)
                    {
                        continue; // tree crops list the tree as input... just ignore it.
                    }

                    if (!IsAttainable(input.Key))
                    {
                        if (gatherMissingInfo)
                        {
                            Common.AddToList(ref missingInputs, input.Key);
                            break;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

            }


            if (process.ProcessToolSet != null)
            {
                foreach (var toolAlternative in process.ProcessToolSet.Tools)
                {
                  //  AttainableInfo toolInfo = null;
                    bool foundTool = false;
                    foreach (var tool in toolAlternative.ToolsAndProductivity)
                    {
                        EntityType toolEntityType = tool.Item1;

                        if (IsAttainable(toolEntityType))
                        {
                            foundTool = true;
                            break; // one tool is enough to trace. we are done with this alternative.
                        }                       
                    }

                    if (!foundTool)
                    {
                        if (gatherMissingInfo)
                        {
                            // store one set of tools for feedback:
                            foreach (var tool in toolAlternative.ToolsAndProductivity)
                            {
                                Common.AddToList(ref missingTools, tool.Item1);
                            }

                            break; // failed to trace this process to the root       
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }



            if (missingResource == null && missingSkill == null && missingInputs == null && missingTools == null && missingSpecialSite == null && missingPolicy == null)
            {
                return true;
            }
            else
            {
                if (gatherMissingInfo)
                {
                   
                    foreach (var item in process.Outputs)
                    {
                        if (!item.IsWasteProduct) // make this an option??
                             
                        {
                          
                            AttainableInfo info;
                            Dictionary<ProcessType, AttainableInfo> infos;
                            if (!attainableInfoInProgress.TryGetValue(item.FinalEntityTypeToCreate, out infos))
                            {
                                infos = new Dictionary<ProcessType, AttainableInfo>();
                                attainableInfoInProgress[item.FinalEntityTypeToCreate] = infos;
                            }

                          /*  else // with salvage, the info will be set many many times for all the different salvage processes
                            {
                                info.UnavailableResource = missingResource;
                                info.UnavailableSkill = missingSkill;
                                info.UnavailableInputs = missingInputs;
                                info.UnavailableTools = missingTools;
                                info.NoProcess = false;
                                //info.ProcessCount = info.ProcessCount + 1;
                            }*/

                            if (!infos.TryGetValue(process, out info))
                            {
                                info = new AttainableInfo(-1);
                                infos.Add(process, info);
                            }

                            info.UnavailableResource = missingResource;
                            info.UnavailablePolicy = missingPolicy;
                            info.UnavailableSkill = missingSkill;
                            info.UnavailableInputs = missingInputs;
                            info.UnavailableTools = missingTools;
                            info.UnavailableSpecialSite = missingSpecialSite;

                           // info.NoProcess = false;
                        }
                    }

                    /*
                     * AttainableInfo info = new AttainableInfo(-1)
                    {
                        UnavailableResource = missingResource,
                        UnavailableSkill = missingSkill,
                        UnavailableInputs = missingInputs,
                        UnavailableTools = missingTools,
                        //  NoProcess = noProcess
                    };
                     * 
                    foreach (var item in process.Outputs)
                    {
                        if (!item.IsWasteProduct // make this an option??
                             && !attainableInfoInProgress.ContainsKey(item.FinalEntityTypeToCreate)) 
                         {
                             attainableInfoInProgress[item.FinalEntityTypeToCreate] = info;
                         }
                    }
                    */
                }

                return false;
            }
        }

     


     /*   public AttainableInfo GetAttainableInfo(EntityType entityType)
        {           
            // creates on demand attainability info. alternatively we need to create info for every entity type in the game... or at least the ones that can appear in inventorypanel...
            AttainableInfo info;
            if (!attainableInfo.TryGetValue(entityType, out info))
            {
                info = new AttainableInfo(-1) { NoProcess = true }; // delete this??? is confusing
                attainableInfo.Add(entityType, info);
            }

            return info;

           
        }*/


        /// <summary>
        /// Main way to get attainability info. Remember to make sure that InventorySettings gets an Update call
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public Dictionary<ProcessType, AttainableInfo> GetAttainableInfo(EntityType entityType)
        {
            // creates on demand attainability info. alternatively we need to create info for every entity type in the game... or at least the ones that can appear in inventorypanel...
            Dictionary<ProcessType, AttainableInfo> info;
            if (!attainableInfo.TryGetValue(entityType, out info))
            {   
                /*
                info = new AttainableInfo(-1) { NoProcess = true }; // delete this??? is confusing
                attainableInfo.Add(entityType, info);*/
            }

            return info;
        }

        public static bool HasSpecialSite(ProcessType process)
        {
            List<EntityID> entities;
            if (process.ActingOnType.IntelligenceType == null)
            {
                if (The.InGameUI.UIAllegiance.SharedKnowledge.AllKnownEntities.AllEntities.TryGetValue(process.ActingOnType, out entities)
                       && entities.Count > 0)
                {
                    return true;
                }
            }
            else
            {
                // need to look up members (sentry) in another collection:
                if (The.InGameUI.UIAllegiance.Members.Any(m => m.EntityType == process.ActingOnType))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasResources(ProcessType process)
        {
            HashSet<ResourceID> resources;
            if (The.InGameUI.UIAllegiance.SharedKnowledge.PlaySiteKnowledge.AllKnownResourceContainers.TryGetValue(process.ResourceTypeInput, out resources)
                   && resources.Count > 0
                   && resources.Any(r => HasResourceItems(r)))
            {
                return true;
            }

            return false;
        }

        private static bool HasResourceItems(ResourceID resourceID)
        {
            ResourceContainer resource = LookUp<ResourceContainer, ResourceID>.FindByID(resourceID);

            if (resource.NoOfHarvestableItems > 0)
                return true;

            return false;
        }

     

        public InventorySettings()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                FilterPropertySettings = new FilterPropertySettings();
                SortingSettings = new SortingSettings<SortColumns>(SortColumns.Name, SortingSettings<SortColumns>.DefaultSortOrder);
            }

            AvailableColors = GameData.Instance.GUIConstants.TrackingColors.ToList();
        }

        public bool GetTrackedColorAndTooltip(EntityType entityType, out Color? color, out string toolTip)
        {
            toolTip = null;
            color = null;
            bool isTrackTarget = false;

           
            if (entityType != null)
            {               

                bool isTrackedInput, isTrackedOutput, isTrackedTool;
                List<TrackTarget> inputForTrackedEntityTypes, outputFromTrackedEntityTypes, toolForTrackedEntityTypes;


                IsTracked(entityType,
                            out isTrackTarget,
                            out isTrackedInput,
                            out isTrackedOutput,
                            out isTrackedTool,
                            out inputForTrackedEntityTypes,
                            out outputFromTrackedEntityTypes,
                            out toolForTrackedEntityTypes);

                color = GetRowColorAndToolTip(inputForTrackedEntityTypes, outputFromTrackedEntityTypes, toolForTrackedEntityTypes, out toolTip);

                if (isTrackTarget)
                {
                    TrackTarget trackedEntityType;
                    TrackedTargets.TryGetValue(entityType, out trackedEntityType);

                    color = trackedEntityType.Color;
                    toolTip = barTooltipBeingTracked + (toolTip == null ? null : " \n" + toolTip);
                }

                if (color == null)
                {
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }


        public static Color? GetRowColorAndToolTip(List<TrackTarget> inputForTrackedEntityTypes, List<TrackTarget> outputFromTrackedEntityTypes, List<TrackTarget> toolForTrackedEntityTypes, out string toolTip)
        {
            Color? barColor = null;
            int noOfColorChanges = 0;
            toolTip = "";
            string toolTipPart = "";

            if (inputForTrackedEntityTypes != null)
            {
                barColor = GetColorFromCollections(InOutOrTool.Input, inputForTrackedEntityTypes, barColor, noOfColorChanges, out noOfColorChanges, toolTipPart, out toolTipPart);
                toolTip = toolTip + toolTipPart;
                toolTipPart = "";
            }

            if (outputFromTrackedEntityTypes != null)
            {
                barColor = GetColorFromCollections(InOutOrTool.Output, outputFromTrackedEntityTypes, barColor, noOfColorChanges, out  noOfColorChanges, toolTipPart, out toolTipPart);
                toolTip = toolTip + toolTipPart;
                toolTipPart = "";
            }

            if (toolForTrackedEntityTypes != null)
            {
                barColor = GetColorFromCollections(InOutOrTool.Tool, toolForTrackedEntityTypes, barColor, noOfColorChanges, out  noOfColorChanges, toolTipPart, out toolTipPart);
                toolTip = toolTip + toolTipPart;
                toolTipPart = "";
            }

            toolTip = toolTip != "" ? "This item is:" + toolTip : null;

            return barColor;
        }

        public enum InOutOrTool { Input, Output, Tool }

        private static Color? GetColorFromCollections(InOutOrTool status, List<TrackTarget> trackTargets, Color? color, int startAmount, out int noOfColorChanges, string toolTip, out string modifiedToolTip)
        {
            noOfColorChanges = startAmount;

            modifiedToolTip = toolTip;

            foreach (var input in trackTargets)
            {
                bool shouldBeColored = false;

                switch (status)
                {
                    case InOutOrTool.Input:
                        shouldBeColored = input.ShowInputs;
                        break;
                    case InOutOrTool.Output:
                        shouldBeColored = input.ShowOutputs;
                        break;

                    case InOutOrTool.Tool:
                        shouldBeColored = input.ShowTools;
                        break;
                }

                if (shouldBeColored)
                {
                    if (color != input.Color)
                    {
                        color = input.Color;
                        noOfColorChanges++;
                    }
                    if (noOfColorChanges > 1)
                    {
                        color = Color.White;
                    }
                    modifiedToolTip = modifiedToolTip + "\n" + " - " + input.EntityType.Name;
                }

            }

            if (trackTargets.Count > 0 && modifiedToolTip != toolTip)
            {
                switch (status)
                {
                    case InOutOrTool.Input:
                        modifiedToolTip = "\n Input for:\n" + modifiedToolTip;
                        break;
                    case InOutOrTool.Output:
                        modifiedToolTip = "\n Output from:\n" + modifiedToolTip;
                        break;
                    case InOutOrTool.Tool:
                        modifiedToolTip = "\n Tool for making:\n" + modifiedToolTip;
                        break;
                }
            }

            return color;
        }


        public bool IsTracked(EntityType entityType,
            out bool isTrackTarget,
            out bool isTrackedInput,
            out bool isTrackedOutput,
            out bool isTrackedTool,
            out List<TrackTarget> inputForTrackedEntityTypes,
            out List<TrackTarget> outputFromTrackedEntityTypes,
            out List<TrackTarget> toolForTrackedEntityTypes)
        {
            TrackTarget trackTarget;
            TrackedTargets.TryGetValue(entityType, out trackTarget);

            isTrackTarget = trackTarget == null ? false : true;
            isTrackedInput = InputForTrackTargets.TryGetValue(entityType, out inputForTrackedEntityTypes);
            isTrackedOutput = OutputFromTrackTargets.TryGetValue(entityType, out outputFromTrackedEntityTypes);
            isTrackedTool = ToolForTrackTargets.TryGetValue(entityType, out toolForTrackedEntityTypes);

            return isTrackTarget;
        }


        public bool HasAvailableTrackingSlots()
        {
            return AvailableColors.Count > 0;
        }

      

        public void StartTracking(EntityType entityType)
        {
            TrackTarget trackedEntityType = new TrackTarget(entityType, false, false, false);

            Color assignedColor = AvailableColors[0];
            trackedEntityType.Color = assignedColor;
            AvailableColors.Remove(assignedColor);

            TrackedTargets.Add(entityType, trackedEntityType);

            AddTrackTarget(trackedEntityType);
        }

        private void AddTrackTarget(TrackTarget trackedEntityType)
        {
            // add to lists
            AddTrackedTargetToSupersets(trackedEntityType);

            if (TrackTargetsChanged != null)
            {
                TrackTargetsChanged.Invoke();
            }
        }

        private void AddTrackedTargetToSupersets(TrackTarget trackedEntityType)
        {
            if (trackedEntityType.InputForTrackTarget != null)
            {
                foreach (var item in trackedEntityType.InputForTrackTarget)
                {
                    Common.AddToMultiList(InputForTrackTargets, item, trackedEntityType);
                }
            }

            if (trackedEntityType.OutputForTrackTarget != null)
            {
                foreach (var item in trackedEntityType.OutputForTrackTarget)
                {
                    Common.AddToMultiList(OutputFromTrackTargets, item, trackedEntityType);
                }
            }
            if (trackedEntityType.ToolsForTrackTarget != null)
            {
                foreach (var item in trackedEntityType.ToolsForTrackTarget)
                {
                    Common.AddToMultiList(ToolForTrackTargets, item, trackedEntityType);
                }
            }
        }


        public bool ToggleTracking(EntityType entityType)
        {
            settingsAreDirty = true;
            TrackTarget trackedEntityType;
            if (!TrackedTargets.TryGetValue(entityType, out trackedEntityType))
            {
                if (HasAvailableTrackingSlots())
                {
                    StartTracking(entityType);

                    //  The.InGameUI.InventoryPanel.PopulateEntityTrackingCombo();

                    return true;
                    // tb.ToolTip = InventoryPanel.tbTrackBeingTrackedTooltip;
                }
            }
            else
            {
                StopTracking(trackedEntityType);

                // tb.ToolTip = InventoryPanel.tbTrackNotTrackedTooltip;
                //  The.InGameUI.InventoryPanel.PopulateEntityTrackingCombo();

                return false;
            }
           
            return false;

        }


        public void StopTracking(TrackTarget trackedEntityType)
        {
            AvailableColors.Add(trackedEntityType.Color);
            TrackedTargets.Remove(trackedEntityType.EntityType);

            // remove from lists
            if (trackedEntityType.InputForTrackTarget != null)
            {
                foreach (var item in trackedEntityType.InputForTrackTarget)
                {
                    Common.RemoveFromMultiList(InputForTrackTargets, item, trackedEntityType, true);
                }
            }
            if (trackedEntityType.OutputForTrackTarget != null)
            {
                foreach (var item in trackedEntityType.OutputForTrackTarget)
                {
                    Common.RemoveFromMultiList(OutputFromTrackTargets, item, trackedEntityType, true);
                }
            }
            if (trackedEntityType.ToolsForTrackTarget != null)
            {
                foreach (var item in trackedEntityType.ToolsForTrackTarget)
                {
                    Common.RemoveFromMultiList(ToolForTrackTargets, item, trackedEntityType, true);
                }
            }

            if (TrackTargetsChanged != null)
            {
                TrackTargetsChanged.Invoke();
            }
        }


        public void TrackTargetSettingsChanged()
        {
            settingsAreDirty = true;

            if (TrackTargetsChanged != null)
            {
                TrackTargetsChanged.Invoke();
            }
        }

       

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
           /* this.SortedBy = sn.DoEnum(SortedBy);
            this.SortOrder = sn.DoEnum(SortOrder);*/

            this.IsExpanded = sn.DoBool(IsExpanded);
            this.includeSalvageProcesses = sn.DoBool(includeSalvageProcesses);
            this.AvailabilitySettings = sn.DoEnum(AvailabilitySettings);

            this.TrackedTargets = sn.DoDictionary(TrackedTargets);
            this.AvailableColors = sn.DoList(AvailableColors);

            this.AndOrSetting = sn.DoEnum(AndOrSetting);

            settingsAreDirty = sn.DoBool(settingsAreDirty);
            staticData = sn.DoHashSet(staticData);

            this.FilterPropertySettings = (FilterPropertySettings)sn.DoISnapshot(FilterPropertySettings);
            this.SortingSettings = (SortingSettings<SortColumns>)sn.DoISnapshot(SortingSettings);

            sn.Ignore(gameStateDependentData);
            sn.Ignore(beingEvaluated);
            sn.Ignore(baseData);
            sn.Ignore(TrackTargetsChanged);
            //sn.Ignore(availableSkillsInOwnerExpedition);
            sn.Ignore(attainableInfo);
            sn.Ignore(attainableInfoInProgress);
            sn.Ignore(isOwnedOrTradable);
            sn.Ignore(processesToDo);
            sn.Ignore(AllAvailableItems);
            sn.Ignore(ToolForTrackTargets);
            sn.Ignore(InputForTrackTargets);
            sn.Ignore(OutputFromTrackTargets);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            FilterPropertySettings.LoadPostProcess(sn);
            SortingSettings.LoadPostProcess(sn);

            foreach (var item in TrackedTargets)
            {
                item.Value.LoadPostProcess(sn);

                AddTrackTarget(item.Value);
            }          

        }
        #endregion
    }
}
