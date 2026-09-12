using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Items;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    public class GoalHarvest : CompositeGoal, ITopLevelGoal
    {
        public ProcessJob harvestJob;
        JobID? snapshotJob;

        public List<ProcessJob> queuedJobs = new List<ProcessJob>();
        List<JobID> snapshotQueuedJobs;
        

        public OwnerID? OwnerOfHarvest;

        public List<EntityID> Tools = new List<EntityID>();

        public double TimeSpentInTopLevelGoal { get; set; }

        /// <summary>
        /// has productivity info
        /// </summary>
        public ToolTypeCombination ToolTypeCombination;
        ToolTypeCombinationID? snapshotToolCombo;

        Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools;

        public GoalHarvest(Entity entity, ProcessJob job, OwnerID? ownerOfHarvest, List<EntityGroupID> ownersOfVehicles,
            List<EntityID> tools,
            Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools,
            ToolTypeCombination toolTypeCombination)
            : base(entity)
        {

            
             this.harvestJob = job;
             this.ownersOfVehicles = ownersOfVehicles;
             this.OwnerOfHarvest = ownerOfHarvest;

             this.Tools = tools;
             this.ToolTypeCombination = toolTypeCombination;
             this.replenishItemsForTools = replenishItemsForTools;

           
             // new:
         //   this.resourceItem = item;
           // item.TargetedForHarvestingBy = entity;

          /*   container.TargetedForHarvestingBy = owner;

             this.container = container;
            */
        }

       
        private void ClaimAdditionalHarvestJobs()
        {           
            EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(harvestJob.HarvestJob.Owner); //  LookUpEntityGroups

            if (owner == null)
            {
                Status = Goals.Status.Failed;
                return;
            }

            List<ProcessJob> jobsOfType = owner.ProductionJobs[harvestJob.HarvestJob.ResourceType.ResourceItemType];

            // see how many items we can harvest from the same container:
            int noOfHarvestableCropItems = harvestJob.HarvestJob.Item.Container.NoOfHarvestableItems; // container.NoOfHarvestableItems;

            if (noOfHarvestableCropItems > 1) // we already claimed one...
            {
                // first claim jobs for specific item in this container:
                foreach (Job prospectiveJob in jobsOfType)
                {
                    ProcessJob proposedHarvestJob = prospectiveJob as ProcessJob;
                    if (
                        proposedHarvestJob != harvestJob
                        && proposedHarvestJob != null
                        && proposedHarvestJob.HarvestJob != null
                        && proposedHarvestJob.HarvestJob.CanBeQueuedInGoal() //  proposedHarvestJob.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded //RequiresWork // can't chain autojobs...
                        && proposedHarvestJob.TakenBy.Count < proposedHarvestJob.MaxJobPositions
                        && proposedHarvestJob.HarvestJob.IsSpecificJob == true
                        && proposedHarvestJob.HarvestJob.Item != null
                        && harvestJob.HarvestJob.Item.Container.ResourceItems.Contains(proposedHarvestJob.HarvestJob.Item))
                    {
                        // the job gets claimed here!!!
                        proposedHarvestJob.TakeJob(entity);
                       
                        //proposedHarvestJob.HarvestJob.Item.TargetedForHarvestingBy = entity;

                        queuedJobs.Add(proposedHarvestJob);

                        noOfHarvestableCropItems--;

                        if (noOfHarvestableCropItems <= 1)
                        {
                            return;
                        }
                    }
                }

             
                // the general ones:

                // claim a job for each!
                // TODO: also find harvestable items and assign them to each job we claim!!!
               /* foreach (Job prospectiveJob in jobsOfType)
                {
                    ProcessJob proposedHarvestJob = prospectiveJob as ProcessJob;

                    if (proposedHarvestJob != null 
                        && proposedHarvestJob.HarvestJob != null 
                        && proposedHarvestJob.TakenBy.Count < proposedHarvestJob.MaxJobPositions
                        && proposedHarvestJob.HarvestJob.IsSpecificJob == false
                        && proposedHarvestJob.HarvestJob.Item == null) // find unassigned general harvest jobs
                    {
                        // the job gets claimed here!!!
                        proposedHarvestJob.TakenBy.Add(entity);
                        proposedHarvestJob.HarvestJob.Item.AssignedToJob = proposedHarvestJob; // also claim the item!

                        queuedJobs.Add(proposedHarvestJob);

                        noOfHarvestableCropItems--;

                        if (noOfHarvestableCropItems <= 1)
                        {
                            return;
                        }
                    }
                }*/
            }
        }



        /* 
         /// <summary>
        /// perhaps add these later! to harvest adjacent tiles, and other crop trees in the same tile.
        /// </summary>
        private void FindNextCropToHarvest()
        {
            Crop centerCrop = FindFreeCropOnTile(entity.MapPosition);
            if (centerCrop != null)
            {
               
            }
        }

        /// <summary>
        /// perhaps add these later! to harvest adjacent tiles, and other crop trees in the same tile.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
       private Crop FindFreeCropOnTile(Point pos)
        {
            TerrainTile tile = UWGame.SimSide.Instance.Map.TileMap[pos.X][pos.Y];

            Crop examinedCrop;
            if (harvestJob.HarvestJob.CropType.Parent is TreeType)
            {
                
                Tree tree;
                foreach (Entity treeEntity in tile.TreesOnTile)
                {
                    treeEntity.Find(out tree);
                    if (tree.Crops.TryGetValue(harvestJob.HarvestJob.CropType, out examinedCrop))
                    {
                        if (examinedCrop.GetRipeItem() != null)
                        {
                            return examinedCrop;
                        }
                    }
                }
            }
            else
            {
                Terrain terrain;
                if (tile.Terrain != null)
                {
                    terrain = tile.Terrain;

                    examinedCrop = FindFreeCropOnTerrain(terrain);

                    if (examinedCrop != null)
                    {
                        return examinedCrop;
                    }
                }
                else
                {
                    for (int sx = 0; sx < 3; sx++)
                    {
                        for (int sy = 0; sy < 3; sy++)
                        {
                            terrain = tile.TerrainSubtiles[sx][sy];

                            examinedCrop = FindFreeCropOnTerrain(terrain);

                            if (examinedCrop != null)
                            {
                                return examinedCrop;
                            }
                        }
                    }
                }                
            }

            return null;
        }

        private Crop FindFreeCropOnTerrain(Terrain terrain)
        {
            Crop examinedCrop;
            foreach (KeyValuePair<string, LowVegetation> kvp in terrain.Vegetation)
            {
                examinedCrop = kvp.Value.Crop;
                if ( examinedCrop.GetRipeItem() != null) // TODO if method is needed
                {
                    return examinedCrop;
                }
            }

            return null;
        }
        */

       

        public GoalHarvest()
        {
        }


        protected override void Activate()
        {
           
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            // wait a bit, to see if we are asked to cancel by other agents.
            AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));
            
            // LOCKS
            
            harvestJob.TakeJob(entity);

            SetLocksOnReplenishItems(harvestJob, replenishItemsForTools);          

            //////
           Vector3 destination;
           Entity tree;
           GatheringSite site;
           GetTreeAndGatheringSite(out tree, out site);
         
           if (tree != null)
           {
               destination = tree.AccessPoint.Value;
           }
           else
           {
               destination = harvestJob.HarvestJob.Item.Container.AccessPoint;
           }

           List<ItemType.TaskType> gearTasks = null;
           AddNightActivityGear(ref gearTasks);

           FindOptionalEquipmentIfNeeded(destination, harvestJob, taskTypesForGear: gearTasks); 


            List<IKnownEntityData> toolsData = null;
            if (!ResolveTools(Tools, ref toolsData))
            {
                return;
            }

            // gather tools:
            if (!GatherToolsOrWeapons(toolsData, ownersOfVehicles, harvestJob, false, StorageCompartment.Haul))
            {
                return;
            }

            ReplenishToolsOrWeapons(replenishItemsForTools, ownersOfVehicles, harvestJob);


            // register as harvester to keep others out of the way...
            SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;
            sharedKnowledge.PlaySiteKnowledge.AddCropHarvester(harvestJob.HarvestJob.ResourceType.ResourceItemType, entity, harvestJob.HarvestJob.Item.Container); // container);
            


            float resourceItemBulk = harvestJob.HarvestJob.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value;
            float totalDropped;
            DropUnneededItemsToMakeCapacity(resourceItemBulk, e => !Tools.Contains(e.EntityID), out totalDropped, StorageCompartment.Haul);
                      
            
            
            if (site != null && tree != null)
            {
                if (site.CanAddVisitor(ref entity))
                {
                    AddSubgoal(new GoalArriveAsVisitor(entity, tree.EntityID, ownersOfVehicles));
                    PlaceStationaryToolsAtWorkSite(toolsData, null); // drop at whatever location we arrive at
                }
                else
                {
                    Status = Status.Failed;
                    return;
                }
            }
            else
            {
                destination = harvestJob.HarvestJob.Item.Container.AccessPoint;
                AddSubgoal(new GoalMoveToPosition(entity, destination, ownersOfVehicles) { IsFinalDestination = true }); // do this everywhere else!!!

                PlaceStationaryToolsAtWorkSite(toolsData, destination);
            }

           

            if (entity.HasStance())
            {
                ChangeStance(entity.Locomotor.Stance.PickRandomProcessStance(harvestJob.ProcessType.StanceTypes));
            }

            AddSubgoal(new GoalDoProduce(entity, harvestJob, OwnerOfHarvest, Tools, ToolTypeCombination));                   
         //   AddSubgoal(new GoalDoHarvest(entity, harvestJob, Tools, ToolTypeCombination) { OwnerOfHarvest = this.OwnerOfHarvest });

            ClaimAdditionalHarvestJobs();


            // move this to Evaluator instead? At least an accessibility test...
         /*   if (job.GetWorkLocation(entity, out tile, out tileCenterOffset))
            {
                // wait a bit, to see if we are asked to cancel by other agents.
                AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));

                // move to our assigned work location
                // AddSubgoal(new GoalMoveToPosition(entity, job.BuildingToConstruct.FrontDoorTilePos, GoalMoveToPosition.VehicleUse.FreeUpAfterUse, ownersOfVehicles));                 
                AddSubgoal(new GoalMoveToPosition(entity, tile, tileCenterOffset, ownersOfVehicles));

                AddSubgoal(new GoalTurnToFace(entity, new Vector2(job.BuildingToConstruct.Location.X, job.BuildingToConstruct.Location.Y)));

                // start constructing
                AddSubgoal(new GoalDoConstruct(entity, job));
            }
            else
            {
                // failed to find a work location.
                Status = Status.Failed;
            }*/

        }

        private void GetTreeAndGatheringSite(out Entity treeEntity, out GatheringSite site)
        {
            site = null;
            treeEntity = null;
            CropItem cropItem = harvestJob.HarvestJob.Item as CropItem;
            if (cropItem != null)
            {
                Tree tree = cropItem.crop.Parent as Tree;
                if (tree != null)
                {
                    site = (tree.Parent).GatheringSite;
                    treeEntity = tree.Parent;
                }
            }
        }

        public override bool RequiresBoldStance()
        {
            return harvestJob.RequiresBoldStance;

        }

        protected override bool ArePreconditionsOK()
        {
            if (!AreToolsOK(Tools))
                return false;

            return harvestJob.HarvestJob.Item.AssignedToJob == harvestJob // resourceItem.TargetedForHarvestingBy == entity 
                && !harvestJob.HarvestJob.Item.Container.IsDestroyed(entityIntelligence.Allegiance.SharedKnowledge); 

        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {

            // preconditions!               
            if (!preconditionsRegulator.IsReady() ||
                ArePreconditionsOK())
            {
                //process the subgoals
                Status = ProcessSubgoals(elapsed);
            }
            else
            {
                Status = Status.Failed; // Status.Completed; 
            }

            if (Status == Status.Completed)
            {   // very important!

                SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;
                sharedKnowledge.PlaySiteKnowledge.RemoveCropHarvester(harvestJob.HarvestJob.ResourceType.ResourceItemType, entity);

                Entity tree;
                GatheringSite site;
                GetTreeAndGatheringSite(out tree, out site);
                if (site != null) // && tree != null)
                {
                    site.RemoveVisitor(entity.EntityID);
                }

               
                // won't this remove AssignedToJob on the tool, and also InUseBy... dangerous when we want to keep using it? No, we reset the locks right after.            
                RemoveProcessToolLocks(harvestJob, Tools, replenishItemsForTools);

               
              //  DestroyJobAndRemoveLocks(ref harvestJob, Tools, replenishItemsForTools);
               
                // now we want to harvest the other items from this crop!
                if (queuedJobs.Count > 0)
                {
                    // continue with another harvest job
                    harvestJob = queuedJobs[0];
                    queuedJobs.RemoveAt(0);

                    // drop harvested crop items if we're full - they will be left in a neat pile and can then be located by the job manager and efficiently hauled to their destination...

                    float totalDropped;
                    DropUnneededItemsToMakeCapacity(harvestJob.HarvestJob.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value,
                                                    e => e.EntityType == harvestJob.HarvestJob.ResourceType.ResourceItemType,
                                                    out totalDropped, StorageCompartment.Haul);

                    // NEW:
                    // set locks on tools for the new job ??
                   // SetLocksOnReplenishItems(harvestJob, replenishItemsForTools);  // we should have done all replenishing at this point.

                    SetLocksOnToolsOrWeapons(Tools, harvestJob);

                    AddSubgoal(new GoalDoProduce(entity, harvestJob, OwnerOfHarvest, Tools, ToolTypeCombination));
                    //AddSubgoal(new GoalDoHarvest(entity, harvestJob, Tools, ToolTypeCombination) { OwnerOfHarvest = this.OwnerOfHarvest });

                    Status = Goals.Status.Active; // Continue!
                }

            }
            else if (Status == Status.Failed)
            {   // we don't want it anymore...
                AbandonJobs();
            }


        }


        public override bool HandleMessage(Message message)
        {
            //first, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            //if the msg was not handled, test to see if this goal can handle it
            if (handled == false)
            {
                switch (message.MessageType)
                {
                    // someone wants us to stop doing this job:
                    case Message.MessageTypes.CancelJobOrItemInUse:
                    case Message.MessageTypes.CancelJobForAIReset:

                        Status = Status.Failed;

                        AbandonJobs();

                        return true; //msg handled

                    /*case Message.MessageTypes.Hit:
                        {
                            RemoveAllSubgoals();

                            return false;
                        }*/
                    default: return false;
                }
            }
            else
            {
                return true;
            }
        }


        private void AbandonJobs()
        {
            harvestJob.Abandon(entity);

            foreach (Job queuedJob in queuedJobs)
            {   // ditch these too...
                queuedJob.Abandon(entity);
            }
        }

        /*
        private void DropItemsOfType(Entity entity, EntityType itemType)
        {
            //SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
            EntityID id;
            IKnownEntityData itemData;
            EntityResult result;
            for (int i = entity.AgentStorage.ItemStorage.StoredItems.Count - 1; i >= 0; i--)
            {
                id = entity.AgentStorage.ItemStorage.StoredItems[i];
                result = entityIntelligence.GetKnownData(id, out itemData);

                if (result == EntityResult.SeenDirectly)
                {
                    if (itemData.EntityType == itemType)
                    {
                        AddSubgoal(new GoalDropItem(entity, id, null, null));
                    }
                }
                else
                {
                    entity.AgentStorage.ItemStorage.RemoveOutdatedItem(id);
                }
            }
            
        }
        */
        public override string GetStatus()
        {
            return "Harvesting";
        }

        public double ScoreGoal()
        {
           // return ScoreJobGoal(harvestJob);         
            ToolParams toolParams = new ToolParams()
            {
                Tools = this.Tools,
                ReplenishStatus = null,
                JobDurationInDays = null,
                ToolProductivity = GoalProduce.GetToolCombinationProductivity(ToolTypeCombination), 
            };
                       

            return ScoreJobGoal(harvestJob, toolParams);  
        }

        public override bool IsSame(Jobs.Job job)
        {
            return job == this.harvestJob;

           
        }

        public override void Deactivate()
        {
            if (harvestJob != null)
            {
                // unregister as harvester...
                SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;

                sharedKnowledge.PlaySiteKnowledge.RemoveCropHarvester(harvestJob.HarvestJob.ResourceType.ResourceItemType, entity);

                             
                RemoveProcessToolLocks(harvestJob, Tools, replenishItemsForTools);

               // RemoveLocksFromJob(harvestJob, Tools, replenishItemsForTools);

                AbandonJobs();

                //ReleaseLocksOnTools(Tools, harvestJob);

                //// de-assign replenish items:
                //AssignReplenishItems(harvestJob, replenishItemsForTools, false);

                Entity tree;
                GatheringSite site;
                GetTreeAndGatheringSite(out tree, out site);
                if (site != null) // && tree != null)
                {
                    site.RemoveVisitor(entity.EntityID);
                }
            }

        }


        /// <summary>
        /// USEFUL??? this method can be blocked since it uses the entity's threat stance!
        /// </summary>
        /// <param name="dMap"></param>
        /// <param name="entity"></param>
        /// <param name="discomfortBelowValue"></param>
        /// <returns></returns>
        /*    public static List<AI.Pathfinding.PathFinderNode> FindPathToComfort(DiscomfortMap dMap, Entity entity, byte discomfortBelowValue)
            {
                // we can only pass one argument to the function - encapsulate them in a struct.
                AStarSearch.DijkstraTestNodeDelegate findComfortableTileDelegate =
                    (AStarSearch.DijkstraTestNodeDelegate)Delegate.CreateDelegate(typeof(AStarSearch.DijkstraTestNodeDelegate),
                    new Map.InfluenceMap.DiscomfortTileIsBelowValueParameters(dMap, discomfortBelowValue),
                    UWGame.SimSide.Instance.Map.InfluenceMapTileIsComfortableInfo);

                Intelligence intelligenceComponent = entity.Intelligence;

                // use entity's current stance to find the nearest comfortable tile:
                return intelligenceComponent.PathPlanner.FindItemAndGetPath(
                    UWGame.SimSide.Instance.Map.GetMovementMap(intelligenceComponent.ProtectionLevel,
                    entity.EntityType.ThreatCategory, intelligenceComponent.ThreatStance).Map[entity.GetTransportType()],
                    findComfortableTileDelegate, 200, entity.MapPosition);

            }*/


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

            this.snapshotJob = sn.SnapshotID<Job, JobID>(harvestJob);

            if (queuedJobs != null)
            {
                snapshotQueuedJobs = queuedJobs.Select(j => j.ID).ToList();
            }
            this.snapshotQueuedJobs = sn.DoList(snapshotQueuedJobs);
            this.OwnerOfHarvest = sn.DoEnumNullable(OwnerOfHarvest);
            this.Tools = sn.DoList(Tools);
            this.replenishItemsForTools = sn.DoMultiMap(replenishItemsForTools);
            this.snapshotToolCombo = sn.SnapshotID<ToolTypeCombination, ToolTypeCombinationID>(ToolTypeCombination);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            sn.Ignore(queuedJobs);
            sn.Ignore(harvestJob);
            sn.Ignore(ToolTypeCombination);


            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            if (snapshotQueuedJobs != null)
            {
                queuedJobs = snapshotQueuedJobs.Select(j => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList();
            }
            snapshotQueuedJobs.Clear(); // remember to clear/set to null for the next save

           // harvestJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);

            if (snapshotJob != null)
            {
                harvestJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }

            ToolTypeCombination = LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(snapshotToolCombo);

        }

        #endregion
    }
}
