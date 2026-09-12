using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.HUD_Windows;
using WindowSystem;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Entities;
using GameStateManagement;
using UWGame.Control;

namespace UWGame.ClientSide.Interface
{
    /// <summary>
    /// has a plastic panel part and an expandable HUD panel part
    /// </summary>
    public class TalkPanel
    {
        public HUDTalkPanel HUDTalkPanel;

        Window plasticPanel;
        Window crtWindow;
     
        Image faceImage;
       
        CRTAnimator crtAnimator;
        UIComponent crtContent;

        const double timeToShowFaceMean = 6d;

        double timeLeftToShowFace = 0d;

        public TalkPanel()
        {
            int height = 10;

            int hudWidth = 248;
            int hudLeft = 13; // 10;
            int hudHeight = 160;

            plasticPanel = new Window(The.InGameUI.gui);
            plasticPanel.Skin = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("twitterpanel_frame"); //"event_displaypanel"); 
            plasticPanel.CornerSize = 31;
            plasticPanel.Margin = 0;
            plasticPanel.IsMovable = false;
            plasticPanel.Resizable = false;
            plasticPanel.HasCloseButton = false;
            plasticPanel.Position = new Point(0, (The.Client.ScreenHeight / 2) - (hudHeight) - 50); // dimensions.Top);
            plasticPanel.WindowSize = new Vector2(32, 370);
            plasticPanel.Level = Level.Bottom; // .RockBottom;
            plasticPanel.Show();

            // a transparent window to hold the CRT screen:
            crtWindow = new Window(The.InGameUI.gui);
          //  crtWindow.Skin = The.InGameUI.gui.GUISpriteSheet.GetSourceRectangle("event_displaypanel"); //"event_base_small2");
            crtWindow.CornerSize = 7;
            crtWindow.Margin = 0;
            crtWindow.IsMovable = false;
            crtWindow.Resizable = false;
            crtWindow.HasCloseButton = false;
            crtWindow.Position = new Point(hudLeft + 3, plasticPanel.Y + 28); 
            crtWindow.WindowSize = new Vector2(hudWidth - 6, hudHeight);
            crtWindow.Level = Level.Bottom;
            crtWindow.HasCRTOrLCDComponents = true;
            crtWindow.Show();


            HUDTalkPanel = new HUDTalkPanel(hudLeft, plasticPanel.Y + 184, hudWidth, hudHeight);

            //*** CRT ****
          /*  faceImage = new WindowSystem.Image(The.InGameUI.gui);
            crtWindow.Add(faceImage);
            faceImage.RenderType = RenderType.CRTAndLCD;
            faceImage.ScaleImageToSizeOfControl = true;
            faceImage.Width = hudWidth;
            faceImage.Height = hudHeight;
            faceImage.X = 0;
            faceImage.Y = 0;*/


            crtAnimator = new CRTAnimator(crtWindow, /*faceImage,*/ crtWindow.AbsolutePosition, Point.Zero, crtWindow.Width, crtWindow.Height, ReflectionToUse.None, 0.8f);

          /*  crtScreen = The.InGameUI.DisplayPanelRenderer.AddCRT(faceImage, new Point(faceImage.AbsolutePosition.X + 2,
                  faceImage.AbsolutePosition.Y + 2),
                  faceImage.Width - 4, faceImage.Height - 4, 
                  crtWindow.Level, crtWindow,
                  ReflectionToUse.None, true, 0.8f);
            */

       /*     animationControl = new AnimatedImage(gui);
            crtWindow.Add(animationControl);
            animationControl.Texture = gui.GUI_CRT_SpriteSheet.Texture; // Important
            animationControl.Position = SurfacePanel.Position;
            animationControl.ScaleImageToSizeOfControl = true;
            animationControl.Width = SurfacePanel.Width;
            animationControl.Height = SurfacePanel.Height;
            animationControl.RenderType = RenderType.CRTAndLCD;*/


            GUIManager gui = The.InGameUI.gui;

            crtContent = new UIComponent(The.InGameUI.gui);
            crtContent.Width = crtAnimator.SurfacePanel.Width;
            crtContent.Height = crtAnimator.SurfacePanel.Height;
            crtContent.RenderType = RenderType.CRTAndLCD;

            faceImage = new WindowSystem.Image(The.InGameUI.gui);
        //    crtContent.Add(faceImage);
            faceImage.Texture = The.InGameUI.gui.GUI_CRT_SpriteSheet.Texture;
            faceImage.RenderType = RenderType.CRTAndLCD;
            faceImage.ScaleImageToSizeOfControl = true;
            faceImage.Width = hudWidth;
            faceImage.Height = hudHeight;
            faceImage.X = 0;
            faceImage.Y = 0; 

            crtAnimator.ChangeContent(crtContent);

        }


        public void Update(GameTime gameTime)
        {
            crtAnimator.Update(gameTime);

            if (timeLeftToShowFace > 0)
            {
                timeLeftToShowFace -= gameTime.ElapsedGameTime.TotalSeconds;

                if (timeLeftToShowFace <= 0)
                {
                    timeLeftToShowFace = 0;
                    crtAnimator.Switch();
                    crtContent.Remove(faceImage);
                }
            }
        }

        public void ShowSpeaker(TalkEvent talkEvent)
        {           
            Entity speaker = Entity.FindByID(talkEvent.SpokenBy);
            if (speaker != null)
            {
                crtAnimator.Switch();

                faceImage.SetSkinLocation(SkinState.Normal,speaker.PersonEntity.GetPortraitForTalkDisplay(The.InGameUI.gui), null, null, true);

                crtContent.Add(faceImage);

                // after this delay the face image will come on:
                timeLeftToShowFace = The.Client.ClientRandomGenerator.RandomNormalDistribution(timeToShowFaceMean, 0.4);
            }
        }

        public void Hide()
        {
            plasticPanel.Hide();
            HUDTalkPanel.Hide();
            crtWindow.Hide();
        }
    }
}
