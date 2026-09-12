using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities
{
   
    public class RequiresReplenishType
    {
        public string ReplenishProcess;

        [XmlIgnore]
        private ProcessType ReplenishProcessType;

        /// <summary>
        /// Same as eating/consume... could these be scrapped with optional inputs...? 
        /// </summary>
        [XmlIgnore]
        public Dictionary<EntityType, ProcessType> ReplenishProcesses;
        


        /// <summary>
        /// for substances
        /// </summary>
        public RequiresFuelType RequiresFuelType;

        // add magazintype here?
       // public MagazineContainerType MagazineContainerType;



        public void PostLoadContentInitialize(EntityType parent)
        {
            if (RequiresFuelType != null)
            {
                RequiresFuelType.PostLoadContentInitialize();
            }


            if (!string.IsNullOrEmpty(ReplenishProcess))
            {
                ReplenishProcessType = GameData.Instance.AllProcessTypes[ReplenishProcess];
                ReplenishProcessType.IsReplenishProcess = true;
                ReplenishProcessType.ReplenishAction = AI.Goals.GoalReplenish.ReplenishAction.Refuel; // Needed? important to show where the outputs shpould be placed

                // iterate over possible items, create a process for each
                foreach (var item in RequiresFuelType.FuelEntityTypes)
                {
                    InitReplenishProcess(parent, ReplenishProcessType, item, ref ReplenishProcesses);
                }
            }
        }

       
        public static void InitReplenishProcess(EntityType parent, ProcessType baseProcess, EntityType item, ref Dictionary<EntityType, ProcessType> ReplenishProcesses)
        {
            ProcessType process = new ProcessType();
            process.KeyName = parent.KeyName + "_replenish_" + item.KeyName;

            process.Inputs = new Input[] { 
                        new Input(){ Entity = item.KeyName, IsConsumed = false, 
                            Amount = new InputAmount()
                        {
                             NoOfItems = 1
                        }}};

            // why not copy original..?
            process.Name = baseProcess.Name;
            process.WorkOrTimeNeeded = baseProcess.WorkOrTimeNeeded;
            process.AgentActionState = baseProcess.AgentActionState;
            process.ReplenishAction = baseProcess.ReplenishAction;
            process.IsReplenishProcess = true;
            process.AgentAnimationStates = baseProcess.AgentAnimationStates;
            process.UseWorkerEnergyAsProductionFactor = false;
            process.Stances = baseProcess.Stances;
            process.JobTypeKey = baseProcess.JobTypeKey;

            GameData.Instance.AllProcessTypes.Add(process.KeyName, process);
            GameData.InitializeComputerGeneratedData(process);
            process.PostLoadContentInitialize();

            Common.AddToDictionary(ref ReplenishProcesses, item, process);

        }
    }
}
