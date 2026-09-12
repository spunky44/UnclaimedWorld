using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.ClientSide.Map;

namespace UWGame.ClientSide.Particles
{
    public class FireAndSmoke
    {
        public FirePlume FirePlume;
        public ParticleEmitter SmokePlume;
        public LightSource LightSource;


        public Entity AttachedToEntity;

        
        public float Size
        {
            set
            {
                FirePlume.Scale = value;
                FirePlume.Intensity = value;

                SmokePlume.EmitterScale = value;
                                
            }
        }


        /// <summary>
        /// 0 - 1
        /// </summary>
        public float SmokeAmount
        {
            set
            {
                SmokePlume.Intensity = value;
            }
        }

       

        public void Remove()
        {
          //  The.Client.ParticleManager.RemoveFire(AttachedToEntity);

        }
    }
}
