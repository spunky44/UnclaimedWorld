using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data
{
    /// <summary>
    /// loads/defines the game data that this scenario depends on
    /// </summary>
    public class Scenario7DataLoader : DataLoader
    {

        public Scenario7DataLoader(): base(Config.DataType.RGScenario, 0.1f)
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
                    FireMode = ActionSetsToFire.FirstValid,
                    SetsOfActions = new ActionSetType[]
                    {
                        
                        new ActionSetType()
                        {
                            KeyName = "0ec7b15f-cc72-4f92-81d2-154f63b4619e",
                            Condition = new CustomCondition()
                            {
                                 TargetObject = new TargetObject() { TargetObjectType = TargetObjectType.TriggeringEntity },
                                  PropertyCondition = new PropertyCondition() { PropertyKey = "vehicleType", ConstantStringEqual = "Boat" /* "isBoatTransport", BoolValue = true*/ }
                            },
                            Actions = new EventActionType[]{
                                new EventActionDialog("3374bba8-bae3-4262-ae63-51afea836470")
                                {                                   
                                    DisplayText = new DynamicText(){ Text = "BOAT ARRIVES \nThe characteristic sound and smell of its old gasifier engine filled the air as the trader's boat approached. The colonists of Nadova's Site helped it to moor, eager to see what it brought. \n After the transactions were done and the various pieces of gossip and news had been exchanged, the boatman bid farewell. Soon the boat had disappeared back down the river." },
                                    DisplayImage = "RiverBoat"
                                }                               
                            } 
                            
                        },
                         new ActionSetType()
                        {
                            KeyName = "6180e5bc-575d-4882-a4ed-bdd8f9fa71bb",
                            Condition = new CustomCondition()
                            {
                                 TargetObject = new TargetObject() { TargetObjectType = TargetObjectType.TriggeringEntity },
                                  PropertyCondition = new PropertyCondition() { PropertyKey = "vehicleType", ConstantStringEqual = "Aircraft" /*, BoolValue = true*/ }
                            },
                            Actions = new EventActionType[]{
                                new EventActionDialog("37eb765b-9cf4-4f62-af97-f1c2bf264111")
                                {                                   
                                    DisplayText = new DynamicText(){ Text = "AIRCRAFT LANDS \nThe nimble aircraft landed swiftly and the members of Nadova's Site approached it just as quickly, braving the strong wind from its rotors, eager to see what it brought. \n After the transactions were done and the various pieces of gossip and news had been exchanged, the aircraft took off, soon shrinking to a dot on the horizon." },
                                    DisplayImage = "Rescue"
                                }                               
                            } 

                             // AIRCRAFT LANDS \nThe nimble aircraft landed swiftly and the members of Nadova's Site approached it just as quickly, braving the strong wind from its rotors, eager to see what it brought. \n After the transactions were done and the various pieces of gossip and news had been exchanged, the aircraft took off, soon shrinking to a dot on the horizon.

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

        protected override List<Maps.MapEditor.EntityData> InitEntityData()
        {
            return EntityDataLoader.Init();
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