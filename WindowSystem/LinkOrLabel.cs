using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowSystem
{
    /// <summary>
    /// can display either a link or a label as appropriate. Wouldn't it be better to set Enabled state on a hyperlink...
    /// </summary>
    public class LinkOrLabel: UIComponent
    {
        public Hyperlink Hyperlink;

        public Label Label;


        public enum Modes { Link, Label };


        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;

                Label.Width = value;
                Hyperlink.Width = value;
            }
        }


        public override string ToolTip
        {
            get
            {
                return Hyperlink.ToolTip;
            }
            set
            {
                Hyperlink.ToolTip = value;
                Label.ToolTip = value;
            }
        }

        public string Text
        {
            set
            {
                Hyperlink.Text = value;
                Label.Text = value;

                base.Width = Hyperlink.Width;
                base.Height = Hyperlink.Height;
            }
        }


        public Modes Mode
        {
            set
            {
                if (value == LinkOrLabel.Modes.Label)
                {
                    Label.Visible = true;
                    Hyperlink.Visible = false;
                }
                else
                {
                    Label.Visible = false;
                    Hyperlink.Visible = true;
                }
            }
        }



        public LinkOrLabel(GUIManager gui, Label.LabelType labelType): base(gui)
        {
            Hyperlink = new Hyperlink(gui);
            Add(Hyperlink);
           
            Label = new Label(gui);
            Label.Init(labelType);
            Add(Label);
          

        }

    }
}
