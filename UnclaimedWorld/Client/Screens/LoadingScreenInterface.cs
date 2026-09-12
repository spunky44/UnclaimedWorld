using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using InputEventSystem;
using UWGame.ClientSide.Interface;
using GameStateManagement;

namespace UWGame.ClientSide.Screens
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Control;
    using Microsoft.Xna.Framework;
    using WindowSystem;
    using UWGame.ClientSide.Interface.HUD_Windows;
    using UWGame.SimSide.Scenarios;
    using UWGame.ClientSide.Screens.Loading;
    using UWGame.SimSide.AllGameData;
    using UWGame.SimSide;
    
    public class LoadingScreenInterface : CommonInterface
    {

        int loadPanelWidth = 600; //730;
        int totalHeight = 800;

        
        int left, top;

        EventDialog dialog;

        public LoadingScreen loadingScreen;

        private StartGameParams startGameParams;


        HintPanel hintPanel;

        public LoadingScreenInterface(LoadingScreen screen, StartGameParams startGameParams,  UnclaimedWorld game)
            : base(game)
        {

            this.loadingScreen = screen;
            this.startGameParams = startGameParams;

            game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
            /*
            if (game.GraphicsDeviceManager.PreferredBackBufferWidth < loadPanelWidth)
            {
                throw new Exception("Screen resolution is too low.");
            }
            */

            // center the panels:
            left = (game.Controller.DrawArea.Width - loadPanelWidth) / 2;
            top = (game.Controller.DrawArea.Height - totalHeight) / 2;

            hintPanel = new HintPanel(this);

            hintPanel.ShowInScreenSpace((gui.ScreenWidth - hintPanel.DisplayWindow.Width) / 2, gui.ScreenHeight - hintPanel.DisplayWindow.Height - 80);

            
        }

      //  public static string displayText = "JOURNAL ENTRY #1 \nDATE: 03-10 2238 \nLOCATION: 43 22.5N 124 17.7W \nRECORDED BY: Ward Conlan. ALSO PRESENT: Joaquin Lehner, Augustine Yeboah and Ilya Khan \n \nWe have no way of contacting the other mission members as long as our satellite transmitter is defective. Until connection is back we will keep a locally stored journal. This is the first entry. \n  \nWe have escaped the catastrophic attack by quadites that happened app. 2 hours ago. Only minor injuries are reported. Our vehicle however, is non-functional after damage sustained in the attack and a subsequent crash-landing. \n  \nWe've landed in a firegrass biome. Geographical data are incomplete, and this biome has only been partially documented during the previous months of research. Still, we know enough about this environment to expect species of quadites. \n \nWe only hope not to see the vicious swarmer quadite that attacked us earlier today.";
      //  public static string displayImage = "FleeingSkimmer"; // for now, the image has to be in the CRT Content folder (rebuild the GUI_CRT_Sprites.xml spritesheet)

        public override void LoadContent()
        {
            base.LoadContent();
                   
            InitDialog();

            SetInterfaceCursor();

            LoadHints();

            SelectHint();
        }

        private void LoadHints()
        {
            GameData.Instance.AllHints = DataLoader.HandleOtherDataList(BaseDataLoader.InitHints,
                         // GameData.Instance.AllHints,
                          "hints.xml");

        }

       

        private void SelectHint()
        {
            hintPanel.ShowHint(loadingScreen.Controller);    
        }

        private void InitDialog()
        {
            string loadingDialogText = null;
            string loadingHeadingText = null;
            string imageName = null;
            
            if (startGameParams != null && startGameParams.StartScenarioParams != null)
            {
                loadingDialogText = startGameParams.StartScenarioParams.GetLoadingDialogText();
                loadingHeadingText = startGameParams.StartScenarioParams.Scenario.ScenarioData.LoadingDialogHeading;
                imageName = startGameParams.StartScenarioParams.Scenario.ScenarioData.LoadingDialogImage;
            }

            if (loadingDialogText != null)
            {
                dialog = new EventDialog(this);


                dialog.ShowImageAndText(imageName, loadingHeadingText, loadingDialogText, 
                        mode: EventDialog.Mode.OtherDialog); 

                dialog.ShowInScreenSpace(200, 200);
                dialog.Window.CenterWindow();

                // dialog.ShowCloseButton = false;

                dialog.Window.Close += new CloseHandler(Form_Close);

                loadingScreen.WaitForUser = true;
            }
        }

        void Form_Close(UIComponent sender)
        {
            loadingScreen.OKToStartGame();
        }

      

        void loadPanel_SaveOrLoadClick(object sender, EventArgs e)
        {
          //  loseGameScreen.StartMapEditor(loadPanel.MapDataXmlPath);
        }

        public void PromptUserToContinue()
        {
            if (dialog != null)
            {
                dialog.SetButtonText(0, "CONTINUE");
            }
        }

     /*   public override void Update(GameTime gameTime)
        {
            //input.Update(gameTime);

            

        
        }*/



    }
}


