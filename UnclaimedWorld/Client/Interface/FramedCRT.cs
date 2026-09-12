using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Renderables;
using UWGame.ClientSide.Map;
using UWGame.ClientSide;
using UWGame.Control;

namespace UWGame.ClientSide.Interface  
{
    public class FramedCRT
    {
        public enum State { Off, TurningOn, SwitchingOff, On, ChangingChannel }

        private State state;

        public CRTScreen CRTScreen;
        Box frame, frameDropShadow;

        public Window DisplayWindow;

        public Window cablesWindow;

        public CRTTextCharAnimator crtTextAnimatorCharacter;
        public CRTTextLineAnimator crtTextAnimatorLine;

        private UIComponent SurfacePanel;

        public Rectangle SurfaceRect;

        private AnimatedImage animationControl;

        private int edgeWidth = 37; //39;

        public Image backgroundNoise;

      //  public Animation2DPlayer Player;
        
        private float timeBetweenInterference;
        private float timeBetweenInterferencePassed;

        public CRTNoise CRTNoise;

        private CommonInterface intf;

        public bool ShowCables = true;

        public FramedCRT(CommonInterface intf, Rectangle dimension, Rectangle source, Level level) // Point position, Point dimension)
        {
            this.intf = intf;
            GUIManager gui = intf.gui;
          /*  
            Game game = UWGame.SimSide.Instance.ScreenManager.Game;

            Interface intf = intf;*/

            this.CRTNoise = new CRTNoise(intf.gui);

            //Player = new Animation2DPlayer();



            SurfaceRect = new Rectangle(source.X + edgeWidth, source.Y + edgeWidth, source.Width - 2 * edgeWidth, source.Height - 2 * edgeWidth);
            Rectangle croppedDestination = new Rectangle(dimension.X + edgeWidth, dimension.Y + edgeWidth, dimension.Width - 2 * edgeWidth, dimension.Height - 2 * edgeWidth);


            Rectangle rect;

            DisplayWindow = new Window(gui);
            // Sequence matters for skins!!!
            DisplayWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle("CRT_Frame_Scalable");
            DisplayWindow.CornerSize = 60;
            DisplayWindow.Margin = 0; // 7;
            DisplayWindow.Resizable = false;
            DisplayWindow.IsMovable = false;
            DisplayWindow.Level = level;
            DisplayWindow.Position = new Point(dimension.X, dimension.Y);
            DisplayWindow.WindowSize = new Vector2(dimension.Width, dimension.Height);  //new Vector2(screenDimensions.Width + 90, screenDimensions.Height + 40);
            DisplayWindow.HasCloseButton = false;
            DisplayWindow.HasCRTOrLCDComponents = true;
            DisplayWindow.HasOverlayComponents = true;
            DisplayWindow.ShowPanel = false;

            DisplayWindow.DebugTag = "crtWindow";

            int screenWidth = dimension.Width - 2 * edgeWidth;
            int screenHeight = dimension.Height - 2 * edgeWidth;

            /*rect = gui.GUISpriteSheet.GetSourceRectangle("CRTpopup_cable");
            cablesWindow = InGameInterface.CreateBackgroundWindow(gui, rect, new Point(DisplayWindow.X + DisplayWindow.Width - 13, DisplayWindow.Y + 90));
            cablesWindow.Hide();*/

            SurfacePanel = new UIComponent(intf.gui);
            DisplayWindow.Add(SurfacePanel); 
            SurfacePanel.ClipThis = false;
            SurfacePanel.Position = new Point(SurfaceRect.X - DisplayWindow.AbsolutePosition.X, SurfaceRect.Y - DisplayWindow.AbsolutePosition.Y); 
            SurfacePanel.Width = SurfaceRect.Width; 
            SurfacePanel.Height = SurfaceRect.Height;   
            SurfacePanel.RenderType = RenderType.CRTAndLCD;

            animationControl = new AnimatedImage(intf.gui);
            DisplayWindow.Add(animationControl);
            animationControl.Texture = intf.gui.GUI_CRT_SpriteSheet.Texture; // Important
            animationControl.RenderType = RenderType.CRTAndLCD;
            animationControl.ClipThis = false;
            animationControl.ScaleImageToSizeOfControl = true;
            animationControl.Width = SurfacePanel.Width;
            animationControl.Height = SurfacePanel.Height;
            animationControl.Position = SurfacePanel.Position;
            
            //animationControl.RenderType = RenderType

            

         /*   frameDropShadow = new Box(gui);
            frameDropShadow.RenderType = RenderType.Overlay;
            rect = gui.GUISpriteSheet.SourceRectangle("CRT_Dropshadow_scalable");
            frameDropShadow.SetSkinLocation(SkinState.Normal,rect);
            frameDropShadow.CornerSize = 67;
            frameDropShadow.Position = new Point(6, 6);
            frameDropShadow.Width = dimension.Width;
            frameDropShadow.Height = dimension.Height;
            DisplayWindow.Add(frameDropShadow);*/

            frame = new Box(gui);
            frame.RenderType = RenderType.Overlay;
            rect = gui.GUISpriteSheet.GetSourceRectangle("CRT_Frame_Scalable_NoLight");//gui.GUISpriteSheet.SourceRectangle("CRT_Frame_Scalable");
            frame.SetSkinLocation(SkinState.Normal,rect);
            frame.CornerSize = 60;
            frame.Position = new Point(0, 0);
            frame.Width = dimension.Width;
            frame.Height = dimension.Height;
            DisplayWindow.Add(frame);
                        
            Panel.AddDustOnFrame(intf.gui, new Rectangle(frame.X, frame.Y, frame.Width, frame.Height), 37, DisplayWindow, RenderType.Overlay);

            DisplayWindow.Hide();

           // crtScreen = intf.DisplayPanelRenderer.AddModelDisplayCRT(new Point(400, 200), new Point(400, 260), 600, 400, modelDisplayWindow.Level, modelDisplayWindow);

          /*  CRTScreen = intf.DisplayPanelRenderer.AddCRT(new Point(noise.AbsolutePosition.X,
                    noise.AbsolutePosition.Y),
                    noise.Width, noise.Height, DisplayWindow.Level, DisplayWindow, true);*/


            CRTScreen = intf.DisplayPanelRenderer.AddCRT(SurfacePanel, SurfaceRect, croppedDestination, DisplayWindow.Level, DisplayWindow,
                ReflectionToUse.Big, true);

            crtTextAnimatorCharacter = new CRTTextCharAnimator(gui);
           // crtTextAnimatorCharacter.Mode = CRTTextCharAnimator.AnimationMode.Character;
            DisplayWindow.Add(crtTextAnimatorCharacter);

            crtTextAnimatorLine = new CRTTextLineAnimator(gui);
           // crtTextAnimatorLine.Mode = CRTTextAnimator.AnimationMode.Line;
            DisplayWindow.Add(crtTextAnimatorLine);
        }


      /*  public CRTTextAnimator.AnimationMode AnimationMode
        {
            set
            {
                crtTextAnimatorCharacter.Mode = value;
            }
        }*/

        public static void ClearContent(UIComponent crtContent)
        {
            for (int i = 0; i < crtContent.Controls.Count; i++)
            {
                if (crtContent.Controls[i].Name != "Noise")
                {
                    crtContent.Controls.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// The content panel is not added! Call ChangeContent() on refresh/show!
        /// </summary>
        /// <returns></returns>
        public UIComponent GetNewSurfaceContent()
        {
          //  Game game = UWGame.SimSide.Instance.ScreenManager.Game;
            UIComponent crtContent = new UIComponent(intf.gui);
            crtContent.Width = SurfaceRect.Width;
            crtContent.Height = SurfaceRect.Height;
            crtContent.RenderType = RenderType.CRTAndLCD;

            backgroundNoise = new Image(intf.gui);
            Rectangle rect = intf.gui.GUI_CRT_SpriteSheet.GetSourceRectangle("EmptyBG_Dark");
            backgroundNoise.SetSkinLocation(SkinState.Normal,rect);
            backgroundNoise.Texture = intf.gui.GUI_CRT_SpriteSheet.Texture;
            crtContent.Add(backgroundNoise); 
            backgroundNoise.Position = Point.Zero; // new Point(croppedSource.X - DisplayWindow.AbsolutePosition.X, croppedSource.Y - DisplayWindow.AbsolutePosition.Y); // new Point(edgeWidth, edgeWidth);
            backgroundNoise.Width = SurfaceRect.Width; //screenWidth;
            backgroundNoise.Height = SurfaceRect.Height; //screenHeight;
            backgroundNoise.ScaleImageToSizeOfControl = true;
            backgroundNoise.RenderType = RenderType.CRTAndLCD;
           // backgroundNoise.DebugTag = "TVBackground";
            backgroundNoise.Name = "Noise";

            // backgroundNoise.ClipThis = false; // !!!


            return crtContent;
        }

        /// <summary>
        /// change the panel shown on the screen
        /// </summary>
        /// <param name="content"></param>
        public void ChangeContent(UIComponent content)
        {
            /*if (!SurfacePanel.Controls.Contains(content))
            {*/
                SurfacePanel.Controls.Clear();
                SurfacePanel.Add(content);

                crtTextAnimatorCharacter.Clear();
                crtTextAnimatorLine.Clear();

                AddLabelsToAnimator(content);

            /* OLD
                Label label;
                foreach (UIComponent control in content.Controls)
                {
                    label = control as Label;
                    if (label != null)
                    {
                        crtTextAnimator.Add(label);
                    }
                }
            */
                
          //  }

            crtTextAnimatorCharacter.StartAnimating();
            crtTextAnimatorLine.StartAnimating();
        }

        private void AddLabelsToAnimator(UIComponent control)
        {
            Label label;
            foreach (UIComponent child in control.Controls)
            {
                label = child as Label;
                if (label != null) 
                {
                    if (label.AnimateOnCRTScreen == Label.AnimationMode.Character) //label.ID == "CRTHeading") // only animate heading?)
                    {
                        crtTextAnimatorCharacter.Add(label);
                    }
                    else if (label.AnimateOnCRTScreen == Label.AnimationMode.Line)
                    {
                        crtTextAnimatorLine.Add(label);
                    }
                }

                AddLabelsToAnimator(child);
            }
        }

      

        public void PlayInterference()
        {
            animationControl.StartAnimation(CRTNoise.interference);
        }

        public void PlayNoReception()
        {
            animationControl.StartAnimation(CRTNoise.NoReception);
        }

        public void Switch()
        {
            //SurfacePanel.Y += 20;
            NoLightOnFrame();
            animationControl.StartAnimation(CRTNoise.switchChannelBlackFrame);
            animationControl.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(SwitchChannelBlackFrameAnimationEnded);
        }

        void SwitchChannelBlackFrameAnimationEnded()
        {
            //SurfacePanel.Y -= 5;
            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(SwitchChannelBlackFrameAnimationEnded);
            
            LightOnFrame();

            if (The.Client.ClientRandomGenerator.Next(5,"FramedCRT",false) == 4)
            {   // play the long one about 20% of the time...
                animationControl.StartAnimation(CRTNoise.switchChannel);
            }
            else
            {
                animationControl.StartAnimation(CRTNoise.interference);
            }

            animationControl.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(SwitchChannelAnimationEnded);


        }

        void SwitchChannelAnimationEnded()
        {
            
            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(SwitchChannelAnimationEnded);
            // todo: start new looping anim:
           // animationControl.Reset();
        }

        public void Show()
        {
            DisplayWindow.Show();
            if (ShowCables)
            {
                cablesWindow.Show();
            }
            intf.gui.BringToBottom(cablesWindow);

            if (state == State.Off)
            {
               // SurfacePanel.Add(animationControl);

                intf.gui.PlaySound(GUIManager.CRTTurnOn); // .CRTTurnOn.Play(GUIManager.MasterSFXVolume, 0f, 0f);

                animationControl.StartAnimation(CRTNoise.turnOn);
                animationControl.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(TurnOn);
              //  player.StartAnimation(turnOn);
                // make sure we are notified when the animation ends:
             //   player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(TurnOn);
            }
        }

        

        public void TurnOn()
        {
            state = State.On;
            LightOnFrame();

         //   SurfacePanel.Remove(animationControl);

            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(TurnOn); 

        }

        private void LightOnFrame()
        {
            Rectangle rect = intf.gui.GUISpriteSheet.GetSourceRectangle("CRT_Frame_Scalable");
            frame.SetSkinLocation(SkinState.Normal,rect);
        }

        public void TurnOff()
        {
            state = State.Off;

            NoLightOnFrame();

         //   SurfacePanel.Remove(animationControl);

            animationControl.Player.AnimationEndedEvent -= new Animation2DPlayer.AnimationEnded(TurnOff);

        }

        private void NoLightOnFrame()
        {
            Rectangle rect = intf.gui.GUISpriteSheet.GetSourceRectangle("CRT_Frame_Scalable_NoLight");
            frame.SetSkinLocation(SkinState.Normal,rect);
        }

        public void Hide()
        {
            DisplayWindow.Hide();
            cablesWindow.Hide();

            if (state == State.On)
            {
                //TurnOff();
                // problem: the animation doesn't end before the window is hidden
              //  SurfacePanel.Add(animationControl);

                animationControl.Player.StartAnimation(CRTNoise.turnOff);
                // make sure we are notified when the animation ends:
                animationControl.Player.AnimationEndedEvent += new Animation2DPlayer.AnimationEnded(TurnOff);
            }

        }

        public static void RotateModel(GameTime gameTime, Entity vehicleToShow, ref float rotation, Vector3 location)
        {        
            // rotate skimmer to see it from all angles:
            rotation += 0.5f * (float)gameTime.ElapsedGameTime.TotalSeconds;

            /*   vehicleToShow.Direction = Vector3.UnitX * 1000.0f * (float)Math.Cos(0.5f * rotation) +
                              Vector3.UnitY * 1000.0f * (float)Math.Sin(0.5f * rotation)
                              - Vector3.UnitZ * 550.0f;
               vehicleToShow.Direction.Normalize();*/

            rotation = rotation % MathHelper.TwoPi;

            vehicleToShow.SetRotationAndDir(rotation); // TODO DECOUPLE, once this is a Standalone Renderable, it will need a Renderable behavior to set transforms and such

            /*
            vehicleToShow.Locomotor.Rotation = rotation;         
            vehicleToShow.Locomotor.NormalizedMoveDir = new Vector3(
                (float)Math.Cos(rotation),
                (float)Math.Sin(rotation), 0f); */

            //vehicleToShow.Vehicle.Direction.Normalize();
         //   vehicleToShow.Location = location; 
                           
        

        }

      /*     public static void DrawCRTNoiseBackground(ref Rectangle destination)
        {
         Rectangle rect = gui.GUISpriteSheet.SourceRectangle("EmptyBG_Dark");
            UWGame.SimSide.Instance.spriteBatch.Begin();
            UWGame.SimSide.Instance.spriteBatch.Draw(gui.GUISpriteSheet.Texture, destination, rect, Color.White);
            UWGame.SimSide.Instance.spriteBatch.End();

        }*/

        public static void DrawModel(Entity entityToShow, /*Dictionary<string, ModelData> allModels,*/ Matrix view)
        {
            
          //  ModelData modelData = GameData.Instance.AllModels[entityToShow.EntityType.Renderable.RenderAsModelType.ModelName];
            ModelData modelData = entityToShow.Renderable.RenderAsModel.ModelData; //??
            entityToShow.Renderable.RenderAsModel.ComputeMatricesForDrawing(AnimatedModel.Transformations.OnlyRotation, 
                modelData.CRTDisplayScale);

            entityToShow.Renderable.Draw(GameWorldRenderer.RenderTechnique.StandardMonochrome,
                ref view, ref The.Client.PerspectiveProjection, 1f, modelData.CRTDisplayLightIntensity); 
            
        }

    }
}
