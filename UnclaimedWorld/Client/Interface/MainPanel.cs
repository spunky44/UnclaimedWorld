using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using UWGame.ClientSide.Interface;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// this panel is placed at the top left of the screen and has a menu button on it, as well as a pause and a help button
    /// </summary>
    public class MainPanel
    {
        public Window DisplayWindow;
        protected InGameInterface intf = The.InGameUI;

        const int height = 52;

        //const int x = 760;

        public ImageButton btHelp;

        public TextButton tbMain; 
        public TextButton btPause;

        public TextButton btNormalSpeed;
        public TextButton bt2GameSpeed;
        public TextButton bt4GameSpeed;

        RadioGroup speedGroup;

        /// <summary>
        /// this is drawn in the draw loop...
        /// </summary>
        Rectangle pauseIcon;
        Rectangle pausePosition;

        const int buttonHeight = 32;

        
        public MainPanel()
        {
            int width;
            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
                width = 250; // 185;
            }
            else
            {
                width = 160;
            }

            DisplayWindow = new Window(intf.gui);
          //  window.Position = new Point(The.Client.GraphicsDevice.Viewport.Width - width, The.Client.GraphicsDevice.Viewport.Height - height);
            DisplayWindow.Position = new Point(0, 0);
            DisplayWindow.WindowSize = new Vector2(width, height); //Interface.Instance.mainPanelHeight);
            DisplayWindow.Level = Level.Bottom;

            DisplayWindow.IsMovable = false;
            DisplayWindow.Resizable = false;
            DisplayWindow.Margin = 0;
            DisplayWindow.HasCloseButton = false;

            DisplayWindow.Skin = intf.gui.GUISpriteSheet.GetSourceRectangle("optionspanel");
            DisplayWindow.CornerSize = 52;

            DisplayWindow.Show(); //Make it visible


            /*   RadioGroup buttonGroup = new RadioGroup(intf.gui); // radio group screws up OnMouseOut!!!
               window.Add(buttonGroup);
               buttonGroup.Width = window.Width;
               buttonGroup.Height = window.Height;
               */
            int buttonLeft = 22;
            int buttonTop = 6;

           
            tbMain = AddBlackTextButton(buttonLeft, buttonTop, "MENU", "Show the game menu");
            tbMain.ScaleWidthToFitText();
            tbMain.Click += new ClickHandler(main_Click);
            tbMain.DebugTag = "tbMain";

            if (The.Sim.Mode == Sim.EngineMode.Game)
            {
               /* tbTutorial = AddLightBoxButton(tbMain.Right + 6, buttonTop, "TUTORIAL", 3);
                tbTutorial.Click += new ClickHandler(tbTutorial_Click);*/

                btHelp = new ImageButton(intf.gui);
                DisplayWindow.Add(btHelp);
                btHelp.InitWithIcon(ImageButtonType.Black, "help_button_icon", true);
                btHelp.X = tbMain.Right + 5;
                btHelp.Y = buttonTop;
                btHelp.ToolTip = "Show various help topics";
                btHelp.Click += new ClickHandler(tbHelp_Click);
                btHelp.Height = buttonHeight;

                speedGroup = new RadioGroup(The.InGameUI.gui);
                DisplayWindow.Add(speedGroup);
                speedGroup.X = btHelp.Right + 4;
                speedGroup.Y = buttonTop;
                speedGroup.Height = buttonHeight;
                speedGroup.Width = 128;
                speedGroup.NewMemberChecked += speedGroup_NewMemberChecked;
                int spacing = -3;
                               
                btPause = AddSpeedButton(speedGroup, 0, Speeds.Pause, "II", "Pause the game");        
                btNormalSpeed = AddSpeedButton(speedGroup, btPause.Right + spacing, Speeds.Normal, "1X", "Set to normal game speed.");
                bt2GameSpeed = AddSpeedButton(speedGroup, btNormalSpeed.Right + spacing, Speeds.TwiceNormal, "2X", "Set to 2 times the normal game speed.");
                bt4GameSpeed = AddSpeedButton(speedGroup, bt2GameSpeed.Right + spacing, Speeds.FourTimesNormal, "4X", "Set to 4 times the normal game speed.");
          


                // Sprite icon:
                pausePosition = new Rectangle();
                pauseIcon = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("HUD_pause_big");
                pausePosition.Width = pauseIcon.Width;
                pausePosition.Height = pauseIcon.Height;
                pausePosition.X = btPause.Right - 12;
                pausePosition.Y = btPause.Bottom + 25;


                SetSpeedButton(The.Sim.Speed); // fill 

            }

        }

        class SpeedArg: EventArgs
        {
            public Speeds Speed;
        }

        void speedGroup_NewMemberChecked(ICanBeChecked arg1, EventArgs arg2)
        {
            Speeds speed = ((SpeedArg)((UIComponent)arg1).EventArgs).Speed;
            switch (speed)
            {
                case Speeds.Pause:
                    if (The.Sim.IsPaused == false)
                    {
                        The.Client.PauseGame();
                    }
                    break;

                case Speeds.Normal: 
                case Speeds.TwiceNormal:
                case Speeds.FourTimesNormal:
                    if (The.Sim.IsPaused)
                    {
                        The.Client.ResumeGame();
                    }

                    The.Client.SetGameSpeed(speed);
                    break;               

            }
        }

       /*  void btNormalSpeed_Click(UIComponent sender, EventArgs e)
        {
            
        }

        void bt2GameSpeed_Click(UIComponent sender, EventArgs e)
        {
            
        }

        private void PauseButtonClick(UIComponent sender, EventArgs e)
        {
           
           else
            {
                The.Client.ResumeGame();
            }
        }*/

       

       /* void tbTutorial_Click(UIComponent sender, EventArgs e)
        {
            if (!intf.HUDTutorialPanel.DisplayWindow.Visible)
            {
                intf.HUDTutorialPanel.ShowInScreenSpace(sender.AbsolutePosition.X,
                   sender.AbsolutePosition.Y + sender.Height); 
            }
            else
            {
                intf.HUDTutorialPanel.Hide();
            }
        }*/

        void tbHelp_Click(UIComponent sender, EventArgs e)
        {
            if (!intf.HUDHelpPanel.DisplayWindow.IsVisibleAndActive)
            {
                intf.HUDHelpPanel.ShowInScreenSpace(sender.AbsolutePosition.X,
                   sender.AbsolutePosition.Y + sender.Height); 
            }
            else
            {
                intf.HUDHelpPanel.Hide();
            }
        }

        // invoked from Command
        public void OnSetSpeed(Speeds speed)
        {
            SetSpeedButton(speed);

        }

        private void SetSpeedButton(Speeds speed)
        {
            switch (speed)
            {
                case Speeds.Pause:
                    speedGroup.SelectMember(btPause);
                    break;

                case Speeds.Normal:
                    speedGroup.SelectMember(btNormalSpeed);
                    break;

                case Speeds.TwiceNormal:
                    speedGroup.SelectMember(bt2GameSpeed);
                    break;

                case Speeds.FourTimesNormal:
                    speedGroup.SelectMember(bt4GameSpeed);
                    break;
            }
        }

        public void OnPause()
        {
            SetSpeedButton(Speeds.Pause);
        }

        public void OnResume()
        {
            SetSpeedButton(Speeds.Normal);
        }

       
        void main_Click(UIComponent sender, EventArgs e)
        {

            The.InGameUI.ShowInGameMenu(); 

            /*
            intf.HUDBuildPanel.ShowInScreenSpace(sender.AbsolutePosition.X,
               sender.AbsolutePosition.Y - intf.HUDBuildPanel.DisplayWindow.Height); 
           */
        }

        private TextButton AddBlackTextButton(int buttonLeft, int buttonTop, string text, string tooltip)
        {
            TextButton button = new TextButton(intf.gui);
            DisplayWindow.Add(button);
            button.Text = text;
            button.Position = new Point(buttonLeft, buttonTop);
            button.Init(TextButton.TextButtonType.Black);
            button.Height = buttonHeight;
            button.ToolTip = tooltip;
            return button;
        }

        private TextButton AddSpeedButton(RadioGroup rgSpeed, int buttonLeft, Speeds speed, string text, string tooltip)
        {
            TextButton button = new TextButton(intf.gui);
            rgSpeed.Add(button);
            button.Text = text;           
            button.Position = new Point(buttonLeft, 0);
            button.Init(TextButton.TextButtonType.BlackSlim);
            button.CheckedMode = CheckedModes.CanBeChecked;
            button.Height = buttonHeight;
            button.ScaleWidthToFitText();
            button.ToolTip = tooltip;
            button.EventArgs = new SpeedArg() { Speed = speed };
            return button;
        }

        public void DrawPauseIcon()
        {
            The.Client.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            The.Client.spriteBatch.Draw(The.InGameUI.gui.GUISpriteSheet.Texture, pausePosition, pauseIcon, The.InGameUI.SelectedCyclePlayer.GetCurrentColor(Color.White)); // color);
            The.Client.spriteBatch.End();            
        }
    }
}
