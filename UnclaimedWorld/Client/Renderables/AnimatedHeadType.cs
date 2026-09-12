using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.ClientSide.Renderables
{
    public class AnimatedHeadType
    {
        /// <summary>
        /// must be in the correct order starting from the bottom of the spine column and ending with the head for it to work.
        /// </summary>
        public string[] SpineBones;

        public float TurnToLookLerpFactor = 0.06f;

        public float PitchForward = 0.12f;

    }
}
