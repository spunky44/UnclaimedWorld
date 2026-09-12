using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Editor.MapTools
{
    public class RadiusOption : ToolOption
    {

        public override int Order
        {
            get 
            { 
                return 1; 
            }
        }

        RadiusSetting setting;

        FillableBar fbToolSize;

        public RadiusOption(GUIManager gui): base(gui)
        {

            Label lbl = new Label(gui);
            lbl.Init(Label.LabelType.LCDNormal);
            Add(lbl);
            lbl.Text = "SIZE";
            lbl.FitToText();
            // lbl.Y = ypos;

            fbToolSize = new FillableBar(gui, FillableBar.FillableBarType.LCDSlider, false, false);
            Add(fbToolSize);
            fbToolSize.X = 60;
            // fbToolSize.Y = ypos;
            fbToolSize.Width = 160;
            fbToolSize.MaxValue = 300;
            fbToolSize.ShowMaxValueLabelAtEnd = true;
            fbToolSize.ShowNotches = false;
            fbToolSize.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Never;
            fbToolSize.SliderMouseUp += fbToolSize_SliderMouseUp;

        }

        void fbToolSize_SliderMouseUp(object sender, EventArgs e)
        {
            setting.Value = (float)fbToolSize.Value;
        }

        public void Set(RadiusSetting setting)
        {
            this.setting = setting;

            fbToolSize.Value = (int)setting.Value;
            fbToolSize.UpdateSliderPosition();

        }
    }
}
