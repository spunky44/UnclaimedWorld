using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Overland.Locations.Projections
{
    public abstract class Projection : IEquatable<Projection>
    {
        public double CentralMeridianLon { get; protected set; }
        public double CentralMeridianScale { get; protected set; }
        public double LongitudeDelta { get; protected set; }
        public double FalseNorthing { get; protected set; }
        public double FalseEasting { get; protected set; }
        public Ellipsoid Ellipsoid { get; protected set; }

        public bool Equals(Projection other)
        {
            bool cmlEquals = CentralMeridianLon == other.CentralMeridianLon;
            bool cmsEquals = CentralMeridianScale == other.CentralMeridianScale;
            bool ldEquals = LongitudeDelta == other.LongitudeDelta;
            bool fnEquals = FalseNorthing == other.FalseNorthing;
            bool felEquals = FalseEasting == other.FalseEasting;

            return cmlEquals & cmsEquals & ldEquals & fnEquals & felEquals & Ellipsoid.Equals(other.Ellipsoid);
        }
    }
}
