using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;

namespace UWGame.SimSide.Communication
{
    /// <summary>
    /// defines the ability to do long-range (off site) communication
    /// </summary>
    public interface ICommunicates
    {
        //bool IsInCommunicationRange(ICommunicates otherParty, out CommunicationMethod? workingMethod);

        Site Site { get; }

        GeodeticCoordinate? Coords { get; }

        bool CanCommunicate(CommunicationMethod method, double distance);
    }



    public class Communicates
    {
        public static bool IsInCommunicationRange(ICommunicates from, ICommunicates to, out CommunicationMethod? workingMethod, Site toSite, GeodeticCoordinate? toCoords)
        {
            workingMethod = null;


            // two allegiances on the same site can always communicate:
            if (from.Site != null && from.Site == toSite)
            {
                workingMethod = CommunicationMethod.Direct;
                return true;
            }

            if (from.Coords == null || from.Coords == null)
                return false; // something is wrong,,


            // get the surface distance:
            double distance = The.Sim.World.GetAirDistance(toCoords.Value, from.Coords.Value);

            if (distance < GameData.Instance.Constants.VisualCommunicationRangeInKms)
            {
                workingMethod = CommunicationMethod.Visual; // NEW: default comm method
                return true;
            }


            workingMethod = null;

            CommunicationMethod method;
            Array methods = Enum.GetValues(typeof(CommunicationMethod));
            foreach (var item in methods)
            {
                // test each method...
                method = (CommunicationMethod)item;
                if (from.CanCommunicate(method, distance))
                {
                    // test both ends:
                    if (to.CanCommunicate(method, distance))
                    {
                        workingMethod = method;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsInCommunicationRange(ICommunicates from, ICommunicates to, out CommunicationMethod? workingMethod)
        {
            return IsInCommunicationRange(from, to, out workingMethod, to.Site, to.Coords);
           
        }


    }
}
