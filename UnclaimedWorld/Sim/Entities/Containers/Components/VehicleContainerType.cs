using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Vehicles;
using System.Xml.Serialization;
using UWGame.SimSide.Overland;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Buildings;

namespace UWGame.SimSide.Entities.Containers.Components
{
    public class VehicleContainerType : ContainerType
    {
        public enum VehicleTypes { Aircraft, Boat, LandVehicle }


        public VehicleTypes VehicleType;

       // public Vector2[] Doors;

        public RequiresReplenishType RequiresReplenishType;

        public ItemStorageType ItemStorageType;
        

        [XmlIgnore]
        public int MaxPassengers
        {
            get { return maxPassengers; }
        }

        private int maxPassengers;

        public global::UWGame.SimSide.Maps.SurfaceType.TransportType Transport;

        public float LoadingRadius;


       
        public float UnladenWeight;

        //   public float MaxAngularVelocity;

        /// <summary>
        /// This is most the vehicle can speed up or slow down in one second.
        /// </summary>
        public float MaxAcceleration;

        public float Deceleration;

       

        public RouteType[] CanNavigateRoutes;

        public AircraftType Aircraft;

        /// <summary>
        /// km/day
        /// </summary>
        public float AverageOverlandTravelSpeed;


        public TerminalType.TypesOfTerminal? CanUseTerminal;


        //   public Vector3 ModelOffset;
        //   public float ModelScale = 1f;

        /// <summary>
        /// Offsets for when the rotation = 0f, meaning when the vehicle faces right!
        /// </summary>
        /* public Vector2? DriversEntrance;
         public Vector2? PassengersEntrance1;
         public Vector2? PassengersEntrance2;
         public Vector2? PassengersEntrance3;
         public Vector2? LoadingPoint;


         public Vector2? DriversSeat;
         public Vector2? PassengersSeat1;
         public Vector2? PassengersSeat2;
         public Vector2? PassengersSeat3;
         public Vector2? CargoHold;*/

        //    public PassengerOrCargoSlotType DriversSeat;
        public PassengerOrCargoSlotType[] PassengerOrCargoSlotTypes;


        public enum Function { PersonalTransport, Hauling, Other }

        public Function MainFunction;

        public override RequiresReplenishType GetRequiresReplenishType()
        {
            return RequiresReplenishType;
        }


        public VehicleContainerType()
        { }

        public VehicleContainerType(float? itemStorageCapacity = null)
        {
            if (itemStorageCapacity.HasValue)
            {
                ItemStorageType = new ItemStorageType(itemStorageCapacity.Value);
            }

        }

        public override Container CreateContainer(Entity parent)
        {
            return new VehicleContainer(parent);
        }

        public override void Initialize()
        {
            base.Initialize();

            maxPassengers = 0;

            if (PassengerOrCargoSlotTypes != null)
            {
                foreach (PassengerOrCargoSlotType slot in PassengerOrCargoSlotTypes)
                {
                    if (slot.PassengerSlotType != null)
                    {
                        maxPassengers++;
                    }

                }
            }

            if (ItemStorageType != null)
            {
                ItemStorageType.Initialize();
            }

        }

        public override void PostInitValidate(EntityType parent, ref List<string> listOfErrors)
        {
            base.PostInitValidate(parent, ref listOfErrors);

            // check if the cargo spaces are correct:
            float totalStorageCapacity = 0f;

            if (ItemStorageType != null)
            {
                totalStorageCapacity = ItemStorageType.GetTotalCapacity();
            }

            float slotCapacity = 0;
            if (PassengerOrCargoSlotTypes != null)
            {
                for (int i = 0; i < PassengerOrCargoSlotTypes.Length; i++)
                {
                    CargoSlotType cargoSlot = PassengerOrCargoSlotTypes[i].CargoSlotType;
                    if (cargoSlot != null)
                    {
                        slotCapacity += cargoSlot.Capacity;
                    }
                }

                if (slotCapacity != totalStorageCapacity)
                {
                    EntityType.CreateValidationError(ref listOfErrors, 
                        string.Format("There is a mismatch between the total cargo slot capacity ({0}) and the total item storage capacity ({1}). They should be equal. Remember that a passenger/cargo slot has the capacity '2'.", slotCapacity, totalStorageCapacity));
                }

                
            }

        }


       
        public bool CanUseRoute(RouteType? routeType, bool isAirRoute, double distance)
        {
            if (Aircraft != null)
            {
                if (isAirRoute)
                {
                    return true;
                }
            }

            if (routeType.HasValue)
            {
                if (CanNavigateRoutes != null)
                {
                    if (CanNavigateRoutes.Contains(routeType.Value))
                    {
                        return true;
                    }
                }

            }

            return false; // TODO
        }
    }
}
