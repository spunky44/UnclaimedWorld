using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.Interface.HUD_Windows;
using GameStateManagement;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Entities;


namespace UWGame.SimSide.InGameEvents.Actions
{
    /// <summary>
    /// lose the game if this action is executed
    /// </summary>
    public class LoseGameAction : EventActionType, IGameData
    {
        /// <summary>
        /// this can be null. then the modal dialog will be skipped
        /// </summary>
        public DynamicText ModalDialogText;
        public string ModalDialogImage;

        public string LoseScreenText;


        /// <summary>
        /// we need a reference for when Sim and Client dissappear
        /// </summary>
        Controller screenManager;

        
         public LoseGameAction(string keyName): base(keyName)
        {

        }

         public LoseGameAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public void Execute(EventAction eventAction, ref string failReason)
        {
            screenManager = The.Sim.Controller;

            The.Client.PauseGame();
            The.Sim.IsGameOver = true;

            if (The.Client != null) // don't show dialogs and don't pause in headless mode
            {
                if (ModalDialogText != null && !string.IsNullOrEmpty(ModalDialogText.Text))
                {

                    // show a dialog first if required:
                    EventDialog dlg = EventActionDialog.ShowAndSaveEventDialog(ModalDialogImage, "", ModalDialogText.GetSubstitutedText(action), true);  // "DEFEAT" MP: right now, I don't know what the header should say

                    dlg.Window.Close += new WindowSystem.CloseHandler(DisplayWindow_Close);
                                       
                }
                else
                {
                    GoToLoseGameScreen();
                }
            }
            else
            {
                GoToLoseGameScreen(); // where to end in headless mode???
            }

            return true;
        }

        void DisplayWindow_Close(WindowSystem.UIComponent sender)
        {
            GoToLoseGameScreen();   
        }

        private void GoToLoseGameScreen()
        {
            // this makes Sim and Client dissappear:
            LoadingScreen.StartTransition(screenManager, LoadLoseGameScreen, false); // true);

           // The.Client.LoadLoseGameScreen();
        }


        /// <summary>
        /// Loading screen callback for activating the gameplay screen.
        /// </summary>
        void LoadLoseGameScreen(object sender, EventArgs e)
        {
           // The.Client.LoadLoseGameScreen();

            LoadLoseGameScreen();
        }

        
        private void LoadLoseGameScreen()
        {

            foreach (GameScreen screen in screenManager.GetScreens())
                screen.ExitScreen();

            screenManager.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
            screenManager.AddScreen(new LoseGameScreen(screenManager, LoseScreenText));
        }
       
    }
}
