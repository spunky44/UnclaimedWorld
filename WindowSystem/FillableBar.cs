using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using InputEventSystem;

namespace WindowSystem
{
    /// <summary>
    /// a fillable (progress) bar that can have slider controls attached to it
    /// </summary>
    public class FillableBar : UIComponent
    {
        public enum FillableBarType { Default, ProgressBar, HUDSlider, HUDSliderWhite, LCDSlider, LCDSliderWhite, LCDIndicator, StockOrders };

     //   public enum FillableBarColor { Green, Red, Yellow, Blue };

        #region Default Properties
        private static int defaultEdgeSize = 4;
        #endregion

        #region Fields
        Bar underBar;
        Bar valueBar;
        int valueBarIndex;
        Label lblMaxValue;
        FillableBarSlider slider, slider2, slider3;

        TextButton tbDecrease, tbIncrease;

        List<Icon> notchesPool;
        List<Icon> notchesInUse;


        private Rectangle underBarRect;
        private Rectangle valueBarRect;
        private int maxValue;
        
        private int barValue; // for LCD and HUD sliders these two values are equal. For Indicator type, they differ.
       // private int sliderValue;

        private Rectangle skin;
        private FillableBarType type;
        private Color color;


        private float sliderButtonDelay;
        private float timeBetweenSliderButtonIncrements;
        
        #endregion


        public Func<int, string> DisplayValueFunction;


        public event /*MouseUpHandler*/ EventHandler SliderMouseUp, SliderMouseDown;


        #region Properties

        bool showNotches = false;
        public bool ShowNotches
        {
            set
            {
                if (showNotches != value)
                {
                    showNotches = value;

                    UpdateNotches();
                }
            }
        }

        public int MaxValue
        {
            get { return maxValue; }
            set {

#if !RELEASE
                if (value < 0)
                {
                    throw new Exception();
                }
#endif
                SetValues(this.barValue, SliderValue, value);

                UpdateNotches();

#if !RELEASE
                if (MaxValue < 0)
                {
                    throw new Exception();
                }
#endif
            }
        }

        private int stepSize = 1;
        public int StepSize
        {
            get { return stepSize; }
            set 
            {
                if (stepSize != value)
                {
                    stepSize = value;

                    UpdateNotches();
                }
            }
        }

        public int GrowScaleFromThisValue;

        public int GrowToMaximum;

        public int BarWidth
        {
            get
            {
                return underBar.Width;
            }
        }

        public string MaxSliderValueSymbol
        {
            get
            {
                if (slider != null)
                {
                    return slider.MaxSliderValueSymbol;
                }

                return null;
            }
            set
            {
                if (slider != null)
                {
                    slider.MaxSliderValueSymbol = value;
                }
            }
        }

        public string MaxSliderValueTooltip
        {           
            set
            {
                if (slider != null)
                {
                    slider.MaxSliderValueTooltip = value;
                }
            }
        }

        

        /// <summary>
        /// For LCD and HUD slider types, this property controls both the value bar and the slider value - they are locked together.
        /// For Indicator, it controls just the value bar.
        /// 
        /// the set property also gets called from the slider component when the user drags the slider. 
        /// When setting this programmatically, remember to call UpdateSliderPosition()!!
        /// </summary>
        public int Value
        {
            get { return barValue; }
            set 
            {
                SetValues(value, SliderValue, this.maxValue);
                
            }
        }


        public int SliderValue
        {
            get 
            {
                if (slider != null)
                {
                    return slider.Value; 
                }
                else return barValue; 
            }

            set
            {
                SetValues(barValue, value, this.maxValue);
                
            }
        }


        bool ValueBarAndSliderAreLocked
        {
            get
            {
                return type != FillableBarType.LCDIndicator;
            }
        }

      /*  private int multiplier = 1;

        public int Multiplier
        {
            get { return this.multiplier; }
            set { this.multiplier = value; }
        }*/
              

        public Rectangle Skin
        {
            get { return skin; }
            set { skin = value; }
        }

        public FillableBarType Type
        {
            get { return type; }
            set { type = value; }
        }

        public Color KnobTextColor
        {
            set
            {
                if (slider != null)
                {
                    slider.TextColor = value;
                }
                if (slider2 != null)
                {
                    slider2.TextColor = value;
                }
                if (slider3 != null)
                {
                    slider3.TextColor = value;
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
                if (base.Enabled != value)
                {
                    base.Enabled = value;

                    underBar.Enabled = value;
                   // valueBar.Enabled = value;

                    lblMaxValue.Enabled = value;

                    if (slider != null)
                    {
                        slider.Enabled = value;
                    }

                    if (tbIncrease != null)
                    {
                        tbIncrease.Enabled = value;
                    }

                    if (tbDecrease != null)
                    {
                        tbDecrease.Enabled = value;
                    }

                    if (base.Enabled)
                    {
                        /*   if (enabledColor.HasValue)
                           {
                               Color = enabledColor.Value;
                           }*/

                        valueBar.CurrentSkinState = SkinState.Normal;
                    }
                    else
                    {
                        /* if (DisabledColor.HasValue)
                         {
                             Color = DisabledColor.Value;
                         }*/

                        valueBar.CurrentSkinState = SkinState.Disabled;

                    }
                }
            }
        }

        public Color? ButtonColor
        {
            set
            {

            }
        }

        /// <summary>
        /// tints both underbar and valuebar, but keeps the indicator/slider
        /// </summary>
        public Color Color
        {
            get
            {
                return this.color;
            }
            set
            {
                if (this.color != value)
                {
                    this.color = value;
                    this.underBar.SetSkinLocation(SkinState.Normal, null /* underBarRect*/, this.color, this.color);
                    this.valueBar.SetSkinLocation(SkinState.Normal, null /* valueBarRect*/, this.color, this.color);
                }
            }
        }

        /// <summary>
        /// tints both underbar and valuebar as well as buttons/slider
        /// </summary>
        public Color ColorAllControls
        {
            get
            {
                return this.color;
            }
            set
            {
                if (this.color != value)
                {
                    this.color = value;
                    this.underBar.SetSkinLocation(SkinState.Normal, null, this.color, this.color);
                    this.valueBar.SetSkinLocation(SkinState.Normal, null, this.color, this.color);

                    if (tbIncrease != null)
                    {
                        tbIncrease.NormalColor = value;
                    }

                    if (tbDecrease != null)
                    {
                        tbDecrease.NormalColor = value;
                    }

                    if (slider != null)
                    {
                        slider.NormalColor = value;
                    }
                }
            }
        }
        
        
        /// <summary>
        /// using a single white sprite and tint it to any color.
        /// </summary>
        public Color BarColor
        {
            get {
                 return this.color;
            }
            set {

                this.color = value;
                this.valueBar.SetSkinLocation(SkinState.Normal, valueBarRect, this.color, this.color);
            }
        }

        public Color UnderBarColor
        {
           /* get
            {
                return this.color;
            }*/
            set
            {

              //  this.color = value;
                this.underBar.SetSkinLocation(SkinState.Normal, valueBarRect, value, value);
            }
        }
               
        private string Text
        {
            get { return this.lblMaxValue.Text; }
            set {

                this.lblMaxValue.Text = value; 
                this.lblMaxValue.FitToText();                
            }
        }

        public string SliderTooltip
        {
            set
            {
                if (slider != null)
                {
                    slider.ToolTip = value;
                }
            }
        }

        public string ButtonTooltip
        {
            set
            {
                if (tbDecrease != null)
                {
                    tbDecrease.ToolTip = value;
                    tbIncrease.ToolTip = value;
                }
            }
        }


        #endregion

        #region Constructors
       

        public FillableBar(GUIManager guiManager, FillableBarType type, bool canGrow = true, bool includeButtons = false,
            float? timeBetweenButtonIncrements = null, float? sliderButtonDelay = null)
            : base(guiManager)
        {
            this.Type = type;
            //this.game = game;

            switch (type)
            {
                case FillableBarType.Default:
                case FillableBarType.ProgressBar:

                    this.underBar = new Bar(guiManager);
                    this.valueBar = new Bar(guiManager);
                    this.lblMaxValue = new Label(guiManager);
                    
                    this.valueBarRect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_barfill_white");
                    valueBar.SetSkinLocation(SkinState.Normal, valueBarRect);
                    valueBar.SetSkinLocation(SkinState.Disabled, valueBarRect, Color.Gray, Color.Gray);
                   /* valueBar.SetSkinLocation(SkinState.Hover, valueBarRect, Color.DarkGray, Color.DarkGray);
                    valueBar.SetSkinLocation(SkinState.HoverDisabled, valueBarRect, Color.DarkGray, Color.DarkGray);
                  */

                    valueBar.EdgeSize = 2;   
                    valueBar.Height = valueBarRect.Height;

                    Add(underBar);                   
                    valueBarIndex = Add(valueBar);
                  //  Add(lblMaxValue);

                   
                    underBarRect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_barfill_grey"); // "lcd_bar_empty"); // 
                    underBar.SetSkinLocation(SkinState.Normal, underBarRect);
                   /* underBar.SetSkinLocation(SkinState.Hover, underBarRect, Color.DarkGray, Color.DarkGray);
                    underBar.SetSkinLocation(SkinState.HoverDisabled, underBarRect, Color.DarkGray, Color.DarkGray);
                  */
                    underBar.EdgeSize = 2; // defaultEdgeSize;
                    underBar.Height = underBarRect.Height;

                    barLeftMargin = 0;

                    Height = underBarRect.Height;

                    EnableHover();

                    break;

                case FillableBarType.LCDSlider:
                    InitBarWithSlider(guiManager, Label.LabelType.LCDNormal, FillableBarSlider.SliderType.LCD, canGrow, "lcd_bar_empty", "lcd_bar_filled", 2, FillableBarSlider.ShowValueLabelModes.Always, true, includeButtons, sliderButtonDelay, timeBetweenButtonIncrements);
                   
                    break;
                case FillableBarType.LCDSliderWhite:
                    InitBarWithSlider(guiManager, Label.LabelType.LCDNormal, FillableBarSlider.SliderType.LCDWhite, canGrow, "lcd_bar_empty", "lcd_bar_filled_white", 2, FillableBarSlider.ShowValueLabelModes.Always, true, includeButtons, sliderButtonDelay, timeBetweenButtonIncrements);
                   
                    break;

                case FillableBarType.HUDSlider:
                    InitBarWithSlider(guiManager, Label.LabelType.HUDWindow, FillableBarSlider.SliderType.HUD, canGrow, "HUD_slider_base", "HUD_slider_fill", 3, FillableBarSlider.ShowValueLabelModes.Always, true, includeButtons, sliderButtonDelay, timeBetweenButtonIncrements);          

                    break;

                case FillableBarType.HUDSliderWhite:
                    InitBarWithSlider(guiManager, Label.LabelType.HUDWindow, FillableBarSlider.SliderType.HUD, canGrow, "HUD_slider_base_white", "HUD_slider_fill_white", 3, FillableBarSlider.ShowValueLabelModes.Always, true, includeButtons, sliderButtonDelay, timeBetweenButtonIncrements);

                    break;

                case FillableBarType.LCDIndicator:

                    InitBarWithSlider(guiManager, Label.LabelType.LCDNormal, FillableBarSlider.SliderType.Indicator, false, "lcd_bar_empty", "lcd_bar_filled_white", 2, FillableBarSlider.ShowValueLabelModes.Never, false, false, null, null);
                    DebugTag = "indicatorBar";

                    EnableHover();

                    break;
              
                case FillableBarType.StockOrders:

                    this.underBar = new Bar(guiManager);
                    this.valueBar = new Bar(guiManager);
                    this.lblMaxValue = new Label(guiManager);
                    lblMaxValue.ZOrder = 1f;

                    this.slider = new FillableBarSlider(guiManager, FillableBarSlider.SliderType.StockExportSlider, Label.LabelType.LCDNormal, FillableBarSlider.ShowValueLabelModes.WhenDragging);
                    //eSlider.ZOrder = 1f;
                    this.slider2 = new FillableBarSlider(guiManager, FillableBarSlider.SliderType.StockProductionSlider, Label.LabelType.LCDNormal, FillableBarSlider.ShowValueLabelModes.WhenDragging);
                    //pSlider.ZOrder = 2f;
                    this.slider3 = new FillableBarSlider(guiManager, FillableBarSlider.SliderType.StockImportSlider, Label.LabelType.LCDNormal, FillableBarSlider.ShowValueLabelModes.WhenDragging);
                    //iSlider.ZOrder = 3f;

                    Add(underBar);
                    valueBarIndex = Add(valueBar);
                    Add(slider);
                    Add(slider2);
                    Add(slider3);
                    Add(lblMaxValue);

                    slider.CanGrow = canGrow;
                    slider2.CanGrow = canGrow;
                    slider3.CanGrow = canGrow;

                    this.underBarRect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_barempty_green");
                    underBar.SetSkinLocation(SkinState.Normal,underBarRect);
                    underBar.EdgeSize = defaultEdgeSize;
                    //underBar.Width = this.Width;
                    underBar.Height = underBarRect.Height;

                    this.valueBarRect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_barfill_green");
                    valueBar.SetSkinLocation(SkinState.Normal,valueBarRect);
                    valueBar.EdgeSize = 0;
                    //valueBar.Width = this.Width - underBar.EdgeSize * 2;
                    valueBar.Height = valueBarRect.Height - 1;

                    lblMaxValue.Init(Label.LabelType.LCDNormal); // Label.LabelType.LCDHeader);
                    lblMaxValue.Text = "";
                    lblMaxValue.CanHaveFocus = false;

                  //  this.barOffset = 0;
                    break;
            }

           

            this.Value = 0;
            this.MaxValue = 0;
            
           // Height = this.underBarRect.Height + 50;
        }


        private void UpdateNotches()
        {
            
            if (showNotches == true &&
                maxValue > 1)
            {
                float? distanceBetweenNotches = null;

                distanceBetweenNotches = (((float)StepSize / (float)maxValue) * (float)BarWidth);
                

                int neededNotches;
                int notchesToAdd = 0;
               
                if (distanceBetweenNotches > 3)
                {
                    neededNotches = maxValue / StepSize - 1;
                }
                else 
                {
                    neededNotches = 0;
                }

                if (notchesInUse != null)
                {
                    notchesToAdd = neededNotches - notchesInUse.Count;
                }
                else 
                {
                    notchesToAdd = neededNotches;
                }

                if (notchesToAdd > 0)
                {
                    int added = 0;
                    if (notchesPool != null)
                    {
                        // reuse notches
                        int notchesToTakeFromPool = Math.Min(notchesToAdd, notchesPool.Count);
                        for (int i = notchesToTakeFromPool - 1; i >= 0; i--)
                        {
                            Icon notch = notchesPool[i];
                            Insert(notch, valueBarIndex);
                            //Add(notch);
                            Util.AddToList(ref notchesInUse, notch);
                            notchesPool.RemoveAt(i);

                            added++;
                        }
                    }

                    for (int i = 0; i < notchesToAdd - added; i++)
			        {
                        // create new notches
                        Icon notch = new Icon(guiManager);
                        notch.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("lcd_notch"));
                        notch.ResizeControlToFitImage();
                        notch.Y = underBar.Y;
                        
                       // Add(notch);
                        Insert(notch, valueBarIndex);
                        Util.AddToList(ref notchesInUse, notch);
			        }
                }
                else if (notchesToAdd < 0)
                {
                    if (notchesInUse != null)
                    {
                        for (int i = -notchesToAdd - 1; i >= 0; i--)
                        {
                            RemoveNotch(i);
                        }
                    }
                }

                if (notchesInUse != null && distanceBetweenNotches.HasValue)
                {
                    for (int i = 0; i < notchesInUse.Count; i++)
                    {
                        Icon notch = notchesInUse[i];

                        notch.X = (int)Math.Round(underBar.X + ((float)i + 1f) * distanceBetweenNotches.Value);
                    }
                }
            }
            else
            {
                if (notchesInUse != null)
                {
                    for (int i = notchesInUse.Count - 1; i >= 0; i--)
                    {
                        RemoveNotch(i);
                    }                   
                }
            }
            
        }

        private void RemoveNotch(int i)
        {
            Icon notch = notchesInUse[i];
            Remove(notch);
            Util.AddToList(ref notchesPool, notch);
            notchesInUse.RemoveAt(i);
        }

        //const int underbarYPos = 4;

        private void InitBarWithSlider(GUIManager guiManager, WindowSystem.Label.LabelType labelType, FillableBarSlider.SliderType sliderKnobType, 
            bool canGrow, string emptyBarSprite, string filledBarSprite, int underbarYPos, FillableBarSlider.ShowValueLabelModes showLabel, bool canMoveSlider, bool includeButtons
            , float? sliderButtonDelay, float? timeBetweenSliderButtonIncrements)
        {
            this.underBar = new Bar(guiManager);
            this.valueBar = new Bar(guiManager);
            this.lblMaxValue = new Label(guiManager);
            lblMaxValue.ZOrder = 1f;

            this.slider = new FillableBarSlider(guiManager, sliderKnobType, labelType, showLabel);
            slider.CanGrow = canGrow;
            slider.ShowValueLabel = showLabel; // FillableBarSlider.ShowValueLabelModes.Always;

            slider.DebugTag = "slider";
            
            if (canMoveSlider)
            {
                slider.SliderMouseUp += new EventHandler(slider1_SliderMouseUp);
                slider.SliderMouseDown += new EventHandler(slider1_SliderMouseDown);

                underBar.CanHaveFocus = true; // receive clicks in the trough
                underBar.MouseDown += underBar_MouseDown;
               // valueBar.MouseDown += underBar_MouseDown;
            }

            if (includeButtons)
            {
                tbDecrease = new TextButton(guiManager);
                tbIncrease = new TextButton(guiManager);

                InitButton(tbDecrease);
                InitButton(tbIncrease);

                tbDecrease.Text = "-";
                tbIncrease.Text = "+";

                /*tbDecrease.Click += tbDecrease_Click;
                tbIncrease.Click += tbIncrease_Click;
                */

                tbDecrease.MouseDown += tbDecrease_MouseDown;
                tbIncrease.MouseDown += tbIncrease_MouseDown;

                tbDecrease.MouseUp += tbDecrease_MouseUp;
                tbIncrease.MouseUp += tbIncrease_MouseUp;

                tbDecrease.LoseFocus += tbDecrease_LoseFocus;
                tbIncrease.LoseFocus += tbIncrease_LoseFocus;

                this.sliderButtonDelay = sliderButtonDelay.Value;
                this.timeBetweenSliderButtonIncrements = timeBetweenSliderButtonIncrements.Value;


               // barLeftMargin += tbDecrease.Width + 4;
            }

            Add(underBar);
            valueBarIndex = Add(valueBar);
            Add(slider);
            Add(lblMaxValue);


            this.underBarRect = guiManager.GUISpriteSheet.GetSourceRectangle(emptyBarSprite); 
            underBar.SetSkinLocation(SkinState.Normal,underBarRect);
            underBar.EdgeSize = defaultEdgeSize;
            underBar.Height = underBarRect.Height;

            this.valueBarRect = guiManager.GUISpriteSheet.GetSourceRectangle(filledBarSprite); 
            valueBar.SetSkinLocation(SkinState.Normal,valueBarRect);
            valueBar.EdgeSize = 0;
            valueBar.Height = underBar.Height;

            valueBar.SetSkinLocation(SkinState.Disabled, valueBarRect, Color.Gray, Color.Gray);
                  

            // make room for the slider to overlap at the top
            underBar.Y = underbarYPos;
            valueBar.Y = underBar.Y;

            lblMaxValue.Init(labelType);
            lblMaxValue.Text = "";
            lblMaxValue.CanHaveFocus = false;

            if (type == FillableBarType.HUDSlider)
            {
                lblMaxValue.Y = 0;
            }
            else
            {
                lblMaxValue.Y = -1;
            }

           // slider.ShowValueLabel = FillableBarSlider.ShowValueLabelModes.Always; //true;

            // TODO: figure out how to fire the SliderUp event when the user stops scrolling the mouse wheel...         
            CanReceiveMouseWheelEvents = false; // true


            //  this leaves a gap to the left for the slider to position with its middle at the 0 position
            underBar.X = GetBarLeftPos(); // barLeftMargin;
            valueBar.X = underBar.X; // barLeftMargin;

            Height = this.underBarRect.Height + 2 * underbarYPos;
        }

        void tbIncrease_LoseFocus()
        {
            OnLoseFocusReset();
        }

        void tbDecrease_LoseFocus()
        {
            OnLoseFocusReset();
        }

        

        void tbIncrease_MouseUp(MouseEventArgs args)
        {
            if (SliderMouseUp != null)
            {
                SliderMouseUp.Invoke(this, EventArgs);
            }

            isIncreasing = false;

        }

        void tbDecrease_MouseUp(MouseEventArgs args)
        {
            if (SliderMouseUp != null)
            {
                SliderMouseUp.Invoke(this, EventArgs);                
            }

            isDecreasing = false;

        }

        bool isDecreasing, isIncreasing;
        float buttonDownStarted = 0;

        void tbIncrease_MouseDown(MouseEventArgs args)
        {
            if (SliderMouseDown != null)
            {
                SliderMouseDown.Invoke(this, EventArgs);
            }

            isIncreasing = true;
            buttonDownStarted = 0;

            Increase();
        }

        void tbDecrease_MouseDown(MouseEventArgs args)
        {
            if (SliderMouseDown != null)
            {
                SliderMouseDown.Invoke(this, EventArgs);
            }

            isDecreasing = true;
            buttonDownStarted = 0;

            Decrease();
        }

        private void Increase()
        {
            int newValue = slider.Value + StepSize;
            SetValues(newValue, newValue, MaxValue);
            UpdateSliderPosition();
        }

       // const float delay = 0.5f;

       // const float timeBetweenButtonIncrements = 0.1f;
        

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (isDecreasing || isIncreasing)
            {
                buttonDownStarted += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (buttonDownStarted >= this.sliderButtonDelay) // delay in seconds before button down has an effect
                {
                    float division = (buttonDownStarted - sliderButtonDelay) / this.timeBetweenSliderButtonIncrements;

                    if (division > 1f)
                    {
                   // float modulo = buttonDownStarted % 0.1f;
                   // if (modulo > 0.05f)
                   // {
                        buttonDownStarted = sliderButtonDelay + division;
                        if (isDecreasing)
                        {
                            Decrease();
                        }
                        else if (isIncreasing)
                        {
                            Increase();
                        }
                    }
                    
                }
                else
                {
                  //  buttonDownStarted += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }

       

        private void Decrease()
        {
            int newValue = slider.Value - StepSize;
            SetValues(newValue, newValue, MaxValue);
            UpdateSliderPosition();
        }

        /*
        void tbIncrease_Click(UIComponent sender, EventArgs e)
        {
            SetValues(barValue, slider.Value + 1, MaxValue);
        }

        void tbDecrease_Click(UIComponent sender, EventArgs e)
        {
            SetValues(barValue, slider.Value - 1, MaxValue);
        }*/

        private void InitButton(TextButton tb)
        {
            Add(tb);

            if (type == FillableBarType.LCDSlider)
            {
                tb.Init(TextButton.TextButtonType.LCDSliderButton);

                tb.Y = -2; // top part is clipped...
            }
            else if (type == FillableBarType.LCDSliderWhite)
            {
                tb.Init(TextButton.TextButtonType.LCDSliderButtonWhite);

                tb.Y = -2; // top part is clipped...
            }
            else if (type == FillableBarType.HUDSlider)
            {
                tb.Init(TextButton.TextButtonType.HUDSliderButton);
               // tb.DebugTag = "HUDSliderButton";
                
            }
            else if (type == FillableBarType.HUDSliderWhite)
            {
                tb.Init(TextButton.TextButtonType.HUDSliderButtonWhite);
                tb.DebugTag = "HUDSliderButton";

            }

            tb.Width = 16;
        }

        void slider1_SliderMouseUp(object sender, EventArgs e)
        {
            if (SliderMouseUp != null)
            {
                SliderMouseUp.Invoke(this, EventArgs);
                //SliderMouseUp.Invoke(args);

            }

            UpdateValueBarWidth();
        }

        void slider1_SliderMouseDown(object sender, EventArgs e)
        {
            if (SliderMouseDown != null)
            {
                SliderMouseDown.Invoke(this, EventArgs);

            }
        }

        public static int RoundToIncrements(int value, int roundTo)
        {
            var remainder = value % roundTo;
            var result = remainder < roundTo - remainder
                ? (value - remainder) //round down
                : (value + (roundTo - remainder)); //round up
            return result;
        }

        /// <summary>
        /// When the mouse is pressed inside the thumb trough, the thumb moves
        /// to that position. 
        /// </summary>
        /// <param name="args"></param>
        void underBar_MouseDown(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left)
            {
                // Convert mouse location to scrollbar location
                int xPosition = args.Position.X - underBar.AbsolutePosition.X;

                // round if stepsizes are active:
                //int roundedPosition = slider.GetClosestPosition(xPosition);

                int roundedValue = slider.GetValueFromPosition(xPosition, true);
                //int roundedValue = GetRoundedValue(newValue);

                SetValues(roundedValue, roundedValue, MaxValue);
                UpdateSliderPosition();

                // this is considered 'moving the slider'! Invoke both Down + Up
                if (SliderMouseDown != null)
                {
                    SliderMouseDown.Invoke(this, EventArgs); 
                }

                if (SliderMouseUp != null)
                {
                    SliderMouseUp.Invoke(this, EventArgs); // necessary to make the slider position have an effect on the form
                }               
            }
        }

       
        #endregion

        bool hoverEnabled = false;
        private void EnableHover()
        {
            valueBar.SetSkinLocation(SkinState.Hover, valueBarRect, Color.DarkGray, Color.DarkGray);
            underBar.SetSkinLocation(SkinState.Hover, underBarRect, Color.DarkGray, Color.DarkGray);
            valueBar.SetSkinLocation(SkinState.HoverDisabled, valueBarRect, Color.DarkGray, Color.DarkGray);
            underBar.SetSkinLocation(SkinState.HoverDisabled, underBarRect, Color.DarkGray, Color.DarkGray);
                  
            hoverEnabled = true;
        }


        public string GetDisplayValue(int value)
        {
            if (DisplayValueFunction != null)
            {
                return DisplayValueFunction(value);
            }
            else
            {
                return value.ToString();
            }
        }


        protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOver(sender, args);

            if (hoverEnabled)
            {
                if (Enabled)
                {
                    valueBar.CurrentSkinState = SkinState.Hover;
                    underBar.CurrentSkinState = SkinState.Hover;
                }
                else
                {
                    valueBar.CurrentSkinState = SkinState.HoverDisabled;
                    underBar.CurrentSkinState = SkinState.HoverDisabled;
                }               
            }

        }

        protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
        {
            base.OnMouseOut(sender, args);

            if (hoverEnabled)
            {
                if (Enabled)
                {
                    valueBar.CurrentSkinState = SkinState.Normal;
                    underBar.CurrentSkinState = SkinState.Normal;
                }
                else
                {
                    valueBar.CurrentSkinState = SkinState.Disabled;
                    underBar.CurrentSkinState = SkinState.Disabled;
                }
            }
        }


        private void OnLoseFocusReset()
        {
            /*   if (isDragging)
           {

               this.isDragging = false;

               if (EndMoving != null)
                   EndMoving.Invoke(this);
           }*/

            isIncreasing = false;
            isDecreasing = false;
            

        }

        /// <summary>
        /// we get this when going modal - end resizing cleanly.
        /// </summary>
        protected override void OnLoseFocus()
        {
            base.OnLoseFocus();

            OnLoseFocusReset();

        
        }

        private void SetValues(int barValue, int sliderValue, int maxValue)
        {
            //this.value = Value;
            //this.maxValue = MaxValue;

            
            switch (this.type)
            {

                case FillableBarType.Default:
                    this.barValue = barValue;

                    if (barValue > maxValue)
                        this.barValue = maxValue;

                    this.maxValue = maxValue;

                    
                    this.Text = "" + this.Value + "/" + this.MaxValue;
                    break;

                case FillableBarType.ProgressBar:
                    this.barValue = barValue;

                    if (barValue > maxValue)
                    {
                        this.barValue = maxValue;
                        //TODO: Invoke Finished
                        this.Text = "" + this.barValue + "%";
                    }
                    this.maxValue = maxValue;
                    break;

                case FillableBarType.StockOrders:

                    //int valuetouse = 0;
                    if (slider.Value >= slider2.Value && slider.Value >= slider3.Value)
                    {
                        this.maxValue = slider.Value;
                    }
                    else if (slider2.Value >= slider.Value && slider2.Value >= slider3.Value)
                    {
                        this.maxValue = slider2.Value;
                    }
                    else if (slider3.Value >= slider.Value && slider3.Value >= slider2.Value)
                    {
                        this.maxValue = slider3.Value;
                    }

                    /*if (MaxValue > this.maxValue)
                        this.maxValue = MaxValue;*/
                    this.barValue = slider3.Value;
                    this.Text = "" + barValue;

                    break;
                //this.maxValue = valuetouse;

                case FillableBarType.HUDSlider:
                case FillableBarType.HUDSliderWhite:
                case FillableBarType.LCDSlider:
                case FillableBarType.LCDSliderWhite:
             //   case FillableBarType.LCDIndicator:

                    // value bar and slider arre locked together.
                    barValue = Util.Clamp(barValue, 0, maxValue);
                  

                    this.barValue = barValue;

                    this.maxValue = maxValue;

                    slider.Value = barValue;
                                        

                    this.Text = GetDisplayValue(MaxValue);

                    break;

                case FillableBarType.LCDIndicator:
                      
                    // value bar and slider can differ

                    barValue = Util.Clamp(barValue, 0, maxValue);
                    sliderValue = Util.Clamp(sliderValue, 0, maxValue);
                  

                    this.barValue = barValue;

                    this.maxValue = maxValue;

                    this.slider.Value = sliderValue;
                    
                    this.Text = MaxValue.ToString(); 

                    break;
            }

            UpdateSizesAndPositionsWithNewWidth(); // also needed here?

            //this.OnResize(this); // calls it directly, hmmm
        }


        

        public int KnobWidth
        {
            set
            {
                slider.KnobWidth = value;
            }
        }


        public FillableBarSlider.ShowValueLabelModes ShowValueLabel
        {
            get
            {
                return slider.ShowValueLabel;
            }

            set
            {
                slider.ShowValueLabel = value;
            }
        }

        public int SliderScaleStartX
        {
            get
            {
                return GetBarLeftPos(); // barLeftMargin;
            }
        }

        private bool showMaxValueLabelAtEnd = true;
        public bool ShowMaxValueLabelAtEnd
        {
            get
            {
                return showMaxValueLabelAtEnd;
            }

            set
            {
                if (value != showMaxValueLabelAtEnd)
                {
                    showMaxValueLabelAtEnd = value;
                    if (showMaxValueLabelAtEnd == false)
                    {
                        Remove(lblMaxValue);
                    }
                    else
                    {
                        Add(lblMaxValue);
                    }
                }

            }

        }

        private const int buttonMargin = 0; // 2;

        private const int distanceToMaxValueLabel = 24;

        /// <summary>
        /// this leaves a gap to the left for the slider to position with its middle at the 0 position
        /// 
        /// don't use directly - if buttons are included, a further distance gets added to this
        /// </summary>
        private int barLeftMargin = 8; //6;

        /// <summary>
        /// same for the righmost slider position
        /// </summary>
        private int barRightMargin = 10; 


        /// <summary>
        /// This is necessary to call when setting the value programmatically (not via mouse) to avoid infinite loops between the bar control and the slider control
        /// </summary>
        public void UpdateSliderPosition()
        {
            slider.UpdateSliderPositionAndSize(null);

            UpdateValueBarWidth();
        }

        public void SetTags()
        {
            underBar.DebugTag = "underBar";
            valueBar.DebugTag = "valueBar";
        }

        /*protected override void OnMouseDown(InputEventSystem.MouseEventArgs args)
        {
            base.OnMouseDown(args);

            /*if (args.Button == InputEventSystem.MouseButtons.Left)
                Value += 1;
            else if (args.Button == InputEventSystem.MouseButtons.Right)
                MaxValue += 1;*/
        //}


        
        protected override void OnMouseWheelChanged(int wheelChange)
        {
            base.OnMouseWheelChanged(wheelChange);

            // TODO: figure out how to fire the SliderUp event when the user stops scrolling the mouse wheel...
            return;

            if (slider != null)
            {
                if (wheelChange > 0)
                {
                    Value += 1;
                }
                else if (wheelChange < 0)
                {
                    Value -= 1;
                }
              //  Value += wheelChange;
                UpdateSliderPosition();
            }
        }

        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
               
                if (base.Width != value)
                {
                    base.Width = value; // this calls OnResize which will reposition buttons etc. Let's call it after the bar widths have been set

                    int barWidth = Width;
                    if (type == FillableBarType.HUDSlider
                     || type == FillableBarType.HUDSliderWhite)
                    {
                        barWidth = Width - 58; // make room for max label // 50;
                    }
                    else if (type == FillableBarType.LCDSlider
                          || type == FillableBarType.LCDSliderWhite)
                    {
                        barWidth = Width - 45; // make room for max label (3 digits) 40;
                    }

                    if (tbDecrease != null)
                    {
                        barWidth = barWidth - tbDecrease.Width - tbIncrease.Width - 4 * buttonMargin;
                    }

                    this.underBar.Width = barWidth;
                    this.underBar.MaxWidth = barWidth;
                    this.underBarRect.Width = barWidth;

                    UpdateSizesAndPositionsWithNewWidth();
                }
            }
        }

      /*  public void SetBarWidth(int width)
        {
            this.underBar.Width = width;
            this.underBar.MaxWidth = width;
            this.underBarRect.Width = width;
        }*/

        int GetDistanceToMaxLabel()
        {
            if (tbIncrease != null)
            {
                return distanceToMaxValueLabel + tbIncrease.Width + 2 * buttonMargin;
            }
            else return distanceToMaxValueLabel;
        }

        int GetBarLeftPos()
        {
            if (tbDecrease != null)
            {
                return barLeftMargin + tbDecrease.Width + 2 * buttonMargin;
            }
            else return barLeftMargin;
        }

        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            UpdateSizesAndPositionsWithNewWidth();
        }

        private void UpdateSizesAndPositionsWithNewWidth()
        {
            if (this.type == FillableBarType.StockOrders)
            {
                int valueToUse = 0;

                if (slider.X >= slider2.X && slider.X >= slider3.X)
                    valueToUse = slider.X;
                else if (slider2.X >= slider.X && slider2.X >= slider3.X)
                    valueToUse = slider2.X;
                else if (slider3.X >= slider.X && slider3.X >= slider2.X)
                    valueToUse = slider3.X;

                valueToUse += 30;

                if (!(Width == valueToUse) && (valueToUse) > MinWidth && valueToUse <= 230)
                    Width = valueToUse;

                underBar.Position = new Point(0, 20); // multiple line layout - move the bar down?
            }

            int underBarWidth = this.Width;

            if (ShowMaxValueLabelAtEnd)
            {
                underBarWidth -= distanceToMaxValueLabel;
            }
            else if (slider != null)
            {
                underBarWidth -= GetBarLeftPos(); // barLeftMargin;
            }


            underBar.Width = underBarWidth;

            if (valueBar != null)
            {
                //valueBar.Position = new Point(underBar.Position.X + underBar.EdgeSize, underBar.Position.Y + underBar.EdgeSize);
                UpdateValueBarWidth();
            }

            if (this.type == FillableBarType.ProgressBar)
            {
                valueBar.Height = Height;
                underBar.Height = Height;
            }

            if (this.type == FillableBarType.StockOrders)
            {
                lblMaxValue.Position = new Point(valueBar.Position.X + valueBar.Width - (lblMaxValue.TextWidth / 2), underBar.Y + (underBar.Height / 2 - lblMaxValue.TextHeight / 2));

                if (lblMaxValue.X < 10 || this.maxValue == 0)
                    lblMaxValue.Position = new Point(10, underBar.Y + (underBar.Height / 2 - lblMaxValue.TextHeight / 2));
            }
            else
            {

                if (tbIncrease != null)
                {
                    tbIncrease.X = underBar.Right + barRightMargin; // buttonMargin;

                    this.lblMaxValue.X = tbIncrease.Right + buttonMargin;

                }
                else
                {
                    this.lblMaxValue.X = underBar.Right + barRightMargin;
                }

            }
        }

        private void UpdateValueBarWidth()
        {
            
            valueBar.Position = underBar.Position; // valuebar is flush with underbar

            int width;

            if (maxValue == 0)
            {
                width = underBar.Width;
            }
            else
            {
                if (slider != null && ValueBarAndSliderAreLocked)
                {
                    width = slider.X - valueBar.X + 4;
                }
                else
                {
                    width = (int)(((this.barValue / (float)(this.maxValue)) * underBar.Width));
                }
            }

            valueBar.Width = width; // minWidth is 1


        /*    valueBar.Visible = false;
            return;*/

            if (width <= 1)
            {
                valueBar.Visible = false;
            }
            else
            {
                valueBar.Visible = true;
            }
        }
    }
}
