using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities
{
    public class PalatableFood
    {
        public string FoodTag;

        /// <summary>
        /// 0 - abhorrent, 0.5(default value): passable, 1: delicious 
        /// </summary>
        public float Palatability = 0.5f;
    }
}
