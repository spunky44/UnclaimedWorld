using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data
{
    public class ProcessLoader
    {

        public static List<Processes.ProcessType> Init()
        {
            List<ProcessType> listOfProcessTypes = new List<ProcessType>();

            listOfProcessTypes.Add(new ProcessType()
            {
                KeyName = "makeSulfurSmokeBomb",
                DeleteRecord = true
            });

            return listOfProcessTypes;
        }
    }
}
