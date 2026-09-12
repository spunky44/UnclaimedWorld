using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities.RepairTypes
{
    public enum RepairProcess
    {
        Production, // repair is possible using the same process as for producing the parent, using an input item of the same type in the case of parts replacement, no input for integrity/condition repair
        None, //repair is not possible
        Custom // repair uses a special process with input(s)
    }

    public class RepairType
    {
        /// <summary>
        /// Production: repair is possible using the same process as for producing the parent, using an input item of the same type in the case of parts replacement, no input for integrity/condition repair
        /// None: repair is not possible
        /// Custom: repair uses a special process with input(s), defined in ProcessType below
        /// </summary>
        public RepairProcess Repair;

        /// <summary>
        /// fill this in for Custom repairtype
        /// </summary>
        public string ProcessType;

        /// <summary>
        /// if defined, and Production is used, the repair process will hafve its production time computed from this factor and the production process' time.
        /// </summary>
        public float? ProductionTimeFactor;

        public void PreDataCompleteValidate(ref List<string> listOfErrors)
        {
            if (Repair == RepairProcess.Custom)
            {
                if (ProcessType == null)
                {
                    EntityType.CreateValidationError(ref listOfErrors, "Process is required when Custom is defined");
                }
            }
            else
            {
                if (ProcessType != null)
                {
                    EntityType.CreateValidationError(ref listOfErrors, "Process should not be defined unless Custom is defined");
                }
            }

        }

        public void PostDataCompleteInitialize()
        {
           
        }

        /// <summary>
        /// either creates a process or gets a reference. returns null if repair is not possible
        /// </summary>
        /// <param name="repairAction"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public ProcessType GetProcessType(RepairAction repairAction, EntityType parent, EntityType part)
        {
            if (Repair == RepairProcess.Production)
            {
                List<ProcessType> productionProcess;
                if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(parent, out productionProcess))
                {
                    return CreateRepairProcess(repairAction, parent, productionProcess[0], part);
                }

                return null;
            }
            else if (Repair == RepairProcess.Custom)
            {
                if (ProcessType != null)
                {
                    return GameData.Instance.AllProcessTypes[ProcessType];
                }
            }

            return null;
        }

        public const string integrityProcessName = "Repairing integrity";
        public const string integrityProcessSummary = "Fixing the integrity of the structure so it does not fall apart";

        public const string partsConditionProcessName = "Reconditioning";
        public const string partsConditionProcessSummary = "Repairing a '{0}' part to improve its condition.";
        public const string partsConditionProcessSummaryNoPlaceholder = "Repairing a part to improve its condition.";

        private ProcessType CreateRepairProcess(RepairAction repairAction, EntityType parent, ProcessType productionProcess, EntityType part)
        {
            // creates unique processes for every EntityType

           // ProcessType process = new ProcessType(productionProcess, );
            string key = "";
            string summary = "";
            string processName = "Doing maintenance"; //mp was: "Repairing" which was confusing because it doesn't work on broken structures
            string keyName = "";

            switch (repairAction)
            {
                case RepairAction.Integrity:
                    key = "integrity";
                    processName = integrityProcessName;
                    summary = integrityProcessSummary;

                    keyName = parent.KeyName + "_" + key;
                    break;
                case RepairAction.RemovePart: // create processes for every part type
                    key = "removePart";
                    keyName = parent.KeyName + "_" + key + "_" + part.KeyName;

                    break;
                case RepairAction.AddPart: // create processes for every part
                    key = "addPart";
                    keyName = parent.KeyName + "_" + key + "_" + part.KeyName;

                    break;
                case RepairAction.ReplacePart: // create processes for every part
                    key = "replacePart";
                    keyName = parent.KeyName + "_" + key + "_" + part.KeyName;

                    break;
                case RepairAction.PartsCondition: // create processes for every part
                    key = "reconditionPart";
                    processName = partsConditionProcessName;
                    summary = string.Format(partsConditionProcessSummary, part.Name.ToLower(Config.Culture));
                    keyName = parent.KeyName + "_" + key + "_" + part.KeyName;

                    break;
                case RepairAction.Condition: 
                    key = "recondition";
                    keyName = parent.KeyName + "_" + key; // + "_" + item.KeyName;

                    break;
            }

            ProcessType process = new ProcessType(productionProcess, keyName, null);


            float workOrTimeNeeded;
            if (this.ProductionTimeFactor.HasValue)
            {
                workOrTimeNeeded = productionProcess.GetTimeNeeded() * ProductionTimeFactor.Value; //  productionProcess.WorkOrTimeNeeded.DaysNeeded * productionTimeFactor.Value;
            }
            else
            {
                // compute default time:
                workOrTimeNeeded = productionProcess.GetTimeNeeded();

                if (repairAction == RepairAction.Integrity)
                { 
                    // repairing integrity does not scale with number of inputs
                    workOrTimeNeeded /= 3f; 
                }
                else
                {
                    int totalInputs = 2;
                    if (productionProcess.Inputs != null)
                    {
                        // scale with the number of inputs in the regular production process
                        totalInputs += productionProcess.Inputs.Sum(i => i.Amount.NoOfItems ?? 0);
                    }

                    workOrTimeNeeded /= totalInputs;
                }
            }

            process.WorkOrTimeNeeded = new WorkOrTime()
            {
                DaysNeeded = workOrTimeNeeded // this is for bringing the condition/progress from 0 to 1. // 0.02f // ?? TODO - scale with amount of damage?
            };

            process.Name = processName; // "Repairing";
            process.SummaryDescription = summary;

            process.RepairAction = repairAction;
            process.JobTypeKey = null; // uses static job type // "repairingJobType";
            
            if (IsReplaceAction(repairAction))
            {               
                process.Inputs = new Input[] { new Input() { 
                    IsConsumed = true,  // TODO: Repair processes don't have an output but also don't consume their input! needs a change in validation/  SimProcess to handle this case
                    Amount = new InputAmount() { NoOfItems = 1 }, Entity = part.KeyName } };
            }
            else
            {
                // no input (or output)
                process.Inputs = null;
                process.Outputs = null;
            }

            process.UseOriginalProcessEvents = false; // don't use construction events

            process.InitDynamicProcess();

            /*
            // copy fields
            process.AgentActionState = productionProcess.AgentActionState;
            process.AgentAnimationStates = productionProcess.AgentAnimationStates;

            process.UseWorkerEnergyAsProductionFactor = productionProcess.UseWorkerEnergyAsProductionFactor;
            process.Stances = productionProcess.Stances;
            process.RequiredSkill = productionProcess.RequiredSkill;
            process.PhysicalWorkFactor = productionProcess.PhysicalWorkFactor;
            process.ProcessToolSetKey = productionProcess.ProcessToolSetKey;

            GameData.Instance.AllProcessTypes.Add(process.KeyName, process); // ??           
            GameData.InitializeComputerGeneratedData(process);

            process.PostLoadContentInitialize();// ??

            */


            return process;

        }

        public static bool IsReplaceAction(RepairAction action)
        {
            if (action == RepairAction.ReplacePart || action == RepairAction.RemovePart || action == RepairAction.AddPart)
            {
                return true;
            }

            return false;
        }


    }
}
