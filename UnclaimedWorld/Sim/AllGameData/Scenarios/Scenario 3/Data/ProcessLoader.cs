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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data
{
    public class ProcessLoader
    {

        public static List<Processes.ProcessType> Init()
        {
           
            List<ProcessType> listOfProcessTypes = new List<ProcessType>();


            //this process needs to be in the general processloader because of the way that detection works...:

            SerializableDictionary<string, ChanceToTakeStance[]> constructionStances = AllGameData.ProcessLoader.kneelingOrStandingProduction;

            SerializableDictionary<string, ChanceToTakeStance[]> salvageStructureStances = AllGameData.ProcessLoader.kneelingOrStandingProduction; // new[] { LeggedLocomotor.Stance.Standing, LeggedLocomotor.Stance.Kneeling };


            listOfProcessTypes.Add(new ProcessType()
            {
                Name = "Build rope bridge",  //mp not sure if the object  is shown in task manager? then delete it here
                KeyName = "buildRopeBridge", //tutorial island
                JobTypeKey = "constructionJobType",
                RequiredSkill = "bushcraft",
                SummaryDescription = "Build a simple bridge with wire rope for crossing the gorge",
                Description = "The wire rope from the boat should be sufficient to build this.", //
                PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                Stances = AllGameData.ProcessLoader.kneelingProduction,
                //    RequiresBoldStance = true, //mp no reason for bold stance!

                Inputs = new[] {
                        new Input(){ Entity = "item:lines", IsConsumed = true, Amount = new InputAmount(){  NoOfItems = 1 } }
                    },

                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.8f * AllGameData.ProcessLoader.timeForSmallCraftingTask },

            });


            listOfProcessTypes.Add(new ProcessType()
            {
                Name = "Construct",
                KeyName = "constructSignalPyre",
                JobTypeKey = "constructionJobType",
                RequiredSkill = "menial", // "bushcraft",
                PhysicalWorkFactor = AllGameData.ProcessLoader.constructionExertion,
                Stances = constructionStances,

                Inputs = new[] { new Input() { Entity = "item:firewood", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true }, 
                   new Input() { Entity = "item:spoakBranches", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = false }             
               },

                Outputs = new[] {
                        new Output(){ EntityTypeToCreate = "structure:signalPyre", Amount = new OutputAmount(){ NoOfItems = 1 }},
                        new Output(){ EntityTypeToCreate = "item:firewood", IsWasteProduct = true, Amount = new OutputAmount(){ NoOfItems = 1 }} //because the action of lighting it requires an input. so i make sure there's firewood.

                        },
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForSmallCraftingTask },//should be very quick
                AgentActionState = AnimAction.Building

            });

            listOfProcessTypes.Add(new ProcessType()
            {
                Name = "Light signal pyre",
                KeyName = "lightSignalPyre",
                RequiredSkill = "menial",
                SummaryDescription = "Light the fire and start signalling for help",
                //   Description = "", //
                PhysicalWorkFactor = AllGameData.ProcessLoader.moderateWork,
                Stances = AllGameData.ProcessLoader.kneelingProduction,
                Inputs = new[] { new Input() { Entity = "item:firewood", Amount = new InputAmount() { NoOfItems = 1 }, IsConsumed = true }   //MP gives an error if it does not have an input                         
               },

                RequiresBoldStance = false,

                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = AllGameData.ProcessLoader.timeForTinyCraftingTask },

            });

            listOfProcessTypes.Add(new ProcessType()
            {
                Name = "Salvage",
                KeyName = "salvageBoatWreck",
                RequiredSkill = "menial",
                PhysicalWorkFactor = AllGameData.ProcessLoader.salvageStructure,
                IsSalvageProcess = true,
                Stances = salvageStructureStances,
                Inputs = new[] {
                    new Input(){ Entity = "structure:boatWreck", Amount = new InputAmount(){  NoOfItems = 1 } }},


                Outputs = new[] {
                                new Output() { EntityTypeToCreate = "item:strippedCatamaran", IsWasteProduct = true,Amount = new OutputAmount(){  NoOfItems = 1 }, RelativePlacement = new Vector2(6f, -29f) } , //4f, -29f   11f, -10f // IT WILL NOT GET DISPLACED CORRECTLY IF TERRAIN HAS BLOCKED TILES
                                new Output() { EntityTypeToCreate = "item:catamaranMast", IsWasteProduct = true,Amount = new OutputAmount(){  NoOfItems = 1 }, RelativePlacement = new Vector2(-84f, 64f) } ,  //11f, -10f  // IT WILL NOT GET DISPLACED CORRECTLY IF TERRAIN HAS BLOCKED TILES
                                                                     // made a hack to the itembillboard, i erased some of the billboard because i couldnt get it to sort beneath the catamaran hull.
                                new Output() { EntityTypeToCreate = "item:lines", Amount = new OutputAmount() { NoOfItems = 1 } },
                                new Output() { EntityTypeToCreate = "item:metalWire", Amount = new OutputAmount() { NoOfItems = 1 } }
                                 
                    },
                WorkOrTimeNeeded = new WorkOrTime() { DaysNeeded = 0.7f * AllGameData.ProcessLoader.timeForSmallCraftingTask },
                AgentActionState = AnimAction.Salvaging,
                AgentAnimationStates = null,

            });
                       

            // removed this recipe to simplify event triggers in tut
            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeBakedCommonOilTubers",
                DeleteRecord = true
            });




            return listOfProcessTypes;
        }
       
    }
}
