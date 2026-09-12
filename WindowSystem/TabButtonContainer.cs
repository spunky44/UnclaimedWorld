using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WindowSystem
{
    public class TabButtonContainer
    {
        public ImageButton button;

        public Label label;

        public Image markings;


        /// <summary>
        /// markingsflavour 2 is for long text.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="buttonLeft"></param>
        /// <param name="buttonTop"></param>
        /// <param name="flavour"></param>
        /// <param name="markingsFlavour"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public ImageButton AddHorizontalMetalPanelButton(GUIManager gui, Window window, int markingsLeft /*int buttonLeft*/, int markingsTop /*int buttonTop*/, int flavour, int markingsFlavour, string text)
        {

            Rectangle rect;
          //  int markingsLeft = buttonLeft - 7;
         //   int markingsTop = buttonTop - 9;
            markings = new Image(gui);
            window.Add(markings);
            markings.Position = new Point(markingsLeft, markingsTop);
            rect = gui.GUISpriteSheet.GetSourceRectangle(string.Format("metal_markings_{0}horiz", markingsFlavour));
            markings.SetSkinLocation(SkinState.Normal,rect);
            markings.ResizeControlToFitImage();
            label = new Label(gui);
            window.Add(label);
            label.Text = text;
            label.Init(Label.LabelType.PlainPanelNormal);
            label.Position = new Point(markingsLeft + (markings.Width - label.Width) / 2, markingsTop - 5);

            int buttonLeft = markingsLeft + 7;
            int buttonTop = markingsTop + 9;
            button = new ImageButton(gui);
            window.Add(button);
            button.Position = new Point(buttonLeft, buttonTop);
            button.Init(ImageButtonType.MetalPanelHorizontal, flavour);

            return button;
        }


        /// <summary>
        /// markingsflavour 2 is for long text.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="buttonLeft"></param>
        /// <param name="buttonTop"></param>
        /// <param name="flavour"></param>
        /// <param name="markingsFlavour"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public ImageButton AddMetalPanelButton(GUIManager gui, Window window, int buttonLeft, int buttonTop, int flavour, int markingsFlavour, string text)
        {

            Rectangle rect;
            int markingsLeft = buttonLeft - 6;
            int markingsTop = buttonTop - 15;
            markings = new Image(gui);
            window.Add(markings);
            markings.Position = new Point(markingsLeft, markingsTop);
            rect = gui.GUISpriteSheet.GetSourceRectangle(string.Format("metal_markings_{0}", markingsFlavour));
            markings.SetSkinLocation(SkinState.Normal,rect);
            markings.ResizeControlToFitImage();
            label = new Label(gui);
            window.Add(label);
            label.Text = text;
            label.Init(Label.LabelType.PlainPanelNormal);
            label.Position = new Point(markingsLeft + (markings.Width - label.Width) / 2, markingsTop - 5);

            button = new ImageButton(gui);
            window.Add(button);
            button.Position = new Point(buttonLeft, buttonTop);
            button.Init(ImageButtonType.MetalPanel, flavour);

            return button;
        }
    }
}
