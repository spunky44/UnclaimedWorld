using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using UWGame.SimSide;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.AI;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// upper right
    /// </summary>
    public class StatusScreen
    {
        public Window DisplayWindow;

        Image plastic, fingerprint;

        Box plasticEdge;
             

        public const int HeightOfStatusImage = 130; //100;


        //ImageButton btNext;
      
        
        CRTAnimator crtAnimator;

        /// <summary>
        /// has the center button on it...
        /// </summary>
        Window centerWindow;
        ImageButton btTrack;

        public bool IsOn
        {
            get
            {
                return crtAnimator.IsOn;
            }
        }

      /*  private void InitAnims()
        {
            
        
            GUIManager gui = The.InGameUI.gui;

            switchChannelSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);

       

            // xna 4:
            switchChannelBlackFrameSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);
            switchChannelBlackFrameSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame1_small")) { Color = new Color(0f, 0f, 0f, 1f) });

            microSwitch = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.05f, false);
            microSwitch.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame5_small")) { Color = new Color(0.1f, 0.1f, 0.1f, 0.1f) });
            microSwitch.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6_small")) { Color = new Color(0f, 0f, 0f, 0f) });


            switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame1_small")));
            switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame2_small")));
            switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame3_small")));
            switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame3_small")) { Color = new Color(0f, 0f, 0f, 1f) }); 
            switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame4_small")) { Color = new Color(0.67f, 0.67f, 0.67f, 0.67f) });
            switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame5_small")) { Color = new Color(0.5f, 0.5f, 0.5f, 0.5f) });
            switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6_small")) { Color = new Color(0.27f, 0.27f, 0.27f, 0.27f) });
            switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6_small")) { Color = new Color(0f, 0f, 0f, 0f) });

            interferenceSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);
            interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame4_small")) { Color = new Color(0.67f, 0.67f, 0.67f, 0.67f) });
            interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame5_small")) { Color = new Color(0.5f, 0.5f, 0.5f, 0.5f) });
            interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6_small")) { Color = new Color(0.27f, 0.27f, 0.27f, 0.27f) });
            interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("switch_frame6_small")) { Color = new Color(0f, 0f, 0f, 0f) });

            NoReceptionSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.05f, true);
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame1_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame1_small")));
          //  NoReception.Cells.Add(new Cell(gui.GUISpriteSheet.SourceRectangle("no_reception_frame1")){ Color = new Color(0f, 0f, 0f, 1f) });
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame2_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame2_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame3_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame3_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame4_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame4_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame5_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame5_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame6_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame6_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7_small")));
            NoReceptionSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.GetSourceRectangle("no_reception_frame7_small")) { Color = new Color(0f, 0f, 0f, 1f) });
            
            
        
        }*/

        int crtHeight = 214;
        int crtWidth = 277;

        public const int CRTPlasticEdgeCenter = 11; // where the display corner should be

        public StatusScreen()
        {
            GUIManager gui = The.InGameUI.gui;
            Game game = The.Sim.Controller.Game;

            InGameInterface intf = The.InGameUI;

            int cycleWindowWidth = 41;

            centerWindow = new Window(The.InGameUI.gui);
          //  centerWindow.Position = new Point(The.Client.GraphicsDevice.Viewport.Width - cycleWindowWidth, 235);
            centerWindow.Position = new Point(The.Client.Controller.DrawArea.Width - cycleWindowWidth, 235);
            centerWindow.WindowSize = new Vector2(cycleWindowWidth, 33); //36); 
            centerWindow.Level = Level.Middle;
            centerWindow.IsMovable = false;
            centerWindow.Resizable = false;
            centerWindow.Margin = 0;
            centerWindow.HasCloseButton = false;
            centerWindow.Skin = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("centerentitybutton_basepatch");
            centerWindow.CornerSize = 5; // 15;
            centerWindow.Hide(); //hide it


            btTrack = new ImageButton(The.InGameUI.gui);
            centerWindow.Add(btTrack);
            btTrack.Position = new Point(3, 1);
            btTrack.Init(ImageButtonType.CenterOnEntity);
            btTrack.Click += new ClickHandler(center_Click);
            btTrack.RightClick += new ClickHandler(center_RightClick);
            btTrack.ZOrder = 1f;
            btTrack.ToolTip = "Left click to center on the selected entity. Right click to track the entity.";


            DisplayWindow = new Window(gui);
            // Sequence matters for skins!!!
            DisplayWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle("TV_panel"); // empty?
            DisplayWindow.CornerSize = 7;
            DisplayWindow.Margin = 0; // 7;
            DisplayWindow.Resizable = false;
            DisplayWindow.IsMovable = false;
            DisplayWindow.Position = new Point(The.InGameUI.MainLeft, 0);
            DisplayWindow.WindowSize = new Vector2(InGameInterface.InterfaceWidth - 6, 246); // 280); // InGameInterface.SmallPanelTop);  
            DisplayWindow.HasCloseButton = false;
            DisplayWindow.HasCRTOrLCDComponents = true;
            DisplayWindow.HasOverlayComponents = true;
            DisplayWindow.Level = Level.Bottom;
            DisplayWindow.Show();
            DisplayWindow.DebugTag = "tvpanel";

            Rectangle rect;
            //DrawRubber(gui, game);



            /*    plastic = new Image(gui);
                rect = gui.GUISpriteSheet.GetSourceRectangle("TV_plastic");
                plastic.SetSkinLocation(SkinState.Normal,rect);        
               // DisplayWindow.Add(plastic);
                plastic.Position = new Point(20, 20);
                plastic.Width = crtWidth; 
                plastic.Height = DisplayWindow.Height;          
                plastic.ScaleImageToSizeOfControl = true;
                */


            /*  fingerprint = new Image(gui);
              rect = gui.GUISpriteSheet.GetSourceRectangle("Fingerprint");
              fingerprint.SetSkinLocation(SkinState.Normal,rect);
              plastic.Add(fingerprint); // clips to plastic!
              fingerprint.Alpha = 0.42f; // 0.85f;
              fingerprint.Position = new Point(90, 188);
              fingerprint.ResizeControlToFitImage();
              */

            Point framePosition = new Point(20, 20);

            AddCRTPlasticFrame(gui, DisplayWindow, framePosition /*new Point(plastic.X, plastic.Y)*/, crtWidth, crtHeight, out plasticEdge);



            /*  int buttonX = plastic.X + 6;
              int expandY = plasticEdge.Y + plasticEdge.Height + 5;
              */

            /*action = new ImageButton(gui);
            DisplayWindow.Add(action);
            action.Position = new Point(buttonX, expandY);
            action.Init(ImageButtonType.MinimapRubber);
            action.Click += new ClickHandler(action_Click); //new ClickHandler(ExpandEntityPanel_OnPress);
            action.RenderType = RenderType.Overlay;
            action.ZOrder = 1f;
            action.ToolTip = actionTooltip;

            lblAction = new Label(gui);
            DisplayWindow.Add(lblAction);
            lblAction.Text = "ACTION";
            lblAction.Init(Label.LabelType.PlainPanelNormal);
            lblAction.Color = Label.OffWhiteColor;
            lblAction.Position = new Point(action.X + (action.Width - lblAction.TextWidth) / 2, btLabelY); // center..
            lblAction.RenderType = RenderType.Overlay;
            

            buttonX = action.Right + 12; // plasticEdge.X + (plasticEdge.Width - expand.Right) / 3;*/

            /*   lblPrevious = new Label(gui);      
               lblPrevious.Text = "PREVIOUS";
               lblPrevious.Init(Label.LabelType.PlainPanelNormal);
               lblPrevious.Color = Label.OffWhiteColor;
               lblPrevious.Position = new Point(buttonX, btLabelY);
               lblPrevious.RenderType = RenderType.Overlay;
               */
            //btPrevious = new ImageButton(gui);       
            //btPrevious.Position = new Point(buttonX, expandY);
            //btPrevious.InitWithIcon(ImageButtonType.TVButton, "TV_icon_previous2", false); //ImageButtonType.MinimapRubber);
            ////btPrevious.Click += new ClickHandler(btPrevious_Click);
            //btPrevious.RenderType = RenderType.Overlay;
            //btPrevious.ZOrder = 1f;
            //btPrevious.ToolTip = "Select the previous entity";

            //buttonX = btPrevious.Right + 4; // plasticEdge.X + (plasticEdge.Width - expand.Width - expand.X) / 3;
            // plasticEdge.X + 2 * plasticEdge.Width / 3;

            /*lblNext = new Label(gui);
            lblNext.Text = "NEXT";
            lblNext.Init(Label.LabelType.PlainPanelNormal);
            lblNext.Color = Label.OffWhiteColor;
            lblNext.Position = new Point(buttonX, btLabelY);
            lblNext.RenderType = RenderType.Overlay;
            */


            /*
            buttonX = buttonX  + 8;
            btTrack = new ImageButton(gui);
            btTrack.Position = new Point(buttonX, expandY);
            btTrack.InitWithIcon(ImageButtonType.TVButton, "TV_icon_track", true); 
            //  btNext.Click += new ClickHandler(next_Click);
            btTrack.RenderType = RenderType.Overlay;            
            btTrack.ZOrder = 1f;
            btTrack.ToolTip = "Left click to center on the selected entity. Right click to track the entity.";
            */

            Image displayDust = Panel.AddDust(gui, DisplayWindow);
            displayDust.RenderType = RenderType.Overlay;
            displayDust.DebugTag = "event_dust";

            /* Image displayDust = new Image(gui);            
             rect = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
             displayDust.SetSkinLocation(SkinState.Normal,rect);
             displayDust.Alpha = 0.04f; // 0.07f;
             DisplayWindow.Add(displayDust);
             displayDust.Position = new Point(0, 0);
             displayDust.ResizeControlToFitImage();
             displayDust.RenderType = RenderType.Overlay;
             displayDust.DebugTag = "event_dust";
           */


            // contains a CRT screen:
            crtAnimator = new CRTAnimator(DisplayWindow,
                                            framePosition,
                                            framePosition,
                                            crtWidth,
                                            crtHeight,
                                            ReflectionToUse.Small, 1f);


        }


      /*  public UIComponent GetNewSurfaceContent(out Label lblStatusHeading, out Label lblStatusInfo1, out Label lblStatusInfo2)
        {
            Game game = The.Sim.Controller.Game;
            UIComponent statusContent = GetNewSurfaceContent();

            int statusTextX = 8, statusTextWidth = 200;

            lblStatusHeading = new Label(The.InGameUI.gui);
            statusContent.Add(lblStatusHeading);
            lblStatusHeading.Position = new Point(statusTextX, 14);
            lblStatusHeading.Init(Label.LabelType.CRTNormal); // Label.LabelType.CRTGlow);
            lblStatusHeading.Width = statusTextWidth;
            lblStatusHeading.Height = 24;
            lblStatusHeading.DebugTag = "CRTStatusHeader";

            lblStatusInfo1 = new Label(The.InGameUI.gui);
            statusContent.Add(lblStatusInfo1);
            lblStatusInfo1.Position = new Point(statusTextX, 80);
            lblStatusInfo1.Init(Label.LabelType.CRTSmall);
            lblStatusInfo1.Width = statusTextWidth;

            lblStatusInfo2 = new Label(The.InGameUI.gui);
            statusContent.Add(lblStatusInfo2);
            lblStatusInfo2.Position = new Point(statusTextX, 100);
            lblStatusInfo2.Init(Label.LabelType.CRTSmall);
            lblStatusInfo2.Width = statusTextWidth;

            return statusContent;
        }*/

        public UIComponent GetNewSurfaceContent()
        {
            GUIManager gui = The.InGameUI.gui;

            UIComponent crtContent = new UIComponent(The.InGameUI.gui);
            crtContent.Width = crtAnimator.SurfacePanel.Width;
            crtContent.Height = crtAnimator.SurfacePanel.Height;
            crtContent.RenderType = RenderType.CRTAndLCD;

            Image noise = new Image(The.InGameUI.gui);
            Rectangle rect = gui.GUI_CRT_SpriteSheet.GetSourceRectangle("EmptyBG_Dark");
            noise.SetSkinLocation(SkinState.Normal,rect);
            noise.Texture = gui.GUI_CRT_SpriteSheet.Texture;
            crtContent.Add(noise);
            noise.Position = new Point(0, 0);
            noise.Width = crtContent.Width;
            noise.Height = crtContent.Height;
            noise.ScaleImageToSizeOfControl = true;
            noise.RenderType = RenderType.CRTAndLCD;

            return crtContent;
        }



        public void ChangeContent(UIComponent content)
        {
            crtAnimator.ChangeContent(content);
                       

            //UpdateActionButton();
        }

        //private Regulator actionRegulator = new Regulator(1);

        public void Update(GameTime gameTime)
        {
            crtAnimator.Update(gameTime);

           /*
            if (actionRegulator.IsReady())
            {
                UpdateActionButton();
            }*/
        }


        public void Refresh()
        {
            //UpdateActionButton();
        }
        

        /*private void UpdateActionButton()
        {
            bool canBeSalvaged, canBeHunted, hasSpecialActions;
            IKnownEntityData entityData;

            if (The.Sim.Mode == Sim.EngineMode.Game
                && HUDEntityActionPanel.EntityHasActions(out canBeSalvaged, out canBeHunted, out hasSpecialActions, out entityData,The.InGameUI.SelectedEntity))
            {
                action.Enabled = true;
                action.ToolTip = actionTooltip;
                lblAction.Color = Label.OffWhiteColor;
            }
            else
            {
                action.Enabled = false;
                action.ToolTip = "No actions possible.";
                lblAction.Color = Color.Black;
            }
        }*/

       

        void center_Click(UIComponent sender, EventArgs e)
        {           
            The.InGameUI.EnableTracking(false);
            if (The.InGameUI.SelectedEntity != null)
            {
                IKnownEntityData data;
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out data);
                if (data != null)
                {
                    The.InGameUI.ZoomToEntity(data);
                }
            }
        }

        void center_RightClick(UIComponent sender, EventArgs e)
        {
            if (The.InGameUI.SelectedEntity != null)
            {
                IKnownEntityData data;
                The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out data);
                if (data != null)
                {
                    The.InGameUI.ZoomToEntity(data);
                    The.InGameUI.EnableTracking(!The.InGameUI.TrackSelectedEntity);
                }
            }
            //intf.TrackSelectedEntity = !intf.TrackSelectedEntity;
        }

        public static void AddCRTPlasticFrame(GUIManager gui, Window form, Point crtPos, int crtWidth, int crtHeight, out Box plasticEdge)
        {
            plasticEdge = new Box(gui);       
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("TV_plastic_edge");
            plasticEdge.SetSkinLocation(SkinState.Normal,rect);
            plasticEdge.CornerSize = 30; //MP me screwing around. was 15
            plasticEdge.Position = new Point(crtPos.X - CRTPlasticEdgeCenter, crtPos.Y - CRTPlasticEdgeCenter); //new Point(plastic.X + 10, plastic.Y + 10);
            plasticEdge.Width = (int)(crtWidth + 2 * CRTPlasticEdgeCenter); //(int)(plastic.Width + 2 * plasticEdgeCenter); //(int)(plastic.Width - 20);
            plasticEdge.Height = crtHeight + 2 * CRTPlasticEdgeCenter; // 2 * 11
            plasticEdge.RenderType = RenderType.Overlay;
            form.Add(plasticEdge);
            plasticEdge.DebugTag = "TV_plastic_edge";
          
        }

        public static int GetPlasticFrameHeight()
        {
            return CRTPlasticEdgeCenter * 2;
        }

        public static int GetPlasticFrameWidth()
        {
            return CRTPlasticEdgeCenter * 2;
        }

        public static void action_Click(UIComponent sender, EventArgs e)
        {
            if (The.InGameUI.SelectedEntity.HasValue)
            {
                The.InGameUI.HUDActionPanel.ShowInScreenSpace(sender.AbsolutePosition.X - The.InGameUI.HUDActionPanel.DisplayWindow.Width,
                   sender.AbsolutePosition.Y, false);
            }
        }

        

        public void ShowCenterButton(bool show)//ClickHandler leftClick, ClickHandler rightClick) ///*string toolTip,*/ ClickHandler leftClick, ClickHandler rightClick)
        {
            btTrack.Visible = show;

           // DisplayWindow.Add(btTrack);


        }

        public void Show()
        {
            centerWindow.Show();

            DisplayWindow.Show();
        }

        public void Hide()
        {
           
            centerWindow.Hide();

            DisplayWindow.Hide();
        }

       /* public void HideCenterButton() 
        {
            DisplayWindow.Remove(btTrack);
         
        }*/

       /* public void ShowNextButton(string toolTip, ClickHandler onNextClick)
        {
            DisplayWindow.Add(btNext);
         

            btNext.ToolTip = toolTip;

            btNext.Click += onNextClick;

        }

        public void HideNextButton(ClickHandler onNextClick)
        {
            DisplayWindow.Remove(btNext);
            DisplayWindow.Remove(lblNext);


            btNext.Click -= onNextClick;
        }*/

        //public void ShowPreviousButton(string toolTip, ClickHandler onPreviousClick)
        //{
        //    DisplayWindow.Add(btPrevious);
           
        //    /*DisplayWindow.Add(lblPrevious);
        //    lblPrevious.Init(Label.LabelType.PlainPanelNormal);
        //    */

        //    btPrevious.ToolTip = toolTip;

        //    btPrevious.Click += onPreviousClick;

        //}

        //public void HidePreviousButton(ClickHandler onPreviousClick)
        //{
        //    DisplayWindow.Remove(btPrevious);
        //    DisplayWindow.Remove(lblPrevious);

        //    btPrevious.Click -= onPreviousClick;
        //}

      

        public void TurnOn()
        {

            The.InGameUI.gui.PlaySound(GUIManager.CRTTurnOn);

            crtAnimator.TurnOn();

            /*
            animationControl.Player.StartAnimation(The.InGameUI.framedCRT.CRTNoise.turnOn);
            // make sure we are notified when the animation ends:
            animationControl.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(TurnOnAnimFinished);
            */
        }

        public void TurnOff()
        {
            crtAnimator.TurnOff();

            /*
            animationControl.Player.StartAnimation(The.InGameUI.framedCRT.CRTNoise.turnOff);
            // make sure we are notified when the animation ends:
            animationControl.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(TurnOffAnimFinished); 
            */
        }

      /*  void TurnOffAnimFinished()
        {
            isOn = false;
            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(TurnOffAnimFinished); 
        }

        void TurnOnAnimFinished()
        {
            isOn = true;
            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(TurnOnAnimFinished);
        }*/

        public void Switch()
        {
            crtAnimator.Switch();
        }

    /*    public void Switch()
        {
            int random = Globals.Instance.Random.Next(10);
            if (random <= 1)
            {   // play the long one about 20% of the time...
                longShakeIsPlaying = true;

                The.InGameUI.gui.PlaySound(GUIManager.WhiteNoise);      
            }
            else if (random <= 3)
            {
                shortShakeIsPlaying = true;
            }
            else 
            {
                microSwitchIsPlaying = true;
            }

           // return; // !!!

            animationControl.StartAnimation(switchChannelBlackFrameSmall);
            animationControl.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(SwitchChannelBlackFrameAnimationEnded);
        }

        void SwitchChannelBlackFrameAnimationEnded()
        {            
           // SurfacePanel.Y += SurfacePanel.Height / 4;
            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(SwitchChannelBlackFrameAnimationEnded);

            if (longShakeIsPlaying)
            {   
                animationControl.StartAnimation(switchChannelSmall);
            }
            else if (shortShakeIsPlaying)
            {
                animationControl.StartAnimation(interferenceSmall);
            }
            else
            {
                animationControl.StartAnimation(microSwitch);
            }

            animationControl.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(SwitchChannelAnimationEnded);

        }

        void SwitchChannelAnimationEnded()
        {
            SetSurfacePanelPosition();
            longShakeIsPlaying = false;
            shortShakeIsPlaying = false;
            microSwitchIsPlaying = false;

            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(SwitchChannelAnimationEnded);
            // todo: start new looping anim:
            // animationControl.Reset();
        }*/

      

       /* void ExpandEntityPanel_OnPress(object sender, EventArgs e)
        {
            The.InGameUI.ExpandOrCollapse(); 
           
        }*/



      /*  private void InitDisplay(GUIManager gui, Game game)
        {
            SurfacePanel = new UIComponent(gui);
            DisplayWindow.Add(SurfacePanel);
            SetSurfacePanelPosition();
            SurfacePanel.Width = plasticEdge.Width;
            SurfacePanel.Height = plasticEdge.Height - 6;
            SurfacePanel.RenderType = RenderType.CRTAndLCD;
            SurfacePanel.DebugTag = "statusSurface";            

            animationControl = new AnimatedImage(gui);
            DisplayWindow.Add(animationControl);            
            animationControl.Texture = gui.GUI_CRT_SpriteSheet.Texture; // Important
            animationControl.Position = SurfacePanel.Position;
            animationControl.ScaleImageToSizeOfControl = true;
            animationControl.Width = SurfacePanel.Width;
            animationControl.Height = SurfacePanel.Height;
            animationControl.RenderType = RenderType.CRTAndLCD;
        
                

        }*/

       /* private void SetSurfacePanelPosition()
        {
            SurfacePanel.Position = new Point(plasticEdge.X, plasticEdge.Y);
        }*/

        /// <summary>
        /// tile the rubber image
        /// </summary>
        /// <param name="gui"></param>
        /// <param name="game"></param>
        private void DrawRubber(GUIManager gui, Game game)
        {
            int x = 0;
            int y = 0;

            Image rubber;
            Rectangle rect = gui.GUISpriteSheet.GetSourceRectangle("TV_rubber_texture");
            while(x < DisplayWindow.Width)
            {
                y = 0;
                while ( y < DisplayWindow.Height)
                {
                    rubber = new Image(gui);                    
                    rubber.SetSkinLocation(SkinState.Normal,rect);
                    rubber.Alpha = 0.2f; // 0.36f;
                    DisplayWindow.Add(rubber);
                    rubber.Position = new Point(x, y);
                    rubber.ResizeControlToFitImage();
                    y += rect.Height;
                }

                x += rect.Width;                
            }
            
            
        }



        public void SetCenterButtonChecked(bool enable)
        {
            btTrack.IsChecked = enable;
           
        }
    }

    public struct PositionAtTime
    {
        public float YPositionFraction;
        public float TimeFraction;

    }
}
