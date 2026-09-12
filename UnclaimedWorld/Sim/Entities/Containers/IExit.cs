using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// used for choreographing entering, exiting, the physical placement after uncontaining, etc
    /// </summary>
    public enum ExitDoor
    {
        Door1 = 0,
        Door2 = 1,
        Door3 = 2,
        Door4 = 3,
        Max = 4, // the structure type with the most doors in the game
        NextAvailable = 5,
        NoneAvailable =6,	// need a door, but none currently available
        NoneNeeded = 7	// don't need a door reservation
    };
    /// <summary>
    /// IExit is for arbitrating the approach and departure of other entities
    /// to and from its parent entity's container.
    /// ALso uniquely capable of arbitrating the use of multiple doors to
    /// avoid traffic jams between approaching and/or departing entities
    /// This interface can be paired with IDock, which will handle the
    /// Approach and departure geometries and clearences, but that interface's
    /// implementations should defer to IExit (if present) for rally points, etc.
    /// </summary>
    public interface IExit
    {
        /// <summary>
        /// Ask this before reserveDoor as a kind of no-commitment check.
        /// If no doors are available for any purpose, then returns false
        /// </summary>
        bool IsDoorAvailable();

        /// <summary>
        /// Which door should I use, based on my EntityType (general case)
        /// or based on my instance coondition (more specific case)
        /// </summary>
         ExitDoor ReserveDoorForEntryOrExit(Entity entity, bool exiting);

        /// <summary>
        /// Here is the entity for you to enter/exit to the world in your own special way
        /// </summary>
        void UseDoor(Entity entity, ExitDoor door, bool exiting);

        /// <summary>
        /// if you get permission to enter/exit, but then don't/can't call usedoor, 
        /// you should call this to "give up" your permission
        /// </summary>
        void UnreserveDoor(ExitDoor door);

        /// <summary>
        /// assign a dynamic "rally point" for agents to move towards, either upon exiting or before entering
        /// </summary>
        void SetRallyPoint(Vector3 pos, ExitDoor door);

        /// <summary>
        /// retrieve a "rally point" for agents to move towards upon exiting, or just before entering    
        /// </summary>
        Vector3 GetRallyPoint(ExitDoor door = ExitDoor.NextAvailable);

        /// <summary>
        /// get the natural "rally point" for agents to move towards. this might likely be defined in the ContainerType class
        /// </summary>
        bool GetNaturalRallyPoint(ref Vector3 rallyPoint, bool offset = true);

        /// <summary>
        /// access to the "Door" position of the structure that we might exit/enter 
        /// </summary>
        bool GetDoorPosition(ref Vector3 position, bool exiting, out ExitDoor doorThatWasUsed, ExitDoor door = ExitDoor.NextAvailable);
       
        /// <summary>
        /// the single, generic access point, it is the naturalRallyPoint, unless specified otherwise in ContainerType
        /// 
        /// For containers with doors, this will be the rally point in front of one of the doors!
        /// </summary>
        Vector3 ComputeAccessPoint();
            //IDock.GetDockAccessPoint() should consult this method if that interface is also extended

        /// <summary>
        /// Returns true if it is okay for the docker to approach and prepare to dock. 
        /// False could mean the queue is full, for example.
        /// </summary>
        bool IsClearToApproach(Entity docker);
        //In impl, this will consult with IExit.IsDoorAvailable()

        /// <summary>
        /// Give Entity a Queue point to move to, and record that that point is taken.
        /// Returning null means there are none free
        /// </summary>
      //  bool ReserveApproachPosition(ref Entity docker, ref Vector3 position, out int index);
        //In impl, this will consult with IExit ReserveDoor()

        /// <summary>
        /// Give Entity the next Queue point to move to, and record that that point is taken.
        /// </summary>
        bool AdvanceApproachPosition(ref Entity docker, ref Vector3 position, out int index);
        //In impl, this will consult with IExit ReserveDoor()

        void GetDebugMarkers();

        /// <summary>
        /// Return true when it is OK for docker to begin entering the dock 
        /// The Dock will lift the restriction on one particular docker on its own, 
        /// so you must continually ask.
        /// </summary>
        bool IsClearToEnter(Entity docker);
        //In impl, this will consult with IExit.IsDoorAvailable()


        /// <summary>
        /// Return true when it is OK for docker to request a new Approach position.  The dock is in
        /// charge of keeping track of holes in the line, but the docker will remind us of their spot.
        /// </summary>
        bool IsClearToAdvance(Entity docker, int dockerIndex);

        /// <summary>
        /// Give Entity the point that is the start of his docking path
        /// Returning null means there is none free
        /// All functions take docker as arg so we could have multiple docks on a building.  
        /// Docker is not assumed, it is recorded and checked.    
        /// </summary>
        void GetEnterPosition(ref Entity docker, ref Vector3 position);
        //In impl, this will consult with IExit.GetRallyPoint()


        /// <summary>
        /// Give Entity the middle point of the dock process where the action() happens 	
        /// </summary>
        void GetDockPosition(ref Entity docker, ref Vector3 position);
        //In impl, this will consult with IExit.GetDoorPosition()

        /// <summary>
        /// Give Entity the point to move to when he is done  
        /// </summary>
        void GetDeparturePosition(ref Entity docker, ref Vector3 position);
        //In impl, this will consult with IExit.GetRallyPoint()


        /// <summary>
        /// Entity has reached the Enter Point.
        /// </summary>
        void OnApproachRallyReached(ref Entity docker);

        /// <summary>
        /// Entity has reached the Dock point        
        /// </summary>
        void OnDockReached(ref Entity docker);

        /// <summary>
        /// Entity has reached the exit.  He is no longer busy
        /// </summary>
        void OnDepartureRallyReached(ref Entity docker);

        /// <summary>
        /// Perform our specific action on visiting entity.
        /// examples, fill his basket with apples, his tank with fuel, his belly with food...
        /// Returning FALSE means there is nothing for you to do so entity should leave
        /// </summary>
        bool Action(ref Entity docker);

        /// <summary>
        /// Clear entity from any reserved points, and if entity was the reason we were Busy, we aren't anymore.
        /// </summary>
        void CancelDock(ref Entity docker);

        /// <summary>
        /// Is the dock open to accepting dockers
        /// </summary>
        bool DockOpen
        {
            get;
        }

        /// <summary>
        ///can entity dock here?
        /// </summary>
        bool IsAllowedtoDock(ref Entity dockingEntity);

        /// <summary>
        /// A minority of docks want to give you a final command to their rally point. 
        /// this should refer to data in ContainType in many cases
        /// </summary>
        bool UsesRallyPointAfterUndock
        {
            get;
        }
    }


    /// <summary>
    /// IDock is the interface that controls approaching, staying and departing
    /// non-moving entities, visiting them, rendezvousing with them, campfires
    /// trees, amphitheaters. It is also responsible for finding the right spot
    /// to enter transports, vehicles, tunnels, houses, etc.
    /// In selected Component classes, both IDock and IExit may be present,
    /// in these hybrid cases, the IDock implementations should defer some of the
    /// geometry computations to the IExit mehtods, to arbitrate the use of
    /// multiple doors, and to engage the container upon entering/exiting
    /// </summary>
    interface IDock
    {



        ///// <summary>
        ///// this is the same as the ExitAccessPoint in IExit, but if a component extends both IExit and IDock, then the IExit one wins, becuase it may be more specific (multiple doors, etc.)
        ///// </summary>
        //Vector3 AccessPoint
        //{
        //    get;
        //    set;
        //}
        ////In impl, this should defer to IExit.GetExitAccessPoint, if IExit is extended by subclass


    }


}
