/*#define REPLAY_GETS
#define REPLAY_STATES*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using UWGame.ClientSide.Screens;
using UWGame.Control.Input;
using GameStateManagement;
using System.Xml.Serialization;
using UWGame.Control.Commands;

namespace UWGame.Control.Replays
{
    public class Replayer
    {
        public ReplayData CurrentReplay;
        private bool isActive = false;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
        }
        public bool IsPlaying = false;

        public enum ReplayingMode
        {
            InterfaceMode, //Only uses recorded mouse and keyboard input.
            CommandMode, //Only uses recorded commands, you can use the key and mouse freely but you cannot do any actions (commands block this)
            HybridMode //Runs both commands and the recorded mouse and keyboard (This is the old default mode that we were using)
        }

        public ReplayingMode Mode = ReplayingMode.InterfaceMode; //ReplayingMode.CommandMode; //ReplayingMode.CommandMode; // ReplayingMode.InterfaceMode; //InterfaceMode; CommandMode;
       
        private List<string> randomGetMessages = new List<string>();
        private InputManager inputManager;
        private UnclaimedWorld game;
        private Controller controller;

        private int currentAIStateIndex = 0;
        private int currentRandomGetIndex = 0;
        private List<string> savedAIStates = new List<string>();
        
        public Replayer(InputManager inputManager, Controller controller,UnclaimedWorld game)
        {
            this.inputManager = inputManager;
            this.game = game;
            this.controller = controller;
        }

        public void Update()
        {
            if (IsPlaying)
            {
                CurrentReplay.Update(Mode);

                
               /* if (CurrentReplay.Update(Mode) == false)
                {
                    EndReplay();
                }*/
            }
        }

        public void AdvanceFrame()
        {
            if (IsPlaying)
            {
                CurrentReplay.currentFrameIndex++;

                if (CurrentReplay.ReplayEnded())
                {
                    EndReplay();
                }
            }
        }

        public void StartReplay()
        {
            inputManager.SetToReplayMode();

            CurrentReplay.currentFrameIndex = 0; // NEWREPLAY

            IsPlaying = true;
        }

        public void EndReplay()
        {
            The.InGameUI.MenuDialog.QuitToMainMenu();
            inputManager.SetToDefaultMode();
            IsPlaying = false;
            isActive = false;
            CurrentReplay.Reset();
        }

        public void Initialize(/*string saveFilePath*/)
        {
            //LoadReplay("ReplayTestFile.txt");
        }

        public void LoadReplay(string replayFolderPath, float? timeToPause)
        {
            string replayFilePath = replayFolderPath + "\\" + Config.replayFileName;
            string commandFilePath = replayFolderPath + "\\" + Config.commandFileName;
            string gameParamsFilePath = replayFolderPath + "\\" + Config.GameParamsFileName;

            string randomGetsFilePath = replayFolderPath + "\\" + Config.randomGetsFileName;
            string savedAIStatesFilePath = replayFolderPath + "\\" + Config.AIStatesFileName;

            #if REPLAY_GETS
            FileStream inStream = File.OpenRead(randomGetsFilePath);
                System.IO.StreamReader replayFileReader = new StreamReader(inStream);

                string currentGetRandomMessage = null;
                while (replayFileReader.EndOfStream == false)
                {
                    currentGetRandomMessage = replayFileReader.ReadLine();
                
                    randomGetMessages.Add(currentGetRandomMessage);
                }

                replayFileReader.Close();
            #endif

            #if REPLAY_STATES
                inStream = File.OpenRead(savedAIStatesFilePath);
                replayFileReader = new StreamReader(inStream);

                string currentSavedAIState = null;
                while (replayFileReader.EndOfStream == false)
                {
                    currentSavedAIState = replayFileReader.ReadLine();

                    savedAIStates.Add(currentSavedAIState);
                }

                replayFileReader.Close();
            #endif             

            CurrentReplay = new ReplayData(controller);
            CurrentReplay.LoadReplay(replayFolderPath, replayFilePath, commandFilePath, gameParamsFilePath, timeToPause);

            isActive = true;

            game.GraphicsDeviceManager.PreferredBackBufferWidth = CurrentReplay.screenWidth;
            game.GraphicsDeviceManager.PreferredBackBufferHeight = CurrentReplay.screenHeight;

            game.GraphicsDeviceManager.ApplyChanges();

           
            LoadingScreen.StartTransitioningToGame(game.Controller,
                CurrentReplay.StartGameParams, 
                true);
          
        }

        public void DummyMethod(object sender, EventArgs e)
        {

        }

        public string GetCurrentSavedAIState()
        {
            #if REPLAY_STATES
                string currentAIState = savedAIStates[currentAIStateIndex];
                currentAIStateIndex++;
                return currentAIState;
            #else
                return null;
            #endif
        }

        public string GetCurrentSavedRandomGet()
        {
            #if REPLAY_GETS
                if (currentRandomGetIndex < randomGetMessages.Count)
                {
                    string currentAIState = randomGetMessages[currentRandomGetIndex];
                    currentRandomGetIndex++;
                    return currentAIState;
                }
                else
                {
                    return null;
                }
            #else
                return null;
            #endif
        }
    }
}

