using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace UWGame.ClientSide.Renderables
{
    /// <summary>
    /// handles temporary changes in alpha, pulsing frequency etc.
    /// 
    /// has a basic constant value over the duration. perhaps add other classes that can do curves or linear functions...
    /// </summary>
    public class Envelope<T>
    {
        //TODO: TODO See why this class had RevertToPermanentValue and Function? Do we want to keep created envelopes in effect.
        //private Tuple<float, T>[] Function;

       // private bool RevertToPermanentValue;

        private float duration = 0f;
        private T value;

        public Envelope(float duration, T value)
        {
            // TODO: Complete member initialization
            this.duration = duration;
            this.value = value;
        }

        
        public bool Update(GameTime gameTime)
        {
            
            duration -= gameTime.ElapsedGameTime.Milliseconds;
            if (duration < 0)
            {
                return false;
            }
            return true;
        }


        public T GetValue()
        {
            //T returnValue = Function[0].Item2;
            return value;
        }

    }
}

 