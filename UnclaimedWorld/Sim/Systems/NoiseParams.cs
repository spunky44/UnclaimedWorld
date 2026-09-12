using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWGame.SimSide.Systems
{
    /// <summary>
    /// data parameters for simplex noise
    /// </summary>
    public class NoiseParams
    {
        /// <summary>
        /// changes the scale of the noise map, how big the continuous areas are
        /// </summary>
        public float? NoiseFrequency;

        /// <summary>
        /// the noise is in range 0 to 2
        /// changes the amplitude of the noise map values
        /// </summary>
        public float? NoiseAmplitude;

        /// <summary>
        /// fill in this to shift the noise value up or down
        /// </summary>
        public float? NoiseAddend;


    }

}
