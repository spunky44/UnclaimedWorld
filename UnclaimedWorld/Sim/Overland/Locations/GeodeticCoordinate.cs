using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Locations
{
    /// <summary>
    /// Represents a geodetic coordinate
    /// 
    /// With 6371 kn as the value for R the meridian length of 1 degree of latitude on the sphere is 111.2 km
    /// 
    /// </summary>
    public struct GeodeticCoordinate //: IEquatable<GeodeticCoordinate>, ISnapshot
    {
        private readonly double latitude;
        private readonly double longitude;

        public double Latitude
        {
            get
            {
                return latitude;
            }
        }

        public double Longitude
        {
            get
            {
                return longitude;
            }
        }

        private const Int32 EqualityDecimals = 4;

       
        public GeodeticCoordinate(double longitude, double latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;

        }

        /// <summary>
        ///  9.80°N (latitude), 71.56°W (longitude)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0:N2} (latitude), {1:N2} (longitude)", latitude, longitude);
            
        }


        /* // needed?
        public override bool Equals(GeodeticCoordinate other)
        {
            bool latitudeEquals = Math.Round(Latitude, EqualityDecimals) == Math.Round(other.Latitude, EqualityDecimals);
            bool longitudeEquals = Math.Round(Longitude, EqualityDecimals) == Math.Round(other.Longitude, EqualityDecimals);

            return latitudeEquals && longitudeEquals;
        }*/



    }


    /*
    public class GeodeticCoordinate : IEquatable<GeodeticCoordinate> , ISnapshot
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        private const Int32 EqualityDecimals = 4;

        public bool Equals(GeodeticCoordinate other)
        {
            bool latitudeEquals = Math.Round(Latitude, EqualityDecimals) == Math.Round(other.Latitude, EqualityDecimals);
            bool longitudeEquals = Math.Round(Longitude, EqualityDecimals) == Math.Round(other.Longitude, EqualityDecimals);

            return latitudeEquals & longitudeEquals;
        }

        public GeodeticCoordinate()
        {

        }

        public GeodeticCoordinate(double longitude, double latitude)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;

        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Longitude = sn.DoDouble(Longitude);
            this.Latitude = sn.DoDouble(Latitude);
           

            return this;
        }



        public void LoadPostProcess(Snapshotter sn)
        {
          

        }


        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        #endregion
    }*/
}
