using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide.Entities;
namespace UWGame.SimSide.Vehicles
{
    public enum SteeringType
    {
        Pivot,
        HardTurn,
        SlowTurn
    }

    public class Aircraft 
    {       
        public SteeringType SteeringType;
                
        public float VerticalSpeed;

        public float RightDuctFanAngle = 0f;
        public float LeftDuctFanAngle = 0f;

        public float PropellerSpeed = 0f;
    }
}
