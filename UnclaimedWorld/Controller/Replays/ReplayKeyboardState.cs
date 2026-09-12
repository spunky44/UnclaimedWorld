using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
namespace UWGame.Control.Replays
{
    public class ReplayKeyboardState
    {
        public Dictionary<Keys,bool> KeyStates;
        public ReplayKeyboardState()
        {
            KeyStates = new Dictionary<Keys,bool>();
        }

        public bool IsKeyDown(Keys keyToCheck)
        {
            bool keyIsDown;
            if (KeyStates.TryGetValue(keyToCheck, out keyIsDown) == false)
            {
                return false;
                //throw new Exception("Replay Manager does not support all of the required keys!");     
            }
            return keyIsDown;
        }
    }
}
