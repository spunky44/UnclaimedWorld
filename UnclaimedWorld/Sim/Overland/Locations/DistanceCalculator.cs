using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Overland.Locations
{
    /// <summary>
    ///  /// Source: http://beavergeodesy.codeplex.com/SourceControl/latest#CrazyBeavers.Geodesy/DistanceCalculator.cs      
    /// </summary>
    public static class DistanceCalculator
    {
       
        /// <summary>
        /// 
        /// Calculates the distance between two geodetic coordinates using the Haversine formula.
        /// </summary>
        /// <param name="coordinate1">The first coordinate.</param>
        /// <param name="coordinate2">The second coordinate.</param>
        /// <returns>The distance between the coordinates in kilometers.</returns>
        public static double Haversine(GeodeticCoordinate coordinate1, GeodeticCoordinate coordinate2, double worldRadius)
        {
            double latDelta = (coordinate1.Latitude - coordinate2.Latitude) * (Math.PI / 180d);
            double lonDelta = (coordinate1.Longitude - coordinate2.Longitude) * (Math.PI / 180d);

            double a = Math.Sin(latDelta / 2) * Math.Sin(latDelta / 2) +
                       Math.Cos(coordinate1.Latitude * (Math.PI / 180d)) * Math.Cos(coordinate2.Latitude * (Math.PI / 180d)) *
                       Math.Sin(lonDelta / 2) * Math.Sin(lonDelta / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return worldRadius * c;
        }

        /// <summary>
        /// Calculates the distance between two geodetic coordinates using the Spherical law of cosines.
        /// </summary>
        /// <param name="coordinate1">The first coordinate.</param>
        /// <param name="coordinate2">The second coordinate.</param>
        /// <returns>The distance between the coordinates in kilometers.</returns>
        public static double Spherical(GeodeticCoordinate coordinate1, GeodeticCoordinate coordinate2, double worldRadius)
        {
            double d =
                Math.Acos(Math.Sin(coordinate1.Latitude * (Math.PI / 180d)) * Math.Sin(coordinate2.Latitude * (Math.PI / 180d)) +
                          Math.Cos(coordinate1.Latitude * (Math.PI / 180d)) * Math.Cos(coordinate2.Latitude * (Math.PI / 180d)) *
                          Math.Cos(coordinate2.Longitude * (Math.PI / 180d) - coordinate1.Longitude * (Math.PI / 180d)));
            return d * worldRadius;
        }

        /// <summary>
        /// Calculates a new coordinate from a bearing and distance from a specified coordinate.
        /// </summary>
        /// <param name="start">The initial coordinate.</param>
        /// <param name="bearing">The bearing from the initial coordinate in radians.</param>
        /// <param name="distance">The distance from the initial coordinate in kilometers.</param>
        /// <returns>A new geodetic coordinate representing the new point.</returns>
        public static GeodeticCoordinate CoordFromDistance(GeodeticCoordinate start, double bearing, double distance, double worldRadius)
        {
            distance = distance / worldRadius;
           // bearing *= Math.PI / 180d;

            double latStart = start.Latitude.ToRadians();
            double lonStart = start.Longitude.ToRadians();

            var latEnd = Math.Asin(Math.Sin(latStart) * Math.Cos(distance) +
                                  Math.Cos(latStart) * Math.Sin(distance) * Math.Cos(bearing));
            var lonEnd = lonStart + Math.Atan2(Math.Sin(bearing) * Math.Sin(distance) * Math.Cos(latStart),
                                         Math.Cos(distance) - Math.Sin(latStart) * Math.Sin(latEnd));
            lonEnd = (lonEnd + 3 * Math.PI) % (2 * Math.PI) - Math.PI;  // normalise to -180...+180

            return new GeodeticCoordinate(lonEnd.ToDegrees(), latEnd.ToDegrees()); // { Latitude = latEnd.ToDegrees(), Longitude = lonEnd.ToDegrees() };
        }

        /*
        public static GeodeticCoordinate CoordFromDistance(GeodeticCoordinate start, double bearing, double distance, double worldRadius)
        {
            distance = distance / worldRadius;
            bearing *= Math.PI / 180d;

            double latStart = start.Latitude.ToRadians();
            double lonStart = start.Longitude.ToRadians();

            var latEnd = Math.Asin(Math.Sin(latStart) * Math.Cos(distance) +
                                  Math.Cos(latStart) * Math.Sin(distance) * Math.Cos(bearing));
            var lonEnd = lonStart + Math.Atan2(Math.Sin(bearing) * Math.Sin(distance) * Math.Cos(latStart),
                                         Math.Cos(distance) - Math.Sin(latStart) * Math.Sin(latEnd));
            lonEnd = (lonEnd + 3 * Math.PI) % (2 * Math.PI) - Math.PI;  // normalise to -180...+180

            return new GeodeticCoordinate() { Latitude = latEnd.ToDegrees(), Longitude = lonEnd.ToDegrees() };
        }*/





        public static GeodeticCoordinate GetIntermediatePoint(GeodeticCoordinate start, GeodeticCoordinate end, double fraction, double distance, double worldRadius)
        {
            /*
            Intermediate points on a great circle

           In previous sections we have found intermediate points on a great circle given either the crossing latitude or longitude. 
            * Here we find points (lat,lon) a given fraction of the distance (d) between them. Suppose the starting point is (lat1,lon1) 
            * and the final point (lat2,lon2) and we want the point a fraction f along the great circle route. f=0 is point 1. f=1 is point 2. 
            * The two points cannot be antipodal ( i.e. lat1+lat2=0 and abs(lon1-lon2)=pi) because then the route is undefined. 
            * The intermediate latitude and longitude is then given by:

                   A=sin((1-f)*d)/sin(d)
                   B=sin(f*d)/sin(d)
                   x = A*cos(lat1)*cos(lon1) +  B*cos(lat2)*cos(lon2)
                   y = A*cos(lat1)*sin(lon1) +  B*cos(lat2)*sin(lon2)
                   z = A*sin(lat1)           +  B*sin(lat2)
                   lat=atan2(z,sqrt(x^2+y^2))
                   lon=atan2(y,x)
         
            */

            distance = distance / worldRadius; //??? seems like a radius is needed



            double lat1 = MathHelper.ToRadians((float)start.Latitude); // start.Latitude;
            double lon1 = MathHelper.ToRadians((float)start.Longitude);
            double lat2 = MathHelper.ToRadians((float)end.Latitude);
            double lon2 = MathHelper.ToRadians((float)end.Longitude);

            double lat, lon;

            double A = Math.Sin((1d - fraction) * distance) / Math.Sin(distance);
            double B = Math.Sin(fraction * distance) / Math.Sin(distance);

            double cosLat1 = Math.Cos(lat1);
            double cosLat2 = Math.Cos(lat2);

            double x = A * cosLat1 * Math.Cos(lon1) + B * cosLat2 * Math.Cos(lon2);
            double y = A * cosLat1 * Math.Sin(lon1) + B * cosLat2 * Math.Sin(lon2);
            double z = A * Math.Sin(lat1) + B * Math.Sin(lat2);
            lat = Math.Atan2(z, Math.Sqrt(Math.Pow(x, 2d) + Math.Pow(y, 2d)));
            lon = Math.Atan2(y, x);

            lat = MathHelper.ToDegrees((float)lat);
            lon = MathHelper.ToDegrees((float)lon);

            return new GeodeticCoordinate(lon, lat);

        }


        /// <summary>
        /// Does not seem to be working. We will need it if we want to rotate the mission marker...
        /// 
        /// result is in Radians!
        /// 0: North
        /// Pi: South
        /// Pi/2: West
        /// 3Pi/2: East
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static double GetBearing(GeodeticCoordinate? start, GeodeticCoordinate? end)
        {
            if (start == null || end == null)
            {
                return 0.0f;
            }

            double lat1 = start.Value.Latitude;
            double lon1 = start.Value.Longitude * -1;
            double lat2 = end.Value.Latitude;
            double lon2 = end.Value.Longitude * -1;
            double y = Math.Atan2(Math.Sin(lon1 - lon2) * Math.Cos(lat2), Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2));
            const double x = 2 * Math.PI;
            double result = y - x * Math.Floor(y / x);

            double bearing = result;

            return bearing;

            /*
            double lat1 = DegreesToRadians(start.Latitude);
            double lon1 = DegreesToRadians(start.Longitude) * -1;
            double lat2 = DegreesToRadians(end.Latitude);
            double lon2 = DegreesToRadians(end.Longitude) * -1;
            double y = Math.Atan2(Math.Sin(lon1 - lon2) * Math.Cos(lat2), Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon1 - lon2));
            const double x = 2 * Math.PI;
            double result = y - x * Math.Floor(y / x);
            
            double bearing = RadiansToDegrees(result);

            return bearing;*/
        }
    }
}
