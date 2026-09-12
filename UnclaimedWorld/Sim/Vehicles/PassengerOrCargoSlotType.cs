using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.Vehicles
{
    /// <summary>
    /// if both passenger and cargo, then we can carry 1 man or 2 units of bulk.
    /// </summary>
    public class PassengerOrCargoSlotType
    {
        public string AttachPointName;

        public PassengerSlotType PassengerSlotType;

        public CargoSlotType CargoSlotType;
        
       // public Vector2? Entrance;

        public Entrance Entrance;

        
        /// <summary>
        /// not attachment point for model!
        /// </summary>
        public Vector2? PointToFaceAtEntrance;

        
    }

    public class Entrance
    {
        public ExitDoor ExitDoor;
        public Vector2? Offset;
        
    }
}
