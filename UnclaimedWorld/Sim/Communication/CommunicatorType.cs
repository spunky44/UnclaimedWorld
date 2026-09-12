using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide.Communication
{
    public enum CommunicationMethod 
    { 
        Satellite, Radio, 
        Visual, // probably don't want to define this explicitly... especially since only one comm method can be defined...
        Direct // Direct/face-to-face comm is only possible when the two allegiances share the same site - not during Missions
    }  

    public class CommunicatorType
    {
        public CommunicationMethod Method;

        /// <summary>
        /// in kms.
        /// a null value means infinite range
        /// </summary>
        public double? Range;



        public bool IsInRange(double distance)
        {
            if (Range == null
                || Range >= distance)
            {
                return true;
            }

            return false;
        }


        public static string GetName(CommunicationMethod method)
        {
            switch (method)
            {
                case CommunicationMethod.Direct:
                    return "Direct";
                case CommunicationMethod.Visual:
                    return "Visual";
                case CommunicationMethod.Radio:
                    return "Radio";
                case CommunicationMethod.Satellite:
                    return "Satellite";

            }

            return "";
        }

    }
}
