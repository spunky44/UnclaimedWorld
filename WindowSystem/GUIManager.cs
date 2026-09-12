#region File Description
//-----------------------------------------------------------------------------
// File:      GUIManager.cs
// Namespace: WindowSystem
// Author:    Aaron MacDougall
//-----------------------------------------------------------------------------
#endregion

#region License
//-----------------------------------------------------------------------------
// Copyright (c) 2007, Aaron MacDougall
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of Aaron MacDougall nor the names of its contributors may
//   be used to endorse or promote products derived from this software without
//   specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using InputEventSystem;
using SpriteSheetRuntime;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace WindowSystem
{
    /// <summary>
    /// This is a game component that implements IUpdateable. It manages GUI
    /// components, in particular passing on events and handling focus.
    /// Don´t use Game.Window.Client bounds for getting the screen boundaries, it changes if we minimize the game. Use ScreenHeight and ScreenWidth instead.
    /// </summary>
    public partial class GUIManager //: DrawableGameComponent
    {
        #region Default Properties
        private static string defaultSkinTexture = "Textures/DefaultStyle";//"Content/Textures/DefaultStyle";
        #endregion

        #region Fields
        private static ContentManager contentManager;
       
        public InputData InputData;
        

        /// <summary>
        /// only contains the windows that are currently displayed!
        /// </summary>
        private Dictionary<Level, List<UIComponent>> controls;

        private HashSet<Window> allWindows;

        /// <summary>
        /// Only this control should be allowed to handle mouse and keyboard events
        /// </summary>
        private UIComponent focusedControl;

        /// <summary>
        /// only this control should handle the mouse scroll wheel event
        /// </summary>
        private UIComponent mouseWheelTarget;


        //   private MouseCursor mouseCursor;
        private UIComponent modalControl;

        public MouseSprites MouseSprite;

        /// <summary>
        /// only ingame, hmmm...
        /// </summary>
        public Dictionary<string, Color> CustomColors = new Dictionary<string,Color>();


        public static string LCDandHUDSubheadingFontPath = "Fonts/LCDandHUDSubHeading";
        public static SpriteFont LCDandHUDSubheadingFont;

        public static string LCDInterfaceBoldFontPath = "Fonts/LCD_bold";
        public static SpriteFont LCDInterfaceBoldFont;

              
     
        /// <summary>
        /// bitmap font, 8pt
        /// still used in map editor panel
        /// </summary>
        public static string MediumButtonFaceFontPath = "Fonts/newtown_8pt"; //"Content/Fonts/NewtownButtonFace";
        public static SpriteFont MediumButtonInterfaceFont;

       
        public static string CRTGlowFontPath = "Fonts/CRTGlow";
        public static SpriteFont CRTBigGlowFont;

        public static string LCDandHUDBodyFontPath = "Fonts/LCDandHUDBody"; // works great
        /// <summary>
        /// NEW: also used for panel buttons
        /// </summary>
        public static SpriteFont LCDandHUDFont;

        public static SpriteFont VeryLargeInterfaceFont;


        /*   public static string CRTBasicFontPath = "Content/Fonts/CRTBasic";//"Content/Fonts/CRT_18pt";
           public static SpriteFont CRTBasicFont;*/
        public static string CRTBasicFontPath = "Fonts/CRT_18pt";
        public static SpriteFont CRTBasicFont;

             

        public static SoundEffect Click1;
        // public static SoundEffect Bip1;
        // public static SoundEffect Clack;

        public static SoundEffect BeepBasicPanel;
        public static SoundEffect BeepLCD;
        public static SoundEffect BeepMainPanel;
        public static SoundEffect BeepMetalPanel;

        // public static SoundEffect Typing;
        public static SoundEffect WhiteNoise;
        public static SoundEffect CRTTurnOn;

        public static SoundEffect PlaceBuildingBeep;

        // public float MasterSFXVolume = 0.5f;


        /// my own sprite sheet from GameEngine
        public SpriteSheet GUISpriteSheet, GUI_CRT_SpriteSheet;

        private SpriteBatch spriteBatch;



        //   public bool mouseIsInInterface { get; set; }

        public Level LCDLevel = Level.RockBottom;
        public bool MouseIsInLCDInterface; // { get; set; }

        //    public bool MouseIsInHUD;

        // public SpriteFont LCDInterfaceFont;

        #endregion

        public delegate void ShowTooltipHandler(UIComponent sender);
        public delegate void WindowClosedHandler(Window sender);

        public delegate void HyperlinkClickedHandler(uint? entityID, uint? containerID, uint? zoneID, Point? mapPos /*IHyperlinkTarget clickedEntity*/, MouseButtonClicked button);

        public delegate void SetMouseCursorHandler(MouseSprites mouseSprite);

        #region Events
        public event ShowTooltipHandler ShowTooltip;
        public event ShowTooltipHandler HideTooltip;

        public event HyperlinkClickedHandler HyperlinkClicked;
        public event SetMouseCursorHandler SetMouseCursorEvent;

        public event WindowClosedHandler WindowClosed;

        public event Action<Window> MouseOverWindow;
        public event Action<Window> MouseOutOfWindow;


        #endregion

        #region Properties
        /// <summary>
        /// Get/Set the content manager used by the GUI system.
        /// </summary>
        /// <value>Must not be null.</value>
        public /*static*/ ContentManager ContentManager
        {
            get { return contentManager; }
            set
            {
                Debug.Assert(value != null);
                contentManager = value;
            }
        }

        /// <summary>
        /// Gets the texture containing the graphics of all GUI components.
        /// </summary>
        /*   internal Texture2D SkinTexture
           {
               get { return this.skinTexture; }
           }
           */

        internal Texture2D SkinTexture
        {
            get { return GUISpriteSheet.Texture; }
        }

        /// <summary>
        /// Gets the mouse cursor.
        /// </summary>
        /*  internal MouseCursor MouseCursor
          {
              get { return this.mouseCursor; }
          }*/



        //  public bool MouseIsInInterface;

        #endregion


        public Game Game;

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The currently running Game object.</param>
        public GUIManager(Game game, int width, int height, InputData inputData, ContentManager content, bool receiveInputEvents = true)
        // : base(game)
        {
            this.Game = game;
            this.ScreenWidth = width;
            this.ScreenHeight = height;

            ContentManager = content;

            InputData = inputData;

            this.controls = new Dictionary<Level, List<UIComponent>>(); //new List<UIComponent>();
            controls.Add(Level.FoggyBottom, new List<UIComponent>());
            controls.Add(Level.RockBottom, new List<UIComponent>());
            controls.Add(Level.Bottom, new List<UIComponent>());

            controls.Add(Level.BelowBelowBelowMiddle, new List<UIComponent>());
            controls.Add(Level.BelowBelowMiddle, new List<UIComponent>());
            controls.Add(Level.BelowMiddle, new List<UIComponent>());
            controls.Add(Level.Middle, new List<UIComponent>());

            controls.Add(Level.Dialogs, new List<UIComponent>());
            controls.Add(Level.StackedDialogs, new List<UIComponent>());

            controls.Add(Level.EntityTypeInfo, new List<UIComponent>());
            controls.Add(Level.EventDialog, new List<UIComponent>());
            controls.Add(Level.Menu, new List<UIComponent>());
            controls.Add(Level.MessageBox, new List<UIComponent>());
            controls.Add(Level.Tooltip, new List<UIComponent>());
            controls.Add(Level.ComboBoxList, new List<UIComponent>());

            allWindows = new HashSet<Window>();

            this.focusedControl = null;
            this.modalControl = null;

            // Ensure this and mouse are always drawn on top, mouse is MaxValue
            // this.DrawOrder = int.MaxValue - 1; GUICHANGE

            // Get input event system, and register event handlers
            //this.inputEvents = (IInputEventsService)this.Game.Services.GetService(typeof(IInputEventsService));
            if (receiveInputEvents)
            {
                // remember to unregister to prevent memory leaks for load and new game!
                InputData.RequestingFocus += RequestingFocus;
                InputData.MouseWheelMove += InputData_MouseWheelMove;

                InputData.KeyDown += InputData_KeyDown;
                InputData.KeyUp += InputData_KeyUp;

                InputData.MouseMove += InputData_MouseMove; // CheckMouseStatus; NEW #EVENTCHG
                InputData.MouseDown += InputData_MouseDown; // NEW #EVENTCHG
                InputData.MouseUp += InputData_MouseUp; // NEW #EVENTCHG
            }

            // Create graphical mouse cursor
            /*   this.mouseCursor = new MouseCursor(game, this);
               this.mouseCursor.Initialize();
               */

            LoadContent();

        }


        #endregion


        

        void InputData_KeyDown(KeyEventArgs args) // NEW #EVENTCHG
        {
            if (focusedControl != null)
            {
                focusedControl.KeyDownIntercept(args);
            }

        }

        void InputData_KeyUp(KeyEventArgs args) // NEW #EVENTCHG
        {
            if (focusedControl != null)
            {
                focusedControl.KeyUpIntercept(args);
            }

        }

        void InputData_MouseDown(MouseEventArgs args) // NEW #EVENTCHG
        {
            if (focusedControl != null)
            {
                focusedControl.MouseDownIntercept(args);
            }

        }

        void InputData_MouseUp(MouseEventArgs args) // NEW #EVENTCHG
        {
            if (focusedControl != null)
            {
                focusedControl.MouseUpIntercept(args);
            }

        }


        void InputData_MouseMove(MouseEventArgs args)
        {
            if (focusedControl != null)
            {
                focusedControl.MouseMoveIntercept(args);
            }

            CheckMouseStatus(args);
        }

        void InputData_MouseWheelMove(int wheelChange)
        {
            if (mouseWheelTarget != null)
            {
                mouseWheelTarget.MouseWheelIntercept(wheelChange);
            }
        }

        public void SetMousePosition(int x, int y)
        {
            if (InputData != null)
            {
                InputData.SetMousePosition(x, y); //) mouseSet.Invoke(x, y);
            }
        }

        public void Destroy()
        {
            if (InputData != null)
            {
                // cut the references to InputData (which lives in Controller for the entire app session), this should prevent mmemory leaks:
                InputData.RequestingFocus -= RequestingFocus;
                InputData.MouseMove -= InputData_MouseMove;
                InputData.MouseWheelMove -= InputData_MouseWheelMove;

                InputData.KeyDown -= InputData_KeyDown;
                InputData.KeyUp -= InputData_KeyUp;

                InputData.MouseDown -= InputData_MouseDown;
                InputData.MouseUp -= InputData_MouseUp;
            }

            foreach (KeyValuePair<Level, List<UIComponent>> levelControls in controls)
            {
                foreach (UIComponent control in levelControls.Value)
                {
                    control.Destroy();

                }
            }

            /* foreach (var item in allWindows)
             {
                 if (item.DebugTag == "DataTypeTooltip")
                 {

                 }

                 // this will not reach controls which are not currently added to a form. This is common for various expansion panels etc.
                 item.Destroy();
             }*/

            allWindows.Clear();

            controls.Clear();

            // here we attempt to release all references in case the guimanager cannot be GC'ed...
            focusedControl = null;
            modalControl = null;

            HideTooltip = null;
            HyperlinkClicked = null;
            SetMouseCursorEvent = null;
            ShowTooltip = null;
            WindowClosed = null;

            /*
              // when we are removed, we are not unregistered from GraphicsDeviceManager.DeviceDisposing?? this prevents GUIManager from being GC'ed :(
              game.Components.Remove(this);

         
              base.Dispose(true);*/
        }

        /// <summary>
        /// Create SpriteBatch object and load default graphics.
        /// </summary>
        /// <param name="loadAllContent">Which type of content to load.</param>     
        protected /*override*/ void LoadContent()
        {

            //SkinTextureFileName = defaultSkinTexture;

            GUISpriteSheet = ContentManager.Load<SpriteSheet>("GUI\\GUISprites");
            GUI_CRT_SpriteSheet = ContentManager.Load<SpriteSheet>("GUI\\GUI_CRT_Sprites");


            CRTBigGlowFont = ContentManager.Load<SpriteFont>(CRTGlowFontPath);
            CRTBigGlowFont.Spacing = -3;

            CRTBasicFont = ContentManager.Load<SpriteFont>(CRTBasicFontPath);
            CRTBasicFont.Spacing = -4;


            LCDandHUDFont = ContentManager.Load<SpriteFont>(LCDandHUDBodyFontPath);
            //LCDandHUDFont.Spacing = -1f;
            LCDandHUDFont.LineSpacing = 17;
            LCDandHUDSubheadingFont = ContentManager.Load<SpriteFont>(LCDandHUDSubheadingFontPath);

            LCDInterfaceBoldFont = ContentManager.Load<SpriteFont>(LCDInterfaceBoldFontPath);
          
            MediumButtonInterfaceFont = ContentManager.Load<SpriteFont>(MediumButtonFaceFontPath);
            MediumButtonInterfaceFont.Spacing = -2; //-1;

            //    ButtonFont = ContentManager.Load<SpriteFont>(ButtonFontPath);
            //   ButtonFont.LineSpacing = 14; // sets TextHeight correctly, otherwise the the text is cut off in the middle... .spritefont/Nuclex needs this..?
            
            Click1 = ContentManager.Load<SoundEffect>("Sounds/CLICK14A_lowVolume");

            //  base.LoadContent();

            BeepBasicPanel = ContentManager.Load<SoundEffect>("Sounds/button beeps/Click3b");
            BeepLCD = ContentManager.Load<SoundEffect>("Sounds/button beeps/lcd_buttonclick_lowVolume");
            BeepMainPanel = ContentManager.Load<SoundEffect>("Sounds/button beeps/menuBeep");
            BeepMetalPanel = ContentManager.Load<SoundEffect>("Sounds/button beeps/metalpanel button_lowVolume");
            // Typing = ContentManager.Load<SoundEffect>("Sounds/typing/typing_lowVolume");
            CRTTurnOn = ContentManager.Load<SoundEffect>("Sounds/tv/tvStaticTurnOn");
            WhiteNoise = ContentManager.Load<SoundEffect>("Sounds/tv/whitenoise_lowVolume");
            PlaceBuildingBeep = ContentManager.Load<SoundEffect>("Sounds/BIP2_lowVolume");


            // MLo: spriteBatch is now constructed just-in-time in Draw()
            // this.spriteBatch = new SpriteBatch(GraphicsDevice);

        }

        public void PlaySound(SoundEffect sound)
        {
            sound.Play(); //MasterSFXVolume, 0f, 0f);
        }

        public void ShowToolTip(UIComponent control)
        {
            if (ShowTooltip != null)
                ShowTooltip.Invoke(control);
        }

        public void HideToolTip(UIComponent control)
        {
            if (HideTooltip != null)
                HideTooltip.Invoke(control);
        }

        public void WindowWasClosed(Window window)
        {
            if (WindowClosed != null)
                WindowClosed.Invoke(window);
        }

        public void MouseIsOverWindow(Window window)
        {
            if (MouseOverWindow != null)
                MouseOverWindow.Invoke(window);
        }

        public void MouseIsOutOfWindow(Window window)
        {
            if (MouseOutOfWindow != null)
                MouseOutOfWindow.Invoke(window);
        }


        public enum MouseButtonClicked { Left, Right }
        public void HyperLinkClicked(uint? entityID, uint? resourceContainerID, uint? zoneID, Point? mapPos, MouseButtonClicked button)
        {
            if (HyperlinkClicked != null)
                HyperlinkClicked.Invoke(entityID, resourceContainerID, zoneID, mapPos, button);
        }

        /// <summary>       
        /// </summary>
        protected /*override*/ void UnloadContent()
        {
            // Only gets called on app end...
            foreach (KeyValuePair<Level, List<UIComponent>> levelControls in controls)
            {
                foreach (UIComponent control in levelControls.Value)
                {
                    control.UnloadGraphicsContent(true); // unloadAllContent);

                }
            }

        }




        /// <summary>
        /// Update all child components.
        /// </summary>      
        public /*override*/ void Update(GameTime gameTime)
        {
            // Warning! This method gets called once after we have removed the GUIManager component from the Game.Components collection!

            List<Window> controlsToRemove = null;
            foreach (KeyValuePair<Level, List<UIComponent>> levelControls in controls)
            {
                foreach (UIComponent control in levelControls.Value)
                {
                    //if (control.Enabled)
                    control.Update(gameTime);

                    Window window = control as Window;
                    if (window != null && window.HideThisNow)
                    { // we cannot remove from teh collection while iterating
                        if (controlsToRemove == null)
                        {
                            controlsToRemove = new List<Window>();
                        }
                        controlsToRemove.Add(window);
                        window.HideThisNow = false;
                    }
                }
            }

            if (controlsToRemove != null)
            {
                foreach (Window control in controlsToRemove)
                {
                    control.CloseWindow();
                }
            }

            //  base.Update(gameTime);
        }

        /// <summary>
        /// Adds a top-level control if it hasn't already been added. Also
        /// initializes control.
        /// Important! Set Level first!!!
        /// </summary>
        /// <param name="control">Control to add.</param>
        public void Add(UIComponent control)
        {
            List<UIComponent> levelControls;
            Window window = control as Window;
            if (window != null)
            {
                levelControls = controls[window.Level];
            }
            else
            {
                levelControls = controls[control.Level];
                //  levelControls = controls[Level.Bottom];
            }


            if (!levelControls.Contains(control))
            {
                control.Parent = null;
                control.Initialize();
                levelControls.Add(control);
            }
        }

        public void EndSpriteBatch()
        {
            if (spriteBatch == null)
                return;

            spriteBatch.End();
        }
        public void BeginSpriteBatch()
        {
            if (spriteBatch == null)
                return;
            //  spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None); // XNA 3
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        }

        /// <summary>
        /// register top level components (Windows) to do cleanup later
        /// </summary>
        /// <param name="window"></param>
        public void RegisterWindow(Window window)
        {
            allWindows.Add(window);
        }


        /// <summary>
        /// Removes a top-level control.
        /// </summary>
        /// <param name="control">Control to remove.</param>
        public void Remove(UIComponent control)
        {
            List<UIComponent> levelControls;
            Window window = control as Window;
            if (window != null)
            {
                levelControls = controls[window.Level]; // WARNING! Level is 2 different properties!!

            }
            else
            {
                levelControls = controls[control.Level]; // WARNING! Level is 2 different properties!!              
            }



            if (levelControls.Remove(control))
                control.CleanUp();
        }

        /// <summary>
        /// Bring the control to the front. Currently only works with top-level
        /// controls.
        /// </summary>
        internal void BringToTop(UIComponent control)
        {
            List<UIComponent> levelControls;
            Window window = control as Window;
            if (window != null)
            {
                levelControls = controls[window.Level];
            }
            else
            {
                levelControls = controls[Level.Bottom];
            }

            if (levelControls.Remove(control))
                levelControls.Add(control);
        }

        public void BringToBottom(Window window)
        {
            List<UIComponent> levelControls;

            levelControls = controls[window.Level];

            if (levelControls.Remove(window))
                levelControls.Insert(0, window);
        }

        /// <summary>
        /// Accessor retrieves currently focused control.
        /// </summary>
        /// <returns>The currently focused control.</returns>
        public UIComponent GetFocus()
        {
            return this.focusedControl;
        }

        public UIComponent MouseWheelReceiver()
        {
            return this.mouseWheelTarget;
        }

        /// <summary>
        /// Sets the currently focused control. Also tells old and new controls
        /// about the change in focus.
        /// </summary>
        /// <param name="control">Control receiving focus.</param>
        public void SetFocus(UIComponent control)
        {

            if (control != this.focusedControl)
            {

                // Check that control can have focus
                if (control != null && !control.CanHaveFocus)
                    return;


                if (control != null && control.Visible == false)
                {
                    return;
                }

               
                // Update mFocusedControl here in case a control calls
                // GetFocus() in OnLoseFocus().
                UIComponent oldFocus = this.focusedControl;
                this.focusedControl = control;


                // Take focus from the last control with focus
                if (oldFocus != null)
                    oldFocus.TakeFocus();

                // Give control to new control
                if (this.focusedControl != null)
                {
                    this.focusedControl.GiveFocus();
                }
            }
        }

        /// <summary>
        /// Sets the current mousr cursor to be drawn.
        /// </summary>
        /// <param name="state">New mouse state.</param>
        public void SetMouseCursor(MouseSprites state)
        {
            MouseSprite = state;

            if (SetMouseCursorEvent != null)
            {
                SetMouseCursorEvent.Invoke(state);
            }


            //this.mouseCursor.SetMouseState(state);
        }

        /// <summary>
        /// Retrieves the current modal control. If null is returned, then no
        /// control has modal control.
        /// </summary>
        /// <returns>Modal control, or null if there is none.</returns>
        public UIComponent GetModal()
        {
            return this.modalControl;
        }

        /// <summary>
        /// Sets the modal control. Modal only allows modal control or it's
        /// children to receive focus. If null is passed, GUI is no longer in
        /// modal mode.
        /// </summary>
        /// <param name="modalControl">Control to set to modal.</param>
        public void SetModal(UIComponent modalControl)
        {
            this.modalControl = modalControl;

            SetFocus(null); // clear the currently focused control as it may no longer be allowed

        }





        public int ScreenHeight
        {
            get;
            private set;
            /*  get
              {
                  return Game.GraphicsDevice.Viewport.Height;
              }*/
        }

        public int ScreenWidth
        {
            get;
            private set;

           /* get
            {
                return Game.GraphicsDevice.Viewport.Width;
            }*/
        }

        public void Draw(GameTime gameTime, RenderType typesToRender, Level levelToDraw)
        {

            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            // spriteBatch = new SpriteBatch(GraphicsDevice);

            //  if (this.skinTexture != null)
            //   {
            // At this stage the scissor rectangle is the whole screen
            Rectangle parentScissor = new Rectangle(
                0,
                0,
                ScreenWidth, //  Game.GraphicsDevice.Viewport.Width,
                ScreenHeight // Game.GraphicsDevice.Viewport.Height
                );

            //  spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None); // XNA 3
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            List<UIComponent> levelControls = controls[levelToDraw];

            foreach (UIComponent control in levelControls)
            {

                Window window = control as Window;
                if (window != null)
                {   // optimization: cull windows that have no components to be drawn
                    if (window.Visible) // GCCHANGE: !!??
                    {
                       
                        switch (typesToRender)
                        {
                            case RenderType.CRTAndLCD:
                                if (!window.HasCRTOrLCDComponents)
                                    continue;
                                break;
                            case RenderType.Overlay:
                                if (!window.HasOverlayComponents)
                                    continue;
                                break;

                        }
                        control.Draw(this.spriteBatch, parentScissor, typesToRender, 1f);
                    }

                }
                else
                {
                    control.Draw(this.spriteBatch, parentScissor, typesToRender, 1f);
                }

                //   control.Draw(this.spriteBatch, parentScissor, typesToRender, 1f);

            }


            this.spriteBatch.End();



            //   base.Draw(gameTime);
        }

        #region Event Handlers

        /// <summary>
        /// Event handler called when requesting focus. Checks if a new control
        /// should receive focus. If so it brings it's top-level window to the
        /// top and calls SetFocus() with the focused control.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        private void RequestingFocus(MouseEventArgs args, TestMode mode)
        {
            UIComponent result = null;
            UIComponent parentControl = null;

            // Asks each top-level control if it or it's children should
            // receive focus.
            foreach (KeyValuePair<Level, List<UIComponent>> levelControls in controls)
            {
                foreach (UIComponent control in levelControls.Value)
                {
                    if ((mode == TestMode.Focus && control.CanHaveFocus)
                     || (mode == TestMode.MouseWheel && control.CanReceiveMouseWheelEvents))
                    {
                        UIComponent child = control.CheckFocus(args.Position.X, args.Position.Y, mode); // UIComponent.TestMode.Focus);

                        if (child != null)
                        {
                            if (child.Visible == false)
                            {
                               // return;
                            }

                            // Child has focus, so store the child control
                            result = child;

                            if (result is ScrollBar)
                            {

                            }

                            //  parentControl = control;
                        }
                    }
                }
            }

            // When modal, only allow modal control, its children, and null through
            // #COMBOBOX - can we make an exception for the listbox scrollbar since the listbox is 'floating'???
            if (this.modalControl != null && result != null)
            {
                if (!this.modalControl.IsChild(result))
                    return;
            }


            if (mode == TestMode.Focus)
            {
                /*
                 * don't change order of windows when user clicks!!!
                 * 
                 * TODO: should we do this with HUD windows..?
                 * 
                if (parentControl != null)
                    parentControl.BringToTop();
                */

                SetFocus(result);

                // Ensure a newly active control gets the correct mouse status
                CheckMouseStatus(args);
            }
            else
            {
                mouseWheelTarget = result;
            }
        }





        // Window mouseOverWindow;

        /// <summary>
        /// Event handler called when the mouse is moved. Asks each control to
        /// check it's mouse status (whether the mouse is over or not). Control
        /// invokes the MouseOut event, while this method invokes the MouseOver
        /// event.
        /// </summary>
        /// <param name="args">Mouse event arguments.</param>
        private void CheckMouseStatus(MouseEventArgs args)
        {
            UIComponent result = null;

            //inputEvents.MouseIsInLCDInterface = false;
            MouseIsInLCDInterface = false;
            LCDLevel = Level.RockBottom;

            // At this stage the scissor rectangle is the whole screen
            Rectangle parentScissor = new Rectangle(
                0,
                0,
                ScreenWidth, //  Game.GraphicsDevice.Viewport.Width,
                ScreenHeight // Game.GraphicsDevice.Viewport.Height
                );


            // Ask each control to check its mouse status
            foreach (KeyValuePair<Level, List<UIComponent>> levelControls in controls)
            {
                foreach (UIComponent control in levelControls.Value)
                {
                    // Early out if any control is animating (moving or resizing)
                    if (control.IsAnimating)
                        return;


                    UIComponent temp = control.CheckMouseStatus(args, parentScissor);

                    // When modal, only allow modal control, its children, and null through
                    if (temp != null && (this.modalControl == null || this.modalControl.IsChild(temp)))
                    {
                        // MouseOut last result
                        if (result != null && result.IsMouseOver)
                        {
                            result.InvokeMouseOut(args);

                        }

                        result = temp;
                    }
                }
            }

            // Window parentWindow = null;
            if (result != null)
            {
             
                // Indirectly invoke MouseOver event
                if (!result.IsMouseOver)
                    result.InvokeMouseOver(args);
            }
        }

        #endregion



        public bool IsMouseInInterface(int mouseX, int mouseY)
        {
            // check the top-level controls (windows) only:
            foreach (KeyValuePair<Level, List<UIComponent>> levelControls in controls)
            {
                foreach (UIComponent control in levelControls.Value)
                {
                    if (control is Window && ((Window)control).IsBackgroundGraphics)
                    {
                        continue;
                    }
                    else if (control.CheckCoordinates(mouseX, mouseY))
                    {
                        return true;
                    }
                }

            }

            return false;
        }



        #region Utility Functions

        public static string NumberKeyToString(KeyEventArgs args, bool allowDecimals)
        {
            switch (args.Key)
            {

                #region Numbers
                case Keys.NumPad0:
                    return "0";
                case Keys.NumPad1:
                    return "1";
                case Keys.NumPad2:
                    return "2";
                case Keys.NumPad3:
                    return "3";
                case Keys.NumPad4:
                    return "4";
                case Keys.NumPad5:
                    return "5";
                case Keys.NumPad6:
                    return "6";
                case Keys.NumPad7:
                    return "7";
                case Keys.NumPad8:
                    return "8";
                case Keys.NumPad9:
                    return "9";
                case Keys.D0:
                    return (args.Shift) ? "" : "0";
                case Keys.D1:
                    return (args.Shift) ? "" : "1";
                case Keys.D2:
                    return (args.Shift) ? "" : "2";
                case Keys.D3:
                    return (args.Shift) ? "" : "3";
                case Keys.D4:
                    return (args.Shift) ? "" : "4";
                case Keys.D5:
                    return (args.Shift) ? "" : "5";
                case Keys.D6:
                    return (args.Shift) ? "" : "6";
                case Keys.D7:
                    return (args.Shift) ? "" : "7";
                case Keys.D8:
                    return (args.Shift) ? "" : "8";
                case Keys.D9:
                    return (args.Shift) ? "" : "9";
                #endregion

                #region Extra

                case Keys.Decimal:
                    return (allowDecimals ? "." : "");
                case Keys.OemComma:
                    return (allowDecimals ? "," : "");

                #endregion

                default:
                    return "";
            }
        }

        /// <summary>
        /// Converts KeyEventArgs to a string.
        /// </summary>
        /// <param name="args">Key event arguments.</param>
        /// <returns>string version of the arguments.</returns>
        public static string KeyToString(KeyEventArgs args)
        {
            switch (args.Key)
            {
                #region Alphabet
                case Keys.A:
                    return (args.Shift) ? "A" : "a";
                case Keys.B:
                    return (args.Shift) ? "B" : "b";
                case Keys.C:
                    return (args.Shift) ? "C" : "c";
                case Keys.D:
                    return (args.Shift) ? "D" : "d";
                case Keys.E:
                    return (args.Shift) ? "E" : "e";
                case Keys.F:
                    return (args.Shift) ? "F" : "f";
                case Keys.G:
                    return (args.Shift) ? "G" : "g";
                case Keys.H:
                    return (args.Shift) ? "H" : "h";
                case Keys.I:
                    return (args.Shift) ? "I" : "i";
                case Keys.J:
                    return (args.Shift) ? "J" : "j";
                case Keys.K:
                    return (args.Shift) ? "K" : "k";
                case Keys.L:
                    return (args.Shift) ? "L" : "l";
                case Keys.M:
                    return (args.Shift) ? "M" : "m";
                case Keys.N:
                    return (args.Shift) ? "N" : "n";
                case Keys.O:
                    return (args.Shift) ? "O" : "o";
                case Keys.P:
                    return (args.Shift) ? "P" : "p";
                case Keys.Q:
                    return (args.Shift) ? "Q" : "q";
                case Keys.R:
                    return (args.Shift) ? "R" : "r";
                case Keys.S:
                    return (args.Shift) ? "S" : "s";
                case Keys.T:
                    return (args.Shift) ? "T" : "t";
                case Keys.U:
                    return (args.Shift) ? "U" : "u";
                case Keys.V:
                    return (args.Shift) ? "V" : "v";
                case Keys.W:
                    return (args.Shift) ? "W" : "w";
                case Keys.X:
                    return (args.Shift) ? "X" : "x";
                case Keys.Y:
                    return (args.Shift) ? "Y" : "y";
                case Keys.Z:
                    return (args.Shift) ? "Z" : "z";
                #endregion

                #region Numbers
                case Keys.NumPad0:
                    return "0";
                case Keys.NumPad1:
                    return "1";
                case Keys.NumPad2:
                    return "2";
                case Keys.NumPad3:
                    return "3";
                case Keys.NumPad4:
                    return "4";
                case Keys.NumPad5:
                    return "5";
                case Keys.NumPad6:
                    return "6";
                case Keys.NumPad7:
                    return "7";
                case Keys.NumPad8:
                    return "8";
                case Keys.NumPad9:
                    return "9";
                case Keys.D0:
                    return (args.Shift) ? ")" : "0";
                case Keys.D1:
                    return (args.Shift) ? "!" : "1";
                case Keys.D2:
                    return (args.Shift) ? "@" : "2";
                case Keys.D3:
                    return (args.Shift) ? "#" : "3";
                case Keys.D4:
                    return (args.Shift) ? "$" : "4";
                case Keys.D5:
                    return (args.Shift) ? "%" : "5";
                case Keys.D6:
                    return (args.Shift) ? "^" : "6";
                case Keys.D7:
                    return (args.Shift) ? "&" : "7";
                case Keys.D8:
                    return (args.Shift) ? "*" : "8";
                case Keys.D9:
                    return (args.Shift) ? "(" : "9";
                #endregion

                #region Extra
                case Keys.OemPlus:
                    return (args.Shift) ? "+" : "=";
                case Keys.OemMinus:
                    return (args.Shift) ? "_" : "-";
                case Keys.OemOpenBrackets:
                    return (args.Shift) ? "{" : "[";
                case Keys.OemCloseBrackets:
                    return (args.Shift) ? "}" : "]";
                case Keys.OemQuestion:
                    return (args.Shift) ? "?" : "/";
                case Keys.OemPeriod:
                    return (args.Shift) ? ">" : ".";
                case Keys.OemComma:
                    return (args.Shift) ? "<" : ",";
                case Keys.OemPipe:
                    return (args.Shift) ? "|" : "\\";
                case Keys.Space:
                    return " ";
                case Keys.OemSemicolon:
                    return (args.Shift) ? ":" : ";";
                case Keys.OemQuotes:
                    return (args.Shift) ? "\"" : "'";
                case Keys.OemTilde:
                    return (args.Shift) ? "~" : "`";
                #endregion

                default:
                    return "";
            }
        }
        #endregion
    }
}