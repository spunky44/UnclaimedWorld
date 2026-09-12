using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Scenarios;

namespace UWGame.SimSide.AllGameData.Scenarios
{
    /// <summary>
    /// this class manages save/load of scenarios, both RG and user made
    /// </summary>
    public class AllScenarioLoader
    {

         /// <summary>
        /// gets the list of scenario descriptions
        /// </summary>
        /// <returns></returns>
        public static List<SimSide.Scenarios.Scenario> LoadAllScenarioHeaders()
        {
            List<SimSide.Scenarios.Scenario> rgScenarios = RGScenarioLoader.LoadAllScenarioHeaders();
            List<SimSide.Scenarios.Scenario> userScenarios = LoadUserScenarioHeaders();

            rgScenarios.AddRange(userScenarios);


            return rgScenarios;
        }

        private static List<SimSide.Scenarios.Scenario> LoadUserScenarioHeaders()
        {
            List<SimSide.Scenarios.Scenario> userScenarios = new List<Scenario>();


            // look in the file folder to get user scenarios:
            string userPath = Config.GetDataFolderPath(Config.DataType.UserScenarios);

            // each scenario is in a folder, but we need the header files, not just the folder names:
            string[] mapFolders = System.IO.Directory.GetDirectories(userPath, "*", System.IO.SearchOption.TopDirectoryOnly);
            
            foreach (string folder in mapFolders) // mapFolders)
            {
                Scenario scenarioHeader;
                BaseDataLoader.DeserializeObject(folder, ScenarioLoader.scenarioHeaderFileName, out scenarioHeader, Config.DataType.UserMaps);

                userScenarios.Add(scenarioHeader);
            }


            return userScenarios;
        }

        public static Scenario LoadScenarioHeader(string name, Source source) // SimSide.Scenarios.Scenario scenario)
        {
            if (source == Source.RefactoredGames)
            {
                return RGScenarioLoader.LoadScenarioHeader(name);
            }
            else
            {
                // read from disk:               
                // TODO!!! for replays!!
               // BaseDataLoader.DeserializeObject(scenario.Name, ScenarioLoader.ScenarioDataFileName, out scenarioData, Config.DataType.UserScenarios);
                return null;
            }
        }

        public static ScenarioData LoadScenarioData(SimSide.Scenarios.Scenario scenario)
        {
           
            ScenarioData scenarioData;
            if (scenario.Source == Source.RefactoredGames)
            {
                scenarioData = RGScenarioLoader.LoadScenarioData(scenario);
            }
            else
            {
                // read from disk:               
                BaseDataLoader.DeserializeObject(scenario.Name, ScenarioLoader.ScenarioDataFileName, out scenarioData, Config.DataType.UserScenarios);
                                
            }

            scenarioData.Initialize();

            // validate the data:
            Dictionary<string, List<string>> allPostInitValidationErrors = new Dictionary<string,List<string>>();
            List<string> listOfErrors = new List<string>();
            allPostInitValidationErrors.Add(scenario.Name, listOfErrors);

            scenarioData.PostInitValidate(listOfErrors);

            BaseDataLoader.DisplayAllValidationErrors(allPostInitValidationErrors);

           // scenarioData.LoadContent(The.Client.Content); 

            return scenarioData;

        }


        public static DataLoader GetScenarioDataLoader(SimSide.Scenarios.Scenario scenario)
        {
            if (scenario.Source == Source.RefactoredGames)
            {
                ScenarioLoader scenarioLoader = RGScenarioLoader.GetScenarioLoader(scenario);

                return scenarioLoader.GetDataLoader();
            }
            else
            {
                // user scenarios - only the file folder exists   
                // create an instance of DataLoader that can only deserialize?

                return new UserDataLoader();

              //  BaseDataLoader.DeserializeObject(scenario.Name, ScenarioLoader.ScenarioDataFileName, out scenarioData, Config.DataType.UserMaps);

            }

        }

    }
}
