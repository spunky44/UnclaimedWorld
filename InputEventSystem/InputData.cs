#region File Description
//-----------------------------------------------------------------------------
// File:      InputData.cs, original file name was InputEventSystem.cs
// Namespace: InputEventSystem
// Author:    Aaron MacDougall
// Info:      Based on article by John Sedlak.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace InputEventSystem
{
    /// <summary>
    /// encapsulates input so that it is not possible for clients to see its source - either a device or a recorded file. The client should not be able to tell the difference.
    /// The input source is set from InputManager, in Controller (ScreenManager)
    /// 
    /// contains events as well as pollable data
    /// 
    /// events are fired/invoked by InputManager. This class does not have an Update method.
    /// </summary>
    /// 

    public delegate void SetMousePositionHandler(int x,int y);

    public delegate void RequestingFocusHandler(MouseEventArgs args, TestMode mode);
    public delegate void MouseDownHandler(MouseEventArgs args);
    public delegate void MouseUpHandler(MouseEventArgs args);
    public delegate void MouseMoveHandler(MouseEventArgs args);
    public delegate void MouseWheelHandler(int wheelChange); // MouseEventArgs args);

    public delegate void KeyDownHandler(KeyEventArgs args);
    public delegate void KeyUpHandler(KeyEventArgs args);

   public enum TestMode { Focus, MouseWheel }


    public class MouseEventArgs
    {
        public MouseState State;
        public MouseButtons Button;
        public Point Position;
    }

    public class KeyEventArgs
    {
        public Keys Key;
        public bool Control = false;
        public bool Shift = false;
        public bool Alt = false;
    }

    public enum MouseButtons
    {
        None,
        Left,
        Right
    }
    public class InputData
    {
        private KeyboardState oldKeyState;
        private KeyboardState newKeyState;
        private KeyboardState KeyboardState
        {
            set
            {
                oldKeyState = newKeyState;
                newKeyState = value;
            }
        }

        /// <summary>
        /// modified by zoom factor!
        /// </summary>
        private MouseState oldMouseState;
        private MouseState newMouseState;

        private MouseState MouseState
        {
            set
            {
                oldMouseState = newMouseState;
                newMouseState = value;
                
            }
        }

        public void SetMousePosition(int x, int y)
        {
            mouseSet.Invoke(x,y);
        }

        public void Reset()
        {
            MouseDown = null;
            MouseUp = null;
            RequestingFocus = null;
            MouseMove = null;
            KeyUp = null;

            keyDown = null;
           
        }


        /// <summary>
        /// Used to track each key status.
        /// </summary>
        protected class InputKey
        {
            public Keys Key;
            public bool Pressed;
            public int Countdown;
        }


        private const int RepeatDelay = 500;
        private const int RepeatRate = 50;

        /// <summary>
        /// we use this to track key status: up, down and repeat
        /// </summary>
        private List<InputKey> keys;

        #region Events

        // don't hook up from UIComponent - go via GUIManager. otherwise, leaks will appear again.

        public event MouseDownHandler MouseDown;
        public event MouseUpHandler MouseUp;
        public event RequestingFocusHandler RequestingFocus;
        public event MouseMoveHandler MouseMove;
        public event MouseWheelHandler MouseWheelMove;
        private event SetMousePositionHandler mouseSet;
        public event KeyUpHandler KeyUp;

        private KeyDownHandler keyDown;
        public event KeyDownHandler KeyDown
        {
            add
            {
                keyDown -= value; // Lars: prevents subscribing twice!! I had to add this because the TextBox was receiving 2 key down events, writing 2 characters. I was unable to find out why it was subscribed twice...
               
                keyDown += value;
              
            }
            remove
            {
                keyDown -= value;
            }
        }

      
        #endregion

        public InputData(SetMousePositionHandler setMouseHandler)
        {
            if (setMouseHandler != null)
            {
                mouseSet += new SetMousePositionHandler(setMouseHandler);
            }
            
            this.keys = new List<InputKey>();

            // Loop through the Keys enumeration

            // --Does this work????
            foreach (string key in Enum.GetNames(typeof(Keys)))
            {
                bool found = false;

                // Search for the key in our list
                foreach (InputKey compareKey in keys)
                {
                    if (compareKey.Key == (Keys)Enum.Parse(typeof(Keys), key))
                        found = true;
                }

                // If it wasn't found, we need to add it
                if (!found)
                {
                    // Create the key instance and set the values
                    InputKey newKey = new InputKey();
                    newKey.Key = (Keys)Enum.Parse(typeof(Keys), key);
                    newKey.Pressed = false;
                    newKey.Countdown = RepeatDelay;

                    // Add the key.
                    this.keys.Add(newKey);
                }
            }
        }

        public bool RightButtonDown
        {
            get
            {
                return newMouseState.RightButton == ButtonState.Pressed;
            }
        }
        public bool LeftButtonDown
        {
            get
            {
                return newMouseState.LeftButton == ButtonState.Pressed;
            }
        }

        public int mouseX
        {
            get
            {
                return newMouseState.X;
            }
        }

        public int mouseY
        {
            get
            {
                return newMouseState.Y;
            }
        }

        public bool WasKeyDown(Keys key)
        {
            return oldKeyState.IsKeyDown(key);
        }
        public bool IsKeyDown(Keys key)
        {
            return newKeyState.IsKeyDown(key);
        }
        public bool IsKeyUp(Keys key)
        {
            return newKeyState.IsKeyUp(key);
        }

        /// <summary>
        /// is key pressed for the first time?
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyTapped(Keys key)
        {
            return !oldKeyState.IsKeyDown(key) && newKeyState.IsKeyDown(key);
        }

        public bool IsKeyReleased(Keys key)
        {
            return oldKeyState.IsKeyDown(key) && !newKeyState.IsKeyDown(key);
        }
        public Keys[] GetPressedKeys()
        {
            return newKeyState.GetPressedKeys();
        }

       


        public void UpdateNewState(KeyboardState newKeyboardState, MouseState newMouseState)
        {
            MouseState = newMouseState;
            KeyboardState = newKeyboardState;
        }

        public void UpdateKeepOldState()
        {           
             oldKeyState = newKeyState;
             oldMouseState = newMouseState;
        }


        /// <summary>
        /// invokes the mouse and keyboard events that UIComponents are subscribing to.
        /// </summary>
        /// <param name="gameTime"></param>
        public void UpdateEvents(GameTime gameTime)
        {
            #region Update Mouse

            // Check for mouse move event
            if ((newMouseState.X != oldMouseState.X) || (newMouseState.Y != oldMouseState.Y))
            {
                if (MouseMove != null)
                {
                    MouseEventArgs mouseEvent = new MouseEventArgs();
                    mouseEvent.State = newMouseState;
                    mouseEvent.Button = MouseButtons.None;

                    // Cap mouse position to the window boundaries
                    mouseEvent.Position = new Point(newMouseState.X, newMouseState.Y);

                    if (MouseMove != null)
                    {
                        MouseMove.Invoke(mouseEvent);
                    }
                }
            }

            if (newMouseState.ScrollWheelValue != oldMouseState.ScrollWheelValue)
            {
                MouseEventArgs mouseEvent = new MouseEventArgs();
                mouseEvent.State = newMouseState;
                mouseEvent.Position = new Point(newMouseState.X, newMouseState.Y);
                mouseEvent.Button = MouseButtons.None;

                // first find the control that is allowed to handle the wheel change event:
                if (RequestingFocus != null)
                    RequestingFocus.Invoke(mouseEvent, TestMode.MouseWheel);

                // now send out the event...
                int wheelChange = newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;

                if (MouseWheelMove != null)
                {
                    MouseWheelMove.Invoke(wheelChange);
                }
            }

            if (newMouseState.LeftButton != oldMouseState.LeftButton)
            {
                if ((MouseUp != null) || (MouseDown != null))
                {
                    MouseEventArgs mouseEvent = new MouseEventArgs();
                    mouseEvent.State = newMouseState;
                    mouseEvent.Position = new Point(newMouseState.X, newMouseState.Y);
                    mouseEvent.Button = MouseButtons.Left;

                    if (newMouseState.LeftButton == ButtonState.Released)
                    {
                        if (MouseUp != null)
                        {
                            MouseUp.Invoke(mouseEvent);
                        }
                    }
                    else
                    {
                        if (MouseDown != null)
                        {
                            // first find the control that should have "focus" - when the mosue event fires, the controls will test to see if they are the one that have focus...
                            // Must request focus first, to prevent mousedown
                            // event from being swallowed up
                            if (RequestingFocus != null)
                                RequestingFocus.Invoke(mouseEvent, TestMode.Focus);

                            MouseDown.Invoke(mouseEvent);
                        }
                    }
                }
            }

            if (newMouseState.RightButton != oldMouseState.RightButton)
            {
                if ((MouseUp != null) || (MouseDown != null))
                {
                    MouseEventArgs mouseEvent = new MouseEventArgs();
                    mouseEvent.State = newMouseState;
                    mouseEvent.Position = new Point(newMouseState.X, newMouseState.Y);
                    mouseEvent.Button = MouseButtons.Right;

                    if (newMouseState.RightButton == ButtonState.Released)
                    {
                        if (MouseUp != null)
                            MouseUp.Invoke(mouseEvent);
                    }
                    else
                    {
                        if (MouseDown != null)
                        {
                            // NEW: Right click can give focus:
                            // Must request focus first, to prevent mousedown
                            // event from being swallowed up
                            if (RequestingFocus != null)
                                RequestingFocus.Invoke(mouseEvent, TestMode.Focus);

                            MouseDown.Invoke(mouseEvent);
                        }
                    }
                }
            }
            #endregion

            #region Update Keyboard
            // Create the Arguments class and fill in with the modifier data
            KeyEventArgs keyEvent = new KeyEventArgs();

          

            foreach (Keys key in newKeyState.GetPressedKeys()) // Keyboard.GetState().GetPressedKeys())
            {
                if (key == Keys.LeftAlt || key == Keys.RightAlt)
                    keyEvent.Alt = true;
                else if (key == Keys.LeftShift || key == Keys.RightShift)
                    keyEvent.Shift = true;
                else if (key == Keys.LeftControl || key == Keys.RightControl)
                    keyEvent.Control = true;
            }

            // Loop through our keys
            foreach (InputKey key in this.keys)
            {
                // If they are any of the modifier keys, skip them
                if (key.Key == Keys.LeftAlt || key.Key == Keys.RightAlt ||
                    key.Key == Keys.LeftShift || key.Key == Keys.RightShift ||
                    key.Key == Keys.LeftControl || key.Key == Keys.RightControl
                    )
                    continue;

                // Check if the key was pressed
              //  bool pressed = Keyboard.GetState().IsKeyDown(key.Key);
                bool pressed = newKeyState.IsKeyDown(key.Key);

                // If it was, decrement the countdown for that key
                if (pressed)
                    key.Countdown -= gameTime.ElapsedGameTime.Milliseconds;

                if ((pressed) && (!key.Pressed)) // If it is pressed, but wasn't before...
                {
                    // Set some flags and invoke the KeyDown event
                    key.Pressed = true;
                    keyEvent.Key = key.Key;

                    if (keyDown != null)
                        keyDown.Invoke(keyEvent);
                }
                else if ((!pressed) && (key.Pressed)) // If it isn't pressed, but was before...
                {
                    // Set some flags, reset the countdown
                    key.Pressed = false;
                    key.Countdown = RepeatDelay;
                    keyEvent.Key = key.Key;

                    // Invoke the Key Up event
                    if (KeyUp != null)
                        KeyUp.Invoke(keyEvent);
                }

                // If the Key's Countdown has zeroed out, reset it, and make
                // sure that KeyDown fires again
                if (key.Countdown < 0)
                {
                    keyEvent.Key = key.Key;

                    if (keyDown != null)
                        keyDown.Invoke(keyEvent);

                    key.Countdown = RepeatRate;
                }
            }
            #endregion
        }
    }
}
/*
 
 */
//namespace InputEventSystem
//{
    //#region Delegates
    //public delegate void KeyDownHandler(KeyEventArgs args);
    //public delegate void KeyUpHandler(KeyEventArgs args);
    //public delegate void MouseDownHandler(MouseEventArgs args);
    //public delegate void MouseUpHandler(MouseEventArgs args);
    //public delegate void MouseMoveHandler(MouseEventArgs args);
    //#endregion

    ///// <summary>
    ///// An enumeration of the mouse buttons the system handles.
    ///// </summary>
    //public enum MouseButtons
    //{
    //    None,
    //    Left,
    //    Right
    //}

    ///// <summary>
    ///// TODO: delete this. Game modules should no longer access input other than via InputData.
    ///// InputData can have event stuff???
    ///// 
    ///// Interface for other classes to access for input events.
    ///// </summary>
    //public interface IInputEventsService
    //{
    //    #region Events
    //    event KeyDownHandler KeyDown;
    //    event KeyUpHandler KeyUp;
    //    event MouseDownHandler MouseDown;
    //    event MouseUpHandler MouseUp;
    //    event MouseDownHandler RequestingFocus;
    //    event MouseMoveHandler MouseMove;
    //    #endregion

    //    /// <summary>
    //    /// Retrieves the current mouse x-position.
    //    /// </summary>
    //    /// <returns>Current mouse x-position.</returns>
    //    int GetMouseX();

    //    /// <summary>
    //    /// Retrieves the current mouse y-position.
    //    /// </summary>
    //    /// <returns>Current mouse y-position.</returns>
    //    int GetMouseY();

    //   /* bool MouseIsInInterface { get; set; }
    //    bool MouseIsInLCDInterface { get; set; }*/
    //}

    //#region Event Argument Classes
    //public class KeyEventArgs
    //{
    //    public Keys Key;
    //    public bool Control = false;
    //    public bool Shift = false;
    //    public bool Alt = false;
    //}

    //public class MouseEventArgs
    //{
    //   // public MouseStateWrapper State;
    //    public MouseButtons Button;
    //    public Point Position;
    //}
    //#endregion

    ///// <summary>
    ///// TODO: delete this whole class. distribute code to either InputData or InputManager as appropriate.
    ///// 
    ///// Checks for input each update, and fires events for other classes to use
    ///// for triggering events.
    ///// </summary>
    //public partial class InputEvents : GameComponent, IInputEventsService
    //{
    //    /// <summary>
    //    /// Used to track each key status.
    //    /// </summary>
    //    protected class InputKey
    //    {
    //        public Keys Key;
    //        public bool Pressed;
    //        public int Countdown;
    //    }

    //    #region Fields
    //    private const int RepeatDelay = 500;
    //    private const int RepeatRate = 50;
    //    private List<InputKey> keys;
        
    //    private bool clickWasCaptured;
    //    private bool mouseIsInLCDInterface;
    //    #endregion

    //    #region Events
    //    public event KeyDownHandler KeyDown;
    //    public event KeyUpHandler KeyUp;
    //    public event MouseDownHandler MouseDown;
    //    public event MouseUpHandler MouseUp;
    //    public event MouseDownHandler RequestingFocus;
    //    public event MouseMoveHandler MouseMove;
    //    #endregion

    //    #region Constructors
    //    /// <summary>
    //    /// Constructor.
    //    /// </summary>
    //    /// <param name="game">The currently running Game object.</param>
    //    public InputEvents(Game game , bool addService = true)
    //        : base(game)
    //    {
    //        this.keys = new List<InputKey>();
            
    //        if (addService)
    //        {
    //            AddAsService();
    //        }
    //    }
    //    #endregion

    //    /// <summary>
    //    /// TODO: delete this - only keep a reference to InputData
    //    /// </summary>

    //    public void AddAsService()
    //    {
    //        base.Game.Services.AddService(typeof(IInputEventsService), this);          
    //    }
     
    //    public int GetMouseX()
    //    {
    //        return mouseStateWrapper.X;
    //    }

    //    public int GetMouseY()
    //    {
    //        return mouseStateWrapper.Y;
    //    }

    //    /// <summary>
    //    /// TODO: do we need this???
    //    /// Sets up key tracking data.
    //    /// </summary>
    //    public override void Initialize()
    //    {
    //        // Loop through the Keys enumeration
    //        foreach (string key in Enum.GetNames(typeof(Keys)))
    //        {
    //            bool found = false;

    //            // Search for the key in our list
    //            foreach (InputKey compareKey in keys)
    //            {
    //                if (compareKey.Key == (Keys)Enum.Parse(typeof(Keys), key))
    //                    found = true;
    //            }

    //            // If it wasn't found, we need to add it
    //            if (!found)
    //            {
    //                // Create the key instance and set the values
    //                InputKey newKey = new InputKey();
    //                newKey.Key = (Keys)Enum.Parse(typeof(Keys), key);
    //                newKey.Pressed = false;
    //                newKey.Countdown = RepeatDelay;

    //                // Add the key.
    //                this.keys.Add(newKey);
    //            }
    //        }

    //        base.Initialize();
    //    }

    //    /// <summary>
    //    /// TODO: move this to InputManager and call explicitly from Controller. (don't inherit GameComponent or Service)
    //    /// 
    //    /// Goes through each key checking it's state compared to the last
    //    /// frame, triggering events if necessary. Updates the current mouse
    //    /// state and triggers events if necessary.
    //    /// </summary>
    //    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    //    public override void Update(GameTime gameTime)
    //    {
    //       /*
    //        #region Update Keyboard
    //        // Create the Arguments class and fill in with the modifier data
    //        KeyEventArgs keyEvent = new KeyEventArgs();

    //        foreach (Keys key in Keyboard.GetState().GetPressedKeys())
    //        {
    //            if (key == Keys.LeftAlt || key == Keys.RightAlt)
    //                keyEvent.Alt = true;
    //            else if (key == Keys.LeftShift || key == Keys.RightShift)
    //                keyEvent.Shift = true;
    //            else if (key == Keys.LeftControl || key == Keys.RightControl)
    //                keyEvent.Control = true;
    //        }

    //        // Loop through our keys
    //        foreach (InputKey key in this.keys)
    //        {
    //            // If they are any of the modifier keys, skip them
    //            if (key.Key == Keys.LeftAlt || key.Key == Keys.RightAlt ||
    //                key.Key == Keys.LeftShift || key.Key == Keys.RightShift ||
    //                key.Key == Keys.LeftControl || key.Key == Keys.RightControl
    //                )
    //                continue;

    //            // Check if the key was pressed
    //            bool pressed = Keyboard.GetState().IsKeyDown(key.Key);

    //            // If it was, decrement the countdown for that key
    //            if (pressed)
    //                key.Countdown -= gameTime.ElapsedGameTime.Milliseconds;

    //            if ((pressed) && (!key.Pressed)) // If it is pressed, but wasn't before...
    //            {
    //                // Set some flags and invoke the KeyDown event
    //                key.Pressed = true;
    //                keyEvent.Key = key.Key;

    //                if (KeyDown != null)
    //                    KeyDown.Invoke(keyEvent);
    //            }
    //            else if ((!pressed) && (key.Pressed)) // If it isn't pressed, but was before...
    //            {
    //                // Set some flags, reset the countdown
    //                key.Pressed = false;
    //                key.Countdown = RepeatDelay;
    //                keyEvent.Key = key.Key;

    //                // Invoke the Key Up event
    //                if (KeyUp != null)
    //                    KeyUp.Invoke(keyEvent);
    //            }

    //            // If the Key's Countdown has zeroed out, reset it, and make
    //            // sure that KeyDown fires again
    //            if (key.Countdown < 0)
    //            {
    //                keyEvent.Key = key.Key;

    //                if (KeyDown != null)
    //                    KeyDown.Invoke(keyEvent);

    //                key.Countdown = RepeatRate;
    //            }
    //        }
    //        #endregion
    //        */
    //        #region Update Mouse

    //        MouseIsInInterface = false;

    //        // Check for mouse move event
    //        if ((mouseStateWrapper.X != mouseStateWrapper.previousMouseState.X) || (mouseStateWrapper.Y != mouseStateWrapper.previousMouseState.Y))
    //        {
    //            if (MouseMove != null)
    //            {
    //                MouseEventArgs mouseEvent = new MouseEventArgs();
    //                mouseEvent.State = mouseStateWrapper;
    //                mouseEvent.Button = MouseButtons.None;

    //                // Cap mouse position to the window boundaries
    //                mouseEvent.Position = new Point(mouseStateWrapper.X, mouseStateWrapper.Y);
    //                if (mouseEvent.Position.X < 0)
    //                    mouseEvent.Position.X = 0;
    //                if (mouseEvent.Position.Y < 0)
    //                    mouseEvent.Position.Y = 0;

    //                Rectangle bounds = this.Game.Window.ClientBounds;

    //                if (mouseEvent.Position.X > bounds.Width)
    //                    mouseEvent.Position.X = bounds.Width;
    //                if (mouseEvent.Position.Y > bounds.Height)
    //                    mouseEvent.Position.Y = bounds.Height;


    //                MouseMove.Invoke(mouseEvent);
    //            }
    //        }

    //        if (mouseStateWrapper.LeftButtonDown != mouseStateWrapper.PreviousLeftButtonDown)
    //        {
    //            if ((MouseUp != null) || (MouseDown != null))
    //            {
    //                MouseEventArgs mouseEvent = new MouseEventArgs();
    //                mouseEvent.State = mouseStateWrapper;
    //                mouseEvent.Position = new Point(mouseStateWrapper.X, mouseStateWrapper.Y);
    //                mouseEvent.Button = MouseButtons.Left;

    //                if (mouseStateWrapper.LeftButtonDown == false)
    //                {
    //                    if (MouseUp != null)
    //                        MouseUp.Invoke(mouseEvent);
    //                }
    //                else
    //                {
    //                    if (MouseDown != null)
    //                    {
    //                        // Must request focus first, to prevent mousedown
    //                        // event from being swallowed up
    //                        if (RequestingFocus != null)
    //                            RequestingFocus.Invoke(mouseEvent);

    //                        MouseDown.Invoke(mouseEvent);
    //                    }
    //                }
    //            }
    //        }

    //        if (mouseStateWrapper.RightButtonDown != mouseStateWrapper.PreviousRightButtonDown)
    //        {
    //            if ((MouseUp != null) || (MouseDown != null))
    //            {
    //                MouseEventArgs mouseEvent = new MouseEventArgs();
    //                mouseEvent.State = mouseStateWrapper;
    //                mouseEvent.Position = new Point(mouseStateWrapper.X, mouseStateWrapper.Y);
    //                mouseEvent.Button = MouseButtons.Right;

    //                if (mouseStateWrapper.RightButtonDown == false)
    //                {
    //                    if (MouseUp != null)
    //                        MouseUp.Invoke(mouseEvent);
    //                }
    //                else
    //                {
    //                    if (MouseDown != null)
    //                    {
    //                        // NEW: Right click can give focus:
    //                        // Must request focus first, to prevent mousedown
    //                        // event from being swallowed up
    //                        if (RequestingFocus != null)
    //                            RequestingFocus.Invoke(mouseEvent);

    //                        MouseDown.Invoke(mouseEvent);
    //                    }
    //                }
    //            }
    //        }

    //        // Update mouse state
    //        #endregion

    //        base.Update(gameTime);
    //    }

    //    #region IInputEventsService Implementation
    //    /// <summary>
    //    /// Retrieves the current mouse x-position.
    //    /// </summary>
    //    /// <returns>Current mouse x-position.</returns>
     

    //    /// <summary>
    //    /// Retrieves the current mouse y-position.
    //    /// </summary>
    //    /// <returns>Current mouse y-position.</returns>

    //    public bool MouseIsInInterface
    //    {
    //        get { return clickWasCaptured; }
    //        set { clickWasCaptured = value; }
    //    }

    // /*   public bool MouseIsInLCDInterface
    //    {
    //        get { return mouseIsInLCDInterface; }
    //        set { mouseIsInLCDInterface = value; }
    //    }*/

    //    #endregion
    //}
//}