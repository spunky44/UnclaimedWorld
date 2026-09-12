#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace Control
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        #region Fields

        public KeyboardState CurrentKeyboardState;
        public GamePadState CurrentGamePadState;

        public KeyboardState LastKeyboardState;
        public GamePadState LastGamePadState;

        #endregion

        #region Properties




        #endregion

        #region Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            LastKeyboardState = CurrentKeyboardState;
            LastGamePadState = CurrentGamePadState;

            CurrentKeyboardState = Keyboard.GetState();
            CurrentGamePadState = GamePad.GetState(PlayerIndex.One);

        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update.
        /// </summary>
        bool IsNewKeyPress(Keys key)
        {
            return (CurrentKeyboardState.IsKeyDown(key) &&
                    LastKeyboardState.IsKeyUp(key));
        }


        #endregion
    }
}
