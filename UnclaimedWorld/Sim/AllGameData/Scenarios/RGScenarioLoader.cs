using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.InGameEvents.Actions;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
//using UWGame.SimSide.AllGameData.Scenarios.Scenario_1;

namespace UWGame.SimSide.AllGameData.Scenarios
{
    /// <summary>
    /// manages Refactored Games (RG) (vanilla) scenarios
    /// unlike basic game data, scenarios are only loaded when needed. When in NoSerialze mode, they are just instantiated.
    /// </summary>
    public class RGScenarioLoader
    {
        /// <summary>
        /// the loader class is just a shell, it does not contain the data
        /// </summary>
        private static List<ScenarioLoader> rgScenarioLoaders;

        /// <summary>
        /// static ctor
        /// </summary>
        static RGScenarioLoader()
        {
            rgScenarioLoaders = new List<ScenarioLoader>();
 //         rgScenarioLoaders.Add(new Scenarios.Scenario_1.Scenario1Loader());

            // add the other RG scenarios to the list here:

            rgScenarioLoaders.Add(new Scenarios.Scenario_1.Scenario1Loader());

            rgScenarioLoaders.Add(new Scenarios.Scenario_2.Scenario2Loader());

            rgScenarioLoaders.Add(new Scenarios.Scenario_3.Scenario3Loader());

            rgScenarioLoaders.Add(new Scenarios.Scenario_4.Scenario4Loader());

            rgScenarioLoaders.Add(new Scenarios.Scenario_5.Scenario5Loader());

            rgScenarioLoaders.Add(new Scenarios.Scenario_4x.Scenario4xLoader());

            rgScenarioLoaders.Add(new Scenarios.Scenario_6.Scenario6Loader());

            rgScenarioLoaders.Add(new Scenarios.Scenario_4y.Scenario4yLoader());

            rgScenarioLoaders.Add(new Scenarios.Scenario_7.Scenario7Loader());

        }


        /// <summary>
        /// serializes all vanilla scenarios
        /// </summary>
        public static void Serialize()
        {
            foreach (var item in rgScenarioLoaders)
            {
                item.WriteScenario();
            }
        }


        /// <summary>
        /// gets the list of scenario descriptions/headers - not the data
        /// </summary>
        /// <returns></returns>
        public static List<SimSide.Scenarios.Scenario> LoadAllScenarioHeaders()
        {
            List<SimSide.Scenarios.Scenario> headers = new List<Scenario>();

            foreach (var item in rgScenarioLoaders)
            {
               
                Scenario scenario = item.GetScenarioHeader();
                        
                // exclude scenarios still unfit for release:
#if RELEASE
                if (scenario.IsInDevelopment)
                    continue;
#endif

                headers.Add(scenario);    
            }         

            return headers;

        }

        public static SimSide.Scenarios.Scenario LoadScenarioHeader(string name)
        {
            // this throws if a save file refers to a scenario that no longer exists. In release, the exception will be ignored.
            // in Debug, press F5 to ignore. And clean up the outdated save files in the folder
            ScenarioLoader loader = rgScenarioLoaders.FirstOrDefault(l => l.FolderName == name);
            return loader.GetScenarioHeader();           

        }

        public static ScenarioLoader GetScenarioLoader(Scenario scenario)
        {
            return rgScenarioLoaders.FirstOrDefault(l => l.FolderName == scenario.Name);
        }

        public static ScenarioData LoadScenarioData(Scenario scenario)
        {

            ScenarioLoader loader = GetScenarioLoader(scenario);

            if (loader != null)
            {
                return loader.GetScenarioData();
            }


            return null; // should never happen...
        }
        
    }
}
