using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data
{
    /// <summary>
    /// loads/defines the game data that this scenario depends on
    /// </summary>
    public class Scenario2DataLoader : DataLoader
    {

        public Scenario2DataLoader(): base(Config.DataType.RGScenario, 0.1f)
        {

        }


        protected override List<InGameEvents.Actions.EventActionType> InitEventActionTypes()
        {
            return EventActionLoader.Init();
        }

        protected override List<InGameEvents.PolledEventType> InitGlobalConditionalEvents()
        {
            return PolledEventsLoader.Init();
        }




        protected override List<AgentActionHook> InitAgentActionHooks()
        {
            return EventHooksLoader.InitAgentActionHooks();
        }

        protected override List<AttackTypeActionHook> InitAttackTypeEventHooks()
        {
            return EventHooksLoader.InitAttackTypeHooks();
        }

        protected override List<ProcessTypeActionHook> InitProcessTypeEventHooks()
        {
            return EventHooksLoader.InitProcessTypeHooks();
        }

        protected override List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
        {
            return DetectionEventHooksLoader.InitDetectEntityTypeHooks();
        }

        protected override List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
        {
            return DetectionEventHooksLoader.InitDetectResourceTypeHooks();
        }

        protected override List<InGameEvents.Actions.ActionSets> InitActionSets()
        {
            return ActionSetsLoader.Init();
        }

        protected override List<EntityType> InitEntityTypes()
        {
            List<EntityType> listOfEntityTypes = new List<EntityType>();

    
            StructureLoader.Init(listOfEntityTypes);
            ItemsLoader.Init(listOfEntityTypes);
  

            return listOfEntityTypes;
        }

        protected override List<Processes.ProcessType> InitProcessTypes()
        {
            return ProcessLoader.Init();
        }

        protected override List<EntityTypeDescription> InitEntityTypeDescriptions()
        {
            return EntityTypeDescriptionLoader.Init();
        }

    }
}