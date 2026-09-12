using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using InputEventSystem;

namespace WindowSystem
{
    /// <summary>
    /// the sliding knob with a label on it
    /// </summary>
    public class FillableBarSlider : UIComponent
    {
        public enum SliderType
        {
            HUD, LCD, LCDWhite, // these sliders have the same value as the value bar (the value bar is for visual effect)
            Indicator, // the indicator and the value bar can differ
            StockExportSlider, StockProductionSlider, StockImportSlider  // not in use         
        };


        public event MouseUpHandler MouseUp;


        #region Fields
     
        Box knob;
            

        /// <summary>
        /// the number label on the knob that appears on drag.
        /// </summary>
        Label label;

        public event EventHandler SliderMouseUp, SliderMouseDown;

        private const float ShrinkScaleWhenSliderIsBelowThisPercentage = 0.8f;

        public int MinimumSliderWidth = 14;

        /// <summary>
        /// distance around the number to the icon edge
        /// </summary>
        private int labelHorizontalPadding = 2;

        /// <summary>
        /// distance around the number to the sprite edges
        /// </summary>
       /* int? labelLeftPadding;
        int? labelRightPadding;
        */
       


        Boolean isDragging;

        private int value;
        public int Value
        {
            get
            {
                // this slider's value corresponds to the bar value
                if (type == SliderType.HUD || type == SliderType.LCD || type == SliderType.LCDWhite)
                {
                    return parentBar.Value;
                }
                else
                {
                    return value;
                }
            }
            set
            {
                if (type != SliderType.HUD && type != SliderType.LCD && type != SliderType.LCDWhite)
                {
                    this.value = value;
                }

                if (label != null)
                {                   
                    SetNumberLabel(value);                  
                }
            }
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                label.Enabled = value;
                knob.Enabled = value;

                base.Enabled = value;
            }
        }

        public string MaxSliderValueSymbol
        {
            get;
            set;
        }

        private string maxSliderValueTooltip;
        public string MaxSliderValueTooltip
        {
            get
            {
                return maxSliderValueTooltip;
            }
            set
            {
                maxSliderValueTooltip = value;
                TooltipWidth = 80; // make extra room
            }
        }

        private void SetNumberLabel(int value)
        {
            int valueToDisplay = (value / parentBar.StepSize) * parentBar.StepSize; // round down

            string text = null;

            if (MaxSliderValueSymbol != null)
            {
                if (valueToDisplay == parentBar.MaxValue)
                {
                    text = MaxSliderValueSymbol;
                }
            }

            if (text == null)
            {
                if (parentBar.DisplayValueFunction != null)
                {
                    text = parentBar.DisplayValueFunction(valueToDisplay);
                }
                else if (valueToDisplay > 99)
                {
                    text = "**";
                }
                else
                {
                    text = valueToDisplay.ToString();
                }
            }

            label.Text = text;
        }

        SliderType type;

      //  private int maxValue;
      //  private GUIManager guimanager;
        private Point mouseClickPosition;
        private Point mouseOffsetPosition;
        private Game game
        {
            get
            {
                return guiManager.Game;
            }
        }

        #endregion

        /// <summary>
        /// will the bar's max value grow when the slider is dragged over the current limit?
        /// </summary>
        public bool CanGrow = true;


       

        public enum ShowValueLabelModes { Always, WhenDragging, Never }


        private ShowValueLabelModes showValueLabel = ShowValueLabelModes.WhenDragging;
        public ShowValueLabelModes ShowValueLabel
        {
            get
            {
                return showValueLabel;
            }

            set
            {
                if (showValueLabel != value)
                {
                    showValueLabel = value;
                    if (showValueLabel == ShowValueLabelModes.Always)
                    {
                        Add(label);
                    }
                    else //if (!showValueLabel && Controls.Contains(label))
                    {
                        Remove(label);
                    }
                }
            }

        }

        bool IsShowingMaximumSymbol()
        {
            return MaxSliderValueSymbol != null && parentBar.Value == parentBar.MaxValue;
        }
       
        public override string ToolTip
        {
            get
            {
                if (IsShowingMaximumSymbol() && MaxSliderValueTooltip != null)
                {                   
                    return MaxSliderValueTooltip;
                }
                else
                {                   
                    return parentBar.GetDisplayValue(parentBar.Value);
                }
              
            }
          
        }

        public Color NormalColor
        {
            set
            {
                knob.NormalColor = value;
            }
        }

        public int KnobWidth
        {
            set
            {
                knob.Width = value;
            }
        }

        FillableBar parentBar
        {
            get
            {
                return (FillableBar)Parent;
            }
        }

        public Color TextColor
        {
            set
            {
                label.NormalColor = value;
            }
        }

        #region Constructors

        const int digitTooltipWidth = 30;

      /// <summary>
      /// game reference is used to hide mouse cursor
      /// </summary>
      /// <param name="guiManager"></param>
      /// <param name="game"></param>
      /// <param name="type"></param>
      /// <param name="labelType"></param>
        public FillableBarSlider(GUIManager guiManager, SliderType type, WindowSystem.Label.LabelType labelType, ShowValueLabelModes showValueLabel)
            : base(guiManager)
        {
            
            this.type = type;
           // this.knob = new Icon(guiManager);
            this.knob = new Box(guiManager);
            this.label = new Label(guiManager);
                    
            TooltipWidth = digitTooltipWidth;

            Rectangle rect;

            switch (type)
            {
                case SliderType.LCD:
                    InitSliderKnobSingleLine(labelType, "lcd_bar_knob", "lcd_bar_knob_disabled", 6, 4, 1, -2, true, showValueLabel);

                    break;

                case SliderType.LCDWhite:
                    InitSliderKnobSingleLine(labelType, "lcd_bar_knob_white", "lcd_bar_knob_disabled", 6, 4, 1, -2, true, showValueLabel);

                    break;

                case SliderType.HUD:
                    InitSliderKnobSingleLine(labelType, "HUD_slider_knob", "HUD_slider_knob", 4, 3, 0, 0, true, showValueLabel);

                    break;
                case SliderType.Indicator:
                    InitSliderKnobSingleLine(labelType, "lcd_bar_indicatorLine", "lcd_bar_indicatorLine", 1, 0, 1, -2, false, showValueLabel);

                    break;

                case SliderType.StockExportSlider:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("ordercontrol_handle_E");
                    this.ZOrder = 0.1f;

                    InitSliderKnobMultipleLines(labelType, rect);

                    break;

                case SliderType.StockProductionSlider:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("ordercontrol_handle_E"); //"ordercontrol_handle_P"); // Lars missing asset hack
                    this.ZOrder = 0.2f;

                    InitSliderKnobMultipleLines(labelType, rect);
                   
                    break;

                case SliderType.StockImportSlider:
                    rect = guiManager.GUISpriteSheet.GetSourceRectangle("ordercontrol_handle_E"); //"ordercontrol_handle_I"); // Lars missing asset hack
                    this.ZOrder = 0.3f;

                    InitSliderKnobMultipleLines(labelType, rect);

                    break;
            }

            
           // Width = knob.Width + knob.X + 10;
            Width = knob.Width;
            Height = knob.Height + knob.Y;
            value = 0;
        }

        #endregion

        void InitSliderKnobSingleLine(Label.LabelType labelType, string sprite, string disabledSprite, int cornerSize, 
            int labelHorizontalPadding,
            int labelYPosition, int sliderYPosition, bool isMovable, ShowValueLabelModes showValueLabel)
        {
            
            Add(knob);
            //Add(label);

            Rectangle rect = guiManager.GUISpriteSheet.GetSourceRectangle(sprite); 
            knob.SetSkinLocation(SkinState.Normal,rect);
            knob.SetSkinLocation(SkinState.Hover, rect, TextButton.lcdTooltipHoverTint, TextButton.lcdTooltipHoverTint);


           // knob.ResizeControlToFitImage();
            rect = guiManager.GUISpriteSheet.GetSourceRectangle(disabledSprite); // "HUD_slider_knob");
            knob.SetSkinLocation(SkinState.Disabled, rect);
                       

            //NEW:
            knob.CornerSize = cornerSize; // 3;
            knob.Width = rect.Width;
            knob.Height = rect.Height;

            ShowValueLabel = showValueLabel;

            // if we want to display the slider value as a tooltip, this control must have focus instead of the box - we can't do both
            if (isMovable)
            {
                CanHaveFocus = true;
                ToolTip = "-"; // needed to trigger the dynamic tooltip
               /* knob.CanHaveFocus = true;
                knob.MouseMove += new InputEventSystem.MouseMoveHandler(Icon_MouseMove);
                knob.MouseDown += new InputEventSystem.MouseDownHandler(Icon_MouseDown);
                knob.MouseUp += new InputEventSystem.MouseUpHandler(Icon_MouseUp);*/
            }
            else
            {
                CanHaveFocus = false; // Indicator: no tooltip either
            }

            Position = new Point(-knob.X, sliderYPosition); // - 1); // 0);

            this.labelHorizontalPadding = labelHorizontalPadding; 
                       
            if (ShowValueLabel != ShowValueLabelModes.Never)
            {
                label.Y = labelYPosition; // 1;
                label.Init(labelType);
                label.Text = "0";

                SetKnobAndLabelSize();
            }
           /* else
            {
                knob.Width = 16;
            }*/
        }

        void InitSliderKnobMultipleLines(Label.LabelType labelType, Rectangle rect)
        {
            Add(knob);
           
            knob.SetSkinLocation(SkinState.Normal,rect);
          //  knob.ResizeControlToFitImage();

            MouseMove += new InputEventSystem.MouseMoveHandler(Icon_MouseMove);
            MouseDown += new InputEventSystem.MouseDownHandler(Icon_MouseDown);
            MouseUp += new InputEventSystem.MouseUpHandler(Icon_MouseUp);
         
            Position = new Point(-knob.X, 0);

            label.Position = new Point(knob.X, 0);
            label.Init(labelType);
            label.Text = "";
            label.Width = knob.Width; // +icon.X + 10;

        }


        /// <summary>
        /// don't call this when losing focus because a different button was clicked (focus is still set after the mouse moves away!), 
        /// or when a modal dialog appears.
        /// </summary>
        /// <param name="args"></param>
        void Icon_MouseUp(InputEventSystem.MouseEventArgs args)
        {
            if (!Enabled)
                return;

            isDragging = false;

            //NEW: snap to increments:
            int newValue = (Value / parentBar.StepSize) * parentBar.StepSize;           
            SetSliderValue(newValue); 



            if (ShowValueLabel == ShowValueLabelModes.WhenDragging) //if (!ShowValueLabel)
            {
                Remove(label);
            }

            game.IsMouseVisible = true;
            mouseWasHidden = false;

            SetMouseCursorPosition();            

            // snap the slider to a proper interval:
            UpdateSliderPositionAndSize(null);

            if (SliderMouseUp != null)
            {
                SliderMouseUp.Invoke(this, EventArgs);
            }

            // signal that the user selected a value
            if (this.MouseUp != null)
            {
                this.MouseUp.Invoke(args);
            }
        }

        private void SetMouseCursorPosition()
        {
            guiManager.SetMousePosition(this.AbsolutePosition.X + this.mouseOffsetPosition.X, this.AbsolutePosition.Y + this.mouseOffsetPosition.Y);

        }

        bool mouseWasHidden = false;

        void Icon_MouseDown(InputEventSystem.MouseEventArgs args)
        {
            if (!Enabled)
                return;

            isDragging = true;

            if (ShowValueLabel == ShowValueLabelModes.WhenDragging) // !ShowValueLabel)
            {
                Add(label);
            }

            this.mouseClickPosition = args.Position;
            this.mouseOffsetPosition = new Point(args.Position.X - this.AbsolutePosition.X, args.Position.Y - this.AbsolutePosition.Y);

            if (SliderMouseDown != null)
            {
                SliderMouseDown.Invoke(this, EventArgs);
            }
            
            mouseWasHidden = true;
            game.IsMouseVisible = false;
        }


        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);


            Icon_MouseMove(args);
        }

        /// <summary>
        /// the box cannot take focus, so pass the call
        /// </summary>
        /// <param name="args"></param>
        protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOver(sender, args);

            if (Enabled && !isDragging) // don't set the knob state while dragging, causes flicker
            {
                knob.CurrentSkinState = SkinState.Hover;
                
            }
        }

        protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOut(sender, args);

            if (Enabled)
            {
                knob.CurrentSkinState = SkinState.Normal;
            }
        }

        protected override void OnMouseUp(MouseEventArgs args)
        {
            base.OnMouseUp(args);

            Icon_MouseUp(args);
        }


        protected override void OnMouseDown(MouseEventArgs args)
        {
            base.OnMouseDown(args);

            Icon_MouseDown(args);
        }

        protected override void OnLoseFocus()
        {
            // happens when a modal dialog appears, or when another button is pressed...
            base.OnLoseFocus();

            isDragging = false;


        }

        void Icon_MouseMove(InputEventSystem.MouseEventArgs args)
        {
            if (!Enabled)
                return;

            if (isDragging)
            {
                //int positionOnSlider = this.X + icon.Width / 2;
                int positionOnSlider = this.X + knob.Width / 2 - parentBar.SliderScaleStartX;

                int xDiff = args.Position.X - this.mouseClickPosition.X;

                if (xDiff < 0)
                {

                }

                this.mouseClickPosition = this.AbsolutePosition;

                
              //  guiManager.SetMousePosition(this.mouseClickPosition.X, this.mouseClickPosition.Y);
                guiManager.SetMousePosition(this.mouseClickPosition.X, this.mouseClickPosition.Y + Height / 2);

               // SetMouseCursorPosition();


                positionOnSlider += xDiff;

                positionOnSlider = Math.Max(positionOnSlider, 0);

                if (!CanGrow)
                {
                    // set position to the end at maximum:
                    positionOnSlider = Math.Min(positionOnSlider, parentBar.BarWidth);

                    int newValue = GetValueFromPosition(positionOnSlider);
                    
                    SetSliderValue(newValue); 
                }
                else
                { 
                    // handle growing/shrinking the scale here.

                    int newValue;
                  
                    if (positionOnSlider > parentBar.BarWidth
                        && parentBar.MaxValue < parentBar.GrowToMaximum)
                    {
                        // go beyond current maximum:
                        newValue = parentBar.MaxValue + 1;
                    }
                    else 
                    {
                        newValue = GetValueFromPosition(positionOnSlider);                        
                    }

                    // possibly alter MaxValue:
                    SetSliderValue(newValue);

                    // now set position to the end at maximum:
                    positionOnSlider = Math.Min(positionOnSlider, parentBar.BarWidth);

                }


                UpdateSliderPositionAndSize(positionOnSlider);
               
            }
        }


        public override void CleanUp()
        {
            // this method gets called when the slider is removed from its form. Make sure that the mouse cursor re-appears if we had previously hidden it:
            if (mouseWasHidden && game.IsMouseVisible == false)
            {
                game.IsMouseVisible = true;
                mouseWasHidden = false;
            }

            base.CleanUp();
        }

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                if (base.Visible != value)
                {
                    // gets called from parent.
                    if (value == true)
                    {
                        if (parentBar.MaxValue == 0)
                        {
                            return; // NEW - hide the knob if no values can be chosen
                        }
                    }

                    base.Visible = value;
                }
            }
        }

     

        /// <summary>
        /// This is necessary to call when setting the value programmatically (not via mouse) to avoid infinite loops between the bar control and the slider control
        /// </summary>
        /// <param name="positionOnSlider"></param>
        public void UpdateSliderPositionAndSize(int? positionOnSlider)
        {
            if (!positionOnSlider.HasValue)
            {
                // compute the pos from the value. 
                positionOnSlider = GetPositionFromValue();
            }

            if (parentBar.MaxValue == 0)
            {
                this.Visible = false; // NEW - hide the knob if no values can be chosen
            }
            else
            {
                this.Visible = true;

                if (ShowValueLabel != ShowValueLabelModes.Never)
                {
                    SetNumberLabel(Value);
                    SetKnobAndLabelSize();
                }
            }


            int sliderOffset = 0;
            if (Value == parentBar.MaxValue && knob.Width < 4)
            {   
                // the 3 pixels wide principles indicator was placed with 2/3rds over the end because its width was rounded to 1 instead of 2, making it almost dissappear.                             
                sliderOffset = -1;

                //this.X = positionOnSlider.Value - (int)Math.Round(knob.Width / 2f) + parentBar.SliderScaleStartX; // OLD: So round the number properly (1.5 => 2).
            }
            
            this.X = positionOnSlider.Value - knob.Width / 2 + parentBar.SliderScaleStartX + sliderOffset;
            
        }

        private void SetKnobAndLabelSize()
        {
            label.FitToText();

            int iconWidth = Math.Max(MinimumSliderWidth, label.Width + 2 * labelHorizontalPadding);
            knob.Width = iconWidth;
            this.Width = knob.Width;

            knob.CenterChildHorizontally(label);

        }

        /// <summary>
        /// returns the closest step
        /// </summary>
        /// <param name="positionOnSlider"></param>
        /// <returns></returns>
      /*  public int GetClosestPosition(int positionOnSlider)
        {
            if (parentBar.StepSize > 1)
            {
                int stepSizeInPixels = ;
                return FillableBar.RoundToIncrements(positionOnSlider, parentBar.StepSize);                

            }
            else
            {
                return positionOnSlider;
            }
        }*/

        /// <summary>
        /// applies scaling to the slider pixel position to compute the slider value
        /// </summary>
        /// <param name="positionOnSlider"></param>
        /// <returns></returns>
        public int GetValueFromPosition(int positionOnSlider, bool roundToNearestStep = false)
        {
         //   int value = (int)((((float)positionOnSlider) / ((float)parentBar.BarWidth)) * (float)parentBar.MaxValue);

            int value = (int)Math.Round((((float)positionOnSlider) / ((float)parentBar.BarWidth)) * (float)parentBar.MaxValue);

            if (roundToNearestStep)
            {
                int roundedValue = FillableBar.RoundToIncrements(value, parentBar.StepSize); // parentBar.rou(value);

                return roundedValue;
            }

            return value;
        }

        private int GetPositionFromValue() //int position)
        {
            // NEW: clamp to stepsized increments
            int noOfIncrements = parentBar.MaxValue / parentBar.StepSize;

            noOfIncrements = Util.Clamp(noOfIncrements, 1, 10000000);

            int incrementPosition = Value / parentBar.StepSize;

            int pixelPosition = (int)((float)incrementPosition * ((float)parentBar.BarWidth / (float)noOfIncrements));
            return pixelPosition;

                        
        }


       /* public int GetPositionClampedToIncrements(int xPos)
        {
            int noOfIncrements = parentBar.MaxValue / parentBar.StepSize;

            int incrementPosition = Value / parentBar.StepSize;

            int pixelPosition = (int)((float)incrementPosition * ((float)parentBar.BarWidth / (float)noOfIncrements));
            return pixelPosition;

        }*/

        private void SetSliderValue(int newValue) 
        {
          
            if (newValue < 0)
                newValue = 0;


            if (type == SliderType.HUD || type == SliderType.LCD || type == SliderType.LCDWhite)
            {
                parentBar.Value = newValue; 
            }
            else
            {
                this.value = newValue;
            }

            if (CanGrow)
            {
                if (newValue > parentBar.MaxValue
                    && newValue <= parentBar.GrowToMaximum)
                {
                    // 'grow' the bar scale
                    parentBar.MaxValue = newValue;

                }
                else if (parentBar.MaxValue > parentBar.GrowScaleFromThisValue
                            && newValue < ShrinkScaleWhenSliderIsBelowThisPercentage * parentBar.MaxValue)
                {
                    // shrink the scale:
                    parentBar.MaxValue = parentBar.MaxValue - 1;

                }

            }
        }


        /// <summary>
        /// not used..
        /// </summary>
        /// <param name="pos"></param>
        private void MoveSliderToPosition(Point pos)
        {
            
          
        }
    }
}
