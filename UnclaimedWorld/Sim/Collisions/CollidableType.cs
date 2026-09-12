using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Collisions
{
    public class CollidableType
    {
        
        public float Forgiveness = 1f;

        /// <summary>
        /// either fill in the radius (to create a simple circle) or the list of shapes
        /// </summary>
        public float? CircleRadius;

        /// <summary>
        /// is overridden by GeometryLayout!
        /// </summary>
        public CollideShape2D[] Shapes;

    }
}
