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
using WindowSystem;
using Steamworks;
using UWGame.Steam;
using UWGame.SimSide.Allegiances.Statistics;

namespace UWGame.SimSide.InGameEvents.Actions
{
    /// <summary>
    /// the game is won if this action is executed
    /// </summary>
    public class WinGameAction: EventActionType, IGameData
    {
        /// <summary>
        /// this can be null. then the modal dialog will be skipped
        /// </summary>
        public DynamicText ModalDialogText;
        public string ModalDialogImage;


        public string WinScreenText;
               
        public string ContinueGameTooltip;
        public string EndGameTooltip;

        public bool AllowContinueGame = true;

      //  public string WinAchievement;

        /// <summary>
        /// fire these when the user chooses "Continue playing"
        /// </summary>
        public ActionSets ContinueActions;

        /// <summary>
        /// fire these when the user chooses to end the game - no delays are allowed and talk actions won't work...
        /// </summary>
        public EventActionType[] EndGameActions;



        /// <summary>
        /// we need a reference for when Sim and Client dissappear
        /// </summary>
        Controller controller;

        
        public WinGameAction(string keyName): base(keyName)
        {

        }

         public WinGameAction()           
        {

        }

        public override bool Execute(EventAction action, ref string failReason) //public void Execute(EventAction eventAction, ref string failReason)
        {
            controller = The.Sim.Controller;
            

            CheckAchievements(); // get the achievement even if choosing to continue


            if (The.Client != null) // don't show dialogs and don't pause in headless mode
            {
                The.Client.PauseGame();
           
                if (ModalDialogText != null && !string.IsNullOrEmpty(ModalDialogText.Text)) 
                {

                   
                    EventDialog dialog = EventActionDialog.ShowAndSaveEventDialog(
                        ModalDialogImage, 
                        "", 
                        ModalDialogText.GetSubstitutedText(action), 
                        true,
                        true,
                        "EXIT SCENARIO",
                        EndGameTooltip, //"You have succeeded: At least one explorer has survived long enough to get rescued. Clicking here will end your current game and take you to the main menu.",
                        null,
                        AllowContinueGame,
                        "CONTINUE PLAYING",
                        ContinueGameTooltip, //"You have succeeded the scenario, but you can keep on playing by selecting this option - you will NOT get rescued again however!",
                        null
                        );

                    dialog.ButtonClicked += new Action<string, int>(dialog_ButtonClicked);

                 /*   dialog.OnOKClicked += new EventHandler(dialog_OnOKClicked);
                    dialog.OnCancelClicked += new EventHandler(dialog_OnCancelClicked);*/
                }
                else
                {
                    GoToWinGameScreen();
                }
            }
            else
            {
                // how does the game end in headless..?
              //  GoToWinGameScreen();
            }

            return true;
        }


      

        void dialog_ButtonClicked(string arg1, int index)
        {
            if (index == 0)
            {
                GoToWinGameScreen();
            }
            else
            {
                ContinuePlaying();
            }
        }

       /* void dialog_OnCancelClicked(object sender, EventArgs e)
        {
            ContinuePlaying();
        }

        void dialog_OnOKClicked(object sender, EventArgs e)
        {
            GoToWinGameScreen();
        }*/

        public override void ExtractNestedActionTypes(ref List<string> duplicateKeyErrors)
        {
            base.ExtractNestedActionTypes(ref duplicateKeyErrors);

            if (ContinueActions != null)
            {
                ContinueActions.ExtractNestedGameData(ref duplicateKeyErrors);
            }

            if (EndGameActions != null)
            {
                foreach (var item in EndGameActions)
                {
                    item.ExtractNestedActionTypes(ref duplicateKeyErrors);
                }
            }
        }

        private void GoToWinGameScreen()
        {
            The.Sim.IsGameOver = true;

            if (EndGameActions != null)
            {
                foreach (var item in EndGameActions)
                {
                    EventAction eventAction = new EventAction(item, null);
                    eventAction.Execute();                    
                }
            }

           /* The.InGameUI.EventDialog.OnOKClicked -= new EventHandler(dialog_OnOKClicked);
            The.InGameUI.EventDialog.OnCancelClicked -= new EventHandler(dialog_OnCancelClicked);
            */

            The.InGameUI.EventDialog.ButtonClicked -= new Action<string, int>(dialog_ButtonClicked);

            LoadingScreen.StartTransition(The.Sim.Controller, LoadWinGameScreen, false);
        }

        private void CheckAchievements()
        {
           // ScenarioWinAchievement winAchievement;
            StatsAndAchievements ach = The.Sim.Controller.StatsAndAchievements;

            if (The.Sim.StartGameParams.StartScenarioParams != null
                && The.Sim.StartGameParams.StartScenarioParams.Scenario.Source == Scenarios.Source.RefactoredGames)
            {
                string scenarioName = The.Sim.StartGameParams.StartScenarioParams.ScenarioName;
               
                if (!ach.IsAchievementUnlocked(AchievementID.tutorialCompleted)
                    && scenarioName == "TUTORIAL - Castaways")
                {
                    ach.UnlockAchievement(AchievementID.tutorialCompleted);
                }

                if (!ach.IsAchievementUnlocked(AchievementID.clayPitCompleted)
                   && scenarioName == "The Clay Pit")
                {
                    ach.UnlockAchievement(AchievementID.clayPitCompleted);
                }

                if (scenarioName == "Making Headway")
                {
                    if (!ach.IsAchievementUnlocked(AchievementID.headwayCompleted))
                    {
                        ach.UnlockAchievement(AchievementID.headwayCompleted);
                    }

                    if (!ach.IsAchievementUnlocked(AchievementID.headwayAlternative))
                    {
                        var produced = The.Sim.PlaySite.PlayerAllegiance.Statistics.ProductionStatistics.Totals[ProductionStatistics.StatTypes.Produced];
                        int total;
                        if (!produced.TryGetValue(GameData.Instance.AllEntityTypes["item:gaskets"], out total) || total == 0)
                        {
                            ach.UnlockAchievement(AchievementID.headwayAlternative);
                        }
                    }

                }

                if (!ach.IsAchievementUnlocked(AchievementID.twinklerIslandNoDeaths)
                  && scenarioName == "Twinkler Island")
                {
                    bool difficultyMembersOK = true;
                     if (The.Sim.StartGameParams.StartScenarioParams.MainDifficultyKey == "normal")// named Hard in display...   
                     {
                         // must have saved 5th member
                         if (The.Sim.PlaySite.PlayerAllegiance.IndependentMembers.Count < 5)
                         {
                             difficultyMembersOK = false;
                         }
                     }
                     else if (The.Sim.StartGameParams.StartScenarioParams.MainDifficultyKey == "easy") // called Normal in display
                     {
                         difficultyMembersOK = true;
                     }
                     else
                     {
                         difficultyMembersOK = false; // can't achieve in Custom
                     }

                    if (difficultyMembersOK)
                    {
                        PopulationStatistics stats = The.Sim.PlaySite.PlayerAllegiance.Statistics.PopulationStatistics;
                        if (stats.IndependentDeaths.Count == 0 && stats.Emigration.Count == 0)
                        {
                            ach.UnlockAchievement(AchievementID.twinklerIslandNoDeaths);
                        }
                    }
                }

                if (!ach.IsAchievementUnlocked(AchievementID.twinklerIslandNoKills)
                 && scenarioName == "Twinkler Island")
                {
                    if (The.Sim.StartGameParams.StartScenarioParams.MainDifficultyKey == "normal" // named Hard in display...           
                        || The.Sim.StartGameParams.StartScenarioParams.MainDifficultyKey == "easy") // called Normal in display
                    {
                        KillStatistics stats = The.Sim.PlaySite.PlayerAllegiance.Statistics.KillStatistics;
                        if (!stats.Kills.Any(kvp => kvp.Value > 0))
                        {
                            ach.UnlockAchievement(AchievementID.twinklerIslandNoKills);
                        }
                    }
                }



                /*
                if (GameData.Instance.ScenarioWinAchievements.TryGetValue(The.Sim.StartGameParams.StartScenarioParams.ScenarioName, out winAchievement))
                {
                    The.Sim.Controller.StatsAndAchievements.UnlockAchievement(winAchievement);              
                }*/
            }
        }


        private void ContinuePlaying()
        {
            if (ContinueActions != null)
            {
                bool expired;
                ContinueActions.Fire(null, null, null, out expired);
            }

            The.InGameUI.EventDialog.ButtonClicked -= new Action<string, int>(dialog_ButtonClicked);
            /*
            The.InGameUI.EventDialog.OnOKClicked -= new EventHandler(dialog_OnOKClicked);
            The.InGameUI.EventDialog.OnCancelClicked -= new EventHandler(dialog_OnCancelClicked);*/
        }

        public void LoadWinGameScreen(object sender, EventArgs e)
        {

            foreach (GameScreen screen in controller.GetScreens())
                screen.ExitScreen();

            controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
            controller.AddScreen(new WinGameScreen(controller, WinScreenText)); //, WinAchievement));
        }
      
    }
}
