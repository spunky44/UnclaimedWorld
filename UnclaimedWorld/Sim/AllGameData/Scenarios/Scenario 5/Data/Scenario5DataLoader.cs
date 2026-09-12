using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data
{
    /// <summary>
    /// loads/defines the game data that this scenario depends on
    /// </summary>
    public class Scenario5DataLoader : DataLoader
    {

        public Scenario5DataLoader(): base(Config.DataType.RGScenario, 0.1f)
        {

        }


        protected override List<InGameEvents.Actions.EventActionType> InitEventActionTypes()
        {
            return EventActionLoader.Init();
        }

        protected override List<AllegianceEventType> InitAllegianceEvents()
        {
            List<AllegianceEventType> list = new List<AllegianceEventType>();

            list.Add(new AllegianceEventType()
            {
                KeyName = "cargoDeliveredToPlayer",
                Event = AllegianceEvents.CargoDeliveredToPlayer,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new ActionSetType[]
                    {
                        new ActionSetType()
                        {
                            KeyName = "sdfd367yw6tyutyutyutyruuetyjty4",
                            Actions = new EventActionType[]{
                                new EventActionDialog(){
                                    KeyName = "ttetttttt567567e75676eu5eu5eur",
                                   
                                    DisplayText = new DynamicText(){ Text = "BOAT ARRIVES \nThe characteristic sound and smell of its old gasifier engine filled the air as the trader's boat approached. The townspeople helped it to moor, eager to see what it brought. \n After the transactions were done and the various pieces of gossip and news had been exchanged, the boatman bid farewell. Soon the boat had disappeared behind the cliffs, only the smoke lingering in the air." },
                                    DisplayImage = "RiverBoat"                                      
                                }

                            } 
                        }     
                    }
                }
            });


            list.Add(new AllegianceEventType()
            {
                KeyName = "transportAbortedContract",
                Event = AllegianceEvents.TransportToPlayerAborted,
                ActionSets = new ActionSets()
                {
                    SetsOfActions = new ActionSetType[]
                    {
                        new ActionSetType()
                        {
                            KeyName = "0cad94e56utydddddddddytjddgjdtyu26972",
                            Actions = new EventActionType[]{
                                new EventActionDialog(){
                                    KeyName = "24tyutyutycb12-11a4r78kuijkdeuyta2",
                                    
                                    DisplayText = new DynamicText(){ Text = "Mission aborted." },
                                    DisplayImage = "RiverBoat"                                      
                                }

                            } 
                        }                                

                    }
                }
            });


            return list;

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

        protected override List<ClientSide.HelpTopics.HelpTopic> InitTutorialTopics()
        {
            return TutorialLoader.Init();
        }

        protected override List<EntityType> InitEntityTypes()
        {
            List<EntityType> listOfEntityTypes = new List<EntityType>();

    
            StructureLoader.Init(listOfEntityTypes);
            ItemsLoader.Init(listOfEntityTypes);
  

            return listOfEntityTypes;
        }

        protected override List<Maps.MapEditor.EntityData> InitEntityData()
        {
            return EntityDataLoader.Init();
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