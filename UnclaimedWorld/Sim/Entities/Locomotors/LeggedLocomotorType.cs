using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Locomotors
{
    public class LeggedLocomotorType
    {
       

        /// <summary>
        /// 0 - 1
        /// 0: terrain has full negative move effect
        /// 1: terrain has no effect
        /// </summary>
        public float TerrainNegateFactor = 0f;

        /// <summary>     
        /// NEW: in pixels per second
        /// </summary>
        public float WalkNormalSpeed;
        public float WalkSlowSpeed;
        public float WalkFastSpeed;       
        public float RunSpeed;
        public float HaulSpeed; 

    }
}
