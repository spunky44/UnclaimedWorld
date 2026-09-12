using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using System.Linq;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    class GoalHaul : CompositeGoal, ITopLevelGoal
    {
        public HaulingJob job;

        JobID? snapshotJob;

        public OwnerID? NewOwner;

        private EntityGroupID ownerOfJobsID;

        public double TimeSpentInTopLevelGoal { get; set; }

        /// <summary>
        /// only the outer goal will have this collection of additional hauling goals, waiting to be added and activated
        /// </summary>
        private Queue<GoalHaul> goalsToNest = new Queue<GoalHaul>();
        Queue<GoalID> snapshotGoalsToNest;

        /// <summary>
        /// nested goals that have been added, but may not have been activated yet - still needs to be cancelled because they have a lock on their job.
        /// </summary>
        private List<GoalHaul> addedNestedGoals = new List<GoalHaul>();
        List<GoalID> snapshotAddedNestedGoals;

        /// <summary>
        /// used when this goal is nested:
        /// </summary>
        public GoalHaul ParentGoal;
        GoalID? snapshotParent;

        private EntityID? vehicleID;

        /// <summary>
        /// this is used when unloading from a vehicle to a building:
        /// 
        /// TODO!
        /// </summary>
        public List<HaulingJob> DestinationPiledItemJobs;
        List<JobID> snapshotDestinationPiledItemJobs;

        /// <summary>
        /// this is used when loading a vehicle in order to load multiple items at a time from the same tile
        /// </summary>
        private List<Tuple<HaulingJob, IKnownEntityData>> SourcePiledItemJobs; // TODO: refactor to EntityID

        public bool DestinationPileIsHandled = false;
        public bool SourcePileIsHandled = false;

        private bool isFindingExtraJobs = false;

        /// <summary>
        /// this flag is used to decide if the vehicle can be freed after the goal terminates
        /// </summary>
        public bool IsOuterGoal = true;

        /// <summary>
        /// if this has been set from the outer hauling goal, we won't bend down to pick up...
        /// </summary>
        public bool IsStandingBentOverItem = false;

        private bool isCancelled = false;

        /* DEBUGGING */
        private EntityID itemToHaul;
        private bool isActivated = false;


        public GoalHaul(Entity entity, HaulingJob job, EntityID item, EntityID? vehicle, OwnerID? newOwner, List<EntityGroupID> ownersOfVehicles, EntityGroupID ownerOfJobs)
            : base(entity)
        {
            itemToHaul = item;

            this.job = job;

            if (job.ID == (JobID)3288)
            {

            }


            HaulingJobAnyItemOfType anyJob = job as HaulingJobAnyItemOfType;
            if (anyJob != null)
            {
                string logText = "";
                if (job.Item.HasValue)
                {
                    logText = "job item before: " + job.Item.Value.ToString() + ", now: ";
                }

                logText += item.ToString() + "was set by " + entity.ToString();

                anyJob.AddLog(logText);
            }

            this.job.Item = item;


            this.vehicleID = vehicle;
            this.ownersOfVehicles = ownersOfVehicles;
            NewOwner = newOwner;
            this.ownerOfJobsID = ownerOfJobs;


            /*   NewOwnership = newOwnership;
               NewBelongingCollection = newBelongingCollection;
               */

            if (vehicleID != null)
            {
                SourcePiledItemJobs = new List<Tuple<HaulingJob, IKnownEntityData>>(); // new List<HaulingJob>();
                DestinationPiledItemJobs = new List<HaulingJob>();
            }
        }

        public GoalHaul(Entity entity, HaulingJob job, EntityID item, EntityID? vehicle, GoalHaul parentGoal, bool isOuterGoal, OwnerID? newOwner, List<EntityGroupID> ownersOfVehicles)
            : base(entity)
        {
            itemToHaul = item;
                      

            this.job = job;

            if (job.ID == (JobID)3288)
            {

            }

#if !RELEASE
            if (job.Item == (EntityID)36896)
            {
                throw new Exception();
            }
#endif
            HaulingJobAnyItemOfType anyJob = job as HaulingJobAnyItemOfType;
            if (anyJob != null)
            {
                string logText = "";
                if (job.Item.HasValue)
                {
                    logText = "job item before: " + job.Item.Value.ToString() + ", now: ";
                }

                logText += item.ToString() + "was set by " + entity.ToString();

                anyJob.AddLog(logText);
            }

            this.job.Item = item;


            this.vehicleID = vehicle;
            this.ownersOfVehicles = ownersOfVehicles;
            this.IsOuterGoal = isOuterGoal;
            this.ParentGoal = parentGoal;

            /*  if (item == null)
              {
                  throw (new Exception("Item is null."));
              }*/

            NewOwner = newOwner;

            if (vehicleID != null)
            {
                SourcePiledItemJobs = new List<Tuple<HaulingJob, IKnownEntityData>>(); // new List<HaulingJob>();
                DestinationPiledItemJobs = new List<HaulingJob>();
            }
        }


        public GoalHaul()
        {
        }




        /*   public void SetOwnershipOnPickedupItems(OwnerTypes newOwnership, ItemCollection newBelongingCollection)
           {
           //    changeOwnership = true;
               NewOwnership = newOwnership;
               NewBelongingCollection = newBelongingCollection;
           }*/

        protected override void Activate()
        {
            // NEW: set locks in Activate instead of in SetGoal:
            if (IsOuterGoal)
            {   // nested (additional hauling jobs) have already been taken
                job.TakeJob(entity);
            }

            if (job.Item.HasValue == false)
            {
                // item should never be null when the goal is active! This bug is probably fixed now.
                System.Diagnostics.Debug.Assert(false, "Item is null!");
             
               // throw new Exception("Item is null!");

                Status = Goals.Status.Failed; // NEW
                return;
            }

            IKnownEntityData itemData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Item.Value, out itemData)))
            {
                return;
            }

            IKnownEntityData vehicleData = null;
            if (vehicleID != null)
            {
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(vehicleID.Value, out vehicleData)))
                {
                    return;
                }
                else
                {                   
                    entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(vehicleData.EntityID, entity.EntityID);                    
                }
            }


            // abort if someone else is going for the item:
            // Item can be null? Cancelled job?        
            if (itemData.AssignedToJob == null || itemData.AssignedToJob == job.ID)
            {
                Status = Status.Active;

                isActivated = true;

                itemData.AssignedToJob = job.ID; // Set locks on MemoryFacts too.

                //ownerOfItem = itemData.OwnedBy;

                //make sure the subgoal list is clear.
                RemoveAllSubgoals();

                entity.AgentStorage.MountedToolOrWeapon = null;

                //***********
                // add more hauling jobs
                if (IsOuterGoal)
                {

                    ExtraCargoResult result = FindExtraCargo(itemData, vehicleData); // can fail

                    if (Status == Goals.Status.Failed)
                        return;


                    if (result == ExtraCargoResult.Wait)
                    {
                        Status = Goals.Status.Inactive;
                        return; // wait...
                    }
                    else
                    {
                        DropItemsOverCapacity(itemData); // can fail

                        if (Status == Goals.Status.Failed)
                            return;
                    }

                    if (job.RequiresBoldStance) // outer goal only..? would it be better to set Bold after the item is reached...?
                    {
                        entityIntelligence.SetBoldStance();
                    }                   
                }

                // will mount certain items: - TODO: only Outer goal!!!
                UnfoldGoal(itemData);

            }
            else
            {
                Status = Status.Failed;
            }

        }

        private void SetLongDistanceHaulingFlag()
        {
            // affects whether the back hauling anim gets used.
          
            if (vehicleID == null   // only if not going by vehicle.
                && goalsToNest.Count == 0) // only the innermost goal sets this flag.
            {
                Vector3? haulToLocation;
                if (job.GetToLocation(entity, out haulToLocation))
                {
                    float haulDistance = Common.DistanceOctile(entity.PlaySiteLocation, haulToLocation.Value);

                   
                    if (haulDistance > GameData.Instance.Constants.DistanceForLongHaul)
                    {
                        entity.Renderable.SetAnimationStateFlag(ClientSide.Renderables.AnimModifier.Far);
                    }
                }
                else
                {
                    Status = Goals.Status.Failed;
                    return;
                }
            }
        }

        /// <summary>
        /// if true, then we do not want to stand up after bending down (play 2nd part of pickup anim)
        /// avoid 'doing squats'...
        /// </summary>
        /// <returns></returns>
        private bool IsNextPickupItemInSameSpot(Vector3 spot)
        {
            if (goalsToNest.Count > 0)
            {
                IKnownEntityData itemData;
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(goalsToNest.Peek().job.Item.Value, out itemData)))
                {
                    return false;
                }

                if (MapManager.WorldPosToSubtile(spot) == MapManager.WorldPosToSubtile(itemData.PlaySiteLocation))
                {
                    return true;
                }

            }

            return false;
        }

        public override void OnEnter()
        {
            base.OnEnter();


            //  itemData.AssignedToJob = job; // Set locks on MemoryFacts too!!

        }

        public override void Terminate()
        {
            base.Terminate();

            // the innermost goal will terminate first and clear this flag
            entity.Renderable.ClearAnimationStateFlag(ClientSide.Renderables.AnimModifier.Far);
           
        }

        private void UnfoldGoal(IKnownEntityData itemData)
        {
            Vector3 knownItemLocation = itemData.PlaySiteLocation;

            if (vehicleID == null)
            {

                if (IsOuterGoal 
                  && job.ToLocation.HasValue)
                {
                    // arm ourselves if needed. pick the furthest point from camp:
                    float distanceToDestinationFromCamp = Common.DistanceOctile(entityIntelligence.CurrentExpedition.Location.Value, job.ToLocation.Value);
                    float distanceToItemFromCamp = Common.DistanceOctile(entityIntelligence.CurrentExpedition.Location.Value, knownItemLocation);

                    Vector3 equipmentDestination;
                    if (distanceToDestinationFromCamp > distanceToItemFromCamp)
                    {
                        equipmentDestination = job.ToLocation.Value;
                    }
                    else
                    {
                        equipmentDestination = knownItemLocation;
                    }

                    FindOptionalEquipmentIfNeeded(equipmentDestination, job, mountWeapon: false);
                }


                if (MapManager.WorldPosToSubtile(entity.Location.Value) != MapManager.WorldPosToSubtile(itemData.PlaySiteLocation))
                {
                    // move to item
                    float maxDistanceSquared = (float)Math.Pow((double)GameData.Instance.Constants.InteractionDistanceForAgents, 2d);

                    AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, itemData,
                        GoalMoveToPosition.VehicleUse.NoVehicle)
                        {
                            PermittedDistanceSquaredToDestination = maxDistanceSquared
                        });

                }


                bool mountHauledItem = false;
                if (IsOuterGoal && goalsToNest.Count == 0)
                {
                    // if we are only hauling a spear or rifle for example, put it in hand:
                    mountHauledItem = itemData.EntityType.CanBeMounted(entity);
                }

                bool playBendDownAnim;
                bool playStandUpAnim;
                if (IsNextPickupItemInSameSpot(itemData.PlaySiteLocation))
                {
                    // tell the next goal that bending down is unneeded:
                    goalsToNest.Peek().IsStandingBentOverItem = true;

                    playStandUpAnim = false;
                }
                else
                {
                    playStandUpAnim = true;
                }

                playBendDownAnim = !IsStandingBentOverItem;


                // pick up    

              


                if (!PickupItemOrUnloadFirst(itemData, mountHauledItem, playBendDownAnim, playStandUpAnim, StorageCompartment.Haul))
                {
                    return;
                }


                if (goalsToNest.Count > 0)
                {   // pick up any extra cargo before going to the destination:
                    GoalHaul addedGoal = goalsToNest.Dequeue();
                    addedGoal.ParentGoal = this;
                    addedGoal.goalsToNest = goalsToNest;

                    AddSubgoal(addedGoal);
                    addedNestedGoals.Add(addedGoal);
                }


                SetLongDistanceHaulingFlag();
                               

                // move to destination              
                AddSubgoal(new GoalDropItem(entity, job.Item.Value, job.RequiredByProcessJob,
                    job.ToLocation, job.ToStorage, true, job.ID)); // #HAULMANAGERFIX

            }
            else
            {   // we are hauling by vehicle

                IKnownEntityData vehicleData;
                if (EntityResultCausesFailedGoal(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(vehicleID.Value, out vehicleData)))
                {
                    return;
                }

                // are we in a different vehicle than the one we are going to use?
                // or is the vehicle not near the item site?
                //if (entity.DrivingVehicle != UsedVehicle &&  UsedVehicle.MapPosition != job.Item.MapPosition)

                Entity drivenVehicle;
                if (!entity.GetDrivenVehicle(out drivenVehicle))
                {
                    Status = Goals.Status.Failed;
                    return;
                }

                if (drivenVehicle != vehicleData &&
                    Common.DistanceOctile(vehicleData.PlaySiteLocation, knownItemLocation) > ((VehicleContainerType)vehicleData.EntityType.ContainerType).LoadingRadius)
                {
                    // NEW: parking is handled by GoalMoveToDestination and FindParkingSpot
                    // what if no parking spot is found within LoadingRadius...? 
                    // move to vehicle and get in:
                    AddSubgoal(new GoalEnterVehicleAsDriver(entity, vehicleID.Value, null));
                    AddSubgoal(new GoalMoveToPosition(entity, knownItemLocation, ownersOfVehicles, GoalMoveToPosition.VehicleUse.KeepVehicle));

                }


                //NEW
                if (!SourcePileIsHandled)
                {   // go through the nested goals/jobs, and set a flag if they have spawned their appropriate goals.
                    HandleItemsThatWillBePiledAtSource(SourcePiledItemJobs, itemData);

                    SourcePiledItemJobs.Add(new Tuple<HaulingJob, IKnownEntityData>(job, itemData));
                }

                MoveItemsInSourcePile(SourcePiledItemJobs);

                // load                      


                /*
                bool moreItemsWillBeLoadedHere = false;
                if (goalsToNest.Count > 0)
                {
                    Point nextItemPosition = UWGame.SimSide.Instance.GetKnownMapPosition(entityIntelligence.Allegiance, goalsToNest.Peek().job.Item);
                    if (nextItemPosition == knownItemPosition) //(ParentGoal != null && ParentGoal.job.Item.MapPosition == job.Item.MapPosition);
                    {
                        moreItemsWillBeLoadedHere = true;
                    }
                }
                
                // DOES this work???             
                if (!moreItemsWillBeLoadedHere)
                {   // get in the vehicle.
                    // do this in Activate GoalMove instead.
                   // AddSubgoal(new GoalEnterVehicleAsDriver(entity, UsedVehicle, null));
                }*/


                bool hasNestedGoals = goalsToNest.Count > 0;

                if (hasNestedGoals)
                {
                    HandleItemsThatWillBePiledAtDestination();

                    // pick up any extra cargo before going to the destination:
                    GoalHaul addedGoal = goalsToNest.Dequeue();
                    addedGoal.goalsToNest = goalsToNest;
                    addedGoal.ParentGoal = this;
                    AddSubgoal(addedGoal);
                }

                if (!hasNestedGoals && // if there are no more nested goals to unfold
                    (ParentGoal != null // and we are the innermost goal
                    || IsOuterGoal)) //  or if we are the outer goal (when there is only 1 haul goal)
                {   // now travel to the hauling destination 

                    // Vector3 to = job.ToLocation;

                    // this subgoal should select a proper parking spot for us...

                    // NEW! specify the vehicle we are using - this allows the goal to determine if it is necessary to enter it first.
                    AddSubgoal(new GoalMoveToPosition(entity, job.ToLocation.Value, ownersOfVehicles, GoalMoveToPosition.VehicleUse.KeepVehicle) { UsedVehicle = this.vehicleID });
                    AddSubgoal(new GoalExitVehicle(entity, vehicleID.Value));
                }


                if (!DestinationPileIsHandled)
                {
                    DestinationPiledItemJobs.Add(job);
                }

                MoveItemsInDestinationPile(DestinationPiledItemJobs);

            }
        }

        private ExtraCargoResult FindExtraCargo(IKnownEntityData itemData, IKnownEntityData vehicleData)
        {
            float capacity;
            bool isUsingAirTransport = false;


            if (vehicleData != null)
            {
                //vehicleData.InUseBy = entity.EntityID;

                entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(vehicleData.EntityID, entity.EntityID);          

                // we have a vehicle. See if we can add more cargo. Compile a queued list of extra goals to nest.
                // TODO: update this section to account for items carried along but not hauled... see below.
                if (vehicleData.ContainsEntity(job.Item.Value))
                {
                    // item is already on board:
                    capacity = vehicleData.TotalItemStorageCapacity.Value - vehicleData.TotalStored.Value;
                }
                else
                {
                    capacity = vehicleData.TotalItemStorageCapacity.Value - vehicleData.TotalStored.Value - itemData.Bulk;
                }

                if (((VehicleContainerType)vehicleData.EntityType.ContainerType).Aircraft != null)
                {
                    isUsingAirTransport = true;
                }
            }
            else
            {
                // calculate our capacity. if we are carrying items with hauling jobs that have not been taken, don't count those items.
                // after we have our full load, dump stuff that we can't carry...
                capacity = entity.AgentStorage.ItemStorage.TotalCapacity;

                // first subtract the bulk of the item that goes with the outer goal:
                if (!entity.AgentStorage.ItemStorage.Contains(job.Item.Value))
                {
                    capacity -= itemData.Bulk;
                }

                Item itemComponent;
                IKnownEntityData carriedItemData;
                EntityID carriedItem;
                for (int i = entity.AgentStorage.ItemStorage.StoredItems.Count - 1; i >= 0; i--)
                {
                    carriedItem = entity.AgentStorage.ItemStorage.StoredItems[i];

                    EntityResult result = entityIntelligence.GetKnownData(carriedItem, out carriedItemData);

                    if (result != EntityResult.SeenDirectly) // EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(carriedItem, out carriedItemData))
                    {
                        // obsolete item... update the storage collections:
                        entity.AgentStorage.ItemStorage.RemoveOutdatedItem(carriedItem);
                    }
                    else
                    {
                        Entity itemEntity = (Entity)carriedItemData;

                        itemComponent = itemEntity.Item;

                        if (/*itemComponent.EquippedBy == entity // weapons or stuff that we carry for protection, for instance... this cannot be dropped so reduce the capacity by its bulk
                            ||*/ ItemIsCurrentlyHauledByUs(itemEntity) // this item is currently being hauled by us... reduce capacity by its bulk because it cannot be dropped/taken again                           
                            )
                        {
                            capacity -= itemEntity.Bulk;
                        }
                    }
                }
            }

            // we are getting ready to haul additional items.
            // in case the entity is carrying items, for which the job manager has not created hauling jobs, do this now.
            // do this on item pickup/creation please...
            //  HaulingJobManager.CreateHaulingJobsOutOfBand(UsedVehicle != null? UsedVehicle : entity, ownerOfJobs);


            HaulingJob addedJob;
            IKnownEntityData addedItem;

            Vector3? lastDestination = job.ToLocation;            
            EntityID? lastDestinationEntity = job.GetToStorageEntity; // .ToStorageEntity;
            Vector3 lastItemLocation = itemData.PlaySiteLocation;



            while (capacity > 0f)
            {
                if (entity.ID == (EntityID)25178 && itemData.EntityID == (EntityID)25221)
                {

                }

                ExtraCargoResult result = FindExtraCargo(entity, itemData, lastDestination, lastDestinationEntity,
                    capacity, isUsingAirTransport,
                    out addedJob, out addedItem);


                if (Status == Goals.Status.Failed)
                    return ExtraCargoResult.NoneFound;


                if (result == ExtraCargoResult.Wait)
                {
                    /*We are waiting. We must now do something to prevent being seen as completed. */

                    AddSubgoal(new GoalWait(entity));
                    isFindingExtraJobs = true;

                    // if we resume after wait - start over.                
                    CancelNestedGoals();

                    return ExtraCargoResult.Wait;
                }

                if (result == ExtraCargoResult.OK)
                {
                    /* if (!entity.ItemStorage.Contains(addedItem))
                     {
                         bulkOfAddedItemsNotCarried += addedItem.Bulk; // we need room to pick this up...
                     }*/

                    if (entity.ID == (EntityID)25178 && addedItem.EntityID == (EntityID)25221)
                    {

                    }

                    //What should entity.CurrentGoalScore be? It is given same score as outer job for now...
                    addedJob.TakeJob(entity);
                    addedItem.AssignedToJob = addedJob.ID;

                    GoalHaul addedGoal = new GoalHaul(entity, addedJob, addedItem.EntityID,
                        vehicleID, this, false, addedJob.NewOwner, ownersOfVehicles);

                    // the nested goals will be added in the Activate method:
                    goalsToNest.Enqueue(addedGoal);

                    lastDestination = addedJob.ToLocation;
                    if (addedJob.ToStorage.HasValue)
                    {
                        lastDestinationEntity = addedJob.ToStorage.Value.StorageEntity; // ToStorageEntity;
                    }
                    else
                    {
                        lastDestinationEntity = null;
                    }

                    lastItemLocation = addedItem.PlaySiteLocation;
                    // this is only approximate... since we can have several compartments...

                    capacity = capacity - addedItem.Bulk;


                }
                else
                {
                    //   DropItemsOverCapacity(bulkOfAddedItemsNotCarried);
                    return ExtraCargoResult.OK; // no more cargo found... signal that we are done.                                
                }

            }

            //   DropItemsOverCapacity(bulkOfAddedItemsNotCarried);
            return ExtraCargoResult.OK; // no more cargo found / no capacity...         
        }


        private bool ItemIsCurrentlyHauledByUs(Entity item) // Item item)
        {
            Job assignedToJob = EvaluateJob.ResolveAssignedToJob(item);
            return (assignedToJob != null && assignedToJob.TakenBy.Contains(entity));
        }

        /// <summary>
        /// calculate the space we need to carry everything, and drop any unnecessary items...
        /// TODO: vehicles too!
        /// </summary>
        private void DropItemsOverCapacity(IKnownEntityData itemData) //double bulkOfAddedItemsNotCarried)
        {
            // calculate how much space we need:
            float bulkOfAddedItemsNotCarried = 0.0f;

            // remember the item that goes with the outer goal:
            if (!entity.AgentStorage.ItemStorage.Contains(job.Item.Value))
            {
                bulkOfAddedItemsNotCarried += itemData.Bulk; // we need room to pick this up...
            }

            foreach (GoalHaul goal in goalsToNest)
            {
                if (!entity.AgentStorage.ItemStorage.Contains(goal.job.Item.Value))
                {
                    IKnownEntityData nestedGoalItemData;
                    EntityResult result = entityIntelligence.GetKnownData(goal.job.Item.Value, out nestedGoalItemData);
                    if (EntityResultCausesFailedGoal(result))
                    {
                        return;
                    }

                    bulkOfAddedItemsNotCarried += nestedGoalItemData.Bulk; // we need room to pick this up...
                }
            }


            float totalStorageNeeds = entity.AgentStorage.ItemStorage.TotalStored + bulkOfAddedItemsNotCarried;
            if (Common.IsGreaterThan(totalStorageNeeds, entity.AgentStorage.ItemStorage.TotalCapacity))//totalStorageNeeds > entity.AgentStorage.ItemStorage.TotalCapacity) // we need to drop something...
            {
                List<Entity> listOfItemsToDrop = new List<Entity>();

                Item itemComponent;

                for (int i = entity.AgentStorage.ItemStorage.StoredItems.Count - 1; i >= 0; i--)
                {
                    EntityID carriedItem = entity.AgentStorage.ItemStorage.StoredItems[i];
                    Entity itemEntity;
                    if (EntityIsNotSeenDirectly(carriedItem, out itemEntity))
                    {
                        // we can remove this
                        entity.AgentStorage.ItemStorage.StoredItems.RemoveAt(i);
                    }
                    else
                    {
                        itemComponent = itemEntity.Item;

                        if (//itemComponent.EquippedBy == null && 
                            !ItemIsCurrentlyHauledByUs(itemEntity))
                        {
                            totalStorageNeeds -= itemEntity.Bulk;
                            listOfItemsToDrop.Add(itemEntity);

                            if (Common.IsLessThanOrEqual(totalStorageNeeds, entity.AgentStorage.ItemStorage.TotalCapacity))//totalStorageNeeds <= entity.AgentStorage.ItemStorage.TotalCapacity)
                            {
                                break;
                            }
                        }
                    }
                }


                //This should not be able to occur. This should be a problem that have been resolved and calculated earlyer (perhaps in the evaluators)
                //if (Common.IsGreaterThanOrEqual(totalStorageNeeds, entity.AgentStorage.ItemStorage.TotalCapacity))//totalStorageNeeds > entity.AgentStorage.ItemStorage.TotalCapacity) // we still need to drop something!!!!
                //{
                //    Status = Goals.Status.Failed;
                //    return;
                //}

                foreach (Entity item in listOfItemsToDrop)
                {
                    AddSubgoal(new GoalDropItem(entity, item.EntityID));
                }
            }
        }

        private enum ExtraCargoResult { OK, NoneFound, Wait }
        private ExtraCargoResult FindExtraCargo(Entity entity, IKnownEntityData lastItem, Vector3? lastDestination, EntityID? lastDestinationEntity, //Vector3 lastItemLocation, 
            double capacity, bool usingAirTransport, out HaulingJob addedJob, out IKnownEntityData addedItem)
        {
            // find the best job:
            /*double currentScore = 0;
            double bestScore = 0;
            */
            addedJob = null;
            addedItem = null;

            if (capacity <= 0)
                return ExtraCargoResult.NoneFound; // false;

            IKnownEntityData bestItem = null;
            HaulingJob bestJob = null;
            float bestDistance = 1000000;


            MovementMap moveMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel,
                       entity.EntityType, entity.Intelligence.ThreatStance);

            IKnownEntityData destinationEntityData, lastDestinationEntityData = null;
            EntityID? toStorageEntityID;

            if (lastDestinationEntity.HasValue)
            {
                if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(lastDestinationEntity.Value, out lastDestinationEntityData)))
                {
                    return ExtraCargoResult.NoneFound; // shouldn't happen...
                }
            }


            EntityGroup ownerOfJobs = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerOfJobsID);

            if (ownerOfJobs == null)
            {
                Status = Goals.Status.Failed;
                return ExtraCargoResult.NoneFound;
            }

            Job job;

            // optimize with quadtree..? 
            for (int j = ownerOfJobs.HaulingJobs.Count - 1; j >= 0; j--)
            {
                job = ownerOfJobs.HaulingJobs[j];

                if (job.TakenBy.Count < job.MaxJobPositions)
                {
                    toStorageEntityID = ((HaulingJob)job).GetToStorageEntity; // ToStorageEntity;
                    destinationEntityData = null;
                    if (toStorageEntityID.HasValue)
                    {
                        if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(toStorageEntityID.Value, out destinationEntityData)))
                        {
                            // remove the job, the storage entity is gone:
                           
                            DestroyJobAndRemoveLocks(ref job);

                            continue;
                        }
                    }

                    if (job is HaulingJobAnyItemOfType)
                    {
                        HaulingJobAnyItemOfType hj = (HaulingJobAnyItemOfType)job;

                        EntityGroup itemsGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(hj.ItemsToHaulGroup);
                        if (itemsGroup == null)
                        {
                            // remove the job, the item owner is gone:                          
                            //  bool hasBeenDestroyed;
                            DestroyJobAndRemoveLocks(ref job);

                            continue;
                        }

                        if (itemsGroup.Items.ContainsKey(hj.RequiredItemType) &&
                            hj.RequiredItemType.ItemType.MaximumBulk <= capacity) // can we carry this item?
                        {
                            // search for closest item of specific type                              
                            List<EntityID> itemsOfNeededType = itemsGroup.Items[hj.RequiredItemType];

                            EntityID id;
                            IKnownEntityData itemData;
                            for (int i = itemsOfNeededType.Count - 1; i >= 0; i--)
                            {
                                id = itemsOfNeededType[i];
                                if (!GoalEvaluator.HandleOwnerDataResult(entityIntelligence.Allegiance.SharedKnowledge,
                                    id, itemsGroup, out itemData))
                                {
                                    continue;
                                }
                                else
                                {

                                    RateCargoResult result = RateItemAsAdditionalCargo(hj, destinationEntityData, itemData, lastItem, lastDestination, lastDestinationEntityData, usingAirTransport, ref bestItem, ref bestDistance, ref bestJob, moveMap, entity);
                                    if (result == RateCargoResult.Finished)
                                    {
                                        if (itemData != null && !itemData.IsCompleted())
                                        {                                           
                                            System.Diagnostics.Debug.Assert(false, "Assigned incomplete item?");
                                        }

                                        addedJob = hj;
                                        addedItem = itemData;
                                        return ExtraCargoResult.OK; //true;
                                    }
                                    else if (result == RateCargoResult.Wait)
                                    {

                                        return ExtraCargoResult.Wait;
                                    }

                                }
                            }
                        }
                    }
                    else if (job is HaulingJobSpecificItem)
                    {
                        // specific item to haul
                        // rate this specific item:       
                        HaulingJobSpecificItem hj = (HaulingJobSpecificItem)job;

                        IKnownEntityData itemData;
                        EntityResult entityResult = entityIntelligence.GetKnownData(hj.Item.Value, out itemData);

                        if (entityResult == EntityResult.Destroyed || entityResult == EntityResult.EntityStatusIsNowUnknown)
                        {
                            // remove this outdated job:                           
                            // bool hasBeenDestroyed;
                            DestroyJobAndRemoveLocks(ref job);

                            continue;
                        }


                        if (itemData.Bulk <= capacity)
                        {
                            RateCargoResult result = RateItemAsAdditionalCargo(hj, destinationEntityData, itemData, lastItem, lastDestination, lastDestinationEntityData, usingAirTransport, ref bestItem, ref bestDistance, ref bestJob, moveMap, entity);
                            if (result == RateCargoResult.Finished)
                            {
                                if (itemData != null && !itemData.IsCompleted())
                                {
                                    System.Diagnostics.Debug.Assert(false, "Assigned incomplete item?");
                                }

                                addedJob = hj;
                                addedItem = itemData;
                                return ExtraCargoResult.OK; //true;
                            }
                            else if (result == RateCargoResult.Wait)
                            {
                                return ExtraCargoResult.Wait;
                            }
                        }
                        /* else if (hj.TakenBy.Count == 0)
                         {
                             // the item is no longer free, it is being picked up by someone else, possibly to fulfill an ItemType hauling job.
                             // we want to delete this job.
                             EvaluateHaulingJobs.RecordOutdatedJob(ref outdatedJobs, hj);
                         }*/

                    }

                }
                // }

            }


            if (bestItem != null)
            {
                addedItem = bestItem;
                addedJob = bestJob;
                return ExtraCargoResult.OK; //true;
            }

            return ExtraCargoResult.NoneFound;
            //false;

        }

        private void DestroyJobAndResetThreatstance()
        {
            if (IsOuterGoal)
            {
                ResetThreatStance(job);
            }

            DestroyJobAndRemoveLocks(ref job);
        }

        private enum RateCargoResult { Wait, Finished, KeepLooking }
        /// <summary>
        /// only give one hauling job as argument
        /// </summary>
        /// <param name="hj"></param>
        /// <param name="hjs"></param>
        /// <param name="item"></param>
        /// <param name="lastItemLocation"></param>
        /// <param name="lastDestination"></param>
        /// <param name="usingAirTransport"></param>
        /// <param name="bestItem"></param>
        /// <param name="bestDistance"></param>
        /// <param name="bestJob"></param>
        /// <param name="moveMap"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private RateCargoResult RateItemAsAdditionalCargo(HaulingJob hj, IKnownEntityData toStorageEntityData, IKnownEntityData item, IKnownEntityData lastItem, // Vector3 lastItemLocation, 
            Vector3? lastDestination, IKnownEntityData lastDestinationEntity, bool usingAirTransport, ref IKnownEntityData bestItem, ref float bestDistance, ref HaulingJob bestJob,
            MovementMap moveMap, Entity entity)
        {
            if (entity.ID == (EntityID)25178 && item.EntityID == (EntityID)25221)
            {

            }

            HaulingJobAnyItemOfType hja = hj as HaulingJobAnyItemOfType;
            HaulingJobSpecificItem hjs = hj as HaulingJobSpecificItem;

           
            // why can't we call IsItemValidForHauling instead..

            if (((hja != null && item.IsUnassigned(entityIntelligence.Allegiance.SharedKnowledge))
              || (hjs != null && item.IsUnassignedToAnythingButThisJob(hjs, entityIntelligence.Allegiance.SharedKnowledge)))
               && item.IsItemValidForHauling(entity, entityIntelligence, hj))

           /* if (((hja != null && item.IsUnassigned(groupToCheck))
              || (hjs != null && item.IsUnassignedToAnythingButThisJob(hjs, groupToCheck)))

                            // don't take items hauled by other agents.... I think the evaulator allows this..?
              && (item.ContainedBy == null || item.StoredPermanentlyIn.HasValue  //  laying in the open or inside a building
                || entity.AgentStorage.Contains(item.EntityID)) // - or carried by us. must match GoalPickup!!!

              && !hj.IsStoredInTarget(item) 
              && item.CanBeHauled() 
                )*/
            {

                // don't allow taking other agent's items, we won't send them drop messages at this stage...
                if (item.ContainedBy.HasValue)
                {
                    Entity containingEntity = Entity.FindByID(item.ContainedBy.Value);
                    if (containingEntity != null && containingEntity.AgentStorage != null && containingEntity.AgentStorage.Contains(item.EntityID))
                    {
                        return RateCargoResult.KeepLooking;
                    }
                }
                
                float Item1ToItem2 = -1f;
                float Destination1ToDestination2 = -1f;
                float Destination1ToItem2 = -1f;

                if (!usingAirTransport)
                {

                    RegionMap footRegionMap = moveMap.Layers[SurfaceType.TransportType.Foot].RegionMap;
                    RegionMap.Result result1 = footRegionMap.GetDistanceToEntity(entity, lastItem, item, ref Item1ToItem2); //  MapManager.WorldPosToSubtile(lastItemLocation), itemSubtilePosition, ref Item1ToItem2);

                    if (result1 == RegionMap.Result.Wait)
                    {
                        return RateCargoResult.Wait;
                    }
                    else if (result1 == RegionMap.Result.NoAccess)
                    {
                        return RateCargoResult.KeepLooking; // ExtraCargoResult.NoneFound; // RegionMap.Result.NoAccess;
                    }

                    RegionMap.Result result2 = footRegionMap.GetDistanceToEntityUsingWorldLocation(entity, lastDestinationEntity, toStorageEntityData, ref Destination1ToDestination2,
                         lastDestination, hj.ToLocation);

                    if (result2 == RegionMap.Result.Wait)
                    {
                        return RateCargoResult.Wait;
                    }
                    else if (result2 == RegionMap.Result.NoAccess)
                    {
                        return RateCargoResult.KeepLooking;
                    }


                    RegionMap.Result result3 = footRegionMap.GetDistanceToEntityUsingWorldLocation(entity, lastDestinationEntity, item, ref Destination1ToItem2, lastDestination, hj.ToLocation);
                   
                    if (result3 == RegionMap.Result.Wait)
                    {
                        return RateCargoResult.Wait;
                    }
                    else if (result3 == RegionMap.Result.NoAccess)
                    {
                        return RateCargoResult.KeepLooking;
                    }

                }
                else
                {                   
                    Item1ToItem2 = Common.DistanceOctile(lastItem.PlaySiteLocation, item.PlaySiteLocation); // knownItemLocation);

                    Vector3 lastDestinationValue = lastDestination ?? lastDestinationEntity.AccessPoint.Value;
                    Vector3 thisDestinationValue = hj.ToLocation ?? toStorageEntityData.AccessPoint.Value; // lastDestinationEntity.Location;

                    Destination1ToDestination2 = Common.DistanceOctile(lastDestinationValue, thisDestinationValue);

                    Destination1ToItem2 = Common.DistanceOctile(lastDestinationValue, item.PlaySiteLocation);
                }

                if (Item1ToItem2 + Destination1ToDestination2 < 0.6f * Destination1ToItem2) // fudge factor to compensate for carrying stuff back and forth
                {   // this item can be carried as additional cargo:

                    if (Item1ToItem2 + Destination1ToDestination2 == 0) //Item1ToItem2 == 0)
                    {   // if it is at the same location, we are happy.

                        bestItem = item;
                        bestJob = hj;
                        bestDistance = Item1ToItem2 + Destination1ToDestination2;
                        return RateCargoResult.Finished;
                        //return true; // We are DONE!!!
                    }
                    else if (Item1ToItem2 + Destination1ToDestination2 < bestDistance || bestItem == null)
                    {   // good, but keep looking:
                        bestItem = item;
                        bestJob = hj;
                        bestDistance = Item1ToItem2 + Destination1ToDestination2;
                    }

                }
            }
            return RateCargoResult.KeepLooking; //false;
        }

        public void HandleItemsThatWillBePiledAtSource(List<Tuple<HaulingJob, IKnownEntityData>> piledSourceJobs, IKnownEntityData itemData)
        {
            Point knownItemPosition = MapManager.WorldPosToTile(itemData.PlaySiteLocation);

            if (goalsToNest != null && goalsToNest.Count > 0)
            {
                foreach (GoalHaul goal in goalsToNest)
                {
                    IKnownEntityData nestedItemData;
                    if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(goal.job.Item.Value, out nestedItemData)))
                    {
                        return;
                    }

                    Point nextKnownItemPosition = MapManager.WorldPosToTile(nestedItemData.PlaySiteLocation);

                    if (nextKnownItemPosition == knownItemPosition)
                    {
                        piledSourceJobs.Add(new Tuple<HaulingJob, IKnownEntityData>(goal.job, nestedItemData));
                        goal.SourcePileIsHandled = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }

        }


        public void HandleItemsThatWillBePiledAtDestination()
        {
            if (!DestinationPileIsHandled)
            {
                if (goalsToNest != null && goalsToNest.Count > 0)
                {
                    foreach (GoalHaul goal in goalsToNest)
                    {
                        if ((goal.job.ToStorage.HasValue && job.ToStorage.HasValue 
                            && goal.job.ToStorage.Value.StorageEntity == job.ToStorage.Value.StorageEntity) //goal.job.ToStorageEntity.HasValue && goal.job.ToStorageEntity == job.ToStorageEntity)
                            ||
                            (MapManager.WorldPosToTile(goal.job.ToLocation.Value) == MapManager.WorldPosToTile(job.ToLocation.Value)))
                        {
                            DestinationPiledItemJobs.Add(goal.job);
                            goal.DestinationPileIsHandled = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// move the items we have determined have the same destination. Take multiple trips back and forth if necessary.
        /// </summary>
        /// <param name="positionOfPile"></param>
        /// <param name="piledDestinationJobs"></param>
        private void MoveItemsInSourcePile(List<Tuple<HaulingJob, IKnownEntityData>> piledSourceJobs)
        {
            if (piledSourceJobs != null)
            {
                if (piledSourceJobs.Count == 1)
                {
                    AddSubgoal(new GoalPickup(entity, piledSourceJobs[0].Item1.Item.Value, NewOwner));
                    AddSubgoal(new GoalLoad(entity, vehicleID.Value, piledSourceJobs[0].Item1.Item.Value, NewOwner));

                }
                else if (piledSourceJobs.Count > 1)
                {
                    double capacity = entity.AgentStorage.ItemStorage.TotalCapacity - entity.AgentStorage.ItemStorage.TotalStored;
                    double currentCapacity;
                    List<Tuple<HaulingJob, IKnownEntityData>> itemsToMoveInOneGo = new List<Tuple<HaulingJob, IKnownEntityData>>();

                    int i = 0;
                    bool isFirstTrip = true;

                    while (i < piledSourceJobs.Count)
                    {
                        currentCapacity = capacity;
                        itemsToMoveInOneGo.Clear();


                        while (currentCapacity > 0
                            && i < piledSourceJobs.Count
                            && piledSourceJobs[i].Item2.Bulk <= currentCapacity)
                        {   // pile it up
                            itemsToMoveInOneGo.Add(piledSourceJobs[i]);
                            currentCapacity = currentCapacity - piledSourceJobs[i].Item2.Bulk;
                            i++;
                        }

                        // pick it all up
                        for (int j = 0; j < itemsToMoveInOneGo.Count; j++)
                        {
                            EntityID? outerContainer = itemsToMoveInOneGo[j].Item2.ContainedBy; // ContainingBuilding;
                            if (outerContainer != null)
                            {
                                //  Entity outerContainerEntity = Entity.FindByID(outerContainer.Value);
                                IKnownEntityData containerData;
                                entityIntelligence.GetKnownData(outerContainer.Value, out containerData);

                                if (containerData != null && containerData.EntityType.ContainerType is VehicleContainerType && outerContainer != vehicleID)
                                //if (itemsToMoveInOneGo[j].Item2.Item.OnBoard != null && itemsToMoveInOneGo[j].Item.Item.OnBoard != UsedVehicle)
                                {   // we must first unload this item:                                
                                    AddSubgoal(new GoalUnload(entity, outerContainer.Value,
                                        itemsToMoveInOneGo[j].Item1.Item.Value));
                                }
                            }

                            AddSubgoal(new GoalPickup(entity, itemsToMoveInOneGo[j].Item1.Item.Value, NewOwner));
                        }


                        // load everything
                        for (int j = 0; j < itemsToMoveInOneGo.Count; j++)
                        {

                            AddSubgoal(new GoalLoad(entity, vehicleID.Value, itemsToMoveInOneGo[j].Item1.Item.Value, NewOwner));
                        }

                        isFirstTrip = false;
                    }
                }

            }

        }


        /// <summary>
        /// move the items we have determined have the same destination. Take multiple trips back and forth if necessary.
        /// </summary>
        /// <param name="positionOfPile"></param>
        /// <param name="piledDestinationJobs"></param>
        private void MoveItemsInDestinationPile(List<HaulingJob> piledDestinationJobs)
        {
            if (piledDestinationJobs != null)
            {
                if (piledDestinationJobs.Count == 1)
                {
                    AddSubgoal(new GoalUnload(entity, vehicleID.Value,
                        piledDestinationJobs[0].Item.Value));
                    AddSubgoal(new GoalPickup(entity, piledDestinationJobs[0].Item.Value, NewOwner));

                    AddSubgoal(new GoalDropItem(entity, piledDestinationJobs[0].Item.Value, piledDestinationJobs[0].RequiredByProcessJob, job.ToLocation, job.ToStorage));

                }
                else if (piledDestinationJobs.Count > 1)
                {
                    double capacity = entity.AgentStorage.ItemStorage.TotalCapacity - entity.AgentStorage.ItemStorage.TotalStored;
                    double currentCapacity;
                    List<HaulingJob> itemsToMoveInOneGo = new List<HaulingJob>();

                    int i = 0;
                    bool isFirstTrip = true;

                    HaulingJob thisJob;

                    foreach (HaulingJob job in piledDestinationJobs)
                    {
                        // is this best?
                        AddSubgoal(new GoalUnload(entity, vehicleID.Value, job.Item.Value));
                    }

                    while (i < piledDestinationJobs.Count)
                    {
                        currentCapacity = capacity;
                        itemsToMoveInOneGo.Clear();

                        IKnownEntityData itemData;
                        if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(piledDestinationJobs[i].Item.Value, out itemData)))
                        {
                            return;
                        }

                        while (currentCapacity > 0
                            && i < piledDestinationJobs.Count
                            && itemData.Bulk <= currentCapacity)
                        {   // pile it up

                            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(piledDestinationJobs[i].Item.Value, out itemData)))
                            {
                                return;
                            }

                            itemsToMoveInOneGo.Add(piledDestinationJobs[i]);
                            currentCapacity = currentCapacity - itemData.Bulk;
                            i++;

                        }

                        // carry it all in and assign to requiring job:
                        for (int j = 0; j < itemsToMoveInOneGo.Count; j++)
                        {
                            //is it better to unload everything at once?
                            //  AddSubgoal(new GoalUnload(entity, UsedVehicle, itemsToMoveInOneGo[j].Item));
                            AddSubgoal(new GoalPickup(entity, itemsToMoveInOneGo[j].Item.Value, NewOwner));
                        }

                        //  AddSubgoal(new GoalMoveToPosition(entity, job.To, GoalMoveToPosition.VehicleUse.NoVehicle, ownersOfVehicles, job.ToTileCenterOffset));                        

                        for (int j = 0; j < itemsToMoveInOneGo.Count; j++)
                        {
                            thisJob = itemsToMoveInOneGo[j];
                                                       
                            AddSubgoal(new GoalDropItem(entity, thisJob.Item.Value, thisJob.RequiredByProcessJob, 
                                thisJob.ToLocation, job.ToStorage));
                        }

                        isFirstTrip = false;
                    }
                }

            }

        }

        public double ScoreGoal()
        {
            // only score outer goal!
            if (ParentGoal != null)
            {
                return ParentGoal.ScoreGoal();
            }
            else
            {
                //  double result = 0;
                //   RegionMap footRegionMap = entityIntelligence.Allegiance.SharedKnowledge.GetFootRegionMap(entity); // UWGame.SimSide.Instance.Map.RegionMapManager.HumanFootExposedNormalRegionMap; //RegionMaps[TerrainType.TransportType.Foot];

                IKnownEntityData vehicle = null;
                if (vehicleID.HasValue)
                {
                    if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(vehicleID.Value, out vehicle)))
                    {
                        Status = Goals.Status.Failed;
                        return 0;
                    }
                }

                if (entity.ID == (EntityID)4889) 
                {

                }

                return ScoreJobGoal(job, null, null,
                    new HaulingParams()
                    {
                        Item = job.Item.Value, // crash here where Item is null for an AnyItemJob (illegal state when the goal is Active!): http://steamcommunity.com/app/284100/discussions/2/540740501213132797/
                        Vehicle = vehicle
                    });


                //   GoalEvaluator.CalculateResult calcResult = EvaluateHaulingJobs.ScoreThisJob(footRegionMap, entity, job, 1, null, null, out result, null, null, new HaulingParams() { Item = job.Item.Value, Vehicle = vehicle }); 
                //vehicle, ref result, GoalEvaluator.Priority);

                /*   if (calcResult == GoalEvaluator.CalculateResult.Done)
                   {
                     //  result = EvaluateHaulingJobs.AddTimeAndAgeContributions(result, GoalEvaluator.ScoreTimeOfDay(), GoalEvaluator.GetAgeContribution(), GoalEvaluator.Priority);

                       return result;
                   }
                   else
                   {
                       // we don't have time to wait for the score... return the cached score.
                       // also, we want to disregard the answer when it comes back...
                       return GetCurrentGoalScore();
                   }*/
            }
        }

        public override bool IsSame(Job job)
        {   // does this work as intended... goal switching when hauling should probably be limited
            // to the en-route fase...
            if (DestinationPiledItemJobs != null && DestinationPiledItemJobs.Count > 0)
            {
                foreach (HaulingJob piledJob in DestinationPiledItemJobs)
                {
                    if (job == piledJob)
                    {
                        return true;
                    }
                }
            }

            if (goalsToNest != null && goalsToNest.Count > 0)
            {
                // not yet subgoals:
                foreach (GoalHaul nestedGoal in goalsToNest)
                {
                    if (nestedGoal.job == job)
                    {
                        return true;
                    }
                }
            }

            if (addedNestedGoals != null && addedNestedGoals.Count > 0)
            {
                // these are also subgoals, but is is easier to iterate this collection:
                foreach (GoalHaul nestedGoal in addedNestedGoals)
                {
                    if (nestedGoal.job == job)
                    {
                        return true;
                    }
                }
            }

            return job == this.job;
        }


        protected override bool ArePreconditionsOK()
        {
            if (job.Item == null)
            {
                return false;
            }

            IKnownEntityData data;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Item.Value, out data)))
            {
                return false;
            }

            // PARTFIX: test PartOf
            if (!data.CanBeHauled())
                return false;


            // are we still the owner of the item:
            // should we check Contains in an EntityGroup here? If so, we need an ID
            /*   if (data.OwnedBy != ownerOfItem) 
                   return false;*/

            return true;
        }

        public void AssertAllSubgoalJobsTaken()
        {
#if DEBUG
            if (job != null)
            {
                if (!job.TakenBy.Contains(entity))
                {
                    // this makes it possible for other agents to take the job!!!?!?
                    // nested haul jobs should always be assigned!
                    System.Diagnostics.Debug.Assert(false, "subgoal no longer assigned??");
                }
            }

            if (Subgoals != null)
            {
                foreach (var item in Subgoals)
                {
                    GoalHaul haulGoal = item as GoalHaul;
                    if (haulGoal != null)
                    {
                        haulGoal.AssertAllSubgoalJobsTaken();
                    }
                }
            }
#endif
        }

        /// <summary>
        /// don't give the bonus during the "negotiation phase"
        /// </summary>
        /// <returns></returns>
        private bool GetsRecentlyHauledScoreBonus()
        {
            return TimeSpentInTopLevelGoal > 3d;
        }

        private void SetRecentlyHauledOnAllItems()
        {
            if (job != null && job.Item.HasValue)
            {
                entityIntelligence.Memory.SetLastHauledItem(job.Item.Value);
            }

            // get all nested or added haul goals also
            if (addedNestedGoals != null)
            {
                foreach (var item in addedNestedGoals)
                {
                    if (item.job != null // null when completed..
                        && item.job.Item.HasValue)
                    {
                        entityIntelligence.Memory.SetLastHauledItem(item.job.Item.Value);
                    }
                }

                foreach (var item in goalsToNest)
                {
                    if (item.job != null // null when completed..
                        && item.job.Item.HasValue)
                    {
                        entityIntelligence.Memory.SetLastHauledItem(item.job.Item.Value);
                    }
                }
            }
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
           
            if (job is HaulingJobAnyItemOfType && job.Item == null)
            {
                System.Diagnostics.Debug.Assert(false, "Item is null!");
               // throw new Exception("Item is null!");

                Status = Goals.Status.Failed; // NEW
                return;
            }

            AssertAllSubgoalJobsTaken();


            if (!preconditionsRegulator.IsReady() ||
                ArePreconditionsOK())
            {

                // above can fail
                if (Status == Status.Active)
                {
                    //process the subgoals
                    // if an inner GoalHaul fails, all the goals will fail.
                    Status = ProcessSubgoals(elapsed);

                    if (GetsRecentlyHauledScoreBonus()) 
                    {
                        // after X seconds
                        // set the timepoint every update
                        SetRecentlyHauledOnAllItems();
                      

                    }  
                }
            }
            else
            {
                Status = Status.Failed;
            }
            
            // move this block to OnExit??
            if (Status == Status.Completed)
            {
                // very important!
                job.MarkClientAsInputNotEnroute();
                DestroyJobAndResetThreatstance();

                // let other people use the vehicle now:
                FreeUpVehicle();
            }
            else if (Status == Status.Failed)
            {
                job.MarkClientAsInputNotEnroute();

                // the job is on offer again:    why not call AbandonJob??? Deactivate() would call it
              //  job.Abandon(entity);

                FreeUpVehicle();
            }


        }

        public override string ToString()
        {
            string JobToString;
            if (job != null)
            {
                JobToString = job.ToString();
            }
            else
            {
                JobToString = "Job Has Been Destroyed";
            }
            return string.Format("{0} {1}", base.ToString(), JobToString);
        }


        public override string GetStatus()
        {
            return "Hauling";
        }

        private void AbandonJob()
        {           
            if (!isCancelled)
            {
               
                // if we are terminated, drop the item if still carried:
                // but not if we are cancelling/switching the goal ourselves, and need the spear for attacking
                if (job != null)
                {

                    if (job.TakenBy.Count > 1)
                    {
                        throw new Exception("more than one taker? that's wrong");
                    }

                    if (job.TakenBy.Count > 0 && // there is also a bug where we are hanging on to a job which we abandoned... The Assert method should fire in debug...
                        !(job.TakenBy.Count > 0 && job.TakenBy.Get(0) != entity)) // don't set Item to null if this job has already been taken by someone else!!! this should not happen...
                    {

                        IKnownEntityData itemData;
                        EntityResult result = entityIntelligence.GetKnownData(job.Item.Value, out itemData); // can Item be null here, when cancelling nested haul goals?
                        if (result == EntityResult.SeenDirectly)
                        {
                            Entity item = (Entity)itemData;

                            if (!entityIntelligence.Memory.NeedsItemForSwitchedGoal(item.ID)) // if GoalAttack is the new goal, hold onto the spear... don't leak items...
                            {
                                entity.AgentStorage.Uncontain(item);
                            }
                        }

                        if (itemData != null && itemData.AssignedToJob == job.ID)
                        {
                            itemData.AssignedToJob = null;
                        }

                        HaulingJobAnyItemOfType anyJob = job as HaulingJobAnyItemOfType;
                        // the job is no longer associated with an item, if it was an AnyItem job.
                        if (anyJob != null && job.Item.HasValue)
                        {
                           
                            anyJob.AddLog(job.Item.Value.ToString() + " was set to null by " + entity.ToString());

                            job.Item = null;
                        }
                    }
                    

                    // let other people use the vehicle now:
                    FreeUpVehicle();

                    RemoveLocksFromJob(job); // calls job.Abandon()
                    
                    isCancelled = true;

                  //  job = null; // too early to set job null... is referenced at end of Process...
                }

                CancelNestedGoals();

            }

        }


        /// <summary>
        /// assert function
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public bool GoalHasJob(Job job)
        {
            if (this.job == job)
            {
                return true;
            }

            foreach (var item in Subgoals)
            {
                GoalHaul goalHaul = item as GoalHaul;
                if (goalHaul != null)
                {
                    if (goalHaul.GoalHasJob(job)) // it seems some of these goals' jobs are not assigned/taken by the entity holding them?
                    {
                        return true;
                    }
                }
            }

            foreach (var item in addedNestedGoals)
            {
                GoalHaul goalHaul = item as GoalHaul;
                if (goalHaul != null)
                {
                    if (goalHaul.GoalHasJob(job))
                    {
                        return true;
                    }
                }
            }

            foreach (var item in goalsToNest)
            {
                GoalHaul goalHaul = item as GoalHaul;
                if (goalHaul != null)
                {
                    if (goalHaul.GoalHasJob(job))
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        public override void Deactivate()
        {
            AbandonJob();

            /* if (entity.ToString().Contains("Yeboah"))
             {

             }*/

            // OLD:

            if (job != null)
            {
                //  RemoveLocksFromJob(job);

                if (IsOuterGoal)
                {
                    ResetThreatStance(job);
                }
            }

            //  CancelNestedGoals();

        }



        /// <summary>
        /// should only be called on the outer goal anyways...
        /// </summary>
        /// <returns></returns>
        public override bool RequiresBoldStance()
        {
            return job.RequiresBoldStance;

        }



        private void CancelNestedGoals()
        {
            if (goalsToNest != null && goalsToNest.Count > 0)
            {
                foreach (GoalHaul nestedGoal in goalsToNest)
                {
                    nestedGoal.AbandonJob();

                    nestedGoal.RemoveIDEntry();
                }

                goalsToNest.Clear();
            }

            if (addedNestedGoals != null)
            {
                foreach (var item in addedNestedGoals)
                {
                    if (!item.isActivated)
                    {
                        item.AbandonJob();
                    }

                    item.RemoveIDEntry();
                }

                addedNestedGoals.Clear();
            }
        }

        private void FreeUpVehicle()
        {
            // let other people use the vehicle now:
            if (vehicleID != null && IsOuterGoal)
            {
                IKnownEntityData data;
                EntityResult result = entityIntelligence.GetKnownData(vehicleID.Value, out data);

                if (data != null)
                {
                    entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(data.EntityID, entity.EntityID);   
                }
            }

        }

        public override bool HandleMessage(Message message)
        {
            //first, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            //if the msg was not handled, test to see if this goal can handle it
            if (handled == false
                || message.MessageType == Message.MessageTypes.CancelJobOrItemInUse
                || message.MessageType == Message.MessageTypes.CancelJobForAIReset)
            {
                switch (message.MessageType)
                {
                    // someone wants us to stop doing this job:
                    case Message.MessageTypes.CancelJobOrItemInUse:
                    case Message.MessageTypes.CancelJobForAIReset:

                        Status = Status.Failed;

                        // Let's do all the cleanup in Deactivate. It will be called right after this.
                        /*
                        AbandonJob();
                        CancelNestedGoals(); 
                        */

                        // OLD:
                        /*
                        if (goalsToNest != null && goalsToNest.Count > 0)
                        {
                            foreach (GoalHaul nestedGoal in goalsToNest)
                            {
                                nestedGoal.AbandonJob();
                            }
                        }*/

                        return true; //msg handled

                    case Message.MessageTypes.DistanceFound:
                    case Message.MessageTypes.DistanceFoundNoAccess:
                        if (isFindingExtraJobs)
                        {
                            isFindingExtraJobs = false;

                            IKnownEntityData itemData;
                            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Item.Value, out itemData)))
                            {
                                return true;
                            }

                            IKnownEntityData vehicleData = null;
                            if (vehicleID != null)
                            {
                                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(vehicleID.Value, out vehicleData)))
                                {
                                    return true;
                                }
                            }

                            // clear any Wait goal:
                            RemoveAllSubgoals();

                            // find the jobs in Activate instead...
                            /*    ExtraCargoResult result = FindExtraCargo(itemData, vehicleData);

                                if (result == ExtraCargoResult.Wait)
                                {
                                    return true; // wait again...´?
                                }
                                else
                                {
                                    DropItemsOverCapacity(itemData);
                                }

                                UnfoldGoal(itemData);
                                */

                            return true;
                        }

                        return false;
                    default: return false;
                }
            }
            else
            {
                return true;
            }
        }


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.snapshotJob = sn.SnapshotID<Job, JobID>(job);

            this.NewOwner = sn.DoEnumNullable(NewOwner);
            this.ownerOfJobsID = sn.DoEnum(ownerOfJobsID);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            if (goalsToNest != null)
            {
                //snapshotGoalsToNest = new Queue<GoalID>(goalsToNest.Select(g => g.ID));
                if (snapshotGoalsToNest == null)
                {
                    this.snapshotGoalsToNest = new Queue<GoalID>();
                }

                // NEW HACK: filter invalid goals...
                foreach (var goal in goalsToNest)
                {
                    if (goal.ID != GoalID.Invalid)
                    {
                        snapshotGoalsToNest.Enqueue(goal.ID);
                    }
                }
            }

            this.snapshotGoalsToNest = sn.DoQueue(snapshotGoalsToNest);

            if (addedNestedGoals != null)
            {
                // OLD : //snapshotAddedNestedGoals = addedNestedGoals.Select(g => g.ID).ToList();
                
                //*HACK*// 2014-10-27
                //This is an fix that will resolve an error that only shows up in load.
                //This is most likley a bug on the game side but currently it does not seem to affect the game.
                //the error is that the nestedgoal list contains invalid IDS.

                //Changed this so it will not add Invalid IDS as the game can have these in the snapshotAddedNestedGoals.
                //But it would remove these during cleanups.
                if (snapshotAddedNestedGoals == null)
                {
                    this.snapshotAddedNestedGoals = new List<GoalID>();
                }
                this.snapshotAddedNestedGoals.Clear();
                foreach (var snapShotNestedGoal in addedNestedGoals)
                {
                    if (snapShotNestedGoal.ID != GoalID.Invalid)
                    {
                        this.snapshotAddedNestedGoals.Add(snapShotNestedGoal.ID);
                    }
                }              
            }

            this.snapshotAddedNestedGoals = sn.DoList(snapshotAddedNestedGoals);

            this.snapshotParent = sn.SnapshotID<Goal, GoalID>(ParentGoal);
            this.vehicleID = sn.DoEntityIDNullable(vehicleID);
            //   this.DestinationPiledItemJobs = (List<HaulingJob>)sn.DoList(DestinationPiledItemJobs); // TODO
            //  this.SourcePiledItemJobs = sn.DoList(SourcePiledItemJobs);  // TODO
            this.DestinationPileIsHandled = sn.DoBool(DestinationPileIsHandled);
            this.SourcePileIsHandled = sn.DoBool(SourcePileIsHandled);
            this.isFindingExtraJobs = sn.DoBool(isFindingExtraJobs);
            this.IsOuterGoal = sn.DoBool(IsOuterGoal);
            this.IsStandingBentOverItem = sn.DoBool(IsStandingBentOverItem);
            this.isCancelled = sn.DoBool(isCancelled);
            this.itemToHaul = sn.DoEntityID(itemToHaul);
            this.isActivated = sn.DoBool(isActivated);

            sn.Postpone(snapshotDestinationPiledItemJobs);

            sn.Ignore(job);
            sn.Ignore(goalsToNest);
            sn.Ignore(addedNestedGoals);
            sn.Ignore(DestinationPiledItemJobs);
            sn.Ignore(SourcePiledItemJobs);
            sn.Ignore(ParentGoal);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            if (snapshotGoalsToNest != null)
            {
                goalsToNest = new Queue<GoalHaul>(snapshotGoalsToNest.Select(g => (GoalHaul)LookUpGoals.FindByID(g)));
            }
            snapshotGoalsToNest.Clear(); // remember to clear/set to null for the next save

            if (snapshotAddedNestedGoals != null)
            {
                foreach (var gg in snapshotAddedNestedGoals)
                {
                    if (gg == GoalID.Invalid)
                    {
                        int i = 0;
                    }
                }
                addedNestedGoals = snapshotAddedNestedGoals.Select(g => (GoalHaul)LookUpGoals.FindByID(g)).ToList();
            }
            snapshotAddedNestedGoals.Clear(); // remember to clear/set to null for the next save

            if (snapshotParent.HasValue)
            {
                ParentGoal = (GoalHaul)LookUpGoals.FindByID(snapshotParent.Value);
            }
            if (snapshotJob != null)
            {
                job = (HaulingJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }

        }

        #endregion

    }
}
