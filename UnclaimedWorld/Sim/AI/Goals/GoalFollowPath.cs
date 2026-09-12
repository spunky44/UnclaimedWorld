using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using UWGame.SimSide.AI.Pathfinding;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Items;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    public class GoalFollowPath : CompositeGoal
    {
        private const int numberSpacing = 100;

        public AI.Activities.GroupMoveActivity GroupMoveActivity;



        public List<PathFinderNode> Path;

        public List<Waypoint> WaypointPath;

        /// <summary>
        /// can be null
        /// </summary>
        public GoalMoveToPosition parentGoal;
        GoalID? snapshotParent;

        /// <summary>
        /// holds 4 points for catmull-rom spline
        /// </summary>
        //    private List<Vector2> PathPointsForSpline; // = new List<Vector2>();

        private Dictionary<Point, Point> rejectedPassengerDestinations;

        public bool MovingOutOfHarmsWay = false;

        private Vector3? endLocation;

        public float PermittedDistanceSquaredToDestination = GameData.Instance.Constants.DistanceSquaredLimitForWaypoints; // default


        /// <summary>
        /// Path must be supplied!!! endLocation is optional.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="path"></param>
        /// <param name="parentGoal"></param>
        /// <param name="ownersOfVehicles"></param>
        /// <param name="endLocation"></param>
        /// <param name="groupMoveActivity"></param>
        public GoalFollowPath(Entity owner, List<PathFinderNode> path, GoalMoveToPosition parentGoal, List<EntityGroupID> ownersOfVehicles, Vector3? endLocation, /* Vector2? endOffset,*/ GroupMoveActivity groupMoveActivity)
            : base(owner)
        {
            if (path == null) // && !endLocation.HasValue)
            {
                // error!
                throw new Exception("Path is null!");
            }


            this.Path = path;
            this.parentGoal = parentGoal;
            this.ownersOfVehicles = ownersOfVehicles;
            this.endLocation = endLocation;
            //this.endOffset = endOffset;
            this.GroupMoveActivity = groupMoveActivity;
        }




        public GoalFollowPath()
        {
        }


        protected override void Activate()
        {
            Status = Status.Active;

            // process path
            if (Path != null)
            {
                List<Vector3> path = SplitPathIntoSegmentsAndSmoothe(Path);
                Path = null; // let's erase the old path now we don't need it anymore. Instead of having to snapshot it... it contains duplicates...

                InsertAdditionalWaypoints(path);

                int number = numberSpacing;
                WaypointPath = new List<Waypoint>();
                foreach (Vector3 location in path)
                {
                    WaypointPath.Add(new Waypoint(number, location));
                    number += numberSpacing; //??? numbering increases by 100 so there is plenty of room to insert additional waypoints.
                }

                if (WaypointPath.Count == 0)
                {
                    // error?
                    throw (new Exception());
                }
            }
            /*  else // DELETE THIS...???
            { // move within the same tile:
                if (endOffset != null)
                {
                    WaypointPath = new List<Waypoint>();
                    WaypointPath.Add(new Waypoint(numberSpacing, MapManager.TileToWorldPosVector2(entity.MapPosition) + endOffset.Value));
                }
            }*/

            if (WaypointPath.Count == 0)
            {
                // error?
                throw (new Exception());
            }

            Entity vehicle;
            if (!entity.GetDrivenVehicle(out vehicle))
            {
                Status = Goals.Status.Failed;
                return;
            }

            if (vehicle != null)
            {   // update passenger info
                // we may be able to pick some of these up - no longer!
                vehicle.Vehicle.WaitingForPassengers = 0;
                // find drop off points for passengers on board:
                ManagePassengersWithNewPath(vehicle);

            }


            GetNextWaypoint();

        }

        private void InsertAdditionalWaypoints(List<Vector3> path)
        {
            Vector3 currentWaypoint;
            Vector3 previousWaypoint;

            for (int i = 0; i < path.Count; i++)
            {
                currentWaypoint = path[i];
                if (i > 0)
                {
                    previousWaypoint = path[i - 1];
                    if (!WaypointsAreAdjacent(previousWaypoint, currentWaypoint))
                    {
                        // step along the two waypoints
                        // step up the counter with the number we inserted:
                        i += AddExtraWaypointsBetweenWaypoints(path, i, previousWaypoint, currentWaypoint);

                    }

                }
                else
                {
                    Vector3 entityLocation = entity.PlaySiteLocation;
                    // compare current location with next waypoint:
                    /*   if (Vector2.DistanceSquared(entityLocation, currentWaypoint)
                           > MapManager.tileWidthSquared)
                       {*/
                    // step along the two waypoints, test each point:
                    // step up the counter with the number we inserted:
                    i += AddExtraWaypointsBetweenWaypoints(path, 0, entityLocation, currentWaypoint);
                    //}

                }

            }
        }

        private void ManagePassengersWithNewPath(Entity drivenVehicle)
        {

            Vehicle vehicleComponent;
            drivenVehicle.Find(out vehicleComponent);

            List<Passenger> newItinerary = new List<Passenger>();
            Passenger currentPassenger;
            // find new drop off points for passengers on board:
            for (int i = 0; i < vehicleComponent.PassengerItinerary.Count; i++)
            {
                Vector2? dropOffPoint = null;
                currentPassenger = vehicleComponent.PassengerItinerary[i];
                if (vehicleComponent.Passengers.Contains(currentPassenger.Entity)) // is he on board?
                {
                    bool addNewWaypoint;
                    int waypointIndex;
                    if (FindDropoffPoint(entity.PlaySiteLocation, currentPassenger.Destination,
                        out waypointIndex))
                    {
                        newItinerary.Add(new Passenger(null, waypointIndex, currentPassenger.Destination,
                            currentPassenger.Entity));
                    }
                    else
                    {
                        // tell this guy to get off now
                        currentPassenger.Entity.SendMessage(new Message(entity, Message.MessageTypes.GetOff, null));
                    }

                    /* OLD: if (FindDropoffPoint(entity.GetLocation(), currentPassenger.Destination,
                          out dropOffPoint, ))
                      {
                          newItinerary.Add(new Passenger(null, dropOffPoint.Value, currentPassenger.Destination,
                              currentPassenger.Entity));
                      }
                      else
                      {
                          // tell this guy to get off now
                          currentPassenger.Entity.HandleMessage(new Message(entity, Message.MessageTypes.GetOff, null));
                      }*/
                }
                // TODO: How can we pick these up with a new path? Send messages?
                /*  else
                  { // see if any old rendez vous points are usable (where passenger is NOT on board!!!):
                      if (currentPassenger.RendezvousWaypointNumber.HasValue)
                      {   // look through the entire path:
                          for (int n = 0; n < WaypointPath.Count; n++)
                          {
                              if (WaypointPath[n].Number == currentPassenger.RendezvousWaypointNumber.Value)
                              {
                                  newItinerary.Add(currentPassenger);
                                  entity.DrivingVehicle.WaitingForPassengers++;
                              }
                          }
                      }
                  }*/
            }

            vehicleComponent.PassengerItinerary = newItinerary;

        }

        /*  
         * protected override void Activate()
          {
              Status = Status.Active;
              if (entity.DrivingVehicle != null)
              {   // update passenger info
                  // we may be able to pick some of these up:
                  entity.DrivingVehicle.WaitingForPassengers = 0;

                  Vehicle vehicle = entity.DrivingVehicle;

                  List<Passenger> newItinerary = new List<Passenger>();
                  Passenger currentPassenger;
                  // find new drop off points for passengers on board:
                  for (int i = 0; i < vehicle.PassengerItinerary.Count; i++)
                  {
                      Point? dropOffPoint = null;
                      currentPassenger = vehicle.PassengerItinerary[i];
                      if (vehicle.Passengers.Contains(currentPassenger.Entity)) // is he on board?
                      {
                          if (FindDropoffPoint(entity.MapPosition, currentPassenger.Destination,
                              out dropOffPoint))
                          {
                              newItinerary.Add(new Passenger(null, dropOffPoint.Value, currentPassenger.Destination,
                                  currentPassenger.Entity));
                          }
                          else
                          {
                              // tell this guy to get off now
                              currentPassenger.Entity.HandleMessage(new Message(entity, Message.MessageTypes.GetOff, null));
                          }
                      }
                      else
                      { // see if any old rendez vous points are usable (where passenger is NOT on board!!!):
                          if (currentPassenger.RendezvousWaypointNumber.HasValue)
                          {   // look through the entire path:
                              for (int n = 0; n < Path.Count; n++)
                              {
                                  if (Path[n].X == currentPassenger.Rendezvous.Value.X &&
                                      Path[n].Y == currentPassenger.Rendezvous.Value.Y)
                                  {
                                      newItinerary.Add(currentPassenger);
                                      entity.DrivingVehicle.WaitingForPassengers++;
                                  }
                              }
                          }
                      }
                  }

                  entity.DrivingVehicle.PassengerItinerary = newItinerary;

              }
              else
              {           
                  entity.StartWalkAnimation();
              }

              List<Vector2> path = SplitPathIntoSegmentsAndSmoothe(Path);
              int number = numberSpacing;
              foreach (Vector2 location in path)
              {
                  WaypointPath.Add(new Waypoint(number, location));
                  number += numberSpacing; // numbering increases by 100 so there is plenty of room to insert additional waypoints.
              }
            

              GetNextWaypoint();
                       
          }
         * 
         * 
         * private void UpdateSplineList(PathFinderNode nextNode)
          {
              if (this.PathPointsForSpline.Count == 0)
              {
                  // first time
                  // add starting node twice:
                  AddSplineWaypoint(entity.MapPosition.X, entity.MapPosition.Y);
                  AddSplineWaypoint(entity.MapPosition.X, entity.MapPosition.Y);

                  // add current target:
                  AddSplineWaypoint(nextNode.X, nextNode.Y);

                  if (Path.Count > 0)//1)
                  {   // add next target:
                      AddSplineWaypoint(Path[0].X, Path[0].Y);
                  }
                  else
                  {
                      // add current target again.
                      AddSplineWaypoint(nextNode.X, nextNode.Y);
                  }

              }
              else
              {
                  PathPointsForSpline.RemoveAt(0);

                  if (Path.Count > 0)//1)
                  {   // look ahead at next target again:
                      AddSplineWaypoint(Path[0].X, Path[0].Y);
                  }
                  else
                  {
                      // add current target again.
                      AddSplineWaypoint(nextNode.X, nextNode.Y);
                  }
              }

          } */

        /*   private void UpdateSplineList(Vector2 next)
           {
               if (this.PathPointsForSpline.Count == 0)
               {
                   // first time
                   // add starting node twice:
                   Vector3 location = entity.GetLocation();
                   Vector2 loc = new Vector2(location.X, location.Y);
                   PathPointsForSpline.Add(loc);
                   PathPointsForSpline.Add(loc);
              
                   // add current target:
                   PathPointsForSpline.Add(next);

                   if (WaypointPath.Count > 0)//1)
                   {   // add next target:                   
                       PathPointsForSpline.Add(WaypointPath[0]);
                   }
                   else
                   {
                       // add current target again.                    
                       PathPointsForSpline.Add(next);
                   }

               }
               else
               {
                   PathPointsForSpline.RemoveAt(0);

                   if (WaypointPath.Count > 0)
                   {   // look ahead at next target again:                    
                       PathPointsForSpline.Add(WaypointPath[0]);
                   }
                   else
                   {
                       // add current target again.
                       PathPointsForSpline.Add(next);
                   }
               }     

           }

           private void AddSplineWaypoint(int x, int y)
           {
            
               PathPointsForSpline.Add(new Vector2(MapManager.tileWidth * (x + 0.5f), MapManager.tileHeight * (y + 0.5f)));
           }

           private void AddSplineWaypoint(Vector2 waypoint)
           {
               PathPointsForSpline.Add(waypoint);
           }
           */
        private void GetNextWaypoint()
        {
            if (WaypointPath.Count == 0)
            {
                // error?

                throw (new Exception());
            }

            Waypoint next;
            Vector2? lookAheadDirection = null; // the vehicles need this to slow down in turns.
            Vector3 location = entity.PlaySiteLocation;
            do
            {
                next = WaypointPath[0]; // error here?
                WaypointPath.RemoveAt(0);
            }   // skip waypoints we are already near.
            while (WaypointPath.Count > 0 && Vector2.DistanceSquared(location.ToVector2(), next.Location.ToVector2()) < 4f);

            if (WaypointPath.Count > 0)
            {
                Vector2 lookahead = (WaypointPath[0].Location - next.Location).ToVector2();
                lookahead.Normalize();
                lookAheadDirection = lookahead;

            }

            Status = Status.Active;

            bool isLastEdge = WaypointPath.Count == 0;

            float permittedDistance;
            if (isLastEdge)
            {
                permittedDistance = PermittedDistanceSquaredToDestination;
            }
            else
            {
                permittedDistance = GameData.Instance.Constants.DistanceSquaredLimitForWaypoints;
            }

            GoalTraverseEdgeBetweenWaypoints newGoal = new GoalTraverseEdgeBetweenWaypoints(entity, next.Location, lookAheadDirection,
                GoalTraverseEdgeBetweenWaypoints.ComputeWaypointRay(next.Location, entity),
                next.Number, isLastEdge, MovingOutOfHarmsWay, ownersOfVehicles, GroupMoveActivity)
                {
                    PermittedDistanceSquaredToDestination = permittedDistance
                };

            AddSubgoal(newGoal);

            if (GroupMoveActivity != null && GroupMoveActivity.IsLeader(entity))
            {   // update followers:
                Waypoint followingWaypoint = null;
                if (WaypointPath.Count > 0 && WaypointPath[0].Location != next.Location)
                {   // look ahead:
                    followingWaypoint = WaypointPath[0];
                }
                AssignWaypointsToFollowers(next, followingWaypoint, entity, GroupMoveActivity, WaypointPath.Count == 0);
            }
        }

        public static void AssignWaypointsToFollowers(Waypoint leadersNextWaypoint, Waypoint leadersFollowingWaypoint, Entity leader, GroupMoveActivity groupMoveActivity, bool isLastEdge)
        {
            if (groupMoveActivity.Members.Count > 0)
            {
                Entity member;

                float rotation;
                Vector2 futureFormationHeading;
                Vector3 leaderPos = leader.PlaySiteLocation;
                // Vector2 leaderPosition = new Vector2(leaderPos.X, leaderPos.Y);
                if (leadersFollowingWaypoint == null)
                {
                    futureFormationHeading = new Vector2(leadersNextWaypoint.Location.X - leaderPos.X, leadersNextWaypoint.Location.Y - leaderPos.Y);
                }
                else
                {   // look ahead:
                    futureFormationHeading = new Vector2(leadersFollowingWaypoint.Location.X - leaderPos.X, leadersFollowingWaypoint.Location.Y - leaderPos.Y);
                }

                rotation = (float)Math.Atan2(futureFormationHeading.Y, futureFormationHeading.X);
                Matrix rotMatrix = Matrix.CreateRotationZ(rotation);

                for (int i = 0; i < groupMoveActivity.Members.Count; i++)
                {
                    member = groupMoveActivity.Members[i];
                    if (member != leader)
                    {
                        Intelligence memberIntelligence = member.Intelligence;

                        // the proposed member position:
                        Vector3 memberPosition = groupMoveActivity.ComputePositionFromLeader(member, leadersNextWaypoint.Location, rotMatrix);
                        Waypoint newWaypoint;

                        // see if there is a clear line of sight (and movement) from the leader's waypoint to the follower's:
                        MovementMap memberMoveMap = memberIntelligence.Allegiance.SharedKnowledge.GetMovementMap(memberIntelligence.ProtectionLevel, member.EntityType, memberIntelligence.ThreatStance);
                        SurfaceType.TransportType memberTransport = member.GetTransportType();
                        if (MapManager.IsPathClearToPoint(leadersNextWaypoint.Location, member.PlaySiteLocation, memberMoveMap, memberTransport))
                        {
                            newWaypoint = new Waypoint(leadersNextWaypoint.Number, memberPosition, isLastEdge);
                        }
                        else
                        {
                            // the line is blocked.
                            // make a map search to find closest valid waypoint
                            MovementMap moveMap = memberIntelligence.Allegiance.SharedKnowledge.GetMovementMap(memberIntelligence.ProtectionLevel,
                                member.EntityType, memberIntelligence.ThreatStance);

                            Point closestPoint = leader.Intelligence.PathPlanner.GetClosestPointToDestination(moveMap.Layers[member.GetTransportType()],
                                memberPosition, 400, leader.PlaySiteLocation);

                            newWaypoint = new Waypoint(leadersNextWaypoint.Number, MapManager.TileToWorldPos(closestPoint), isLastEdge);
                        }

                        memberIntelligence.GroupMoveAssignedWaypoint = newWaypoint;

                        // tell followers to stop waiting:
                        member.SendMessage(new Message(Message.MessageTypes.StartGroupMovement));
                    }

                }

            }
        }

        /// <summary>
        /// Sets status Failed if path is blocked.
        /// </summary>
        /*    private void GetNextPathNode()
            {
                //get a reference to the next edge
                PathFinderNode next;
                do
                {
                    next = Path[0];
                    Path.RemoveAt(0);

                } while (next.X == entity.MapPosition.X && next.Y == entity.MapPosition.Y);

                if (entity.DrivingVehicle != null)
                {

                }

                UpdateSplineList(next);      

                Status = Status.Active;
           
                AddSubgoal(new GoalTraverseEdge(entity, next, Path.Count == 0, PathPointsForSpline, MovingOutOfHarmsWay, ownersOfVehicles));
            }*/

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
            /*  ActivateIfInactive();

              if (Status == Goals.Status.Active)
              {*/
            if (!IsDelayed(elapsed))
            {
                Status = ProcessSubgoals(elapsed);


                if (Status == Status.Completed)
                {
                    if (WaypointPath.Count > 0)
                    {
                        GoalTraverseEdgeBetweenWaypoints traverseEdge = null;
                        if (Subgoals.Count > 0)
                        {
                            traverseEdge = Subgoals.Peek() as GoalTraverseEdgeBetweenWaypoints;
                        }

                        if (traverseEdge != null)
                        {
                            Entity vehicleEntity;
                            if (!entity.GetDrivenVehicle(out vehicleEntity))
                            {
                                // handle vehicle blown up...
                                Status = Goals.Status.Failed;

                                // ExitIfFailedOrCompleted();
                                return; // Status;
                            }

                            if (vehicleEntity != null)
                            {

                                Vehicle vehicleComponent;
                                vehicleEntity.Find(out vehicleComponent);

                                if (vehicleComponent.PassengerItinerary != null &&
                                    vehicleComponent.PassengerItinerary.Count > 0)
                                {
                                    HandlePassengerDropoffsAndPickups(traverseEdge, vehicleComponent);
                                }
                                else
                                {
                                    GetNextWaypoint();
                                }
                            }
                            else
                            {
                                GetNextWaypoint();
                            }
                        }

                    }
                    else if (GroupMoveActivity != null)
                    {

                    }
                }
            }
            /*  }

              ExitIfFailedOrCompleted();

              return Status;*/
        }

        private void HandlePassengerDropoffsAndPickups(GoalTraverseEdgeBetweenWaypoints traverseEdge, Vehicle vehicleComponent)
        {
            // handle drop offs and pick ups:
            Passenger passenger;
            bool waitHere = false;
            int startAtNextIndex = 0;
            while (IsAtRendezvous(vehicleComponent, traverseEdge.Number, out passenger, ref startAtNextIndex))
            {
                /*We are waiting. We must now do something to prevent being seen as completed. */
                AddSubgoal(new GoalWaitForPassenger(entity, 12, passenger.Entity));
                // don't send the message until the goal is active and ready to handle the answer!
                //passenger.Value.Entity.HandleMessage(new Message(Owner, Message.MessageTypes.HopOnBoard, null)); 
                waitHere = true;

            }

            // we may drop somebody off here also:
            while (IsAtDropoff(vehicleComponent, traverseEdge.Number, out passenger))
            {
                vehicleComponent.TellPassengerToGetOff(entity, passenger);

                StartDelay(0.8);
            }

            Status = Status.Active;
            if (!waitHere)
            {
                GetNextWaypoint();
                // Status = Status.Active;
            }
        }

        /*   public override Status Process(GameTime elapsed)
           {
               //if status is inactive, call Activate()
               ActivateIfInactive();

               if (!IsDelayed(elapsed))
               {
                   Status = ProcessSubgoals(elapsed);

                   //if there are no subgoals present check to see if the path still has edges.
                   //remaining. 
                   if (Status == Status.Completed && Path.Count > 0)
                   {
                       if (entity.DrivingVehicle != null && entity.DrivingVehicle.PassengerItinerary != null &&
                           entity.DrivingVehicle.PassengerItinerary.Count > 0)
                       {
                           Passenger? passenger;
                           bool waitHere = false;
                           int startAtNextIndex = 0;
                           while (IsAtRendezvous(out passenger, ref startAtNextIndex))
                           {
                               //We are waiting. We must now do something to prevent being seen as completed. 
                               AddSubgoal(new GoalWaitForPassenger(entity, 12, passenger.Value.Entity));
                               // don't send the message until the goal is active and ready to handle the answer!
                               //passenger.Value.Entity.HandleMessage(new Message(Owner, Message.MessageTypes.HopOnBoard, null)); 
                               waitHere = true;

                           }

                           // we may drop somebody off here also:
                           while (IsAtDropoff(out passenger))
                           {
                               entity.DrivingVehicle.TellPassengerToGetOff(entity, passenger.Value);
                           
                               StartDelay(0.8);
                           }

                           Status = Status.Active;
                           if (!waitHere)
                           {
                               GetNextPathNode();
                               // Status = Status.Active;
                           }
                        
                       }
                       else
                       {
                           GetNextPathNode();
                       }
                   }
               }

               return Status;
           }*/

        private bool IsAtRendezvous(Vehicle vehicleComponent, int waypointNumber, out Passenger passenger, ref int lastIndex)
        {
            passenger = null;

            for (int i = lastIndex; i < vehicleComponent.PassengerItinerary.Count; i++)
            {
                if (waypointNumber == vehicleComponent.PassengerItinerary[i].RendezvousWaypointNumber &&
                    !vehicleComponent.Passengers.Contains(vehicleComponent.PassengerItinerary[i].Entity)) // not already on board...
                {
                    passenger = vehicleComponent.PassengerItinerary[i];
                    lastIndex = i + 1;
                    return true;
                }

            }
            return false;
        }
        /*
         * private bool IsAtRendezvous(int waypointNumber, out Passenger? passenger, ref int lastIndex)
        {
            passenger = null;
            for (int i = lastIndex; i < entity.DrivingVehicle.PassengerItinerary.Count; i++)
            {
                if (entity.MapPosition == entity.DrivingVehicle.PassengerItinerary[i].Rendezvous &&
                    !entity.DrivingVehicle.Passengers.Contains(entity.DrivingVehicle.PassengerItinerary[i].Entity)) // not already on board...
                {
                    passenger = entity.DrivingVehicle.PassengerItinerary[i];
                    lastIndex = i + 1;
                    return true;
                }
                
            }
            return false;
        }
         * 
         *  private bool IsAtDropoff(out Passenger? passenger)
        {
            passenger = null;
            for (int i = 0; i < entity.DrivingVehicle.PassengerItinerary.Count; i++)
            {
                if (entity.MapPosition == entity.DrivingVehicle.PassengerItinerary[i].Dropoff &&
                    entity.DrivingVehicle.Passengers.Contains(entity.DrivingVehicle.PassengerItinerary[i].Entity)) // is on board...
                {
                    passenger = entity.DrivingVehicle.PassengerItinerary[i];
                    return true;
                }
            }
            return false;
        }
         * */

        private bool IsAtDropoff(Vehicle vehicleComponent, int waypointNumber, out Passenger passenger)
        {
            passenger = null;

            for (int i = 0; i < vehicleComponent.PassengerItinerary.Count; i++)
            {
                if (waypointNumber == vehicleComponent.PassengerItinerary[i].DropoffWaypointNumber &&
                    vehicleComponent.Passengers.Contains(vehicleComponent.PassengerItinerary[i].Entity)) // is on board...
                {
                    passenger = vehicleComponent.PassengerItinerary[i];
                    return true;
                }
            }
            return false;
        }


        private Vector3? GetDestination()
        {
            if (WaypointPath.Count > 0)
            {
                return WaypointPath[WaypointPath.Count - 1].Location;
            }

            return null;
        }

        public override bool HandleMessage(Message message)
        {
            //We are a composite goal. Therefore, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            if (!handled)
            {
                switch (message.MessageType)
                {
                    case Message.MessageTypes.DrivenVehicleIsNear:
                        // Are we looking for a ride? We must not be driving already.

                        // we must not go here again after getting GoToRendezvousPoint!

                        if (parentGoal != null && parentGoal.VehicleUseByGoal == GoalMoveToPosition.VehicleUse.FreeUpAfterUse &&
                            parentGoal.UsedVehicle == null)
                        {
                            if (!entity.IsInAVehicle())
                            {
                                //Hitch A Ride:
                                Entity vehicle;
                                if (!message.Sender.GetDrivenVehicle(out vehicle))
                                {
                                    // his vehicle got blown up...                                   
                                    return true;
                                }

                                if (vehicle != null)
                                {
                                    Vehicle vehicleComponent;
                                    vehicle.Find(out vehicleComponent);

                                    // disregard bulk of carried items - we are not hauling anything...
                                    if (vehicleComponent.Passengers.Count + vehicleComponent.WaitingForPassengers < ((VehicleContainerType)vehicle.EntityType.ContainerType).MaxPassengers
                                        /* (Point)message.OtherInfo == parentGoal.Destination */)
                                    {
                                        // Pick me up:
                                        Point currentPos;
                                        // currentPos = GetTraversingDestination();
                                        Vector3? destination = GetDestination();
                                        if (destination != null)
                                        {
                                            message.Sender.SendMessage(
                                                new Message(entity, Message.MessageTypes.PickMeUp,
                                                new FromTo(entity.PlaySiteLocation, destination.Value)));  //new FromTo(currentPos, parentGoal.Destination)));
                                        }
                                    }
                                }

                            }
                        }
                        return true;

                    case Message.MessageTypes.PickMeUp:
                        {
                            Entity vehicleEntity;
                            if (!entity.GetDrivenVehicle(out vehicleEntity))
                            {
                                // handle vehicle blown up...
                                Status = Goals.Status.Failed;
                                return true;
                            }

                            if (vehicleEntity != null)
                            {
                                int? dropOffPointIndex;
                                // see if it makes sense to pick up this passenger:
                                if (FindPassengerRoute(((FromTo)message.OtherInfo).From, ((FromTo)message.OtherInfo).To, out dropOffPointIndex))
                                {
                                    // select rendez vous point:
                                    //  Point currentPos;
                                    //  currentPos = GetTraversingDestination();
                                    int rendezvousIndex;
                                    FindPassengerRendezvousPoint(((FromTo)message.OtherInfo).From, out rendezvousIndex);

                                    message.Sender.SendMessage(
                                                    new Message(entity, Message.MessageTypes.GoToRendezvousPoint, WaypointPath[rendezvousIndex].Location));

                                    Vehicle vehicleComponent;
                                    vehicleEntity.Find(out vehicleComponent);

                                    vehicleComponent.PassengerItinerary.Add(
                                        new Passenger(WaypointPath[rendezvousIndex].Number, WaypointPath[dropOffPointIndex.Value].Number, ((FromTo)message.OtherInfo).To, message.Sender));

                                    vehicleComponent.WaitingForPassengers++;

                                }
                            }
                            return true;
                        }
                    case Message.MessageTypes.GoToRendezvousPoint:
                        {

                            Entity vehicleEntity;
                            if (!message.Sender.GetDrivenVehicle(out vehicleEntity))
                            {
                                // his vehicle got blown up...
                                return true;
                            }

                            if (vehicleEntity != null)
                            {
                                Vehicle vehicleComponent;
                                vehicleEntity.Find(out vehicleComponent);

                                Vector3 positionToStandAt;
                                vehicleComponent.GetRendezvousPoint((Vector3)message.OtherInfo, entity.PlaySiteLocation, out positionToStandAt);

                                if (positionToStandAt != entity.Location)
                                {
                                    AddSubgoal(new GoalMoveToPosition(entity, positionToStandAt, ownersOfVehicles, GoalMoveToPosition.VehicleUse.NoVehicle));
                                }
                                // this goal either waits for the vehicle or sends a message to the driver that he can start:
                                AddSubgoal(new GoalEnterVehicleAsPassenger(entity, vehicleEntity, this, null, ownersOfVehicles));

                                // entityIntelligence.ListeningForTriggers[(int)TriggerTypes.DrivingVehicle] = false;
                            }

                            return true;
                        }
                    case Message.MessageTypes.StartGroupMovement:
                        if (GroupMoveActivity.IsFollower(entity))
                        {
                            // we are finding our way towards a waypoint already - prevent this goal being cleared:
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    default: return false;

                }
            }
            else return true;

        }

        private void AddRejectedPassengerDestination(Point destination)
        {
            if (rejectedPassengerDestinations == null)
            {
                rejectedPassengerDestinations = new Dictionary<Point, Point>();
            }

            rejectedPassengerDestinations.Add(destination, destination);

        }

        private bool FindPassengerRoute(Vector3 passengerLocation, Vector3 destination, out int? dropOffWaypointIndex)
        {
            dropOffWaypointIndex = null;
            Point destinationTile = MapManager.WorldPosToTile(destination);
            if (rejectedPassengerDestinations != null &&
                rejectedPassengerDestinations.ContainsKey(destinationTile)
                || WaypointPath.Count < GameData.Instance.AIConstants.ShortestRouteThatAcceptsPassengers)
            {
                return false;
            }

            int waypointIndex;
            if (FindDropoffPoint(passengerLocation, destination, out waypointIndex))
            {
                dropOffWaypointIndex = waypointIndex;
                return true;
            }
            else
            {
                AddRejectedPassengerDestination(destinationTile);
                return false;
            }
        }

        /*    private void AddNewWaypoint(int waypointIndex, Vector2 dropOffPoint)
            {
                int newNumber = numberSpacing / 2;

                if (waypointIndex > 0)
                {
                    Waypoint previousWaypoint = WaypointPath[waypointIndex - 1];
                    if (waypointIndex < WaypointPath.Count)
                    {
                        newNumber = (previousWaypoint.Number + WaypointPath[waypointIndex + 1].Number) / 2;
                    }
                    else
                    {
                        newNumber = previousWaypoint.Number + numberSpacing;
                    }
                    previousWaypoint.nextWaypointIsAdjacent = null;
                    previousWaypoint.DirectionToNextWaypoint = null;
                    previousWaypoint.DistanceToNextWaypoint = null;

                    WaypointPath[waypointIndex - 1] = previousWaypoint;
                    WaypointPath.Insert(waypointIndex, new Waypoint(newNumber, dropOffPoint));
                }
                else
                {
                    WaypointPath.Insert(waypointIndex, new Waypoint(newNumber, dropOffPoint));
                }

            }*/

        /*   private bool FindDropoffPoint(Point passengerLocation, Point passengerDestination, out Point? dropOffPoint)
           {
               int minDistance = 1000000000;
               int distance;
               int minDistanceIndex = -1;

               int lastValidDropoffPoint = 3; // don't drop off people less than X tiles from our destination - it looks stupid...

               for (int i = 0; i < Path.Count - lastValidDropoffPoint; i++) 
               {
                   distance = Common.DistanceSquared(Path[i].X, Path[i].Y, passengerDestination);

                   if (distance < minDistance)
                   {
                       minDistance = distance;
                       minDistanceIndex = i;
                   }
               }

               // test the final path node:
               int finalNode = Path.Count - 1;
               distance = Common.DistanceSquared(Path[finalNode].X, Path[finalNode].Y, passengerDestination);

               if (distance < minDistance)
               {
                   minDistance = distance;
                   minDistanceIndex = finalNode;
               }


               if (minDistance < Common.DistanceSquared(passengerLocation, passengerDestination))
               {
                   dropOffPoint = new Point(Path[minDistanceIndex].X, Path[minDistanceIndex].Y);
                   return true;
               }

               dropOffPoint = passengerLocation;
               return false;
           }*/
        /*
        private bool WaypointsAreAdjacent(Waypoint w1, Waypoint w2)
        {
            if (w1.nextWaypointIsAdjacent != null)
            {
                return w1.nextWaypointIsAdjacent.Value;
            }
            else
            {
                // compare and store the result! We consider waypoints in the same tile adjacent for this purpose!!!
                Point p1 = MapManager.WorldPosToTile(w1.Position);
                Point p2 = MapManager.WorldPosToTile(w1.Position);
                bool isAdjacent = p1 == p2 || Common.IsAdjacent(p1, p2);
                w1.nextWaypointIsAdjacent = isAdjacent;

                return isAdjacent;               
            }
        }
        */
        private bool WaypointsAreAdjacent(Vector3 w1, Vector3 w2)
        {

            // We consider waypoints in the same tile adjacent for this purpose!!!
            Point p1 = MapManager.WorldPosToTile(w1);
            Point p2 = MapManager.WorldPosToTile(w2);
            bool isAdjacent = (p1 == p2 || Common.IsAdjacent(p1, p2));
            // w1.nextWaypointIsAdjacent = isAdjacent;

            return isAdjacent;

        }

        private int AddExtraWaypointsBetweenWaypoints(List<Vector3> path, int startingIndex, Vector3 location1, Vector3 location2)
        {
            // compute the vector and distance:
            float distanceToNextWaypoint = Vector2.Distance(location1.ToVector2(), location2.ToVector2());
            float stepSize = GameData.Instance.Constants.TilesBetweenAddedWaypoints * MapManager.tileSize;
            int newWaypointCounter = 0;

            if (distanceToNextWaypoint > stepSize)
            {
                Vector2 directionToNextWaypoint =
                    (location2 - location1).ToVector2() / distanceToNextWaypoint;

                int stepsToTake = (int)(distanceToNextWaypoint / stepSize);

                Vector3 currentPosition = location1;

                for (newWaypointCounter = 0; newWaypointCounter < stepsToTake; newWaypointCounter++)
                {
                    currentPosition += new Vector3(MapManager.tileSize * directionToNextWaypoint, 0f);

                    path.Insert(startingIndex, currentPosition);
                    startingIndex++;
                }
            }
            return newWaypointCounter;
        }

        /*   private bool FindClosestPointBetweenWaypoints(Vector2 location1, Vector2 directionToNextWaypoint,
               float distanceBetweenWaypoints, float minDistance, ref Vector2 closestPoint, Vector3 targetLocation)
           {
               int stepsToTake = (int)(distanceBetweenWaypoints / MapManager.tileWidth);
           
               float distance;
            
               Vector2 currentPosition = location1;
               Vector2? minDistancePosition = null;

               for (int i = 0; i < stepsToTake; i++)
               {
                   currentPosition += MapManager.tileWidth * directionToNextWaypoint;

                   distance = Common.DistanceSquared(targetLocation, currentPosition);


                   if (distance < minDistance)
                   {
                       minDistance = distance;
                       minDistancePosition = currentPosition;
                   }
               }

               if (minDistancePosition != null)
               {
                   closestPoint = minDistancePosition.Value;
                   return true;
               }
               return false;

           }*/


        private bool FindDropoffPoint(Vector3 passengerLocation, Vector3 passengerDestination, out int dropOffWayPointIndex)
        {
            float minDistance = 1000000000; // just any large number.
            float distance;

            // don't drop off people less than 3 tiles from our destination - it looks stupid...
            FindClosestWaypoint(new Vector3(passengerDestination.X, passengerDestination.Y, 0f), 3, out dropOffWayPointIndex, ref minDistance);

            // test the final destination:
            int finalNode = WaypointPath.Count - 1;
            distance = Common.DistanceOctile(WaypointPath[finalNode].Location, passengerDestination);

            if (distance < minDistance)
            {
                minDistance = distance;
                dropOffWayPointIndex = finalNode;
            }

            // see if the drop off point is better than the passengers own distance:
            if (minDistance < Common.DistanceOctile(passengerLocation, passengerDestination))
            {
                return true;
            }

            return false;
        }

        private bool FindClosestWaypoint(Vector3 targetLocation, int cutOff,
            out int closestWayPointIndex, ref float minDistance)
        {

            closestWayPointIndex = -1;

            float distance;


            //  float minimumDistanceSquaredToFinalDestination = (float)Math.Pow(3f * MapManager.tileWidth, 2f); 

            // int dropOffWayPointIndex = -1;

            Waypoint currentWaypoint;

            for (int i = 0; i < WaypointPath.Count - cutOff; i++)
            {
                currentWaypoint = WaypointPath[i];

                distance = Common.DistanceOctile(targetLocation, currentWaypoint.Location);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestWayPointIndex = i;
                }

            }

            if (closestWayPointIndex != -1)
            {
                return true;
            }
            return false;
        }

        /*
        private bool FindClosestWaypoint(Vector3 targetLocation, 
            out Vector2? closestPoint, out int closestWayPointIndex, out bool closestPointIsNewWaypoint, ref float minDistance)
        {
            closestPointIsNewWaypoint = false;
            closestWayPointIndex = -1;
          
            float distance;

            // don't drop off people less than 3 tiles from our destination - it looks stupid...
            //  float minimumDistanceSquaredToFinalDestination = (float)Math.Pow(3f * MapManager.tileWidth, 2f); 

            //bool testPointsBetweenWaypoints = false;

            //bool bestDropoffIsNewWaypoint = false;
            Vector2 newDropoff = Vector2.Zero;
            // int dropOffWayPointIndex = -1;

            Waypoint previousWaypoint;
            Waypoint currentWaypoint;

            float distanceToNextWaypoint;

            Vector2 entityLocation = new Vector2(entity.GetLocation().X, entity.GetLocation().Y);

            for (int i = 0; i < WaypointPath.Count; i++)
            {
                currentWaypoint = WaypointPath[i];
                if (i > 0)
                {
                    previousWaypoint = WaypointPath[i - 1];
                    if (!WaypointsAreAdjacent(previousWaypoint, currentWaypoint))
                    {
                        previousWaypoint.nextWaypointIsAdjacent = false;

                        if (previousWaypoint.DirectionToNextWaypoint == null)
                        { // we didn't already compute the vector and distance:
                            previousWaypoint.DistanceToNextWaypoint =
                                Vector2.Distance(currentWaypoint.Position, previousWaypoint.Position);
                            previousWaypoint.DirectionToNextWaypoint =
                                (currentWaypoint.Position - previousWaypoint.Position) / previousWaypoint.DistanceToNextWaypoint;

                        }

                        // step along the two waypoints, test each point:
                        closestPointIsNewWaypoint = FindClosestPointBetweenWaypoints(previousWaypoint.Position, previousWaypoint.DirectionToNextWaypoint.Value,
                            previousWaypoint.DistanceToNextWaypoint.Value, minDistance, ref newDropoff, targetLocation);

                    }
                    else
                    {
                        previousWaypoint.nextWaypointIsAdjacent = true;
                        distance = Common.DistanceSquared(targetLocation, currentWaypoint.Position);
                        if (distance < minDistance)
                        {
                            closestPointIsNewWaypoint = false;
                            minDistance = distance;
                            closestWayPointIndex = i;
                        }
                    }
                    // for structs, we must store the changes:
                    WaypointPath[i - 1] = previousWaypoint;

                }
                else
                {  // compare current location with next waypoint:
                    if (Common.DistanceSquared(entity.Location, currentWaypoint.Position)
                        > MapManager.tileWidthSquared)
                    {
                        // step along the two waypoints, test each point:

                        float entityDistanceToWaypoint = Vector2.Distance(entityLocation, currentWaypoint.Position);
                        Vector2 entityDirectionToWaypoint = currentWaypoint.Position - entityLocation;
                        //entityDirectionToWaypoint.X = currentWaypoint.Position.X - entity.Location.X;
                        //entityDirectionToWaypoint.Y = currentWaypoint.Position.Y - entity.Location.Y;
                        entityDirectionToWaypoint = entityDirectionToWaypoint / entityDistanceToWaypoint;

                        closestPointIsNewWaypoint = FindClosestPointBetweenWaypoints(entityLocation, entityDirectionToWaypoint,
                            entityDistanceToWaypoint, minDistance, ref newDropoff, targetLocation);
                    }

                }

                // for structs, we must store the changes:
                //WaypointPath[i] = currentWaypoint;

            }

            if (closestPointIsNewWaypoint)
            {
                closestPoint = newDropoff;
            }

        }
        */
        private void FindPassengerRendezvousPoint(Vector3 passengerLocation, out int rendezvousWayPointIndex)
        {
            float minDistance = 1000000000;//Common.DistanceSquared(passengerLocation, vehicleLocation);

            FindClosestWaypoint(passengerLocation, 3, out rendezvousWayPointIndex, ref minDistance);

        }

        /*
         
          private Point FindPassengerRendezvousPoint(Point passengerLocation, Point vehicleLocation)
        {
            int minDistance = Common.DistanceSquared(passengerLocation, vehicleLocation);
            int distance;
            int minDistanceIndex = -1;
            for (int i = 0; i < Path.Count - 3; i++) // don't pick up at less than X tiles from destination
            {
                distance = Common.DistanceSquared(Path[i].X, Path[i].Y, passengerLocation);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    minDistanceIndex = i;
                }
                else
                {
                    if (minDistanceIndex == -1)
                    {
                        return vehicleLocation;
                    }
                    else
                    {
                        return new Point(Path[minDistanceIndex].X, Path[minDistanceIndex].Y);
                    }
                }
            }

            return new Point(Path[minDistanceIndex].X, Path[minDistanceIndex].Y);
        }*/

        public override void Terminate()
        {
            // PLEASE remember to always call base.Terminate to ensure that all subgoals get terminated!!!
            base.Terminate();

            entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal; //??!?!?

        }


        /*     private void SmoothPath(List<Vector2> path)
             {
                 if (path.Count > 2)
                 {
                     int edge1 = 0, edge2 = 1;
                     Vector2 edge1Source, edge2Destination, pathDestination;
                     pathDestination = path[path.Count -1];

                     List<Vector2> smoothedPath = new List<Vector2>();

                     do
                     {
                         edge1Source = path[edge1];
                         edge2Destination = path[edge2 + 1];


                    

                         edge2++;
                     }
                     while (edge2Destination != pathDestination);               


                 }
                 else return path;

             }*/

        /*  private bool EdgeIsOnRoadOrPath(Vector2 from, Vector2 to)
          {
              MapManager map = UWGame.SimSide.Instance.map;
              DiscomfortMap = map.GetDiscomfortMap(entity.ProtectionLevel, entity.EntityType.ThreatCategory, entity.ThreatStance);

              if (from.X / (map.tileWidth / 2) == 0f &&
                  from.Y / (map.tileHeight / 2) == 0f)
              {

              }

          }*/

        /// <summary>
        /// Please ensure that nodes are adjacent!
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        /*   private bool EdgeIsOnRoadOrPath(PathFinderNode from, PathFinderNode to)
           {
               // STERAIN: PathFinderNodes are now subtiles...

               if (Common.DistanceSquared(from, to) == 1)
               {
                   MapManager map = UWGame.SimSide.Instance.Map;

                   Common.Direction direction = Common.GetDirection(from, to);
                  // Common.Direction oppositeDirection = Common.Mirror(direction);

                   // STERAIN: NEW:
                   Point fromTile = new Point(from.X / 3, from.Y / 3);
                   Point toTile = new Point(to.X / 3, to.Y / 3);

                   if (map.TileMap[from.X, from.Y].HasPath((int)direction) || 
                       map.TileMap[to.X, to.Y].HasPath((int)direction)) 
                   {
                       return true;
                   }
                   else
                   {
                       return false;
                   }
               }
               else return false;

           }*/


        /// <summary>
        /// Please ensure that nodes are adjacent!
        /// Delete this???
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private bool EdgeIsOnRoadOrPath(PathFinderNode from, PathFinderNode to)
        {
            // STERAIN: PathFinderNodes are now subtiles... but why is this step even necessary? the smoother ought to compare costs, right...?
            /*
            if (Common.DistanceSquared(from, to) == 1)
            {
                MapManager map = UWGame.SimSide.Instance.Map;

                Common.Direction direction = Common.GetDirection(from, to);
                // Common.Direction oppositeDirection = Common.Mirror(direction);

                // STERAIN: NEW:
                Point fromTile = new Point(from.X / 3, from.Y / 3);
                Point toTile = new Point(to.X / 3, to.Y / 3);

                // REDO THIS!!!

                // are paths/roads always "slim"?
                if (map.TileMap[from.X, from.Y].HasPath((int)direction) ||
                    map.TileMap[to.X, to.Y].HasPath((int)direction))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else return false;*/

            return false;

        }

        /*
        private int FindStartOfSegmentNotOnRoads(List<PathFinderNode> path, int startAt)
        {
            for (int i = startAt; i < path.Count - 1; i++)
            {
                if (!EdgeIsOnRoadOrPath(path[i], path[i + 1]))
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindEndOfSegmentNotOnRoads(List<PathFinderNode> path, int startAt)
        {
            for (int i = startAt; i < path.Count - 1; i++)
            {
                if (EdgeIsOnRoadOrPath(path[i], path[i + 1]))
                {
                    return i;
                }
            }

            return -1;
        }*/

        private List<Vector3> SplitPathIntoSegmentsAndSmoothe(List<PathFinderNode> path)
        {
            List<Vector3> waypoints = new List<Vector3>();

            Vector3 start = entity.PlaySiteLocation;

            // add the entity's current location as the starting point.
            // waypoints.Add(new Vector2(entity.Location.X, entity.Location.Y));

            MovementMap moveMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);

            int pathEnd = path.Count - 1;

            int startIndexOfSegmentNotOnRoads = 0, endIndexOfSegmentNotOnRoads = 0;
            endIndexOfSegmentNotOnRoads = pathEnd;


            bool firstLoop = true;

            waypoints.Add(start);

            do
            {
                // see if we can omit this step now?!!!
                // startIndexOfSegmentNotOnRoads = FindStartOfSegmentNotOnRoads(path, endIndexOfSegmentNotOnRoads);

                if (startIndexOfSegmentNotOnRoads != -1)
                {   // we found a segment to smoothe:
                    if (startIndexOfSegmentNotOnRoads > endIndexOfSegmentNotOnRoads)
                    {    // convert part up to start of segment.

                        // first time through? add the starting point:
                        /*  if (firstLoop)
                          {
                              waypoints.Add(start);
                          }*/

                        ConvertPathSegmentWithoutSmoothing(path, endIndexOfSegmentNotOnRoads, startIndexOfSegmentNotOnRoads, waypoints);

                    }

                    // see if we can omit this step now?!!!
                    /* endIndexOfSegmentNotOnRoads = FindEndOfSegmentNotOnRoads(path, startIndexOfSegmentNotOnRoads);
                     if (endIndexOfSegmentNotOnRoads == -1)
                     {
                         // complete the conversion to the end.
                         endIndexOfSegmentNotOnRoads = pathEnd;
                     }*/


                    // do the conversion:
                    if (endIndexOfSegmentNotOnRoads - startIndexOfSegmentNotOnRoads > 0)//1)
                    {
                        List<Vector3> segment = new List<Vector3>();

                        ConvertPathSegmentWithoutSmoothing(path, startIndexOfSegmentNotOnRoads, endIndexOfSegmentNotOnRoads, segment);

                        if (waypoints.Count == 1) //0)
                        {   // the first time through
                            // add the starting point within the same tile as path node 0:
                            segment.Insert(0, start);

                            // add a corresponding path node (for edge cost lookup in the smoothing algorithm):                       
                            path.Insert(0, path[0]);

                            // update counters!!!
                            startIndexOfSegmentNotOnRoads++;
                            pathEnd++;
                            endIndexOfSegmentNotOnRoads++;
                        }

                        if (endLocation.HasValue && endIndexOfSegmentNotOnRoads == pathEnd)
                        {   // add the ending position within the same subtile as the last path node: 
                            segment.Add(endLocation.Value); // MapManager.SubTileToWorldPos(path[path.Count - 1]) + endOffset.Value);
                            // add a corresponding path node (for edge cost lookup in the smoothing algorithm):
                            path.Add(path[path.Count - 1]);
                        }

                        /*
                        if (endOffset != null && endIndexOfSegmentNotOnRoads == pathEnd)
                        {   // add the ending position within the same tile as the last path node: 
                            segment.Add(MapManager.SubTileToWorldPos(path[path.Count - 1]) + endOffset.Value);
                            // add a corresponding path node (for edge cost lookup in the smoothing algorithm):
                            path.Add(path[path.Count - 1]);
                        }
            */

                        if (segment.Count > 2)
                        {
                            SmoothSegmentNotOnRoads(path, segment, moveMap);
                        }

                        waypoints.AddRange(segment);
                    }

                }
                else
                {   // no more segments found to smoothe.
                    // convert the rest.

                    // first time through? add the starting point:
                    /*  if (firstLoop)
                      {
                          waypoints.Add(start);
                      }*/

                    ConvertPathSegmentWithoutSmoothing(path, endIndexOfSegmentNotOnRoads, pathEnd, waypoints);

                    // add the ending position within the same tile as the last path node: 
                    if (endLocation.HasValue)
                    {
                        waypoints.Add(endLocation.Value);
                    }
                    /*   if (endOffset != null)
                       {
                           waypoints.Add(MapManager.SubTileToWorldPos(path[path.Count - 1]) + endOffset.Value);
                       }*/

                    break;
                }

                firstLoop = false;
            }
            while (endIndexOfSegmentNotOnRoads < pathEnd);

            // remove duplicates:
            int i = 0;
            while (i < waypoints.Count - 1)
            {
                if (waypoints[i] == waypoints[i + 1])
                {
                    waypoints.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            return waypoints;

        }

        /*
        private List<Vector3> SplitPathIntoSegmentsAndSmoothe(List<PathFinderNode> path)
        {
            List<Vector3> waypoints = new List<Vector3>();

            Vector3 start = entity.GetLocationWhenInVehicle();

            // add the entity's current location as the starting point.
            // waypoints.Add(new Vector2(entity.Location.X, entity.Location.Y));

            MovementMap moveMap = UWGame.SimSide.Instance.Map.GetMovementMap(entityIntelligence.ProtectionLevel, entity.EntityType.ThreatCategory, entityIntelligence.ThreatStance);

            int pathEnd = path.Count - 1;

            int startIndexOfSegmentNotOnRoads = 0, endIndexOfSegmentNotOnRoads = 0;
            endIndexOfSegmentNotOnRoads = pathEnd; // STERAIN

            

            bool firstLoop = true;

            do
            {
                // STERAIN: see if we can omit this step now?!!!
               // startIndexOfSegmentNotOnRoads = FindStartOfSegmentNotOnRoads(path, endIndexOfSegmentNotOnRoads);

                if (startIndexOfSegmentNotOnRoads != -1)
                {   // we found a segment to smoothe:
                    if (startIndexOfSegmentNotOnRoads > endIndexOfSegmentNotOnRoads)
                    {    // convert part up to start of segment.

                        // first time through? add the starting point:
                        if (firstLoop)
                        {
                            waypoints.Add(start);
                        }

                        ConvertPathSegmentWithoutSmoothing(path, endIndexOfSegmentNotOnRoads, startIndexOfSegmentNotOnRoads, waypoints);

                    }

                  

                    // do the conversion:
                    if (endIndexOfSegmentNotOnRoads - startIndexOfSegmentNotOnRoads > 0)//1)
                    {
                        List<Vector3> segment = new List<Vector3>();

                        ConvertPathSegmentWithoutSmoothing(path, startIndexOfSegmentNotOnRoads, endIndexOfSegmentNotOnRoads, segment);

                        if (waypoints.Count == 0)
                        {   // the first time through
                            // add the starting point within the same tile as path node 0:
                            segment.Insert(0, start);
                            // add a corresponding path node (for edge cost lookup in the smoothing algorithm):
                            // node is a struct, so this adds a copy:
                            path.Insert(0, path[0]);

                            // update counters!!!
                            startIndexOfSegmentNotOnRoads++;
                            pathEnd++;
                            endIndexOfSegmentNotOnRoads++;
                        }

                        if (endLocation.HasValue && endIndexOfSegmentNotOnRoads == pathEnd)
                        {   // add the ending position within the same subtile as the last path node: 
                            segment.Add(endLocation.Value); // MapManager.SubTileToWorldPos(path[path.Count - 1]) + endOffset.Value);
                            // add a corresponding path node (for edge cost lookup in the smoothing algorithm):
                            path.Add(path[path.Count - 1]);
                        }

                     
                        if (segment.Count > 2)
                        {
                            SmoothSegmentNotOnRoads(path, segment, moveMap);
                        }

                        waypoints.AddRange(segment);
                    }

                }
                else
                {   // no more segments found to smoothe.
                    // convert the rest.

                    // first time through? add the starting point:
                    if (firstLoop)
                    {
                        waypoints.Add(start);
                    }

                    ConvertPathSegmentWithoutSmoothing(path, endIndexOfSegmentNotOnRoads, pathEnd, waypoints);

                    // add the ending position within the same tile as the last path node: 
                    if (endLocation.HasValue)
                    {
                        waypoints.Add(endLocation.Value);
                    }
              
                    break;
                }

                firstLoop = false;
            }
            while (endIndexOfSegmentNotOnRoads < pathEnd);

            // remove duplicates:
            int i = 0;
            while (i < waypoints.Count - 1)
            {
                if (waypoints[i] == waypoints[i + 1])
                {
                    waypoints.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            return waypoints;
        }
        */

        /* OLD:

        private List<Vector2> SplitPathIntoSegmentsAndSmoothe(List<PathFinderNode> path)
        {
            List<Vector2> waypoints = new List<Vector2>();

            Vector2 start = new Vector2(entity.GetLocationWhenInVehicle().X, entity.GetLocationWhenInVehicle().Y);
           
            // add the entity's current location as the starting point.
           // waypoints.Add(new Vector2(entity.Location.X, entity.Location.Y));

            MovementMap moveMap = UWGame.SimSide.Instance.Map.GetMovementMap(entityIntelligence.ProtectionLevel, entity.EntityType.ThreatCategory, entityIntelligence.ThreatStance);

            int startIndexOfSegmentNotOnRoads = 0, endIndexOfSegmentNotOnRoads = 0;

            int pathEnd = path.Count - 1;

            bool firstLoop = true;

            do
            {
                // STERAIN: see if we can omit this step now?!!!
                startIndexOfSegmentNotOnRoads = FindStartOfSegmentNotOnRoads(path, endIndexOfSegmentNotOnRoads);

                if (startIndexOfSegmentNotOnRoads != -1)
                {   // we found a segment to smoothe:
                    if (startIndexOfSegmentNotOnRoads > endIndexOfSegmentNotOnRoads)
                    {    // convert part up to start of segment.

                        // first time through? add the starting point:
                        if (firstLoop)
                        {
                            waypoints.Add(start); 
                        }

                        ConvertPathSegmentWithoutSmoothing(path, endIndexOfSegmentNotOnRoads, startIndexOfSegmentNotOnRoads, waypoints);

                    }

                    // STERAIN: see if we can omit this step now?!!!
                    endIndexOfSegmentNotOnRoads = FindEndOfSegmentNotOnRoads(path, startIndexOfSegmentNotOnRoads);
                    if (endIndexOfSegmentNotOnRoads == -1)
                    {
                        // complete the conversion to the end.
                        endIndexOfSegmentNotOnRoads = pathEnd;
                    }

                    // do the conversion:
                    if (endIndexOfSegmentNotOnRoads - startIndexOfSegmentNotOnRoads > 0)//1)
                    {
                        List<Vector2> segment = new List<Vector2>();

                        ConvertPathSegmentWithoutSmoothing(path, startIndexOfSegmentNotOnRoads, endIndexOfSegmentNotOnRoads, segment);
                        
                        if (waypoints.Count == 0)
                        {   // the first time through
                            // add the starting point within the same tile as path node 0:
                            segment.Insert(0, start);
                            // add a corresponding path node (for edge cost lookup in the smoothing algorithm):
                            // node is a struct, so this adds a copy:
                            path.Insert(0, path[0]);
                            
                            // update counters!!!
                            startIndexOfSegmentNotOnRoads++;
                            pathEnd++;
                            endIndexOfSegmentNotOnRoads++;
                        }

                        if (endOffset != null && endIndexOfSegmentNotOnRoads == pathEnd)
                        {   // add the ending position within the same tile as the last path node: 
                            segment.Add(MapManager.TileToWorldPos(path[path.Count - 1]) + endOffset.Value);
                            // add a corresponding path node (for edge cost lookup in the smoothing algorithm):
                            path.Add(path[path.Count - 1]);
                        }

                        if (segment.Count > 2)
                        {
                            SmoothSegmentNotOnRoads(path, segment, moveMap);
                        }

                        waypoints.AddRange(segment);
                    }              

                }
                else
                {   // no more segments found to smoothe.
                    // convert the rest.

                    // first time through? add the starting point:
                    if (firstLoop)
                    {
                        waypoints.Add(start);
                    }

                    ConvertPathSegmentWithoutSmoothing(path, endIndexOfSegmentNotOnRoads, pathEnd, waypoints);

                    // add the ending position within the same tile as the last path node: 
                    if (endOffset != null)
                    {
                        waypoints.Add(MapManager.TileToWorldPos(path[path.Count - 1]) + endOffset.Value);
                    }

                    break;
                }

                firstLoop = false;
            }
            while (endIndexOfSegmentNotOnRoads < pathEnd);

            // remove duplicates:
            int i = 0;
            while (i < waypoints.Count - 1)
            {
                if (waypoints[i] == waypoints[i + 1])
                {
                    waypoints.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            return waypoints;

        }
        */

        /// <summary>
        /// This will only be an approximate list of visited tiles. The vehicle may well skirt around some of them.
        /// </summary>
        /// <returns></returns>
        /*      private List<Point> ComputeListOfVisitedTiles()
              {
                  Waypoint previousWaypoint, currentWaypoint;
                  List<Point> interveningTiles;
                  for (int i = 0; i < WaypointPath.Count - lastValidDropoffPoint; i++)
                  {
                      currentWaypoint = WaypointPath[i];

                      if (previousWaypoint != null && !WaypointsAreAdjacent(previousWaypoint, currentWaypoint))
                      {
                          // test all intervening tiles:
                          interveningTiles = GetTilesTouchedByLine(previousWaypoint, currentWaypoint);
                      }

                  }

              }
              */


        private void SmoothSegmentNotOnRoads(List<PathFinderNode> originalPath, List<Vector3> path, MovementMap moveMap)
        {
            // we use original path to easily look up the cost of edges.
            // this is the counter for the original path:
            int edge2Counter = 1;
            AI.Pathfinding.PathFinderNode edge2CounterFollowingNode;

            int edge1 = 0, edge2 = 1;
            Vector3 edge1Source, edge2Destination, pathDestination;
            pathDestination = path[path.Count - 1];

            edge2Destination = path[edge2 + 1];

            List<Vector3> pathCopy = null;

            if (moveMap.IDName.Contains("Human") && path.Count < 30 && Common.DistanceOctile(pathDestination, path[0]) * 2f < path.Count * 16)
            {
                // short & curly path?
                pathCopy = new List<Vector3>(path);
            }

            // MapManager.SubtileValue[][] map = moveMap.Layers[entity.GetTransportType()].Values;
            SubtileLayers map = moveMap.Layers[entity.GetTransportType()];

            // the cost to compare with should be the lowest cost of the two edges:
            byte costToCompareWith;

            byte edge2Cost;

            // keep updating the lowest cost as the list of nodes is traversed.
            AI.Pathfinding.PathFinderNode edge1Destination = originalPath[edge1 + 1];

            byte collapsedPartLowestCost = MapManager.GetCost(map.GetValue(edge1Destination.AbsoluteX, edge1Destination.AbsoluteY)); // OLD: moveMap.GetBaseCostOfEdge(transport, originalPath[edge1], originalPath[edge1 + 1]);

            do
            {
                // compute the minimum cost that is used to evaluate the collapsed path
                edge2CounterFollowingNode = originalPath[edge2Counter + 1];
                edge2Cost = MapManager.GetCost(map.GetValue(edge2CounterFollowingNode.AbsoluteX, edge2CounterFollowingNode.AbsoluteY)); //OLD: moveMap.GetBaseCostOfEdge(transport, originalPath[edge2Counter], originalPath[edge2Counter + 1]);

                // should be maximum, right?
                costToCompareWith = Math.Max(collapsedPartLowestCost, edge2Cost); //Math.Min(collapsedPartLowestCost, edge2Cost);

                edge1Source = path[edge1];
                edge2Destination = path[edge2 + 1];

                // test the proposed edge:
                if (EdgeHasLowerOrEqualCost(edge1Source, edge2Destination, costToCompareWith, map, ref collapsedPartLowestCost))
                {
                    path.RemoveAt(edge2);
                    // this causes edge1 and edge2 to point at the next edge...

                }
                else
                {
                    edge1 = edge2;
                    edge2++;
                }

                edge2Counter++;

            }
            while (edge2Destination != pathDestination);

            if (pathCopy != null && pathCopy.Count < 30
                && path.Count > 6 && Common.DistanceOctile(pathDestination, path[0]) * 2f < path.Count * 16)
            {

            }
        }


        private bool EdgeHasLowerOrEqualCost(Vector3 from, Vector3 to, byte highestCost, SubtileLayers map, ref byte newLowestCostAlongLine)
        {

            SurfaceType.TransportType transport = entity.GetTransportType();
            // get the tiles
            List<Point> subtiles = MapManager.GetSubtilesTouchedByLine(from.ToVector2(), to.ToVector2());
            Point previousSubtile = subtiles[0];

            byte currentCost, costToTest;
            // Common.Direction tileDir;
            Point subtile;
            for (int i = 1; i < subtiles.Count; i++)
            {
                subtile = subtiles[i];
                // we don't test edges within the same tile:
                if (subtile != previousSubtile)
                {
                    // go from tile to tile, examine all edges to see that they are: 
                    // not blocked
                    // not higher cost (terrain + discomfort)
                    //tileDir = Common.GetDirection(previousSubtile, subtile);

                    if (!The.Map.SubtileIsOnMap(subtile))
                        continue;


                    costToTest = MapManager.GetCost(map.GetValue(subtile));
                    if (costToTest == 0)
                    {
                        return false;
                    }
                    else
                    {
                        // STERAIN:
                        currentCost = costToTest; // moveMap.GetBaseCostOfEdge(transport, previousSubtile, subtiles[i]);
                        if (currentCost > highestCost)
                        {
                            return false;
                        }
                        else
                        { // if equal or lower:
                            newLowestCostAlongLine = currentCost;
                        }
                    }
                }

                previousSubtile = subtile; // subtiles[i];
            }

            return true;

        }

        private void ConvertPathSegmentWithoutSmoothing(List<PathFinderNode> path, int startAt, int endAt, List<Vector3> appendToList)
        {
            for (int i = startAt; i <= endAt; i++)
            {
                appendToList.Add(MapManager.SubTileToWorldPos(path[i]));
            }
        }

        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.Path = sn.DoList(Path);
            this.WaypointPath = sn.DoList(WaypointPath);
            snapshotParent = sn.SnapshotID<Goal, GoalID>(parentGoal);
            this.rejectedPassengerDestinations = sn.DoDictionary(rejectedPassengerDestinations);
            this.MovingOutOfHarmsWay = sn.DoBool(MovingOutOfHarmsWay);
            this.endLocation = sn.DoVector3Nullable(endLocation);
            this.PermittedDistanceSquaredToDestination = sn.DoFloat(PermittedDistanceSquaredToDestination);

            sn.Ignore(numberSpacing);
            sn.Ignore(GroupMoveActivity);
            sn.Ignore(parentGoal);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (Path != null)
            {
                foreach (var item in Path)
                {
                    item.LoadPostProcess(sn);
                }
            }

            if (WaypointPath != null)
            {
                foreach (var item in WaypointPath)
                {
                    item.LoadPostProcess(sn);
                }
            }

            if (snapshotParent.HasValue)
            {
                parentGoal = (GoalMoveToPosition)LookUpGoals.FindByID(snapshotParent.Value);
            }
        }

        #endregion
    }

    //  public struct Waypoint//: ISnapshot
    public class Waypoint : ISnapshot // changed to a class for easier snapshotting..
    {
        public Vector3 Location;
        public int Number;

        public bool IsLastWaypoint;

        /// <summary>
        /// These values are cached computations from the hitch hike code.
        /// </summary>
        /*    public bool? nextWaypointIsAdjacent;
            public Vector2? DirectionToNextWaypoint;
            public float? DistanceToNextWaypoint;
            */


        public Waypoint(int number, Vector3 location, bool isLastWaypoint = false)
        {
            this.Location = location;
            this.Number = number;
            this.IsLastWaypoint = isLastWaypoint;


        }

        public Waypoint()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Location = sn.DoVector3(Location);
            this.Number = sn.DoInt32(Number);
            this.IsLastWaypoint = sn.DoBool(IsLastWaypoint);

            return this;
        }


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }

    struct FromTo
    {
        public Vector3 From; //Point From;
        public Vector3 To; //Point To;

        public FromTo(Vector3 f, Vector3 to)
        {
            this.From = f;
            this.To = to;
        }

    }
}
