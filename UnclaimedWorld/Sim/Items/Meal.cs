using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
namespace UWGame.SimSide.Items
{
    public class Meal
    {
        /// <summary>
        /// value from 0-1
        /// </summary>
        public float Sustenance;
        /// <summary>
        /// value from 0-1
        /// </summary>
        public float Variety;

        public string NameOfDish;
        //public string FlavourText;

        public Meal(float sustenance, float variety)
        {
            this.Sustenance = sustenance;
            this.Variety = variety;
            
        }

    
    }
}
