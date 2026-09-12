using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using InputEventSystem;

namespace WindowSystem
{
    public class Hyperlink: TextButton
    {
        //public IHyperlinkTarget LinkedEntity;

        /// <summary>
        /// only fill in one of these link targets:
        /// </summary>

        public uint? TargetResourceContainerID;
        public uint? TargetEntityID;
        public Point? TargetMapPosition;
        public uint? TargetZoneID;

        const string toolTip = "LMB: Select the link target.\n RMB: Center on target.\n Double click: Select and center.";
            

        private Color normalColor = Color.Black;

        /// <summary>
        /// use this to set the link color in its normal state!
        /// do not set Color!
        /// </summary>
        public Color NormalColor
        {
            get
            {
                return normalColor;
            }
            set
            {
                if (normalColor != value)
                { 
                    // only set the label color now if it is in 'normal' state:
                    if (base.LabelColor == normalColor)
                    {
                       
                        base.LabelColor = value;
                    }

                    normalColor = value;

                    Rectangle rect = GUIManager.GUISpriteSheet.GetSourceRectangle("hyperlink");
                    buttonBox.SetSkinLocation(SkinState.Normal, rect, normalColor, normalColor);
                } 
            }
        }

        public static Color HoverColor = Color.Yellow;
        public static Color PressedColor = Color.DarkGray;

        
        public Hyperlink(GUIManager guiManager, RenderType renderType = WindowSystem.RenderType.CRTAndLCD)
            : base(guiManager)
        {
            Type = TextButtonType.Hyperlink; // affects formatting, and expands the button to fit the text

            Click += new ClickHandler(Hyperlink_Click);
            RightClick += new ClickHandler(Hyperlink_RightClick);

            DebugTag = "testHyper";
            base.buttonBox.DebugTag = "hyperBar";

            MinHeight = 1; // is 25 for textbuttons??

            Rectangle rect = GUIManager.GUISpriteSheet.GetSourceRectangle("hyperlink");

            buttonBox.SetSkinLocation(SkinState.Normal, rect, NormalColor, NormalColor);           
            buttonBox.SetSkinLocation(SkinState.Hover, rect, HoverColor, HoverColor);
            buttonBox.SetSkinLocation(SkinState.Pressed, rect, PressedColor, PressedColor);

          //  buttonBox.SetSkinLocation(SkinState.Disabled, null, lcdDisabledColor, lcdDisabledColor);
          //  buttonBox.SetSkinLocation(SkinState.HoverDisabled, null, lcdDisabledColor, lcdDisabledColor);
           

            Height = rect.Height;
            CornerSize = 4;

          //  Skin = rect;
          //  rect = GUIManager.GUISpriteSheet.GetSourceRectangle("hyperlink_hover");
          //  HoverSkin = rect;
          //  PressedSkin = rect;

            Font = GUIManager.LCDandHUDFont;
            ToolTip = toolTip;
                      
            RenderType = renderType;

           // CenterVertically(buttonBox);
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                if (base.Enabled != value)
                {
                    if (value == false)
                    {
                        buttonBox.Visible = false; // hide the underline
                        ToolTip = null;
                    }
                    else
                    {
                        buttonBox.Visible = true;
                        ToolTip = toolTip;
                    }

                    base.Enabled = value;
                }
            }
        }

        void Hyperlink_RightClick(UIComponent sender, EventArgs e)
        {
            guiManager.HyperLinkClicked(TargetEntityID, TargetResourceContainerID, TargetZoneID, TargetMapPosition, //LinkedEntity, 
                GUIManager.MouseButtonClicked.Right);
        }

        void Hyperlink_Click(UIComponent sender, EventArgs e)
        {
            guiManager.HyperLinkClicked(TargetEntityID, TargetResourceContainerID, TargetZoneID, TargetMapPosition, 
                //LinkedEntity, 
                GUIManager.MouseButtonClicked.Left);
        }

     /*   public new void Init(TextButtonType type)
        {
            throw new Exception("NAH!");
        }*/

        protected override void OnMouseOver(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            LabelColor = HoverColor;

            this.buttonBox.CurrentSkinState = SkinState.Hover;

            base.OnMouseOver(sender, args);
        }

        protected override void OnMouseOut(UIComponent sender, InputEventSystem.MouseEventArgs args)
        {
            LabelColor = NormalColor;

            this.buttonBox.CurrentSkinState = SkinState.Normal;

            base.OnMouseOut(sender, args);
        }

        protected override void OnMouseDown(InputEventSystem.MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                this.buttonBox.CurrentSkinState = SkinState.Pressed;

                LabelColor = PressedColor;
            }

            base.OnMouseDown(args);
        }



        protected override void OnMouseUp(InputEventSystem.MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                if (CheckCoordinates(args.Position.X, args.Position.Y))
                {
                    this.buttonBox.CurrentSkinState = SkinState.Hover;

                    LabelColor = HoverColor;
                }
                else
                {
                    this.buttonBox.CurrentSkinState = SkinState.Normal;

                    LabelColor = NormalColor;

                }
            }
            
            base.OnMouseUp(args);
        }


        public static string ToLink(string name, long id, bool useUpperCase = false)
        {
            // §E0¤Ward Conlan§
            StringBuilder text = new StringBuilder();
            text.Append("§E");
            text.Append(id.ToString());
            text.Append("¤");

            /* if (PersonEntity != null)
             {*/
            if (useUpperCase)
            {
              //  text.Append(name.ToUpper(Config.Culture));
            }
            else
            {
                text.Append(name);
            }

            /* }
             else
             {
                 if (useUpperCase)
                 {
                     text.Append(name.ToUpper(Config.Culture));
                 }
                 else
                 {
                     text.Append(name);
                 }
             }*/

            text.Append("§");

            return text.ToString();
        }
    }
}
