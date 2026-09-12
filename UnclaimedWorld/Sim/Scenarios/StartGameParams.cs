using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Scenarios
{

    public class StartGameParams : ISnapshot 
    {
        public StartScenarioParams StartScenarioParams;
        public StartDebugScenarioParams StartDebugScenarioParams;
        public StartGameEditorParams StartGameEditorParams;

        /// <summary>
        /// an optional save game file to restore the Sim from after the scenario has loaded. 
        /// If this is filled, the map data will not be read in from xml and bitmaps, instead save data will be used
        /// </summary>
        public string SavedGameToLoad;
               

        public override string ToString()
        {
            if (StartScenarioParams != null)
                return StartScenarioParams.ToString();

            if (StartDebugScenarioParams != null)
                return StartDebugScenarioParams.ToString();

            if (StartGameEditorParams != null)
                return StartGameEditorParams.ToString();

            return null;
        }

        public bool IsSameScenario(StartGameParams otherParms) 
        {
            if (otherParms.StartScenarioParams != null) 
            {
                if (StartScenarioParams != null)
                {
                    if (StartScenarioParams.ScenarioName == otherParms.StartScenarioParams.ScenarioName
                        && StartScenarioParams.Source == otherParms.StartScenarioParams.Source) 
                    {
                        return true;
                    }
                    else return false;
                }
                else return false;
            }
            else
            {
                // if current scenario is a debug scenario, and other scenario is also, return true.

                if (StartScenarioParams != null)
                {
                    return false;
                }
                else return true;

            }
        }



        public enum RGScenario { FieldsOfTauCeti, MuckrootMiningCamp, TwinklerIsland, MakingHeadway, TheClayPit, None }

        RGScenario? scenario = null;
        public RGScenario GetRGScenario()
        {
            if (scenario == null)
            {
               
                if (StartScenarioParams != null
                    && StartScenarioParams.Scenario.Source == Scenarios.Source.RefactoredGames)
                {

                    string name = StartScenarioParams.ScenarioName;

                    if (name == "Fields of Tau Ceti - Cudgel Hills (L)"
                                    || name == "Fields of Tau Ceti - Cudgel Hills (M)"
                                    || name == "Fields of Tau Ceti - Alluvial Plain")
                    {
                        scenario = RGScenario.FieldsOfTauCeti;
                    }
                    else if (name == "Muckroot Mining Site")
                    {
                        scenario = RGScenario.MuckrootMiningCamp;
                    }
                    else if (name == "Twinkler Island")
                    {
                        scenario = RGScenario.TwinklerIsland;
                    }
                    else if (name == "Making Headway")
                    {
                        scenario = RGScenario.MakingHeadway;
                    }
                    else if (name == "The Clay Pit")
                    {
                        scenario = RGScenario.TheClayPit;
                    }
                    else
                    {
                        scenario = RGScenario.None;
                    }
                }
                else
                {
                    scenario = RGScenario.None;
                }
            }

            return scenario.Value;
        }


#region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.StartScenarioParams = (StartScenarioParams)sn.DoISnapshot(StartScenarioParams);
            this.StartDebugScenarioParams = (StartDebugScenarioParams)sn.DoISnapshot(StartDebugScenarioParams);

            sn.Ignore(SavedGameToLoad);
            sn.Ignore(StartGameEditorParams);
            sn.Ignore(scenario);

            return this;

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (StartScenarioParams != null)
            {
                this.StartScenarioParams.LoadPostProcess(sn);
            }

            if (StartDebugScenarioParams != null)
            {
                this.StartDebugScenarioParams.LoadPostProcess(sn);
            }
        }

#endregion

    }

    public class StartScenarioParams : ISnapshot 
    {
        /// <summary>
        /// we use this to read in GameData from xml, regardless if we are starting a new game, replaying or loading a savegame.
        /// </summary>
        [XmlIgnore]
        public Scenario Scenario;

        private string scenarioName;
        /// <summary>
        /// used together with Source to load the scenario from a replay or a save game file
        /// </summary>
        public string ScenarioName
        {
            get
            {
                return scenarioName;
            }
            set
            {
                scenarioName = value;
            }
        }

        public Source Source;

        /// <summary>
        /// Options will not be used if SaveGame is filled. 
        /// But for the end screen we may still need it for scoring etc.
        /// </summary>
        public SerializableDictionary<string, Option> Options; // we use serializable because we xml serialize it for the replay file!
        Dictionary<string, string> snapshotOptions;


        public string /* Difficulty*/ MainDifficultyKey;


        public override string ToString()
        {
            return ScenarioName;
        }


        /// <summary>
        /// compiled from selected Options
        /// </summary>
        /// <returns></returns>
        public string GetLoadingDialogText()
        {
            string text = Scenario.ScenarioData.LoadingDialogText ?? "";

            // overrides/additions from options

            // first order the options as specified:
            List<Option> options = Options.Values.ToList();
            var orderedOptions = options.OrderBy(o => o.LoadingDialogTextOrder ?? 0);
              
              //  OptionSets = ordered.ToArray();
            
            foreach (var option in orderedOptions)
            {
                if (option.LoadingDialogText != null)
                {
                    // append/override as defined:
                    if (option.LoadingDialogTextMode == Option.LoadingDialogTextModes.Append)
                    {
                        text += option.LoadingDialogText;
                    }
                    else
                    {
                        text = option.LoadingDialogText;
                    }

                    text += "\n\n";//extra whitespace signals end of reading, also makes it easier to scroll to end
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            return text;

        }

        /// <summary>
        /// retrieves the scenario data from the data folders from the name and a source flag. not used when starting fresh..?
        /// </summary>
        public void LoadScenarioFromName()
        {
            // load the data:            
            Scenario = AllScenarioLoader.LoadScenarioHeader(ScenarioName, Source);
            Scenario.ScenarioData = AllScenarioLoader.LoadScenarioData(Scenario);

            // Main menu hack... we don't need the scenario content when displaying the load/save dialog...
            if (The.Client != null)
            {
                Scenario.ScenarioData.LoadContent(The.Client.Content);
            }
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.ScenarioName = sn.DoString(ScenarioName);
            this.Source = (Scenarios.Source)sn.DoEnum(Source);


            //MainDifficultyKey was added in version 2 - v. 1.0.1.1
            if ((uint)version >= 2)
            {
                this.MainDifficultyKey = sn.DoString(MainDifficultyKey);
            }
            else
            {
                this.MainDifficultyKey = null; //or just null, probably
            }
           

            if (sn.mode != Snapshotter.Mode.Load)
            {
                snapshotOptions = Options.ToDictionary(k => k.Key, k => k.Value.KeyName);
            }

            this.snapshotOptions = sn.DoDictionary(snapshotOptions);

            sn.Ignore(Scenario);
            sn.Ignore(Options);

            return this;

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion((Snapshotter.Version)2); // 1.0.1.0 // Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

           // Scenario = AllScenarioLoader.LoadScenarioHeader(ScenarioName, Source);
            LoadScenarioFromName();

            Options = new SerializableDictionary<string,Option>();
            foreach (var kvp in snapshotOptions)
            {
                // crashes here if the save game is outdated. Just click 'Continue'. There will be an exception for every outdated save file in the folder.
                // To avoid this, either:
                // 1. Delete outdated save files from the save folder
                // 2. Go to Debug -> Exceptions and uncheck "Common Language Runtime Exception (Thrown)"
                OptionSet optionSet = Scenario.ScenarioData.OptionSets.First(s => s.KeyName == kvp.Key); 

                Option option = optionSet.Options.First(o => o.KeyName == kvp.Value);

                Options.Add(kvp.Key, option);
             
            }            
        }

        #endregion
    }

    public class StartDebugScenarioParams : ISnapshot  //: StartGameParams
    {
        public PlaceGameEntities.DebugScenarios ScenarioKey;

        public string MapKey;

        public override string ToString()
        {
            return ScenarioKey.ToString();
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            ScenarioKey = sn.DoEnum(ScenarioKey);
            MapKey = sn.DoString(MapKey);

            return this;

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);


        }

        #endregion
    }

    public class StartGameEditorParams //: StartGameParams
    {
       // public string MapToLoad;
        /// <summary>
        /// we use a string because we XML serialize this
        /// </summary>
        public string MapToLoadPath; // DirectoryInfo MapToLoad;

        public Sim.EngineMode EngineMode;

        public override string ToString()
        {
            return System.IO.Path.GetDirectoryName(MapToLoadPath); //.Name;
        }

    }
}
