/*#define RECORD_GETS
#define RECORD_STATES*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Screens;
using Microsoft.Xna.Framework.Input;
using UWGame.Control;
using UWGame.Control.Input;
using InputEventSystem;
using UWGame.SimSide;
using GameStateManagement;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UWGame.Control.Commands;
using System.Xml.Serialization;
using UWGame.SimSide.Commands;
using System.Xml;
using UWGame.Contol;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Scenarios;

namespace UWGame.Control.Replays
{
    public class Recorder
    {
        public bool isRecording = false;

        private BinaryWriter replayWriter;
        private StreamWriter randomWriter;
        private StreamWriter stateWriter;
        private PositionableStreamWriter commandWriter;
       
        Controller controller;

        public int currentFrameIndex = 0;
        private string closingTag;

        public bool RecordDuringPlay = false;
    
        int savedMessageIndex = 0;

        /// <summary>
        /// don't hold onto input data instances in controller classes. We want input data to be replaced after game ends to release all event handlers!
        /// </summary>
        /// <param name="controller"></param>
        /// 
        public Recorder( Controller controller)
        {
            this.controller = controller;
        }
       
        public void RecordCommand(Command command)
        {
            command.frameCalled = currentFrameIndex;

            commandWriter.SetPositionFromEnd(-closingTag.Length);
            commandWriter.writer.Write(SerializeCommand(command));
            commandWriter.writer.Write(closingTag);
            commandWriter.writer.Flush();

#if DEBUG
            Console.WriteLine(currentFrameIndex + " RECORDED: " + command.ToString());
#endif
        }

        public string SerializeCommand(Command toSerialize)
        {

            List<Command> listWithACommand = new List<Command>();
            listWithACommand.Add(toSerialize);


            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.OmitXmlDeclaration = true;
            xmlSettings.Indent = true;
            


            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Command>));
            StringWriter textWriter = new StringWriter();
            XmlWriter serializeWriter = XmlWriter.Create(textWriter, xmlSettings);


            // Valideringsfejl for forekomst: '2' er ikke en gyldig værdi for (enum ID type)

            xmlSerializer.Serialize(serializeWriter, listWithACommand);

            // NEW:
           // XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
       
            ////Add an empty namespace and empty value
            //ns.Add("", "");  

            ////xmlSerializer.Serialize(serializeWriter, toSerialize);
            //xmlSerializer.Serialize(serializeWriter, toSerialize, ns);
            //    //dont output xml tag at top
            //    //Replace closing and starting tag
            string text = textWriter.ToString();
            text = text.Substring(text.IndexOf('>') +1);
            text = text.Replace("</ArrayOfCommand>", "");

            text += "\r\n";

            return text;
        }

        /// <summary>
        /// a recording is now several files in a folder
        /// </summary>
        /// <param name="startGameParams"></param>
        private void OpenNewReplayFiles(StartGameParams startGameParams) //PlaceGameEntities.DebugScenarios mapEnum)
        {
            string folderPath = Config.GetDataFolderPath(Config.DataType.Replays);
            if (!Directory.Exists(folderPath)) //Directory.Exists(Config.ReplayFolderPath) == false)
            {
                DirectoryInfo directory = Directory.CreateDirectory(folderPath); //Config.ReplayFolderPath);
            }

            string date = System.DateTime.Now.ToString("yyyyMMdd");
            string hour = System.DateTime.Now.TimeOfDay.Hours.ToString(); 
            string minute = System.DateTime.Now.TimeOfDay.Minutes.ToString();
            string folderName = startGameParams.ToString() + " " + date + " " + hour + " " + minute + " " + UnclaimedWorld.GetVersionAsString();


            string replayFolderPath = Config.GetDataFolderPath(Config.DataType.Replays, folderName);
            //string replayFilePath = Config.ReplayFolderPath + fileName + Config.ReplayFileExtension;
           
          //  string CommandFilePath = Config.ReplayFolderPath + fileName + ".xml"


            //if (File.Exists(replayFilePath))

            if (Directory.Exists(replayFolderPath))
            {
                string tempFolderPath = replayFolderPath;
                int replayFileNumber = 1;
                while (Directory.Exists(tempFolderPath))
                {
                    replayFileNumber++;
                    tempFolderPath = replayFolderPath + replayFileNumber.ToString();
                }

                replayFolderPath = tempFolderPath;
                folderName += replayFileNumber.ToString();
            }

            Directory.CreateDirectory(replayFolderPath);

            string replayFilePath = Config.GetDataFolderPath(Config.DataType.Replays, folderName, Config.replayFileName); 
            string commandFilePath = Config.GetDataFolderPath(Config.DataType.Replays, folderName, Config.commandFileName);

            string randomGetsFilePath = Config.GetDataFolderPath(Config.DataType.Replays, folderName, Config.randomGetsFileName);
            string savedAIStatesFilePath = Config.GetDataFolderPath(Config.DataType.Replays, folderName, Config.AIStatesFileName);


            BaseDataLoader.SerializeObject(startGameParams, folderName, Config.GameParamsFileName, Config.DataType.Replays);
     

            FileStream outStream = File.Create(replayFilePath);
            replayWriter = new System.IO.BinaryWriter(outStream);
            
            commandWriter = new PositionableStreamWriter(commandFilePath);

            closingTag = "</ArrayOfCommand>";
            commandWriter.writer.Write("<ArrayOfCommand xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
            commandWriter.writer.Write(closingTag);
            commandWriter.writer.Flush();
  
            #if RECORD_GETS
                #if !RELEASE
                    outStream = File.Create(randomGetsFilePath);
                    randomWriter = new System.IO.StreamWriter(outStream);
                #endif
            #endif

            #if RECORD_STATES
                #if !RELEASE
                    outStream = File.Create(savedAIStatesFilePath);
                    stateWriter = new System.IO.StreamWriter(outStream);
                #endif
            #endif

        }

        public void SaveStartGameParams(StartGameParams startGameParams) //  PlaceGameEntities.DebugScenarios debugScenario)
        {
            if (RecordDuringPlay)
            {
                OpenNewReplayFiles(startGameParams); //debugScenario);

              //  replayWriter.Write((int)debugScenario);
              //  replayWriter.Flush();
            }
        }

        public void SaveEntityAIState(string entityAIState)
        {
            #if RECORD_STATES
                stateWriter.WriteLine(entityAIState);
                stateWriter.Flush();
            #endif
        }
        public void SaveRandomGet(string getMessage)
        {
            #if RECORD_GETS
                randomWriter.WriteLine(getMessage);
                randomWriter.Flush();
            #endif
        }

        public void Initialize()
        {
            
        }

        public void Update(GameTime gameTime)
        {
            if (isRecording == true)
            {
                SaveFrame(gameTime);

              //  AdvanceFrame();
               
            }
        }

        public void AdvanceFrame()
        {
            if (isRecording)
            {
                currentFrameIndex++;
            }
        }

        public void StartRecording(int? randomSeed)
        {
            int screenHeight = The.Sim.Controller.GraphicsDevice.PresentationParameters.BackBufferHeight;
            int screenWidth = The.Sim.Controller.GraphicsDevice.PresentationParameters.BackBufferWidth;

            isRecording = true;
            currentFrameIndex = 0;

            replayWriter.Write(screenHeight);
            replayWriter.Write(screenWidth);
            replayWriter.Write(randomSeed ?? 0); // zero means null...

            replayWriter.Write(MainMenuInterface.GetMajor());
            replayWriter.Write(MainMenuInterface.GetMinor());
            replayWriter.Write(MainMenuInterface.GetBuild());
            replayWriter.Write(MainMenuInterface.GetRevision());
         
            replayWriter.Flush();
        }


        /// <summary>
        /// let's delete, so there are no more than 15 files...
        /// </summary>
        public void CleanupOldRecordedFiles()
        {
            if (controller.Options.MaxNoOfRecordedGamesToKeep < 0)
                return; // keep all

            string replayFolderPath = Config.GetDataFolderPath(Config.DataType.Replays);

            List<DirectoryInfo> directories;
            List<string> listOfReplayFiles = GetListOfReplayFolders(replayFolderPath, out directories);

            var sortedFolders = directories.OrderByDescending(i => i.CreationTimeUtc);

            int index = 0;
            foreach (var item in sortedFolders)
            {
                if (index >= controller.Options.MaxNoOfRecordedGamesToKeep)
                {
                    try
                    {
                        item.Delete(true);
                       // System.IO.Directory.Delete(item.Delete savegamePath);
                    }
                    catch (Exception ex)
                    {
                        //output.ShowError("Could not delete the file. Message: " + ex.Message);
                    }
                }

                index++;
            }
        }

        public static List<string> GetListOfReplayFolders(string replayFolderPath, out List<DirectoryInfo> directories)
        {            
            List<string> listOfReplayFolders = new List<string>();
            directories = new List<DirectoryInfo>();

            string[] replayFolderPaths = Directory.GetDirectories(replayFolderPath);

            DirectoryInfo info;
            foreach (string replayFolder in replayFolderPaths)
            {               
                info = new DirectoryInfo(replayFolder);
                string folderName = info.Name;
                
                listOfReplayFolders.Add(folderName);
                directories.Add(info);
            }

            return listOfReplayFolders;
        }


        public void StopRecording()
        {
            savedMessageIndex = 0;
            isRecording = false;

            if (replayWriter != null)
            {
                replayWriter.Close();
            }
            if (stateWriter != null)
            {
                stateWriter.Close();
            }
            if (randomWriter != null)
            {
                randomWriter.Close();
            }
            if (commandWriter != null)
            {
                commandWriter.Close();
            }
        }

        /*public void SerializeFrame(GameTime gameTime)
        {
            List<Keys> changedKeys = new List<Keys>();
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (frameInput.IsKeyDown(key) != frameInput.WasKeyDown(key))
                {
                    changedKeys.Add(key);
                }
            }
            ReplayFrame frameToSerialize = new ReplayFrame(gameTime, frameInput.mouseX, frameInput.mouseY, frameInput.LeftButtonDown, frameInput.RightButtonDown, changedKeys);
            
            serializationWriter.Serialize(outStream, frameToSerialize);
        }*/
        public void SaveFrame(GameTime gameTime)
        {
            replayWriter.Write(gameTime.TotalGameTime.Ticks);
            replayWriter.Write(gameTime.ElapsedGameTime.Ticks);

            replayWriter.Write(controller.InputData.mouseX);//Mouse coordinates
            replayWriter.Write(controller.InputData.mouseY);

            replayWriter.Write(controller.InputData.LeftButtonDown);
            replayWriter.Write(controller.InputData.RightButtonDown);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (controller.InputData.IsKeyDown(key) != controller.InputData.WasKeyDown(key))
                {
                    //Save the key that has changed state 
                    replayWriter.Write(true);// write true to indicate that there is information to get
                    replayWriter.Write((int)key);//save key enum value
                }
            }
            replayWriter.Write(false);// write false to indicate that there is no more information to get

            ReplayVerificationData verification = controller.RetrieveVerificationData();

            verification.Write(replayWriter);

            replayWriter.Flush();

#if DEBUG
           // Console.WriteLine(currentFrameIndex + " " + verification.RepresentativeEntityLocation.X.ToString());
#endif
        }
    }
}
