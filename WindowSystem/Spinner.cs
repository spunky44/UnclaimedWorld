using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem
{
    public class Spinner: UIComponent
    {
        public event CountChangedHandler CountChanged;
        public delegate void CountChangedHandler(int newCount, EventArgs e);

        protected ImageButton btIncrease, btDecrease;

        protected TextBox tbNumber;

        protected int noOfDigits = 3;

        protected int defaultWidth = 40;

        private double seconds;
        private bool increaseIsPressed = false, decreaseIsPressed = false;
        
        private int maxValue = 10;

        private float increaseFraction = 0f;
        private bool changedCounter = false;
        private float currentSpeed;
        private const float acceleration = 8;
        private const float maxSpeed = 100; // per second
        private const float minSpeed = 3;

        public enum SpinnerType { LCD, HUD }

        public Spinner(GUIManager guiManager)
            : base(guiManager)
        {
            #region Create Child Controls
            btIncrease = new ImageButton(guiManager);
            btDecrease = new ImageButton(guiManager);
            tbNumber = new TextBox(guiManager);
            #endregion

            #region Add Child Controls
            Add(btIncrease);
            Add(btDecrease);
            Add(tbNumber);
            #endregion

            Width = defaultWidth;

            tbNumber.IsEditable = true;

            btIncrease.MouseDown += new InputEventSystem.MouseDownHandler(btIncrease_MouseDown);
            btIncrease.MouseUp += new InputEventSystem.MouseUpHandler(btIncrease_MouseUp);

            btDecrease.MouseDown += new InputEventSystem.MouseDownHandler(btDecrease_MouseDown);
            btDecrease.MouseUp += new InputEventSystem.MouseUpHandler(btDecrease_MouseUp);
            NoOfDigits = 3;
        }

        public override string ToolTip
        {
            get
            {
                return base.ToolTip;
            }
            set
            {
                base.ToolTip = value;
                btIncrease.ToolTip = value;
                btDecrease.ToolTip = value;
            }
        }

        void btDecrease_MouseUp(InputEventSystem.MouseEventArgs args)
        {
            increaseIsPressed = false;
            decreaseIsPressed = false;
            currentSpeed = minSpeed;
            increaseFraction = 0f;

            if (!changedCounter)
            {
                Increase(-1);
            }
            changedCounter = false;
        }

        void btIncrease_MouseUp(InputEventSystem.MouseEventArgs args)
        {
            increaseIsPressed = false;
            decreaseIsPressed = false;
            currentSpeed = minSpeed;
            increaseFraction = 0f;

            if (!changedCounter)
            {
                Increase(1);
            }
            changedCounter = false;
        }

        void btDecrease_MouseDown(InputEventSystem.MouseEventArgs args)
        {
            decreaseIsPressed = true;
        }

        void btIncrease_MouseDown(InputEventSystem.MouseEventArgs args)
        {            
            increaseIsPressed = true;
        }

        private void Increase(int amount)
        {
            changedCounter = true;

            int oldCount = Count;
            int newCount = oldCount + amount;

            if (newCount > maxValue)
            {
                newCount = maxValue;
            }
            else if (newCount < 0)
            {
                newCount = 0;
            }

            if (newCount != oldCount)
            {
                Count = newCount;

                if (CountChanged != null)
                {
                    CountChanged.Invoke(Count, EventArgs);
                }
            }
        }

        public void Init(SpinnerType type)
        {
           // this.type = type;
         
            switch (type)
            {
                case SpinnerType.LCD:
                    btIncrease.Init(ImageButtonType.LCDIncrease);
                    btDecrease.Init(ImageButtonType.LCDDecrease);
                                        
                    Font = GUIManager.LCDandHUDFont;

                    tbNumber.IsEditable = false;
                    tbNumber.Y = 1;

                    Height = btIncrease.Height + btDecrease.Height; // 2 * btIncrease.Height;

                    //Color = Color.White;
                    break;

                case SpinnerType.HUD:
                    btIncrease.Init(ImageButtonType.HUDIncrease);
                    btDecrease.Init(ImageButtonType.HUDDecrease);
                                        
                    Font = GUIManager.LCDandHUDFont;

                    tbNumber.IsEditable = true;
                    tbNumber.Y = 1;
                    tbNumber.Init(TextBox.TextBoxType.HUD);

                    Height = btIncrease.Height + btDecrease.Height; // 2 * btIncrease.Height;

                    //Color = Color.White;
                    break;

            }

        }

        public int NoOfDigits
        {
            get { return noOfDigits; }
            set 
            { 
                noOfDigits = value;
                string digit = "";
                tbNumber.Width = tbNumber.GetTextWidth(digit.PadRight(noOfDigits, '9'));
                maxValue = (int)Math.Pow(10, noOfDigits) - 1;

                Width = tbNumber.Width + btIncrease.Width;
            }
        }

        public int Count
        {
            get { return int.Parse(tbNumber.Text); }
            set
            {
                this.tbNumber.Text = value.ToString();                
            }
        }

        /// <summary>
        /// The font used to draw button text.
        /// </summary>
        /// <value>Must not be a valid path.</value>
        public SpriteFont Font
        {
            set
            {
                tbNumber.Font = value;                
            }
        }

     /*   public Color Color
        {
            set
            {
                tbNumber.Color = value;
            }
        }*/


        /// <summary>
        /// Handle the acceleration here
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (increaseIsPressed)
            {
                // increaseFraction += (float)(gameTime.ElapsedRealTime.TotalSeconds * currentSpeed);  // XNA 3 // use REAL time for this...
                increaseFraction += (float)(gameTime.ElapsedGameTime.TotalSeconds * currentSpeed);  

                if (increaseFraction >= 1)
                {
                    Increase((int)increaseFraction);

                    increaseFraction = Math.Max(0f, increaseFraction - (int)increaseFraction);
                }

                if (currentSpeed < maxSpeed)
                {
                    //currentSpeed += (float)(gameTime.ElapsedRealTime.TotalSeconds * acceleration); // XNA 3 // use REAL time for this...
                    currentSpeed += (float)(gameTime.ElapsedGameTime.TotalSeconds * acceleration); 
                }

                if (currentSpeed > maxSpeed)
                {
                    currentSpeed = maxSpeed;
                }
            }
            else if (decreaseIsPressed)
            {
                increaseFraction += (float)(gameTime.ElapsedGameTime.TotalSeconds * currentSpeed);  // // XNA 3 // use REAL time for this...
                if (increaseFraction >= 1)
                {
                    Increase(-(int)increaseFraction);

                    increaseFraction = Math.Max(0f, increaseFraction - (int)increaseFraction);
                }

                if (currentSpeed < maxSpeed)
                {
                    currentSpeed += (float)(gameTime.ElapsedGameTime.TotalSeconds * acceleration); // XNA 3 //  use REAL time for this...
                }

                if (currentSpeed > maxSpeed)
                {
                    currentSpeed = maxSpeed;
                }

            }
            
        /*    if (GUIManager.GetFocus() != this)
            {
                if (this.label.IsCursorShown)
                {
                    this.label.IsCursorShown = false;
                    this.seconds = 0;
                }
            }
            else
            {
                this.seconds += gameTime.ElapsedRealTime.TotalSeconds;

                if (this.seconds > 0.5)
                    this.label.IsCursorShown = false;
                else
                    this.label.IsCursorShown = true;

                if (this.seconds > 1)
                    this.seconds = 0;
            }
            */
            base.Update(gameTime);
        }

        /// <summary>
        /// Refresh child controls when control is resized.
        /// </summary>
        /// <param name="sender">Resized control.</param>
        protected override void OnResize(UIComponent sender)
        {
            base.OnResize(sender);

            if (btIncrease != null && tbNumber != null)
            {
                // button widths are fixed:
                tbNumber.Width = Width - btIncrease.Width;
                tbNumber.Height = Height;

                btIncrease.X = tbNumber.Width;
                btDecrease.X = tbNumber.Width;

                btIncrease.Y = 0;
                btDecrease.Y = btIncrease.Height;//Height / 2;


            }

        }


    }
}
