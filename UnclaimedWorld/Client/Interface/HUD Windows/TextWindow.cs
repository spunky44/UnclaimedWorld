using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.Interface.HUD_Windows;
using WindowSystem;

namespace UWGame.Client.Interface.HUD_Windows
{
    public class TextWindow : HUDWindow
    {
        private Label text;

        int borderMargin = 4;
        public TextWindow(bool hasSurface)
            : base(0, 0, hasSurface)
        {
            text = new Label(gui);
            text.Init(Label.LabelType.HUDWindow);
            DisplayWindow.Height = text.Height + borderMargin * 2;
            DisplayWindow.CenterChildVertically(text);
            Add(text);
        }
        public string Text
        {
            get
            {
                return text.Text;
            }
            set
            {
                text.Text = value;
                DisplayWindow.Width = text.Width + borderMargin * 2;
                DisplayWindow.CenterChildHorizontally(text);
            }
        }
        public override void Update(Microsoft.Xna.Framework.GameTime elapsed)
        {
            base.Update(elapsed); 
        }
        public override void Hide()
        {
            
        }
    }
}
