using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Maps.MapEditor;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.AllGameData.Scenarios
{

    /// <summary>
    /// This class does not contain the scenario data - just methods to create or load the scenario.
    /// inherit from this for every RG scenario class
    /// </summary>
    public abstract class ScenarioLoader
    {
        /// <summary>
        /// NOTE: this will be equal to the scenario name!
        /// Also used to link achievements! If the folder name is changed, make sure to change the achievement key too, in GameData
        /// </summary>
        public abstract string FolderName { get; }

        public const string scenarioHeaderFileName = "scenario.xml";
        public const string ScenarioDataFileName = "scenarioData.xml";

        public void WriteScenario()
        {
            Scenario scenarioHeader = GetScenarioHeader();
            BaseDataLoader.SerializeObject(scenarioHeader, FolderName, scenarioHeaderFileName, Config.DataType.RGScenario); 

            ScenarioData scenarioData = GetScenarioData();
            BaseDataLoader.SerializeObject(scenarioData, FolderName, ScenarioDataFileName, Config.DataType.RGScenario); 



        }

        public Scenario GetScenarioHeader()
        {
            //Scenario scenarioHeader = GetScenario();

            Scenario scenario = null;
            if (Sim.CurrentSerializeMode != Sim.SerializeMode.Read) //Sim.CurrentSerializeMode == Sim.SerializeMode.NoSerialize)
            {
                scenario = InitScenarioHeader();
              
              /*  scenario = new Scenario()
                {
                    // KeyName = "lostExplorers",
                    Name = FolderName,
                    TimeDateYear = new DateAndTime.TimeDateYear() { Year = 0, Day = 1, TimeOfDay = 0.36 },
                    MapKey = "f Map DemoIsland Jan 2014",
                    SummaryDescription = "After escaping a deadly swarm, a handful of PRECOL explorers crash land on an island and must survive until help arrives",
                    Description = "We have escaped the catastrophic attack by quadites that occurred just hours ago. Only minor injuries are reported. Our vehicle however, is non-functional after damage sustained in the attack and a subsequent crash-landing. \n  \nWe've landed in a firegrass biome. Geographical data are incomplete, and this biome has only been partially documented during the previous months of research. Still, we know enough about this environment to expect species of quadites. \n \nWe only hope not to see the vicious swarmer quadite that attacked us earlier today.", // We follow a group of scientists that are lost in the wilderness. They were part of the PRECOL team of explorers that were tasked with finding a suitable place for the first colony - unfortunately, it didn't go so well. \nThe site they had chosen was swarmed by alien wildlife, many team members were killed and those who managed to escape are now scattered in the surroundings. \n \nOur survivors are now far away from the rest of the Tau Ceti settlers, so chances of rescue are slim. \n \nThey will need to use their knowledge of the environment and survival techniques to hold out until they are hopefully rescued and rejoin the other settlers.

                    // Scenarios\Scenario 1\Scenario Screen\aircraftWreck_scenario_thumb.png
                    ThumbnailImage = "Scenarios/Scenario 1/Scenario Screen/aircraftWreck_scenario_thumb",
                    Image = "Survival",
                    SortOrder = 1

                };*/


            }
          /*  else
            {
                BaseDataLoader.DeserializeObject(FolderName, scenarioHeaderFileName, out scenario, Config.DataType.RGScenario);
            }*/

          
            BaseDataLoader.SerializeAndDeserializeObject(scenario, ref scenario, FolderName, scenarioHeaderFileName, Config.DataType.RGScenario);

            scenario.SetRGSource(); // mark this data as coming from RG (this field is not serialized on purpose!)


            return scenario;
        }

        protected abstract Scenario InitScenarioHeader(); 


        //protected abstract Scenario GetScenario();

        /// <summary>
        /// returns the scenario data, either by creating it  (NoSerialize) or by reading from disk
        /// </summary>
        /// <returns></returns>
        public ScenarioData GetScenarioData()
        {
             ScenarioData scenario = null;
             if (Sim.CurrentSerializeMode != Sim.SerializeMode.Read) //Sim.CurrentSerializeMode == Sim.SerializeMode.NoSerialize)
             {
                 scenario = InitScenarioData();               
             }

             BaseDataLoader.SerializeAndDeserializeObject(scenario, ref scenario, FolderName, ScenarioDataFileName, Config.DataType.RGScenario);

             return scenario;
        }


        protected abstract ScenarioData InitScenarioData(); 


        /// <summary>
        /// returns the file that can serialize/deserialize the special data types this scenario depends on
        /// </summary>
        /// <returns></returns>
        public abstract DataLoader GetDataLoader();
       


        public static EventActionType SpawnItemOtherSite(string keyname, string entityType,
                                                         string ownerAllegiance, string ownerExpeditionName,
                                                         double delay, string name = null, string site = null, string container = null, bool? offerForSale = null, int amount = 1)
        {
            EntityData entityData = new Maps.MapEditor.EntityData()
            {
                //Location = locationOffset.ToVector3(),
                EntityKey = entityType,               
                Name = name

            };

            if (!string.IsNullOrEmpty(ownerAllegiance))
            {
                entityData.OwnedBy = new AllegianceAndExpedition()
                {
                    AllegianceKey = ownerAllegiance,
                    ExpeditionKey = ownerExpeditionName
                };
            }

            SpawnEntityAction e = new SpawnEntityAction()
            {
                KeyName = keyname,
                DelayInSeconds = delay,                                   
                EntityData = entityData,
                Amount = new ValueNode() { Int = amount }                
            };

            if (container != null)
            {
                e.AddToContainer = new ContainerLocation()
                {                   
                    OfferForSale = offerForSale ?? false,
                    TargetObject = new TargetObject()
                    {
                        TargetObjectType = TargetObjectType.World,
                        GetList = new GetList()
                        {
                            HasPropertiesListKey = "sites", //"entities",
                            FilterCondition = new InGameEvents.Conditions.PropertyCondition() { PropertyKey = "name", ConstantStringEqual = site /* container*/ },
                            NextList = new GetList()
                            {
                                HasPropertiesListKey = "entities",
                                FilterCondition = new InGameEvents.Conditions.PropertyCondition() { PropertyKey = "name", ConstantStringEqual = container }                           
                            }
                        }                         
                    }
                };
            }


            return e;
        }




        /// <summary>
        /// not just items
        /// </summary>
        /// <param name="keyname"></param>
        /// <param name="locationOffset"></param>
        /// <param name="entityType"></param>
        /// <param name="owner"></param>
        /// <param name="delay"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EventActionType SpawnItemAtStartLocation(string keyname, Vector2 locationOffset, string entityType, 
            string ownerAllegiance, string ownerExpeditionName,
            double delay, string name = null, float? rotation = null, int amount = 1)
        {
            EntityData entityData = new Maps.MapEditor.EntityData()
            {
                Location = locationOffset.ToVector3(),
                EntityKey = entityType,                
                Rotation = rotation,
                //Owner = owner,
                Name = name

            };

            if (!string.IsNullOrEmpty(ownerAllegiance))
            {
                entityData.OwnedBy = new AllegianceAndExpedition()
                {
                    AllegianceKey = ownerAllegiance,
                    ExpeditionKey = ownerExpeditionName
                };
            }

            SpawnEntityAction e = new SpawnEntityAction()
            {
                KeyName = keyname,
                DelayInSeconds = delay,
                                  
                DynamicLocation = new DynamicLocation()
                {
                    PropertyKey = "startingLocation"
                },
                EntityData = entityData,
                Amount = new ValueNode() { Int = amount }                
            };

            return e;
        }

        public static ProcessAction RunProcess(string newEventKeyName, double delay, string processToUse, string actingOnEntity)
        {
            return new ProcessAction()
            {
                KeyName = newEventKeyName,
                DelayInSeconds = delay,

                FinishProcessImmediately = true,
                ProcessType = processToUse,
                ActingOnEntityName = actingOnEntity,
                WorkerObject = new TargetObject()
                {
                    GetList = new InGameEvents.PropertyObjects.GetList()
                    {
                        // get a random allegiance member
                        HasPropertiesListKey = "allegiances",
                        FilterCondition = new InGameEvents.Conditions.PropertyCondition()
                        {
                            PropertyKey = "keyName",
                            ConstantStringEqual = "playerAllegiance"
                        },
                        NextList = new InGameEvents.PropertyObjects.GetList()
                        {
                            HasPropertiesListKey = "persons" // returns all persons of player allegiance, then further up, the first item is picked implicitly
                        }
                    }
                }

            };
        }

        public static EventActionType SpawnEntity(string keyname, Vector2 location, string entityType, string ownerAllegiance, string ownerExpeditionName, double delay, string name = null, string processToUse = null, string actingOnEntity = null, int? amount = null)
        {
            EntityData entityData = null;

            if (entityType != null)
            {
                entityData = new Maps.MapEditor.EntityData()
                        {
                            Location = location.ToVector3(),
                            EntityKey = entityType,
                            Name = name

                        };
            }

            if (!string.IsNullOrEmpty(ownerAllegiance))
            {
                entityData.OwnedBy = new AllegianceAndExpedition()
                {
                    AllegianceKey = ownerAllegiance,
                    ExpeditionKey = ownerExpeditionName
                };
            }

            EvalNode amountNode = null;
            if (amount.HasValue)
            {
                amountNode = new ValueNode()
                {
                    Int = amount.Value
                };
            }

            return new SpawnEntityAction(keyname)
            {                
                DelayInSeconds = delay,
               
                EntityData = entityData,
                ProductionProcessToUse = processToUse,
                ActingOnEntityName = actingOnEntity,
                Amount = amountNode                
            };
        }

       
        public static EventActionType SpawnItemInsideContainer(string keyname, string container, string entityType,
            string ownerAllegiance, string ownerExpeditionName,
            double delay, bool? offerForSale = null, float? bulk = null, string storageCondition = null, string upgradeCategory = null, bool? isProductionOutput = null)
        {


            EntityData entityData = new Maps.MapEditor.EntityData()
            {              
                EntityKey = entityType,
                Bulk = bulk
            };

            if (!string.IsNullOrEmpty(ownerAllegiance))
            {
                entityData.OwnedBy = new AllegianceAndExpedition()
                {
                    AllegianceKey = ownerAllegiance,
                    ExpeditionKey = ownerExpeditionName
                };
            }

            SpawnEntityAction e = new SpawnEntityAction()
            {
                KeyName = keyname,
                DelayInSeconds = delay,
                
                EntityData = entityData,
                AddToContainer = new ContainerLocation()
                {                         
                    OfferForSale = offerForSale ?? false,
                    StorageCondition = storageCondition,
                    UpgradeCategory = upgradeCategory,
                    IsProductionOutput = isProductionOutput ?? false,

                    TargetObject = new InGameEvents.PropertyObjects.TargetObject()
                    {
                        TargetObjectType = InGameEvents.PropertyObjects.TargetObjectType.Root,
                        GetList = new InGameEvents.PropertyObjects.GetList()
                        {
                            HasPropertiesListKey = "entities",
                                FilterCondition = new InGameEvents.Conditions.PropertyCondition(){ PropertyKey = "name", ConstantStringEqual = container }                               
                        }
                    }
                }         
            };



            return e;
        }
    }
}
