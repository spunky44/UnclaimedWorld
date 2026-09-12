using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data
{
    public class ProcessLoader
    {

        public static List<Processes.ProcessType> Init()
        {
            List<ProcessType> listOfProcessTypes = new List<ProcessType>();

           
            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeImprovisedCookingPot",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "salvageSkimmerHull",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "salvageSkimmerTail",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "salvageSkimmerEngineSide",
                DeleteRecord = true
            });
            
            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "salvageSkimmerEngineTop",
                DeleteRecord = true
            });


                
            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeVarmintBomb",
                DeleteRecord = true
            });
            /*
            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "constructClayGranary",
                DeleteRecord = true
            });*/

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeLandMine",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeIronSpear",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeSteelKnife",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeIronArrow",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeIronHoe",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeBrickMold",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeBellows",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeHammer",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeSulfurPowder",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeBlackPowderRifleAmmo",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeCharcoal",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeVat",
                DeleteRecord = true
            });


            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "buildRopeBridge", // no metal wire on twinkler island, only Vine
                DeleteRecord = true
            });

         /*   listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "constructImprovisedSmithy",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "constructKiln",
                DeleteRecord = true
            });*/

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeStoneHammer",
                DeleteRecord = true
            });

         /*   listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "constructclayHut",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "constructCaneHut",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "constructMudbrickWorkbench",
                DeleteRecord = true
            });

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "constructMudbrickKitchen",
                DeleteRecord = true
            });*/
            //

            return listOfProcessTypes;
        }
    }
}
