using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Editor.MapTools
{
    public class AlphaOption : ToolOption
    {

        public override int Order
        {
            get 
            { 
                return 2; 
            }
        }

        AlphaSetting setting;

        FillableBar fbToolAlpha;

        public AlphaOption(GUIManager gui)
            : base(gui)
        {

            Label lbl = new Label(gui);
            lbl.Init(Label.LabelType.LCDNormal);
            Add(lbl);
            lbl.Text = "ALPHA";
            lbl.FitToText();
            // lbl.Y = ypos;

            fbToolAlpha = new FillableBar(gui, FillableBar.FillableBarType.LCDSlider, false, false);
            Add(fbToolAlpha);
            fbToolAlpha.X = 60;
            // fbToolSize.Y = ypos;
            fbToolAlpha.Width = 160;
            fbToolAlpha.MaxValue = 100;
            fbToolAlpha.ShowMaxValueLabelAtEnd = false;
            fbToolAlpha.ShowNotches = false;
            fbToolAlpha.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Never;
            fbToolAlpha.SliderMouseUp += fbTool_SliderMouseUp;

        }

        void fbTool_SliderMouseUp(object sender, EventArgs e)
        {
            setting.Value = fbToolAlpha.Value / 100f;
        }

        public void Set(AlphaSetting setting)
        {
            this.setting = setting;

            fbToolAlpha.Value = (int)(100f * setting.Value);
            fbToolAlpha.UpdateSliderPosition();
        }
    }
}
