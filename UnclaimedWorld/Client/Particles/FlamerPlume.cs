using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
namespace UWGame.ClientSide.Particles
{
    public class FlamerPlume
    {
        public Vector2 Position;

        public float Intensity;

        // keep a timer that will tell us when it's time to add more particles to the
        // flame plume.
       
        public const float TimeBetweenFlamerPuffs = .05f;
        public float timeTillFlamerPuff = 0.0f;
    }
}
