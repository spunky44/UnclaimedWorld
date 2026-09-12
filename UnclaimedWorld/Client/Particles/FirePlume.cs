using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
namespace UWGame.ClientSide.Particles
{
    public class FirePlume
    {
        public Vector2 Position;

        /// <summary>
        /// "Spread"?
        /// </summary>
        public float Intensity;

        public float Scale = 1f;

        // keep a timer that will tell us when it's time to add more particles to the
        // smoke plume.
        public const float TimeBetweenFlames = 2f; //.5f;
        public float timeTillPuff = 0.0f;

        public void Initialize()
        {
            timeTillPuff = 0.0f;

        }
    }
}
