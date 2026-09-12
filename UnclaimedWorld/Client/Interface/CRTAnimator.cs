using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.Control;
 

namespace UWGame.ClientSide.Interface
{
    public class CRTAnimator
    {
        public UIComponent SurfacePanel;
        AnimatedImage animationControl;

        CRTScreen crtScreen;

      //  UIComponent edgeControl;

        Point absolutePosition, relativePosition;
        int width, height;

        private bool isOn = false;

        private bool microSwitchIsPlaying = false;

        private bool longShakeIsPlaying = false;

        public Animation2D switchChannelSmall, switchChannelBlackFrameSmall, interferenceSmall, microSwitch, NoReceptionSmall;

        private static float[] longShakeEdges = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f };
        private static float[] longShakeYPositions = new float[] { 0.25f, -0.1f, 0.2f, 0.1f, 0.3f, 0f, 0.2f, -0.1f, 0f };


        private bool shortShakeIsPlaying = false;

        private static float[] shortShakeEdges = new float[] { 0.1f, 0.5f, 0.8f };
        private static float[] shortShakeYPositions = new float[] { 0.25f, -0.15f, 0f };

        Window DisplayWindow;

       // float alpha;
        
        public CRTAnimator(Window window, Point absolutePosition, Point relativePosition, int width, int height,/* UIComponent edge,*/ ReflectionToUse reflection, float alpha = 1f)
        {
           // edgeControl = edge;
            this.absolutePosition = absolutePosition;
            this.relativePosition = relativePosition;
            this.width = width;
            this.height = height;

            DisplayWindow = window;

            InitDisplay(reflection, alpha);

            InitAnims();

        }

        public bool IsOn
        {
            get
            {
                return isOn;
            }
        }

        private void InitDisplay(ReflectionToUse reflection, float alpha)
        {
            GUIManager gui = The.InGameUI.gui;
            Game game = The.Sim.Controller.Game;

            SurfacePanel = new UIComponent(gui);
            DisplayWindow.Add(SurfacePanel);
            SetSurfacePanelPosition();
          //  SurfacePanel.Width = edgeControl.Width; // width;
           // SurfacePanel.Height = edgeControl.Height - 6; // height - 6;
            SurfacePanel.Width = width;
            SurfacePanel.Height = height - 6;
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
            animationControl.DebugTag = "animationControl";
          

            crtScreen = The.InGameUI.DisplayPanelRenderer.AddCRT(SurfacePanel,
                    new Point(absolutePosition.X + 4, absolutePosition.Y + 4),
                    width - 8, height - 8, DisplayWindow.Level, DisplayWindow,
                    reflection, true, alpha);
            
           /*  if (The.Sim.ScreenManager.GraphicsLevelSetting == GameStateManagement.ScreenManager.GraphicsLevel.High)
            {
               crtScreen = The.InGameUI.DisplayPanelRenderer.AddCRT(SurfacePanel,
                    new Point(edgeControl.AbsolutePosition.X + 4, edgeControl.AbsolutePosition.Y + 4),
                    edgeControl.Width - 8, edgeControl.Height - 8, DisplayWindow.Level, DisplayWindow,
                    reflection, true, alpha);

            }*/
        }

        private void InitAnims()
        {

            /*   turnOnSmall = new Animation2D(gui.GUISpriteSheet.Texture, 0.25f, false);
               turnOnSmall.Cells.Add(new Cell(gui.GUISpriteSheet.SourceRectangle("CRT-turnon-frame1_small")));
               turnOnSmall.Cells.Add(new Cell(gui.GUISpriteSheet.SourceRectangle("CRT-turnon-frame2_small")) );
               turnOnSmall.Cells.Add(new Cell(gui.GUISpriteSheet.SourceRectangle("CRT-turnon-frame3_small")) );
               turnOnSmall.Cells.Add(new Cell(gui.GUISpriteSheet.SourceRectangle("CRT-turnon-frame3_small")) { Color = Animation2D.transp });
            
               turnOffSmall = new Animation2D(gui.GUISpriteSheet.Texture, 0.1f, false);
               turnOffSmall.Cells.Add(new Cell(gui.GUISpriteSheet.SourceRectangle("CRT-turnon-frame3_small")) { Color = Animation2D.halfTransp });
               turnOffSmall.Cells.Add(new Cell(gui.GUISpriteSheet.SourceRectangle("CRT-turnon-frame2_small")) { Color = Animation2D.halfTransp });
               turnOffSmall.Cells.Add(new Cell(gui.GUISpriteSheet.SourceRectangle("CRT-turnon-frame1_small")));
   */
            GUIManager gui = The.InGameUI.gui;

            switchChannelSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);

            // xna 3:
            /*  switchChannelBlackFrameSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);
              switchChannelBlackFrameSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame1_small")) { Color = new Color(0f, 0f, 0f, 1f) });

              microSwitch = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.05f, false);
              microSwitch.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame5_small")) { Color = new Color(1f, 1f, 1f, 0.1f) });
              microSwitch.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame6_small")) { Color = new Color(1f, 1f, 1f, 0f) });


              switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame1_small")));
              switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame2_small")));
              switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame3_small")));
              switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame3_small")) { Color = new Color(0f, 0f, 0f, 1f) }); // NEW!
              switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame4_small")) { Color = new Color(1f, 1f, 1f, 0.67f) });
              switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame5_small")) { Color = new Color(1f, 1f, 1f, 0.5f) });
              switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame6_small")) { Color = new Color(1f, 1f, 1f, 0.27f) });
              switchChannelSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame6_small")) { Color = new Color(1f, 1f, 1f, 0f) });

              interferenceSmall = new Animation2D(gui.GUI_CRT_SpriteSheet.Texture, 0.08f, false);
              interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame4_small")) { Color = new Color(1f, 1f, 1f, 0.67f) });
              interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame5_small")) { Color = new Color(1f, 1f, 1f, 0.5f) });
              interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame6_small")) { Color = new Color(1f, 1f, 1f, 0.27f) });
              interferenceSmall.Cells.Add(new Cell(gui.GUI_CRT_SpriteSheet.SourceRectangle("switch_frame6_small")) { Color = new Color(1f, 1f, 1f, 0f) });

              */

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



        }

        public void ChangeContent(UIComponent content)
        {
            if (!SurfacePanel.Controls.Contains(content))
            {
                for (int i = 0; i < SurfacePanel.Controls.Count; i++)
                {
                    if (SurfacePanel.Controls[i] != animationControl)
                    {
                        SurfacePanel.Controls.Remove(SurfacePanel.Controls[i]);
                        i--;
                    }
                }
                //SurfacePanel.Controls.Clear();

                SurfacePanel.Add(content);
            }

        }


        public void Update(GameTime gameTime)
        {
            // TODO: it is wrong to move SurfacePanel 
            // the CRT screen appears to move outside its frame...
            // - it should be the contents that move...
            if (longShakeIsPlaying || shortShakeIsPlaying)
            {
                float progress = animationControl.Player.GetProgress();
                float YPosFraction = 0f;

                if (longShakeIsPlaying)
                {

                    int index = Common.GetStairStepIndex(progress, longShakeEdges);
                    YPosFraction = longShakeYPositions[index];
                }
                else if (shortShakeIsPlaying)
                {

                    int index = Common.GetStairStepIndex(progress, shortShakeEdges);
                    YPosFraction = shortShakeYPositions[index];
                }

                // only move the contents!
                SetSurfaceYPosition((int)(YPosFraction * SurfacePanel.Height));
                
            }
         /*   else
            {
                UIComponent content = SurfacePanel.Controls[0];
                content.Y = 0;
            }*/
            
        }

        private void SetSurfaceYPosition(int position)
        {
            UIComponent content = SurfacePanel.Controls[0];
            content.Y = position;
        }

        private void SetSurfacePanelPosition()
        {          
            SurfacePanel.Position = relativePosition;         
        }

        public void TurnOn()
        {

          //  The.InGameUI.gui.PlaySound(GUIManager.CRTTurnOn);

            animationControl.Player.StartAnimation(The.InGameUI.framedCRT.CRTNoise.turnOn);
            // make sure we are notified when the animation ends:
            animationControl.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(TurnOnAnimFinished);

        }

        public void TurnOff()
        {
            // SurfacePanel.Add(animationControl);

            animationControl.Player.StartAnimation(The.InGameUI.framedCRT.CRTNoise.turnOff);
            // make sure we are notified when the animation ends:
            animationControl.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(TurnOffAnimFinished);

        }

        void TurnOffAnimFinished()
        {
            isOn = false;
            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(TurnOffAnimFinished);
        }

        void TurnOnAnimFinished()
        {
            isOn = true;
            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(TurnOnAnimFinished);
        }

        public void Switch()
        {
            int random = The.Client.ClientRandomGenerator.Next(10, "CRTAnimator", false);

          
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

            SetSurfaceYPosition(0);
               
            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(SwitchChannelAnimationEnded);
            // todo: start new looping anim:
            // animationControl.Reset();
        }

    }
}
