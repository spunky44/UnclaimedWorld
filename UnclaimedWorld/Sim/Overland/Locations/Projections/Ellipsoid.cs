using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Overland.Locations.Projections
{
    public abstract class Ellipsoid : IEquatable<Ellipsoid>
    {
        public double SemiMajorAxis { get; protected set; }
        public double Flattening { get; protected set; }

        public bool Equals(Ellipsoid other)
        {
            bool smaEquals = SemiMajorAxis == other.SemiMajorAxis;
            bool fEquals = Flattening == other.Flattening;

            return smaEquals & fEquals;
        }
    }
}
