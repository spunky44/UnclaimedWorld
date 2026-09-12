using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Entities.Body
{
    public class BodyPartFunction
    {
        public enum FunctionType { Locomotion, Vision, Appearance, Manipulation, Agility, Strength, UserComfort, Structure }

        public FunctionType Function;

        /// <summary>
        /// 0 - 1
        /// </summary>
        public float Weight;
    }
}
