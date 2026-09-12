using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.AllGameData.Constants
{
    public class PhysicalWork
    {
      
        public float RangedFighting = 2f;
        public float MeleeFighting = 4f;

        public float Sleeping = 0.5f; // try to prevent starving while sleeping // 1f;

        /// <summary>
        /// can be overridden by stances
        /// </summary>
        public float IdleExertionDefault = 1.3f;

        // moved to StanceType
       /* public float Lying = 1.2f;     
        public float Sitting = 1.2f;
        public float Standing = 1.4f;*/

        public float Driving = 1.4f;

        public float Walking = 3.2f;
        public float Running = 5f;

        public float HaulingLightLoad = 3.5f;
        public float HaulingMediumLoad = 4f;       
        public float HaulingHeavyLoad = 4.5f;

        /// <summary>
        /// used when process type data has not been filled in
        /// </summary>
        public float DefaultWork = 2f;

        public float MaxPhysicalWorkCost = 5.0f;
    }
}
