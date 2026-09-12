using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Items;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Systems;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Resources;
using System.Diagnostics;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.SimSide.Processes
{
    public enum SimProcessID : long
    {
        First = 0L,
        Invalid = long.MaxValue,
        Max = Invalid
    }

    public enum RepairAction 
    { 
        Integrity, // refurbishing the integrity of the root only
        ReplacePart, // means first removing the part, then replacing it with one of the same type
        RemovePart, // removes a damaged part so it can be reconditioned in a workshop
        AddPart, // adds a new part if there is an empty slot for it
        PartsCondition, // recondition parts inplace
        Condition // for reconditioning when not a part
    }
       

    /// <summary>
    /// represents a physical process which can either be started by an agent or some other way.
    /// BUT - ProcessJob creates the process even for non-existing entities! So it is also an abstract container!!
    /// 
    /// Cases:
    /// 1. process is started by an agent, and needs an agent's work
    /// 2. process is started by an agent, but proceeds on its own. Set the tools!
    /// 3. process starts on and proceeds on its own.
    /// 
    /// 1) should update every tick, the others with longer intervals.
    /// 
    /// Produce() is called at each step. 
    /// 
    /// There may be a ProcessJob or a Goal that keeps a reference to the process. But the process should not know about Jobs or Goals.
    /// 
    /// More than one process can affect an entity at the same time.
    /// </summary>
    [DebuggerDisplay("{ProcessType}")]
    public class SimProcess : ILookUp<SimProcess, SimProcessID>, ISnapshot, ISleepingUpdatable, IKnownProcess
    {

        public ProcessType ProcessType
        {
            get;
            private set;
        }


      //  public Regulator ChanceToDestroyToolRegulator;

        /// <summary>
        /// call assignInput to do that... only allowed for SimProcess!
        /// </summary>
        public Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> AssignedInputs { get; set; }

        /// <summary>
        /// Harvesting: this represents the 'item' that has not been created yet!
        /// 
        /// Assign it, even if the job is not a specific job, once a target has been found.
        /// 
        /// Let's consume this at start. The outputs are also created at start. If interrupted, it should be possible to restart with the current output progress..
        /// </summary>
        public ResourceItemID? ResourceItem;
        //  public IResourceItem Item;


        public StorageCompartment? PlaceProductsInCompartment;

        /// <summary>
        /// the slot to use when upgrading
        /// </summary>
        public UpgradeCategory UpgradeCategory { get; set; }

        public List<EntityID> OutputEntities { get; private set; }

        /// <summary>
        /// only if Job and output entities is null do we use this:
        /// </summary>
        private float Progress = 0f;

        /// <summary>
        /// this is needed to see/unsee processes in FOW.
        /// </summary>
        public Point? MapPosition { get; set; }

      //  public GoalReplenish.ReplenishAction? ReplenishAction;


     
       
        public float? MaxBulkToExtract;

        public bool LimitBulkExtractionByNutrients;


        /// <summary>
        /// workers should come and go...
        /// </summary>
        List<Tuple<EntityID, ToolTypeCombinationID?>> workers;

        /// <summary>
        /// call assign and remove
        /// </summary>
        public List<Tuple<EntityID, ToolTypeCombinationID?>> Workers
        {
            get
            {
                return workers;
            }
        }


        /// <summary>
        /// register with SharedKnowledge if FOW matters.
        /// </summary>
        public IDActionEvent<SimProcess> ProcessStartedEvent = new IDActionEvent<SimProcess>();

        /// <summary>
        /// is invoked before the process is destroyed
        /// 
        /// register with SharedKnowledge if FOW matters.
        /// </summary>
        public IDActionEvent<SimProcess> ProcessCompletedEvent = new IDActionEvent<SimProcess>();

        /// <summary>
        /// is invoked right after ID is set invalid and the process removed
        /// 
        /// register with SharedKnowledge if FOW matters.
        /// </summary>
        public IDActionEvent<SimProcess> ProcessDestroyedEvent = new IDActionEvent<SimProcess>();

        public IDActionEvent<SimProcess> ProcessProducingEvent = new IDActionEvent<SimProcess>();



      //  public IDActionEvent<Entity> InputDestroyedEvent = new IDActionEvent<Entity>();


        public SimProcessID ProcessID
        {
            get { return ID; }
        }

        #region Moved from ProcessJob

        /// <summary>   
        /// where should the output be placed?
        /// only fill one: 
        /// 
        /// ContainerToPlaceOutputsIn: for instance a containing building (workshop) where production takes place, or for consuming, the entity itself - NOTE: this is NOT the ToolOutput container!!
        /// ProductionSiteLocation: location in the open
        /// ImmovableInputLocation: location of immovable input in the open (should this be an entity and AccessPoint instead???)
        /// 
        /// Use these before the output has been created, or if the process does not have an output
        /// - a site gets assigned on job creation, or when the agent selects the job in the evaluator (carcass butchering)
        /// - when production starts, the output entity is created and its location can be used instead
        /// </summary>
        public EntityID? ContainerToPlaceOutputsIn { get; set; } // NEW: used when a replenish target is inside a container

        /// <summary>
        /// this needs to be assigned during evaluation for the input hauling jobs
        /// </summary>
        public EntityID? ImmovableInput
        {
            get
            {
                return immovableInput;
            }
            set
            {
                immovableInput = value;

                if (value != null)
                {
                    groundLocation = null; // mutex
                }

                RecomputeMapPosition();
            }
        } 
        EntityID? immovableInput;

        /// <summary>
        /// needs this for structures. is also used for Special actions - NEW: replenish jobs also
        /// </summary>
        public Vector3? GroundLocation
        {
            get
            {
                return groundLocation;
            }
            set
            {
                groundLocation = value;
                RecomputeMapPosition();
            }
        }
        Vector3? groundLocation;
       
        /// <summary>
        /// this also needs to be assigned during evaluation for the input hauling jobs
        /// 
        /// if this becomes invalid, the whole job should be removed.?
        /// </summary>
        public EntityID? ImmovableTool
        {
            get
            {
                return immovableTool;
            }
            set
            {
                immovableTool = value;

                if (value != null)
                {
                    groundLocation = null; // mutex
                }

                RecomputeMapPosition();
            }
        }
        EntityID? immovableTool;

        /// <summary>
        /// tools that the process depends on being present - if something happens to them, the Process should stop/abort
        /// 
        /// contrast with handtools, carried by a worker...
        /// 
        /// will there ever be a non-job process that uses tools??? probably not.
        /// these tools may also exist in ImmovableTool...
        /// </summary>
        public List<EntityID> StationaryTools { get; set; }

        /// <summary>
        /// has productivity and degrade info for process tools
        /// 
        /// ensure that worker tool combos are a subset of this
        /// </summary>
        public ToolTypeCombination StationaryToolsTypeCombination;
        ToolTypeCombinationID? snapshotProcessToolCombo;


       /* public float SkillProductivity
        {
            get
            {
                return accumulatedSkillProductivity;
            }
        }

        public float ToolProductivity
        {
            get
            {
                return accumulatedToolProductivity;
            }
        }

        public float EnergyProductivity
        {
            get
            {
                return accumulatedEnergyProductivity;
            }
        }

        public float TotalProductivity
        {
            get
            {
                return accumulatedTotalProductivity;
            }
        }*/


       /* private float accumulatedToolProductivity;
        private float accumulatedSkillProductivity;
        private float accumulatedEnergyProductivity;
        private float accumulatedTotalProductivity;
        */

        private Productivity productivity;
        public Productivity Productivity
        {
            get
            {
                return productivity;
            }
           
        }

        public float ProgressSpeed { get; private set; }

        /// <summary>
        /// Measure of the crowding effect of the workplace on the labour efficiency.
        /// </summary>
    //    public float AverageLaborEfficiency = 1f;


        #endregion

        /// <summary>
        /// SimProcess is abstract before starting. these entities may not exist.
        /// store the relationship we believe they are in, so they can be validated later
        /// 
        /// NOTE: for repair actions on parts, this.Entity will be the part, not the parent entity/root!
        /// </summary>
        public EntityAndRoot? ActingOnEntity { 
            get; 
            set; 
        }

        /// <summary>
        /// NOTE: for repair actions on parts, this will be the part, not the parent entity!
        /// </summary>
       /* public EntityID? ActingOnEntity { get; set; }

        /// <summary>
        /// Root of ActingOn
        /// Only for validating the parts status
        /// </summary>
        public EntityID? ActingOnEntityRoot { get; private set; }
        */

        private bool ignoreProgressCap;

        private bool suppressSpawningEvents;
    
        /// <summary>
        /// ownership should be assigned by the allegiance/entity if it still exists via an event
        /// 
        /// NEW: using this for allegiance for created robots/terminals
        /// </summary>
        OwnerID? ownerOfOutput;
        
       

        public enum StatusOfProcess
        {
            Complete,
            Failed,
            Active,
        };

        private StatusOfProcess status;
        private StatusOfProcess Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                if (value == StatusOfProcess.Failed)
                {
                    int i = 0;
                }
            }
        }


        public bool IsStarted
        {
            get;
            private set;
        }

        public SimProcess()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");

        }


        public SimProcess(ProcessType processType, EntityAndRoot? actingOnEntity, //EntityID? actingOnEntity, EntityID? actingOnEntityRoot, 
            bool ignoreProgressCap = false, bool isSpawning = false)
        {
            this.ProcessType = processType;
            this.ActingOnEntity = actingOnEntity; // assigned for Special Actions and Replenish
           // this.ActingOnEntityRoot = actingOnEntityRoot;
            this.ignoreProgressCap = ignoreProgressCap;
            this.suppressSpawningEvents = isSpawning;

            if (ProcessType.InputsByType != null)
            {
                AssignedInputs = new Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>>();

                foreach (KeyValuePair<EntityType, Input> kvp in ProcessType.InputsByType)
                {
                    Input input = kvp.Value;
                    // initialization only, no assignment of items:
                    AssignedInputs.Add(input.EntityType, new List<Tuple<EntityID, WorldLocation>>());
                }
            }

            if (ProcessType.ProcessToolSet != null
                || (ProcessType.MaxWorkers > 0 && ProcessType.WorkNeeded != WorkerNeededOptions.StartRemotely))
            {
                productivity = new Productivity();
            }

            AddToLookup();

            CreateRegulators();

            bool intervalChanged = false;
            RecomputeUpdateInterval(out intervalChanged);
        }

        /// <summary>
        /// when the evaluator sets inputs etc., we have to recalc the position
        /// i guess this only makes sense for unstarted processes... Memory facts don't exist for unstarted processes.     
        /// </summary>
        private void RecomputeMapPosition()
        {
            Vector3? location;
            GetCurrentLocation(out location, null, true);

            if (location.HasValue)
            {
                MapPosition = MapManager.WorldPosToTile(location.Value);
            }
            else
            {
                MapPosition = null;
            }
        }

        #region ISleepingUpdatable

       
        private double? timePointInSeconds;
        public double? TimePointInSeconds
        {
            get
            {
                return timePointInSeconds;
            }
        }

        public void SetNextTimepoint(double? timepoint)
        {
            this.timePointInSeconds = timepoint;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="wasDestroyed"></param>
        public void Update(GameTime gameTime, out bool wasDestroyed)
        {
            wasDestroyed = false;

            float? progressDelta = null;
            float toolProductivity = 0f, skillProductivity = 0f, energyProductivity = 0f;


            if (RequiresContinuousSubstances())
            {
                if (phase == Phase.RequestSubstances)
                {
                    // if some requests are waiting for an answer, wait until the answer comes back
                    // should tools request electricity???
                    if (requestedSubstances != null && requestedSubstances.Count > 0)
                    {
                        return;
                    }
                    else
                    {
                        if (!EstimateProgressDelta(out progressDelta, 
                            out toolProductivity, out skillProductivity, out energyProductivity))
                        {
                            HandleDestroyedOutput();
                            return;
                        }

                        // request substances
                        RequestSubstances(progressDelta.Value);
                    }

                    phase = Phase.Produce;
                    return;
                }
            }


            // ready for the next production step.
           
            if (!progressDelta.HasValue)
            {
                if (!EstimateProgressDelta(out progressDelta,
                    out toolProductivity, out skillProductivity, out energyProductivity))
                {
                    HandleDestroyedOutput();
                    return;
                }
            }


            float clampedProgressDelta;
            Produce(progressDelta.Value, out wasDestroyed, out clampedProgressDelta);

            if (productivity != null)
            {
                productivity.SaveProductivityStats(clampedProgressDelta, toolProductivity, skillProductivity, energyProductivity, GetUpdateInterval());
            }

        }


       

        public void ProduceTillCompletion()
        {
            bool wasDestroyed;
            float clampedProgressDelta;
            Produce(1f, out wasDestroyed, out clampedProgressDelta);
        }

        private void Produce(float progressDelta, out bool wasDestroyed, out float clampedProgressDelta)
        {
            wasDestroyed = false;

            Produce(progressDelta, out clampedProgressDelta);


            FireProducingEvent();


            if (IsCompleted())
            {
                ProductionFinished();

                wasDestroyed = true;
            }
            else
            {
                if (RequiresContinuousSubstances())
                {
                    assignedSubstances.Clear();
                    phase = Phase.RequestSubstances;
                }
                else
                {
                    phase = Phase.Produce;
                }
            }           
        }


        private double? updateInterval;
        public double? UpdateInterval
        {
            get
            {
                return updateInterval;
            }

            private set
            {
                if (!Common.IsEqual(updateInterval, value))
                {
                    updateInterval = value;

                    SleepyUpdater<SimProcess> updater = LookUpSleepyUpdater<SimProcess>.FindByID(SleepyUpdater);
                    if (updater != null)
                    {
                        updater.NotifyUpdateIntervalChanged(this);
                    }
                }
            }

        }

        void ISleepingUpdatable.CreateSleepyLookupCollection()
        {

        }

        public static void CreateSleepyLookupCollection()
        {
            LookUpSleepyUpdater<SimProcess>.Create();
        }



        public SleepyUpdaterID SleepyUpdater { get; set; }


        /// <summary>
        /// Remember to keep this up-to-date when more functionality is added to the Update method!
        /// Otherwise performance will suffer because of unnecessary updates, or the object may not receive any Update calls when it needs it.
        /// 
        /// Worker processes should update every frame. Other processes, degrade, plant growth, once every x seconds.
        /// The ProcessType should define the update interval.
        /// 
        /// Higher update frequency means the process progresses more smoothly, at a higher CPU cost.
        /// Perhaps consider making the Client lerp between values to smooth out the progress...
        /// 
        /// Processes that need substances from a pool should not update more often than the assigner for the pool. They would not be able to produce more often than the assigner allows anyway.
        /// Processes that only require work, such as eating, can update a lot more often.
        /// 
        /// </summary>
        public void RecomputeUpdateInterval(out bool intervalWasChanged)
        {
            intervalWasChanged = false;

            double? tempInterval = null, currentInterval = null;


            tempInterval = ProcessType.FinalUpdateInterval;
            UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);


            /*    tempInterval = UpdateTimePoints.ComputeIntervalFromTimepoint(this.ExpiryTimePointInSeconds);
                UpdateTimePoints.GetSoonestInterval(tempInterval, ref currentInterval);
                */

            if (!Common.IsEqual(UpdateInterval, currentInterval))
            {
                UpdateInterval = currentInterval; // if changed, will alert the sleepy updater to resort the list

                intervalWasChanged = true;
            }
        }


        bool RequiresContinuousSubstances()
        {
            return ProcessType.TotalContinuousSubstanceInputTypes != null && ProcessType.TotalContinuousSubstanceInputTypes.Count > 0;
        }

        /// <summary>
        /// send requests to the different substance pools we are connected to
        /// </summary>
        void RequestSubstances(float estimatedProgressDelta)
        {
            // compute needed amounts
          /*  float? estimatedProgressDelta;
            if (!EstimateProgressDelta(out estimatedProgressDelta))
            {

            }*/

            // place request in each pool
            //TODO:

        }

        enum Phase { RequestSubstances, Produce }

        Phase phase;


        public static SimProcess FindById(SimProcessID? id)
        {
            SimProcess process = LookUp<SimProcess, SimProcessID>.FindByID(id);
            return process;
        }

        bool EstimateProgressDelta(out float? progressDelta, 
            out float toolProductivity, out float skillProductivity, out float energyProductivity)
        {
            progressDelta = null;
            float currentProgress;
            if (!GetRealProgress(out currentProgress)) // this should not depend on knowledge about outputs.
            {
                toolProductivity = skillProductivity = energyProductivity = 0f;

                return false;
            }

            // get the work that is available:
            // workers cannot divide their work between processes, so there is no need to send requests. we can draw it directly. Also, pledging the work is not needed.
            float cumulativelyEstimatedProgress = currentProgress;
            progressDelta = 0f;

            // this includes tool productivitiy. too much hasssle to split calc by hand tools and other tools - we have a single factor computed already.
            if (ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded) // .RequiresWork)
            {
                EstimateProgressFromWorkers(ref progressDelta, ref cumulativelyEstimatedProgress, out toolProductivity, out skillProductivity, out energyProductivity);
            }
            else
            {
                float newProgressDelta;
                EstimateProgress(null, StationaryToolsTypeCombination,
                    currentProgress, 1f, out newProgressDelta, 
                    out toolProductivity, out skillProductivity, out energyProductivity); // progressDelta);

                progressDelta = newProgressDelta;

                //EstimateContinuousProgress(ref progressDelta);
            }


            EstimateProgressFromProperties(ref progressDelta);

            if (!ignoreProgressCap)
            {
                CapProgress(ref progressDelta);
            }

            return true;
        }


      
        

        private void CapProgress(ref float? totalProgressDelta)
        {
            if (ProcessType.ProgressPerSecondCap.HasValue)
            {
                float maxProgress = (float)(ProcessType.ProgressPerSecondCap.Value * GetUpdateInterval());
                totalProgressDelta = Math.Min(maxProgress, totalProgressDelta.Value);
            }

        }


        float GetUpdateInterval()
        {
            return (float)(updateInterval ?? The.Sim.GameTime.ElapsedGameTime.TotalSeconds);
        }


        /// <summary>
        /// used for modeling evaporation etc.
        /// </summary>
        /// <param name="totalProgressDelta"></param>
        private void EstimateProgressFromProperties(ref float? totalProgressDelta)
        {
            if (ProcessType.ProgressFactorProperties != null)
            {
                // read properties on input or actingOnEntity.
                foreach (var item in ProcessType.ProgressFactorProperties)
                {
                    PropertyResult? result = null;
                    float? factor = null;

                    Entity target = null;
                    if (item.UseActingOnEntity == true && ActingOnEntity.HasValue)
                    {
                        target = Entity.FindByID(ActingOnEntity.Value.Entity);                       
                    }
                    else if (item.InputType != null)
                    {
                        List<Tuple<EntityID, WorldLocation>> inputsOfType;
                        if (AssignedInputs.TryGetValue(GameData.Instance.AllEntityTypes[item.InputType], out inputsOfType))
                        {
                            target = Entity.FindByID(inputsOfType[0].Item1);
                        }
                    }                   
                    else if (item.Factor != null)
                    {
                        result = item.Factor.Evaluate(null, EntityAndRoot.GetEntity(ActingOnEntity), null, null);
                        if (result.HasValue && result.Value.NumberResult.HasValue)
                        {
                            factor = result.Value.NumberResult.Value;
                        }
                    }

                 
                    if (target != null)
                    {
                       
                        if (item.PropertyKey != null)
                        {
                            result = target.GetPropertyValue(item.PropertyKey, null);

                            if (result.HasValue && result.Value.NumberResult.HasValue)
                            {
                                factor = result.Value.NumberResult.Value;
                            }
                        }
                        else if (item.SubstanceKey != null)
                        {
                            SubstanceAmount amount;
                            if (target.SubstanceBulkAmounts.TryGetValue(GameData.Instance.AllSubstanceTypes[item.SubstanceKey], out amount))
                            {
                                factor = amount.Amount;
                            }
                        }
                    }


                    if (factor.HasValue)
                    {
                        if (item.ShiftByAmount.HasValue)
                        {
                            factor += item.ShiftByAmount.Value;
                        }

                        if (item.ScaleByAmount.HasValue)
                        {
                            factor *= item.ScaleByAmount.Value;
                        }

                        totalProgressDelta *= factor;                   
                    }                    
                }
            }
        }


     /*   private void EstimateProgressFromTools(ref float? totalProgressDelta)
        {
            float toolProductivity = ToolTypeCombination != null ? ToolTypeCombination.Productivity : 1f;

            totalProgressDelta *= toolProductivity;
        }*/

        private void EstimateProgressFromWorkers(ref float? totalProgressDelta, ref float cumulativelyEstimatedProgress,
            out float toolProductivity, out float skillProductivity, out float energyProductivity)
        {
            toolProductivity = skillProductivity = energyProductivity = 0f;

            if (workers != null && workers.Count > 0)
            {
                Entity worker;
                float averageLaborEfficiency = (float)ProcessJob.GetAverageLaborReturn(workers.Count, ProcessType.MaxWorkers);


                for (int i = workers.Count - 1; i >= 0; i--)
                {
                    var item = workers[i];
               
                    worker = Entity.FindByID(item.Item1);
                    if (worker != null)
                    {
                        float progressDelta;

                        float thisToolProductivity, thisSkillProductivity, thisEnergyProductivity;

                        EstimateProgress(worker, LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(item.Item2), cumulativelyEstimatedProgress, 
                            averageLaborEfficiency, out progressDelta,
                            out thisToolProductivity, out thisSkillProductivity, out thisEnergyProductivity);

                        toolProductivity += thisToolProductivity / workers.Count;
                        skillProductivity += thisSkillProductivity / workers.Count;
                        energyProductivity += thisEnergyProductivity / workers.Count;
                        

                        totalProgressDelta += progressDelta;
                        cumulativelyEstimatedProgress += progressDelta;

                        if (NonLivingEntity.IsCompleted(cumulativelyEstimatedProgress))
                            break;

                    }
                    else
                    {
                        // the worker is gone... remove from the list.
                        workers.Remove(item);

                        if (workers.Count == 0)
                        {
                            // this probably means this process should end..
                            totalProgressDelta = 0f;
                            HandleNoWorkers();
                        }
                    }
                }
            }
            else
            {
                
                totalProgressDelta = 0f;
                HandleNoWorkers();
            }
        }

        public bool GetFixedJobLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false)
        {
            location = null;

            IKnownEntityData outputData;
            if (!GetFirstOutputEntityData(out outputData, sharedKnowledge, ignoreKnowledge))
            {
                return false; // invalid output
            }

            if (outputData != null)
            {
                location = outputData.AccessPoint; 
                return true;
            }
            else
            {
                return GetProductionSiteLocation(out location, sharedKnowledge, ignoreKnowledge);
            }

        }

        /// <summary>   
        /// moved from ProcessJob
        /// Returns false if the site has become invalid.
        /// </summary>
        /// <returns></returns>
        public bool GetProductionSiteLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false)
        {
            location = null;

            if (ContainerToPlaceOutputsIn != null)
            {
                IKnownEntityData siteData;
                EntityResult result;
                if (ignoreKnowledge == true)
                {
                    siteData = Entity.FindByID(ContainerToPlaceOutputsIn.Value);
                    if (siteData != null)
                    {
                        result = EntityResult.SeenDirectly; // not necessarily
                    }
                    else
                    {
                        result = EntityResult.Destroyed;
                    }
                }
                else
                {
                    result = sharedKnowledge.GetKnownData(ContainerToPlaceOutputsIn.Value, out siteData);
                }

                if (GoalEvaluator.EntityDataResultCausesSkip(result))
                {
                    HandleDestroyedOutputContainer();

                    return false;
                }
                else
                {
                    location = siteData.AccessPoint;
                    return true;
                }
            }
            else if (GroundLocation.HasValue)
            {
                location = GroundLocation.Value;
                return true;
            }


            return true;
        }

       
        /// <summary>
        /// moved from ProcessJob
        /// 
        /// either tool or input can be used as parameter
        /// </summary>
        /// <param name="item"></param>
        /// <param name="location"></param>
        /// <param name="siteData"></param>
        /// <returns></returns>
        public bool GetImmovableEntityLocation(EntityID item, out Vector3? location, ref IKnownEntityData siteData, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false)
        {
            location = null;

            EntityResult result;
            if (ignoreKnowledge)
            {
                siteData = Entity.FindByID(item);
                if (siteData != null)
                {
                    result = EntityResult.SeenDirectly;
                }
                else
                {
                    result = EntityResult.Destroyed;
                }
            }
            else
            {
                result = sharedKnowledge.GetKnownData(item, out siteData);
            }
            
            if (GoalEvaluator.EntityDataResultCausesSkip(result)) //GoalEvaluator.EntityDataResultCausesSkip(owner.GetAllegiance().SharedKnowledge.GetKnownData(item, out siteData)))
            {
                return false;
            }
            else
            {
                location = siteData.AccessPoint;
                return true;
            }
        }

        /// <summary>
        /// moved from ProcessJob
        /// 
        /// returns false if output is no longer valid.
        /// output is null if the product does not exist yet
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        /// <param name="outputData"></param>
        /// <returns></returns>
        private bool GetFirstOutputEntityData(out IKnownEntityData outputData, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false)
        {
            EntityID? firstOutput = GetFirstOutputEntity();

            if (firstOutput.HasValue)
            {
                EntityResult result;
                if (ignoreKnowledge == true)
                {
                    outputData = Entity.FindByID(firstOutput.Value);

                    if (outputData != null)
                    {
                        result = EntityResult.SeenDirectly;
                    }
                    else
                    {
                        result = EntityResult.Destroyed;
                    }
                }
                else
                {
                    result = sharedKnowledge.GetKnownData(firstOutput.Value, out outputData);
                }
              

                if (GoalEvaluator.EntityDataResultCausesSkip(result))
                {
                    if (result == EntityResult.NewUnknownEntity) // common source of bugs...
                    {

                    }

                    HandleDestroyedOutput();

                    return false;  // invalid/destroyed output...
                }

                return true;
            }
            else
            {
                outputData = null;
                return true;
            }
        }


        /// <summary>
        /// moved from ProcessJob
        /// 
        /// ignoreKnowledge is for internal process use.
        /// 
        /// returns false if the job is now destroyed.
        /// the location of either the production site, the outputs or the immovable input, or null if no location exists yet.
        ///
        /// Destroys the job if the immovable tool is invalid.
        /// 
        /// tool and input will never both be filled!
        /// </summary>
        /// <returns></returns>
        public bool GetCurrentLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false)
        {
            IKnownEntityData inputData = null, toolData = null;
            Vector3? fixedLocation;
            if (GetFixedJobLocation(out fixedLocation, sharedKnowledge, ignoreKnowledge) == false)
            {
                location = null;
                return false;
            }

            if (!fixedLocation.HasValue)
            {
                // carcass, when 1st time evaluated
                if (ImmovableInput.HasValue)
                {
                    if (GetImmovableEntityLocation(ImmovableInput.Value, out location, ref inputData, sharedKnowledge, ignoreKnowledge))
                    {
                        return true;
                    }
                    else
                    {
                        HandleDestroyedInput(); // NEW

                        ImmovableInput = null;
                        return false;
                    }
                }
                else if (ImmovableTool.HasValue)
                {
                    if (GetImmovableEntityLocation(ImmovableTool.Value, out location, ref toolData, sharedKnowledge, ignoreKnowledge))
                    {
                        return true;
                    }
                    else
                    {
                        // if the immovable tool is invalid, then so is the job, and most likely the process too.
                        // OLD: destroy the job. TODO: destroy Process...?
                        HandleDestroyedImmovableTool();
                        //Destroy(true);

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
            }
        }

     

        #endregion

        public bool OutputExists(out bool outputExists, SharedKnowledge sharedKnowledge)
        {
            outputExists = false;
          
            IKnownEntityData entityData;
            if (GetFirstOutputEntityData(out entityData, sharedKnowledge))
            {
                if (entityData != null)
                {
                    outputExists = true;
                }
                else outputExists = false;

                return true;
            }

            return false;
        }


        /// <summary>
        /// returns false if invalid/destroyed output...
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        /// <param name="isCompleted"></param>
        /// <returns></returns>
        public bool IsCompleted(out bool isCompleted, SharedKnowledge sharedKnowledge)
        {
            isCompleted = false;
            float progress;
            if (GetKnownProgress(sharedKnowledge, out progress))
            {
                isCompleted = NonLivingEntity.IsCompleted(progress);
                return true;
            }

            return false;

        }
       
        /// <summary>
        /// only for direct use.
        /// </summary>
        /// <returns></returns>
        private bool IsCompleted()
        {
            if (ProcessType.HasNoEndpoint)
            {
                return false;
            }
           /* else if (ProcessType.RepairAction.HasValue)
            {
                switch(ProcessType.RepairAction.Value)
                {
                    case RepairAction.Integrity:
                        Entity actingOnEntity = Entity.FindByID(ActingOnEntity.Value);
                        if (actingOnEntity != null)
                        {
                            float? integrity = actingOnEntity.NonLivingEntity.Integrity;
                            return integrity.HasValue && Common.IsGreaterThanOrEqual(integrity.Value, 1f);
                        }
                        else
                        {
                            HandleDestroyedActingOn();
                        }
                        break;
                }

                return false;
            }*/
            else
            {
                float progress;
                if (GetRealProgress(out progress))
                {
                    if (NonLivingEntity.IsCompleted(progress))
                        return true;
                }
            }

            return false;
        }

        void CreateRegulators()
        {
            //ChanceToDestroyToolRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1d / GameData.Instance.Constants.ToolUseDestroyChanceInterval, "Process");
        }

        Dictionary<SubstanceType, Tuple<SubstancePoolID, float>> requestedSubstances;

        Dictionary<SubstanceType, Tuple<SubstancePoolID, float>> assignedSubstances;

        /// <summary>
        /// substance containers will call this asynchronically when we have been assigned a substance amount. 
        /// Any substance not used in this production round must be placed back in the substance container!
        /// </summary>
        public void AssignSubstance(SubstancePool pool, float amount)
        {
            requestedSubstances.Remove(pool.SubstanceType);

            Common.AddToDictionary(ref assignedSubstances, pool.SubstanceType, new Tuple<SubstancePoolID, float>(pool.ID, amount));
        }


        private void RequestSubstance()
        {
            // save the request if it was not satisfied immediately:

            // SubstancePool.Request();
        }


       
        private void HandleDestroyedOutput()
        {
            Destroy(); 
        }

        private void HandleDestroyedOutputContainer()
        {
            Destroy(); 
        }

        private void HandleDestroyedInput()
        {
            Destroy(); // tested for: build turnip hut, destroyed input in/out of FOW
                        // butcher carcass
        }

        private void HandleDestroyedActingOn()
        {
            Destroy();
        }

        private void HandleDestroyedImmovableTool()
        {
            Destroy(); 
        }

        private void HandleNoWorkers()
        {
            //allow paused construction...

        }

        private void HandleNoSubstances()
        {
            // TODO: halt/destroy should depend on the process.
            Destroy();
        }

        /// <summary>
        /// consume inputs and create outputs
        /// 
        /// Starting a job process should never happen in FOW. Either the robot/machine is in radio contact or there's an agent doing it.
        /// This should set the tools that will be used throughout, if the there is no agent involved.
        /// 
        /// should return a status that tells the agent about problems. The agent is responsible for setting/removing locks like AssignedToJob on inputs (and tools).
        /// 
        /// Only when started, the process will become visible on the map. Assign it to the Processes lists of all involved entities. If ProductionSiteLocation is set, add a reference on the tile map as well.
        /// 
        /// A process owned by a ProcessJob will always start visible. Either started by an agent, or by a remote controlled tool. In either case, it will be visible.
        /// 
        /// 
        /// 
        /// assign resource (substance) pools, as well as workers.
        /// They will receive continual requests while the process is ongoing.
        /// If a substance pool no longer exists, the process should be terminated.
        /// 
        /// 
        /// An interrupted job can later be taken up again. it will continue with the same process!
        /// TODO: Some tools should cause the process to stop if they are removed from the process.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="entity"></param>
        /// <param name="parentGoal"></param>
        /// <param name="processType"></param>
        /// <param name="ownerOfOutput"></param>
        /// <param name="energyNeeded"></param>
        public StatusOfProcess Start(Entity startingAgent, // can be null   
                                        OwnerID? ownerOfOutput,
                                        ToolTypeCombination stationaryToolsCombo,
                                        List<EntityID> stationaryTools // in cooking, this would be the pot. a knife could be a hand tool carried by an agent
                                       // bool produceWithoutInputs = false,
            ) 
                                       
        {
          //  IsStarted = true; set started after everything checks out...
                      
            if (startingAgent.EntityID == (EntityID)4941)
            {

            }

            this.ownerOfOutput = ownerOfOutput;          
            this.StationaryTools = stationaryTools;
            this.StationaryToolsTypeCombination = stationaryToolsCombo;
                    
            Status = StatusOfProcess.Active;

                    
            if (ProcessType.IsConstruction()
                && OutputEntities != null) // is null for the turnip hut. TODO: check cleared area for special actions too
            {
                if (!CheckStructureAreaCleared(0.01f)) // disabled for now...
                {
                    Status = StatusOfProcess.Failed;
                    return Status;
                }
            }

         
            // for ordered structures, this call does not create the entity (that has been done when the structure was ordered)
            // but sets its state and footprint on the map:
            if (!ConsumeAndCreateOutputs(startingAgent))
            {
                Status = StatusOfProcess.Failed;
                return Status;
            }

            // Turnip hut process will move the turnip shell (Acting On!) in as a part to turnip hut. So re-evaluate the root/part relationship:
            RecreateActingOnEntityRoot();
            

            AssignProcessToInvolvedEntitiesOrTile(false);

            IsStarted = true; // set started after everything checks out...

            // the outputs are still Progress = 0 so do not use memory yet.
            // after SimProcess.Update, Progress becomes > 0, it will use memory and return NewEntity which can cause goals to fail.
            // so we need to detect them now to avoid problems in the worker goal. 
            SeeOutputs(startingAgent);

            FireProductionStartedEvent();

          
            // for autonomous processes, startingAgent will be null, while actingOnEntity may be non-null
           
            if (startingAgent != null)
            {
                Goal.FireEventActions(startingAgent, EntityAndRoot.GetEntity(ActingOnEntity),
                    ProcessType.GetStartHook(), startingAgent.EntityType.IntelligenceType.EventActions,
                    AgentActionHooks.StartProducing, ProcessType.EventActions, this.suppressSpawningEvents);
            }
            else
            {
                Goal.FireEventActions(null, EntityAndRoot.GetEntity(ActingOnEntity),
                    ProcessType.GetStartHook(), ProcessType.EventActions, AgentActionHooks.StartProducing, null, suppressSpawningEvents);
            }

            if (Status == StatusOfProcess.Active)
            {
                if (IsInstantProcess())
                {
                    // finish now:
                    ProduceTillCompletion();
                }
                else
                {
                    The.Sim.PlaySite.PlaySite.AddProcess(this); // start receiving updates               
                }
            }

            return Status;
        }

        private void RecreateActingOnEntityRoot()
        {
            if (ActingOnEntity.HasValue)
            {
                Entity actingOnEntity = Entity.FindByID(ActingOnEntity.Value.Entity);

                ActingOnEntity = actingOnEntity.GetAsEntityAndRoot();
            }
        }


        private bool IsInstantProcess()
        {
            float? progressDelta;
            float toolProductivity, skillProductivity, energyProductivity;
            if (EstimateProgressDelta(out progressDelta, out toolProductivity, out skillProductivity, out energyProductivity))
            {
                return progressDelta >= 1f;
            }

            return false;
        }


        private void SeeOutputs(Entity byEntity)
        {
            if (OutputEntities != null)
            {              
               // IterateOutputEntities(e => byEntity.Intelligence.Allegiance.SharedKnowledge.SeeDetectableIfRelevant(Entity.FindByID(e)));
  
                // ignore uses memory check, the output entity would fail at this stage...
                IterateOutputEntities(e => byEntity.Intelligence.Allegiance.SharedKnowledge.SeeDetectable(Entity.FindByID(e)));
            }
        }



        /// <summary>
        /// outputs that have been created but with progress = 0 will never degrade, so destroy them now.
        /// </summary>
        private void DestroyUnstartedOutputs()
        {
            if (OutputEntities != null)
            {
                for (int i = OutputEntities.Count - 1; i >= 0; i--)
                {
                    Entity entity = Entity.FindByID(OutputEntities[i]);
                    if (entity != null 
                        && !entity.IsCompleted()
                        && (entity.IsStarted() != true 
                        || ProcessType.IsGathering)) // NEW: for harvest processes, destroy incomplete outputs. These processes cannot be resumed...
                    {
                        entity.Destroy();
                    }                    
                }
            }
        }

        private void AssignProcessToInvolvedEntitiesOrTile(bool remove)
        {
            bool wasAssignedToEntity = false;
            //set the reference to this process on all involved entities, so it will be visible to agents and to the player.
            if (StationaryTools != null)
            {
                foreach (var item in StationaryTools)
                {
                    Entity tool = Entity.FindByID(item);
                    if (tool != null)
                    {
                        if (remove)
                        {
                            tool.RemoveProcess(ID);
                        }
                        else
                        {
                            tool.AddProcess(this);
                            wasAssignedToEntity = true;
                        }
                    }
                }
            }

            if (AssignedInputs != null)
            {
                foreach (var item in AssignedInputs)
                {
                    foreach (var item2 in item.Value)
                    {
                        Entity input = Entity.FindByID(item2.Item1);
                        if (input != null)
                        {
                            if (remove)
                            {
                                input.RemoveProcess(ID);
                            }
                            else
                            {
                                input.AddProcess(this);
                                wasAssignedToEntity = true;
                            }
                        }
                    }
                }
            }

            if (OutputEntities != null)
            {
                foreach (var item in OutputEntities)
                {
                    Entity output = Entity.FindByID(item);
                    if (output != null)
                    {
                        if (remove)
                        {
                            output.RemoveProcess(ID);
                        }
                        else
                        {
                            output.AddProcess(this);
                            wasAssignedToEntity = true;
                        }
                    }
                }
            }

            if (ActingOnEntity.HasValue)
            {
                Entity entity = Entity.FindByID(ActingOnEntity.Value.Entity);
                if (entity != null)
                {
                    if (remove)
                    {
                        entity.RemoveProcess(ID);
                    }
                    else
                    {
                        entity.AddProcess(this);
                        wasAssignedToEntity = true;
                    }
                }
            }

            if (remove)
            {
                if (MapPosition.HasValue)
                {
                    The.Map.GetTile(MapPosition.Value).RemoveProcess(this);
                }
            }
            else
            {
                if (!wasAssignedToEntity)
                {
                    // add to the tile instead:
                    if (MapPosition.HasValue)
                    {
                        The.Map.GetTile(MapPosition.Value).AddProcess(this);
                    }
                }
            }

        }
        
      
        public void AssignWorker(Entity entity, ToolTypeCombination tools)
        {
            if (workers == null)
            {
                workers = new List<Tuple<EntityID, ToolTypeCombinationID?>>();
            }

            if (workers.Exists(w => w.Item1 == entity.ID))
            {
                // update (should not happen...)
                RemoveWorker(entity);
                
            }

            workers.Add(new Tuple<EntityID, ToolTypeCombinationID?>(entity.ID, tools != null ? tools.ID : (ToolTypeCombinationID?)null));

        }

        public void RemoveWorker(Entity entity)
        {
            if (workers != null)
            {
                workers.RemoveAll(e => e.Item1 == entity.ID);
            }
        }

        /// <summary>
        /// consume substances from a pool that we received via request
        /// </summary>
        /// <param name="progressDelta"></param>
        /// <returns></returns>
        private float ConsumePooledSubstances(float progressDelta)
        {
            if (!RequiresContinuousSubstances())
            {
                return progressDelta;
            }

            float newProgressDelta = progressDelta;

            foreach (var item in ProcessType.TotalContinuousSubstanceInputTypes)
            {
                float availableSubstance;
                Tuple<SubstancePoolID, float> availableSubstanceInfo;

                if (!assignedSubstances.TryGetValue(item.Key, out availableSubstanceInfo))
                {
                    availableSubstance = 0f;
                    return availableSubstance; // Abort.
                }
                else
                {
                    availableSubstance = availableSubstanceInfo.Item2;
                }

                float substanceProgressFraction = availableSubstance / item.Value;
                if (substanceProgressFraction < 1f)
                {
                    // limit the progress:
                    float thisProgress = progressDelta * substanceProgressFraction;
                    if (thisProgress < newProgressDelta)
                    {
                        newProgressDelta = thisProgress;
                    }
                }
            }

            // now consume:
            if (newProgressDelta > 0f)
            {
                foreach (var item in ProcessType.TotalContinuousSubstanceInputTypes)
                {
                    Tuple<SubstancePoolID, float> availableSubstanceInfo;

                    assignedSubstances.TryGetValue(item.Key, out availableSubstanceInfo);
                    SubstancePool pool = LookUp<SubstancePool, SubstancePoolID>.FindByID(availableSubstanceInfo.Item1);

                    float substanceToConsume = item.Value * newProgressDelta;
                    if (pool != null)
                    {
                        pool.Consume(substanceToConsume);

                    }     
                    else 
                    {
                         return 0f; // Abort.
                    }
                }
            }

            return newProgressDelta;
        }

        /// <summary>
        /// Produce with this amount of energy and the previously calculated max progress for the other factors
        /// Returns an status of the production.  (Failed,Active,Complete)
        /// 
        /// "Energy" should be a SubstanceAmount
        /// 
        /// Arguments here should be a list of substances...
        /// </summary>      
        private StatusOfProcess Produce(float progressDelta, out float realizedProgressDelta)        
        {

            Status = StatusOfProcess.Active;

            // even if there are more outputs, they should all have the same progress:
            float currentProgress;
            if (!GetRealProgress(out currentProgress))
            {
                Status = StatusOfProcess.Failed;
                realizedProgressDelta = 0f;
                return Status;
            }

            /*  if (job != null)
              {
                  // for multiple workers - perhaps reintroduce this feature later:
                  EstimateCumulativeProductionOnJob(elapsed, entity, currentProgress);
              }
              else
              {
                  float progressDelta;
                  ComputeMaximumProgress(entity, currentProgress, 1f, out progressDelta);
              }


              if (job != null)
              {
                  job.EstimationHasStarted = false;
              }
              */

            
            // limit the progress by the substances we have available:
            progressDelta = ConsumePooledSubstances(progressDelta);

            float newProgress = currentProgress + progressDelta;

            if (newProgress > 1f)
            {
                // clamp progress delta:
                float excessDelta = newProgress - 1f;
                realizedProgressDelta = progressDelta - excessDelta;
            }
            else
            {
                realizedProgressDelta = progressDelta;
            }

          //  newProgress = Math.Min(1, newProgress);

            float elapsedTime = GetUpdateInterval(); // The.Sim.GameTime.ElapsedGameTime.TotalSeconds;               
            ProgressSpeed = progressDelta / elapsedTime;
                
            if (Common.IsZero(progressDelta))
            {
                // no work could be done:
                // TODO: for how long should a process be allowed to stall before the job should be destroyed..?
                Status = StatusOfProcess.Failed;
                realizedProgressDelta = 0f;
                return Status;
            }
            else
            {

                HandleWorkersAndTools(elapsedTime, progressDelta);

                // set the same progress on all outputs:

                if (ProcessType.HasOutput) 
                {
                    bool hasMissingOutput = false;
                    IterateOutputEntities(output =>
                    {
                        Entity outputEntity = Entity.FindByID(output);

                        if (outputEntity != null)
                        {
                            outputEntity.NonLivingEntity.Progress = newProgress;
                        }
                        else
                        {
                            // output missing!                                  
                            hasMissingOutput = true;
                        }

                    });

                    if (hasMissingOutput)
                    {
                        Status = StatusOfProcess.Failed;
                        realizedProgressDelta = 0f;
                        return Status;
                    }

                }
                else
                {
                    SetProgressForNoOutput(newProgress);
                }
            }

            return Status;
        }


        public double? ComputeEstimatedCompletionTime()
        {
            if (IsStarted)
            {
                if (!Common.IsZero(ProgressSpeed))
                {
                    float currentProgress;
                    if (!GetRealProgress(out currentProgress))
                    {
                        return null;
                    }

                    double timeLeft = (1f - currentProgress) / ProgressSpeed;

                    timeLeft = Common.ClampBottom(timeLeft, 0d);

                    return The.Sim.TotalUnPausedGameTimeInSeconds + timeLeft;

                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            } 
        }


        private void HandleWorkersAndTools(float elapsed, float progressDelta)
        {
            if (workers != null)
            {
                for (int i = workers.Count - 1; i >= 0; i--)
                {
                    EntityID worker = workers[i].Item1;
                    Entity workerEntity = Entity.FindByID(worker);
                    if (workerEntity != null)
                    { 
                        // hand tools should be handled by the worker!
                        workerEntity.Intelligence.PerformWorkAndAffectHandTools(this, elapsed, progressDelta);
                                               
                       // bool toolsOK = WearDownTools(entity, elapsedTime, tools, ToolTypeCombination, ChanceToDestroyToolRegulator.IsReady());

                        /*  if (!toolsOK)
                          {
                              Status = StatusOfProcess.Failed;
                              return Status;
                          }*/

                    }
                }
            }

            // handle non-hand tools:
            if (StationaryTools != null && StationaryTools.Count > 0)
            {
                bool toolsOK = WearDownTools(null, elapsed, StationaryTools, StationaryToolsTypeCombination);  
                    //false); // Morten has disabled premature destruction.  ChanceToDestroyToolRegulator.IsReady());


            }
        }


       

        public void SetProgressForNoOutput(float progress)
        {
            if (ProcessType.RepairAction == RepairAction.Integrity ||
                ProcessType.RepairAction == RepairAction.Condition ||
                ProcessType.RepairAction == RepairAction.PartsCondition)
            {
                // repair
                if (ActingOnEntity.HasValue)
                {
                    Entity actingOnEntity = Entity.FindByID(ActingOnEntity.Value.Entity);
                    if (actingOnEntity != null)
                    {
                        actingOnEntity.NonLivingEntity.Repair(ProcessType.RepairAction.Value, progress);
                    }
                }
            }
            else if (ProcessType.RepairAction != null && RepairType.IsReplaceAction(ProcessType.RepairAction.Value))
            {
                // repair - replace
                Progress = progress;
            }
            else
            {
                Progress = progress;
            }         

        }


      
     /*   public bool GetKnownProgress(out float progress)
        {
            if (ProcessType.HasOutput)
            {
                EntityGroup resolvedOwner;
                if (!job.ResolveOwner(out resolvedOwner))
                {
                    progress = 0f;
                    return false;
                }

                return GetKnownProgressFromOutputs(resolvedOwner.GetAllegiance().SharedKnowledge, OutputEntities, out progress);
            }
            else
            {
                progress = Progress;
            }

            return true;
        }*/


        /// <summary>
        /// move output entities to SimProcess
        /// give Job and Goal a reference to Process - assign it when the Process starts
        /// </summary>
        /// <param name="iterateMethod"></param>
        public void IterateOutputEntities(Action<EntityID> iterateMethod)
        {            
            if (OutputEntities != null)
            {
                foreach (var item in OutputEntities)
                {
                    iterateMethod(item);
                }
            }
        }

        private bool GetRealProgressFromOutputs(out float progress)
        {
            EntityID? firstOutput = GetFirstOutputEntity(OutputEntities);

            progress = 0f;

            if (firstOutput.HasValue)
            {
                Entity outputEntity = Entity.FindByID(firstOutput.Value);

                if (outputEntity == null)
                {
                    progress = 0f;
                    return false; // invalid/destroyed output...                   
                }

                progress = outputEntity.Progress.Value;
            }

            return true;
        }

        private bool GetRealRepairProgress(out float progress)
        {            
            progress = 0f;

            if (ActingOnEntity.HasValue)
            {
                Entity actingOn = Entity.FindByID(ActingOnEntity.Value.Entity);

                if (actingOn == null)
                {
                    progress = 0f;
                    return false; // invalid/destroyed output...                   
                }

                progress = actingOn.GetRepairProgress(ProcessType.RepairAction.Value); //, null);
            }

            return true;
        }

        private bool GetKnownRepairProgress(SharedKnowledge sharedKnowledge, /*EntityID? partToFix,*/ out float progress)
        {
           // Entity actingOnEntity = Entity.FindByID(ActingOnEntity.Value);

            progress = 0f;

            if (ActingOnEntity.HasValue)
            {
                IKnownEntityData outputData;
                if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(ActingOnEntity.Value.Entity, out outputData)))
                {
                    progress = 0f;
                    return false; // invalid/destroyed output...
                    //return 0f; 
                }

                progress = outputData.GetRepairProgress(ProcessType.RepairAction.Value); //, partToFix);
              
            }

            return true;
        }

        private static bool GetKnownProgressFromOutputs(SharedKnowledge sharedKnowledge, List<EntityID> outputs, out float progress)
        {
            EntityID? firstOutput = GetFirstOutputEntity(outputs);

            progress = 0f;

            if (firstOutput.HasValue)
            {
                IKnownEntityData outputData;
                if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(firstOutput.Value, out outputData)))
                {
                    progress = 0f;
                    return false; // invalid/destroyed output...
                    //return 0f; 
                }

                progress = outputData.Progress.Value;
            }

            return true;
        }

        public static EntityID? GetFirstOutputEntity(List<EntityID> outputs)
        {
            if (outputs != null && outputs.Count > 0)
            {
                return outputs[0];
            }
            else return null;
        }

        public EntityID? GetFirstOutputEntity()
        {
          /*  List<EntityID> outputs = null;
            if (job != null)
            {
                outputs = job.OutputEntities;
            }
            else
            {
                outputs = OutputEntities;
            }*/

            return GetFirstOutputEntity(OutputEntities);
        }

       

        /// <summary>
        /// for outside checking... 
        /// call from Job if possible.
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public bool GetKnownProgress(SharedKnowledge sharedKnowledge, out float progress)
        {
            if (ProcessType.RepairAction.HasValue)
            {
                if (RepairType.IsReplaceAction(ProcessType.RepairAction.Value)) // == RepairAction.ReplacePart)
                {
                    progress = Progress;
                    return true;
                }
                else
                {
                    return GetKnownRepairProgress(sharedKnowledge, out progress);
                }
            }
            else if (OutputEntities != null && OutputEntities.Count > 0)
            {
                return GetKnownProgressFromOutputs(sharedKnowledge, OutputEntities, out progress);
            }
            else
            {
                // only if there is no output entity do we use this! (for place smoke bomb and other special actions)
                progress = Progress;
                return true;
            }

        }

        /// <summary>
        /// for repair, we use the condition of the part/actingOnEntity to mark progress
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        private bool GetRealProgress(out float progress)
        {
            if (ProcessType.RepairAction.HasValue)
            {
                if (RepairType.IsReplaceAction(ProcessType.RepairAction.Value)) // == RepairAction.ReplacePart)
                {
                    progress = Progress;
                    return true;
                }
                else
                {
                    return GetRealRepairProgress(out progress);
                }
            }
            else if (OutputEntities != null && OutputEntities.Count > 0)
            {
                return GetRealProgressFromOutputs(out progress);
            }
            else
            {
                // only if there is no output entity do we use this! (for place smoke bomb and other special actions)
                progress = Progress;
                return true;
            }

        }


       

        public void AddOutputEntity(Entity outputEntity)
        {         
            if (OutputEntities == null)
            {
                OutputEntities = new List<EntityID>();
            }

            OutputEntities.Add(outputEntity.EntityID);              
        }

     /*   private void InitJobEstimate(float currentProgress)
        {

            job.CumulativelyEstimatedProgress = currentProgress;
            job.CumulativelyEstimatedNeededEnergy = 0;

            job.EstimationHasStarted = true;


        }*/


        /// <summary>
        /// only allow multiple workers if Job is defined!
        /// </summary>
        /// <param name="elapsed"></param>
        /// <param name="worker"></param>
        /// <param name="currentProgress"></param>
      /*  private void EstimateCumulativeProductionOnJob(GameTime elapsed, Entity worker, float currentProgress)
        {


            if (job.TakenBy.Count > 1)
            {
                // are we the first worker in this cycle?
                if (!job.EstimationHasStarted)// job.CumulativelyEstimatedNeededItems == null)
                {
                    InitJobEstimate(currentProgress);
                }
            }
            else
            {
                // only one worker - don't make copies:
                //   assignedInputs = job.AssignedInputsToThisJob;
                //    neededItems = job.NeededItems;
                // init:
                job.CumulativelyEstimatedProgress = currentProgress;
                job.CumulativelyEstimatedNeededEnergy = 0;
            }

            if (!NonLivingEntity.IsCompleted(currentProgress))
            {
                float progressDelta;

                ComputeMaximumProgress(worker, job.CumulativelyEstimatedProgress, job.AverageLaborEfficiency, out progressDelta);
                                
                // increase the 'progress' for the next worker to estimate accurately:
                job.CumulativelyEstimatedProgress = maxPossibleProgress;

            }
        }*/

       


        private void EstimateProgress(Entity worker, ToolTypeCombination tools, float currentProgress, float averageLaborEfficiency, out float progressDelta,
            out float toolProductivity, out float skillProductivity, out float energyProductivity)
        {
            double elapsedTimeInSeconds = GetUpdateInterval();

            toolProductivity = tools != null ? tools.Productivity : 1f; 
           
            //float skillFactor = 1f;
            
          //  float energyLevel = 1f;
            if (worker != null)
            {
                skillProductivity = worker.Intelligence.GetSkillProductionFactor(ProcessType.RequiredSkillType);

                energyProductivity = ProcessType.GetEnergyProductivity(worker);
                /*
                if (worker.BiologicalEntity != null)
                {
                    energyLevel = worker.BiologicalEntity.EnergyLevel;
                }*/
            }
            else
            {
                skillProductivity = 1f;
                energyProductivity = 1f;
            }


            progressDelta = ProcessType.CalculateProgressDelta(elapsedTimeInSeconds, /*worker*/ skillProductivity, energyProductivity, toolProductivity, 1f, averageLaborEfficiency);
         
            //  progressDelta = ProcessType.CalculateProgressDelta(elapsedTimeInSeconds, worker, toolProductivity, 1f, averageLaborEfficiency);

        }

      /*  private void EstimateContinuousProgress(ref float? progressDelta)
        {
            double elapsedTimeInSeconds = GetUpdateInterval();

            float toolProductivity = StationaryToolsTypeCombination != null ? StationaryToolsTypeCombination.Productivity : 1f;

            progressDelta = ProcessType.CalculateProgressDelta(elapsedTimeInSeconds, 1f, 1f, toolProductivity, 1f);
            //  progressDelta = ProcessType.CalculateProgressDelta(elapsedTimeInSeconds, null, toolProductivity, 1f);

        }*/

        /// <summary>
        /// public for testing only
        /// </summary>
        public void ProductionFinished()
        {

            Status = StatusOfProcess.Complete;

            Entity worker = null;
            if (workers != null && workers.Count > 0)
            {
                worker = Entity.FindByID(workers[0].Item1);
            }

            IterateOutputEntities(output =>
            {
                Entity outputEntity = Entity.FindByID(output);
                if (outputEntity != null)
                {
                    ProductionFinishedForSingleOutput(outputEntity, worker);
                }
            });

            // to replenish with event spawned items, find some other way, this code cannot be shared...
            if (ProcessType.IsReplenishProcess)
            {
                Entity entityToReplenish = Entity.FindByID(EntityAndRoot.GetEntity(ActingOnEntity));
                if (entityToReplenish != null)
                {
                    IOwner entityToReplenishOwner = null;
                    if (LookUpOwners.ResolveEntityOwner(entityToReplenish, out entityToReplenishOwner)) // hmm, why use the owner here..?
                    {
                        Dictionary<EntityType, List<Entity>> assignedInputData = new Dictionary<EntityType, List<Entity>>();
                        Entity itemToConsumeEntity = null;
                        if (!ValidateInputs(worker, ref itemToConsumeEntity, assignedInputData)) // only replenish keeps the inputs in this list...
                        {
                            Status = StatusOfProcess.Failed;
                            Destroy();
                            return;
                            //return false;
                        }

                        List<Entity> itemEntities;
                        Input input;
                        foreach (var kvp in ProcessType.InputsByType)
                        {
                            input = kvp.Value;
                            if (assignedInputData.TryGetValue(input.EntityType, out itemEntities))
                            {
                                for (int i = 0; i < input.Amount.NoOfItems; i++)
                                {
                                    itemToConsumeEntity = HandleSingleInputItem(itemEntities); // calling this removes elements one at a time.

                                    if (itemToConsumeEntity.ContainedBy.HasValue)
                                    {
                                        // NEW: remove from container (only replenish needs this?)
                                        Entity container = Entity.FindByID(itemToConsumeEntity.ContainedBy);
                                        if (container != null)
                                        {
                                            container.Contains.Remove(itemToConsumeEntity); // this leaves the entity in an illegal state, Site is now null... nad we need it in Destroy.
                                        }
                                    }

                                    GoalReplenish.Replenish(worker, ProcessType.ReplenishAction.Value, itemToConsumeEntity, entityToReplenish, entityToReplenishOwner, StorageCompartment.Haul);
                                }
                            }
                        }
                    }
                }
            }


            if (!ProcessProductionFinished(worker, ProcessType, EntityAndRoot.GetEntity(ActingOnEntity), GetFirstOutputEntity(), this.suppressSpawningEvents)) // this disables special actions
            {
                Status = StatusOfProcess.Failed;
                Destroy();
                return;
            }


            FireProductionFinishedEvent();

            Destroy();  // will already have been called by ProcessJob if it exists. this enables special actions

        }


        /// <summary>
        /// can be called from spawn events too
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="processType"></param>
        /// <param name="actingOnEntity"></param>
        /// <returns></returns>
        public static bool ProcessProductionFinished(Entity worker, ProcessType processType, EntityID? ActingOnEntity, EntityID? firstOutputEntity, bool isSpawning)
        {
            
            if (processType.SetPreparedProperty == true)
            {
                System.Diagnostics.Debug.Assert(ActingOnEntity.HasValue, "no tool set..");

                Entity entityToReplenish = Entity.FindByID(ActingOnEntity);
                if (entityToReplenish != null)
                {
                    SimSide.Items.Tool tool;
                    if (entityToReplenish.Find(out tool))
                    {
                        tool.IsPrepared = true;

                        if (tool.IsPrepared != true)
                        { 
                            // can fail..
                            /*Status = StatusOfProcess.Failed;

                            Destroy();*/

                            return false; // Status;
                        }

                    }
                }
            }

            Entity actingOnEntity = Entity.FindByID(ActingOnEntity);

            if (processType.AttacksVerminValueToSet.HasValue)
            {
                if (actingOnEntity != null
                    && actingOnEntity.EntityType.IntelligenceType != null)
                {
                    actingOnEntity.Intelligence.AttacksVermin = processType.AttacksVerminValueToSet.Value;
                }
            }


            EntityID? targetEntity;

            AgentActionHooks? extraActionHook = null;
            if (processType.IsConstruction()) 
            {
                extraActionHook = AgentActionHooks.CompletedConstructing;

                // target is always the output
                targetEntity = firstOutputEntity;
            }
            else if (processType.IsGathering)
            {
                extraActionHook = AgentActionHooks.CompletedHarvesting;
                targetEntity = firstOutputEntity;
            }
            else
            {
               // extraActionHook = AgentActionHooks.CompletedProducing;
                
                // target is either the actingOnEntity or the output
                if (ActingOnEntity.HasValue) 
                {
                    targetEntity = ActingOnEntity.Value; 
                }
                else
                {
                    // use first output...
                    targetEntity = firstOutputEntity;
                }
            }

           
            Dictionary<AgentActionHooks, List<ActionSets>> workerActions = null;
            if (worker != null)
            {
                workerActions = worker.EntityType.IntelligenceType.EventActions;
            }

            // NEW - fire two sets of events
            if (extraActionHook.HasValue)
            {
                Goal.FireEventActions(worker, targetEntity,
                    extraActionHook.Value, workerActions,
                    extraActionHook.Value, processType.EventActions, isSpawning);
            }

            // NEW: always fire CompletedProducing - this makes it less error prone to script spawned buildings which can be either constructed or process spawned
            // the other more specific hooks can then be used for talks.
            Goal.FireEventActions(worker, targetEntity,
                AgentActionHooks.CompletedProducing, workerActions,
                AgentActionHooks.CompletedProducing, processType.EventActions, isSpawning);


            // also fire an entity event:
            // spawn actions should also result in this being fired
            // should fire on all output, not just one entity...
           /* Entity output = Entity.FindByID(firstOutputEntity);
            if (output != null)
            {
                List<ActionSets> defaultActions;
                EntityType.EventActions.TryGetValue(EntityEventHooks.Created, out defaultActions);
                Goal.FireEventActions(this, null, defaultActions);
            }*/


            if (actingOnEntity != null)
            {
                // SimProcess enables physical special actions - same for all allegiances.

              
                if (processType.DisablesSharedActionProcessTypes != null)
                {
                    foreach (var item in processType.DisablesSharedActionProcessTypes) 
                    {
                        actingOnEntity.DisableSharedSpecialAction(item.OriginalProcess);
                    }
                }

                // SimProcess enables physical special actions - same for all allegiances.
                // Enabling locks for an allegiance should be done in Job!
                if (processType.EnablesSharedActionProcessTypes != null)
                {
                    foreach (var item in processType.EnablesSharedActionProcessTypes)
                    {
                        actingOnEntity.EnableSharedSpecialAction(item.OriginalProcess);
                    }
                }


                if (processType.UsesAnchor()) // only physical availability
                {
                    actingOnEntity.DisableSpecialActionsUsingAnchor();
                }
            }

            return true;
        }


        private bool PickupHarvestedItem(Entity worker, Entity harvestedItem)
        {
           
            IOwner owner = LookUpOwners.FindByID(ownerOfOutput);
            if (owner == null)
            {
                // allow no owner?
               // Status = Goals.Status.Failed;
                return false;
            }

            // pick up immediately?
            if (harvestedItem.Item.Pickup(worker, owner, StorageCompartment.Haul))
            {
                return true;
            }
            else
            {
                // no room? 
                // leave it on the ground:
                harvestedItem.PlaceEntityOnPlaySite(worker.PlaySiteLocation, null, null, new Entity.SetOwnerInfo(owner));

                //harvestedItem.PlaceNewEntityInTheOpen(worker.PlaySiteLocation, owner);

                return false;
            }
        }


        private void FireProductionStartedEvent()
        {
            if (ProcessStartedEvent != null)
            {
                ProcessStartedEvent.Invoke(this); // should not be acted on if in FOW...
            }
        }

        private void FireProductionFinishedEvent()
        {
            if (ProcessCompletedEvent != null)
            {
                ProcessCompletedEvent.Invoke(this); // should not be acted on if in FOW...
            }
        }

        private void FireProducingEvent()
        {
            if (ProcessProducingEvent != null)
            {
                ProcessProducingEvent.Invoke(this); // should not be acted on if in FOW...
            }
        }

       /* private void FireInputDestroyedEvent(Entity input)
        {
            if (InputDestroyedEvent != null)
            {
                InputDestroyedEvent.Invoke(this, input); // should not be acted on if in FOW...
            }
        }*/

        private void FireProcessDestroyedEvent()
        {
            if (ProcessDestroyedEvent != null)
            {
                ProcessDestroyedEvent.Invoke(this); // should not be acted on if in FOW...
            }
        }


        private void ProductionFinishedForSingleOutput(Entity outputEntity, Entity worker)
        {            
            outputEntity.ComeOnline(this.suppressSpawningEvents);

            if (outputEntity.Structure != null)
            {
                EntityID? anchorID = null;
                if (ProcessType.IsSpecialActionType)
                {
                    anchorID = EntityAndRoot.GetEntity(ActingOnEntity);
                }

                outputEntity.Structure.ConstructionFinished(anchorID, false); // createParts); // structure parts are created when construction begins, not when the structure is placed                

            }
            else if (ProcessType.MoveOutputToWorkerWhenCompleted // IsHarvesting
                && worker != null
                && worker.EntityType.ContainerType != null 
                && worker.AgentStorage != null)
            {
            
                // not all items should be picked up immediately, like when digging...
                    PickupHarvestedItem(worker, outputEntity); // we allow the harvested item to stay on the ground if no room                
            
            }            


        }

       

        /// <summary>
        /// returns false if the tools are unusable
        /// 
        /// static, so that unattended production may be possible
        /// </summary>
        /// <param name="elapsedTime"></param>
        /// <returns></returns>
        public static bool WearDownTools(Entity producingEntity, float elapsedTime, List<EntityID> tools, ToolTypeCombination toolTypeCombination /*, bool rollForChanceToDestroy*/)
        {
            if (tools == null)
                return true;

            Tuple<EntityType, float> toolEntityType;
            float destructibility, degradeFactor;

            //bool rollForChanceToDestroy = parentGoal.chanceToDestroyToolRegulator.IsReady();

            Entity toolEntity;

            foreach (var tool in tools) 
            {
                // get the physical tool entity, even if we are producing unattended, the tool has to exist:
                toolEntity = Entity.FindByID(tool);
                if (toolEntity == null)
                {
                    return false;
                }

                toolEntityType = toolTypeCombination.Tools.Find(tt => tt.Item1 == toolEntity.EntityType);
                degradeFactor = toolEntityType.Item2;
                destructibility = 1f - /*toolEntityType.Item1.*/ toolEntity.EntityType.ToolType.Durability.Value;

                float damage = elapsedTime * destructibility * degradeFactor;

                if (damage > 0f)
                {
                    // damage the tool parts               
                    // only do condition damage to leaf nodes. the higher levels then have a computed condition that is the average of their parts.
                    /*if (tool.Parts != null && tool.Parts.Count > 0)
                    {*/
                    bool partWasDestroyed = toolEntity.DoDamage(damage); //, rollForChanceToDestroy);


                    if (partWasDestroyed)
                    {
                        if (producingEntity != null)
                        {
                            The.Client.AddLogEvent(producingEntity.Intelligence.Allegiance, The.Client.Log.EconomicEvent, producingEntity, "A " + toolEntity.EntityType.Name.ToLower(Config.Culture) + " broke while " + producingEntity + " was working with it.");
                        }

                        return false;
                    }
                }
            }

            return true;
        }



        public List<Tuple<EntityID, WorldLocation>> GetAssignedInputs(EntityType entityType)
        {
            List<Tuple<EntityID, WorldLocation>> inputs = null;
            if (AssignedInputs != null)
            {
                AssignedInputs.TryGetValue(entityType, out inputs);
            }
          /*  else if (job != null)
            {
                job.InputsAssignedAndOnSite.TryGetValue(entityType, out inputs);
            }*/

            return inputs;
        }



        /// <summary>
        /// create all outputs/structure, consume all inputs. set progress on outputs to 0.
        /// Returns false if production failed, and sets Status to Failed
        /// </summary>
        /// 
        private bool ConsumeAndCreateOutputs(Entity startingAgent) //, bool produceWithoutInputs) 
        {
            if (ContainerToPlaceOutputsIn == null)
            {
               // if (ProcessType.ContainerToPlaceOutputsIn )

            }

            Entity placeProductsInContainer = null;
            if (ContainerToPlaceOutputsIn != null)
            {
                placeProductsInContainer = Entity.FindByID(ContainerToPlaceOutputsIn.Value);
                if (placeProductsInContainer == null)
                {
                    Status = StatusOfProcess.Failed;
                    return false;
                }
            }

            Dictionary<EntityType, List<Entity>> parts;
            float totalBulkOfInput;
            Dictionary<string, float> extractedSubstances;

            Dictionary<EntityType, List<Entity>> assignedInputData = new Dictionary<EntityType, List<Entity>>();
            Entity itemToConsumeEntity = null;
            Vector3? lastInputLocation = null;
            if (this.suppressSpawningEvents == false) // produceWithoutInputs == false)
            {
                if (!ValidateInputs(startingAgent, ref itemToConsumeEntity, assignedInputData))
                {
                    return false;
                }

                // consume inputs            
                // gather the list of parts that we need to place in output items
                ConsumeInputsAndGatherParts(startingAgent, assignedInputData, out parts, out totalBulkOfInput, out extractedSubstances, out lastInputLocation); //out extractedBulk);
            }
            else
            {
                // for scripting (spawn fishtrap via process).
                parts = new Dictionary<EntityType, List<Entity>>(); // produce with new entity parts
                totalBulkOfInput = 0f;
                extractedSubstances = null;
            }


            if (ProcessType.HasOutput)
            {               
                //******
                // create outputs           
                
                foreach (var output in ProcessType.Outputs)
                {
                    int noOfItemsToCreate = 1;
                    float? bulkOfEachOutputItem; // if null, use fixed bulk for the entity type

                    //totalBulkOfOutputs = 
                    // compute the amounts we are going to create:
                    if (!output.GetOutputAmountsToCreate(totalBulkOfInput, extractedSubstances, out noOfItemsToCreate, out bulkOfEachOutputItem))
                    {
                        Status = StatusOfProcess.Failed;
                        return false;
                    }                    

                    // now create instances of one output type:
                    // if no substances were extracted, no outputs will be created either.
                    if (!CreateOutputsFromInputs(startingAgent, ProcessType, placeProductsInContainer, parts, output, noOfItemsToCreate, bulkOfEachOutputItem))
                    {
                        Status = StatusOfProcess.Failed;
                        return false;
                    }
                }

                // structures
                Structure structureComponent;

                if (OutputEntities != null)
                {
                    foreach (var output in OutputEntities) //  job.OutputEntities)
                    {
                        Entity outputEntity = Entity.FindByID(output);
                        if (outputEntity != null)
                        {
                            if (outputEntity.Find(out structureComponent))  // also byproducts as output???
                            {
                                if (!structureComponent.ConstructionHasStarted()) // output.NonLivingEntity.Progress == 0f) 
                                {   // we are under way now.

                                    EntityID? anchorID = null;
                                    if (ProcessType.IsSpecialActionType)
                                    {
                                        anchorID = EntityAndRoot.GetEntity(ActingOnEntity);
                                    }

                                    structureComponent.ConstructionStarted(anchorID);
                                }
                            }
                        }
                        else
                        {
                            // ordered output is missing:
                            Status = StatusOfProcess.Failed;
                            return false;
                        }
                    }
                }
                else
                {
                    // the process was supposed to yield an output, but nothing could be created. Fail.
                    Status = StatusOfProcess.Failed;
                    return false;
                }
            }



            if (ProcessType.IsSalvageProcess)
            {
                // for salvage jobs, destroy the parts that were not converted to outputs:
                // TURNIPTEST - do all parts become outputs???
                if (assignedInputData.Count > 1)
                {
                    throw (new Exception("Salvaging processes are assumed to only have one input!"));
                }

                DestroyUnusedInputParts(startingAgent, parts, itemToConsumeEntity, lastInputLocation);
            }
           


            return true;

        }


       

        private bool ValidateInputs(Entity startingAgent, ref Entity itemToConsumeEntity, Dictionary<EntityType, List<Entity>> assignedInputData)
        {
            Input input;
            List<Tuple<EntityID, WorldLocation>> itemLocationsAndIDs;
            List<Entity> itemEntities;

            // first test that all inputs are valid (entities must actually exist, not just be believed to exist):
            // also a proximity test
            if (ProcessType.InputsByType != null)
            {
                foreach (var kvp in ProcessType.InputsByType)
                {
                    if (kvp.Key.TreeType != null)
                    {
                        continue;// skip tree inputs for harvesting... perhaps later, resource containers will become substance containers..?
                    } 

                    input = kvp.Value;

                    itemEntities = new List<Entity>();
                    assignedInputData.Add(kvp.Key, itemEntities);

                    itemLocationsAndIDs = GetAssignedInputs(input.EntityType);
                    if (itemLocationsAndIDs != null)
                    {
                        if (itemLocationsAndIDs.Count >= kvp.Value.Amount.NoOfItems)
                        {
                            for (int i = 0; i < kvp.Value.Amount.NoOfItems; i++)
                            {
                                var itemLocation = itemLocationsAndIDs[i];

                                itemToConsumeEntity = Entity.FindByID(itemLocation.Item1);

                                if (!IsMaterialOnSite(startingAgent, itemToConsumeEntity, itemLocation.Item2))
                                {
                                    // the material is destroyed or no longer on site.                             
                                    HandleDestroyedInput(); 
                                    
                                    Status = StatusOfProcess.Failed;
                                    return false;
                                }
                                else
                                {
                                    itemEntities.Add(itemToConsumeEntity);
                                }
                            }
                        }
                        else
                        {
                            Status = StatusOfProcess.Failed;
                            return false;
                        }
                    }
                    else
                    {
                        Status = StatusOfProcess.Failed;
                        return false;
                    }
                }
            }

            if (ResourceItem.HasValue)
            {
                 IResourceItem iResourceItem = LookUp<IResourceItem, ResourceItemID>.FindByID(this.ResourceItem);
                 if (iResourceItem == null)
                 {
                     Status = StatusOfProcess.Failed;
                     return false;
                 }
            }


            return true;

        }



        private void ConsumeInputsAndGatherParts(Entity startingAgent, Dictionary<EntityType, List<Entity>> assignedInputData, out Dictionary<EntityType, List<Entity>> parts,
            out float totalBulkOfInput,
            out Dictionary<string, float> extractedSubstances,
            out Vector3? lastDestroyedInputLocation)
        // out float? extractedBulk)
        {

            //  extractedBulk = null;
            lastDestroyedInputLocation = null;
            extractedSubstances = null;
            parts = new Dictionary<EntityType, List<Entity>>();

            List<Entity> itemEntities;

            Input input;
            Entity itemToConsumeEntity;


            totalBulkOfInput = 0f;  //  job.TotalBulkOfInputs = 0f;

            float? totalBulkToConvert = MaxBulkToExtract;

            if (LimitBulkExtractionByNutrients)
            {
                totalBulkToConvert = GoalEat.CapAmountToConsume(assignedInputData.First().Value[0], startingAgent);
            }
           
            if (ProcessType.IsReplenishProcess == false // after replenish containers become substances, we can change this so it works like normal production... for now, replenish will consume at the end!
                && ProcessType.InputsByType != null)
            {
                foreach (var kvp in ProcessType.InputsByType)
                {
                    input = kvp.Value;
                                       
                    if (assignedInputData.TryGetValue(input.EntityType, out itemEntities))
                    {
                        // will so-called 'Bulk' items need to be implemented...? I don't think so.
                        
                        for (int i = 0; i < input.Amount.NoOfItems; i++)
                        {
                            itemToConsumeEntity = HandleSingleInputItem(itemEntities);

                          //  bool inputWasConsumedOrAddedAsPart = false;


                            if (ProcessType.IsSalvageProcess)
                            {
                                // for a salvage job, get the parts that the input (structure) is composed of.
                                // the parts that are not defined to go into the output will be destroyed in the end.
                                if (itemToConsumeEntity.Parts != null)
                                {
                                    foreach (var part in itemToConsumeEntity.Parts)
                                    {
                                        Common.AddToMultiList(parts, part.EntityType, part);
                                    }
                                }

                                LogInputDestroyedStatistics(itemToConsumeEntity);

                                // then destroy the item/structure itself (without destroying the parts!):
                                // Note: if this is a contained item (like an upgrade item) we lose its location here. This means its parts cannot be ejected properly later...
                                // so save its location now.
                                lastDestroyedInputLocation = itemToConsumeEntity.AccessPoint;
                                itemToConsumeEntity.Destroy(false);
                                
                            }
                            else if (input.BecomesPartOfProductType != null)
                            {
                                LogInputDestroyedStatistics(itemToConsumeEntity);

                                // save the item to become a part
                                Common.AddToMultiList(parts, input.BecomesPartOfProductType, itemToConsumeEntity);
                                itemToConsumeEntity.AssignedToJob = null;

                               // inputWasConsumedOrAddedAsPart = true;
                            }
                            else
                            {
                                // we can extract from many items at a time. All of the extracted bulk (or substances) are put in a dictionary for the outputs to use.

                                float bulkToExtract;


                                if (input.Amount.SubstanceTypes != null) //conversion.SubstanceTypes != null)
                                {
                                    SubstanceComponent substances;
                                    if (itemToConsumeEntity.Find(out substances))
                                    {
                                        foreach (var substanceType in input.Amount.SubstanceTypes)
                                        {
                                            // extract a part of the input:
                                            // extract from different substances (meat etc.)

                                            //   SubstanceType substanceType = input.Amount.Extract.ExtractSubstanceType;

                                            
                                            float bulkOfSubstance = substances.BulkAmounts[substanceType].Amount;
                                            bulkToExtract = GetBulkToExtract(totalBulkToConvert, bulkOfSubstance); // ??? won't work with more than one substance...

                                            substances.ChangeSubstanceBulk(substanceType, -bulkToExtract);

                                            AddToExtractedSubstances(ref extractedSubstances, substanceType, bulkToExtract);

                                            totalBulkOfInput += bulkToExtract;
                                        }
                                    }
                                    else return; //?? Fail?? should have been validated that the substance exists...
                                }
                                else
                                {
                                    // extract from the full bulk
                                    bulkToExtract = GetBulkToExtract(totalBulkToConvert, itemToConsumeEntity.Bulk);

                                    itemToConsumeEntity.Bulk -= bulkToExtract;

                                    totalBulkOfInput += bulkToExtract;
                                }

                                // is there any bulk left in the entity?
                                if (Common.IsLessThanOrEqual(itemToConsumeEntity.Bulk, 0f))
                                {
                                    // fire event for watchers here?
                                  //  FireInputDestroyedEvent(itemToConsumeEntity);
                                  //  LogInputDestroyedStatistics(itemToConsumeEntity); // gather stats on consumed items

                                    LogInputDestroyedStatistics(itemToConsumeEntity);

                                    lastDestroyedInputLocation = itemToConsumeEntity.AccessPoint;
                                    itemToConsumeEntity.Destroy();

                                    //inputWasConsumedOrAddedAsPart = true;
                                }
                                else
                                {
                                    // the input item still exists. release the lock on it, since we are done with it:
                                    // release Assigned to job lock when the process is finished, in the Destroyed event.
                                    // why is this not done in the Goal of the agent instead?                                   
                                    startingAgent.Intelligence.Allegiance.SharedKnowledge.ClearInUseBy(itemToConsumeEntity.EntityID, startingAgent.EntityID);   // for non-jobs like eating

                                }

                                /*
                                if (ProcessType.IsConsumeProcess)
                                {
                                    string eatsOrDrinks;
                                    if (itemToConsumeEntity.EntityType.ItemType.FoodType.IsDrunk)
                                    {
                                        eatsOrDrinks = "drinks";
                                    }
                                    else
                                    {
                                        eatsOrDrinks = "eats";
                                    }

                                    The.Client.AddLogEvent(startingAgent.Intelligence.Allegiance, The.Client.Log.GeneralEvent, startingAgent, eatsOrDrinks + " " + itemToConsumeEntity.EntityType.Name.ToLower(Config.Culture));
                                }*/

                            }

                          /*  if (inputWasConsumedOrAddedAsPart)
                            {
                                LogInputDestroyedStatistics(itemToConsumeEntity); //input.EntityType);
                            }*/

                        }
                    }

                }
            }

            // NEW: consume resources:
            if (ProcessType.IsGathering) 
            {
                // get the crop item:
                IResourceItem iResourceItem = LookUp<IResourceItem, ResourceItemID>.FindByID(this.ResourceItem);
                if (iResourceItem != null)
                {
                    if (iResourceItem.Container.GatherResource(iResourceItem))
                    {

                    }
                }
                else
                {
                    // validate first..?
                   // return false;
                }
            }

        }

      /*  private void LogInputStatistics(EntityType inputType)
        {


        }*/

       

       
        private void LogInputDestroyedStatistics(Entity input)
        {             
            IOwner owner;
            LookUpOwners.ResolveEntityOwner(input, out owner);
            
            // if the owner can see the item, either it is used as input, consumed or eaten by critters...
            IKnownEntityData entityData;
            if (owner != null)
            {
                if (owner.Allegiance.SharedKnowledge.GetKnownData(input.ID, out entityData) == EntityResult.SeenDirectly)
                {
                    if ((Workers != null && Workers.Count > 0)
                        && (ProcessType.IsInnateExtractionProcess || ProcessType.IsConsumeProcess))
                    {
                        Entity worker = Entity.FindByID(Workers[0].Item1);
                        if (worker != null)
                        {
                            if (owner.Allegiance != worker.Intelligence.Allegiance)
                            {
                                owner.Allegiance.Statistics.AddProductionEvent(input.EntityType, Allegiances.Statistics.ProductionStatistics.StatTypes.EatenByCreatures, 1);

                                return;
                            }
                            else
                            {
                                // only count food consumed by independent members
                                if (ProductionStatistics.CountMemberConsumption(worker))
                                {
                                    owner.Allegiance.Statistics.AddProductionEvent(input.EntityType, Allegiances.Statistics.ProductionStatistics.StatTypes.ConsumedFood, 1);

                                    return;
                                }
                            }
                        }                                               
                    }                    

                    // regular production input
                    owner.Allegiance.Statistics.AddProductionEvent(input.EntityType, Allegiances.Statistics.ProductionStatistics.StatTypes.UsedAsInput, 1);                    

                }
            }
        }

        private void AddToExtractedSubstances(ref Dictionary<string, float> extractedSubstances, SubstanceType substanceType, float extractedBulk)
        {
            if (extractedSubstances == null)
                extractedSubstances = new Dictionary<string, float>();

            float previousBulk;
            float newBulk;
            if (extractedSubstances.TryGetValue(substanceType.KeyName, out previousBulk))
            {
                newBulk = previousBulk + extractedBulk;
            }
            else
            {
                newBulk = extractedBulk;
            }

            extractedSubstances[substanceType.KeyName] = newBulk;

        }

        private float GetBulkToExtract(float? totalBulkToConvert, float availableBulk)
        {
            if (totalBulkToConvert.HasValue)
            {
                return Math.Min(availableBulk, totalBulkToConvert.Value);
            }
            else
            {
                return availableBulk;
            }

        }

        /// <summary>
        /// calling this removes elements one at a time from the front of the list.     
        /// </summary>
        /// <param name="itemEntities"></param>
        /// <returns></returns>
        private Entity HandleSingleInputItem(List<Entity> itemEntities)
        {
            Entity itemToConsumeEntity;
            itemToConsumeEntity = itemEntities[0];
            itemEntities.RemoveAt(0);
                       

            if (ImmovableInput == itemToConsumeEntity.EntityID)
            {
                ImmovableInput = null;

                GroundLocation = itemToConsumeEntity.Location; // .AccessPoint; // not the access point!
            }

            if (itemToConsumeEntity.Contains != null)
            {
                // eject contents:
                itemToConsumeEntity.Contains.IterateContained(e => itemToConsumeEntity.Contains.Uncontain(e));
            }
            return itemToConsumeEntity;
        }

        /* private static void SaveItemToBecomeAPart(Dictionary<EntityType, List<Entity>> parts, Entity itemToConsumeEntity, EntityType entityType) // Input input)
         {
             List<Entity> listOfParts = null;

             if (!parts.TryGetValue(entityType, out listOfParts))
             {
                 listOfParts = new List<Entity>();
                 parts.Add(entityType, listOfParts);
             }

             listOfParts.Add(itemToConsumeEntity);
         }*/



        private bool GetGroundLocationForNewOutput(Entity agentEntity, Entity outputEntity, ProcessType process, out Vector3? location, Vector2? offset = null, Entity creatorOfItem = null)
        {
            Vector3? productionLocation;
         //   IKnownEntityData siteData = null, toolData = null;

            location = null;

            if (!GetCurrentLocation(out productionLocation, null, true))
            {
                return false;
            }
           

            if (productionLocation == null)
            {
                productionLocation = agentEntity.Location;
            }

            // OLD:
         /*   if (job != null)
            {
                if (!job.GetCurrentJobLocation(out productionLocation, ref siteData, ref toolData)
                    || !productionLocation.HasValue)
                {
                    // destroys the job too.

                    Status = StatusOfProcess.Failed;
                    return false;
                }
            }
            else
            {
                productionLocation = agentEntity.Location;
            }*/

            if (outputEntity != null || process.IsSalvageProcess == false)
            {
                Entity.AddRandomOffset addRandomOffset;

                if (offset.HasValue) // did the designer specify an offset for the item location? (salvage process...)
                {
                    addRandomOffset = Entity.AddRandomOffset.No;
                    productionLocation += offset.Value.ToVector3();
               
                }
                else
                {
                    addRandomOffset = Entity.AddRandomOffset.Yes;
             
                }

                location = MapManager.FindFreeLocation(productionLocation.Value, 
                    outputEntity.EntityType.ItemType != null, addRandomOffset, creatorOfItem);


            }

            return true;
        }


       
        private bool PlaceNewItem(Entity agentEntity, Entity outputEntity, ProcessType process, Entity container = null, StorageCompartment? placeProductsInCompartment = null, Vector2? offset = null, 
            Entity creatorOfItem = null, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null) // bool isUpgrade = false)
        {
            Entity containerToUse = null;
            Vector3? productionLocation = null;

            if (container != null && container.Contains != null)
            {
                // place the new item in the container (stomach/building/tool):
                if (outputEntity != null || process.IsSalvageProcess == false)
                {
                   // container.Contains.AddToContain(outputEntity, compartment: placeProductsInCompartment, isProductionOutput: true);
                    containerToUse = container;
                }
            }
            else
            {       
                if (!GetGroundLocationForNewOutput(agentEntity, outputEntity, process, out productionLocation, offset, creatorOfItem))
                {
                    return false;
                }     
            }

            IOwner owner = null;
            Expedition expedition = null;
            if (ownerOfOutput.HasValue)
            {
                owner = LookUpOwners.FindByID(ownerOfOutput);
                if (owner == null)
                {
                    // fail
                    Status = StatusOfProcess.Failed;
                    return false;
                }
                else
                {
                    expedition = owner as Expedition;
                }
            }

            if (isProductionOutput && containerToUse != null && upgradeCategory == null) // !isUpgrade)
            {
                IHoldsProductionOutput outputContainer = containerToUse.Contains as IHoldsProductionOutput;
                // test stomach also..?
                if (outputContainer != null && !outputContainer.HasCapacityForOutput(outputEntity))
                {
                    // don't fail production. just place the output outside the container...
                    Entity outerContainer = null;
                    if (!containerToUse.GetContainerOrLocation(ref outerContainer, ref productionLocation))//ref containerToUse, ref productionLocation)) 
                    {
                        // fail
                        Status = StatusOfProcess.Failed;
                        return false;
                    }

                    if (outerContainer != null)
                    {
                        containerToUse = outerContainer;
                        productionLocation = null;
                    }
                    else
                    {
                        // place on ground
                        containerToUse = null;
                    }
                }
            }

            if (!outputEntity.PlaceEntityOnPlaySite(productionLocation, containerToUse, Entity.StructureState.Unfinished, 
                new Entity.SetOwnerInfo(owner), expedition, placeProductsInCompartment: placeProductsInCompartment, isProductionOutput: isProductionOutput, upgradeCategory: upgradeCategory)) // isUpgrade: isUpgrade))
            {
                // place on ground:
                outputEntity.PlaceEntityOnPlaySite(productionLocation, null, Entity.StructureState.Unfinished, 
                    new Entity.SetOwnerInfo(owner), expedition, placeProductsInCompartment: placeProductsInCompartment, isProductionOutput: isProductionOutput);
            }

            if (outputEntity != null || process.IsSalvageProcess == false)
            {
                AddOutputEntity(outputEntity);
            }

            return true;

           
        }

              

        Site Site
        {
            get
            {
                return The.Sim.PlaySite; // for now, processes are a playsite thing...
            }
        }


        private bool CreateRegularProduct(Output output, ref Entity outputEntity, List<Entity> listOfParts, List<Entity> partsForOneItem, Allegiance allegiance)
        {
            
            Entity singlePart;
            // the parts become parts of the output items:
            foreach (var item in output.FinalEntityTypeToCreate.Parts) // get all the parts we need
            {
                for (int j = 0; j < item.Value; j++)
                {
                    // find a part and keep it
                    singlePart = listOfParts.Find(e => e.EntityType == item.Key);

                    if (singlePart != null)
                    {
                        partsForOneItem.Add(singlePart);
                        listOfParts.Remove(singlePart);
                    }
                }
            }

           
            if (outputEntity != null)
            {
                // a structure will have already been created. just assign its parts:
                outputEntity.NonLivingEntity.SetPartsOrCreateNew(partsForOneItem);

                // these calls will recursively init any new parts, but not affect entities which are already init'ed...
                outputEntity.Initialize(Site, allegiance);
                outputEntity.InitializeModelAndOnScreenFunctionality();
            }
            else
            {
                // create a new item using the parts we found:
                outputEntity = Entity.CreateAndInitEntity(output.FinalEntityTypeToCreate, Site, partsForOneItem, allegiance: allegiance);
            }

            return true;
        }

        private static void CreateSalvageProduct(Output output, /*ref*/ Entity outputEntity, List<Entity> listOfParts)
        {

            // the parts of the salvaged entity become the output items themselves:
            // find a part and keep it
           // outputEntity = listOfParts.Find(e => e.EntityType == output.FinalEntityType); // can be null

            if (outputEntity == null)
            {
                //there were no items left of this type,  the parts must have degraded and been destroyed/turned into junk. do nothing...
            }
            else
            {
                listOfParts.Remove(outputEntity);
            }

        }

        public static bool IsMaterialOnSite(Entity startingAgent, IKnownEntityData materialEntity, WorldLocation originalItemLocation)
        {
            if (materialEntity == null)
                return false;

            if (startingAgent != null && startingAgent.ContainsEntity(materialEntity.EntityID))
                return true;    // ok if carried

            WorldLocation currentItemLocation = new WorldLocation(materialEntity.PlaySiteLocation);
            if (Common.DistanceOctile(originalItemLocation, currentItemLocation) > GameData.Instance.Constants.InteractionDistanceForAgents)                           
               // originalItemLocation != currentItemLocation)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// if the designer has specified that the salvage output is less than the parts of the salvage item, then the parts list will still contain items at this point.
        /// destroy them all - except those that were not in the original blueprint (like junk parts)
        /// </summary>
        /// <param name="startingAgent"></param>
        /// <param name="parts"></param>
        /// <param name="salvagedEntity"></param>
        private void DestroyUnusedInputParts(Entity startingAgent, Dictionary<EntityType, List<Entity>> parts, Entity salvagedEntity, Vector3? salvagedEntityLocation)
        {
            Vector3? placeOnGroundLocation = null;
            if (salvagedEntity.Location == null && salvagedEntity.AccessPoint == null) // the salvaged entity is destroyed at this point, sometimes the location will be gone (like if it was contained(=upgrade item))
            {
                placeOnGroundLocation = salvagedEntityLocation;
            }

            Entity part;
            foreach (var entityList in parts)
            {
                
                if (salvagedEntity.EntityType.IsInOriginalBlueprint(entityList.Key)) //) NonLivingEntity.IsInOriginalBlueprint(entityList.Key, salvagedEntity))
                {
                    for (int i = entityList.Value.Count - 1; i >= 0; i--)
                    {
                        part = entityList.Value[i];

                        // We eject first to make sure the parts have a valid location before destroying them (they may contain other items, like ammo...):
                        Container.EjectEntity(part, salvagedEntity, placeOnGround: placeOnGroundLocation); 
                        part.Destroy();

                        entityList.Value.RemoveAt(i);
                    }
                }
                else
                {
                    // This should be reached if a part(s) has degraded into another one
                    // create the degraded products:
                    while (entityList.Value.Count != 0)
                    {
                        Entity degradedItem = entityList.Value[0];

                        PlaceNewItem(startingAgent, degradedItem, ProcessType); 
                        entityList.Value.RemoveAt(0);

                    }
                }
            }
        }

        public bool GetOutputEntityData(Predicate<IKnownEntityData> predicate, out IKnownEntityData matchingData, SharedKnowledge sharedKnowledge)
        {
            matchingData = null;

            if (OutputEntities != null && OutputEntities.Count > 0)
            {               
                IKnownEntityData outputEntityData;
                foreach (var output in OutputEntities)
                {
                    if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(output, out outputEntityData)))
                    {
                        return false;
                    }

                    if (predicate(outputEntityData))
                    {
                        matchingData = outputEntityData;
                        return true;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// returns false if the outputs list has destroyed entities
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="matchingEntity"></param>
        /// <returns></returns>
        private bool GetOutputEntity(Predicate<Entity> predicate, out Entity matchingEntity)
        {
            matchingEntity = null;

            if (OutputEntities != null && OutputEntities.Count > 0)
            {
                Entity outputEntity;
                foreach (var output in OutputEntities)
                {
                    outputEntity = Entity.FindByID(output);

                    if (outputEntity != null)
                    {
                        if (predicate(outputEntity))
                        {
                            matchingEntity = outputEntity;
                            return true;
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

        private bool CreateOutputsFromInputs(Entity startingAgent, ProcessType process, Entity placeProductsInContainer, Dictionary<EntityType, List<Entity>> parts, Output output,
           int noOfItemsToCreate, float? bulkOfEachOutputItem) 
        {

            Entity outputEntity = null;
            List<Entity> listOfParts;

            List<Entity> partsForOneItem = new List<Entity>();

            if (placeProductsInContainer == null)
            {
                placeProductsInContainer = output.GetToolContainerToPlaceOutputIn(StationaryTools);

                if (placeProductsInContainer == null && output.FinalEntityTypeToCreate.Upgrader != null)
                {
                    placeProductsInContainer = Entity.FindByID(ActingOnEntity.Value.Entity);   
                    
                }
            }

            // see if an output of this type exists... this is the case for ordered structures.
            if (!GetOutputEntity(e => e.EntityType == output.FinalEntityTypeToCreate, out outputEntity)) 
            {
                return false;
            }

            // for intelligent outputs:
            Allegiance allegiance = null;
            IOwner owner = LookUpOwners.FindByID(ownerOfOutput);
            if (owner != null)
            {
                allegiance = owner.Allegiance;
            }

            // loop over the outputs we want to create for this single output type
            for (int i = 0; i < noOfItemsToCreate; i++)
            {
               // outputEntity = null;
                partsForOneItem.Clear();
               

                if (parts.TryGetValue(output.FinalEntityTypeToCreate, out listOfParts)
                    && listOfParts.Count > 0)
                {
                    if (process.IsSalvageProcess)
                    {
                        // the parts of the salvaged entity become the output items themselves:
                        // find a part and keep it
                        outputEntity = listOfParts.Find(e => e.EntityType == output.FinalEntityTypeToCreate); // can be null
                        CreateSalvageProduct(output, outputEntity, listOfParts);
                    }
                    else
                    {
                        if (!CreateRegularProduct(output, ref outputEntity, listOfParts, partsForOneItem, allegiance))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    // no parts of that type are available...

                    if (process.IsSalvageProcess)
                    {
                        if (output.IsWasteProduct)
                        {
                            // create the product without using the salvaged parts (called waste):
                            // just create a new item with new parts:
                            outputEntity = Entity.CreateAndInitEntity(output.FinalEntityTypeToCreate, Site, null, allegiance: allegiance);
                        }
                        else
                        {
                            // the part is gone, which probably means it degraded and was destroyed.
                            // we create nothing.
                            return true;
                            //throw new Exception("Tried to salvage a non-waste item from an entity which did not contain items of that type!");
                        }
                    }
                    else
                    {
                        if (outputEntity == null)
                        {
                            // just create a new item with new parts:
                            outputEntity = Entity.CreateAndInitEntity(output.FinalEntityTypeToCreate, Site, null, allegiance: allegiance);
                        }
                    }
                }

                if (outputEntity != null || process.IsSalvageProcess == false)
                {
                    outputEntity.NonLivingEntity.Progress = 0f; // not yet a 'physical object' ! - also for salvaged items!
                }

                if (!process.IsSalvageProcess)
                {
                    /*  if (output.FinalEntityType.ItemType != null
                          && output.FinalEntityType.ItemType.InstanceHasUniqueBulk)
                      {                    
                          outputEntity.Bulk = totalBulkOfInputs * output.Amount.FractionOfInputBulk.Value / noOfItemsToCreate;
                      }*/

                    // assign unique bulk to this item (if Item.MaximumBulk has been defined, it will already have been set):
                    if (bulkOfEachOutputItem.HasValue)
                    {
                        outputEntity.Bulk = bulkOfEachOutputItem.Value;
                    }

                }


                if (!outputEntity.HasLocation) // if it has not been placed already (like the structures placed via the Build menu)
                {
                   // bool isUpgrade = outputEntity.EntityType.Upgrader != null;

                    if (!PlaceNewItem(startingAgent, outputEntity, process, placeProductsInContainer, PlaceProductsInCompartment, output.RelativePlacement, startingAgent, true, upgradeCategory: UpgradeCategory)) // isUpgrade: isUpgrade))
                    {
                        return false;
                    }
                }

                outputEntity = null;
            }

            return true;
        }


        /// <summary>
        /// if called again for an already assigned material, this will now update its stored location - used in the validation check in Produce
        /// </summary>
        /// <param name="item"></param>
        public void AssignInput(IKnownEntityData item, out bool wasAssigned)
        {
            wasAssigned = false;
            Input input;

            // NEW: can't assign when started:
            if (IsStarted)
            {
                return;
            }


            // clean up:
            foreach (var assignedItemsOfType in AssignedInputs)
            {
                for (int i = assignedItemsOfType.Value.Count - 1; i >= 0; i--)
                {
                    var assignedInputOfType = assignedItemsOfType.Value[i];

                    Entity assignedEntity = Entity.FindByID(assignedInputOfType.Item1);
                    if (assignedEntity == null)
                    {
                        assignedItemsOfType.Value.RemoveAt(i);
                    }
                }
            }

            if (ProcessType.InputsByType.TryGetValue(item.EntityType, out input))
            {

                if (input.InputIsImmovable())
                {
                    ImmovableInput = item.EntityID;
                }

                List<Tuple<EntityID, WorldLocation>> assignedOfType;
                if (AssignedInputs.TryGetValue(item.EntityType, out assignedOfType))
                {
                    Tuple<EntityID, WorldLocation> existingEntry = assignedOfType.Find(t => t.Item1 == item.EntityID);
                    if (existingEntry != null)
                    {
                        // replace it:
                        assignedOfType.Remove(existingEntry);
                        assignedOfType.Add(new Tuple<EntityID, WorldLocation>(item.EntityID, new WorldLocation(item.PlaySiteLocation)));

                        wasAssigned = true;
                        return;
                    }
                    else
                    {
                        // add:
                        InputAmount neededAmount = input.Amount;
                        if (neededAmount.NoOfItems > 0) // || neededAmount.Bulk > 0f)
                        {
                            if (neededAmount.NoOfItems.HasValue)
                            {
                                if (assignedOfType.Count < neededAmount.NoOfItems.Value)
                                {
                                    assignedOfType.Add(new Tuple<EntityID, WorldLocation>(item.EntityID, new WorldLocation(item.PlaySiteLocation)));
                                   
                                    wasAssigned = true;                                   
                                }
                            }
                            else
                            {
                                throw new Exception("Not implemented!!");
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// THIS code has been temporarily disabled, but should be enabled again to test for trees in the area...
        /// </summary>
        /// <param name="newProgress"></param>
        /// <returns></returns>
        private bool CheckStructureAreaCleared(float newProgress)
        {
            return true;

            /*
            bool siteIsClearedForNextStage = true;


            Structure structureComponent;
            foreach (var output in OutputEntities)
            {
                Entity outputEntity = Entity.FindByID(output);
                if (outputEntity != null)
                {
                    if (outputEntity.Find(out structureComponent)) // never more than one structure as output.
                    {
                        if (outputEntity.IsStarted() == false && newProgress > 0f)
                        {   // we are under way now.
                            if (!structureComponent.TestAreaIsClearAndAddClearingJobs())
                            {
                                siteIsClearedForNextStage = false;
                                break;
                            }
                        }

                        // if we are about to finish, test that the site is cleared for next stage:
                        if (newProgress >= 1f)
                        {
                            if (!structureComponent.TestAreaIsClearAndAddClearingJobs())
                            {
                                siteIsClearedForNextStage = false;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // output missing!
                    Status = StatusOfProcess.Failed;
                    return false;
                }
            }

            if (!siteIsClearedForNextStage)
            {
                // no work could be done, site is not cleared for next stage:
                Status = StatusOfProcess.Failed;
                return false;
            }


            return true;*/
        }




        /// <summary>
        /// some processes can be aborted from outside, for instance if an agent wants to interrupt it
        /// perhaps the class will also call this method if during Produce it discovers a problem..
        /// </summary>
        public void Abort()
        {


        }

        /// <summary>
        /// true for special actions like build turnip hut
        /// </summary>
        /// <returns></returns>
        public static bool HasFixedLocation(Vector3? groundLocation)
        {
            return groundLocation.HasValue;
        }

        public bool HasFixedLocation()
        {
            return HasFixedLocation(GroundLocation);
        }

        /// <summary>
        /// destroy process when:
        /// 1. completed
        /// 2. output destroyed
        /// 3. last worker has left? 
        /// </summary>
        public void Destroy()
        {
            if (id == (SimProcessID)32)
            {
                id = (SimProcessID)32;
            }


            if (id == SimProcessID.Invalid)
                return;
            
           
            The.Sim.PlaySite.PlaySite.RemoveProcess(this);

            DestroyUnstartedOutputs(); // NEW

            FireProcessDestroyedEvent();    

            AssignProcessToInvolvedEntitiesOrTile(true);

            
            RemoveIDEntry(); // set ID invalid last.
        }

                

        #region ILookup

        private SimProcessID id = SimProcessID.Invalid;
        static SimProcessID IDCounter = SimProcessID.First;

        public SimProcessID ID
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

        public SimProcessID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= SimProcessID.Max)
            {
                throw new Exception("Astounding, SimProcessID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public SimProcessID SnapshotID(Snapshotter sn, SimProcessID id)
        {
            return (SimProcessID)sn.DoEnum(id);
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
            if (ID != SimProcessID.Invalid)
                LookUp<SimProcess, SimProcessID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = SimProcessID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<SimProcess, SimProcessID>.Remove(this);
        }

        void ILookUp<SimProcess, SimProcessID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = SimProcessID.First;
        }

        void ILookUp<SimProcess, SimProcessID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<SimProcess, SimProcessID>.Create();
        }

        #endregion
        #region ISnapshot


        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion((Snapshotter.Version)2);  // Dec, 2016.  increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.ID = SnapshotID(sn, ID);
            IDCounter = sn.DoEnum(IDCounter);


            this.AssignedInputs = sn.DoMultiMap(AssignedInputs); // Tuple!!!
            this.immovableTool = sn.DoEntityIDNullable(immovableTool);
            this.immovableInput = sn.DoEntityIDNullable(immovableInput);
            this.groundLocation = sn.DoVector3Nullable(groundLocation);
            this.ContainerToPlaceOutputsIn = sn.DoEntityIDNullable(ContainerToPlaceOutputsIn);
            this.MaxBulkToExtract = sn.DoFloatNullable(MaxBulkToExtract);
            this.OutputEntities = sn.DoList(OutputEntities);
            this.PlaceProductsInCompartment = sn.DoEnumNullable(PlaceProductsInCompartment);
            this.Progress = sn.DoFloat(Progress);
            this.ProgressSpeed = sn.DoFloat(ProgressSpeed);
            this.IsStarted = sn.DoBool(IsStarted);
            this.Status = sn.DoEnum(Status);
            this.workers = sn.DoList(workers);       
            this.ProcessType = sn.DoGameData(ProcessType);
            this.StationaryTools = sn.DoList(StationaryTools);
            this.ResourceItem = sn.DoEnumNullable(ResourceItem);
            this.ActingOnEntity = sn.DoEntityAndRootNullable(ActingOnEntity);
            snapshotProcessToolCombo = sn.SnapshotID<ToolTypeCombination, ToolTypeCombinationID>(StationaryToolsTypeCombination);
            this.UpgradeCategory = sn.DoGameData(UpgradeCategory);
            this.SleepyUpdater = sn.DoEnum(SleepyUpdater);
            this.ownerOfOutput = sn.DoEnumNullable(ownerOfOutput);
            this.updateInterval = sn.DoDoubleNullable(updateInterval);
            this.timePointInSeconds = sn.DoDoubleNullable(timePointInSeconds);
            this.phase = sn.DoEnum(phase);
                       
            this.productivity = (Productivity)sn.DoISnapshot(productivity);

            this.ProcessCompletedEvent = (IDActionEvent<SimProcess>)sn.DoISnapshot(ProcessCompletedEvent);
            this.ProcessDestroyedEvent = (IDActionEvent<SimProcess>)sn.DoISnapshot(ProcessDestroyedEvent);
            this.ProcessStartedEvent = (IDActionEvent<SimProcess>)sn.DoISnapshot(ProcessStartedEvent);
            this.ProcessProducingEvent = (IDActionEvent<SimProcess>)sn.DoISnapshot(ProcessProducingEvent);

            this.assignedSubstances = sn.DoDictionary(assignedSubstances);
            this.requestedSubstances = sn.DoDictionary(requestedSubstances);
            this.ignoreProgressCap = sn.DoBool(ignoreProgressCap);
            this.suppressSpawningEvents = sn.DoBool(suppressSpawningEvents);
            this.MapPosition = sn.DoPointNullable(MapPosition);

            if ((uint)version >= 2)
            {
                this.LimitBulkExtractionByNutrients = sn.DoBool(LimitBulkExtractionByNutrients);  // #EATFIX
            }
            else
            {
                this.LimitBulkExtractionByNutrients = false;  // #EATFIX
            }
         //   this.LimitBulkExtractionByNutrients = sn.DoBool(LimitBulkExtractionByNutrients);  // #EATFIX

            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (snapshotProcessToolCombo.HasValue)
            {
                StationaryToolsTypeCombination = ToolTypeCombination.FindByID(snapshotProcessToolCombo.Value);
            }

            if (productivity != null)
            {
                productivity.LoadPostProcess(sn);
            }

            RecomputeMapPosition();

            CreateRegulators();
        }

        #endregion

    }
}
