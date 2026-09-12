using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Buildings
{
    /// <summary>
    /// an area on the ground or in a building designated for a specific activity.
    /// </summary>
    public class ActivityArea
    {
        //either:
        public Entity Building;
        //or:
        public List<Point> MapPosition;

    }
}
