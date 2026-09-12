using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace UWGame
{
   
    /// <summary>
    /// let's keep the file path related stuff here...
    /// </summary>
    public static class Config
    {
        public enum DataType { BaseData, RGScenario, RGMap, UserScenarios, UserMods, UserMaps, UserSettings, Replays, SaveGames }

       // public static string ReplayFolderPath;
        public const string DocumentsFolderName = "Unclaimed World";

        #region Recorded games

        public const string GameParamsFileName = "GameParams.xml";
        public const string commandFileName = "Commands.xml";
        public const string replayFileName = "Replay.UWRep"; // should become "Verification.UWRep"
        public const string randomGetsFileName = "RandomCalls.UWRepRand";
        public const string AIStatesFileName = "AIStates.UWRepStates";

        #endregion

        public const string MapDataName = "MapData.xml";

        private const string dataFolder = "data/BaseData";
        private const string scenarioFolder = "data/Scenarios";
        public const string mapsFolder = "data/Maps";

        private const string userModsFolder = "user/Mods";
        private const string userScenarioFolder = "user/Scenarios";
        private const string userMapsFolder = "user/Maps";

        private const string replaysFolder = "Replays";

        private const string saveGamesFolder = "SaveGames"; 

        /// <summary>
        /// changing this will not display all characters correctly when ToUpper gets called.
        /// ToUpper uses the current culture.
        /// The canonical example is Turkey, where the upper case of "i" isn't "I"."
        /// </summary>
        public static System.Globalization.CultureInfo Culture = CultureInfo.InvariantCulture;
       


        static Config()
        {
             /*ReplayFolderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                DocumentsFolderName, 
                "Replays");*/
        }


        /// <summary>
        /// get a path to files/folders in both the vanilla game data and the user data - these are located under bin in the output, or in the user's game dir
        /// 
        /// the folder and file names are optional
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="fileName"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static string GetDataFolderPath(DataType dataType, string folderName = "", string fileName = "")
        {
            string baseFolder;
            switch (dataType)
            {
                case DataType.BaseData:
                    baseFolder = dataFolder;
                    break;
                case DataType.RGScenario:
                    baseFolder = scenarioFolder;
                    break;
                case DataType.RGMap:
                    baseFolder = mapsFolder;
                    break;
                case DataType.UserMaps:
                    baseFolder = userMapsFolder;
                    break;
                case DataType.UserScenarios:
                    baseFolder = userScenarioFolder;
                    break;
                case DataType.UserMods:
                    baseFolder = userModsFolder;
                    break;
                case DataType.UserSettings:
                    return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DocumentsFolderName, folderName, fileName);
                case DataType.Replays:
                    return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DocumentsFolderName, replaysFolder, folderName, fileName);
                case DataType.SaveGames:
                    return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DocumentsFolderName, saveGamesFolder, folderName, fileName);
   
                default:
                    baseFolder = dataFolder;
                    break;
            }

            string newPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), baseFolder, folderName, fileName);
           
            return newPath;
        }

        /// <summary>
        /// get a path to the documents folder on the user's pc (perhpas we can replace this with cloud storage instead)
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="folderName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetDocumentsFolderPath(string folderName = "", string fileName = "")
        {
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
              DocumentsFolderName, folderName, fileName);
            

        }
    }
}
