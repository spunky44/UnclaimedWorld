using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Vehicles
{
    public class Passenger
    {
       
        public Vector3 Destination;
        public Entity Entity;

        public int DropoffWaypointNumber;
        public int? RendezvousWaypointNumber;

        public Passenger(int? RendezvousWaypointNumber, int DropoffWaypointNumber, Vector3 destination, Entity entity)
        {           
            Entity = entity;
            Destination = destination;
            this.DropoffWaypointNumber = DropoffWaypointNumber;
            this.RendezvousWaypointNumber = RendezvousWaypointNumber;
        }

    }

    /*
    public struct Passenger
    {
        //    public Point? Rendezvous;
        //    public Point Dropoff;
        public Vector3 Destination;
        public Entity Entity;

        public int DropoffWaypointNumber;
        public int? RendezvousWaypointNumber;

        public Passenger( int? RendezvousWaypointNumber, int DropoffWaypointNumber, Vector3 destination, Entity entity)
        {
           
            Entity = entity;
            Destination = destination;
            this.DropoffWaypointNumber = DropoffWaypointNumber;
            this.RendezvousWaypointNumber = RendezvousWaypointNumber;
        }

    }*/
}
