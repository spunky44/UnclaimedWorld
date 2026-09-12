using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Items;
using UWGame.SimSide.Resources;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI.Constants;

namespace UWGame.SimSide.Jobs
{
    public class HarvestJob: ISnapshot
    {

        public ProcessJob ProcessJob;
        private JobID snapshotJob;

        /// <summary>
        /// DELETE THIS - JobManager can pick items also
        /// 
        /// Is the job pointing to a specific resource item that should be harvested?
        /// when true, the resource item is assigned immediately to this job, and not de-assigned when the job is abandoned
        /// </summary>
        public bool IsSpecificJob = false;


        /// <summary>
        /// this represents the 'item' that has not been created yet!
        /// 
        /// Assign it, even if the job is not a specific job, once a target has been found.
        /// </summary>
        public IResourceItem Item;
        private ResourceItemID snapshotItem;

        public ResourceType ResourceType;

        public EntityGroupID Owner;


        public Zone Zone;
        private ZoneID snapshotZone;

        public HarvestJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
    
        }

        /// <summary>
        /// general job set in the stock manager
        /// </summary>
        /// <param name="processJob"></param>
        /// <param name="resourceType"></param>
        /// <param name="owner"></param>
       /* public HarvestJob(ProcessJob processJob, ResourceType resourceType, EntityGroupID owner)
        //: base(addToList)
        {
            this.ProcessJob = processJob;
            this.ResourceType = resourceType;

            Owner = owner;

            IsSpecificJob = false;

        }*/


        /// <summary>
        /// specific job set on the map
        /// </summary>
        /// <param name="processJob"></param>
        /// <param name="item"></param>
        /// <param name="owner"></param>
        public HarvestJob(ProcessJob processJob, IResourceItem item, EntityGroupID owner)
        {
            this.ProcessJob = processJob;
            this.Item = item;

            this.ResourceType = item.Container.ResourceType;
            

            SimProcess process = SimProcess.FindById(ProcessJob.ProductionProcess);
            process.ResourceItem = this.Item.ID;

            IsSpecificJob = true;

            item.AssignedToJob = ProcessJob;

            Owner = owner;

            // store a reference on the tile for quick lookup
            TerrainTile tile = The.Map.GetTile(item.Container.MapPosition);

            if (tile.HarvestJobs == null)
            {
                tile.HarvestJobs = new Dictionary<EntityGroupID, Dictionary<ResourceType, List<ProcessJob>>>();
            }
            Dictionary<ResourceType, List<ProcessJob>> allOwnerJobs;
            if (!tile.HarvestJobs.TryGetValue(owner, out allOwnerJobs))
            {
                allOwnerJobs = new Dictionary<ResourceType, List<ProcessJob>>();
                tile.HarvestJobs.Add(owner, allOwnerJobs);
            }
            List<ProcessJob> jobsOfType;
            if (!allOwnerJobs.TryGetValue(ResourceType, out jobsOfType))
            {
                jobsOfType = new List<ProcessJob>();
                allOwnerJobs.Add(ResourceType, jobsOfType);
            }
            jobsOfType.Add(processJob);

        }



        public GoalEvaluator.CalculateResult ScoreThisJobWithoutTools(RegionMap regionMap, ThreatStance threatStanceToUse,
            Entity entity, Intelligence entityIntelligence, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution,
          
            ref double rating, ref IResourceItem currentResourceItem, EntityGroup owner, float priority, ref bool jobIsValid)
        {
            Person personEntity = entity.PersonEntity;

            ProcessType processType = ProcessJob.ProcessType;


            if (entityIntelligence.HasSkill(processType.RequiredSkillType))
            {
                bool isStarted;
                IKnownProcess processData;
                if (!ProcessJob.IsStarted(out isStarted, out processData))
                {
                    // destroys the job too.
                    jobIsValid = false;
                    rating = 0;
                    return GoalEvaluator.CalculateResult.Done;
                }
             
                if (isStarted && 
                    processType.WorkNeeded != WorkerNeededOptions.WorkerNeeded) // !processType.RequiresWork)
                {
                    rating = 0;
                    return GoalEvaluator.CalculateResult.Done;
                }

                if (IsSpecificJob && Item != null)
                {
                    // closestLocation = Item.Container.Location;
                    currentResourceItem = Item;

                    //Item.Container.GetClosestAccessibleHarvestLocation( , , out closestLocation);

                    /*
                    CropItem cropItem = Item as CropItem;
                    if (cropItem != null)
                    {
                        closestLocation = cropItem.crop.Location;
                    }
                    // else??
                    */
                }
                else
                {
                    // Find best resource item on influence/resource map!                                        

                    // Crop bestCrop = null;
                    currentResourceItem = null;
                    ResourceMapForAgent.Result locationResult = entity.Intelligence.CropsMapForAgent.GetBestHarvestLocation(this, ref currentResourceItem);
                    if (locationResult == ResourceMapForAgent.Result.Wait)
                    {
                        // wait for the result...
                        //rating = 0;
                        return GoalEvaluator.CalculateResult.Processing;
                    }
                    else if (locationResult == ResourceMapForAgent.Result.NoTarget
                        || currentResourceItem == null)
                    {
                        rating = 0;
                        return GoalEvaluator.CalculateResult.Done;
                    }

                    // got it!                     
                }
                // this is not too good... for non-specific jobs -  if we cannot reach this item, we should get another target!

              // location is scored later, same as for process jobs.
              /*  RegionMap.Result result = GoalEvaluator.ScoreTravelTime(regionMap, threatStanceToUse, entity.PlaySiteLocation, 
                    currentResourceItem.Container.AccessPoint, entity, ref travelTimeScore);

                if (result == RegionMap.Result.Wait)
                {
                    // wait for the result...
                    //rating = 0;
                    return GoalEvaluator.CalculateResult.Processing;
                }
                else if (result == RegionMap.Result.NoAccess)
                {
                    rating = 0;
                    return GoalEvaluator.CalculateResult.Done;
                }

               
*/

                float currentProgress;
                SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
                if (!processData.GetKnownProgress(sharedKnowledge, out currentProgress)) //, out processData))
                {
                    jobIsValid = false;
                    rating = 0;
                    return GoalEvaluator.CalculateResult.Done;
                }


                /*
                      double jobScore = // don't include travel score here, only after tools... some tools are immovable and will decide the job location.                        
                          0.30 * materialsEstimate + // why include this in score? always 0 or 1
                          0.15 * progressScore +
                          0.10 * marginalLaborScore +
                          0.10 * outputImportance +
                          0.25 * skillScore +
                          uniqueSkillWeight * uniqueSkillScore; // weight is 0.15

                 */
                double progressScore = GoalEvaluator.ScoreJobProgress(currentProgress);

                double marginalLaborScore = GoalEvaluator.ScoreNumberOfWorkers(proposedNumberOfWorkers, ProcessJob.MaxJobPositions);

                double uniqueSkillScore = GoalEvaluator.ScoreUniqueSkill(entityIntelligence, processType.RequiredSkillType);

                double outputImportance = ProcessJob.GetImportance(owner); // Importance ?? 0.5d; // ScoreOutputImportance(ownerOfInputItems);

                double skillScore = GoalEvaluator.ScoreSkill(entity.Intelligence.GetSkillValue(processType.RequiredSkillType));

                float uniqueSkillWeight = GameData.Instance.AIConstants.EvaluatorWeights.UniqueSkillWeight;


                /*
                   public float HarvestImportanceWeight = 0.1f;
                    public float HarvestSkillWeight = 0.25f;
                    public float HarvestProgressWeight = 0.15f;                 
                 */

                EvaluatorWeights weights = GameData.Instance.AIConstants.EvaluatorWeights;

                // rating should not be biased above normal production now that importance exists
                rating = // don't include travel score here
                         0.3 +
                         weights.HarvestProgressWeight * progressScore +  //0.15 
                         0.10 * marginalLaborScore +
                         weights.HarvestImportanceWeight * outputImportance + // 0.10 
                         weights.HarvestSkillWeight * skillScore + // 0.25 
                         uniqueSkillWeight * uniqueSkillScore; // weight is 0.15

              //  rating = 1.0 * skillScore;


                rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);

            }

            return GoalEvaluator.CalculateResult.Done;
        }

        public bool CanBeQueuedInGoal()
        {
            return ProcessJob.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded; //!ProcessJob.IsUnattended();
        }


        public void Destroy()
        {
            TerrainTile tile;

            if (Item != null)
            {
                Crop cropContainer = Item.Container as Crop;
                if (cropContainer != null && cropContainer.ParentEntity != null)
                {
                    cropContainer.ParentEntity.Renderable.SetResourceContainerTintColor(Color.White);
                }
                else
                {
                    TileResourceContainer container = Item.Container as TileResourceContainer;
                    if (container != null)
                    {
                        container.Renderable.SetResourceContainerTintColor(Color.White);
                    }
                }

                if (!IsSpecificJob || ProcessJob.HarvestJob != null)
                {
                    if (Item.AssignedToJob == ProcessJob)
                    {
                        Item.AssignedToJob = null;
                    }
                }

                // remove the reference on the tile:
                tile = The.Map.TileMap[Item.Container.MapPosition.X][Item.Container.MapPosition.Y];

                if (tile.HarvestJobs != null)
                {
                    tile.HarvestJobs[ProcessJob.EntityGroupID][Item.Container.ResourceType].Remove(ProcessJob);
                }

                // remove from the zone too:
                if (Zone != null)
                {
                    Zone.RemoveHarvestJob(ProcessJob);
                }
            }
        }

        public void Abandon(Entity entity)
        {
            //base.Abandon(entity);

            if (!IsSpecificJob)
            {
                if (Item.AssignedToJob == ProcessJob)
                {
                    Item.AssignedToJob = null;
                }

                Item = null;
            }
        }

        public override string ToString()
        {
            if (Item != null)
            {
                return "Harvest " + Item.Container.ResourceType.Name + " at: " + Item.Container.MapPosition;
            }
            else
            {
                return "Harvest " + this.ResourceType.Name;
            }

        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.snapshotJob = (JobID)sn.SnapshotID<Job, JobID>(ProcessJob);
            this.IsSpecificJob = sn.DoBool(IsSpecificJob);
            this.ResourceType = sn.DoGameData(ResourceType);            
            this.snapshotZone = (ZoneID)sn.SnapshotID<Zone, ZoneID>(Zone);
            this.snapshotItem = (ResourceItemID)sn.SnapshotID<IResourceItem, ResourceItemID>(Item);
            this.Owner = sn.DoEnum(Owner);

            sn.Ignore(ProcessJob);

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

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            ProcessJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            Zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone);
            Item = LookUp<IResourceItem, ResourceItemID>.FindByID(snapshotItem);
        }

        public bool IsSnapshotted
        {
            get;
            set;
        }

        #endregion
    }

}
