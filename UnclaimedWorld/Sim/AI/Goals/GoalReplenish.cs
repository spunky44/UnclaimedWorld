using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using Xclna.Xna.Animation;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// i imagine this goal can be used in several replenish actions, such as reload, add fuel, add recharged power cells  
    /// 
    /// not used for replenishing jobs!!!
    /// 
    /// </summary>
    public class GoalReplenish: CompositeGoal
    {
      
        /// <summary>
        /// will be needed until replenish items are fully merged with substances...
        /// </summary>
        public enum ReplenishAction { Reload, Refuel, Recharge }


        private List<EntityID> replenishItems;

       // private EntityID entityToReplenish;
        private EntityAndRoot entityToReplenish;

        /// <summary>
        /// this should be the same compartment as is used for the weapon. Equipment for optional weapons, Haul for tools...
        /// </summary>
        private StorageCompartment compartmentToUse;

        Job job;
        JobID snapshotJob;

        public GoalReplenish(Entity entity,
            EntityAndRoot /*EntityID*/ entityToReplenish, 
            List<EntityID> replenishItems, ProcessType processType, List<EntityGroupID> ownersOfVehicles, Job job, StorageCompartment compartment)
            : base(entity)
        {
            this.entityToReplenish = entityToReplenish;
            this.replenishItems = replenishItems;
            this.ownersOfVehicles = ownersOfVehicles;
            this.compartmentToUse = compartment;

          //  this.action = processType;

            this.job = job;
        }


       

        public GoalReplenish()
        {
        }

        protected override void Activate()
        {
            //make sure the subgoal list is clear.
            RemoveAllSubgoals();
                            
            Status = Status.Active;

            IKnownEntityData entityToReplenishData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entityToReplenish.Entity, out entityToReplenishData)))
            {
                return;
            }
            
           
            List<IKnownEntityData> replenishItemData = new List<IKnownEntityData>();

            // calculate how much space we need:
            float bulkOfAllReplenishItems = 0.0f;

            float roundTripCapacity = entity.AgentStorage.ItemStorage.UnusedCapacity;


            foreach (var item in replenishItems)
            {
                IKnownEntityData itemData;
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(item, out itemData)))
                {
                    return;
                }

                replenishItemData.Add(itemData);

                if (entity.AgentStorage.Contains(itemData.EntityID))
                {
                    roundTripCapacity += itemData.Bulk; // already carrying, does not count against capacity
                }

                bulkOfAllReplenishItems += itemData.Bulk;
            }

           
            float totalStorageNeeds = entity.AgentStorage.ItemStorage.TotalStored + bulkOfAllReplenishItems; // not accurate! shouldn't count carried items
            if (Common.IsGreaterThan(totalStorageNeeds, entity.AgentStorage.ItemStorage.TotalCapacity)) // we need to drop something...
            {
                float totalDropped;
                // make room if needed, and check the correct compartment.
                DropUnneededItemsToMakeCapacity(bulkOfAllReplenishItems, (e => e.AssignedToJob != job.ID), out totalDropped, compartmentToUse);
             //   DropUnneededItemsToMakeCapacity(bulkOfAllReplenishItems, (e => e.AssignedToJob != job), out totalDropped, compartmentToUse);

                roundTripCapacity += totalDropped;               
            }

            // make several trips if needed:
            // start with items already carried
            for (int i = 0; i < replenishItemData.Count; i++)
            {
                IKnownEntityData item = replenishItemData[i];
                if (entity.AgentStorage.Contains(item.EntityID) && i > 0)
                {
                    replenishItemData.RemoveAt(i);
                    replenishItemData.Insert(0, item); // move to start of list
                   // i++;
                }
            }

            /*
            for (int i = replenishItemData.Count - 1; i >= 0; i--)
            {
                IKnownEntityData item = replenishItemData[i];
                if (entity.AgentStorage.Contains(item.EntityID) && i > 0)
                {
                    replenishItemData.RemoveAt(i);
                    replenishItemData.Insert(0, item); // move to start of list
                }
            }*/

            float bulkOfItemsCarriedThisTrip = 0f;
            List<IKnownEntityData> itemsCarriedInTrip = new List<IKnownEntityData>();
            foreach (var itemData in replenishItemData)
            {               
                // pick up the item to replenish with if we are not carrying it:
                if (!entity.AgentStorage.Contains(itemData.EntityID))
                {                   

                    if (bulkOfItemsCarriedThisTrip + itemData.Bulk > roundTripCapacity)
                    {
                        // we need to drop off what we are currently carrying first:

                        AddSubgoal(new GoalMoveToPosition(entity, entityToReplenishData.AccessPoint.Value, ownersOfVehicles) { IsFinalDestination = false });

                       // AddSubgoal(new GoalTurnToFace(entity, entityToReplenishData.Location.ToVector2()));

                        foreach (var item in itemsCarriedInTrip)
	                    {
                            // there is no job for the replenish process, so don't assign to job... instead, the item is assigned to the process directly, this gets done in GoalDoProduce instead
                            AddSubgoal(new GoalDropItem(entity, item.EntityID, null, entityToReplenishData.AccessPoint, null)); 
                        }

                        itemsCarriedInTrip.Clear();                        
                        bulkOfItemsCarriedThisTrip = 0f;
                    }

                    bulkOfItemsCarriedThisTrip += itemData.Bulk;
                    itemsCarriedInTrip.Add(itemData);

                    AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, itemData) { IsFinalDestination = false });

                    entity.Intelligence.Allegiance.SharedKnowledge.SetInUseBy(itemData.EntityID, entity.EntityID);
                    

                    if (!PickupItemOrUnloadFirst(itemData, false, compartment: compartmentToUse))
                    {
                        return;
                    }
                }
                else
                {
                    bulkOfItemsCarriedThisTrip += itemData.Bulk;
                    itemsCarriedInTrip.Add(itemData);
                }
            }

            // either carry the entity to be replenished along, or carry back the replenish items to its location:
            if (!entity.AgentStorage.Contains(entityToReplenish.Entity))
            {
                AddSubgoal(new GoalMoveToPosition(entity, entityToReplenishData.AccessPoint.Value, ownersOfVehicles) { IsFinalDestination = true });

                AddSubgoal(new GoalTurnToFace(entity, entityToReplenishData.Location.Value.ToVector2()));
            }


            foreach (var item in replenishItemData)
            {
               
                AddSubgoal(new GoalDoProduce(entity, 
                    entityToReplenishData.EntityType.ContainerType.GetReplenishProcesses()[item.EntityType], // action, 
                    entityToReplenishData.OwnedBy, item, compartmentToUse, entityToReplenishData.AccessPoint, entityToReplenish));
            }
             
        }


        public static void Replenish(Entity worker, GoalReplenish.ReplenishAction action,
           Entity entityToReplenishWith, Entity entityToReplenish, IOwner entityToReplenishOwner, StorageCompartment compartment)
        {

            switch (action)
            {
                case GoalReplenish.ReplenishAction.Refuel:
                    {
                        Container replenishContainer = entityToReplenish.Contains;
                        replenishContainer.AddToContain(entityToReplenishWith, replenish: true); // TODO: modify VehicleContainer to place the item in the correct subcontainer

                        ((IHasReplenishItems)replenishContainer).ReplenishItems.RequiresFuel.Refuel(entityToReplenishWith);

                        /*   entityToReplenish.Find(out energy);

                           energy.RequiresFuel.Refuel(entityToReplenishWith);*/
                        break;
                    }
                case GoalReplenish.ReplenishAction.Reload:
                    {
                        Container magazine = entityToReplenish.Contains;
                        Entity surplusAmmo;


                        magazine.AddToContain(entityToReplenishWith, out surplusAmmo);

                        if (surplusAmmo != null)
                        {
                          
                            JobID? assignedToJobID = entityToReplenishWith.AssignedToJob;
                          
                            ProcessJob processJob = null;
                            EvaluateJob.ResolveAssignedToProcessJob(entityToReplenishWith, out processJob);
                          

                            if (worker != null &&
                                (processJob == null || processJob.ReplenishJob == null))
                            {
                                // assign the left over ammo to the same job (to keep in spare and avoid hauling it back to camp):
                                // do this before pickup to avoid creating a hauling job for the item
                                surplusAmmo.AssignedToJob = assignedToJobID;

                                // now we have to store a reference in order to de-assign when the goal ends... or search the inventory.                               
                            }

                            if (worker != null)
                            {
                                // the surplus ammo is in limbo, it must either be picked up or placed on the ground/inside a container...
                                if (!surplusAmmo.Item.Pickup(worker, entityToReplenishOwner, compartment))
                                {
                                    // de-assign again if no capacity.
                                    if (surplusAmmo.AssignedToJob == assignedToJobID)
                                    {
                                        surplusAmmo.AssignedToJob = null;
                                    }
                                }
                            }
                        }

                        if (worker != null)
                        {
                          /*  The.Client.AddLogEvent(worker.Intelligence.Allegiance, The.Client.Log.DebugEvent, worker, string.Format("reloaded {0}",
                                        entityToReplenish));*/
                        }

                        break;
                    }

            }
        }

        protected override bool ArePreconditionsOK()
        {
            for (int i = replenishItems.Count - 1; i >= 0; i--)
            {
                IKnownEntityData data;
                EntityID id = replenishItems[i];
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(id, out data)))
                {
                    return false;
                }
            }

            return true;         
        }


        protected override void ProcessWhileActive(GameTime elapsed)
        {
            if (!preconditionsRegulator.IsReady()
                || ArePreconditionsOK())
            {
                Status = ProcessSubgoals(elapsed);

                if (Status == Goals.Status.Failed)
                {
                    // throw new Exception();
                }
            }
            else
            {
                Status = Goals.Status.Failed;
            }

        }



        public override void Deactivate()
        {
            IKnownEntityData itemData;
            // release locks on replenish items:
            foreach (var item in replenishItems)
            {
                entityIntelligence.GetKnownData(item, out itemData);

                if (itemData != null)
                {
                    if (itemData.AssignedToJob == job.ID)
                    {
                        itemData.AssignedToJob = null;
                    }
                  
                    entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(itemData.EntityID, entity.EntityID);   
             
                }
            }

        }

        public override string GetStatus()
        {
            IKnownEntityData itemToReplenishData, replenishData;
            entityIntelligence.GetKnownData(entityToReplenish.Entity, out itemToReplenishData);

          //   IKnownEntityData data;
            EntityID id = replenishItems[0];
            entityIntelligence.GetKnownData(id, out replenishData);

            if (itemToReplenishData != null && replenishData != null)
            {
                return itemToReplenishData.EntityType.ContainerType.GetReplenishProcesses()[replenishData.EntityType].Name;
            }
            else return "";

            /* return action.Name; 
           
            switch (action)
            {
                case ReplenishAction.Refuel:
                    return "Refuelling";
                case ReplenishAction.Recharge:
                    return "Recharging";
                case ReplenishAction.Reload:
                    return "Reloading";
            }

            return "";*/
            
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

          //  this.action = sn.DoGameData(action);
            this.replenishItems = (List<EntityID>)sn.DoList(replenishItems);
            this.entityToReplenish = sn.DoEntityAndRoot(entityToReplenish); // (EntityID)sn.DoEnum(entityToReplenish);
            this.snapshotJob = (JobID)sn.SnapshotID<Job, JobID>(job);
            this.compartmentToUse = sn.DoEnum(compartmentToUse);

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            job = LookUp<Job, JobID>.FindByID(snapshotJob);
        }

        #endregion
     
    }
}
