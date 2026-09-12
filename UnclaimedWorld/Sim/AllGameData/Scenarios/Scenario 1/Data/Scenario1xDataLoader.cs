using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Trade;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data
{
    /// <summary>
    /// loads/defines the game data that this scenario depends on
    /// </summary>
    public class Scenario1DataLoader : DataLoader
    {

        public Scenario1DataLoader(): base(Config.DataType.RGScenario, 0.1f)
        {

        }

        protected override List<EntityType> InitEntityTypes()
        {
            base.InitEntityTypes();

            List<EntityType> listOfEntityTypes = new List<EntityType>();

            // split into files for easier data entry. MAke sure we don't call base data files!!
           
            Scenario_1.Data.ItemsLoader.Init(listOfEntityTypes);
            Scenario_1.Data.StructureLoader.Init(listOfEntityTypes);
            Scenario_1.Data.CreatureLoader.Init(listOfEntityTypes);
          

            return listOfEntityTypes;
        }

        protected override List<PersonalityType> InitPersonalityTypes()
        {
            return PersonalityLoader.Init();
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

        protected override List<Processes.ProcessType> InitProcessTypes()
        {
            return ProcessLoader.Init();
        }

        protected override List<EntityTypeDescription> InitEntityTypeDescriptions()
        {
            return EntityTypeDescriptionLoader.Init();
        }

        /*
        protected override List<TradeProfile> InitTradeProfiles()
        {
            return null;
        }

        protected override List<TradeGroup> InitTradeGroups()
        {
            // remove all trade groups that reference the deleted entities like robots:
            // TODO: find a way to override/delete all IGameData instances in BaseData from the scenario loader... should work when deserializing too, otherwise it is hacky
            List<TradeGroup> tradegroups = new List<TradeGroup>();
            tradegroups.Add(new TradeGroup()
            {
                KeyName = "advancedEquipmentTrade",
                DeleteRecord = true
            });

            tradegroups.Add(new TradeGroup()
            {
                KeyName = "occasionalAdvancedTradeItems",
                DeleteRecord = true
            });

            return tradegroups;

            //return null;
        }*/

    }
}
