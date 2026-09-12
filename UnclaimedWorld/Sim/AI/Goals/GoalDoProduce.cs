using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Processes;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    public class GoalDoProduce : CompositeGoal, IIDEventSubscriber
    {
        /// <summary>
        /// can be null!
        /// 
        /// shouldn't this be an ID if process can destroy the job?
        /// </summary>
        public ProcessJob Job;
        JobID? snapshotJob;

        /// <summary>
        /// is never null.
        /// </summary>        
        /*  public SimProcess ProductionProcess;
          SimProcessID snapshotProcessID;

          */

        public SimProcessID ProductionProcess;

        /// <summary>
        /// if filled, the goal fails if the ID cannot be resolved
        /// </summary>
        public OwnerID? OwnerOfProduct;

        public ToolTypeCombination ToolTypeCombination;
        ToolTypeCombinationID? snapshotToolCombo;

        public List<EntityID> Tools;
        private List<EntityID> intrinsicAndHandTools;

        /// <summary>
        /// Client use only...
        /// </summary>
        private string ToolCombinationNameForDisplay = null;


        List<AnimModifier> statesToTake = new List<AnimModifier>();
        List<AnimModifier> statesToClear = new List<AnimModifier>();

        MethodID processCompleteMethodID;

        private bool processWasCompleted = false;

        /// <summary>
        /// Delete?
        /// for Special Actions, Healing etc, this is the non-input entity that the process is used on...
        /// </summary>
        //  private EntityID? actingOnEntity;

        //*****************


        /// <summary>
        /// i am using this to send data about the produced items back to the top goal in a decoupled manner...
        /// </summary>
        // public IDActionEvent<List<EntityID>> ProductionCompleteEvent = new IDActionEvent<List<EntityID>>();


        protected override void CreateRegulators()
        {
            base.CreateRegulators();
            changeStanceRegulator = new Regulator(The.Sim.GameplayRandomGenerator, UpdatesPerSecondForStanceChange, "GoalDoProduceChangeStance");
        }



        public GoalDoProduce(Entity entity, ProcessJob job, OwnerID? ownerOfProduct, List<EntityID> tools, ToolTypeCombination toolCombo)
            : base(entity)
        {
            this.Job = job;
            this.OwnerOfProduct = ownerOfProduct;
            this.ProductionProcess = job.ProductionProcess.Value;

            /// can be null if process is in FOW. change this to use proper memory lookup instead.
            /*  this.ProductionProcess = SimProcess.FindById(job.ProductionProcess);

              if (this.ProductionProcess == null)
              {

  #if !RELEASE
                  throw new Exception("Process is null...");
  #endif

               
              }*/


            this.ToolTypeCombination = toolCombo;
            this.Tools = tools;
            GetIntrinsicAndHandTools(Tools, out intrinsicAndHandTools);

            // only really needed for jobs, for now:
            //  this.ProductionProcess.ContainerToPlaceOutputsIn = job.ContainerToPlaceOutputsIn;
            // this.actingOnEntity = job.ActingOnEntity;
            //  this.ImmovableInput = job.ImmovableInput;

            CreateRegulators();
        }



        /// <summary>
        /// to be called when no job exists (GoalEat)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="processType"></param>
        /// <param name="inputData"></param>
        /// <param name="maxBulkToExtract"></param>
        /// <param name="ownerOfProduct"></param>
        /// <param name="tools"></param>
        /// <param name="toolCombo"></param>
        /// <param name="placeProductsInContainer"></param>
        /// <param name="placeProductsInCompartment"></param>
        public GoalDoProduce(Entity entity, ProcessType processType, IKnownEntityData inputData, float? maxBulkToExtract, bool limitExtractionByNutrients, OwnerID? ownerOfProduct, List<EntityID> tools, ToolTypeCombination toolCombo,
            EntityID? placeProductsInContainer, StorageCompartment? placeProductsInCompartment, EntityAndRoot? actingOnEntity = null, // EntityID? actingOnEntity = null, EntityID? actingOnEntityRoot = null, 
            ResourceItemID? resourceItem = null)
            : base(entity)
        {

            this.OwnerOfProduct = ownerOfProduct;
            this.Tools = tools;

            GetIntrinsicAndHandTools(Tools, out intrinsicAndHandTools);

            // assign inputs
            // although, for now, eating always has one input, i decided to copy the data structure used in ProcessJob:
            SimProcess process = new SimProcess(processType, actingOnEntity); //, actingOnEntityRoot);
            ProductionProcess = process.ID;
            if (inputData != null)
            {
                process.AssignedInputs = new Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>>();
                process.AssignedInputs.Add(inputData.EntityType, new List<Tuple<EntityID, WorldLocation>> { new Tuple<EntityID, WorldLocation>(inputData.EntityID, new WorldLocation(inputData.PlaySiteLocation)) });
            }
            process.ResourceItem = resourceItem;
            ToolTypeCombination = toolCombo;
            //this.InputItem = input;
            process.MaxBulkToExtract = maxBulkToExtract;
            process.LimitBulkExtractionByNutrients = limitExtractionByNutrients;

            process.ContainerToPlaceOutputsIn = placeProductsInContainer;
            process.PlaceProductsInCompartment = placeProductsInCompartment;

            /*

            ProductionProcess = new SimProcess(processType, actingOnEntity);         
            if (inputData != null)
            {
                ProductionProcess.AssignedInputs = new Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>>();
                ProductionProcess.AssignedInputs.Add(inputData.EntityType, new List<Tuple<EntityID, WorldLocation>> { new Tuple<EntityID, WorldLocation>(inputData.EntityID, new WorldLocation(inputData.PlaySiteLocation)) });
            }
            ProductionProcess.ResourceItem = resourceItem;
            ToolTypeCombination = toolCombo;
            //this.InputItem = input;
            ProductionProcess.MaxBulkToExtract = maxBulkToExtract;
            ProductionProcess.ContainerToPlaceOutputsIn = placeProductsInContainer;
            ProductionProcess.PlaceProductsInCompartment = placeProductsInCompartment; */

        }

        /// <summary>
        /// Replenish - when no job exists.
        /// 
        /// replenish items should become inputs!
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="processType"></param>
        /// <param name="inputData"></param>
        /// <param name="maxBulkToExtract"></param>
        /// <param name="ownerOfProduct"></param>
        /// <param name="tools"></param>
        /// <param name="toolCombo"></param>
        /// <param name="placeProductsInContainer"></param>
        /// <param name="placeProductsInCompartment"></param>
        public GoalDoProduce(Entity entity, ProcessType processType, OwnerID? ownerOfProduct, IKnownEntityData inputData, StorageCompartment? placeProductsInCompartment,
            Vector3? inputLocationWhenProductionBegins, EntityAndRoot actingOnEntity) // EntityID actingOnEntity)
            : base(entity)
        {
            this.OwnerOfProduct = ownerOfProduct;

            SimProcess process = new SimProcess(processType, actingOnEntity);
            ProductionProcess = process.ID;

            // assign inputs, using predicted location... too hacky?
            process.AssignedInputs = new Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>>();
            process.AssignedInputs.Add(inputData.EntityType, new List<Tuple<EntityID, WorldLocation>> { 
                new Tuple<EntityID, WorldLocation>(inputData.EntityID, new WorldLocation(inputLocationWhenProductionBegins.Value)) });

            //ProductionProcess.ReplenishAction = Action;
            //this.InputItem = input;

            process.PlaceProductsInCompartment = placeProductsInCompartment;

        }





        public GoalDoProduce()
        {
        }


        /*  public EntityID? ActingOnEntity
          {
              get
              {
                  return actingOnEntity;
              }
          }*/

        protected override void Activate()
        {
          
            // the process must exist.
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
            if (process == null)
            {
                if (Job != null)
                {
                    Job.Destroy(true, entity);
                }
                Status = Status.Failed;
                return;
            }

            List<IKnownEntityData> toolsData = null;
            if (!ResolveTools(Tools, ref toolsData))
            {
                return;
            }

            //NEW: check that area is cleared:
            if (Job != null)
            {
                bool isCleared;
                if (Job.GetAreaIsCleared(out isCleared))
                {
                    if (!isCleared)
                    {
                        Status = Status.Failed;
                        return;
                    }
                }
                else
                {
                    Status = Status.Failed;
                    return;
                }
            }

            // check that we are carrying any needed hand tools:
            if (!CheckHandTools(toolsData, this.Job))
            {
                return;
            }

            if (!EmptyToolContainers(toolsData))
            {
                return;
            }

            if (process.ProcessType.IsReplenishProcess) // <- crashed here when process was null
            {
                Entity actingOnEntity = GetActingOnEntity(process);
                MountReplenishTarget(actingOnEntity);
            }
            else
            {
                MountTool(toolsData);
            }

            entity.AttachTriggers(process.ProcessType.Triggers);


            Status = Status.Active;

            AddSubgoal(GoalDoProduceAtomic.GetGoal(entity, this));

            if (process.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded) // .RequiresWork)
            {
                process.AssignWorker(entity, ToolTypeCombination);
            }
            else
            {
                AddSubgoal(new GoalWait(entity, 2));
            }

            // NEW: receive the event when process completes successfully
            entityIntelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.RegisterProcessCompletedEvent(ProcessCompleted, process, 
                this, out processCompleteMethodID);
        
        }

         /// <summary>
        /// can be invoked with a delay if the process ends in FOW
        /// </summary>
        /// <param name="process"></param>
        void ProcessCompleted(IKnownProcess process)
        {
            processWasCompleted = true;

            if (process.ProcessType.IsConsumeProcess)
            {
                GoalEat.ConsumeStomachContents(entity); // consume after eating each item
            }
        }


        private Entity GetActingOnEntity(SimProcess process)
        {
            if (process.ActingOnEntity.HasValue)
            {
                Entity actingOnEntity = Entity.FindByID(process.ActingOnEntity.Value.Entity);

                return actingOnEntity;
            }

            return null;
        }

        public static bool GetStationaryTools(List<EntityID> tools, out List<EntityID> stationaryTools)
        {
            stationaryTools = null;
            if (tools != null)
            {
                stationaryTools = new List<EntityID>();

                for (int i = tools.Count - 1; i >= 0; i--)
                {
                    Entity tool = Entity.FindByID(tools[i]);
                    if (tool != null)
                    {
                        if (tool.EntityType.ToolType.ToolHandling == ToolHandlingType.Stationary)
                        {
                            stationaryTools.Add(tool.ID);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool FilterTools(List<EntityID> tools, out List<EntityID> handTools, Predicate<Entity> predicate)
        {
            handTools = null;
            if (tools != null)
            {
                handTools = new List<EntityID>();
                bool allToolsOK = true;
                for (int i = tools.Count - 1; i >= 0; i--)
                {
                    Entity tool = Entity.FindByID(tools[i]);
                    if (tool != null)
                    {
                        if (predicate(tool)) // tool.EntityType.ToolType.ToolHandling == ToolHandlingType.HandTool) 
                        {
                            handTools.Add(tool.ID);
                        }
                    }
                    else
                    {
                        allToolsOK = false;
                    }
                }

                return allToolsOK;
            }

            return true;
        }

        public static bool GetImmobileTools(List<EntityID> tools, out List<EntityID> immobileTools)
        {
            return FilterTools(tools, out immobileTools, t => ToolType.IsImmovable(t.EntityType));
        }

        public static bool GetMobileTools(List<EntityID> tools, out List<EntityID> mobileTools)
        {
            return FilterTools(tools, out mobileTools, t => !ToolType.IsImmovable(t.EntityType));
        }

        public static bool GetHandTools(List<EntityID> tools, out List<EntityID> handTools)
        {
            return FilterTools(tools, out handTools, t => t.EntityType.ToolType.ToolHandling == ToolHandlingType.HandTool);
        }

        public static bool GetIntrinsicAndHandTools(List<EntityID> tools, out List<EntityID> handTools)
        {
            return FilterTools(tools, out handTools, t => t.EntityType.ToolType.ToolHandling == ToolHandlingType.HandTool || t.EntityType.ToolType.ToolHandling == ToolHandlingType.Intrinsic);
        }

        private bool EmptyToolContainers(List<IKnownEntityData> toolsData)
        {
            if (toolsData != null)
            {
                Entity tool;
                foreach (var item in toolsData)
                {
                    tool = item as Entity;
                    if (tool != null)
                    {
                        // don't empty replenish containers!
                        if (tool.EntityType.ContainerType != null)
                        {
                            IHoldsProductionOutput toolContainer = tool.Contains as IHoldsProductionOutput;
                            if (toolContainer != null)
                            {
                                toolContainer.UncontainAllProductionOutput();  // empty old products                         
                            }
                        }
                    }
                    else
                    {
                        Status = Status.Failed;
                        return false;
                    }
                }
            }

            return true;
        }

        #region SimProcess callback

        public override void PerformWork(SimProcess process, float workedTimeInSeconds, float progressDelta)
        {
            UpdateClientToolInfo();

            PerformWorkerEffects(workedTimeInSeconds, progressDelta, process.ProcessType);

            bool toolsOK = SimProcess.WearDownTools(entity, workedTimeInSeconds, intrinsicAndHandTools, ToolTypeCombination);
            //false); // Morten has disabled this.  ChanceToDestroyToolRegulator.IsReady());

        }

        private void PerformWorkerEffects(double workedTimeInSeconds, float progressDelta, ProcessType processType)
        {
            // set the same progress on all outputs:  
            // gain skill, cache the productivity...           
            UseSkill(processType.RequiredSkillType);
            // UseTool(toolTypeCombination);

            if (entity.EntityType.BiologicalType != null)
            {
                entity.BiologicalEntity.SatisfyNeeds(workedTimeInSeconds, progressDelta, processType.SatisfiesWorkerNeeds);
            }
        }

        private void UseSkill(SkillType newSkill)
        {
            //TODO: gain skill

            /*
            if (newSkill != currentSkillInUse
                || entityIntelligence.GetSkillProductionFactor(currentSkillInUse) != entityIntelligence.GetSkillProductionFactor(newSkill))
            {
                currentSkillInUse = newSkill;
                currentSkillLevel = entityIntelligence.GetSkillProductionFactor(newSkill);             
                skillProductionFactorIsDirty = true;
            }        */
        }


        private void UpdateClientToolInfo()
        {
            if (ToolTypeCombination != null)
            {
                ToolCombinationNameForDisplay = "";
                for (int index = 0; index < ToolTypeCombination.Tools.Count; index++)
                {
                    ToolCombinationNameForDisplay += ToolTypeCombination.Tools[index].Item1.Name;
                    if (index != ToolTypeCombination.Tools.Count - 1)
                    {
                        ToolCombinationNameForDisplay += ", ";
                    }
                }

            }
            else
            {
                ToolCombinationNameForDisplay = null;
            }

        }

        #endregion


        public override float? GetCurrentTotalProductivity()
        {
            /*  if (ProductionProcess != null)
              {*/
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
            if (process != null && process.ProcessType.RequiredSkillType != null)
            {
                return ProcessJob.GetTotalFactorProductivity(GetEnergyLevelFactor(),
                    GetToolProductivity() ?? 1f,
                    entityIntelligence.GetSkillProductionFactor(process.ProcessType.RequiredSkillType));
            }

            return null;
        }

        private float GetEnergyLevelFactor()
        {
            float energyLevel = 1f; // robots always have constant efficiency
            if (entity.EntityType.BiologicalType != null)
            {
                energyLevel = entity.BiologicalEntity.EnergyLevel;
            }

            return energyLevel;
        }



        /// <summary>
        /// returns null if no tools in use...
        /// 
        ///TODO:
        // for better display, scale the value based on the highest productivity that is currently available in inventory (later: obtainable?)
        // get the set of tool type combos from ToolTypeCombinations in ProcessType
        // pick the best one, and use its productivity factor as scaling (it should correspond to 1 on the scale)
        /// </summary>
        /// <returns></returns>
        public override float? GetToolProductivity()
        {
            if (ToolTypeCombination != null)
            {
                return ToolTypeCombination.Productivity;
            }
            else
            {
                return null;
            }
        }

        public override string GetToolInUseName()
        {
            return ToolCombinationNameForDisplay;
        }

        public override string GetSkillInUseName()
        {
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
            if (process != null && process.ProcessType.RequiredSkillType != null)
            {
                return process.ProcessType.RequiredSkillType.Name;
            }
            else
            {
                return null;
            }
        }

        public override float? GetSkillProductivity()
        {
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
            if (process != null && process.ProcessType.RequiredSkillType != null)
            {
                return entityIntelligence.GetSkillProductionFactor(process.ProcessType.RequiredSkillType);
            }
            else
            {
                return null;
            }
        }

        public override Goal.DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            if (!requiresExamineAction)
            {
                return DetectionFactor.DetectSome;
            }
            else
            {
                return DetectionFactor.CannotDetect;
            }
        }


        public override Goal.DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
        {
            return DetectionFactor.CannotDetect;
        }

        public override Goal.StealthFactor GetStealthFactor()
        {
            return StealthFactor.NotGood;
        }

        /// <summary>
        /// Always called BEFORE the OnEnter of a next subgoal of the same parent
        /// </summary>
        public override void OnExit()
        {
            base.OnExit();

            IKnownProcess processData;
            entityIntelligence.GetKnownProcessData(ProductionProcess, out processData);
            if (processData != null)
            {
                ClearProcessAnimStates(processData.ProcessType);

                if (Job == null) //Else destroyed in job destroy. <- ??
                {
                    SimProcess process = processData as SimProcess;
                    if (process != null)
                    {
                        // still needed? what about more workers..?
                        process.Destroy(); // always safe..?
                        //  ProductionProcess = null;
                    }
                }
            }

        }


        public const double UpdatesPerSecondForStanceChange = 1d;

        Regulator changeStanceRegulator;

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            if (!preconditionsRegulator.IsReady() ||
                ArePreconditionsOK())
            {
                //process the subgoals (atomic - 0.2s)
                Status subgoalStatus = ProcessSubgoals(elapsed);

                // process must be valid: 
                // OLD: how will we distinguish a completed process from a destroyed one... we don't get an event and the completed process will be destroyed when we look it up...
                // maybe it does not matter that this goal never completes, always fails
                // the standard way a working job is ended is with a CancelJob message from Job.Destroy, invoked from SimProcess. That also gives a Failed result
                SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
                if (process == null) 
                {
                    if (processWasCompleted == false) // NEW
                    {
                        Status = Goals.Status.Failed;
                    }
                    else
                    {
                        Status = Goals.Status.Completed;
                    }

                    return;
                }
               
               
                if (subgoalStatus == Status.Completed)
                {
                    if (process.IsStarted && process.ProcessType.WorkNeeded != WorkerNeededOptions.WorkerNeeded) // .RequiresWork)
                    {
                        Status = Goals.Status.Completed;
                        return;
                    }

                    if (!ValidateSafetyAndTakeAction(GameData.Instance.AIConstants.HighestDiscomfortLevelForWorkToContinue))
                    {
                        return;
                    }

                    float progress;
                    if (!process.GetKnownProgress(entityIntelligence.Allegiance.SharedKnowledge, out progress))
                    {
                        Status = Goals.Status.Failed;
                        return;
                    }


                    // progress is used both for production and salvage...
                    if (!NonLivingEntity.IsCompleted(progress)) 
                    {
                        // NEW: test valid process every update:                  
                       /* if (ProductionProcess.ID == SimProcessID.Invalid)
                        {
                            Status = Goals.Status.Failed;
                            return;
                            //System.Diagnostics.Debug.Assert(false, "Process is invalid? not safe to snapshot now...");
                        }*/


                        ITopLevelGoal newGoal;
                        if (entityIntelligence.Brain.ArbitrateWhileBusy(out newGoal))
                        {
                            // we have a new, better goal.
                            HandleSubstitutedGoalByArbitrator();
                            return;
                        }
                        else
                        {
                            HandleStanceChange(changeStanceRegulator, process.ProcessType.StanceTypes);

                            AddSubgoal(GoalDoProduceAtomic.GetGoal(entity, this));

                           /* if (ProductionProcess.ID == SimProcessID.Invalid)
                            {
                                System.Diagnostics.Debug.Assert(false, "Process is invalid? not safe to snapshot now...");
                            }*/

                            Status = Status.Active;
                            return;
                        }
                    }
                    else
                    {
                        Status = Status.Completed;
                        return;
                    }

                }
                else
                {
                    // the process can complete independently of what the subgoals are doing.
                    float progress;
                    if (!process.GetKnownProgress(entityIntelligence.Allegiance.SharedKnowledge, out progress))
                    {
                        Status = Goals.Status.Failed;
                        return; // Status;
                    }
                    else
                    {
                        if (NonLivingEntity.IsCompleted(progress))
                        {
                            Status = Status.Completed;
                            return;
                        }
                        else
                        {
                            // NEW: test valid process every update:                  
                           /* if (ProductionProcess.ID == SimProcessID.Invalid)
                            {
                                Status = Goals.Status.Failed;
                                return;
                                //System.Diagnostics.Debug.Assert(false, "Process is invalid? not safe to snapshot now...");
                            }*/

                            Status = subgoalStatus;
                            return;
                        }
                    }
                }

            }
            else
            {
                Status = Goals.Status.Failed;
            }

          
            /*
            if (Status == Goals.Status.Active && ProductionProcess.ID == SimProcessID.Invalid)
            {
                //  System.Diagnostics.Debug.Assert(false, "Process is invalid? not safe to snapshot now...");
            }*/
        }



        /// <summary>
        /// while producing, test that the tools are within range and some little scamp did not run away with them.
        /// (destruction state as well as other things are tested in GoalProduce)
        /// </summary>
        /// <returns></returns>
        protected override bool ArePreconditionsOK()
        {
            if (Tools != null)
            {
                return AreToolsOK(entity, Tools);
            }


            return true;
        }


        public override void Deactivate()
        {
            // we don't want it anymore...

            IKnownProcess processData;
            entityIntelligence.GetKnownProcessData(ProductionProcess, out processData);

            //SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
            if (processData != null)
            {

                if (processData.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded) // .RequiresWork)
                {
                    SimProcess process = processData as SimProcess;
                    if (process != null)
                    {
                        process.RemoveWorker(entity);
                    }
                }

                // TODO: reset skills, tools...
                if (entity.AgentStorage != null)
                {
                    if (!processData.ProcessType.IsReplenishProcess)
                    {
                        entity.AgentStorage.MountedToolOrWeapon = null;
                    }
                }

                entity.DeleteTriggers(processData.ProcessType.Triggers);

            }

           /* if (processCompleteMethodID.HasValue)
            {*/
                // cleanup the callback method
                ActionLookup<List<EntityID>>.Remove(processCompleteMethodID);
          //  }
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

            snapshotJob = sn.SnapshotID<Job, JobID>(Job);
            OwnerOfProduct = sn.DoEnumNullable(OwnerOfProduct);
            snapshotToolCombo = sn.SnapshotID<ToolTypeCombination, ToolTypeCombinationID>(ToolTypeCombination);
            this.Tools = sn.DoList(Tools);
            this.statesToTake = sn.DoList(statesToTake);
            this.statesToClear = sn.DoList(statesToClear);

            // #HAULAI
            this.processCompleteMethodID = sn.DoEnum(processCompleteMethodID);
            this.processWasCompleted = sn.DoBool(processWasCompleted);
            

            this.ProductionProcess = sn.DoEnum(ProductionProcess);


            this.ToolCombinationNameForDisplay = sn.DoString(ToolCombinationNameForDisplay);         
            this.intrinsicAndHandTools = sn.DoList(intrinsicAndHandTools);

            sn.Ignore(Job);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            if (snapshotJob.HasValue)
            {
                Job = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }

           
            ToolTypeCombination = LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(snapshotToolCombo);

            LoadPostProcessRegisterMethodIDs();

        }

        public void LoadPostProcessRegisterMethodIDs()
        {
            // TESTING!!!
            // #HAULAI
             SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(ProductionProcess);
             if (process != null)
             {
                 entityIntelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.RegisterProcessCompletedEvent(ProcessCompleted, process,
                    this, out processCompleteMethodID);
             }

            /*
            ActionLookup<IKnownProcess>.Add(processCompleteMethodID,
                    ProcessCompleted);
            */
        }


        #endregion
    }
}
