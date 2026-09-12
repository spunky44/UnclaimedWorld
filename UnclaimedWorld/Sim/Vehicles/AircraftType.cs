using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace UWGame.SimSide.Vehicles
{
    public class AircraftType
    {
        public float MaxAirSpeed;

        public float MaxVerticalAcceleration;
        public float MaxVerticalMoveSpeed;

        public float MaxRollDegreeWhenTurning;

        public float MaxPitchInRadians;

        public float PitchChangeSpeed;

        public float DuctChangeAngleSpeed = MathHelper.PiOver2;

        public float MaxPropellerSpeed = 30f;
        public float PropellerAcceleration = 4f;

        public float CruiseAltitude = 200f; // 200f

        public float AltitudeForDust = 200f;

        [XmlIgnore]
        public float EstimatedTakeOffLandingTime;

        public AircraftType()
        {
            EstimatedTakeOffLandingTime = 1.5f * MaxVerticalMoveSpeed / CruiseAltitude;
        }
/*
        /// <summary>
        /// deceleration rate
        /// </summary>
        private float deceleration = 0.5f;*/

       
    }
}
