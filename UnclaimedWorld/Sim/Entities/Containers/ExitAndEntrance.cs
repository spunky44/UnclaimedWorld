using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Entities.Containers
{
    /// <summary>
    /// interface has Container as param, to prevent non-Containers, IExit implementors to call (GatheringSite)
    /// </summary>
    public class ExitAndEntrance
    {

        public static Vector3 GetRallyPoint(Container container, /*bool hasRallyPointInCourtyard,*/ /*Vector2[] doors,*/ ExitDoor door = ExitDoor.NextAvailable)
        {
            Entity entity = container.Parent;

            Vector3 rallyPoint = entity.PlaySiteLocation;
            if (!GetNaturalRallyPoint(container, ref rallyPoint))
                return rallyPoint; //oops

            Vector2[] doors = container.Parent.EntityType.ContainerType.GetDoors(); // ((HomeContainerType)parent.EntityType.ContainerType).Doors;
            bool hasRallyPointInCourtyard = container.Parent.EntityType.ContainerType.GetHasCourtyard();

            if (doors == null)
                return rallyPoint; //oops

            if (door == ExitDoor.NextAvailable)
            {
                //This SHOULD iterate my doors, and compute rally for the first available one

                //STUPID STUPID STUPID This just picks any old door at random

                //The first fix for this is to always pick the door closest to the entity requesting a door.
                //that way, even if the door gets re-evaluated, it will very likely be the same one each time.

                door = (ExitDoor)(The.Sim.GameplayRandomGenerator.Next(doors.Length, "ExitAndEntrance"));

            }


            if (door < ExitDoor.Max)//one of our indexed doors
            {
                if (doors != null && doors.Length > (int)door)
                {
                    if (hasRallyPointInCourtyard) //((HomeContainerType)parent.EntityType.ContainerType).HasRallyPointInCourtyard)
                    {
                        Vector2 doorOffset = doors[(int)door] * 0.5f; //halfway between natural and door

                        if (entity.FlipHorizontally)
                            doorOffset.X *= -1f;

                        rallyPoint += doorOffset.ToVector3();
                    }
                    else
                    {
                        //compute the rally points away from the center of the building instead
                        rallyPoint = entity.PlaySiteLocation;
                        Vector2 doorOffset = doors[(int)door];
                        //doorOffset.Normalize();
                        doorOffset *= 1.4f;//nominal distance for rally point???

                        if (entity.FlipHorizontally)
                            doorOffset.X *= -1f;

                        rallyPoint += doorOffset.ToVector3();
                    }
                }
            }

            return rallyPoint;
        }

        public static bool GetNaturalRallyPoint(Container container,/* bool hasRallyPointInCourtyard,*/ ref Vector3 rallyPoint, bool offset = true)
        {
            //here we procedurally compute a rally point for the contain structure based on assumptions
            //make an offset in front of the assumed "front" of the structure, distance commensurate with footprint
            //point at least two subtiles away from footprint
            //transform offset by parent matrix
            //adjust based on immovable objects or blocked tiles found here (these cases should have been prevented by the "occupied" tiles in the footprint upon bulding placement.
            //return offset
            bool hasRallyPointInCourtyard = container.Parent.EntityType.ContainerType.GetHasCourtyard();

            rallyPoint = container.Parent.PlaySiteLocation;
            if (hasRallyPointInCourtyard) //((HomeContainerType)parent.EntityType.ContainerType).HasRallyPointInCourtyard)
                return true;//because the middle of the building is the middle of the courtyard

            //just whip up a point in "front" of the building
            float rallyPointDistance = Math.Min(2, container.Parent.EntityType.StructureType.HeightInTiles) * MapManager.tileSize;
            rallyPoint.Y += rallyPointDistance;
            return true;
        }

        public static ExitDoor ReserveDoorForEntryOrExit(Entity container, Entity entity, bool exiting, ref ExitDoor simplifiedDoorToUseIndex)
        {
            Vector2[] doors = container.EntityType.ContainerType.GetDoors(); // ((HomeContainerType)parent.EntityType.ContainerType).Doors;

            //containType may indicate that reservations are not needed, if so just return NoneNeeded

            //iterate doors[] in containType
            //if doors[n] specifies an EntityFilter, apply it to entitytype of entity param
            //if it passes filter then stash this door as a candidate in local list

            //iterate candidates list
            //find closest one to entity location (or smarter logic, if predicting path approach vector, for example)

            //mark the flag in doors[n] to reservedForExit or for Entry based on bool param above  



            //TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO 
            //TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO 

            //this needs to be replaced by a proper impl
            //that keeps a door reserved for entry or exit until the entry or exit is complete
            //and then unreserves it for the next agent to ask for a door
            //this stupid one just rotates the door index arbitrarily
            //regardless of whether other agents might be using the door

            //TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO 
            //TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO          

            if ((int)(++simplifiedDoorToUseIndex) == doors.Length)
                simplifiedDoorToUseIndex = ExitDoor.Door1;

            /*  if (++stupid == ExitDoor.Max)
                  stupid = ExitDoor.Door1;*/

            return simplifiedDoorToUseIndex;


            // return ExitDoor.NoneNeeded;
        }


        public static void GetDebugMarkers(Container container, ref bool preventRecursion)
        {
#if (DEBUG || PROFILE)


            if (Kensei.Dev.Options.GetOption("Overlays.Markers") == false)
                return;

            if (preventRecursion) // so we can call DrawAllPoints from GetDoor, GetRally, etc.
                return;
            preventRecursion = true;

            // MapClient.ClearAllVisitorMarkers(this);

            ExitDoor doorThatWasUsed;
            ExitDoor maxDoor = (ExitDoor)container.Parent.EntityType.ContainerType.GetDoors().Length;
            for (ExitDoor currentDoor = ExitDoor.Door1; currentDoor < maxDoor /*ExitDoor.Max*/; currentDoor++)
            {
                Vector3 doorPosition = Vector3.Zero;

                doorPosition = GetDoorPosition(container, currentDoor); //, doors[(int)currentDoor]); // doors);

                /*
                if (GetDoorPosition(container, ref doorPosition, out doorThatWasUsed, currentDoor))
                {*/
                    The.MapUI.AddDebugMarker(doorPosition, Color.Chartreuse, container);

                    Vector3 rally = GetRallyPoint(container, currentDoor);
                    The.MapUI.AddDebugMarker(rally, Color.Green, container);
               // }

            }

            Vector3 naturalRallyPoint = container.Parent.PlaySiteLocation;
            if (GetNaturalRallyPoint(container, ref naturalRallyPoint))
            {
                The.MapUI.AddDebugMarker(naturalRallyPoint, Color.Gray, container);
            }

            preventRecursion = false;
#endif

        }

      /*  public static bool ReserveApproachPosition(Entity container, ref Entity docker, ref Vector3 position, out int index)
        {
            ExitDoor door = ReserveDoorForEntryOrExit(container, docker, false);

            if (door != ExitDoor.NoneAvailable) //valid door reserved
            {
                index = (int)door;
                ExitDoor doorThatWasUsed;
                GetDoorPosition(container, ref position, false, out doorThatWasUsed, door);
            }

            index = -1;
            return false;
        }*/

        public static bool GetDoorPosition(Container container, ref Vector3 position, ref ExitDoor doorIndex, out ExitDoor doorThatWasUsed)
        {
            position = container.Parent.PlaySiteLocation;

            Vector2[] doors = container.Parent.EntityType.ContainerType.GetDoors();

            if (doorIndex == ExitDoor.NextAvailable)
            {
                //TODO: iterate my doors, and return the first available one

                //  Vector2 doorPosition = new Vector2(0); //replace this with ContainerType data
                //  position += doorPosition.ToVector3();

                // Vector2[] doors = parent.EntityType.ContainerType.HomeContainerType.Doors;
                doorIndex = ExitDoor.Door1;

                position = GetDoorPosition(container, doorIndex); //, doors[(int)doorIndex]); // doors);


                //DrawAllPoints();

                doorThatWasUsed = doorIndex;

                return false;
            }

            if (doorIndex < ExitDoor.Max)//one of our indexed doors
            {
                //  Vector2[] doors = parent.EntityType.ContainerType.HomeContainerType.Doors;

                if (doors != null && doors.Length > (int)doorIndex)
                {
                    position = GetDoorPosition(container, doorIndex); //, doors[(int)doorIndex]); //doors);

                    // DrawAllPoints();

                    doorThatWasUsed = doorIndex;
                    return true;

                }

            }

            //   DrawAllPoints();

            doorThatWasUsed = doorIndex;
            return false;
        }

        public static Vector3 GetDoorPosition(Container container, Vector2 doorOffset) 
        {
            if (container.Parent.EntityType.RenderableTypeMode.RenderAsModelType != null) // WTF? Decouple!!
            {
                return GetRelativePointRotated(container.Parent, doorOffset).ToVector3();
            }
            else
            {
                Vector3 doorPosition = container.Parent.PlaySiteLocation; 

                if (container.Parent.FlipHorizontally) 
                {
                    doorOffset.X *= -1f;
                }

                doorPosition += doorOffset.ToVector3();
                return doorPosition;
            }
        }

        public static Vector3 GetDoorPosition(Container container, ExitDoor door) //, Vector2 doorOffset) 
        {
            Vector2 doorOffset = container.Parent.EntityType.ContainerType.GetDoors()[(int)door];

            return GetDoorPosition(container, doorOffset);
        }

        /// <summary>
        /// copied from Vehicle!
        /// </summary>
        /// <param name="pointOnModel"></param>
        /// <returns></returns>
        public static Vector2 GetRelativePointRotated(Entity container, Vector2? pointOnModel)
        {
            if (pointOnModel.HasValue)
            {
                Matrix rotationMatrix =
                    //Matrix.CreateRotationX(Roll) *
                       Matrix.CreateRotationZ(container.Rotation);

                Vector2 relativeExitPoint = new Vector2(container.Location.Value.X, container.Location.Value.Y) + Vector2.Transform(pointOnModel.Value, rotationMatrix);

                return relativeExitPoint;
            }
            else return new Vector2(container.Location.Value.X, container.Location.Value.Y);
        }

    }
}
