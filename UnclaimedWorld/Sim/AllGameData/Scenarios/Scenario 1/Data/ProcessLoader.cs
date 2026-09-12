using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.ClientSide.Renderables;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data
{
    public class ProcessLoader
    {

        public static List<Processes.ProcessType> Init()
        {
            List<ProcessType> listOfProcessTypes = new List<ProcessType>();

            #region salvage skimmer structures.

            float salvageSkimmer = AllGameData.ProcessLoader.moderateHardWork;



            listOfProcessTypes.Add(new ProcessType()
            {
                Name = AllGameData.ProcessLoader.salvagingName,
                KeyName = "salvageSkimmerHull",
                RequiredSkill = "menial",
                PhysicalWorkFactor = salvageSkimmer,
                IsSalvageProcess = true,
                Stances = AllGameData.ProcessLoader.GetSalvageStructureStances(),
                Inputs = new[] {
                            new Input(){ Entity = "structure:skimmerHull", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                Outputs = new[] {
                            new Output() { EntityTypeToCreate = "item:strippedHull", IsWasteProduct = true,Amount = new OutputAmount(){  NoOfItems = 1 }, RelativePlacement = new Vector2(11f, -10f) } , //(11f, -3f) (28f, -13f)

                            new Output() { EntityTypeToCreate = "item:panelScraps", IsWasteProduct = true, Amount = new OutputAmount(){  NoOfItems = 2 } } , //mp nov 2015 why are panelScraps waste?? they can be used..
                            new Output() { EntityTypeToCreate = "item:textile", IsWasteProduct = false ,Amount = new OutputAmount(){  NoOfItems = 1 } } ,
                            new Output() { EntityTypeToCreate = "item:scrapMetal", IsWasteProduct = false ,Amount = new OutputAmount(){  NoOfItems = 3  } } ,   //2                    
                            new Output(){ EntityTypeToCreate = "item:seatCushions", IsWasteProduct = false , Amount = new OutputAmount(){ NoOfItems = 3 } },
                            new Output(){ EntityTypeToCreate = "item:inactivatedFoodCoolerUnit", IsWasteProduct = false, Amount = new OutputAmount(){ NoOfItems = 1 } } 
                        },

                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForMediumCraftingTask },  //not too much time because the items appear instantaneously, so it might look confusing..
                AgentActionState = AnimAction.Salvaging,
                AgentAnimationStates = null,

            });


            listOfProcessTypes.Add(new ProcessType()
            {
                Name = AllGameData.ProcessLoader.salvagingName,
                KeyName = "salvageSkimmerTail",
                RequiredSkill = "menial",
                PhysicalWorkFactor = salvageSkimmer,
                IsSalvageProcess = true,
                Stances = AllGameData.ProcessLoader.GetSalvageStructureStances(),
                Inputs = new[] {
                            new Input(){ Entity = "structure:skimmerTail", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                Outputs = new[] {
                            new Output() { EntityTypeToCreate = "item:strippedTail", IsWasteProduct = true,Amount = new OutputAmount(){  NoOfItems = 1 }, RelativePlacement = new Vector2(-7f, 15f) } ,
                            new Output() { EntityTypeToCreate = "item:panelScraps", IsWasteProduct = false ,Amount = new OutputAmount(){  NoOfItems = 1 } } ,
                            new Output() { EntityTypeToCreate = "item:scrapMetal", IsWasteProduct = false ,Amount = new OutputAmount(){  NoOfItems = 1 } }
                        
                        },

                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForMediumCraftingTask },
                AgentActionState = AnimAction.Salvaging,
                AgentAnimationStates = null,

            });


            listOfProcessTypes.Add(new ProcessType()
            {
                Name = AllGameData.ProcessLoader.salvagingName,
                KeyName = "salvageSkimmerEngineSide",
                RequiredSkill = "menial",
                PhysicalWorkFactor = salvageSkimmer,
                IsSalvageProcess = true,
                Stances = AllGameData.ProcessLoader.GetSalvageStructureStances(),

                Inputs = new[] {
                            new Input(){ Entity = "structure:skimmerEngineSide", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                Outputs = new[] {
                            new Output() { EntityTypeToCreate = "item:strippedEngineSide", IsWasteProduct = true,Amount = new OutputAmount(){  NoOfItems = 1 }, RelativePlacement = new Vector2(0f, -2f) } ,
                            new Output() { EntityTypeToCreate = "item:superconductingWire", IsWasteProduct = false,Amount = new OutputAmount(){  NoOfItems = 2 } } ,
                            new Output() { EntityTypeToCreate = "item:propellerDome", IsWasteProduct = false, Amount = new OutputAmount(){  NoOfItems = 1 } } ,

                        },

                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForMediumCraftingTask },
                AgentActionState = AnimAction.Salvaging,
                AgentAnimationStates = null,

            });

            listOfProcessTypes.Add(new ProcessType()
            {
                Name = AllGameData.ProcessLoader.salvagingName,
                KeyName = "salvageSkimmerEngineTop",
                RequiredSkill = "menial",
                PhysicalWorkFactor = salvageSkimmer,
                IsSalvageProcess = true,
                Stances = AllGameData.ProcessLoader.GetSalvageStructureStances(),

                Inputs = new[] {
                            new Input(){ Entity = "structure:skimmerEngineTop", Amount = new InputAmount(){  NoOfItems = 1 } }
                        },

                Outputs = new[] {
                            new Output() { EntityTypeToCreate = "item:strippedEngineTop", IsWasteProduct = true,Amount = new OutputAmount(){  NoOfItems = 1 }, RelativePlacement = new Vector2(-2f, -1f) } ,
                            new Output() { EntityTypeToCreate = "item:superconductingWire", IsWasteProduct = false ,Amount = new OutputAmount(){  NoOfItems = 2 } } ,
                            new Output() { EntityTypeToCreate = "item:propellerDome", IsWasteProduct = false, Amount = new OutputAmount(){  NoOfItems = 1 } } ,
                        },

                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForMediumCraftingTask },
                AgentActionState = AnimAction.Salvaging,
                AgentAnimationStates = null,

            });

            #endregion

            #region makeImprovisedCookingPot
            listOfProcessTypes.Add(new ProcessType()
            {
                Name = AllGameData.ProcessLoader.toolmakingName,
                KeyName = "makeImprovisedCookingPot",
                RequiredSkill = "bushcraft",
                PhysicalWorkFactor = AllGameData.ProcessLoader.assembleTool,
                Stances = AllGameData.ProcessLoader.GetToolMakingStances(),
                Inputs = new[] {
                         new Input(){ Entity = "item:propellerDome",  IsConsumed = true, Amount = new InputAmount(){ NoOfItems = 1 } } },

                Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "item:improvisedCookingPot", Amount = new OutputAmount(){ NoOfItems = 1 }}
                    },
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForTinyCraftingTask },
                //Tools = "hammerSoft"
                AgentAnimationStates = new[] { AnimModifier.Improvised }
            });
            #endregion


            return listOfProcessTypes;
        }


    }
}


