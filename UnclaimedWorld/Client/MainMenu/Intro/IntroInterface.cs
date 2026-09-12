using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameStateManagement;
using UWGame.ClientSide.Interface;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.MainMenu.Intro
{
    public class IntroInterface: CommonInterface
    {

        public FramedCRT framedCRT;
        int framedCRTWidth = (int)((4f / 3f) * 500f);
        int framedCRTHeight = 500;

        int frameLeft, frameTop;

     //   IntroPanel introPanel;

        private UIComponent crtContent;
        TextArea taText;

        public IntroInterface(UnclaimedWorld game)
            : base(game)
        {

            // center the frame:
            frameLeft = (game.GraphicsDeviceManager.PreferredBackBufferWidth - framedCRTWidth) / 2;
            frameTop = (game.GraphicsDeviceManager.PreferredBackBufferHeight - framedCRTHeight) / 2;

            Rectangle directSource = new Rectangle(frameLeft, frameTop, framedCRTWidth, framedCRTHeight);

            framedCRT = new FramedCRT(this, new Rectangle(frameLeft, frameTop, framedCRTWidth, framedCRTHeight),
                                                directSource, Level.Middle);
            
            framedCRT.crtTextAnimatorCharacter.TimeBetweenUpdates = 0.2f;

            crtContent = framedCRT.GetNewSurfaceContent();

            taText = new TextArea(gui, ListBoxType.Main);
            crtContent.Add(taText);
           // pnStructure.Add(taStructureDescription);
            taText.Position = new Point(16, 80);
            taText.Width = crtContent.Width;
            taText.Height = crtContent.Height - taText.Position.Y;
            taText.HMargin = 0;
         
           // taText.Font = GUIManager.CRTSmallFontPath;
         //   taText.Color = Label.CRTLightBlue;
            taText.AnimateOnCRTScreen = Label.AnimationMode.Line; // true;


            taText.Text = "entity.EntityType.Description.ToUpper() entity.EntityType.Description.ToUpper() entity.EntityType.Description.ToUpper() entity.EntityType.Description.ToUpper()";

         //   introPanel = new IntroPanel(this);


        }


       


        public void StartMovie()
        {
            framedCRT.ChangeContent(crtContent);

        }

    }
}
