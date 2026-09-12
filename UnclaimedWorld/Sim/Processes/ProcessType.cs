using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UWGame.SimSide.Items;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.ClientSide.Renderables;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Policies;
namespace UWGame.SimSide.Processes
{

    public enum WorkerNeededOptions { StartRemotely, WorkerOnlyNeededToStart, WorkerNeeded }


    /// <summary>
    /// describes a type of production process of input and output item types
    /// </summary>
    public class ProcessType : IGameData, IXmlSerializable
    {
        public string KeyName { get; set; }
        
        /// <summary>
        /// use an "-ing" form
        /// </summary>
        public string Name { get; set; }
        public bool DeleteRecord
        {
            get;
            set;
        }

        public string SummaryDescription;

        public string Description;

        public string SpecialActionCaption;

        public string JobTypeKey;

        [XmlIgnore]
        public ProcessJobType JobType;


        /// <summary>
        /// set this true for processes that are only used for feedback...
        /// </summary>
        public bool IsPseudoProcess;

        public int SortOrder;

        /// <summary>
        /// not always visible...
        /// </summary>
        public string Tooltip;

        /// <summary>
        /// looks for this in Toolsloader so must be defined there. NOT in itemloader
        /// </summary>
        public string ProcessToolSetKey;

        /// <summary>
        /// also in EntityType, but for farm plot, the output is placed in an event, so repeat it here
        /// </summary>
        public TierOrArea TierOrArea;

        [XmlIgnore]
        public TierOrAreaType TierOrAreaType;

        public string UserCannotCancelReason;
        public bool? UserCanCancel;

        /// <summary>
        /// if true, this process will not be considered when producing items.
        /// 
        /// </summary>   
        public bool IsSalvageProcess;


        [XmlIgnore]
        public bool IsSalvageWithoutWaste;

        /// <summary>
        /// true for a bio consume process
        /// </summary>
        public bool IsConsumeProcess;

        /// <summary>
        /// TODO: it would be better to define the resource as input!
        /// </summary>
        public bool IsGathering;

        public RepairAction? RepairAction;
       // public bool IsRepairProcess;

        /// <summary>
        /// when true, the output is placed in the Upgrade part of the container, and the upgrade effects are applied.. 
        /// is set during init based on output properties.
        /// </summary>
        [XmlIgnore]
        public bool IsUpgrade;

        public bool MoveOutputToWorkerWhenCompleted;

        /// <summary>
        /// true (default) if the work duration is affected by the worker's energy level. Only makes sense for processes that require a worker.
        /// </summary>
        public bool UseWorkerEnergyAsProductionFactor = true;

        /// <summary>
        /// how many nutrients are consumed as a factor of the expenditure when sleeping.
        /// default is in the Constants class
        /// </summary>
        public float? PhysicalWorkFactor;


        public bool RequiresBoldStance = false;


        /// <summary>
        /// Set the interval to 0 to update every frame.
        /// 
        /// Worker processes should maybe update every frame. Other processes, degrade, plant growth, once every x seconds.
        /// 
        /// Higher update frequency means the process progresses more smoothly, at a higher CPU cost.
        /// 
        /// Processes that need substances from a pool should not update more often than the assigner for the pool. They would not be able to produce more often than the assigner allows anyway.
        /// Processes that only require work, such as eating, can update a lot more often.
        /// 
        /// </summary>
        public float? UpdateInterval;

        /// <summary>
        /// if true, will allow doing the process job on a broken entity. Used in crop growing etc.
        /// </summary>
        public bool IsMetaAction = false;

        public WorkerNeededOptions WorkNeeded = WorkerNeededOptions.WorkerNeeded;

        /// <summary>
        /// true by default, since before refactoring, all process types required work
        /// </summary>
     /*   public bool RequiresWork = true;

        /// <summary>
        /// if false, the process can start remotely, useful for machinery or special action orders
        /// </summary>
        public bool RequiresWorkToStart = true;
        */

        public int MaxWorkers = 1;


        /// <summary>
        /// only for data entry. in code, use the cached reference to the skill type instead
        /// </summary>
        public string RequiredSkill;


        public string[] RequiredKnowledge;
        public string[] ProducesKnowledge;


      //  public ProcessLabelTypes? LabelType;


        public List<ProgressFactorProperty> ProgressFactorProperties;

        /// <summary>
        /// Move to Input???
        /// 
        /// these substances will be continually consumed from non-entity inputs. TODO: add continous consumption from input item substances as well.
        /// This is the total required for a finite process
        /// </summary>
        public SerializableDictionary<string, float> TotalContinuousSubstanceInput;

        [XmlIgnore]
        public SerializableDictionary<SubstanceType, float> TotalContinuousSubstanceInputTypes;

        /// <summary>
        /// gets set automatically if this process is available as a special action on an entity type, like sentry gun
        /// 
        /// is null for the Original (shared) process!
        /// </summary>
        [XmlIgnore]
        public EntityType ActingOnType;
      
       

        /// <summary>
        /// if true, must not be part of production graph.
        /// Ignore elsewhere too?
        /// </summary>
        [XmlIgnore]
        public bool IsOriginalSpecialAction;


        /// <summary>
        /// An optional cap for the max growth rate in trees, natural processes etc.
        /// </summary>
        public float? ProgressPerSecondCap;

        /// <summary>
        /// it seems like we need a way to link inputs with outputs, for example firewood becomes replenishment in a newly create campfire...
        /// is it enough to link them with EntityType???
        /// should there be an InOut class that contains both Input and Output?
        /// 
        /// then again, there are cases where there is no link... for example heat that gets distributed to all outputs...
        /// 
        /// </summary>
        public Input[] Inputs;

        
        public Output[] Outputs;

        /// <summary>
        /// why was this linked to Output?      
        /// </summary>
        [XmlIgnore]
        public ResourceType ResourceTypeInput;


        /// <summary>
        /// should the needs satisifed be based on time spent, or progress made...?
        /// </summary>
        public NeedSatisfaction[] SatisfiesWorkerNeeds;

       
        /// <summary>
        /// this could be used in auto-mining perhaps..
        /// </summary>
        public bool HasNoEndpoint = false;
       
        public WorkOrTime WorkOrTimeNeeded;

      
        /// <summary>
        /// StancesType key  => list of StanceTypes
        /// The possible stances. null means only 'default' = Standing is possible
        /// </summary>
        public SerializableDictionary<string, ChanceToTakeStance[]> Stances;
       // public LeggedLocomotor.Stance[] Stances;

        [XmlIgnore]
        public Dictionary<StancesType, List<ChanceToTakeStance>> StanceTypes; // List<StanceType>> StanceTypes;

       
        // divide available energy proportionally among all jobs:

     

        /// <summary>
        /// the term to use in the info panel for resource types
        /// default is: "GATHERED AT"
        /// </summary>
     //   public string GatheredAtTerm;

        public AnimAction AgentActionState = AnimAction.Mending;

        /// <summary>
        /// Don't set stance flags here... use Stances instead. That way, Sim and Client will be synchronized.
        /// Also don't set tool flags.
        /// </summary>
        public AnimModifier[] AgentAnimationStates;

        [XmlIgnore]
        public List<AnimModifier> AgentAnimationStatesList;

           
        /// <summary>
        /// triggers that are active during the process
        /// </summary>
        public TriggerType[] Triggers;

        /// <summary>
        /// Set this to true for a degrade process that we would like to see in the production chain, but should be accessible from Inventory panel
        /// </summary>
       // public bool IsProductionDegradeProcess;
       

        #region non-serialized properties

        /// <summary>
        /// These are now set in a separate loading file! To better enable mods/scenarios overriding them with their own actions...
        /// these actions will be fired when the process is complete. the worker will be the triggering entity.
        /// Use this direct method to avoid having to set up a trigger and a listener for the worker.
        /// 
        /// ("Bob sez: That's taken care of.")
        /// 
        /// if the triggers are set on the output entity instead, the worker will not be available.
        /// </summary>
        [XmlIgnore]
        public Dictionary<AgentActionHooks, List<ActionSets>> EventActions = new Dictionary<AgentActionHooks, List<ActionSets>>();

        /// <summary>
        /// true for a creature extraction process - not designer set...
        /// </summary>
        [XmlIgnore]
        public bool IsInnateExtractionProcess;

               

        #region Replenish processes

        /// <summary>
        /// if true, the inputs will be treated differently than for other processes. the flag gets set automatically. One replenish process gets created per input entity type.
        /// 
        /// after replenish containers become substances, we can change this so it works like normal production...
        /// </summary>   
        [XmlIgnore]
        public bool IsReplenishProcess;

        /// <summary>
        /// this is used as the target compartment for the replenish action. Should be refactored to be sort of like output Substance in output/actedOnEntity.       
        /// this is needed to show in which container the replenish process inputs should end up
        /// </summary>      
        public GoalReplenish.ReplenishAction? ReplenishAction;


        #endregion

        /// <summary>
        /// light fire process will set a property Prepared when done. This is assigned automatically...
        /// </summary>
        [XmlIgnore]
        public bool? SetPreparedProperty;

        /// <summary>
        /// added this so different tool upgrades can have different effects on the housing structure
        /// </summary>
        public StateModifier? PreparedToolModifier;

        /// <summary>
        /// enables a process to change this setting on a sentry gun...
        /// </summary>
        public bool? AttacksVerminValueToSet;

        #region Special actions

      

        /// <summary>
        /// after activating, all these allegiance-specific lock processes are enabled.
        ///       
        /// </summary>
        public string[] EnablesSpecialActionLockProcesses;

        /// <summary>
        /// Specific to each allegiance!
        /// after activating, all these processes are disabled.
        /// </summary>
        public string[] DisablesSpecialActionLockProcesses;


        [XmlIgnore]   
        public List<ProcessType> DisablesSpecialActionLockProcessTypes;

        [XmlIgnore]
        public List<ProcessType> EnablesSpecialActionLockProcessTypes;


        public string[] EnablesSharedActionProcesses;

        /// <summary>
        /// Shared between all allegiances!
        /// after completing the process, all these processes are disabled.
        /// 
        /// NOTE: If the process has a structure output it is possible to use an anchor instead, which is easier.
        /// </summary>
        public string[] DisablesSharedActionProcesses;


        [XmlIgnore]
        public List<ProcessType> EnablesSharedActionProcessTypes;

        [XmlIgnore]
        public List<ProcessType> DisablesSharedActionProcessTypes;


        [XmlIgnore]
        public bool? SpecialActionEnabledAtStart
        {
            get;
            private set;
        }

        public bool ShowDisabledSpecialAction = true;


        #endregion

        /// <summary>
        /// The skill that production needs
        /// </summary>
        [XmlIgnore]
        public Entities.SkillType RequiredSkillType; // = GameData.Instance.AllSkillTypes["menial"]; // UWGame.SimSide.Instance.Menial;

        [XmlIgnore]
        public ProcessToolSet ProcessToolSet;


        [XmlIgnore]
        public Dictionary<EntityType, Input> InputsByType;

        /// <summary>
        /// returns true if no input is an item! True for harvesting, hunting etc.
        /// </summary>
        /// <returns></returns>
      /*  [XmlIgnore]
        public bool IsPrimaryProcess;
        */

        /// <summary>
        /// returns true if an output item is a carcass
        /// </summary>
        /// <returns></returns>
        [XmlIgnore]
        public bool IsKilling;

        [XmlIgnore]
        public float FinalUpdateInterval
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// set by copy ctor only
        /// </summary>
        [XmlIgnore]
        private ProcessType originalProcess;

        public ProcessType OriginalProcess
        {
            get
            {
                return originalProcess ?? this;
            }
        }

        [XmlIgnore]
        public bool UseOriginalProcessEvents = true;

        public ProcessType()
        {

        }

        /// <summary>
        /// copy ctor - try and keep this up to date...
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public ProcessType(ProcessType original, string newKeyname, bool? specialActionEnabledAtStart)
        {
            KeyName = newKeyname;
            Name = original.Name; 
            SummaryDescription = original.SummaryDescription;
            Tooltip = original.Tooltip;
            Description = original.Description;
            SortOrder = original.SortOrder;

            SpecialActionEnabledAtStart = specialActionEnabledAtStart;

            SpecialActionCaption = original.SpecialActionCaption;

            UserCanCancel = original.UserCanCancel;

            DisablesSpecialActionLockProcesses = original.DisablesSpecialActionLockProcesses;
            EnablesSpecialActionLockProcesses = original.EnablesSpecialActionLockProcesses;
            DisablesSharedActionProcesses = original.DisablesSharedActionProcesses;
            EnablesSharedActionProcesses = original.EnablesSharedActionProcesses;

            ShowDisabledSpecialAction = original.ShowDisabledSpecialAction;
            originalProcess = original; // needed for event look ups, also scripting which refer to the original key


            ActingOnType = original.ActingOnType; 
            AttacksVerminValueToSet = original.AttacksVerminValueToSet;

            
            Inputs = original.Inputs;
            Outputs = original.Outputs;
            
            AgentActionState = original.AgentActionState;
            AgentAnimationStates = original.AgentAnimationStates;

            UseWorkerEnergyAsProductionFactor = original.UseWorkerEnergyAsProductionFactor;
            Stances = original.Stances;
            RequiredSkill = original.RequiredSkill;
            PhysicalWorkFactor = original.PhysicalWorkFactor;
            RequiresBoldStance = original.RequiresBoldStance;
            ProcessToolSetKey = original.ProcessToolSetKey;
            WorkNeeded = original.WorkNeeded;
            IsMetaAction = original.IsMetaAction;

            HasNoEndpoint = original.HasNoEndpoint;
            WorkOrTimeNeeded = original.WorkOrTimeNeeded; // only shallow copy!!

            Triggers = original.Triggers;
            PreparedToolModifier = original.PreparedToolModifier;
            RepairAction = original.RepairAction;
            IsGathering = original.IsGathering;
            IsConsumeProcess = original.IsConsumeProcess;
            MoveOutputToWorkerWhenCompleted = original.MoveOutputToWorkerWhenCompleted;
            TierOrArea = original.TierOrArea;
            JobTypeKey = original.JobTypeKey;
            IsSalvageProcess = original.IsSalvageProcess;
            ReplenishAction = original.ReplenishAction;

            SatisfiesWorkerNeeds = original.SatisfiesWorkerNeeds;
           
        }

        /// <summary>
        /// call this after overriding any copied properties on the process
        /// </summary>
        public void InitDynamicProcess()
        {
            GameData.Instance.AllProcessTypes.Add(KeyName, this);           
            GameData.InitializeComputerGeneratedData(this);

            Initialize(); 

            PostLoadContentInitialize();

            // production graphs
            GameData.Instance.AddProcessToProductionGraph(this);
        }

        public override string ToString()
        {
            return Name + "(" + KeyName + ")";
        }

        public bool UsesWorkerEnergyAsProductionFactor
        {
            get
            {
                if (WorkNeeded == WorkerNeededOptions.WorkerNeeded) // RequiresWork)
                {
                    return UseWorkerEnergyAsProductionFactor;
                }

                return false;
            }
        }

        private float GetUpdateInterval()
        {
            // if it sets progress, use shorter interval.
            if (UpdateInterval.HasValue)
            {
                return UpdateInterval.Value;
            }
            else if (WorkNeeded == WorkerNeededOptions.WorkerNeeded) // RequiresWork)
            {
                return 2f;
            }
            else if (CreatesNewEntity)
            {
                return 2f;
            }
            else 
            {
                return 8f; // ??
            }
        }

        public void Initialize() 
        {
            
           
            
        }


        public bool AllowProcessOnBrokenTarget()
        {
            return IsMetaAction == true || RepairAction != null;
        }

        public bool NeedsImmovableInput()
        {
            if (Inputs != null)
            {
                if (Inputs.FirstOrDefault(i => i.InputIsImmovable()) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public EntityType ImmovableInput()
        {
            if (Inputs != null)
            {
                foreach (var item in Inputs)
                {
                    if (item.EntityType.IsImmovable())
                    {
                        return item.EntityType;
                    }
                }
            }

            return null;
        }

        public static void InitializeStances(SerializableDictionary<string, ChanceToTakeStance[]> stances, out Dictionary<StancesType, List<ChanceToTakeStance>> stanceTypes)
        {
            stanceTypes = null;

            if (stances != null)
            {
                // StanceTypes = new Dictionary<StancesType, List<StanceType>>();
                stanceTypes = new Dictionary<StancesType, List<ChanceToTakeStance>>();
                foreach (var item in stances)
                {
                    StancesType stancesType = GameData.Instance.AllStancesTypes[item.Key];
                    foreach (var item2 in item.Value)
                    {
                        item2.Initialize();

                        Common.AddToMultiList(stanceTypes, stancesType,
                            item2); // GameData.Instance.AllStanceTypes[item2]);
                    }
                }
            }
        }

        /// <summary>
        /// perhaps we later want to make this designer controlled.
        /// </summary>
        /// <returns></returns>
        public bool UsesAnchor()
        {
            if (IsSpecialActionType
                && IsConstruction())
            {
                return true;
            }

            return false;

        }

        /// <summary>
        /// is false for the Original process...
        /// </summary>
        public bool IsSpecialActionType
        {
            get
            {
                return IsOriginalSpecialAction || ActingOnType != null;
            }
        }

        public float GetTimeNeeded(float totalBulkOfInput = 1f)
        {

            float bulkFactor = 1f;
            // not implemented... causes problems when the evaluator does not know the bulk...
            /* if (ProcessType.WorkOrTimeNeeded.MultiplyByBulk && TotalBulkOfInputs > 0f)
             {
                 factor = TotalBulkOfInputs;
             }*/

            float timeNeeded;
            timeNeeded = WorkOrTimeNeeded.DaysNeeded.Value;
           

            return bulkFactor * timeNeeded;

        }

       /* public float GetTimeNeeded() 
        {           
            if (WorkOrTimeNeeded.DaysOfWorkNeeded.HasValue)
            {
                return WorkOrTimeNeeded.DaysOfWorkNeeded.Value;
            }
            else
            {
                return WorkOrTimeNeeded.DaysNeeded.Value;
            }
        }*/

        public float CalculateProgressDelta(double seconds, float skillProductivity, float energyProductivity, float toolProductivity, float totalInputBulk = 1f, float averageLaborEfficiency = 1f)
        {
           
            float workOrTimeNeeded = GetTimeNeeded(totalInputBulk);

           // float totalDuration = EstimateTotalDurationInDays(worker, workOrTimeNeeded, /*skillProductivity, energyProductivity,*/ toolProductivity, averageLaborEfficiency);
            float totalDuration = EstimateTotalDurationInDays(workOrTimeNeeded, skillProductivity, energyProductivity, toolProductivity, averageLaborEfficiency);

            if (Common.IsZero(totalDuration))
            {
                return 1f; // instant
            }
            else
            {
                return (float)(seconds * The.Sim.DateAndTime.DaysPerSecond / totalDuration);
            }
        }


      /*  public float CalculateProgressDelta(double seconds, Entity worker, float toolProductivity, float totalInputBulk = 1f, float averageLaborEfficiency = 1f)
        {
           // float progressDelta;

            float workOrTimeNeeded = GetTimeNeeded(totalInputBulk);

            float totalDuration = EstimateTotalDurationInDays(worker, workOrTimeNeeded, toolProductivity, averageLaborEfficiency);

            if (Common.IsZero(totalDuration))
            {
                return 1f; // instant
            }
            else
            {
                return (float)(seconds * The.Sim.DateAndTime.DaysPerSecond / totalDuration);
            }
        }*/

        
       /* private float CalculateProgressDeltaFromWorkNeeded(double seconds, Entity worker,
            float manSecondsOfWorkNeeded, float? toolProductivityFactor = 1f, float averageLaborEfficiency = 1f)
        {

            float totalDuration = EstimateTotalDurationInDays(worker, manSecondsOfWorkNeeded, toolProductivityFactor, averageLaborEfficiency);

            return (float)(seconds * The.Sim.DateAndTime.DaysPerSecond / totalDuration);

        }*/

      

        public bool IsConstruction()
        {
            if (this.Outputs != null)
            {
                return Outputs[0].FinalEntityTypeToCreate.StructureType != null;
            }

            return false;
        }

        public AgentActionHooks GetStartHook()
        {
            AgentActionHooks defaultHook;
            if (IsConstruction())
            {
                defaultHook = AgentActionHooks.StartConstructing;
            }
            else if (IsGathering)
            {
                defaultHook = AgentActionHooks.StartHarvesting;
            }
            else
            {
                defaultHook = AgentActionHooks.StartProducing;
            }

            return defaultHook;
        }

        public bool IsPartOfAttainableCalculation()
        {
            return Outputs != null && (IsAccessibleFromInventoryPanel() || IsSalvageProcess == true);
        }

        /// <summary>
        /// why include harvesting and hunting/killing..?
        /// </summary>
        /// <returns></returns>
        public bool IsAccessibleFromInventoryPanel()
        {
            return Outputs != null // "inspect farm plot" 
               // && IsProductionDegradeProcess == false
                && IsPartOfProductionChain();
            /*
                && IsSalvageProcess == false
                && IsConsumeProcess == false
                && IsInnateExtractionProcess == false               
                && IsReplenishProcess == false
                && RepairAction == null;*/

        }
        
        /// <summary>
        /// make clay from clay pit
        /// </summary>
        /// <returns></returns>
       /* public bool IsContinuousProduction()
        {
            return Outputs != null && IsPartOfProductionChain() && !IsHarvesting && !IsKilling;
        }*/

        /// <summary>
        /// these processes are ordered outside the inventory panel, but still are part of the chain for displaying attainable, tooltips and so on.
        /// </summary>
        /// <returns></returns>
        public bool IsPartOfProductionChainButCannotOrderFromInventory()
        {
            return IsGathering || IsKilling || ActingOnType != null || IsUpgrade || IsPseudoProcess;
        }

        public enum ProductionUI { None, Build, Slider, Other }

        public ProductionUI GetProductionUI()
        {
            bool processCanBeOrderedFromInventory = IsSalvageProcess == false && !IsPartOfProductionChainButCannotOrderFromInventory();
            if (processCanBeOrderedFromInventory)
            {
                if (IsConstruction())
                {
                    return ProductionUI.Build;
                }
                else
                {
                    return ProductionUI.Slider;
                }
            }
            else
            {
                return ProductionUI.Other; //None;
            }
        } 


        /// <summary>
        /// controls the data in ProcessYieldsThisOutput and ProcessesUsingThisInput
        /// </summary>
        /// <returns></returns>
        public bool IsPartOfProductionChain()
        {
            return IsSalvageProcess == false
                && IsConsumeProcess == false
                && IsInnateExtractionProcess == false
                && IsReplenishProcess == false
                && RepairAction == null;                
        }

        public float EstimateTotalDurationInDays(Entity worker,
           float timeNeeded, 
            float toolProductivityFactor = 1f, float averageLaborEfficiency = 1f)
        {
            float skillProductivity = 1f;
            float energyProductivity = 1f;
            if (worker != null)
            {
                skillProductivity = worker.Intelligence.GetSkillProductionFactor(RequiredSkillType);

                energyProductivity = GetEnergyProductivity(worker);

                /*
                if (worker.BiologicalEntity != null)
                {
                    energyFactor = worker.BiologicalEntity.EnergyLevel;
                }*/
            }          

            return EstimateTotalDurationInDays(timeNeeded, skillProductivity, energyProductivity, 
                toolProductivityFactor, averageLaborEfficiency);

        }


        /// <summary>
        /// estimate the duration with the available tools, skills, worker energy level...
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="timeNeeded"></param>
        /// <param name="toolProductivity"></param>
        /// <param name="averageLaborEfficiency"></param>
        /// <returns></returns>
        public float EstimateTotalDurationInDays(
           float timeNeeded,
           float skillProductivity = 1f, float workerEnergyProductivity = 1f, 
           float toolProductivity = 1f, float averageLaborEfficiency = 1f)
        {

          /*  float energyLevelFactor = 1f;
           // float skillFactor = 1f;

            // move this:
            if (WorkNeeded == WorkerNeededOptions.WorkerNeeded) // RequiresWork)
            {
                if (UsesWorkerEnergyAsProductionFactor)
                {
                    energyLevelFactor = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyProductionFactor, 1f, workerEnergyLevel);
                }

            }*/
            

            float totalProductivity = ProcessJob.GetTotalFactorProductivity(
                                                    workerEnergyProductivity, 
                                                    toolProductivity, 
                                                    skillProductivity, 
                                                    averageLaborEfficiency);

            return timeNeeded / totalProductivity;

        }

        public float GetEnergyProductivity(Entity worker) //float workerEnergyLevel)
        { 
            float energyLevelFactor = 1f;

            if (worker.BiologicalEntity != null)
            {
                float workerEnergyLevel = worker.BiologicalEntity.EnergyLevel;
                               
                if (WorkNeeded == WorkerNeededOptions.WorkerNeeded) // RequiresWork)
                {
                    if (UsesWorkerEnergyAsProductionFactor)
                    {
                        energyLevelFactor = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyProductionFactor, 1f, workerEnergyLevel);
                    }

                }
            }

            return energyLevelFactor;
        }


        /// <summary>
        /// returns the output that would result from one input to the process.
        /// 
        /// this is just a simplified test that does not consider multiple inputs, tools or energy...
        /// it will only be used for innate extraction processes, not for planning production in general... for that, more advanced methods are needed
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<Tuple<EntityType, float>> TestProcess(IKnownEntityData inputEntity)
        {
            if (Common.IsZero(inputEntity.Bulk))
                return null;


            if (InputsByType.Count != 1)
                return null;

            Input input;
            if (!InputsByType.TryGetValue(inputEntity.EntityType, out input))
            {
                return null;
            }

            // type + bulk of outputs
            List<Tuple<EntityType, float>> products = null;

            // type + bulk of extracted substances
            Dictionary<string, float> extractedSubstances = null;

            // only one input will be handled:
            if (input.Amount.NoOfItems == 1)
            {
                // return the list of output types that would result from processing this input item

                // test conversion:
                if (input.Amount.SubstanceTypes != null)
                {
                    // convert substances:
                    // SubstanceComponent substances;
                    if (inputEntity.SubstanceBulkAmounts != null) // inputEntity.Find(out substances))
                    {
                        foreach (var substanceType in input.Amount.SubstanceTypes)
                        {
                            // extract a part of the input:
                            // extract from different substances (meat etc.)

                            //   SubstanceType substanceType = input.Amount.Extract.ExtractSubstanceType;

                            SubstanceAmount bulkOfSubstance;

                            if (inputEntity.SubstanceBulkAmounts.TryGetValue(substanceType, out bulkOfSubstance)) //= substances.BulkAmounts[substanceType];
                            {
                                if (Common.IsGreaterThan(bulkOfSubstance.Amount, 0f))
                                {
                                    Common.AddToDictionary(ref extractedSubstances, substanceType.KeyName, bulkOfSubstance.Amount);
                                }
                            }
                        }

                        // now see which outputs can be created:
                        if (extractedSubstances != null)
                        {
                            foreach (var output in Outputs)
                            {
                                int noOfItemsToCreate = 1;
                                float? bulkOfEachOutputItem; // if null, use fixed bulk for the entity type
                             
                                // compute the amounts we are going to create:
                                if (!output.GetOutputAmountsToCreate(inputEntity.Bulk, extractedSubstances, out noOfItemsToCreate, out bulkOfEachOutputItem))
                                {
                                    return null;
                                }
                                else
                                {
                                   // Common.AddToList(ref products, output.FinalEntityType);
                                    // store the type+total bulk:
                                    Common.AddToList(ref products, 
                                        new Tuple<EntityType, float>(output.FinalEntityTypeToCreate, 
                                        noOfItemsToCreate * (bulkOfEachOutputItem ?? 0f)));
                                }
                            }

                            return products;
                        }
                    }
                    else
                    {
                        // convert the full bulk of the input - this will always result in outputs as long as the input bulk is > 0:
                        CreateTestOutputs(ref products);

                        return products;
                    }
                }
                else
                {
                    // regular production:
                    CreateTestOutputs(ref products);

                    return products;
                }

            }

            return null;

        }

        private void CreateTestOutputs(ref List<Tuple<EntityType, float>> products)
        {
            products = new List<Tuple<EntityType, float>>();
            foreach (var item in Outputs)
            {
                products.Add(
                    new Tuple<EntityType, float>(item.FinalEntityTypeToCreate, item.FinalEntityTypeToCreate.ItemType.MaximumBulk ?? 0f));
            }
        }
        /*
        private void CreateTestOutputs(ref List<EntityType> products)
        {
            products = new List<EntityType>();         
            foreach (var item in Outputs)
            {
                products.Add(item.FinalEntityType);
            }
        }*/

        public void PostLoadContentInitialize()
        {

            List<ProcessTypeActionHook> eventActions;
            if (UseOriginalProcessEvents
                && GameData.Instance.EventHooksByProcessType.TryGetValue(OriginalProcess, out eventActions)) // won't work with unique processes. need to look up the original
            {

                // group actions by hook type:
                foreach (var item in eventActions)
                {
                    List<ActionSets> actions = null;

                    if (!this.EventActions.TryGetValue(item.Hook, out actions))
                    {
                        actions = new List<ActionSets>();
                        this.EventActions.Add(item.Hook, actions);
                    }

                    actions.Add(GameData.Instance.AllActionSets[item.ActionSetsKey]);

                }
            }

            FinalUpdateInterval = GetUpdateInterval();

            if (IsGathering) // ?
            {
                if (Outputs != null)
                {
                    ResourceType resourceType;
                    foreach (var item in Outputs)
                    {
                        if (GameData.Instance.ItemHarvestSource.TryGetValue(item.FinalEntityTypeToCreate, out resourceType))
                        {
                            this.ResourceTypeInput = resourceType;
                            break;
                        }

                    }
                }
            }
          
          /*
            if (Outputs != null) // why not Inputs???
            {
                ResourceType resourceType;
                foreach (var item in Outputs)
                {
                    if (GameData.Instance.ItemHarvestSource.TryGetValue(item.FinalEntityTypeToCreate, out resourceType))
                    {
                        this.ResourceTypeInput = resourceType;
                        break;
                    }

                }
            }*/

        }


        public void SetIsSalvageProcess()
        {
            IsSalvageProcess = true;
        }


        /// <summary>
        /// returns true if no input is an item! 
        /// True for harvesting, hunting etc.
        /// 
        /// make clay???
        /// </summary>
        /// <returns></returns>
      /*  private bool GetIsPrimaryProcess()
        {
            if (InputsByType != null)
            {
                foreach (var item in InputsByType)
                {
                    if (item.Key.ItemType != null)
                    {
                        return false;
                    }
                }
                
            }
            
            return true;
        }*/

        private bool GetIsKilling()
        {
            if (Outputs != null)
            {
                foreach (var item in Outputs)
                {
                    if (item.FinalEntityTypeToCreate.ItemType != null
                        && item.FinalEntityTypeToCreate.ItemType.CarcassType != null)
                    {
                        return true;
                    }
                }

            }

            return false;
        }


        public bool ConsumeInputsAtBeginning()
        {
            return !IsReplenishProcess;

        }

     /*   protected void Init(Dictionary<EntityType, MaterialInput> inputs, EntityType output, float mansecsNeeded, float energyNeeded)
        {
            this.Inputs = inputs;
            this.Output = output;

            this.ManSecondsOfWorkNeeded = mansecsNeeded;
            this.EnergyNeeded = energyNeeded;
        }*/

        public bool UsesItemsAsInput()
        {
            // TODO
            return true;
        }

     

        public void PreInitValidate(ref List<string> listOfErrors) 
        { }
        public void PostInitValidate(ref List<string> listOfErrors) 
        {
            

        }

        public void PostDataCompleteInitialize()
        {
            if (Inputs != null)
            {
                InputsByType = new Dictionary<EntityType, Input>();
                foreach (var item in Inputs)
                {
                    item.PostDataCompleteInitialize();

                    InputsByType.Add(item.EntityType, item);
                }
            }

            if (RequiredSkill != null)
            {
                RequiredSkillType = GameData.Instance.AllSkillTypes[RequiredSkill];
            }

            if (!string.IsNullOrEmpty(ProcessToolSetKey))
            {
                ProcessToolSet = GameData.Instance.AllProcessToolSets[ProcessToolSetKey];
            }

            if (Outputs != null)
            {
                foreach (var item in Outputs)
                {
                    item.PostDataCompleteInitialize();

                    if (item.FinalEntityTypeToCreate.Upgrader != null)
                    {
                        this.IsUpgrade = true;
                    }
                }
            }

            if (InputsByType != null &&
                IsReplenishProcess == false) // avoid handling outputs... later, we may be able to unify replenish and outputs
            {
                foreach (KeyValuePair<EntityType, Input> kvp in InputsByType)
                {

                    if (kvp.Value.EntityType.ItemType != null && kvp.Value.IsConsumed == false)
                    {
                        if (kvp.Value.BecomesPartOfProductType == null)
                        {
                            // set default value
                            kvp.Value.BecomesPartOfProductType = Outputs[0].FinalEntityTypeToCreate;
                        }
                        else
                        {
                            foreach (var item in Outputs)
                            {
                                if (item.FinalEntityTypeToCreate.KeyName == kvp.Value.BecomesPartOfProduct)
                                {
                                    kvp.Value.BecomesPartOfProductType = item.FinalEntityTypeToCreate;
                                    break;
                                }
                            }
                        }
                    }

                    /*
                    if (kvp.Value.EntityType.NonLivingType != null && kvp.Value.EntityType.NonLivingType.SalvageProcessType == this)
                    {
                        isSalvageProcess = true;
                    }*/
                }
            }

            /*
            if (ProcessToolSet != null)
            {
                ProcessToolSet.Initialize(); // delete this?
            }*/

            if (AgentAnimationStates != null)
            {
                AgentAnimationStatesList = AgentAnimationStates.ToList();
            }

            if (WorkOrTimeNeeded != null)
            {
                WorkOrTimeNeeded.Initialize();
            }

            /*if (TogglesSpecialActionProcess != null)
            {
                TogglesSpecialActionProcessType = GameData.Instance.AllProcessTypes[TogglesSpecialActionProcess];
            }*/

            if (DisablesSpecialActionLockProcesses != null)
            {
                DisablesSpecialActionLockProcessTypes = new List<ProcessType>();
                foreach (var item in DisablesSpecialActionLockProcesses)
                {
                    DisablesSpecialActionLockProcessTypes.Add(GameData.Instance.AllProcessTypes[item]);
                }              
            }

            if (EnablesSpecialActionLockProcesses != null)
            {
                EnablesSpecialActionLockProcessTypes = new List<ProcessType>();
                foreach (var item in EnablesSpecialActionLockProcesses)
                {
                    EnablesSpecialActionLockProcessTypes.Add(GameData.Instance.AllProcessTypes[item]);
                }
            }


            if (DisablesSharedActionProcesses != null)
            {
                DisablesSharedActionProcessTypes = new List<ProcessType>();
                foreach (var item in DisablesSharedActionProcesses)
                {
                    DisablesSharedActionProcessTypes.Add(GameData.Instance.AllProcessTypes[item]);
                }
            }

            if (EnablesSharedActionProcesses != null)
            {
                EnablesSharedActionProcessTypes = new List<ProcessType>();
                foreach (var item in EnablesSharedActionProcesses)
                {
                    EnablesSharedActionProcessTypes.Add(GameData.Instance.AllProcessTypes[item]);
                }
            }

            if (JobTypeKey != null)
            {
                JobType = (ProcessJobType)GameData.Instance.AllJobTypes[JobTypeKey];
                //BaseDataLoader.AddToTagCollection(this, this.JobTypeKey, GameData.Instance.ProcessTypesByJobType); 
            }

            if (TierOrArea != null)
            {
                TierOrAreaType = new TierOrAreaType(TierOrArea);                   
            }  

            InitializeStances(Stances, out StanceTypes);
            
            //IsPrimaryProcess = GetIsPrimaryProcess();
            IsKilling = GetIsKilling();

            IsSalvageWithoutWaste = GetIsSalvageWithoutWaste();
        }

        bool GetIsSalvageWithoutWaste()
        {
            if (IsSalvageProcess)
            {
                Input input = Inputs[0];
                var parts = input.EntityType.NonLivingType.Parts;

                if (parts == null && Outputs == null)
                {
                    return true;
                }

                if (Outputs != null)
                {
                    // compare both ways
                    foreach (var item in Outputs)
                    {
                        if (!item.IsWasteProduct)
                        {
                            int amount;
                            if (!parts.TryGetValue(item.FinalEntityTypeToCreate, out amount)
                                || amount != (item.Amount.NoOfItems ?? 1))
                            {
                                return false;
                            }
                        }
                    }
                }
                else return false;

                if (parts != null)
                {
                    foreach (var item in parts)
                    {
                        var outputOfType = Outputs.FirstOrDefault(o => o.FinalEntityTypeToCreate == item.Key);
                        if (outputOfType == null
                            || (outputOfType.Amount.NoOfItems ?? 1) != item.Value)
                        {
                            return false;
                        }
                    }
                }
                else return false;

                return true;
            }
            else return false;           
        }


        public void PreDataCompleteValidate(ref List<string> listOfErrors) 
        {
            if (Inputs != null)
            {               
                foreach (var item in Inputs)
                {
                    item.PreDataCompleteValidate(ref listOfErrors);
                }
            }


            if (Outputs != null)
            {
                foreach (var item in Outputs)
                {
                    item.PreDataCompleteValidate(ref listOfErrors);
                }
            }


            if (!string.IsNullOrEmpty(ProcessToolSetKey))
            {
                ProcessToolSet tool;
                EntityType.ValidateGameDataTypeExists(ref listOfErrors, ProcessToolSetKey, GameData.Instance.AllProcessToolSets, out tool);
                
            }
        }
       
        public void PostDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (InputsByType != null)
            {

                // regular production
                foreach (var input in InputsByType)
                {
                    input.Value.Validate(ref listOfErrors);

                    if (input.Value.EntityType.ItemType != null && input.Value.IsConsumed == false && input.Value.BecomesPartOfProductType == null) // ignore non-item inputs such as trees and living entities
                    {
                        EntityType.CreateValidationError(ref listOfErrors, "The process input does not have a correct output specified to become part of.");
                    }

                    if (!IsSalvageProcess)
                    {
                        if (input.Value.EntityType.ItemType != null && input.Value.IsConsumed == false
                            && (input.Value.BecomesPartOfProductType.Parts == null || !input.Value.BecomesPartOfProductType.Parts.ContainsKey(input.Key)))
                        {
                            EntityType.CreateValidationError(ref listOfErrors, string.Format("The process input {0} is specified to become a part of the output, but the output entity type does not have such a part defined.", input.Key.KeyName));
                        }
                    }
                    if (input.Value.EntityType.ItemType != null && input.Value.IsConsumed == false)//check if this validation covers all cases where problems could occur
                    {
                        if (Outputs != null)
                        {
                            if (Outputs.Count() == 1 && IsSalvageProcess == false)
                            {
                                //this code validate that all inputs are put into the output,
                                EntityType outputEntityType = GameData.Instance.AllEntityTypes[Outputs[0].EntityTypeToCreate];
                                if (outputEntityType.Parts != null)
                                {
                                    if (outputEntityType.Parts.ContainsKey(input.Value.EntityType))
                                    {
                                        if (outputEntityType.Parts[input.Value.EntityType] != input.Value.Amount.NoOfItems)
                                        {
                                            EntityType.CreateValidationError(ref listOfErrors, "The amounts of inputs did not match the number of parts of the same type in the output.");
                                        }
                                    }
                                }
                                else
                                {
                                    EntityType.CreateValidationError(ref listOfErrors, "The amounts of inputs did not match the number of parts of the same type in the output, because the parts have not been filled out.");
                                }
                            }
                        }
                    }

                    /*
                    We have to avoid the situation where a knife is used as a tool for making a knife-spear, meaning that if player only has one knife, 
                    * he will be allowed to order a knife-spear, but the process will stop as soon as it starts, because the knife is used as both a tool and material, 
                    * which means that he will then be lacking the tool for completing the process.
                    */
                    if (ProcessToolSet != null)
                    {
                        foreach (var item in ProcessToolSet.Tools)
                        {
                            foreach (var tool in item.Tools)
                            {
                                if (tool.ToolEntityTypes.Contains(input.Key))
                                {
                                    EntityType.CreateValidationError(ref listOfErrors, string.Format("The same entity type ({0}) is used as both tool and input. This is currently not allowed...", input.Key));

                                }

                            }

                        }
                    }

                }

                // if there is more than one input, and one of them is immobile, we cannot do this job because hauling jobs cannot be created for the other inputs since we don't know where they need to be hauled to:
                int noOfImmobileInputs = Inputs.Count(i => i.InputIsImmovable());
                if (noOfImmobileInputs > 1)
                {
                    EntityType.CreateValidationError(ref listOfErrors, "Only one input may have HasNoMaximumBulk or a maximum bulk over 1.");

                }

             
                if (ProcessToolSet != null)
                {
                    foreach (var tool in ProcessToolSet.Tools)
                    {
                        if (tool.NeedsImmobileTool())
                        {
                            if (noOfImmobileInputs > 0)
                            {
                                EntityType.CreateValidationError(ref listOfErrors, "It is not permitted to specify both an immobile input (HasNoMaximumBulk = true or maximum bulk over 1) and a required immobile tool.");

                            }
                        }
                    }
                }

                if (IsSalvageProcess)
                {

                    if (Inputs.Length != 1)
                    {
                        EntityType.CreateValidationError(ref listOfErrors, "Salvage processes should always have exactly one input.");
                    }

                    if (Outputs != null)
                    {
                        foreach (var output in Outputs)
                        {

                            // validate that the out type/amount exists in the parts list of the one input entity
                            bool validOutput = false;

                            foreach (var part in Inputs[0].EntityType.Parts)
                            {
                                if (output.EntityTypeToCreate == part.Key.KeyName)
                                {
                                    if (!output.IsWasteProduct)
                                    {
                                        if (output.Amount.NoOfItems.Value > part.Value) // TODO: is this needed to prevent errors?
                                        {
                                            EntityType.CreateValidationError(ref listOfErrors, "Process output had a valid item type but a higher amount than the salvaged item.");
                                        }
                                        validOutput = true;
                                        break;
                                    }
                                    else
                                    {
                                        EntityType.CreateValidationError(ref listOfErrors, string.Format("An output {0} was found in the parts list of the salvagable entity, even though it is marked as a waste product.", output.EntityTypeToCreate));
                                    }
                                }
                            }

                            if (!output.IsWasteProduct)
                            {
                                if (validOutput == false)
                                {
                                    EntityType.CreateValidationError(ref listOfErrors, "An output item did not match any of the salvagable entity's parts.");
                                }
                            }
                        }
                    }

                }

                /* if (!IsSalvageProcess)
                 {
                     ValidatePartsInProductCorrespondsWithInput(ref listOfErrors); // 
                 }*/
            }


            if (Outputs != null)
            {
                foreach (var item in Outputs)
                {
                    item.PostDataCompleteValidate(ref listOfErrors);
                }
            }

            // validate that a skill has been set, if not eating:
            /*  if (!IsConsumeProcess)
              {


              }*/

            if (KeyName == "makeClayPotUnglazed")
            {

            }

         //   ValidateToolCanBeReplenishedInOneGo(ref listOfErrors);

           
        }

        /// <summary>
        /// returns the tier area of the process or its outputs
        /// </summary>
        /// <returns></returns>
        public TierOrAreaType GetTierArea()
        {
            if (TierOrAreaType != null)
            {
                 return this.TierOrAreaType;
            }
            else if (Outputs != null)
            {
                foreach (var item in Outputs)
                {
                    if (!item.IsWasteProduct)
                    {
                        if (item.FinalEntityTypeToCreate.TierOrAreaType != null)
                        {
                            return item.FinalEntityTypeToCreate.TierOrAreaType;
                        }
                        
                    }
                }
            }           

            return null;
        }

        static float? worstCaseSkillAndEnergyFactorsRoot;

        private void ValidateToolCanBeReplenishedInOneGo(ref List<string> listOfErrors)
        {
           // float timeInDays = GetTimeNeeded();
           
          //  GameData.Instance.AIConstants.WorkTimeFactorToEvaluateToolEnergyUse; // *workOrTimeNeeded;    
   
            // validate with worst case of energy levels for tool users

            //float worstCaseEnergyFactor = GameData.Instance.Constants.ZeroEnergyProductionFactor;
            //float worstCaseSkillAndEnergyFactors = GameData.Instance.Constants.LowestCombinedSkillAndEnergyProductionFactors;

            if (ProcessToolSet != null)
            {
                foreach (var toolTypeCombo in ProcessToolSet.ToolTypeCombinations) // one example combination is: knife + chainsaw, prod. bonus 0.8
                {

                    foreach (var tool in toolTypeCombo.Tools)
                    {
                        if (tool.Item1.ContainerType != null)
                        {
                            RequiresReplenishType requiresReplenish = tool.Item1.ContainerType.GetRequiresReplenishType();
                            if (requiresReplenish != null)
                            {
                                if (worstCaseSkillAndEnergyFactorsRoot == null)
                                {
                                   // worstCaseSkillFactor = Skill.GetSkillProductionFactor(0.1f);
                                    worstCaseSkillAndEnergyFactorsRoot = (float)Math.Sqrt(GameData.Instance.Constants.LowestCombinedSkillAndEnergyProductionFactors);
                                }

                                float jobDuration = EstimateTotalDurationInDays(GetTimeNeeded(), 
                                    worstCaseSkillAndEnergyFactorsRoot.Value, worstCaseSkillAndEnergyFactorsRoot.Value, 
                                    toolTypeCombo.Productivity);

                                float neededFuel = requiresReplenish.RequiresFuelType.GetNeededFuel(jobDuration);
                                if (neededFuel > 1f)
                                {
                                    EntityType.CreateValidationError(ref listOfErrors, string.Format("Fuel required for {0}: {1} is too high. Each tool has to be able to be replenished in one go, so the required fuel bulk must not be higher than 1", tool.Item1, neededFuel));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// DISABLED
        /// this valdation seems sensible, but hurts creativity (improvedFireExtuingisher becomes impossible)
        /// </summary>
        /// <param name="listOfErrors"></param>
        private void ValidatePartsInProductCorrespondsWithInput(ref List<string> listOfErrors)
        {
            if (Outputs != null)
            {
                foreach (var output in Outputs)
                {
                    if (output.FinalEntityTypeToCreate.Parts != null)                    
                    { 
                        foreach (var part in output.FinalEntityTypeToCreate.Parts)
                        {
                            Input input;
                            if (InputsByType.TryGetValue(part.Key, out input))
                            {
                                if (input.BecomesPartOfProductType == output.FinalEntityTypeToCreate)
                                {
                                    if (input.IsConsumed)
                                    {
                                        EntityType.CreateValidationError(ref listOfErrors, string.Format("The part type {0} does not have a matching input #1.", part.Key));
                                    }
                                    else if (input.Amount.NoOfItems != part.Value)
                                    {
                                        EntityType.CreateValidationError(ref listOfErrors, string.Format("The part type {0} does not have enough matching inputs.", part.Key));                               
                                    }
                                }
                                else
                                {
                                    EntityType.CreateValidationError(ref listOfErrors, string.Format("The part type {0} does not have a matching input #2.", part.Key));
                                }
                            }
                            else
                            {
                                EntityType.CreateValidationError(ref listOfErrors, string.Format("The part type {0} does not have a matching input #3.", part.Key));
                                continue;
                            }
                            
                        }
                    }

                }
            }

        }

        /// <summary>
        /// validate after production graph has been created
        /// </summary>
        /// <param name="listOfErrors"></param>
        public void PostProcessGraphValidate(List<string> listOfErrors)
        {
            // disabled, since it prevents many production hacks (like stickframe)
           // ValidatePartsInProductCorrespondsWithInput(listOfErrors);



            /* disabled, since it seems obsolete...
            if (Outputs != null)
            {
                foreach (var output in Outputs)
                {
                    // validate butcher outputs:
                    if (output.Amount.Bulk != null)
                    {
                        if (output.Amount.Bulk.FractionOfInputBulk.HasValue)
                        {
                            if (output.FinalEntityType.ItemType != null
                                && output.FinalEntityType.ItemType.MaximumBulk.HasValue)
                            {
                                float minimumSizeOfInput = output.FinalEntityType.ItemType.MaximumBulk.Value / output.Amount.Bulk.FractionOfInputBulk.Value;

                                // validate that the fixed bulk size of this item can be yielded from the smallest creature that may be present in the game:
                                foreach (var input in Inputs)
                                {
                                    float bulk = 0;

                                    ValidateSufficientMinimumBulkOfInput(input.EntityType, output, minimumSizeOfInput, listOfErrors);

                                }
                            }
                        }
                        // TODO: validate substances?
                    }
                }
            }*/

        }

        /* disabled, since it seems obsolete...
        private void ValidateSufficientMinimumBulkOfInput(EntityType input, Output output, float minimumSizeOfInput, List<string> listOfErrors)
        {
          //  inputIsCarcass = false;
              
            if (input.ItemType.CarcassType != null)
            {
             //   inputIsCarcass = true;

                // get the creature type that yields this carcass:
                List<ProcessType> processes;
                if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(input, out processes))
                {                        
                    foreach (var process in processes) // 'hunting' process - only used for production chains
                    {
                        foreach (var item in process.InputsByType)
                        {
                                                        
                            if (item.Key.BiologicalType != null)
                            {
                               
                                bool hasMinimumBulk = item.Key.BiologicalType.MinimumBulk.HasValue;

                                if (hasMinimumBulk == true)
                                {
                                    bool minimumBulkIsLargeEnough = (item.Key.BiologicalType.MinimumBulk.Value > minimumSizeOfInput);
                                    //theBulkThatIsUsed = item.Key.BiologicalType.MinimumBulk.Value;

                                    if (item.Key.BiologicalType.MinimumBulk.Value < minimumSizeOfInput)
                                    {
                                        EntityType.CreateValidationError(ref listOfErrors, string.Format(
                                        "An output item {0} had a fixed bulk[{1}] and a fraction[{2}] set that implies a minimum bulk of the [{5}] carcass of [{4}]. However, the minimum possible bulk of the carcass/bio type is [{3}].",
                                        output.FinalEntityType.KeyName,
                                        output.FinalEntityType.ItemType.MaximumBulk.Value,
                                        output.Amount.Bulk.FractionOfInputBulk.Value,
                                        item.Key.BiologicalType.MinimumBulk.Value,                                      
                                        minimumSizeOfInput,
                                        item.Key));
                                    }

                                   // return minimumBulkIsLargeEnough;
                                }
                            }
                        }
                    }
                }
            }

            if (input.ItemType != null && input.ItemType.MaximumBulk.HasValue)
            {
                if (input.ItemType.MaximumBulk.Value < minimumSizeOfInput)
                {
                    EntityType.CreateValidationError(ref listOfErrors, string.Format(
                                            "An output item {0} had a fixed bulk[{1}] and a fraction[{2}] set that implies a minimum bulk of [{4}]. However, the current bulk of the item type is [{3}].",
                                            output.FinalEntityType.KeyName,
                                            output.FinalEntityType.ItemType.MaximumBulk.Value,
                                            output.Amount.Bulk.FractionOfInputBulk.Value,
                                            input.ItemType.MaximumBulk.Value,
                                            minimumSizeOfInput,
                                            input.ItemType.KeyName));
                }
            } 

        }*/



        public bool HasOutput
        {
            get
            {
                return Outputs != null && Outputs.Length > 0;
            }
        }

        /*
         * OLD: consumption of materials in stages
        public float FindMaxProgressWithAvailableMaterials(float currentProgress, float progressChange, int availableMaterials, float materialStageLength)
        {
            int currentStage = GetStage(currentProgress, materialStageLength);
            int nextStage = GetStage(currentProgress + progressChange, materialStageLength);

            int neededMaterials = nextStage - currentStage;

            if (neededMaterials > 0)
            {
                int consumed = (neededMaterials > availableMaterials ? availableMaterials : neededMaterials);

                if (consumed == neededMaterials)
                {
                    return Math.Min(1f, currentProgress + progressChange);
                }
                else
                {
                    // halt construction (just before) this stage:
                    return (float)Common.ClampBottom(((currentStage + consumed) * materialStageLength - Common.epsilon), 0);
                }
            }

            return Math.Min(1f, currentProgress + progressChange);
        }
        */

      /*  public float CalculateProgressDelta(double seconds, Entities.PersonEntity worker)
        {
            // TODO: Use manhours here???? instead of 0.002!?!?!?! 
            return (float)(seconds * 0.2 * worker.Skills[RequiredSkill].Value); // worker.ConstructionSkill);

        }
        */


        public bool CreatesNewEntity
        {
            get
            {
                if (Outputs != null)
                {
                    return Array.Exists(Outputs, o => o.FinalEntityTypeToCreate != null);
                }

                return false;
            }
        }
        
        public int NeededMaterials(float currentProgress, float progressChange, float materialStageLength)
        {
            int currentStage = GetStage(currentProgress, materialStageLength); // (int)(currentProgress / materialStageLength);
            int nextStage = GetStage(currentProgress + progressChange, materialStageLength);

            return nextStage - currentStage;

        }

        public int? GetOutputAmount(EntityType entityType)
        {
            int? amount;
            Output output = Outputs.FirstOrDefault(o => o.FinalEntityTypeToCreate == entityType);
            if (output != null)
            {
                amount = output.Amount.NoOfItems;
            }
            else
            {
                amount = null;
            }


            return amount;
        }

       

        /// <summary>
        /// Before start: Stage 0. Started: Stage 1. End stage: No. of inputs.
        /// </summary>
        /// <param name="currentProgress"></param>
        /// <param name="materialStageLength"></param>
        /// <returns></returns>
        public static int GetStage(float currentProgress, float materialStageLength)
        {

            if (Common.IsEqual(currentProgress, 0))
            {
                return 0;
            }
            else if (currentProgress >= 1)
            {   // make sure that the end stage is ignored:
                return (int)(1 / materialStageLength);
            }
            else
            {
                return (int)(currentProgress / materialStageLength) + 1;
            }
        }


        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
        }

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ProcessType))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()

            /*
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {
                    EntityType.GetEntityTypePropertySerializer(false),

                    new CustomXmlSerializer.XmlTypeMapping<SkillType, string> ()
                    {
                        GetterMethod = t => t == null ? null : t.KeyName,
                        SetterMethod = s => s == null ? null : GameData.Instance.AllSkillTypes[s]
                    }    ,
                    new CustomXmlSerializer.XmlTypeMapping<EntityType[], string[]>()
                    {
                        GetterMethod = t => t == null ? null : t.KeyName,
                                        
                        SetterMethod = s => s == null ? null : GameData.Instance.AllEntityTypes[s]
                    },

                  StructureType.GetMaterialInputSerializer(false)  // remove this when Inputs is removed!                  
                }*/
        };

        #endregion

       
    }

   /* public class Conversion
    {
        /// <summary>
        /// substance type key is optional. 
        /// will extract from full bulk if not specified
        /// </summary>
        public string[] Substances;


        [XmlIgnore]
        public List<SubstanceType> SubstanceTypes;


        public void Initialize()
        {
            SubstanceTypes = new List<SubstanceType>();
            foreach (var item in Substances)
            {
                SubstanceTypes.Add(GameData.Instance.AllSubstanceTypes[item]);
            }

            
        }

    }*/

    public class InputAmount
    {
        
        /// <summary>
        /// must always be filled!
        /// 
        /// we can extract from many items at a time. All of the extracted bulk (or substances) are put in a dictionary for the outputs to use.                         
        /// </summary>
        public int? NoOfItems;



        /// <summary>
        /// substance types are optional. 
        /// will extract from full bulk if not specified
        /// </summary>
        public string[] Substances;

        [XmlIgnore]
        public List<SubstanceType> SubstanceTypes;


        public void Initialize()
        {
            if (Substances != null)
            {
                SubstanceTypes = new List<SubstanceType>();
                foreach (var item in Substances)
                {
                    SubstanceTypes.Add(GameData.Instance.AllSubstanceTypes[item]);
                }
            }

           
        }

        public string AmountToString()
        {
            /*if (Bulk.HasValue)
            {
                return Bulk.Value.ToString("F1");
            }
            else*/ if (NoOfItems.HasValue)
            {
                return NoOfItems.Value.ToString();
            }

            return "1";
        }


       /* public bool ShouldSerializeBulk()
        {
            return Bulk != null;
        }*/

        public bool ShouldSerializeNoOfItems()
        {
            return NoOfItems != null;
        }

      
    }

    /// <summary>
    /// indicates the bulk of one type of output. The bulk may be converted into one or more items, depending on whether the output item type has a fixed bulk set...
    /// 
    /// TODO: rename to SubstanceBulk and use it in both Input and Output to define an amount of a substance
    /// Add both fractions and fixed values, also max and minimum limits?
    /// </summary>
    public class Bulk
    {
        /// <summary>
        /// Either a fraction of total input bulk
        /// 
        /// TODO: move this out to OutputAmount. Instead, add a float that defines the fraction of a substance
        /// </summary>
        public float? FractionOfInputBulk;

        /// <summary>
        /// or the extracted bulk of a specific substance.
        /// 
        /// If required substances in the input is defined, this has to be defined also in order to link up an output to an input substance.
        /// </summary>
        public string InputSubstance;
    }

    public class OutputAmount
    {
       
        /// <summary>
        /// may or may not be specified
        /// it is possible to set both NoOfItems and FractionOfInputBulk to set the bulk on each item (like 3 wings).
        /// The fraction bulk will be divided among the items.
        /// 
        /// If not specified, the number of ouptut items will be computed from the total input bulk and whether or not the output item type has a maximum bulk set.
        /// </summary>
        public int? NoOfItems;

        /// <summary>
        /// this calculates the output as a fraction of the total inputs (when the inputs have variable bulk sizes like carcasses or logs)
        /// depending of the item type, this will either produce multiple items or a single item with a unique bulk.
        /// 
        /// it is possible to set both noOfItems and FractionOfInputBulk to set the bulk on each item (like 3 wings)
        /// 
        /// here substance dependencies are also defined.
        /// </summary>
        public Bulk Bulk; 

        public string AmountToString()
        {
            /*if (Bulk.HasValue)
            {
                return Bulk.Value.ToString("F1");
            }
            else*/ if (NoOfItems.HasValue)
            {
                return NoOfItems.Value.ToString();
            }

            return "1";
        }

        
        public bool ShouldSerializeNoOfItems()
        {
            return NoOfItems != null;
        }
    }

    /// <summary>
    /// specify the total time or work needed to move progress from 0 to 1.
    /// We can use the same data for open-ended processes!
    /// 
    /// This should be interpreted as 'per unit'?
    /// </summary>
    public class WorkOrTime
    {
        /// <summary>
        /// a full day is 1 (midnight to midnight)
        /// </summary>
        public float? DaysNeeded;

        
        /// <summary>
        /// If filled, this will result in DaysNeeded being overwritten. Only to be used when exactly matching an animation clip duration is required.
        /// </summary>
        public float? TimeInSecondsNeeded;

        /// <summary>
        /// this will factor in the bulk of the total inputs when calcualting the work or time needed.
        /// 
        /// NOT implemented - causes problems when the evaluator does not know the bulk...??
        /// </summary>
        public bool MultiplyByBulk = false;


        public void Initialize()
        {
            if (TimeInSecondsNeeded.HasValue)
            {
                DaysNeeded = (float)(TimeInSecondsNeeded / DateAndTime.secondsPerDay);
            }
        }

        public bool ShouldSerializeManSecondsOfWorkNeeded()
        {
            return DaysNeeded != null;
        }

    }

    /// <summary>
    /// one of these inputs must be used. The designer can specify a tag for convenience, and the list will be compiled from that.
    /// </summary>
  /*  public class InputSet: IXmlSerializable
    {
        public string DesignerItemTag;

        // one of these inputs must be used:
        public Dictionary<EntityType, MaterialInput> Inputs;

        /// <summary>
        /// set this reference if the part becomes part of that product 
        /// </summary>
        public EntityType BecomesPartOfProduct;

        // specify properties and their effects on the product here...
        

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
        }

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(InputSet))
        {
            TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>() 
                {                    
                  StructureType.GetMaterialInputSerializer(false)                     
                }
        };

        #endregion
    }*/
}
