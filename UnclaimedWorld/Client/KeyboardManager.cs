using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace UWGame.ClientSide
{
    public class KeyboardManager  
    {
        KeyboardState oldKeyState;
        KeyboardState newKeyState;

        public KeyboardState KeyboardState
        {
            get { return newKeyState; }
        }

        public void Update()
        {
            oldKeyState = newKeyState;
            newKeyState = Keyboard.GetState();

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

        /// <summary>
        /// is key currently down
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyDown(Keys key)
        {
            return newKeyState.IsKeyDown(key);
        }


    }
}
