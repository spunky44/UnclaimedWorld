using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UWGame.Control;
using UWGame.SimSide;
using Microsoft.Xna.Framework;
using UWGame.Control.Commands;
using System.Xml.Serialization;
using UWGame.ClientSide.Screens;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.SimSide.Scenarios;
using System.Xml;
using System.Text.RegularExpressions;

namespace UWGame.Control.Replays
{
    public class ReplayData
    {
        List<ReplayFrame> frames;

        private int startIndex = -1;
       
        public int currentFrameIndex;
        public int? randomSeed;
        public int screenHeight;
        public int screenWidth;
       // public PlaceGameEntities.DebugScenarios scenario;

        public StartGameParams StartGameParams;


        private int commandIndex = 0;
        private List<Command> commandsToReplay = new List<Command>();

        public string GameVersion;

        private float? TimeLeftToPause;

        private Controller controller;

        public ReplayData(Controller controller)
        {
            this.controller = controller;
            currentFrameIndex = startIndex;
        }


        public double GetStartingSeconds()
        {
            return frames[0].GameTime.TotalGameTime.TotalSeconds;
        }

        private void serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Console.WriteLine
            ("UnknownNode Name: {0}", e.Name);
            Console.WriteLine
            ("UnknownNode LocalName: {0}", e.LocalName);
            Console.WriteLine
            ("UnknownNode Namespace URI: {0}", e.NamespaceURI);
            Console.WriteLine
            ("UnknownNode Text: {0}", e.Text);

            XmlNodeType myNodeType = e.NodeType;
            Console.WriteLine("NodeType: {0}", myNodeType);

          /*  Group myGroup = (Group)e.ObjectBeingDeserialized;
            Console.WriteLine("GroupName: {0}", myGroup.GroupName);
            Console.WriteLine();*/
        }

        public void LoadReplay(string replayFolderPath, string replayFilePath, string commandFilePath, string gameParamsPath, float? timeToPause)
        {
            XmlSerializer s = new XmlSerializer(typeof(List<Command>));


            s.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            

            using (TextReader r = new StreamReader(commandFilePath))
            {
                commandsToReplay = (List<Command>)s.Deserialize(r);
            }

            s.UnknownNode -= new XmlNodeEventHandler(serializer_UnknownNode);
   

            frames = new List<ReplayFrame>();
          
            // load scenario start options too
            BaseDataLoader.DeserializeObject(gameParamsPath, out StartGameParams);
                      
            if (StartGameParams.StartScenarioParams != null)
            {
                // load the data:   
                StartGameParams.StartScenarioParams.LoadScenarioFromName();
                /*scenarioParams.Scenario = AllScenarioLoader.LoadScenarioHeader(scenarioParams.ScenarioName, scenarioParams.Source);
                scenarioParams.Scenario.ScenarioData = AllScenarioLoader.LoadScenarioData(scenarioParams.Scenario);*/
            }

           // scenario = (PlaceGameEntities.DebugScenarios)replayFileReader.ReadInt();

            BinaryFileReader replayFileReader = new BinaryFileReader(replayFilePath);

            screenHeight = replayFileReader.ReadInt();
            screenWidth = replayFileReader.ReadInt();

            int seed = replayFileReader.ReadInt();
            if (seed == 0)
            {
                randomSeed = null;
            }
            else
            {
                randomSeed = seed;
            }

            int major = replayFileReader.ReadInt();
            int minor = replayFileReader.ReadInt();
            int build = replayFileReader.ReadInt();
            int revision = replayFileReader.ReadInt();

            GameVersion = major + "." + minor + "." + build + "." + revision;

            while (replayFileReader.HasReachedEndOfFile() == false)
            {
                ReplayFrame newReplayFrame = new ReplayFrame();
                frames.Add(newReplayFrame);

                newReplayFrame.LoadFrame(replayFileReader);
            }
            //ReplayFrame newReplayFrame = (ReplayFrame)replayFileReader.Deserialize();

            replayFileReader.Close();
            TimeLeftToPause = timeToPause;
        }

        public bool Update(UWGame.Control.Replays.Replayer.ReplayingMode mode)
        {
           // currentFrameIndex++;

            if (mode != Replayer.ReplayingMode.InterfaceMode)
            {
                while (true)
                {
                    if (commandIndex >= commandsToReplay.Count)
                    {
                        break;
                    }

                    Command command = commandsToReplay[commandIndex];
                    if (command.frameCalled == currentFrameIndex)
                    {
                        command.Execute(false); // don't try to update the GUI panels during replay - they may not be shown or initialized.
                        commandIndex++;

#if DEBUG
                        Console.WriteLine(currentFrameIndex + " REPLAYED: " +  command.ToString());
#endif
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (TimeLeftToPause.HasValue)
            {
                TimeLeftToPause -= (float)frames[currentFrameIndex].GameTime.ElapsedGameTime.TotalSeconds;
                if (TimeLeftToPause < 0.0f)
                {
                    TimeLeftToPause = null;
                    controller.PauseReplay();
                }
            }
                       
            

          /*  currentFrameIndex++;

            if (ReplayEnded())
            {
                return false;
            }*/
            
            return true;
        }

        public bool ReplayEnded()
        {
            if (currentFrameIndex < frames.Count)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public ReplayFrame GetCurrentFrame()
        {
            return frames[currentFrameIndex];
        }

        public void Reset()
        {
            currentFrameIndex = startIndex;
            commandIndex = 0;
        }
    }
}
