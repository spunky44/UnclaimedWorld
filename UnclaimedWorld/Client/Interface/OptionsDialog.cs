using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using System.IO;
using UWGame.SimSide.Maps;
using System.Xml.Serialization;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide;
using GameStateManagement;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Interface.LCD;



namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// this in game dialog window contains functions such as Music on/off, music/sound volume etc.
    /// </summary>
    public class OptionsDialog : Panel
    {
        protected Box display;
        protected LCDScreen lcdScreen;
        protected UIComponent lcdSurface;

        Grid surfaceGrid;

        CheckBox cbMusic, cbSound, cbFullscreen, cbBorder, cbHardwareModeSwitch;
        ComboBox cbResolution;

        TextBox tbWidth, tbHeight;

        FillableBar fbMusicVolume, fbSoundVolume, fbZoom;

        RadioButton rbCustom, rbFixed;

        public event EventHandler CancelClick;
        public event EventHandler OKClick;

        RadioGroup rgResolution;

        ErrorsAndMessages errorsAndMessages;

        const int xPos = 6;
        public OptionsDialog(Interface.CommonInterface intf) :
            base(intf, "OPTIONS", new Point(420, 120),
                 new Vector2(400, 600), Level.Menu, PanelType.RegularEdges)
        {
            GUIManager gui = Interface.gui;


            int column2 = 124;

            //  int buttonX = MarginX + spacing;

            FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, BottomMarginForButtons,
                RosterMargin, out display, out lcdSurface, ref lcdScreen);

            CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, false, bottomMargin: 40);

            //LCDInnerPanel innerPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, true, 1f);
            UIComponent panel = new UIComponent(Interface.gui);
            panel.Width = surfaceGrid.SurfaceWidth;
            panel.Height = 600; 
            surfaceGrid.AddEntry("surfaceKey", panel); // innerPanel.Panel);


            Label lblGraphics = AddSectionHeader(6, panel, "GRAPHICS");

            Label lblRestart = new Label(Interface.gui);
            panel.Add(lblRestart);
            lblRestart.Init(Label.LabelType.LCDNormal);
            lblRestart.NormalColor = UIComponent.errorColor;
            lblRestart.Text = "Restart the game to apply Graphics changes!";
            lblRestart.FitToText();
            lblRestart.X = xPos;
            lblRestart.Y = lblGraphics.Bottom + SingleSpacing;

            cbFullscreen = new CheckBox(Interface.gui);
            panel.Add(cbFullscreen);
            cbFullscreen.Init(CheckBoxType.LCD, CheckBoxFlavor.Blue);
            cbFullscreen.Text = "FULL SCREEN";
            cbFullscreen.FitToText();
            cbFullscreen.X = xPos;
            cbFullscreen.Y = lblRestart.Bottom + SingleSpacing; // = new Point(xPos, cbSound.Y + 4 * DoubleSpacing);
            //  cbFullscreen.Label.NormalColor = Color.Black;           
            cbFullscreen.Click += new ClickHandler(cbFullscreen_Click);
                     
         /*   TextArea taWarning = new TextArea(Interface.gui, ListBoxType.LCD);
            panel.Add(taWarning);
            taWarning.Init(Label.LabelType.LCDNormal);
            taWarning.X = xPos;
            taWarning.Y = cbFullscreen.Bottom - 4;
            taWarning.Width = surfaceGrid.SurfaceWidth - taWarning.X;
            */
           // int yPos = taWarning.Bottom + 2 * SingleSpacing;

            int yPos = cbFullscreen.Bottom + SingleSpacing;

            cbHardwareModeSwitch = new CheckBox(Interface.gui);
            panel.Add(cbHardwareModeSwitch);
            cbHardwareModeSwitch.Init(CheckBoxType.LCD, CheckBoxFlavor.Green);
            cbHardwareModeSwitch.Text = "HARDWARE MODE SWITCH";
            cbHardwareModeSwitch.FitToText();
            cbHardwareModeSwitch.X = column2; // xPos;
            cbHardwareModeSwitch.Y = yPos; 
            cbHardwareModeSwitch.ToolTip = "If selected, the game attempts to switch the screen resolution when in fullscreen. If not selected, only the desktop resolution is available for fullscreen. Is selected by default.";
            //  cbFullscreen.Label.NormalColor = Color.Black;           
            cbHardwareModeSwitch.Click += cbHardwareModeSwitch_Click;

            yPos = cbHardwareModeSwitch.Bottom + 2 * SingleSpacing;

            Label lblResolution = new Label(Interface.gui);
            panel.Add(lblResolution); 
            lblResolution.Init(Label.LabelType.LCDHeadingBlue);
            lblResolution.Text = "RESOLUTION";
            lblResolution.FitToText();
            lblResolution.CenterThisVertically(yPos);
            lblResolution.X = xPos;

            yPos = lblResolution.Bottom + 2 * SingleSpacing;

            rgResolution = new RadioGroup(gui);

            rgResolution.NewMemberChecked += rgResolution_NewMemberChecked;

            rbFixed = new RadioButton(gui);
            panel.Add(rbFixed);
            rgResolution.Add(rbFixed, false);
            rbFixed.Init(CheckBoxType.LCDRadioBanner);
            rbFixed.Text = "FIXED:";
            rbFixed.FitToText();
            rbFixed.CenterThisVertically(yPos);
            rbFixed.X = xPos;
            rbFixed.ToolTip = "Select this option to select a fixed resolution among those supported by the video card.";
          
            cbResolution = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            panel.Add(cbResolution); // lcdSurface.Add(cbSource);
            cbResolution.Init(ComboBoxTypes.LCD);
            cbResolution.X = column2; // lblResolution.Right + SingleSpacing; // xPos;
            // cbResolution.Y = lblResolution.Bottom + SingleSpacing;
            //  cbResolution.IsEditable = false;
            cbResolution.Width = 200;
            cbResolution.CenterThisVertically(yPos);
            cbResolution.SelectionChanged += cbResolution_SelectionChanged;

            int yposFields = cbResolution.Bottom + 2 * SingleSpacing;

            rbCustom = new RadioButton(gui);
            panel.Add(rbCustom);
            rgResolution.Add(rbCustom, false);
            rbCustom.Init(CheckBoxType.LCDRadioBanner);
            rbCustom.Text = "CUSTOM:";
            rbCustom.FitToText();
            rbCustom.CenterThisVertically(yposFields);
            rbCustom.X = xPos;
            rbCustom.ToolTip = "Select this option to enter a custom window size. Not available in fullscreen.";


            Label lblWidth = new Label(Interface.gui);
            panel.Add(lblWidth);
            lblWidth.Init(Label.LabelType.LCDHeadingBlue);
            lblWidth.Text = "W:";          
          //  lblWidth.Width = 60;
            lblWidth.FitToText();
            lblWidth.CenterThisVertically(yposFields);
            lblWidth.X = cbResolution.X; // xPos;

            tbWidth = new TextBox(Interface.gui);
            panel.Add(tbWidth);
            tbWidth.Init(TextBox.TextBoxType.LCD);
            tbWidth.X = lblWidth.Right + SingleSpacing;
            tbWidth.Width = 46;
            tbWidth.Height = 20;
            tbWidth.VMargin = 1;
            tbWidth.IsEditable = true;
            tbWidth.IsNumericBox = true;
            tbWidth.Y = lblWidth.Y;
            //tbWidth.CenterThisVertically(yposFields);

            Label lblHeight = new Label(Interface.gui);
            panel.Add(lblHeight);
            lblHeight.Init(Label.LabelType.LCDHeadingBlue);
            lblHeight.Text = "H:";           
            //lblHeight.Width = 60;
            lblHeight.FitToText();
            lblHeight.CenterThisVertically(yposFields);
            lblHeight.X = tbWidth.Right + SingleSpacing;
            // lblWidth.Y = cbResolution.Bottom + 4 * DoubleSpacing; 

            tbHeight = new TextBox(Interface.gui);
            panel.Add(tbHeight);
            tbHeight.Init(TextBox.TextBoxType.LCD);
            tbHeight.X = lblHeight.Right + SingleSpacing;
            tbHeight.Width = tbWidth.Width;
            tbHeight.Height = tbWidth.Height;
            tbHeight.VMargin = tbWidth.VMargin;
            tbHeight.IsEditable = true;
            tbHeight.IsNumericBox = true;
            tbHeight.CenterThisVertically(yposFields);

            cbBorder = new CheckBox(Interface.gui);
            panel.Add(cbBorder);
            cbBorder.Init(CheckBoxType.LCD, CheckBoxFlavor.Purple);
            cbBorder.Text = "BORDER";
            cbBorder.FitToText();
            cbBorder.ToolTip = "Select whether the application window should have a border.";

            cbBorder.Position = new Point(xPos, tbHeight.Bottom + 2 * DoubleSpacing);

            string zoomTooltip = "Set the magnification (pixel zoom) level. Restart the game to see the effect. WARNING: Setting a high magnification level may cause the interface to be truncated. In case of problems, use the ZoomFactor property in Options.xml to revert.";  

            Label lblZoom = new Label(Interface.gui);
            panel.Add(lblZoom);
            lblZoom.Init(Label.LabelType.LCDSmallHeadingBanner);
            lblZoom.Text = "MAGNIFICATION:";
            lblZoom.Position = new Point(xPos, cbBorder.Bottom + 6);
            lblZoom.ToolTip = zoomTooltip;
            lblZoom.TooltipExpires = false;
            lblZoom.TooltipWidth = 300;

            fbZoom = new FillableBar(Interface.gui, FillableBar.FillableBarType.LCDSlider, false, false);
            panel.Add(fbZoom);
            fbZoom.Width = 220;
            fbZoom.X = 128;
            fbZoom.Y = lblZoom.Y; // yPos + 5;
            fbZoom.ToolTip = zoomTooltip;           
            //fbZoom.SliderMouseUp += new EventHandler(fbMusicVolume_SliderMouseUp);
            fbZoom.MaxValue = 400;
            fbZoom.ShowMaxValueLabelAtEnd = true;
            fbZoom.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Always; // .Never; 
            fbZoom.StepSize = 25;
            fbZoom.KnobWidth = 16;
            fbZoom.ShowNotches = true;           
            fbZoom.MaxSliderValueSymbol = null;
            fbZoom.MaxSliderValueTooltip = null;
            fbZoom.DisplayValueFunction = (v => (0.01f * v).ToString("N2"));

            Label lblSoundHeader = AddSectionHeader(280, panel, "SOUND");

            yPos = lblSoundHeader.Bottom + SingleSpacing;

            cbMusic = new CheckBox(Interface.gui);
            panel.Add(cbMusic);
            cbMusic.Text = "MUSIC";
            cbMusic.Init(CheckBoxType.LCD);
            cbMusic.X = xPos; //, lblSoundHeader.Bottom + SingleSpacing);
           // cbMusic.CenterThisVertically(yPos);
            cbMusic.Y = yPos;
            //  cbMusic.Label.NormalColor = Color.Black;
            cbMusic.FitToText();
            //cbMusic.Width = 80;
            cbMusic.Click += new ClickHandler(cbMusic_Click);

            fbMusicVolume = new FillableBar(Interface.gui, FillableBar.FillableBarType.LCDSlider, false, false);
            panel.Add(fbMusicVolume);           
            fbMusicVolume.Width = 150;
           // fbMusicVolume.SetBarWidth(fbMusicVolume.Width - 40);
            fbMusicVolume.X = 128;
            fbMusicVolume.Y = yPos + 5;      
            fbMusicVolume.ToolTip = "Set the music volume.";
           // fbMusicVolume.EventArgs = eventArgs;        
            fbMusicVolume.SliderMouseUp += new EventHandler(fbMusicVolume_SliderMouseUp);
            fbMusicVolume.MaxValue = 100;
            fbMusicVolume.ShowMaxValueLabelAtEnd = false;
            fbMusicVolume.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Never;
            fbMusicVolume.KnobWidth = 16;

            yPos = cbMusic.Y + 2 * DoubleSpacing;
            cbSound = new CheckBox(Interface.gui);
            panel.Add(cbSound);
            cbSound.Text = "SOUND";
            cbSound.Init(CheckBoxType.LCD);
            cbSound.X = xPos;
            cbSound.Y = yPos;
           // cbSound.CenterThisVertically(yPos);
            //  cbSound.Label.NormalColor = Color.Black;
            //cbSound.Width = 80;
            cbSound.FitToText();
            cbSound.Click += new ClickHandler(cbSound_Click);


            fbSoundVolume = new FillableBar(Interface.gui, FillableBar.FillableBarType.LCDSlider, false, false);
            panel.Add(fbSoundVolume);
            fbSoundVolume.Width = 150;
           // fbSoundVolume.SetBarWidth(fbMusicVolume.Width - 40);
            fbSoundVolume.X = 128;
            fbSoundVolume.Y = yPos + 5;
            fbSoundVolume.ToolTip = "Set the sound effects volume.";
            fbSoundVolume.SliderMouseUp += new EventHandler(fbSoundVolume_SliderMouseUp);
            fbSoundVolume.MaxValue = 100;
            fbSoundVolume.ShowMaxValueLabelAtEnd = false;
            fbSoundVolume.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Never;
            fbSoundVolume.KnobWidth = 16;

            errorsAndMessages = new ErrorsAndMessages(lcdSurface, Window.guiManager, xPos, lcdSurface.Height - 2 * DoubleSpacing);
            

            TextButton btOK = AddLowerButton("OK", "Accepts the changes and closes the dialog.", Align.Left);
            btOK.Click += new ClickHandler(btOK_Click);

            TextButton btCancel = AddLowerButton("CANCEL", "Closes the dialog without applying the changes.", Align.Right);
            btCancel.Click += new ClickHandler(btCancel_Click);

         
            AddDirtOnStraightEdges(true);

        }

        void cbHardwareModeSwitch_Click(UIComponent sender, EventArgs e)
        {
            SetGraphicsEnabledStates();
        }

        void rgResolution_NewMemberChecked(ICanBeChecked arg1, EventArgs arg2)
        {
            if (arg1 == rbFixed)
            {
                EnableFixedResolution();
            }
            else
            {
                EnableCustomResolution();
            }
            
        }

        private void DisableResolution()
        {
            rbFixed.Enabled = false;
            rbCustom.Enabled = false;

            cbResolution.Enabled = false;

            tbWidth.Enabled = false;
            tbHeight.Enabled = false;
        }

        void EnableFixedResolution()
        {
            rbFixed.Enabled = true;

            cbResolution.Enabled = true;

            tbWidth.Enabled = false;
            tbHeight.Enabled = false;
        }

        void EnableCustomResolution()
        {
            rbCustom.Enabled = true;

            cbResolution.Enabled = false;

            tbWidth.Enabled = true;
            tbHeight.Enabled = true;
        }


        private int VolumeToSliderIncrementsQuad(float volume)
        {

            double invQuad = 100d * Math.Pow(volume, 0.25d);
           // double invQuad = 100 * Math.Sqrt(volume);

           // float lerped = MathHelper.Lerp(25f, 100f, invQuad);

            return (int)MathHelper.Clamp((float)invQuad, 0f, 100f);
        }

        private float SliderValueToVolumeQuad(int value)
        {          
          
            float valueAsFloat = MathHelper.Clamp((float)value, 0f, 100f);
  
            // quadratic function works best, with the lower end removed...
           // valueAsFloat = MathHelper.Lerp(25f, 100f, valueAsFloat / 100f); // lerp = value1 + (value2 - value1) * amount


            var quad = Math.Pow(valueAsFloat, 4d) * 0.00000001f; 
           // var quad = Math.Pow(value, 2d) * 0.0001f;
          
            return MathHelper.Clamp((float)quad, 0.001f, 1.0f);


            /* // convert to logarithmic scale
            float linearScaled = valueAsFloat / 33f;

            var logged = Math.Log(linearScaled);
            return MathHelper.Clamp((float)logged, 0.001f, 1.0f);*/

        }



        private int VolumeToSliderIncrementsLog(float volume)
        {
            double invLog = Math.Pow(101d, volume) - 1d;


            return (int)MathHelper.Clamp((float)invLog, 0f, 100f);
            
           
            /*
            int invLog = (int)Math.Pow(10d, volume);

            float linearScaled = volume * 33f;

            return (int)MathHelper.Clamp(linearScaled, 0f, 100f);
            */
          
        }


        /// <summary>
        /// 50 -> 0.8
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private float SliderValueToVolumeLog(int value)
        {
            return SliderValueToVolumeQuad(value);



            // convert to logarithmic scale
            float valueAsFloat = MathHelper.Clamp((float)value, 0f, 100f);


            var logged2 = Math.Log(valueAsFloat + 1) / Math.Log(101);
            return MathHelper.Clamp((float)logged2, 0.001f, 1.0f);


            /*
            float linearScaled = valueAsFloat / 33f;

            var logged = Math.Log(linearScaled);
            return MathHelper.Clamp((float)logged, 0.001f, 1.0f);*/

        }

        /// <summary>
        /// update in real time - don't save yet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fbMusicVolume_SliderMouseUp(object sender, EventArgs e)
        {
            Interface.Game.Controller.AudioManager.MusicVolume = SliderValueToVolumeQuad(fbMusicVolume.Value);

           
        }

        void fbSoundVolume_SliderMouseUp(object sender, EventArgs e)
        {
            if (The.Client != null)
            {
                The.Client.Controller.AudioManager.SoundVolume = SliderValueToVolumeQuad(fbSoundVolume.Value);
            }

            GUIManager.BeepBasicPanel.Play();
            //The.Client.ScreenManager.AudioManager.PlaySound();
        }

        private Label AddSectionHeader(int xYpos, UIComponent panel, string text)
        {
            Label lblHeader = new Label(Interface.gui);
            panel.Add(lblHeader);
            lblHeader.Init(Label.LabelType.LCDBigHeaderBanner);
            lblHeader.Text = text;
            lblHeader.X = xPos;
            lblHeader.Y = xYpos; // 6;
            lblHeader.Width = surfaceGrid.SurfaceWidth - lblHeader.X;
            // lblHeader.FitToText();

            return lblHeader;
        }

        void cbResolution_SelectionChanged(UIComponent sender)
        {

        }

        const string emptyKey = "";
        private void PopulateResolutionsCombo()
        {
            cbResolution.Clear();

            DisplayModeCollection modes = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes;

            cbResolution.AddEntry(emptyKey, "");
            foreach (var item in modes)
            {
                if (item.Width >= 800 && item.Format == SurfaceFormat.Color)
                {
                    string displayText = string.Format("Width: {0} Height: {1}", item.Width, item.Height);
                    cbResolution.AddEntry(item, displayText);
                }
            }


            cbResolution.SelectionChanged -= new SelectionChangedHandler(cbResolution_SelectionChanged);
            cbResolution.SelectedIndex = 0;
            cbResolution.SelectionChanged += new SelectionChangedHandler(cbResolution_SelectionChanged);

            /* foreach (var item in graphOptions)
             {
                 cbSource.AddEntry(item, item.ToString());
             } */

        }



        void cbFullscreen_Click(UIComponent sender, EventArgs e)
        {           
            SetGraphicsEnabledStates();
        }

       



        /// <summary>
        /// update in realtime - don't save yet!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbMusic_Click(UIComponent sender, EventArgs e) 
        {
            if (cbMusic.IsChecked)
            {
                Interface.Game.Controller.AudioManager.MusicVolume = Interface.Game.Controller.Options.MusicVolume;
            }
            else
            {
                Interface.Game.Controller.AudioManager.MusicVolume = 0f;
            }

            SetSoundEnabledStates();
        }


        void cbSound_Click(UIComponent sender, EventArgs e)
        {
            if (The.Client != null)
            {
                if (cbSound.IsChecked)
                {
                    The.Client.AudioManager.SoundVolume = Interface.Game.Controller.Options.SoundFXVolume;
                }
                else
                {
                    The.Client.AudioManager.SoundVolume = 0f;
                }
            }

            SetSoundEnabledStates();
        }

        void btCancel_Click(UIComponent sender, EventArgs e)
        {
            // undo the changes:
            ApplySettings();

            Hide();

            if (CancelClick != null)
                CancelClick.Invoke(this, null);

        }

        private bool CanSetResolution()
        {
            return cbFullscreen.IsChecked == false || cbHardwareModeSwitch.IsChecked; 

        }

        private bool ValidateInput()
        {
            if (CanSetResolution())
            {
                if (rbFixed.IsChecked) //cbFullscreen.IsChecked)
                {
                    if (cbResolution.SelectedKey.Equals(emptyKey))
                    {
                        errorsAndMessages.ShowError("Select a resolution from the list.");

                        return false;
                    }
                }
                else
                {
                    if (tbWidth.Text == null || tbHeight.Text == null)
                    {
                        errorsAndMessages.ShowError("Both width and height is required.");
                        return false;
                    }
                    else
                    {
                        int width, height;
                        if (!int.TryParse(tbWidth.Text, out width))
                        {
                            errorsAndMessages.ShowError("Illegal width entered.");
                            return false;
                        }

                        if (!int.TryParse(tbHeight.Text, out height))
                        {
                            errorsAndMessages.ShowError("Illegal height entered.");
                            return false;
                        }

                        if (width > UnclaimedWorld.MaxScreenDimensions.X)
                        {
                            errorsAndMessages.ShowError(string.Format("Illegal width entered. {0} is maximum.", UnclaimedWorld.MaxScreenDimensions.X));
                            return false;
                        }

                        if (height > UnclaimedWorld.MaxScreenDimensions.Y)
                        {
                            errorsAndMessages.ShowError(string.Format("Illegal height entered. {0} is maximum.", UnclaimedWorld.MaxScreenDimensions.Y));
                            return false;
                        }
                    }
                }
            }


            if (fbZoom.Value < 100f)
            {
                errorsAndMessages.ShowError("Magnification must be at least 1.");
                return false;
            }

            errorsAndMessages.Hide();

            return true;
        }

       /* private bool ValidateInput()
        {
            if (cbFullscreen.IsChecked)
            {
                if (cbResolution.SelectedKey.Equals(emptyKey))
                {
                    errorsAndMessages.ShowError("Select a resolution from the list.");

                    return false;
                }
            }
            else
            {
                if (!cbResolution.SelectedKey.Equals(emptyKey)
                    && (!string.IsNullOrEmpty(tbWidth.Text) || !string.IsNullOrEmpty(tbHeight.Text)))
                {
                    errorsAndMessages.ShowError("Either select a fixed resolution or enter custom data.");
                    return false;
                }
                else if (cbResolution.SelectedKey.Equals(emptyKey))
                {
                    if (tbWidth.Text == null || tbHeight.Text == null)
                    {
                        errorsAndMessages.ShowError("Both width and height is required.");
                        return false;
                    }
                    else
                    {
                        int width, height;
                        if (!int.TryParse(tbWidth.Text, out width))
                        {
                            errorsAndMessages.ShowError("Illegal width entered.");
                            return false;
                        }

                        if (!int.TryParse(tbHeight.Text, out height))
                        {
                            errorsAndMessages.ShowError("Illegal height entered.");
                            return false;
                        }

                        if (width > UnclaimedWorld.MaxScreenDimensions.X)
                        {
                            errorsAndMessages.ShowError(string.Format("Illegal width entered. {0} is maximum.", UnclaimedWorld.MaxScreenDimensions.X));
                            return false;
                        }

                        if (height > UnclaimedWorld.MaxScreenDimensions.Y)
                        {
                            errorsAndMessages.ShowError(string.Format("Illegal height entered. {0} is maximum.", UnclaimedWorld.MaxScreenDimensions.Y));
                            return false;
                        }
                    }
                }
            }

            errorsAndMessages.Hide();

            return true;

        }*/

        void btOK_Click(UIComponent sender, EventArgs e)
        {
            if (ValidateInput())
            {
                Options options = Interface.Game.Controller.Options;

                options.MusicEnabled = cbMusic.IsChecked;
                options.SoundEnabled = cbSound.IsChecked;
                options.FullScreen = cbFullscreen.IsChecked;
                options.HardwareModeSwitch = cbHardwareModeSwitch.IsChecked;

                if (!CanSetResolution()) //  cbHardwareModeSwitch.IsChecked)
                {                    
                    options.ResolutionWidth = 0; // not used!
                    options.ResolutionHeight = 0;
                }
                else
                {
                    if (rbFixed.IsChecked)
                    {
                        DisplayMode mode = (DisplayMode)cbResolution.SelectedKey;
                        options.ResolutionWidth = mode.Width;
                        options.ResolutionHeight = mode.Height;
                    }
                    else //if (tbWidth.Text != null && tbHeight.Text != null)
                    {
                        options.ResolutionWidth = int.Parse(tbWidth.Text);
                        options.ResolutionHeight = int.Parse(tbHeight.Text);
                    }
                }

                if (!cbFullscreen.IsChecked)
                {
                    options.Borderless = !cbBorder.IsChecked;

                }

                if (cbMusic.IsChecked)
                {
                    options.MusicVolume = SliderValueToVolumeQuad(fbMusicVolume.Value);
                }

                if (cbSound.IsChecked)
                {
                    options.SoundFXVolume = SliderValueToVolumeQuad(fbSoundVolume.Value);
                }

                options.ZoomFactor = 0.01f * fbZoom.Value;


                ApplyAndSaveOptionsToFile();

                Hide();

                if (OKClick != null)
                    OKClick.Invoke(this, null);
            }

        }



        private void ApplyAndSaveOptionsToFile()
        {
            ApplySettings();

            //BaseDataLoader.SerializeObject(this, "", FileName, Config.DataType.UserSettings);            
            //BaseDataLoader.SerializeObject(The.Client.ScreenManager.Options, "", Options.FileName, Config.DataType.UserSettings);

            Interface.Game.Controller.Options.Write();


        }

        private void ApplySettings()
        {
            Options options = Interface.Game.Controller.Options;

            // apply the settings:
            if (The.Client != null)
            {
                The.Client.AudioManager.Init(options);
            }

            Interface.Game.Controller.AudioManager.Init(options, false); //The.Client.ScreenManager.AudioManager.Init(options, false);

        }



        public override void ShowDialog(bool modal)
        {
            base.ShowDialog(modal);

            Fill();
        }

        private void Fill()
        {
            Options options = Interface.Game.Controller.Options; 

            cbMusic.IsChecked = options.MusicEnabled;
            cbSound.IsChecked = options.SoundEnabled;
            cbFullscreen.IsChecked = options.FullScreen;
            cbHardwareModeSwitch.IsChecked = options.HardwareModeSwitch;
            cbBorder.IsChecked = !options.Borderless;
           
            fbSoundVolume.Value = VolumeToSliderIncrementsQuad(options.SoundFXVolume);
            fbMusicVolume.Value = VolumeToSliderIncrementsQuad(options.MusicVolume);
            fbSoundVolume.UpdateSliderPosition();
            fbMusicVolume.UpdateSliderPosition();

            fbZoom.Value = (int)(options.ZoomFactor * 100f);
            fbZoom.UpdateSliderPosition();

            PopulateResolutionsCombo();

            if (CanSetResolution()) // cbFullscreen.IsChecked == false || cbHardwareModeSwitch.IsChecked)
            {
                object mode = cbResolution.EntriesByKey.Keys.FirstOrDefault(
                    key => key is DisplayMode
                       && ((DisplayMode)key).Width == options.ResolutionWidth
                       && ((DisplayMode)key).Height == options.ResolutionHeight);

                if (mode != null)
                {
                    cbResolution.SelectedKey = mode;
                    EnableFixedResolution();

                    rgResolution.SelectMember(rbFixed);
                }
                else
                {
                    cbResolution.SelectedKey = emptyKey;

                    tbWidth.Text = options.ResolutionWidth.ToString();
                    tbHeight.Text = options.ResolutionHeight.ToString();

                    EnableCustomResolution();

                    rgResolution.SelectMember(rbCustom);
                }
            }
            else
            {
                cbResolution.SelectedKey = emptyKey;
                tbWidth.Text = "";
                tbHeight.Text = "";

                DisableResolution();
            }

           // SetGraphicsEnabledStates();

            if (cbFullscreen.IsChecked)
            {
                cbBorder.Enabled = false;
                
                rbCustom.Enabled = false;
            }
            else
            {
                cbBorder.Enabled = true;

                rbCustom.Enabled = true;
            }

            SetSoundEnabledStates();
        }


        private void SetGraphicsEnabledStates()
        {
            if (cbFullscreen.IsChecked)
            {
                cbBorder.Enabled = false;

                cbHardwareModeSwitch.Enabled = true;

                if (cbHardwareModeSwitch.IsChecked)
                {

                    EnableFixedResolution();

                    rbCustom.Enabled = false;


                    rgResolution.SelectMember(rbFixed);
                }
                else
                {
                    DisableResolution();
                }

            }
            else
            {
                cbBorder.Enabled = true;

                cbHardwareModeSwitch.Enabled = false;

               // EnableCustomResolution();

                rbFixed.Enabled = true;
                cbResolution.Enabled = true;


                rbCustom.Enabled = true;

            }           
        }

        private void SetSoundEnabledStates()
        {
            if (cbMusic.IsChecked)
            {
                fbMusicVolume.Enabled = true;
            }
            else
            {
                fbMusicVolume.Enabled = false;
            }

            if (cbSound.IsChecked)
            {
                fbSoundVolume.Enabled = true;
            }
            else
            {
                fbSoundVolume.Enabled = false;
            }
        }

    }
}
