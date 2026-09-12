using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using Microsoft.Xna.Framework;

using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Expeditions;

using System.Linq;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.ClientSide.Interface.Inventory;

namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// process job is (construction, harvest, production, cooking and many more)
    /// </summary>
    public class ProcessJob : Job, IIDEventSubscriber 
    {

        //specialized jobs - only fill one:
        public BuildingJob BuildingJob;
        public HarvestJob HarvestJob { get; private set; }
        public SalvageJob SalvageJob { get; private set; }
        public ReplenishJob ReplenishJob;
        public RepairJob RepairJob { get; private set; }
        //public UpgradeJob UpgradeJob; // no need for this right now, we can store in Process


        /// <summary>
        /// Special actions (menu) should set this true to prohibit input substitution in case the input is invalid. 
        /// Substitution should happen for inventory jobs..
        /// </summary>
     //   public bool ProhibitOtherInputs;

        /// <summary>     
        /// 
        /// create process immediately... store location etc in it. keep it unstarted.
        /// Only after it has started, may it may end and be destroyed in FOW (no workers). So we keep a ProcessMemory item.
        /// when process ends, we should only get the event if not in FOW! (some machines can communicate!)
        /// </summary>
        public SimProcessID? ProductionProcess = null;

        /// <summary>
        /// the checking job that is assigned to check this unattended process job
        /// </summary>
        public JobID? CheckingJobID;

        /// <summary>
        /// CLIENT collection
        /// </summary>
        public Dictionary<EntityType, List<EntityID>> InputsBeingHauled = new Dictionary<EntityType, List<EntityID>>();
     

        /// <summary>
        /// for client feedback
        /// </summary>
        public List<EntityID> AssignedTools = new List<EntityID>();
                

        //variables used when all workers on the job estimate their progress one after another:
        public float CumulativelyEstimatedProgress;
       
       
      
        public ProcessType ProcessType;

        public double? EstimatedCompletion
        {
            get;
            private set;
        }


        private Collidable<Entity> UpgradeFootprint;


        /// <summary>
        /// set by the job manager, periodically updated
        /// 
        /// Idea: display Importance in task panel, sort by it...
        /// </summary>
        public float GetImportance(EntityGroup owner)
        {                      
            if (ProcessType.HasOutput)
            {
                var output = ProcessType.Outputs.FirstOrDefault(o => o.IsWasteProduct == false);
                if (output != null)
                {
                    EntityType mainOutput = output.FinalEntityTypeToCreate;

                    return owner.GetImportance(mainOutput);
                }
            }

            return EntityGroup.NeutralImportance; // importance;
            
        }


        public override Priority Priority
        {
            get
            {
                return base.Priority;
            }
            set
            {
                if (value != base.Priority)
                {
                    base.Priority = value;

                    // set hauling to the same prio:
                    IterateHaulingJobs(j => j.Priority = value);
                }
            }
        }

        /// <summary>
        /// can be null for certain processes, like place smokebomb, salvage...
        /// 
        /// why is this needed when the processType defines outputs?
        /// </summary>
        public EntityType OutputEntityType
        {
            get;
            private set;
        }

        /// <summary>
        /// Measure of the crowding effect of the workplace on the labour efficiency.
        /// </summary>
     //   public float AverageLaborEfficiency = 1f;

       
     //   public Dictionary<EntityType, float> MaterialsScore = new Dictionary<EntityType, float>();
     //   public float ProgressSpeed = 0f;

        #region GUI flags etc.

        //Only Used For GUI Client reporting! not in AI logic
            

        public bool AllToolsInUse = false;

        public bool AllToolsAreBroken = false;


        #endregion



        MethodID processCompleteMethodID, processDestroyedMethodID, processStartedMethodID;

        MethodID? processProducingMethodID;

        Regulator areaIsClearedRegulator;
        bool? areaIsCleared;

        /// <summary>
        /// placed it here because it belongs to either an Upgrade job or a BuildingJob. MemoryFact is not used when ordering a structure either.
        /// 
        /// Regulate recompute, cache the result.
        /// </summary>
     /*   public bool AreaIsCleared
        {
            get
            {
                if (GeometryLayout != null)
                {
                    return !GeometryLayout.ShapesContainEntities();

                }
                else return true;
            }
        }*/

        public bool ToolsAreAvailable
        {
            get
            {
                return !AllToolsAreBroken && !AllToolsInUse;
            }
        }


        public ProcessJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public ProcessJob(Entities.EntityID? productionContainer, ProcessType processType, EntityType outputEntityType, EntityGroup entityGroup, EntityAndRoot? actingOn = null, // EntityID? actingOn = null, 
            bool isSalvage = false, UpgradeCategory upgradeCategory = null)
            : base(entityGroup, false)
        {
            Init(productionContainer, processType, outputEntityType, entityGroup, actingOn, upgradeCategory);

            if (isSalvage)
            {
                SalvageJob = new SalvageJob(this);
            }

           /* if (upgradeCategory != null)
            {
                UpgradeJob = new UpgradeJob(upgradeCategory);
            }*/

            // add to group now (this depends on OutputEntityType):
            entityGroup.AddJob(this);
            
            ComputeJobType();
            SetDefaultPriority(entityGroup);
            
        }


        /// <summary>
        /// repair
        /// </summary>
        /// <param name="productionContainer"></param>
        /// <param name="processType"></param>
        /// <param name="outputEntityType"></param>
        /// <param name="entityGroup"></param>
        /// <param name="actingOn"></param>
        /// <param name="isSalvage"></param>
        public ProcessJob(Entities.EntityID? productionContainer, ProcessType processType, EntityType outputEntityType, EntityGroup entityGroup, RepairAction repairAction, 
            EntityAndRoot actingOn,//EntityID actingOn, 
            EntityAndRoot? partToFix) //  EntityID? partToFix)
            : base(entityGroup, false)
        {
            Init(productionContainer, processType, outputEntityType, entityGroup, 
                partToFix ?? actingOn, // !!!
                null); 

            RepairJob = new RepairJob(actingOn.Entity, repairAction, partToFix);
                

            // add to group now (this depends on OutputEntityType):
            entityGroup.AddJob(this);

            ComputeJobType();
            SetDefaultPriority(entityGroup);

        }
      
        /// <summary>
        /// harvest job
        /// </summary>
        /// <param name="productionContainer"></param>
        /// <param name="processType"></param>
        /// <param name="outputEntityType"></param>
        /// <param name="entityGroup"></param>
        /// <param name="actingOn"></param>
        public ProcessJob(ProcessType processType, EntityType outputEntityType, EntityGroup entityGroup, IResourceItem resourceItem)
            : base(entityGroup, false)
        {
            Init(null, processType, outputEntityType, entityGroup, null, null);

            HarvestJob = new HarvestJob(this, resourceItem, entityGroup.ID);
                    
            entityGroup.AddJob(this);

            ComputeJobType();
            SetDefaultPriority(entityGroup);
        }

        private void Init(Entities.EntityID? productionContainer, ProcessType processType, EntityType outputEntityType, EntityGroup entityGroup, EntityAndRoot? actingOn, //EntityID? actingOn, 
            UpgradeCategory upgradeCategory)
        {

            // create the process immediately. It will hold various properties until it gets started later.
            SimProcess process = new SimProcess(processType, actingOn);
            ProductionProcess = process.ID;
            process.ContainerToPlaceOutputsIn = productionContainer;
            process.UpgradeCategory = upgradeCategory;

            // register process events, using a FOW filter:
            SharedKnowledge sharedKnowledge = entityGroup.GetAllegiance().SharedKnowledge;
            sharedKnowledge.PlaySiteKnowledge.RegisterProcessDestroyedEvent(ProcessDestroyed, process, this, out processDestroyedMethodID);
            sharedKnowledge.PlaySiteKnowledge.RegisterProcessCompletedEvent(ProcessCompleted, process, this, out processCompleteMethodID);
            sharedKnowledge.PlaySiteKnowledge.RegisterProcessStartedEvent(ProcessStarted, process, this, out processStartedMethodID);

           

            this.OutputEntityType = outputEntityType;
            ProcessType = processType;

            if (IsUnattended())
            {
                MethodID methodID;
                sharedKnowledge.PlaySiteKnowledge.RegisterProcessProducingEvent(ProcessProducing, process, this, out methodID); // processProducingMethodID);
                processProducingMethodID = methodID;
            }

            CreateRegulators();

        }


        
        /// <summary>
        /// should return null if no time/not started yet
        /// </summary>
        /// <returns></returns>
        public double? GetEstimatedCompletionTime()
        {
            return EstimatedCompletion;
        }

        /// <summary>
        /// when do we recompute???
        /// get Produce event?
        /// Also, notify EntityGroup to resort the list!
        /// </summary>
        private void ComputeEstimatedCompletionTime(IKnownProcess knownProcess)
        {
          /*  IKnownProcess knownProcess;
            SharedKnowledge sharedKnowledge;
            bool ownerIsDestroyed, processIsDestroyed;
            ResolveProcess(out knownProcess, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);
            */

            SimProcess process = knownProcess as SimProcess;
            if (process != null)
            {
                EstimatedCompletion = process.ComputeEstimatedCompletionTime();
            }
        }

        public float GetProgressSpeed()
        {
            bool ownerIsDestroyed, processIsDestroyed;

            SharedKnowledge sharedKnowledge;
            IKnownProcess processData;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                return processData.ProgressSpeed; //GetKnownProgress(sharedKnowledge, out progress);
            }
            else
            {
                return 0f;
            }
        }


        private void CreateRegulators()
        {
            areaIsClearedRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d, "ProcessJob");
        }

        public bool GetAreaIsCleared(out bool areaIsClear)
        {
            // turnip hut or pier can't test for items. They have no collidable. Also they don't reserve the area...

            if (BuildingJob == null && !GetIsUpgradeJob())
            {
                areaIsClear = true;
                return true;
            }
            else
            {
                if (areaIsCleared == null || areaIsClearedRegulator.IsReady())
                {
                    float pad;
                    Entity structureEntity;
                    Collidable<Entity> collidable;

                    if (GetCollidableForOutput(out collidable, out pad, out structureEntity))
                    {
                        if (collidable != null)
                        {
                            areaIsCleared = !collidable.ShapesContainEntities(pad,
                                e => e.EntityType.ItemType != null     // only items can block, for now.                               
                                /*e != structureEntity // count the entity if not the parent (self)
                                && e.EntityType.IntelligenceType == null // and not an agent // .IsMobile // allow agents too. critters could be hard to move? also unconscious..?*/
                                );
                        }
                        else
                        {
                            areaIsCleared = areaIsCleared ?? true;
                        }
                    }
                    else
                    {
                        // fail...
                        areaIsClear = false;
                        return false;
                    }

                    /*
                    GeometryLayout geoLayout = GetGeoLayoutForOutput();
                    areaIsCleared = !geoLayout.ShapesContainEntities();

                    if (GetStructureOutputData())
                    {

                    }

                    if (GeometryLayout != null)
                    {
                        areaIsCleared = !GeometryLayout.ShapesContainEntities();

                    }*/
                }

                areaIsClear = areaIsCleared.Value;

                return true;
            }
        }

        private bool GetCollidableForOutput(out Collidable<Entity> collidable, out float pad, out Entity structureEntity)
        {
            pad = 0f;
            structureEntity = null;
            collidable = null;

            if (BuildingJob != null)
            {
                IKnownEntityData structureData;                
                if (GetStructureOutputData(out structureData))
                {
                    structureEntity = structureData as Entity;
                    if (structureEntity != null
                        && structureEntity.CurrentSimState != null)
                    {
                        pad = structureEntity.CurrentSimState.GeometryLayoutType.Pad;
                        collidable = structureEntity.Collidable;
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (GetIsUpgradeJob()) 
            {
                /*
                 TODO: checking area cleared for upgrades requires:
                 * 1. stamp reserved geolayout for simstate with upgrade, do this after stamping the entity itself? cache the simstate in job?
                 * 2. retrieve the simstate for the ordered upgrade here, get/create cache its collidable
                 * 
                 * Note: cookhouse does not yet have footprints for its upgrades...
                 */

              //  return null;

                /*
                 IKnownProcess processData;
                SharedKnowledge sharedKnowledge;
                ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

                if (ownerIsDestroyed == false && processIsDestroyed == false)
                {       
                    SimStateInfo infoAfterUpgrade;

                    if (infoAfterUpgrade != )
                    {

                    }
                }*/

            }

            return true;
        }

        /*
        private GeometryLayout GetGeoLayoutForOutput()
        {

        }*/

        /// <summary>
        /// returns false if the process is no longer valid.
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public bool GetAssignedInputs(out Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> inputs)
        {
            bool ownerIsDestroyed, processIsDestroyed;

            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                inputs = processData.AssignedInputs;

                return true;
            }
            else
            {
                inputs = null;
                return false;
            }
        }

      /*   public override JobType GetJobType()
        {
           if (jobTypeIsDirty)
            {
                ComputeJobType();
                jobTypeIsDirty = false;
            }

            return jobType;           
            
        }*/

       /* public override string GetNoCancelOptionText()
        {
            if (HarvestJob != null)
            {
                return "'Gather' tasks can only be cancelled using the 'gather' window at the zone location"; //This harvest/gather task should be cancelled using the harvest window at the zone location
            }
            else if (ProcessType != null && ProcessType.IsUpgrade)
            {
                return "Upgrade tasks can only be cancelled in the upgrade window (accessed from the structure's 'UPGRADE' button)";
            }
            else if (ProcessType != null && !ProcessType.IsSalvageProcess)
            {
                return "Production tasks can only be cancelled in the production manager panel";
            }

            return base.GetNoCancelOptionText();
        }*/


        protected override void ComputeJobType()
        {
            // allow designer process tags to override static process job types:            
            foreach (var item in GameData.Instance.AllJobTypes)
            {
                if (item.Value is ProcessJobType && item.Value.IsType(this))
                {
                    jobType = item.Value;
                    return;
                }
            }

            foreach (var item in GameData.Instance.AllJobTypes)
            {
                if (item.Value is StaticJobType && item.Value.IsType(this))
                {
                    jobType = item.Value;
                    return;
                }
            }

        }

        public override Vector3? GetCircaLocation()
        {
            GetCurrentJobLocation(out Vector3? location);
            return location;
        }

        public override void GetLocation(out Point? tile, out EntityID? targetEntity, out ZoneID? zoneID)
        {           
           
            tile = null;
            zoneID = null;
            targetEntity = null;

            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {               
                if (processData.ActingOnEntity.HasValue) 
                {
                    targetEntity = processData.ActingOnEntity.Value.Entity; 
                    return;
                }

                if (HarvestJob != null && HarvestJob.Zone != null)
                {
                    zoneID = HarvestJob.Zone.ID;
                    return;
                }

                if (processData.ImmovableInput != null)
                {
                    targetEntity = processData.ImmovableInput;
                    return;
                }

                if (processData.ImmovableTool != null)
                {
                    targetEntity = processData.ImmovableTool;
                    return;
                }

                Vector3? location;
                //   GetProductionSiteLocation(out location);
                processData.GetProductionSiteLocation(out location, sharedKnowledge);

                if (location.HasValue)
                {
                    tile = MapManager.WorldPosToTile(location.Value);

                }
            }
        }

        /// <summary>
        /// smoke bomb, create farm plot etc, do not require ownership
        /// </summary>
        /// <returns></returns>
        public bool RequiresOwnedActingOnEntity()
        {
            if (SalvageJob != null // but salvage does not use ActingOn!?!?
                || RepairJob != null
                || ReplenishJob != null
                || GetIsUpgradeJob() == true) // UpgradeJob != null)
            {
                return true;
            }

            return false;
        }

        /* public override string GetDisplayStatus()
         {
            
             //if (ProcessType.ProcessToolSet != null &&
             //    ProcessType.ProcessToolSet.ToolTypeCombinations.Count > 0&&
             //    AssignedToolIds.Count == 0)
             //{
             //    return "Necessary tools not in inventory";
             //}//TODO: Add tool is already used by someone
             //else 
           
            
             if (TakenBy.Count > 0)
             {
                 return "Worker: " + TakenBy.Get(0).Name;
             }
             else
             {
                 return "Worker: Not Assigned";
             }

         }*/

        public float GetWorstCaseDurationForEnergyEstimation()
        {
            float workOrTimeNeeded = ProcessType.GetTimeNeeded();

            return GameData.Instance.AIConstants.WorkTimeFactorToEvaluateToolEnergyUse * workOrTimeNeeded;
        }


        /// <summary>
        /// we only support one structure output per process - this will return the structure output if it is present
        /// </summary>
        /// <param name="outputData"></param>
        /// <returns></returns>
        public bool GetStructureOutputData(out IKnownEntityData outputData)
        {
            outputData = null;
            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {                
                if (processData.OutputEntities != null)
                {
                    IKnownEntityData entityData;
                    foreach (var item in processData.OutputEntities)
                    {
                        if(!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(item, out entityData)))
                        {
                            if (entityData.EntityType.StructureType != null)
                            {
                                outputData = entityData;
                                return true;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

              /*  if (!processData.GetOutputEntityData(e => e.EntityType.StructureType != null, out outputData, sharedKnowledge))
                {
                    return false;
                }*/
            }

            return true;
        }


        /// <summary>
        /// returns false if the outputs list has destroyed entities
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="matchingEntity"></param>
        /// <returns></returns>
      /*  public bool GetOutputEntity(Predicate<Entity> predicate, out Entity matchingEntity)
        {
            matchingEntity = null;
            
            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                processData.GetOutputEntity(predicate, out matchingEntity);
            }

            return false;

            //return GetOutputEntity(OutputEntities, predicate, out matchingEntity);
        }*/



        public bool IsStarted(out bool isStarted, out IKnownProcess processData)
        {
            isStarted = false;

            bool ownerIsDestroyed, processIsDestroyed;
          
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                isStarted = processData.IsStarted;
                return true;
            }
            else return false;

            /*
             float progress;
             if (!GetKnownProgress(out progress))
             {
                 return false;
             }
             else
             {
                 isStarted = NonLivingEntity.IsStarted(progress);
             }          

             return true;*/
        }


        /// <summary>
        /// returns false if the process is invalid
        /// </summary>
        /// <param name="isStarted"></param>
        /// <returns></returns>
        public bool IsStarted(out bool isStarted)
        {
            IKnownProcess processData;
            return IsStarted(out isStarted, out processData);
        }


        public bool IsUnattended()
        {
            return ProcessType.WorkNeeded == WorkerNeededOptions.WorkerOnlyNeededToStart;
        }


        /// <summary>
        /// Provides the marginal labour efficiency of adding the worker so the proposed number is reached.
        /// Worker no. 1 has efficiency = 1.
        /// Worker no. (maximum) has efficiency of 0.7. 
        /// </summary>
        /// <param name="proposedNumber"></param>
        /// <param name="maxNumber"></param>
        /// <returns></returns>
        public static double GetMarginalLaborReturn(int proposedNumber, int maxNumber)
        {
            if (maxNumber > 1)
            {
                proposedNumber--;
                maxNumber--;
                double fraction = (double)proposedNumber / (double)maxNumber;
                return 1.0 - 0.3 * Math.Pow(fraction, 2.0);
            }
            else return 1.0;
        }

        public static double GetAverageLaborReturn(int proposedNumber, int maxNumber)
        {
            if (proposedNumber == 0)
            {
                return 1d;
            }

            double totalFactor = 0;
            for (int i = 1; i <= proposedNumber; i++)
            {
                totalFactor += GetMarginalLaborReturn(i, maxNumber);
            }

            return totalFactor / proposedNumber;
        }

        public override int MaxJobPositions
        {
            get
            {
                return ProcessType.MaxWorkers;
            }
        }

        private float CalculateProgressDelta(double seconds, Entities.Entity worker,
            float manSecondsOfWorkNeeded, SkillType skill, IKnownProcess processData, float? toolProductivityFactor = 1f)
        {
            float averageLaborEfficiency = 1;
            SimProcess process = processData as SimProcess;
            if (process != null && process.Workers != null)
            {
                averageLaborEfficiency = (float)ProcessJob.GetAverageLaborReturn(process.Workers.Count, ProcessType.MaxWorkers);
            }

            float totalDuration = CalculateWorkDurationInDays(worker, manSecondsOfWorkNeeded, skill, toolProductivityFactor, averageLaborEfficiency);

            return (float)(seconds * The.Sim.DateAndTime.DaysPerSecond / totalDuration);

        }

        public static float CalculateWorkDurationInDays(Entities.Entity worker,
            float workTimeNeeded, SkillType skill, float? toolProductivityFactor = 1f, float averageLaborEfficiency = 1f)
        {

            float energyLevelFactor = 1f;
            if (worker.BiologicalEntity != null)
            {
                energyLevelFactor = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyProductionFactor, 1f, worker.BiologicalEntity.EnergyLevel);
            }

            float totalProductivity = GetTotalFactorProductivity(energyLevelFactor, toolProductivityFactor.Value, worker.Intelligence.GetSkillProductionFactor(skill), averageLaborEfficiency);

            return workTimeNeeded / totalProductivity;

        }

        public static float GetTotalFactorProductivity(float energyLevelFactor, float toolFactor, float skillFactor, float averageLaborEfficiency = 1.0f)
        {
            float skillAndEnergyFactor = energyLevelFactor * skillFactor;
            skillAndEnergyFactor = Common.ClampBottom(skillAndEnergyFactor, GameData.Instance.Constants.LowestCombinedSkillAndEnergyProductionFactors);

            return skillAndEnergyFactor * toolFactor * averageLaborEfficiency;
        }

        public float CalculateProgressDelta(double seconds, float timeNeeded, float? toolProductivityFactor = 1f)
        {
            return (float)Common.CalculateProgressDelta(seconds, timeNeeded) // (seconds * DateAndTime.DaysPerSecond) / timeNeeded))
                * toolProductivityFactor.Value; // ?

        }



        /// <summary>
        /// maintains two-way pointers between tool and job for client feedback
        /// 
        /// not needed/in use for weapons...
        /// </summary>
        /// <param name="toolData"></param>
        public void AssignTool(IKnownEntityData toolData)
        {
            toolData.AssignedToJob = this.ID;

           
           // System.Diagnostics.Debug.Assert(!AssignedTools.Contains(toolData.EntityID), "Don't assign a tool twice!");

            if (!AssignedTools.Contains(toolData.EntityID)) // ignore if the (immovable?) tool was already assigned...
            {               

                AssignedTools.Add(toolData.EntityID);

                 // assert that a tool serving the same function is not already assigned:
                // (cooking bug, where 2 pots (of different types) were assigned)
                // do this by checking the tool combos for a match:
#if !RELEASE
                bool validComboFound = false;
                EntityGroup owner;
                if (ResolveOwner(out owner))
                {                
                    SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;

                    foreach (var combo in ProcessType.ProcessToolSet.ToolTypeCombinations)
                    {
                        bool assignedToolExistsInCombo = true;
                        foreach (var item in AssignedTools)
	                    {
		                    IKnownEntityData assignedToolData;
                            sharedKnowledge.GetKnownData(item, out assignedToolData);
                       
                            if (assignedToolData != null)
                            {
                                if (!combo.Tools.Exists(c => c.Item1 == assignedToolData.EntityType))
                                {
                                    assignedToolExistsInCombo = false;
                                    break;                        
                                }
                            }
	                    }   
           
                        if (assignedToolExistsInCombo)
                        {
                            validComboFound = true;
                            break;
                        }

                    }
                }

                if (!validComboFound)
                {
                    throw new Exception("Invalid tool set!");
                }               
                
#endif


                AddLog("Assigned tool: " + toolData.EntityType.KeyName + ", " + toolData.EntityID);
            }

        }

       /* public void UnassignTool(EntityID toolID)
        {   
            if (ImmovableTool == toolID)
            {
                ImmovableTool = null; 
            }

            AssignedTools.Remove(toolID);

            AddLog("Unassigned outdated tool: " + toolID);
        }*/

        /// <summary>
        /// calling this will keep the task panel up to date.
        /// 
        /// Warning! unassigning an immovable tool will make the job invalid. Better to destroy the job.
        /// </summary>
        /// <param name="toolData"></param>
        public void UnassignTool(IKnownEntityData toolData)
        {
            System.Diagnostics.Debug.Assert(ID != JobID.Invalid, "ID must be valid!");

            if (toolData.AssignedToJob == this.ID)
            {
                toolData.AssignedToJob = null;
                AddLog("Unassigned tool: " + toolData.EntityType.KeyName + ", " + toolData.EntityID);
            }

            // for client feedback only:
            AssignedTools.Remove(toolData.EntityID);

            // only for completed jobs (probably not necessary):
            /*if (ImmovableTool == toolData.EntityID)
            {
                ImmovableTool = null; 
            }*/

           
           
        }

     
        /// <summary>
        /// if called again for an already assigned material, this will now update its stored location - used in the validation check in Produce
        /// </summary>
        /// <param name="item"></param>
        public void AssignInput(IKnownEntityData item)
        {
            SimProcess process = SimProcess.FindById(ProductionProcess);
            bool wasAssigned;
            process.AssignInput(item, out wasAssigned);

            if (wasAssigned)
            {
                item.AssignedToJob = this.ID;
                AddLog("Assigned input: " + item.EntityType.KeyName + ", " + item.EntityID);
            }
            else
            {
                AddLog("Failed to assign input: " + item.EntityType.KeyName + ", " + item.EntityID);        
            }
        }

       
       
        /// <summary>     
        /// Returns false if the site has become invalid.
        /// </summary>
        /// <returns></returns>
        public bool GetProductionSiteLocation(out Vector3? location)
        {
            location = null;

            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                return processData.GetProductionSiteLocation(out location, sharedKnowledge);
            }                       

            return false;          
        }

      

        /// <summary>
        /// moved to Process
        /// why not simply call GetCurrentLocation...?
        /// 
        /// returns false if the output or site has become invalid.
        /// if the ouput exists, returns that location.
        /// </summary>
        /// <returns></returns>     
        public bool GetFixedJobLocation(out Vector3? location)
        {
            location = null;

            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                return processData.GetFixedJobLocation(out location, sharedKnowledge);
            }

            return false;

            /*
            location = null;

            IKnownEntityData outputData;
            if (!GetFirstOutputEntityData(out outputData))
            {
                return false; // invalid output
            }

            if (outputData != null)
            {
                location = outputData.AccessPoint; // OutputEntities[0].Location;
                return true;
            }
            else
            {
                return GetProductionSiteLocation(out location);
            }*/

        }

       
        public bool GetActingOnEntity(out EntityID? actingOnEntity)
        {
            EntityAndRoot? entityAndRoot = null;
            actingOnEntity = null;
            if (GetActingOnEntity(out entityAndRoot))
            {
                actingOnEntity = EntityAndRoot.GetEntity(entityAndRoot);
                return true;
            }

            return false;

            /*
            actingOnEntity = null;
            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                actingOnEntity = EntityAndRoot.GetEntity(processData.ActingOnEntity);
                return true;
            }

            return false;*/
        }

        public bool GetActingOnEntity(out EntityAndRoot? actingOnEntity)
        {
            actingOnEntity = null;
            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                actingOnEntity = processData.ActingOnEntity;
                return true;
            }

            return false;
        }


        public bool GetImmovableTool(out EntityID? tool)
        {
            tool = null;
            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                tool = processData.ImmovableTool;
                return true;
            }

            return false;
        }

        public bool GetImmovableInput(out EntityID? input, out IKnownProcess processData)
        {
            input = null;
            bool ownerIsDestroyed, processIsDestroyed;
           
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                input = processData.ImmovableInput;
                return true;
            }

            return false;
        }

        public bool GetCurrentJobLocation(out Vector3? location)
        {           
            IKnownProcess processData = null;
            return GetCurrentJobLocation(out location, out processData);

            //return GetCurrentJobLocation(out location, ref inputData, ref toolData);

        }

       

        /// <summary>      
        /// TODO: should return a value for Harvest jobs also...
        /// 
        /// returns false if the job is now destroyed.
        /// the location of either the production site, the outputs or the immovable input, or null if no location exists yet.
        ///
        /// Destroys the job if the immovable tool is invalid.
        /// 
        /// tool and input will never both be filled!
        /// </summary>
        /// <returns></returns>
        public bool GetCurrentJobLocation(out Vector3? location, out IKnownProcess processData)
        {
            location = null;
          
            bool ownerIsDestroyed, processIsDestroyed;
           // IKnownProcess processData;
            processData = null;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                if (!processData.GetCurrentLocation(out location, sharedKnowledge))
                {
                    // if the immovable tool is invalid, then so is the process
                    // the process destroyed event is already handled.
                    //   Destroy(true); 

                    return false;
                }

                return true;
            }

            return false;

            /*

            Vector3? fixedLocation;
            if (GetFixedJobLocation(out fixedLocation) == false)
            {
                location = null;
                return false;
            }

            if (!fixedLocation.HasValue)
            {
                // carcass, when 1st time evaluated
                if (ImmovableInput.HasValue)
                {
                    if (GetImmovableEntityLocation(ImmovableInput.Value, out location, ref inputData))
                    {
                        return true;
                    }
                    else
                    {
                        ImmovableInput = null;
                        return false;
                    }
                }
                else if (ImmovableTool.HasValue)
                {
                    if (GetImmovableEntityLocation(ImmovableTool.Value, out location, ref toolData))
                    {
                        return true;
                    }
                    else
                    {
                        // if the immovable tool is invalid, then so is the job, and most likely the process too.
                        // destroy the job.
                        Destroy(true);

                        //UnassignTool(ImmovableTool.Value); // NEW
                        //ImmovableTool = null;

                        return false;
                    }
                }
                else
                {
                    // carcass, when 1st time evaluated
                    location = null;
                    return true;
                }
            }
            else
            {
                location = fixedLocation;
                return true;
            }*/
        }


       

        /// <summary>
        /// when the process needs an immobile input (carcass, log), find a suitable input + location and return them
        /// </summary>
        /// <param name="regionMapToUse"></param>
        /// <param name="ownerOfInputItems"></param>
        /// <param name="entity"></param>
        /// <param name="foundInputItem"></param>
        /// <param name="inputLocation"></param>
        /// <param name="travelTimeScore"></param>
        /// <returns></returns>
        private GoalEvaluator.CalculateResult FindInputItemForProcessJobAndScoreIt(RegionMap regionMapToUse, ThreatStance threatStance, EntityGroup ownerOfInputItems, Entity entity, 
            out IKnownEntityData foundInputItem
            /*, ref double travelTimeScore*/)
        {
            IKnownEntityData bestItem = null;

            foundInputItem = null;
            // inputLocation = null;

            SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;

            float bestDistance = 1000000f;

            if (ProcessType.Inputs != null)
            {
                if (ProcessType.Inputs.Length == 1)
                {
                    Input input = ProcessType.Inputs[0];
                    if (input.InputIsImmovable())
                    {
                        List<EntityID> listOfItems;
                        if (ownerOfInputItems.Items.TryGetValue(input.EntityType, out listOfItems))
                        {
                            float currentDistance = 0f;                            
                          
                            EntityID item;
                            IKnownEntityData itemData;
                            for (int i = listOfItems.Count - 1; i >= 0; i--)
                            {
                                item = listOfItems[i];
                                GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, item, ownerOfInputItems, out itemData);
                                                              
                                if (itemData != null
                                    && itemData.IsUnassignedToAnythingButThisJob(this, sharedKnowledge)
                                    && GoalEvaluator.IsOnPlaySite(itemData)
                                    && itemData.IsCompleted()
                                    && (!Items.Item.IsImmovable(itemData.Bulk) || itemData.ContainedBy == null))   // NEW: if the item bulk is too large to be moved, and it is inside a container, it should be skipped.
                                {

                                    if (!GoalEvaluator.WorkSiteIsSafe(entity, itemData.PlaySiteLocation, threatStance))
                                    {   // if not accessible, skip this.
                                        continue;
                                    }
                                    else
                                    {

                                        RegionMap.Result result = regionMapToUse.GetDistanceToEntity(entity, entity, itemData, ref currentDistance);
                                        if (result == RegionMap.Result.OK)
                                        {
                                            if (currentDistance < bestDistance)
                                            {
                                                bestItem = itemData;
                                                bestDistance = currentDistance;
                                            }
                                        }
                                        else if (result == RegionMap.Result.NoAccess)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            return GoalEvaluator.CalculateResult.Processing;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foundInputItem = bestItem;
           /* if (foundInputItem != null)
            {
                Vector3 inputLocation = foundInputItem.AccessPoint; //.Location;

                double timeCost = GoalEvaluator.GetTimeCostOfDistance(entity, bestDistance);

                travelTimeScore = GoalEvaluator.ScoreTravelTime(timeCost);
            }*/

            return GoalEvaluator.CalculateResult.Done;
        }

       

        public GoalEvaluator.CalculateResult ScoreThisJobWithoutTools(RegionMap regionMap, ThreatStance threatStance, Entity entity, Intelligence entityIntelligence, int proposedNumberOfWorkers,
            double? ageContribution, double? timeContribution, ref double rating,
            ref IKnownEntityData foundInputItem,   
            EntityGroup ownerOfInputItems, float priority, ref bool jobIsValid)
        {
            
            if (ProcessType.KeyName.Contains("alvage"))
               // && (entity.ID == (EntityID)26270 || entity.ID == (EntityID)26247))
            {
              //  throw new Exception();
            }

            if (entityIntelligence.HasSkill(ProcessType.RequiredSkillType))
            {
               
                Vector3? location;               
                IKnownProcess processData;
                if (!GetCurrentJobLocation(out location, out processData)) 
                {
                    // destroys the job too.
                    jobIsValid = false;
                    rating = 0;
                    return GoalEvaluator.CalculateResult.Done;
                }
                
                if (processData.IsStarted && ProcessType.WorkNeeded != WorkerNeededOptions.WorkerNeeded) // !ProcessType.RequiresWork)
                {
                    rating = 0;
                    return GoalEvaluator.CalculateResult.Done;
                }

             
                foundInputItem = null;

                if (!location.HasValue // is null in the case of processes that require immovable tools and immovable inputs, and which are started from the inventory.
                    && !processData.HasFixedLocation() // !HasFixedLocation()
                    && ProcessType.NeedsImmovableInput()) 
                {
                    // no location has been assigned by Job Manager.
                    // we have to set one ourselves
                    // carcass, when 1st time evaluated

                    if (FindInputItemForProcessJobAndScoreIt(regionMap, threatStance, ownerOfInputItems, entity, out foundInputItem) 
                        == GoalEvaluator.CalculateResult.Done)
                    {
                        if (foundInputItem == null)
                        {
                            rating = 0;
                            return GoalEvaluator.CalculateResult.Done;
                        }
                        else
                        {
                            // OK, go on...                          
                           // location = foundInputItem.AccessPoint; 
                            AssignImmovableInput(foundInputItem); // NEW - use same pattern as for immovable tools
                        }
                    }
                    else
                    {
                        rating = 0;
                        return GoalEvaluator.CalculateResult.Processing;
                    }
                }
              

                float currentProgress;
                SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;
                if (!processData.GetKnownProgress(sharedKnowledge, out currentProgress)) //, out processData))
                {
                    jobIsValid = false;
                    rating = 0;
                    return GoalEvaluator.CalculateResult.Done;
                }

                // test this before materials for cleanup purposes
                if (processData.ActingOnEntity.HasValue) // ActingOnEntity.HasValue)
                { 
                    // NEW: also verify the parts structure:                   
                    IKnownEntityData entityData;
                    if (!processData.ActingOnEntity.Value.IsValid(sharedKnowledge, out entityData)) //GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(processData.ActingOnEntity.Value.Entity, out entityData)))
                    {
                        jobIsValid = false;
                        rating = 0;
                        return GoalEvaluator.CalculateResult.Done;
                    }


                    if (!ProcessType.AllowProcessOnBrokenTarget())
                    {
                        //NEW: when not repairing, require a functional target
                        if (!Entity.IsFunctional(entityData))
                        {
                            //jobIsValid = false; // we could destroy the job, but plantingLoop would recreate it...
                            rating = 0;
                            return GoalEvaluator.CalculateResult.Done;
                        }
                    }

                     // NEW: don't allow salvage before all upgrades are done:
                    if (SalvageJob != null)
                    {
                        if (entityData.ContainedUpgrades != null && entityData.ContainedUpgrades.Count > 0)
                        {
                            rating = 0;
                            return GoalEvaluator.CalculateResult.Done;
                        }
                    }
                   
                }
               

                double materialsEstimate = EstimatePowerAndMaterialsReady(entity, sharedKnowledge, foundInputItem, processData, currentProgress);
                if (materialsEstimate == 0)
                {
                    rating = 0;
                    return GoalEvaluator.CalculateResult.Done; 
                }

                double siteIsClearedScore = 1d; // 0d;
                if (!processData.IsStarted)
                {
                    if (!ScoreConstructionSiteIsCleared(out siteIsClearedScore)) 
                    {
                        jobIsValid = false;
                        rating = 0;
                        
                        return GoalEvaluator.CalculateResult.Done;
                    }

                    if (Common.IsZero(siteIsClearedScore))
                    {
                        rating = 0;
                        EvaluateJob.SetAreaNotCleared(this, true);
                        return GoalEvaluator.CalculateResult.Done; 
                    }
                    else
                    {
                        EvaluateJob.SetAreaNotCleared(this, false);
                    }
                }

                double progressScore = GoalEvaluator.ScoreJobProgress(currentProgress);

                double marginalLaborScore = GoalEvaluator.ScoreNumberOfWorkers(proposedNumberOfWorkers, MaxJobPositions);

                double skillScore = GoalEvaluator.ScoreSkill(entity.Intelligence.GetSkillValue(ProcessType.RequiredSkillType));

                double uniqueSkillScore = GoalEvaluator.ScoreUniqueSkill(entityIntelligence, ProcessType.RequiredSkillType);

                double outputImportance = GetImportance(ownerOfInputItems); // Importance ?? 0.5d; // ScoreOutputImportance(ownerOfInputItems);

               
               

                // if the materials aren't ready, set a very low score:
                if (Common.IsEqual(materialsEstimate, 0))
                {
                    rating = GameData.Instance.AIConstants.JobWithZeroMaterialsDesirability; // does this ever execute?
                }
                else
                {
                   /* if (BuildingJob != null)
                    {*/
                        // weights can go over 1 since there are many components..
                        // NEW: no reason to include site is cleared in score
                     
                      /*  double jobScore = // don't include travel score here, only after tools... some tools are immovable and will decide the job location.
                           0.13 * siteIsClearedScore +                        
                           0.28 * materialsEstimate +  // 0.38 * materialsEstimate +
                           0.13 * progressScore +
                           0.13 * marginalLaborScore +
                           0.23 * skillScore +
                           uniqueSkillWeight * uniqueSkillScore; // weight is 0.15
                        */

                     /*   rating = jobScore;

                    }
                    else
                    {*/

                    EvaluatorWeights weights = GameData.Instance.AIConstants.EvaluatorWeights;

                  //  float uniqueSkillWeight = GameData.Instance.AIConstants.EvaluatorWeights.UniqueSkillWeight;

                        // weights can go over 1 since there are many components..

                    double jobScore = // don't include travel score here, only after tools... some tools are immovable and will decide the job location.                        
                          0.30 * materialsEstimate + // why include this in score? always 0 or 1
                          weights.ProcessProgressWeight * progressScore + //  0.15 
                          0.10 * marginalLaborScore +
                          weights.ProcessImportanceWeight * outputImportance + // 0.10 
                          weights.ProcessSkillWeight * skillScore + // 0.25 
                          weights.UniqueSkillWeight * uniqueSkillScore; // weight is 0.15

                    /*
                        double jobScore = // don't include travel score here, only after tools... some tools are immovable and will decide the job location.                        
                           0.35 * materialsEstimate +// 0.45 * materialsEstimate +
                           0.15 * progressScore +
                           0.15 * marginalLaborScore +
                           0.25 * skillScore +
                           uniqueSkillWeight * uniqueSkillScore; // weight is 0.15
                    */
                       

                        rating = jobScore;

                   // }
                    
                    rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);
                }
            }
            else
            {
                rating = 0d;
            }


            return GoalEvaluator.CalculateResult.Done;
        }


      


      



        private bool ScoreConstructionSiteIsCleared(out double score) //Entity structure)
        {
            score = 1;
            // return true;

            bool areaIsClear;
            if (GetAreaIsCleared(out areaIsClear))
            {
                if (areaIsClear)
                {
                    score = 1;
                }
                else
                {
                    score = 0;
                }

                return true;
            }
            else
            {
                score = 0;
                return false; // invalid
            }

            /*
            IKnownEntityData structureEntity;
            if (ProcessJob.GetStructureOutputData(out structureEntity))
            {
                // structureEntity.geol

                if (structureEntity.AreaIsCleared)  //structureEntity.Structure.TestAreaIsClearAndAddClearingJobs()) //structure.Structure.TestAreaIsClearAndAddClearingJobs())
                {
                    score = 1;
                }
                else
                {
                    score = 0;
                }

                return true;
            }
            else
            {
                score = 0;
                return false; // invalid output...
            }*/
        }

        public override bool RequiresBoldStance
        {
            get
            {
                return ProcessType.RequiresBoldStance;
            }
        }


        /// <summary>
        /// set this from EvaluateJob, unstarted jobs only, so any other inputs can be hauled to the job location
        /// </summary>
        /// <param name="entityData"></param>
        public void AssignImmovableInput(IKnownEntityData entityData)
        {
            // this would only be called before process start, so we can be sure the process still exists...         
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess.Value);
         
            Vector3? previousLocation;
            GetCurrentJobLocation(out previousLocation);

            if (entityData != null)
            {
                //process.ImmovableInput = entityData.EntityID;
                AssignInput(entityData);
            }
            else
            {
                process.ImmovableInput = null;
            }

           // process.GroundLocation = null; // mutex


            Vector3? location;
            if (GetCurrentJobLocation(out location))
            {
                HandleJobLocationChange(previousLocation, location);
            }
        }

        /// <summary>
        /// set this from EvaluateJob, unstarted jobs only! otherwise
        /// unassign the old tool first.
        /// 
        /// call with null to de-assign
        /// </summary>
        /// <param name="entityData"></param>
        public void AssignImmovableTool(IKnownEntityData entityData)
        {
            // this would only be called before process start, so we can be sure the process still exists...
            //IKnownProcess processData = 
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess.Value);
         
            Vector3? previousLocation;
            GetCurrentJobLocation(out previousLocation);

            if (entityData != null)
            {
                process.ImmovableTool = entityData.EntityID;

                AssignTool(entityData);
            }
            else
            {
                process.ImmovableTool = null;
            }

            process.GroundLocation = null; // mutex

            Vector3? location;
            if (GetCurrentJobLocation(out location))
            {
                HandleJobLocationChange(previousLocation, location);
            }
        }

        /// <summary>
        /// EvaluateJob should call this to set a location after evaluating tools, in the case where no immovable tools are needed.
        /// </summary>
        public void AssignJobLocation(Vector3 newLocation)
        {
            // this would only be called before process start, so we can be sure the process still exists...            
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess.Value);
         
            process.ImmovableTool = null; // mutex

            Vector3? previousLocation;
            GetProductionSiteLocation(out previousLocation);

         /*   EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);
            Expedition expedition = (Expedition)entityGroup.Parent;
            ProductionSiteLocation = JobManager.FindProductionLocation(expedition);
            */

            process.GroundLocation = newLocation;

            Vector3? location;
            if (GetCurrentJobLocation(out location))
            {
                HandleJobLocationChange(previousLocation, location);
            }
        }

      
      /*  public void UnassignLocation()
        {
            ImmovableTool = null;
            ProductionSiteLocation = null;
        }*/


        private void HandleJobLocationChange(Vector3? previousLocation, Vector3? location)
        {
            // did the location change?
            if ((previousLocation.HasValue && !location.HasValue)
                || (!previousLocation.HasValue && location.HasValue)
                || (previousLocation.HasValue && location.HasValue && Common.DistanceOctile(location.Value, previousLocation.Value) > 0.5))
            {
                //destroy any previous jobs...
                CancelHaulingJobs();

                CancelAllTakers(null); // ??? also cancel any job takers on the way to the previous job location

                EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);

                if (location != null)
                {
                    // (re-)create input hauling jobs
                    CreateHaulingJobsForProcessInputs(entityGroup, location.Value);
                }
            }
        }

       


        public GoalEvaluator.CalculateResult ScoreTools(RegionMap regionMap, Entity entity, Intelligence entityIntelligence, double? ageContribution, double? timeContribution,
            Vector3? location,
            ToolParams toolParams,    
           // ref double? highestToolScore,
            ref Vector3? temporaryGroundLocation,
            ref double rating, float priority)
        {
            // score the tool set:
            double totalToolsScore = 1d;


            GoalEvaluator.CalculateResult toolsResult = ScoreToolSet(entity, toolParams, entity.Location.Value, //ref highestToolScore,
                location, ref temporaryGroundLocation, entityIntelligence.Allegiance.SharedKnowledge, regionMap, out totalToolsScore);

            if (toolsResult == GoalEvaluator.CalculateResult.Processing)
            {
                return GoalEvaluator.CalculateResult.Processing;
            }
            else if (Common.IsZero(totalToolsScore))
            {
                rating = 0;
                return GoalEvaluator.CalculateResult.Done;
            }
            else
            {
                rating = totalToolsScore;

                rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, priority);

                return GoalEvaluator.CalculateResult.Done;
            }
        }




        public GoalEvaluator.CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution,
            ref EvaluateJob.ToolOrWeaponInstanceComboJobData jobData, ToolParams? toolParams, ref double rating, ref bool jobIsValid, bool cacheScore,
             Dictionary<Job, EvaluateJob.ToolOrWeaponInstanceComboJobData> cachedJobScores, Intelligence entityIntelligence, float priority, EntityGroup ownerOfJobs)
        {


            // see if we already scored this job (not the tools)
            if (!cachedJobScores.TryGetValue(this, out jobData))
            {
                // double jobScore = 0d;
                jobData = new EvaluateJob.ToolOrWeaponInstanceComboJobData();


                GoalEvaluator.CalculateResult jobResult = GoalEvaluator.CalculateResult.Done;
                
                if (HarvestJob != null)
                {
                    // score harvest job
                    jobResult = HarvestJob.ScoreThisJobWithoutTools(regionMap, threatStanceToUse, entity, entityIntelligence, proposedNumberOfWorkers, ageContribution, timeContribution, 
                        //ref rating, ref resourceItem, 
                        ref jobData.JobScore, ref jobData.ResourceItem,
                        ownerOfJobs, priority, ref jobIsValid);
                }
                else
                {
                    //score regular job
                    jobResult = ScoreThisJobWithoutTools(regionMap, threatStanceToUse, entity, entityIntelligence, proposedNumberOfWorkers, ageContribution, timeContribution,
                        ref jobData.JobScore, ref jobData.InputItem,  //ref jobData.ImmovableTool, 
                        ownerOfJobs, priority, ref jobIsValid);
                }

                if (jobIsValid == false)
                {
                    // NEW: bail out.
                    return GoalEvaluator.CalculateResult.Done;
                }

                if (jobResult != GoalEvaluator.CalculateResult.Done)
                {
                    return GoalEvaluator.CalculateResult.Processing;
                }
                else
                {
                    // don't cache when we are calculating an updated job score:
                    if (cacheScore)
                    {
                        cachedJobScores.Add(this, jobData);
                    }
                }

                EvaluateJob.SetDebugScoreNoTools(entity, this, jobData.JobScore);
            }

            // if no job location is set, it is because at least one combo has an immovable tool.
            // the first time this job gets eval'ed, compare the scores of all tool combos as we go. 
            // At the end, assign the highest scorer as the location - either the immovable tool location or a ground location.
            // assigning the location makes it possible for inputs to be hauled.
            // the job will score 0 until all inputs are present.
           
            // we will no longer assign a new input if one becomes invalid. instead process and job gets destroyed... a new job is created right after.
            // each time the job is evaluated, check if the tool is valid. If not, de-assign it, and pick another location using the same procedure.
            // #PROCCHANGE - is this logic still needed if we destroy the job + process?

            Vector3? jobLocation = null;

            // get the location (can be null)
            if (HarvestJob != null)
            {
                if (jobData.ResourceItem != null)
                {
                    jobLocation = jobData.ResourceItem.Container.AccessPoint;
                }
            }
            else
            {
                if (jobData.InputItem != null)
                {
                    // the input has not been assigned to the job yet!
                    jobLocation = jobData.InputItem.AccessPoint;
                }
                else
                {                   
                   // IKnownEntityData siteData = null, toolData = null;
                    if (!GetCurrentJobLocation(out jobLocation)) //, ref siteData, ref toolData))
                    {
                        // destroys the job too.
                        jobIsValid = false;
                        return GoalEvaluator.CalculateResult.Done;
                    }                   
                }
            }


            if ((jobLocation == null && HarvestJob == null) // process jobs go in here to try to find a location from the tools. But harvest jobs should not, their location always depends on a resource.
                || jobData.JobScore > 0d) // continue scoring the tools if the job is valid. is zero if materials are not available...
            {
               // score one tools combo.

                double toolSetScore = 1;
                GoalEvaluator.CalculateResult toolsResult;                

                // now score the tools:
                if (toolParams.HasValue && toolParams.Value.Tools.Count > 0)
                {
                  
                   // should assign a tool and create hauling jobs for inputs if needed
                    toolsResult = ScoreTools(regionMap, entity, entityIntelligence, ageContribution, timeContribution,
                        jobLocation, // can be null. 
                        toolParams.Value, 
                        ref jobData.TemporaryGroundLocation,
                        ref toolSetScore, priority);


                    if (toolsResult == GoalEvaluator.CalculateResult.Processing)
                    {
                        return GoalEvaluator.CalculateResult.Processing;
                    }

                    if (jobLocation == null)
                    {
                        // update high scorer among tools combos for this job:
                       // double currentHighscore = jobData.HighestToolScore ?? -1; // this allows assigning broken tools!? Also inaccessible tools!? Tools that need resupply were always over 0 score.
                        double currentHighscore = jobData.HighestToolScore ?? 0d; // changed this since task panel has no feedback that the tool is broken...
                        if (Common.IsGreaterThan(toolSetScore, currentHighscore))
                        {
                            jobData.HighestToolScore = toolSetScore;
                            // store the tool ids
                            jobData.HighestScoringToolCombo = toolParams.Value.Tools; 
                        }
                    }
                }

                if (jobLocation != null) // skip this if a location has not yet been assigned - return score 0.
                {

                    // score travel last
                    EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);
                    if (owner == null)
                    {
                        rating = 0;
                        return GoalEvaluator.CalculateResult.Done;
                    }

                    double travelScore = 0;
                                        
                    
                    RegionMap.Result travelScoreResult = GoalEvaluator.ScoreTravelTime(regionMap, threatStanceToUse, entity.AccessPoint.Value,
                        jobLocation.Value, entity, ref travelScore, this, owner.Parent);

                    if (travelScoreResult == RegionMap.Result.Wait)
                    {
                        // wait for the result...                       
                        return GoalEvaluator.CalculateResult.Processing;
                    }
                    else if (travelScoreResult == RegionMap.Result.NoAccess)
                    {
                        rating = 0;
                        return GoalEvaluator.CalculateResult.Done;
                    }
                    

                    if (ProcessType.WorkNeeded == WorkerNeededOptions.StartRemotely)
                    {
                        // if we can see the target, we can start the job - but that is not easy to determine.
                        travelScore = 1;
                    }

                    rating = CombineJobAndToolScore(jobData.JobScore, toolSetScore, travelScore);

                    EvaluateJob.ApplyJobPriorityModifier(Priority, ref rating); // apply the user priority
                }

            }

            return GoalEvaluator.CalculateResult.Done;
        }


        private double CombineJobAndToolScore(double jobScore, double totalToolsScore, double travelScore)
        {
            if (Common.IsZero(jobScore) || Common.IsZero(totalToolsScore) || Common.IsZero(travelScore))
            {
                return 0d;
            }
            else
            {
               
                double rating;
                EvaluatorWeights weights = GameData.Instance.AIConstants.EvaluatorWeights;
                if (HarvestJob != null)
                {
                  //  rating = 0.2 * jobScore + 0.7 * travelScore + 0.1 * totalToolsScore; // too extreme - causes dithering between harvest and hauling when both targets are in the same spot

                    rating = weights.HarvestJobScoreWeight * jobScore + weights.HarvestTravelScoreWeight * travelScore + weights.HarvestToolScoreWeight * totalToolsScore;
                   // rating = 0.75 * jobScore + 0.15 * travelScore + 0.1 * totalToolsScore;
                }
                else
                {
                    // double rating = 0.9 * jobScore + 0.1 * totalToolsScore;

                    rating = weights.ProcessJobScoreWeight * jobScore + weights.ProcessTravelScoreWeight * travelScore + weights.ProcessToolScoreWeight * totalToolsScore;
                
                    //rating = 0.75 * jobScore + 0.15 * travelScore + 0.1 * totalToolsScore;
                }

                return rating;
            }
        }

        /// <summary>
        /// scores one tool combo
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="toolParams"></param>
        /// <param name="fromLocation"></param>
        /// <param name="jobLocation"></param>
        /// <param name="sharedKnowledge"></param>
        /// <param name="regionMap"></param>
        /// <param name="totalToolsScore"></param>
        /// <returns></returns>
        private GoalEvaluator.CalculateResult ScoreToolSet(Entity entity, ToolParams toolParams,
            Vector3 fromLocation, 
          //  ref double highestToolScore,
            Vector3? jobLocation, 
            ref Vector3? temporaryGroundLocation,
            SharedKnowledge sharedKnowledge, RegionMap regionMap, out double totalToolsScore)
        {
            totalToolsScore = 1d;
            if (toolParams.Tools.Count > 0)
            {
                double toolsScore;
                totalToolsScore = 0d;

                float distanceToCurrentTool;
                float minDistanceToTool = 10000000f;
                IKnownEntityData closestTool = null;

               
                if (!toolParams.JobDurationInDays.HasValue)
                {
                    toolParams.JobDurationInDays = CalculateWorkDurationInDays(entity, ProcessType.GetTimeNeeded(), ProcessType.RequiredSkillType, toolParams.ToolProductivity);
                }

                IKnownEntityData toolData;

                bool needsToAssignImmovableTool = false;
                if (jobLocation == null) // we are evaluating this job for the first time, and no location has been set on the job yet.
                {
                    if (toolParams.ImmovableTool.HasValue) 
                    {
                        // use the immovable tool's location for this combo.

                        if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(toolParams.ImmovableTool.Value, out toolData)))
                        {
                            totalToolsScore = 0d;
                            return GoalEvaluator.CalculateResult.Done;
                        }

                        //the same agent should not assign more than one tool/job... postpone it until the current job is done.
                        needsToAssignImmovableTool = true; // this changes the score to favor unassigned immovable tools
                       
                        jobLocation = toolData.AccessPoint; // .Location; 
                    }
                    else if (!ProcessType.NeedsImmovableInput()) // immovable input mutexes with immovable tool! we shouldn't get to this place if we need immovable inputs, like carcasses... these will be assigned elsewhere.
                    {
                        // pick a ground location:
                        if (!temporaryGroundLocation.HasValue)
                        {

                            EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);
                            Expedition expedition = (Expedition)entityGroup.Parent;
                            temporaryGroundLocation = JobManager.FindProductionLocation(expedition);
                        }

                        jobLocation = temporaryGroundLocation.Value;
                    }
                    else
                    {
                        // it is not possible to assign a location yet.
                        totalToolsScore = 0d;
                        return GoalEvaluator.CalculateResult.Done;
                    }

                }

                foreach (var tool in toolParams.Tools) 
                {                    
                    if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(tool, out toolData)))
                    {
                        totalToolsScore = 0d;
                        return GoalEvaluator.CalculateResult.Done;
                    }


                    GoalEvaluator.CalculateResult toolsResult = ScoreTool(entity, toolData, entity.PlaySiteLocation, jobLocation.Value,
                            sharedKnowledge, regionMap, toolParams.JobDurationInDays.Value, toolParams.ReplenishStatus, needsToAssignImmovableTool, out distanceToCurrentTool, out toolsScore);

                    if (toolsResult == GoalEvaluator.CalculateResult.Processing)
                    {
                        return GoalEvaluator.CalculateResult.Processing;
                    }
                    else if (Common.IsZero(toolsScore))
                    {
                        totalToolsScore = 0d;
                        return GoalEvaluator.CalculateResult.Done;
                    }

                    // now that we have the distance, sort the tools by distance for better pickup
                    if (distanceToCurrentTool < minDistanceToTool && !ToolType.IsImmovable(toolData.EntityType))
                    {
                        minDistanceToTool = distanceToCurrentTool;
                        closestTool = toolData;
                    }

                    totalToolsScore += toolsScore;
                }

                double productivityScore = ScoreToolProductivity(toolParams.ToolProductivity.Value); //tools);

                if (closestTool != null)
                {
                    // place the closest movable tool at the top:
                    toolParams.Tools.Remove(closestTool.EntityID);
                    toolParams.Tools.Insert(0, closestTool.EntityID);
                }

                totalToolsScore = 0.5f * (totalToolsScore / toolParams.Tools.Count) + 0.5f * productivityScore;

            }

            return GoalEvaluator.CalculateResult.Done;
        }


        private double ScoreToolEnergy(Entity entity, IKnownEntityData tool, float durationInDays)
        {

            if (tool.HasEnergyForDuration(durationInDays))
            {
                return 1d;
            }
            else return 0d;


        }


        /// <summary>
        /// if the tool has too little energy left, we don't score all energy item/tool combinations for replenishment. 
        /// Instead, we just check if each tool type is able to be supplied with energy (exists in inventory), and if the energy is accessible.
        /// If replenishment is needed, the evaluator will then find specific energy items to use AFTER selecting the job.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="tool"></param>
        /// <param name="fromLocation"></param>
        /// <param name="jobLocation"></param>
        /// <param name="sharedKnowledge"></param>
        /// <param name="regionMap"></param>
        /// <param name="replenishStatus"></param>
        /// <param name="distanceToTool"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        /// 
        private GoalEvaluator.CalculateResult ScoreTool(Entity entity, IKnownEntityData tool, Vector3 fromLocation, Vector3 jobLocation, SharedKnowledge sharedKnowledge,
            RegionMap regionMap, float durationInDays, Dictionary<EntityType, ReplenishStatus> replenishStatus, bool needsToAssignImmovableTool, 
            out float distanceToTool, out double score)
        {
            distanceToTool = 0;
            score = 0d;
            double toolEnergyScore = 1d;

            // some tools require energy (fuel/powercells) to run:
            // try to determine energy status of the tool
            if (tool.EntityType.ContainerType != null && tool.EntityType.ContainerType.GetRequiresReplenishType() != null) // .RequiresEnergyType != null)
            {
                toolEnergyScore = ScoreToolEnergy(entity, tool, durationInDays);

                if (Common.IsZero(toolEnergyScore))
                {
                    // if the tool has run out of energy, see if energy is available and accessible for replenishment:
                    if (replenishStatus != null)
                    {
                        ReplenishStatus replenishStatusForTool;
                        if (replenishStatus.TryGetValue(tool.EntityType, out replenishStatusForTool))
                        {
                            if (replenishStatusForTool.OwnsItem == true) // always true
                            {
                                // inventory can supply energy. but the energy may be inaccessible.
                                toolEnergyScore = 0.1;
                            }
                            else
                            {
                                // no replenishment available. tool cannot be used.
                                return GoalEvaluator.CalculateResult.Done;
                            }
                        }
                        else
                        {
                            // no replenishment available. tool cannot be used.
                            return GoalEvaluator.CalculateResult.Done;
                        }
                    }
                    else
                    {
                        // if the replenish status was not supplied a a parameter, we are re-scoring the goal.
                        // assume that energy is available:
                        toolEnergyScore = 0.1;
                    }
                }
            }

            double locationScore;
            GoalEvaluator.CalculateResult locationResult = ScoreToolLocation(entity, tool, fromLocation, jobLocation, sharedKnowledge, regionMap, out distanceToTool, out locationScore);

            if (locationResult == GoalEvaluator.CalculateResult.Processing)
            {
                return GoalEvaluator.CalculateResult.Processing;
            }


            double conditionScore = ScoreToolCondition(tool);

            double immovableToolScore = ScoreNeededImmovableTool(tool, needsToAssignImmovableTool);

            if (Common.IsZero(toolEnergyScore) || Common.IsZero(locationScore) || Common.IsZero(conditionScore))
            {
                score = 0f;
            }
            else
            {
                score =  0.7f * locationScore + 0.1 * immovableToolScore + 0.1f * conditionScore + 0.1f * toolEnergyScore;
            }

            return GoalEvaluator.CalculateResult.Done;
        }

        /// <summary>
        /// score unassigned immovable tools higher, this will make unassigned jobs able to start concurrently when there are multiple tools
        /// 
        /// TODO: but only if we do not have a current goal. 
        /// Otherwise we should wait with assigning the tool, or let other agents who are not busy do it. 
        /// This way, if there are 3 smithys and one agent, he will use the same smithy instead of all 3 in turn (which is what currently happens).
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="needsToAssignImmovableTool"></param>
        /// <returns></returns>
        private double ScoreNeededImmovableTool(IKnownEntityData tool, bool needsToAssignImmovableTool)
        {
            if (needsToAssignImmovableTool && ToolType.IsImmovable(tool.EntityType) && tool.AssignedToJob == null)
            {
               /* EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);
                if (entityGroup.Parent.NoOfWorkers > 1) // improve this by only allowing/scoring higher the same number of immovable tool assignments as there are workers...
                {*/
                    return 1d;
               // }
            }
            
            return 0d;
        }

        /// <summary>
        /// these scores have less meaning when the weighted random method is used to select a weapon...
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="tool"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public static bool ScoreIsCarriedToolLocation(Entity entity, IKnownEntityData tool, out double score)
        {
            score = 0d;

            if (entity.Intelligence.IntrinsicTools != null)
            {
                EntityID intrinsicTool;
                if (entity.Intelligence.IntrinsicTools.TryGetValue(tool.EntityType, out intrinsicTool)
                    && tool.EntityID == intrinsicTool)
                {
                    score = 1d;
                    return true;
                }
            }

            if (entity.AgentStorage != null)
            {
                if (entity.AgentStorage.MountedToolOrWeapon == tool.EntityID)
                {
                    score = 1d;
                    return true;
                }

                if (entity.AgentStorage.ItemStorage.Contains(tool.EntityID))
                {
                    score = 0.99d;
                    return true;
                }


                if (entity.AgentStorage.Equipment != null && entity.AgentStorage.Equipment.Contains(tool.EntityID))
                {
                    score = 0.98d;
                    return true;
                }

            }

            return false;
        }

        public static GoalEvaluator.CalculateResult ScoreToLocation(Entity entity, Vector3 fromLocation, Vector3 jobLocation, SharedKnowledge sharedKnowledge, RegionMap regionMap, out double score, float maxRange)
        {
            score = 0d;
            Point fromSubtilePos = MapManager.WorldPosToSubtile(fromLocation);
            float distanceFromPlayerToJob = -1;
            Point destinationSubtilePos = MapManager.WorldPosToSubtile(jobLocation);
            RegionMap.Result result = regionMap.GetDistanceToEntity(entity,
              entity, null, ref distanceFromPlayerToJob, null, destinationSubtilePos);

            if (result == RegionMap.Result.NoAccess)
            {
                return GoalEvaluator.CalculateResult.Done;
            }
            else if (result == RegionMap.Result.Wait)
            {
                return GoalEvaluator.CalculateResult.Processing;
            }

            double timeCost = GoalEvaluator.GetEvaluatorTimeCostOfDistance(entity, distanceFromPlayerToJob - maxRange);

            score = GoalEvaluator.ScoreTravelTime(timeCost);

            return GoalEvaluator.CalculateResult.Done;
        }


        public static GoalEvaluator.CalculateResult ScoreToolLocation(Entity entity, IKnownEntityData tool, Vector3 fromLocation, Vector3 jobLocation, SharedKnowledge sharedKnowledge, RegionMap regionMap, out float distanceToTool, out double score, float toolRange = 0)
        {
            score = 0d;
            distanceToTool = 0f;

            if (ScoreIsCarriedToolLocation(entity, tool, out score))
            {
                return GoalEvaluator.CalculateResult.Done;//t_1
            }
            

            // get distance score.

            // we won't (can't?) calculate the best route between tools and job location (Travelling Salesman problem?) (although there's max 3 so maybe it could be done).
            // so we rate the distance from the entity to the tool, plus the distance from the tool to the job location.
            // maximum score (1) is given when the tool and job are in the same spot as the entity.


            // get the two distances:
            Point fromSubtilePos = MapManager.WorldPosToSubtile(fromLocation);


            RegionMap.Result result1 = regionMap.GetDistanceToEntity(entity,
               entity, tool, ref distanceToTool, null, null, true, null, true,
               entity.Intelligence.Allegiance,true);


            //  Point toolAccessSubtilePos = MapManager.WorldPosToSubtile(tool.AccessPoint); //.Location);

            // Point destinationSubtilePos = toolAccessSubtilePos;


            //  RegionMap.Result result1 = regionMap.GetDistance(entity, fromSubtilePos, destinationSubtilePos, ref distanceToTool);

            if (result1 == RegionMap.Result.NoAccess)
            {
                return GoalEvaluator.CalculateResult.Done;
            }
            else if (result1 == RegionMap.Result.Wait)
            {
                return GoalEvaluator.CalculateResult.Processing;
            }


            //  fromSubtilePos = toolAccessSubtilePos; 
            Point destinationSubtilePos = MapManager.WorldPosToSubtile(jobLocation);
            float distanceFromToolToJob = -1f;

            RegionMap.Result result2 = regionMap.GetDistanceToEntity(entity,
              tool, null, ref distanceFromToolToJob, null, destinationSubtilePos, true, null, true,
              entity.Intelligence.Allegiance, true);

            //  RegionMap.Result result2 = regionMap.GetDistance(entity, fromSubtilePos, destinationSubtilePos, ref distanceFromToolToJob);

            if (result2 == RegionMap.Result.NoAccess)
            {
                return GoalEvaluator.CalculateResult.Done;
            }
            else if (result2 == RegionMap.Result.Wait)
            {
                return GoalEvaluator.CalculateResult.Processing;
            }


            // got the distance. Compute the score:
            double timeCost;

            distanceFromToolToJob = Common.Max(0, distanceFromToolToJob - toolRange);

            timeCost = GoalEvaluator.GetEvaluatorTimeCostOfDistance(entity, distanceFromToolToJob + distanceToTool);

            score = GoalEvaluator.ScoreTravelTime(timeCost);

            score = Common.ClampBottom(score - 0.05, 0d); // make sure that tools on the ground do not have the same distance score as tools in our hand already

            return GoalEvaluator.CalculateResult.Done;

        }

        private double ScoreToolCondition(IKnownEntityData tool)
        {
            return tool.Condition.Value;

        }

        private double ScoreToolProductivity(float productivity) //List<Entity> tools)
        {
            return Common.Clamp(productivity, 0d, 1d);

        }

        public bool GetKnownProgress(out float progress)
        {
            IKnownProcess processData;
            return GetKnownProgress(out progress, out processData);

        }

        /// <summary>
        /// returns false if the output entities are invalid.
        /// don't do comparisons with the progress value. Call NonLivingEntity static functions IsStarted(), IsCompleted() instead
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public bool GetKnownProgress(out float progress, out IKnownProcess processData)
        {
            progress = 0f;

            // OLD
          /*  if (HarvestJob == null && // #PROCHANGE TODO: delete this after GoalHarvest has mbeen migrated to use Process and OutputEntities...
                ProcessType.HasOutput)
            {*/
                bool ownerIsDestroyed, processIsDestroyed;
               
                SharedKnowledge sharedKnowledge;
                ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

                if (ownerIsDestroyed == false && processIsDestroyed == false)
                {
                    return processData.GetKnownProgress(sharedKnowledge, out progress);                    
                }
                else
                {
                    return false;
                }
                                          
          /*  }// OLD
            else
            {
                progress = this.progress ?? 0f;
            }

            return true;*/
        }

        public bool GetContainerToPlaceOutputsIn(out EntityID? container)
        {
            container = null;

            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                container = processData.ContainerToPlaceOutputsIn;
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// returns false if ouput is destroyed.
        /// </summary>
        /// <param name="isCompleted"></param>
        /// <returns></returns>
        public bool IsCompleted(out bool isCompleted)
        {
            isCompleted = false;
            bool ownerIsDestroyed, processIsDestroyed;
          
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {              
                if (processData.IsCompleted(out isCompleted, sharedKnowledge))
                {
                    return true;
                }
            }

            return false;
        }

        public bool OutputExists(out bool outputExists)
        {
            outputExists = false;
            bool ownerIsDestroyed, processIsDestroyed;

            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                if (processData.OutputExists(out outputExists, sharedKnowledge))
                {
                    return true;
                }
            }

            return false;

        }

        /// <summary>
        /// delete, or replace with IsCompleted
        /// 
        /// returns false if output is no longer valid.
        /// output is null if the product does not exist yet
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        /// <param name="outputData"></param>
        /// <returns></returns>
    /*    public bool GetFirstOutputEntityData(out IKnownEntityData outputData)
        {
            outputData = null;
            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                return processData.GetFirstOutputEntityData(out outputData, sharedKnowledge);
            }

            return false;           
        }*/

    /*    public void AssignProcess(SimProcess process)
        {
            ProductionProcess = process.ID;
            process.ProductionCompleteEvent.AddAndRegister(ProcessComplete,
                       this, out processCompleteMethodID);

        }*/

        void ProcessProducing(IKnownProcess process)
        {
            if (EstimatedCompletion == null)
            {
                ComputeEstimatedCompletionTime(process);
            }
        }

        void ProcessStarted(IKnownProcess process)
        {
            UpdateProductionTargets();

        }

        private void UpdateProductionTargets()
        {
            if (ProcessType.Outputs != null)
            {
                EntityGroup owner;
                if (ResolveOwner(out owner))
                {
                    foreach (var output in ProcessType.Outputs)
                    {

                        ProductionOrder stocksTarget;

                        if (owner.ProductionOrders.Orders.TryGetValue(output.FinalEntityTypeToCreate,
                            out stocksTarget))
                        {
                            if (stocksTarget.ProductionJobsToComplete > 0)
                            {
                                stocksTarget.ProductionJobsToComplete--;
                            }
                        }

                    }
                }
            }
        }
    


        void ProcessDestroyed(IKnownProcess process)
        {         
            // NEW: filtered by SharedKnowledge
            CleanupDestroyedProcess(process);

            Destroy(true); // not sure if we want to destroy the job here. It would cause a crash if iterating, for example. Instead, perhaps let the caller clean up./destroy.
               
        }

      /*  void ProcessDestroyed(SimProcess process)
        {
            // this event comes from Process. check if we still believe the process exists:

            EntityGroup owner;
            IKnownProcess processData;
            if (ResolveOwner(out owner))
            {
                SharedKnowledge sharedKnowledge = owner.Parent.Allegiance.SharedKnowledge;
                if (sharedKnowledge.GetKnownProcessData(ProductionProcess.Value, out processData) == ProcessResult.Destroyed)
                {
                    CleanupDestroyedProcess(process);

                    Destroy(true); // not sure if we want to destroy the job here. It would cause a crash if iterating, for example. Instead, perhaps let the caller clean up./destroy.
                }
            }            
        }*/

       
        /// <summary>
        /// can be invoked with a delay if the process ends in FOW
        /// </summary>
        /// <param name="process"></param>
        void ProcessCompleted(IKnownProcess process)
        {
            /*extractedItemID = products[0];

            // mark the product as In use:
            Entity extractedItem = Entity.FindByID(extractedItemID.Value);
            //extractedItem.InUseBy = entity.EntityID;

            EntityGroup group = entity.Intelligence.Allegiance.SharedKnowledge.AllKnownEntities;
            extractedItem.SetInUseBy(group, entity.EntityID);*/

            EntityGroup owner = null;

            // HERE we could toggle allegiance special actions, like Grow crops, Use fertilizer
            if (process.ActingOnEntity.HasValue)
            {                
                if (ResolveOwner(out owner))
                {
                    SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;
                                      
                    // MOVED from SimProcess. SimProcess has the set of shared processes to enable
                    IKnownEntityData actingOnEntityData;
                    if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(process.ActingOnEntity.Value.Entity, out actingOnEntityData)))
                    {
                        if (ProcessType.DisablesSpecialActionLockProcessTypes != null)
                        {
                            foreach (var item in ProcessType.DisablesSpecialActionLockProcessTypes) 
                            {
                                sharedKnowledge.DisableSpecialActionLock(actingOnEntityData, item.OriginalProcess);
                            }
                        }

                     
                        // Enabling locks for an allegiance should be done in Job!
                        if (ProcessType.EnablesSpecialActionLockProcessTypes != null)
                        {
                            foreach (var item in ProcessType.EnablesSpecialActionLockProcessTypes)
                            {
                                sharedKnowledge.EnableSpecialActionLock(actingOnEntityData, item.OriginalProcess);
                            }
                        }
                    }
                }
            }

           
            if (process.OutputEntities != null)
            {               
                if (owner != null || ResolveOwner(out owner))
                {
                    SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;

                    foreach (var item in process.OutputEntities)
                    {
                        IKnownEntityData entityData;
                        if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(item, out entityData)))
                        {
                           /* Entity entity = entityData as Entity;
                            // mudbricks finished in FOW are still remembered at this point, but will be seen right after!
                            // can we avoid the cast?
                            if (entity != null)
                            { */
                                // create hauling jobs instantly for gather jobs:
                                if (process.ProcessType.IsGathering)
                                {
                                    HaulingJobManager.CreateHaulingJobsForItemOutOfBand(entityData, owner);
                                }

                                // NEW: gather stats also:
                                sharedKnowledge.Allegiance.LogProductionStatistics(entityData, process);

                           // }
                        }
                    }
                }
            }

            // gather what we need before destroying the job:
          /*  List<EntityID> itemsToCheckForRepairs = null;
            if (process.StationaryTools != null)
            {
                itemsToCheckForRepairs = new List<EntityID>();
                itemsToCheckForRepairs.AddRange(process.StationaryTools);

                if (owner == null)
                {
                    ResolveOwner(out owner);
                }
            }*/


            AddLog("Completed");

            // only react to this event if not in FOW!
            LogProductionFinished(process);

            Destroy(true);


           // RepairTools(owner, itemsToCheckForRepairs);
        }

        /// <summary>
        /// not used. repair seems to work between jobs..?
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="itemsToCheckForRepairs"></param>
        private static void RepairTools(EntityGroup owner, List<EntityID> itemsToCheckForRepairs)
        {
            if (itemsToCheckForRepairs != null)
            {
                if (owner != null)
                {
                    SharedKnowledge sharedKnowledge = owner.GetAllegiance().SharedKnowledge;

                    for (int i = itemsToCheckForRepairs.Count - 1; i >= 0; i--)
                    {
                        EntityID tool = itemsToCheckForRepairs[i];
                        IKnownEntityData entityData;
                        if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(tool, out entityData)))
                        {
                            // create repair jobs on tools before next process can start:
                            OtherJobManager.CreateRepairJobIfNeeded(owner, entityData);

                        }
                    }
                }
            }
        }

        

        public void ResolveProcess(out IKnownProcess process, out SharedKnowledge sharedKnowledge, out bool ownerIsDestroyed, out bool processIsDestroyed)
        {
            process = null;
            processIsDestroyed = false;
            ownerIsDestroyed = false;

            EntityGroup owner;
            if (ResolveOwner(out owner))
            {
                sharedKnowledge = owner.Parent.Allegiance.SharedKnowledge;
                if (sharedKnowledge.PlaySiteKnowledge.GetKnownProcessData(ProductionProcess.Value, out process) == ProcessResult.Destroyed)
                {
                    processIsDestroyed = true;
                    // we cannot release locks if the process is gone. We have to rely on the destruct event... 
                }
            }
            else
            {
                sharedKnowledge = null;
                ownerIsDestroyed = true;                
            }
        }

       
        
        private void LogProductionFinished(IKnownProcess process)
        {
            /*   if (!ProcessType.IsConsumeProcess // perhaps log finish eating in a better way
                   && !ProcessType.IsInnateExtractionProcess)
               {*/

            // if there is a worker nearby, then we can cast to process:
            SimProcess simProcess = process as SimProcess;

            if (simProcess != null
                && process.OutputEntities != null
                && !ProcessType.IsConsumeProcess 
                && !ProcessType.IsInnateExtractionProcess
                && ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded //RequiresWork
                && simProcess.Workers != null
                && simProcess.Workers.Count > 0)
            {

                Entity worker = Entity.FindByID(simProcess.Workers[0].Item1);

                if (worker == null)
                    return;

                foreach (var item in process.OutputEntities)
                {
                    Entity outputEntity = Entity.FindByID(item);

                    if (outputEntity != null)
                    {

                        string inputString = ".";
                        string inputNames = null;
                        if (ProcessType.InputsByType != null)
                        {
                            string delim = "";
                            foreach (var input in ProcessType.InputsByType)
                            {
                                inputNames += delim;
                                inputNames += input.Key.Name.ToLower(Config.Culture);
                                delim = ", ";
                            }

                            inputString = " from " + inputNames + ".";
                        }

                        The.Client.AddLogEvent(worker.Intelligence.Allegiance, The.Client.Log.EconomicEvent, worker,
                            string.Format("finished producing {0}{1}", outputEntity.EntityType.Name.ToLower(Config.Culture), inputString));
                    }
                }
            }
        }


       

       

        public void CreateHaulingJobsForProcessInputs(EntityGroup entityGroup, Vector3 jobLocation)
        {
            if (ProcessType.InputsByType != null)
            {
                 bool ownerIsDestroyed, processIsDestroyed;
               
                SharedKnowledge sharedKnowledge;
                IKnownProcess processData;
                ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

                if (ownerIsDestroyed == false && processIsDestroyed == false)
                {
                    // put everything in one spot, not in neat piles as for structures... although, if we are building a vehicle or other large object we might want that...
                    foreach (KeyValuePair<EntityType, Input> kvp in ProcessType.InputsByType)
                    {
                        Input input = kvp.Value;

                        /* if (!input.InputIsImmovable())
                         {*/
                        int needHauling = ProcessType.InputsByType[input.EntityType].Amount.NoOfItems.Value - processData.AssignedInputs[input.EntityType].Count; // InputsAssignedAndOnSite[input.EntityType].Count;

                        if (!input.InputIsImmovable())
                        {
                            for (int i = 0; i < needHauling; i++)
                            {
                                // what about existing jobs???

                                HaulingJobAnyItemOfType haulingJob =
                                    new HaulingJobAnyItemOfType(jobLocation, entityGroup, //expedition.OwnedEntities, 
                                       // Priority, // use the priority of the process job..
                                        input.EntityType,
                                        entityGroup.ID, // expedition.OwnedEntities.ID, 
                                        null);

                                haulingJob.Priority = Priority; // use the priority of the process job..
                                haulingJob.RequiredByProcessJob = this;
                            }
                        }
                        else
                        {
                            if (needHauling > 0)
                            {                               
                                if (processData.HasFixedLocation())
                                {
                                    // this job will never be able to completed. it uses a fixed location but requires an immovable input. Destroy the job.
                                    Destroy(true);
                                    return;
                                }                                
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// gets called from JobManager (for production only) as well as from the evaluator (for construction etc.)
        /// 
        /// loop over assigned inputs to check if they are still valid, not destroyed, and not moved.
        /// if invalid items are found, immediately create a hauling job.     
        /// </summary>
        public void RepairAssignedInputs(SharedKnowledge sharedKnowledge)
        {
           /* float progress;
            IKnownProcess processData;
            if (!GetKnownProgress(out progress, out processData))
                return;*/

            System.Diagnostics.Debug.Assert(ID != JobID.Invalid, "ID must be valid!");

            IKnownProcess processData;
            bool isStarted;
            if (!IsStarted(out isStarted, out processData))
            {
                return;
            }

            if (!isStarted //!NonLivingEntity.IsStarted(progress)) //only if not started.
                && processData.ProcessType.Inputs != null) 
            {
                bool hadInvalidInputs = false;
                foreach (var item in processData.AssignedInputs) // InputsAssignedAndOnSite)
                {
                    for (int i = item.Value.Count - 1; i >= 0; i--)
                    {
                        var input = item.Value[i];
                        IKnownEntityData itemData;

                        if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(input.Item1, out itemData)))
                        {
                            item.Value.RemoveAt(i); // destroyed
                            hadInvalidInputs = true;
                        }
                        else
                        {
                            // see if someone moved it:
                            if (!SimProcess.IsMaterialOnSite(null, itemData, input.Item2)) // itemLocation.Item2))
                            {
                                // the material is no longer on site.                             

                                // unassign:
                                if (itemData.AssignedToJob == this.ID)
                                {
                                    itemData.AssignedToJob = null;
                                }

                                // we no longer consider this item as assigned:
                                item.Value.RemoveAt(i);


                                hadInvalidInputs = true;
                            }
                        }
                    }
                }

                if (hadInvalidInputs)
                {
                    EntityGroup owner;
                    if (ResolveOwner(out owner))
                    {
                        // immediately create new hauling jobs
                        Vector3? jobLocation;
                        if (GetCurrentJobLocation(out jobLocation)
                            && jobLocation.HasValue) // GetFixedJobLocation(out jobLocation))
                        {
                            CreateHaulingJobsForProcessInputs(owner, jobLocation.Value);
                        }
                    }
                }
            }
            // return hadInvalidInputs;
        }

        private double EstimatePowerAndMaterialsReady(Entity worker, SharedKnowledge sharedKnowledge, IKnownEntityData estimateUsingThisInput, IKnownProcess processData, float currentProgress)
        {
            // are there materials on site to do work RIGHT NOW?           
            //float cDelta = pJob.ProducerType.CalculateProgressDelta(0.0167, worker);
            float cDelta;

            if (ID == JobID.Invalid) // why..?
                return 0d;

           
            if (ProcessType.WorkOrTimeNeeded.DaysNeeded.HasValue)
            {
                cDelta = CalculateProgressDelta(0.0167, worker, ProcessType.GetTimeNeeded(),
                    ProcessType.RequiredSkillType, processData);
            }
            else
            {
                cDelta = CalculateProgressDelta(0.0167, ProcessType.GetTimeNeeded());
            }
                       
            Input input;
            double sumOfMaterialsConsumedAtStartScores = 0d;
            float scoreForThisMaterial = 0f;
            bool canStartWork = true;
           

            // since we only consume materials on startup, we only need to check if they are available if the job hasn't started yet.
            // TODO: for continual consumption of substances, we need more code.
            
            if (!processData.IsStarted 
                && ProcessType.InputsByType != null)
            {

                RepairAssignedInputs(sharedKnowledge);               

                foreach (KeyValuePair<EntityType, Input> kvp in ProcessType.InputsByType)
                {
                    input = kvp.Value;
                    int availableItems = processData.AssignedInputs[input.EntityType].Count; // InputsAssignedAndOnSite[input.EntityType].Count;

                    if (estimateUsingThisInput != null && estimateUsingThisInput.EntityType == input.EntityType)
                    {   // for processing immobile inputs
                        availableItems++;
                    }

                    if (availableItems >= (input.Amount.NoOfItems ?? 0))
                    {
                        scoreForThisMaterial = 1;
                    }
                    else
                    {
                        scoreForThisMaterial = 0f;
                    }

                  /*  int currentStage = ProcessType.GetStage(currentProgress, input.StageLength);
                    int stageWhereMaterialsRunOut = currentStage + availableItems;
                    

                    int noofStages;
                    if (input.Amount.NoOfItems.HasValue)
                    {
                        noofStages = input.Amount.NoOfItems.Value; //(int)(1 / input.StageLength);
                        if (noofStages > availableItems)
                        {
                            return 0d;
                        }
                    }
                    else
                    {
                        noofStages = 1;
                    }

                    if (stageWhereMaterialsRunOut >= noofStages)
                    {
                        scoreForThisMaterial = 1f;  // we have materials to last till the end - we are fully satisfied.                   
                    }
                    else
                    {
                        // clamp it:
                        stageWhereMaterialsRunOut = Math.Min(stageWhereMaterialsRunOut, noofStages);
                        float distanceToRunOut = stageWhereMaterialsRunOut * input.StageLength - currentProgress;

                        // now get the current speed.
                        float progressSpeed;
                        if (ProgressSpeed < Common.epsilon)
                        {   // if work hasn't started, use the entity's skill to estimate:
                            if (!entityProgressDeltaEstimate.HasValue)
                            {
                                if (ProcessType.WorkOrTimeNeeded.DaysNeeded.HasValue)
                                {
                                    entityProgressDeltaEstimate = CalculateProgressDelta(The.Sim.GameTime.ElapsedGameTime.TotalSeconds, worker, ProcessType.GetTimeNeeded(), ProcessType.RequiredSkillType);
                                }
                                else
                                {
                                    entityProgressDeltaEstimate = CalculateProgressDelta(The.Sim.GameTime.ElapsedGameTime.TotalSeconds, ProcessType.GetTimeNeeded());
                                }
                            }

                            progressSpeed = (float)(entityProgressDeltaEstimate.Value / The.Sim.GameTime.ElapsedGameTime.TotalSeconds);
                        }
                        else
                        {   // add prospective entity's speed if not currently contributing?
                            progressSpeed = ProgressSpeed;
                        }

                        float timeToRunOut = distanceToRunOut / progressSpeed; // Math.Max(0.00001f, );
                        if (timeToRunOut < GameData.Instance.AIConstants.LimitInSecondsToMaterialRunoutToMatter)
                        {   // how do we feel about the time the materials will last?
                            // use a square function to degrade more rapidly as time runs out.

                            scoreForThisMaterial = (float)Math.Pow(timeToRunOut / GameData.Instance.AIConstants.LimitInSecondsToMaterialRunoutToMatter, 2.0);
                            if (scoreForThisMaterial < Common.epsilon)
                            {
                                scoreForThisMaterial = 0;
                            }
                        }
                        else
                        {
                            scoreForThisMaterial = 1f;        // we are fully satisfied.                        
                        }
                    }
                    */
                    //MaterialsScore[input.EntityType] = scoreForThisMaterial;


                    sumOfMaterialsConsumedAtStartScores += scoreForThisMaterial;

                    if (scoreForThisMaterial == 0)
                    {
                        canStartWork = false;
                    }

                }

                // divide by number of input types:
                sumOfMaterialsConsumedAtStartScores = sumOfMaterialsConsumedAtStartScores / ProcessType.InputsByType.Count;
            }
            else
            {
                sumOfMaterialsConsumedAtStartScores = 1;
            }

            if (!canStartWork)
            {
                return 0;
            }

                      
             //******
            // TODO: estimate substances here.
            //*

          /*  double energyDesirability = 1;

            if (neededEnergy > 0 && The.Sim.AvailableEnergy < Common.epsilon)
            {
                // is there a shortfall:
                if (The.Sim.TotalEnergyNeedsLastCycle + neededEnergy > The.Sim.EnergyProductionLastCycle)
                {
                    energyDesirability = The.Sim.EnergyProductionLastCycle / (The.Sim.TotalEnergyNeedsLastCycle + neededEnergy);
                }
                else
                {
                    energyDesirability = 1;
                }

            }          

            return (sumOfMaterialsScores * 0.5d) + (energyDesirability * 0.5d);*/

            return sumOfMaterialsConsumedAtStartScores;
        }

        public override void Abandon(Entity entity, bool isDestroyingJob = false)
        {
            if (BuildingJob != null)
            {
                BuildingJob.Abandon(entity);
            }

            if (HarvestJob != null)
            {
                HarvestJob.Abandon(entity);
            }

            base.Abandon(entity, isDestroyingJob);
        }

        JobID? oldID = null; // hmm, yeah I know...

        public override void Destroy(bool removeTakers, Entity entityToExcludeFromCancel = null)
        {
            if (ID == JobID.Invalid)
                return;

            oldID = ID; // store this for when the callback from Process comes right after... 

            base.Destroy(removeTakers, entityToExcludeFromCancel);

            //RemoveLocksOnInputs(); // replaced with call to Cleanup from ProcessDestroyed.
           
            CancelHaulingJobs(); // NEW!


            if (BuildingJob != null)
            {
                // remove any placed structure marker after cancelling the job:
                BuildingJob.DestroyUnstartedStructure();
            }

            if (HarvestJob != null)
            {
                HarvestJob.Destroy();
            }

            if (ProductionProcess != null)
            {
                SimProcess process = SimProcess.FindById(ProductionProcess);

                if (process != null)
                {
                    process.Destroy(); // this will call CleanupDestroyedProcess to finish.
                }
            }

            // where to place this..?

            // NEW: unassign ActingOn (Repair + Replenish):
           /* if (actingOnEntityData.AssignedToJob == thisID)
            {
                actingOnEntityData.AssignedToJob = null;
                AddLog("Removed 'Acting on' lock: " + actingOnEntityData.EntityType.KeyName + ", " + actingOnEntityData.EntityID);
            }*/
           
        }

        /// <summary>
        /// call this when we know the process is destroyed (not when in FOW!)   
        /// </summary>
        /// <param name="process"></param>
        void CleanupDestroyedProcess(IKnownProcess process)
        {
            // our ID is destroyed by now...
            JobID thisID = ID != JobID.Invalid ? ID : oldID.Value;

            System.Diagnostics.Debug.Assert(thisID != JobID.Invalid, "Need a valid ID to de-assign");

            // release all locks...
            EntityGroup owner;
            if (ResolveOwner(out owner))
            {
                SharedKnowledge sharedKnowledge = owner.Parent.Allegiance.SharedKnowledge;
                IKnownEntityData entityData;

                if (process.AssignedInputs != null)
                {
                    foreach (var kvp in process.AssignedInputs)
                    {
                        foreach (var item in kvp.Value)
                        {
                            if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(item.Item1, out entityData)))
                            {
                                // unassign:

                                if (entityData.AssignedToJob == thisID)
                                {
                                    entityData.AssignedToJob = null;
                                    AddLog("Removed lock on input: " + entityData.EntityType.KeyName + ", " + entityData.EntityID);

                                }
                            }
                        }
                    }
                }

                if (process.ImmovableTool.HasValue)
                {
                    if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(process.ImmovableTool.Value, out entityData)))
                    {
                        // unassign:
                        if (entityData.AssignedToJob == thisID)
                        {
                            entityData.AssignedToJob = null;
                            AddLog("Removed lock on tool: " + entityData.EntityType.KeyName + ", " + entityData.EntityID);

                        }
                    }
                }

                if (process.StationaryTools != null)
                {
                    foreach (var item in process.StationaryTools)
                    {
                        if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(item, out entityData)))
                        {
                            // unassign:
                            if (entityData.AssignedToJob == thisID)
                            {
                                entityData.AssignedToJob = null;
                                AddLog("Removed lock on tool: " + entityData.EntityType.KeyName + ", " + entityData.EntityID);
                            }
                        }
                    }
                }


               
               

                // REMOVED this since Anchor should be physical!
                /* if (process.ActingOnEntity.HasValue)
                {
                    IKnownEntityData actingOnEntityData;
                    if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(process.ActingOnEntity.Value, out actingOnEntityData)))
                    {
                        // re-enable special actions 
                        // why is this done from Job instead of Process?? because of FOW..?
                                      
                       
                        if (ProcessType.UsesAnchor()) 
                        {
                            bool isCompleted;
                            bool isValid = process.IsCompleted(out isCompleted, sharedKnowledge);

                            bool reenableActions = false;
                            if (!isValid)
                            {
                                reenableActions = true; //?
                            }
                            else if (!isCompleted)
                            {
                                // if the process was ongoing, only re-enable actions if not started yet!
                                if (!process.IsStarted)
                                {
                                    reenableActions = true;
                                }
                            }

                            if (reenableActions)
                            {
                                Entity.EnableSpecialActionsUsingAnchor(actingOnEntityData);
                            }
                        }                      
                    } 
                }*/
            }
        }



        /// <summary>
        /// TODO: optimize this!
        /// </summary>
        void CancelHaulingJobs()
        {
            IterateHaulingJobs(j => j.Destroy(true));

            /*
            // assume that process job and hauling job have the same owner...
            EntityGroup jobOwner = LookUp<EntityGroup, EntityGroupID>.FindByID(base.EntityGroupID);

            if (jobOwner != null)
            {
                List<Job> haulingJobs = jobOwner.HaulingJobs; 

                CancelHaulingJobsForProcess(haulingJobs);
            }*/
        }


        public void IterateHaulingJobs(Action<Job> iterateFunction)
        {
            EntityGroup jobOwner = LookUp<EntityGroup, EntityGroupID>.FindByID(base.EntityGroupID);

            if (jobOwner != null)
            {
                List<Job> haulingJobs = jobOwner.HaulingJobs;

                Job haulingJob;
                for (int i = haulingJobs.Count - 1; i >= 0; i--) // reverse loop to enable removal of currently treated item
                {
                    haulingJob = haulingJobs[i];

                    if ((haulingJob as HaulingJob).RequiredByProcessJob == this)//If the item will be assigned to the job we are going to cancel
                    {
                        iterateFunction(haulingJob);
                    }
                }
            }
        }

          
      /*  public void CancelHaulingJobsForProcess() //List<Job> haulingJobs)
        {
            IterateHaulingJobs(j => j.Destroy(true));
        }*/


     /*   private void RemoveLocksOnInputs()
        {
            if (ProcessType.Inputs != null) // unassign inputs that have not been consumed
            {
                EntityGroup entityGroup;
                if (ResolveOwner(out entityGroup))
                {
                    GetAssignedInputs()

                    foreach (var input in ProcessType.Inputs)
                    {

                        if (InputsAssignedAndOnSite.TryGetValue(input.EntityType, out assignedInputs))
                        {
                            for (int index = 0; index < assignedInputs.Count; index++)
                            {
                                IKnownEntityData entityData;
                                EntityID inputEntityID = (assignedInputs[index].Item1);


                                if (GoalEvaluator.HandleOwnerDataResult(entityGroup.GetAllegiance().SharedKnowledge, // The.InGameUI.UIAllegiance.SharedKnowledge,// UIAllegiance?? 
                                                                        inputEntityID,
                                                                        entityGroup, //The.Sim.PlaySite.GetFirstPlayerExpedition().OwnedEntities, 
                                                                        out entityData))
                                {
                                    if (entityData.AssignedToJob == this)
                                    {
                                        entityData.AssignedToJob = null;
                                      
                                        AddLog("Removed lock on input: " + entityData.EntityType.KeyName + ", " + entityData.EntityID);

                                    }
                                }
                                else
                                {
                                    assignedInputs.RemoveAt(index); // cleanup...
                                    index--;
                                }
                            }
                        }
                    }
                }
            }
        }*/

        /*   public override void RemoveFinishedJob()
           {
               if (HarvestJob != null)
               {
                   HarvestJob.Destroy();

                
               }

               base.RemoveFinishedJob();
           }*/

        public bool GetIsUpgradeJob()
        {
            bool ownerIsDestroyed, processIsDestroyed;
            IKnownProcess processData;
            SharedKnowledge sharedKnowledge;
            ResolveProcess(out processData, out sharedKnowledge, out ownerIsDestroyed, out processIsDestroyed);

            if (ownerIsDestroyed == false && processIsDestroyed == false)
            {
                if (processData.UpgradeCategory != null)
                {
                    return true;
                }
            }
          

            return false;

        }


        public override string GetName()
        {           
            return ProcessType.Name;
        }

        public override bool UserCanCancel(out string reason)
        {
            /*
              if (HarvestJob != null)
            {
                return "'Gather' tasks can only be cancelled using the 'gather' window at the zone location"; //This harvest/gather task should be cancelled using the harvest window at the zone location
            }
            else if (ProcessType != null && ProcessType.IsUpgrade)
            {
                return "Upgrade tasks can only be cancelled in the upgrade window (accessed from the structure's 'UPGRADE' button)";
            }
            else if (ProcessType != null && !ProcessType.IsSalvageProcess)
            {
                return "Production tasks can only be cancelled in the production manager panel";
            }
             */

            reason = "";

            if (ProcessType.UserCanCancel.HasValue)
            {
                reason = ProcessType.UserCannotCancelReason ?? "";
                return ProcessType.UserCanCancel.Value;
            }
            else if (HarvestJob != null)
            {
                reason = "'Gather' tasks can only be cancelled using the 'gather' window at the zone location"; 
          
                return false;
            }
            else if (ProcessType.IsUpgrade)
            {
                reason = "Upgrade tasks can only be cancelled in the upgrade window (accessed from the structure's 'UPGRADE' button)";
          
                return false;
            }
            else if (RepairJob != null)
            {
                reason = "Maintenance tasks cannot be cancelled. Use Abandon on items or structures that are not needed.";
          
                return false;
            }
            else if (ReplenishJob != null)
            {
                reason = "Replenish tasks cannot be cancelled. Use Abandon on items or structures that are not needed.";
          
                return false;
            }

            ProcessType.ProductionUI productionMethod = ProcessType.GetProductionUI();
                        
            if (productionMethod == Processes.ProcessType.ProductionUI.Slider)
            {
                reason = "Production tasks can only be cancelled in the production manager panel";
                return false;
            }
                   
            return true; // CanCancel;
        }

        public override string ToString()
        {
            if (HarvestJob != null)
            {
                return HarvestJob.ToString();
            }
            else
            {
                Vector3? jobLocation;
               // IKnownEntityData siteData = null, toolData = null;
                GetCurrentJobLocation(out jobLocation); //, ref siteData, ref toolData);
                if (jobLocation.HasValue)
                {
                    return this.ProcessType.Name + ": " + jobLocation.ToString();
                }
                else return this.ProcessType.Name + ": " + "no location!";
            }
        }

        public static int CompareByEstimatedCompletion(Job job1, Job job2)
        {
            double? completion1 = ((ProcessJob)job1).EstimatedCompletion;
            double? completion2 = ((ProcessJob)job2).EstimatedCompletion;

            if (completion1 == null)
            {
                if (completion2 == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, x
                    // is greater. 
                    return 1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (completion2 == null)
                // ...and y is null, y is greater.
                {
                    return -1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    int retval = completion1.Value.CompareTo(completion2.Value); // x.Length.CompareTo(y.Length);
 
                    return retval;                  
                }
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

            this.BuildingJob = (BuildingJob)sn.DoISnapshot(BuildingJob);
            this.HarvestJob = (HarvestJob)sn.DoISnapshot(HarvestJob);
            this.SalvageJob = (SalvageJob)sn.DoISnapshot(SalvageJob);
            this.ReplenishJob = (ReplenishJob)sn.DoISnapshot(ReplenishJob);
            this.RepairJob = (RepairJob)sn.DoISnapshot(RepairJob);
          
            this.AssignedTools = sn.DoList(AssignedTools);       
            this.CumulativelyEstimatedProgress = sn.DoFloat(CumulativelyEstimatedProgress);          
            this.ProcessType = sn.DoGameData(ProcessType);      
            this.AllToolsInUse = sn.DoBool(AllToolsInUse);
            this.AllToolsAreBroken = sn.DoBool(AllToolsAreBroken);        
            this.InputsBeingHauled = sn.DoMultiMap(InputsBeingHauled);
            this.OutputEntityType = sn.DoGameData(OutputEntityType);     
            this.processCompleteMethodID = sn.DoEnum(processCompleteMethodID);
            this.processDestroyedMethodID = sn.DoEnum(processDestroyedMethodID);
            this.processStartedMethodID = sn.DoEnum(processStartedMethodID);
            this.processProducingMethodID = sn.DoEnumNullable(processProducingMethodID);
            this.EstimatedCompletion = sn.DoDoubleNullable(EstimatedCompletion);

            this.CheckingJobID = sn.DoEnumNullable(CheckingJobID);

           // this.Importance = sn.DoDoubleNullable(Importance);

          //  this.ProhibitOtherInputs = sn.DoBool(ProhibitOtherInputs);

            //this.jobTypeIsDirty = sn.DoBool(jobTypeIsDirty);
          
            ProductionProcess = sn.DoEnumNullable(ProductionProcess);
           
            sn.Ignore(oldID);
            sn.Ignore(areaIsCleared);

            return this;
        }


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (HarvestJob != null)
                HarvestJob.LoadPostProcess(sn);

            if (BuildingJob != null)
                BuildingJob.LoadPostProcess(sn);

            if (SalvageJob != null)
                SalvageJob.LoadPostProcess(sn);

            if (ReplenishJob != null)
                ReplenishJob.LoadPostProcess(sn);

           /* if (UpgradeJob != null)
                UpgradeJob.LoadPostProcess(sn);
            */

            LoadPostProcessRegisterMethodIDs();

            ComputeJobType();

            CreateRegulators();
           
        }

        public void LoadPostProcessRegisterMethodIDs()
        {            
            ActionLookup<IKnownProcess>.Add(processCompleteMethodID,
                    ProcessCompleted);

            ActionLookup<IKnownProcess>.Add(processDestroyedMethodID,
                  ProcessDestroyed);

            ActionLookup<IKnownProcess>.Add(processStartedMethodID,
                ProcessStarted);

            if (processProducingMethodID.HasValue)
            {
                ActionLookup<IKnownProcess>.Add(processProducingMethodID.Value,
                    ProcessProducing);
            }
           
        }


        #endregion




        public bool AssertInputIsAssigned(EntityID? Item)
        {
            if (Item.HasValue)
            {
                SimProcess process = SimProcess.FindById(ProductionProcess);

                
                if (process != null)
                {
                    if (process.IsStarted)
                    {
                        // should not be possible to assign inputs to a started process...
                        return true;
                    }

                    if (process.AssignedInputs.Any(k => k.Value.Any(e => e.Item1 == Item.Value)))
                    {
                        return true;
                    }
                }
               

            }  

            return false;



        }
    }
}
