using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.AI.Pathfinding;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using Xclna.Xna.Animation;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.AllGameData.Constants;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    class GoalTraverseEdgeBetweenWaypointsAtomic: Goal
    {
        //private PathFinderNode toNode;
        public Vector3 To;
        private Point toTile;
        public Vector3 WaypointRay;

        /// <summary>
        /// used by vehicles to facilitate smoother turning
        /// </summary>
        private Vector2? lookAheadDirection;
   //     private Vector2? lookAheadDirection;

        private Vector2? normalizedVectorToWaypoint = null;

        public AI.Activities.GroupMoveActivity GroupMoveActivity;

        private float speedModifier = 1f;

        const double maxPeriodInSeconds = 0.2;
        double goalProgress = 0.0;

        public bool MovingOutOfHarmsWay = false;

        private static Queue<GoalTraverseEdgeBetweenWaypointsAtomic> freeGoals = new Queue<GoalTraverseEdgeBetweenWaypointsAtomic>();

        float permittedDistanceSquared;

        //   public float DistanceBetweenWaypoints;

        /// <summary>
        /// We don't want the entity to slide along obstacles forever.
        /// </summary>
        public double TimeSpentSliding = 0;

        public Common.Direction detectedDirection;
        public bool movesAlong8Dir = false;
        public GoalTraverseEdgeBetweenWaypoints.TileProgress tileProgress = GoalTraverseEdgeBetweenWaypoints.TileProgress.NoDescription;

        //  private float? terrainMoveFactor = null;


        /// <summary>
        /// Can this be used for decelerating on arrival?
        /// </summary>
        private bool isLastNode;

        /// <summary>
        /// the velocity we are accelerating towards
        /// </summary>
        // public float Velocity;



        /// <summary>
        /// used to detect movement along the 8 directions.
        /// </summary>
        private Vector2? NormalizedVectorToWaypoint
        {
            get
            {
                if (normalizedVectorToWaypoint == null)
                {
                    Vector2 norm = new Vector2(To.X - entity.PlaySiteLocation.X, To.Y - entity.PlaySiteLocation.Y);
                    norm.Normalize();
                    normalizedVectorToWaypoint = norm;

                }
                return normalizedVectorToWaypoint.Value;
            }

            set
            {
                normalizedVectorToWaypoint = value;
            }

        }

        /*    private GoalTraverseEdgeBetweenWaypointsAtomic(Entity owner, Vector2 to, bool isLastEdge, List<Vector2> pathPointsForSpline)
                : base(owner)
            {
                To = to;
       
                isLastNode = isLastEdge;
                this.pathPointsForSpline = pathPointsForSpline;
            }*/

     

        public GoalTraverseEdgeBetweenWaypointsAtomic() { }

        private void Init(Entity owner, Vector3 to, Vector2? lookAhead, Point toTile, Vector3 waypointRay, double timeSpentSliding, bool isLastEdge, bool movingOutOfHarmsWay,         
            Common.Direction detectedDirection,
            bool movesAlong8Dir,
            GoalTraverseEdgeBetweenWaypoints.TileProgress tileProgress,
            /*float velocity,*/
            AI.Activities.GroupMoveActivity groupMoveActivity,
            float permittedDistanceSquared)
        {
           
            To = to;
            this.lookAheadDirection = lookAhead;
            this.toTile = toTile;
            this.WaypointRay = waypointRay;

            this.TimeSpentSliding = timeSpentSliding;
            this.permittedDistanceSquared = permittedDistanceSquared;

            isLastNode = isLastEdge;

            MovingOutOfHarmsWay = movingOutOfHarmsWay;
            goalProgress = 0.0;
            
           
            this.detectedDirection = detectedDirection;
            this.movesAlong8Dir = movesAlong8Dir;
            this.tileProgress = tileProgress;
          
            this.GroupMoveActivity = groupMoveActivity;

          /*  steerableFrontWheels = null;
            if (owner.DrivingVehicle != null && owner.DrivingVehicle.Renderable != null)
            {
                owner.DrivingVehicle.Renderable.Find(out steerableFrontWheels);
            }
            else if (owner.Renderable != null)
            {
                owner.Renderable.Find(out steerableFrontWheels);
            }*/

            base.Init(owner);
        }


        public override bool IsSame(Job job)
        {
            return false;
        }

        /// <summary>
        /// necessary to avoid goal hanging on to Entity instances and creating leaks...
        /// </summary>
        public static void ClearPool()
        {
            freeGoals.Clear();
        }

        public static GoalTraverseEdgeBetweenWaypointsAtomic GetGoal(Entity owner, Vector3 to, Vector2? lookAhead, Point toTile, Vector3 waypointRay, double timeSpentSliding, bool isLastEdge, bool movingOutOfHarmsWay,
           // float distanceBetweenWaypoints,
            Common.Direction detectedDirection,
            bool movesAlong8Dir,
            GoalTraverseEdgeBetweenWaypoints.TileProgress tileProgress,
            AI.Activities.GroupMoveActivity groupMoveActivity,
            float permittedDistanceSquared)
        {
            if (freeGoals.Count == 0)
            {   // add some fresh goals, we've run out:
                for (int i = 0; i < 30; i++)
                {
                    freeGoals.Enqueue(new GoalTraverseEdgeBetweenWaypointsAtomic());
                }
            }
            GoalTraverseEdgeBetweenWaypointsAtomic goal = freeGoals.Dequeue();
            goal.Init(owner, to, lookAhead, toTile, waypointRay, timeSpentSliding, isLastEdge, movingOutOfHarmsWay,
               // distanceBetweenWaypoints, 
                detectedDirection, movesAlong8Dir, tileProgress, /*velocity,*/ groupMoveActivity, permittedDistanceSquared);
            return goal;
        }

        public override void RetireGoal()
        {
            freeGoals.Enqueue(this);
        }

        public override float GetExertionLevel()
        {
            PhysicalWork workConstants = GameData.Instance.Constants.PhysicalWork;

            if (entity.DrivingVehicle == null)
            {
                
                if (entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.Run)
                {
                    return workConstants.Running;
                }
                else 
                {
                    if (entity.AgentStorage != null)
                    {
                        float carryingLoad = entity.AgentStorage.ItemStorage.TotalStored / (entity.AgentStorage.ItemStorage.TotalCapacity);

                        if (carryingLoad > 0.9f)
                        {
                            return workConstants.HaulingHeavyLoad;
                        }
                        else if (carryingLoad > 0.3f)
                        {
                            return workConstants.HaulingMediumLoad;
                        }
                        else if (carryingLoad > 0.1f)
                        {
                            return workConstants.HaulingLightLoad;
                        }
                        else
                        {
                            return workConstants.Walking;
                        }
                    }
                    else
                    {
                        return workConstants.Walking;
                    }
                }
            }
            else return workConstants.Driving; // driving 
        }


        public override Goal.StealthFactor GetStealthFactor()
        {
            if (entity.DrivingVehicle == null)
            {
                if (entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.Run)
                {
                    return StealthFactor.Bad;
                }
                else
                {
                    return StealthFactor.NotGood;
                }
            }
            else return StealthFactor.ExtremelyBad; // driving 
        }

        public override Goal.DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            if (!requiresExamineAction)
            {
                return DetectionFactor.DetectGood;
            }
            else return DetectionFactor.CannotDetect;
        }

        public override Goal.DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
        {
            if (!requiresExamineAction)
            {
                return DetectionFactor.DetectSome;
            }
            else return DetectionFactor.CannotDetect;
        }

        protected override void Activate()
        {
            if (GoalTraverseEdgeBetweenWaypoints.IsAtWaypoint(entity.PlaySiteLocation, WaypointRay, To, permittedDistanceSquared))
            {
                Status = Status.Completed;
                return;
            }   
            else
            {
                // go on
            
               // hasEverBeenOnUnblockedTile = false;// to allow for pathing out of blocked tiles

                hasEverBeenOnUnblockedTile = !The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[entity.GetTransportType()], MapManager.WorldPosToSubtile(entity.PlaySiteLocation));

                        //!MapManager.IsBlocked(The.Map.TerrainCosts[entity.GetTransportType()][nextSubtile.X][nextSubtile.Y])


                Status = Status.Active;
               
                GoalTraverseEdgeBetweenWaypoints.SelectMoveSpeed(entity);
               

                if (GroupMoveActivity != null)
                {
                    if (GroupMoveActivity.IsFollower(entity))
                    {
                        speedModifier = GroupMoveActivity.ComputeSpeedModifier(entity);
                    }
                    else if (GroupMoveActivity.IsLeader(entity))
                    {
                        GroupMoveActivity.ComputeLeadersRotationMatrix();
                    }
                }
            }
  
        }

      /* OLD:
       * private void TestProgress(Vector3 location, ref Vector3 moveVector, out bool changeTilePosition)
        {
            NormalizedVectorToWaypoint = null;
            changeTilePosition = false;

            Vector3 newLocation = location + moveVector; 
           
            switch (tileProgress)
            {
                case GoalTraverseEdgeBetweenWaypoints.TileProgress.NoDescription:
                    if (!TestForMovingPastTileBoundary(newLocation, ref moveVector, out changeTilePosition))
                    {
                        TestForEnteringCenterOfTile(newLocation);
                    }
                  
                    break;
                case GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromEdge:
                    if (!TestForEnteringCenterOfTile(newLocation))
                    {
                        TestForMovingPastTileBoundary(newLocation, ref moveVector, out changeTilePosition);
                    }
                   

                    break;
                case GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromCenter:
                    TestForMovingPastTileBoundary(newLocation, ref moveVector, out changeTilePosition);

                    break;
            }
        }*/

      
        private void TestProgress(Vector3 location, ref Vector3 moveVector, Entity vehicleForLeavingTrails) //, /*!!!*/ out bool changeSubtilePosition)
        {
            NormalizedVectorToWaypoint = null;
            bool changeTilePosition = false;

            Vector3 newLocation = location + moveVector;

            if (!The.Map.WorldLocationIsOnMap(newLocation.ToVector2()))
            {
                // clamp
                newLocation = The.Map.ClampWorldPosition(newLocation);
                moveVector = newLocation - location;
            }

            // New - split direction detection (tile boundaries) from collision detection (subtiles)
            // this can fail...
            TestForMovingPastSubtileBoundary(newLocation, ref moveVector, vehicleForLeavingTrails);

            if (Status == Goals.Status.Failed)
            {
                return;
            }

            // recalculate??? no, because we want to make edge trails even if we hit a blocked subtile.
           // newLocation = location + moveVector;

            switch (tileProgress)
            {
                case GoalTraverseEdgeBetweenWaypoints.TileProgress.NoDescription:
                    if (!TestForMovingPastTileBoundary(newLocation, out changeTilePosition, vehicleForLeavingTrails))
                    {
                        TestForEnteringCenterOfTile(newLocation, vehicleForLeavingTrails);
                    }

                    break;
                case GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromEdge:
                    if (!TestForEnteringCenterOfTile(newLocation, vehicleForLeavingTrails))
                    {
                        TestForMovingPastTileBoundary(newLocation, out changeTilePosition, vehicleForLeavingTrails);
                    }


                    break;
                case GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromCenter:
                    TestForMovingPastTileBoundary(newLocation, out changeTilePosition, vehicleForLeavingTrails);

                    break;
            }
        }

        /// <summary>
        /// returns true if we are moving into a new tile!
        /// </summary>
        /// <param name="newLocation"></param>
        /// <param name="changeTilePosition"></param>
        /// <returns></returns>
        private bool TestForMovingPastTileBoundary(Vector3 newLocation, out bool changeTilePosition, Entity vehicle)
        {
            changeTilePosition = false;
            // test the new position:
            Point nextTile = MapManager.WorldPosToTile(newLocation); //MapManager.WorldPosToTile(entity.Location);
            if (nextTile != entity.MapPosition)
            {
                Common.Direction oldDirection = detectedDirection;
                bool oldMovesAlong8Dir = movesAlong8Dir;


                if (The.Map.DetectMovementAlong8Dir(newLocation,
                            NormalizedVectorToWaypoint.Value, ref detectedDirection))
                {
                    movesAlong8Dir = true;
                }
                else
                {
                    movesAlong8Dir = false;
                }

                if (oldMovesAlong8Dir)
                {
                    if (!movesAlong8Dir || oldDirection != detectedDirection)
                    { // leave trails on old tile position if we have moved a complete segment:
                        GoalTraverseEdgeBetweenWaypoints.LeaveTrails(entity, oldDirection, entity.MapPosition.Value, vehicle);
                    }
                }

                changeTilePosition = true;

                return true;
            }

            return false;

        }

        private bool hasEverBeenOnUnblockedTile = false;
        /// <summary>
        /// returns true if we have entered a new subtile (also when the new subtile was blocked.)
        /// </summary>
        /// <param name="newLocation"></param>
        /// <param name="moveVector"></param>
        /// <returns></returns>
        private bool TestForMovingPastSubtileBoundary(Vector3 newLocation, ref Vector3 moveVector, Entity vehicle) //, out bool changeTilePosition)
        {
           
            // test the new position:
            Point nextSubtile = MapManager.WorldPosToSubtile(newLocation); //MapManager.WorldPosToTile(entity.Location);
            Point currentSubtile = MapManager.WorldPosToSubtile(entity.PlaySiteLocation);
            if (nextSubtile != currentSubtile) //entity.MapPosition)
            {
                // we have reached the edge of the tile

                // STERAIN: Detect tile change too???
                tileProgress = GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromEdge;

                Common.Direction oldDirection = detectedDirection;
                bool oldMovesAlong8Dir = movesAlong8Dir;
                
                //It seems as though bounds need to be checked here /Finn               
                if (!The.Map.SubtileIsOnMap(nextSubtile)) //  The.Map.TerrainCosts[entity.GetTransportType()].Length <= nextSubtile.X)
                {                 
                    Status = Status.Failed;
                    // if following a leader, we want a new waypoint as this one is inaccessible: 
                    entityIntelligence.GroupMoveAssignedWaypoint = null;
                    TimeSpentSliding = 0;

                    return true;
                }

                // Test if the next subtile is passable!                
                if (MapManager.IsBlocked(The.Map.TerrainCosts[entity.GetTransportType()].GetValue(nextSubtile))
                    && hasEverBeenOnUnblockedTile) // UWGame.SimSide.Instance.Map.GetTerrainCost(entity.GetTransportType(), entity.MapPosition, nextSubtile) == 0)
                {
                    // blocked!

                    float moveSpeed;


                    if (vehicle != null)
                    {
                        moveSpeed = vehicle.Locomotor.MoveSpeed;
                    }
                    else
                    {
                        moveSpeed = entity.Locomotor.MoveSpeed;
                    }


                    // if waypoint destination is inaccessible, or if we have been sliding for too long,
                    // abort:
                    if (The.Map.TileIsCompletelyBlocked(The.Map.TerrainCosts[entity.GetTransportType()], toTile))
                    {
                        Status = Status.Failed;
                        // if following a leader, we want a new waypoint as this one is inaccessible: 
                        entityIntelligence.GroupMoveAssignedWaypoint = null;
                        TimeSpentSliding = 0;

                        return true;
                    }
                    else if (TimeSpentSliding > (GameData.Instance.AIConstants.MaxTimeForSliding / (moveSpeed * 0.1f)))
                    {   // don't slide for a whole second if we are going fast. 10 is the average movespeed of a person. 50 for a bike.
                        // if following a leader, we want to keep the waypoint! Repath! 
                        Status = Status.Failed;
                        TimeSpentSliding = 0;
                        return true;
                    }
                    else
                    {   // we should be able to reach the waypoint. 
                        // Slide along the obstacle by modifying the move vector:
                        Common.Direction dir = Common.GetDirection(currentSubtile, nextSubtile);

                        switch (dir)
                        {
                            case Common.Direction.North:
                            case Common.Direction.South:
                                if (moveVector.X == 0f)
                                { // never run straight into an obstacle:
                                    Status = Status.Failed;
                                    return true;
                                }
                                else
                                {
                                    moveVector = SlideEastOrWest(moveVector);

                                  
                                    return true;
                                }
                            case Common.Direction.West:
                            case Common.Direction.East:
                                if (moveVector.Y == 0f)
                                {
                                    Status = Status.Failed;
                                    return true;
                                }
                                else
                                {
                                    moveVector = SlideNorthOrSouth(moveVector);

                                   
                                    return true;
                                }
                            default:
                                Status = Status.Failed;
                                return true;
                                                               
                        }
                    }
                }
                else // not blocked
                {
                    /* // TODO: enable 8 dir path impressions on the map again
                     * 
                    if (UWGame.SimSide.Instance.Map.DetectMovementAlong8Dir(newLocation,
                        NormalizedVectorToWaypoint.Value, ref detectedDirection))
                    {
                        movesAlong8Dir = true;
                    }
                    else
                    {
                        movesAlong8Dir = false;
                    }

                    if (oldMovesAlong8Dir)
                    {
                        if (!movesAlong8Dir || oldDirection != detectedDirection)
                        { // leave trails on old tile position if we have moved a complete segment:
                            GoalTraverseEdgeBetweenWaypoints.LeaveTrails(entity, oldDirection, entity.MapPosition);
                        }
                    }
                    */
                   
                    // sample the new subtile terrain and notify the locomotor:
                    if (vehicle != null)
                    {
                        vehicle.Locomotor.SetTerrainModifier(nextSubtile);
                    }
                    else
                    {
                        entity.Locomotor.SetTerrainModifier(nextSubtile);
                    }

                    //Velocity = GoalTraverseEdgeBetweenWaypoints.SetVelocityFromTerrain(entity, nextSubtile, vehicle); 

                    TimeSpentSliding = 0;

                    hasEverBeenOnUnblockedTile = true;

                    return true;
                }
            }

            TimeSpentSliding = 0;

            return false;

        }

        /* OLD:
        private bool TestForMovingPastTileBoundary(Vector3 newLocation, ref Vector3 moveVector, out bool changeTilePosition)
        {
            // TODO: Change this code to test for subtile boundaries instead
            // don't detect movement direction, sample the subtile instead.

            changeTilePosition = false;
            // test the new position:
            Point nextTile = MapManager.WorldPosToTile(newLocation); //MapManager.WorldPosToTile(entity.Location);
            if (nextTile != entity.MapPosition)
            {
                // we have reached the edge of the tile
                tileProgress = GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromEdge;

                Common.Direction oldDirection = detectedDirection;
                bool oldMovesAlong8Dir = movesAlong8Dir;     
                              
                // Test if the next tile is passable!                
                if (UWGame.SimSide.Instance.Map.GetTerrainCost(entity.GetTransportType(), entity.MapPosition, nextTile) == 0)                  
                {
                    // if waypoint destination is inaccessible, or if we have been sliding for too long,
                // abort:
                    if (UWGame.SimSide.Instance.Map.TileIsBlocked(UWGame.SimSide.Instance.Map.TerrainCosts, entity.GetTransportType(), toTile))
                    {
                        Status = Status.Failed;
                        // if following a leader, we want a new waypoint as this one is inaccessible: 
                        entityIntelligence.GroupMoveAssignedWaypoint = null;
                        TimeSpentSliding = 0;
                        
                        return true;
                    }
                    else if (TimeSpentSliding > (AIConstants.MaxTimeForSliding / (entity.GetMoveSpeed() / 10f)))
                    {   // don't slide for a whole second if we are going fast. 10 is the average movespeed of a person. 50 for a bike.
                        // if following a leader, we want to keep the waypoint! Repath! 
                        Status = Status.Failed;
                        TimeSpentSliding = 0;
                        return true;
                    }
                    else
                    {   // we should be able to reach the waypoint. 
                        // Slide along the obstacle by modifying the move vector:
                        Common.Direction dir = Common.GetDirection(entity.MapPosition, nextTile);
                        
                        switch (dir)
                        {
                            case Common.Direction.North:
                            case Common.Direction.South:                                
                                if (moveVector.X == 0f)
                                { // never run straight into an obstacle:
                                    Status = Status.Failed;
                                    return true;
                                }
                                else
                                {
                                    moveVector = SlideEastOrWest(moveVector);
                                    Corner corner = DetectIsAtCornerOfTile(entity.GetLocationWhenInVehicle());
                                    if (corner != Corner.None)
                                    { // the Corner Case - we may want to slide backwards to get around this corner:
                                        if (ReverseSlideDirection(dir, corner, moveVector))
                                        {
                                            moveVector.X *= -1f;
                                        }
                                    }
                                    
                                    return true;
                                }
                            case Common.Direction.West:
                            case Common.Direction.East:                                
                                if (moveVector.Y == 0f)
                                {
                                    Status = Status.Failed;
                                    return true;
                                }
                                else
                                {
                                    moveVector = SlideNorthOrSouth(moveVector);

                                    Corner corner = DetectIsAtCornerOfTile(entity.GetLocationWhenInVehicle());
                                    if (corner != Corner.None)
                                    { // the Corner Case - we may want to slide backwards to get around this corner:
                                        if (ReverseSlideDirection(dir, corner, moveVector))
                                        {
                                            moveVector.Y *= -1f;
                                        }
                                    }
                                    return true;
                                }                         
                            case Common.Direction.NorthEast:
                                // see if we can slide east or north
                                if (CanSlideAcrossTileBoundary(1, 0))
                                {
                                    moveVector = SlideEastOrWest(moveVector);
                                    changeTilePosition = true; // we will be crossing the tile boundary
                                    return true;                                    
                                }
                                else if (CanSlideAcrossTileBoundary(0, -1))
                                {
                                    moveVector = SlideNorthOrSouth(moveVector);
                                    changeTilePosition = true; // we will be crossing the tile boundary
                                    return true;     
                                }
                                else
                                {   // caught in a blocked corner.
                                    Status = Status.Failed;
                                    return true;
                                }
                            case Common.Direction.NorthWest:
                                // see if we can slide west or north
                                if (CanSlideAcrossTileBoundary(-1, 0))
                                {
                                    moveVector = SlideEastOrWest(moveVector);
                                    changeTilePosition = true; // we will be crossing the tile boundary
                                    return true;
                                }
                                else if (CanSlideAcrossTileBoundary(0, -1))
                                {
                                    moveVector = SlideNorthOrSouth(moveVector);
                                    changeTilePosition = true; // we will be crossing the tile boundary
                                    return true;
                                }
                                else
                                {   // caught in a blocked corner.
                                    Status = Status.Failed;
                                    return true;
                                }
                            case Common.Direction.SouthEast:
                                // see if we can slide east or south
                                if (CanSlideAcrossTileBoundary(1, 0))
                                {
                                    moveVector = SlideEastOrWest(moveVector);
                                    changeTilePosition = true; // we will be crossing the tile boundary
                                    return true;
                                }
                                else if (CanSlideAcrossTileBoundary(0, 1))
                                {
                                    moveVector = SlideNorthOrSouth(moveVector);
                                    changeTilePosition = true; // we will be crossing the tile boundary
                                    return true;
                                }
                                else
                                {   // caught in a blocked corner.
                                    Status = Status.Failed;
                                    return true;
                                }
                            case Common.Direction.SouthWest:
                                // see if we can slide west or south
                                if (CanSlideAcrossTileBoundary(-1, 0))
                                {
                                    moveVector = SlideEastOrWest(moveVector);
                                    changeTilePosition = true; // we will be crossing the tile boundary
                                    return true;
                                }
                                else if (CanSlideAcrossTileBoundary(0, 1))
                                {
                                    moveVector = SlideNorthOrSouth(moveVector);
                                    changeTilePosition = true; // we will be crossing the tile boundary
                                    return true;
                                }
                                else
                                {   // caught in a blocked corner.
                                    Status = Status.Failed;
                                    return true;
                                }
                        }
                    }
                }
                else
                {
                   
                    if (UWGame.SimSide.Instance.Map.DetectMovementAlong8Dir(newLocation,
                        NormalizedVectorToWaypoint.Value,
                        //entity.NormalizedMoveDir, 
                        ref detectedDirection))
                    {
                        movesAlong8Dir = true;
                    }
                    else
                    {
                        movesAlong8Dir = false;
                    }

                    if (oldMovesAlong8Dir)
                    {
                        if (!movesAlong8Dir || oldDirection != detectedDirection)
                        { // leave trails on old tile position if we have moved a complete segment:
                            GoalTraverseEdgeBetweenWaypoints.LeaveTrails(entity, oldDirection, entity.MapPosition);
                        }
                    }

                    changeTilePosition = true;
                    //entity.SetNewMapPosition(MapManager.WorldPosToTile(newLocation));

                    // sample the new tile terrain:
                    velocity = GoalTraverseEdgeBetweenWaypoints.SetVelocity(entity, entity.MapPosition, movesAlong8Dir, detectedDirection);

                    TimeSpentSliding = 0;
                    return true;
                }
            }

            TimeSpentSliding = 0;

            return false;

        }*/

        /*
        private bool ReverseSlideDirection(Common.Direction boundary, Corner corner, Vector3 moveVector)
        {
            switch(boundary){                
                case Common.Direction.South:                                
                    if (moveVector.X < 0f && corner == Corner.BottomRight)
                    { 
                        Point eastTile = Common.AddPoints(entity.MapPosition, new Point(1, 0));
                        Point southEastTile = Common.AddPoints(entity.MapPosition, new Point(1, 1));
                        Point southTile = Common.AddPoints(entity.MapPosition, new Point(0, 1));

                        return CanSlideAcrossTileBoundary(entity.MapPosition, eastTile) &&
                            CanSlideAcrossTileBoundary(eastTile, southEastTile) &&
                            CanSlideAcrossTileBoundary(southEastTile, southTile);
                        
                    }
                    else if (moveVector.X > 0f && corner == Corner.BottomLeft)
                    {
                        Point westTile = Common.AddPoints(entity.MapPosition, new Point(-1, 0));
                        Point southWestTile = Common.AddPoints(entity.MapPosition, new Point(-1, 1));
                        Point southTile = Common.AddPoints(entity.MapPosition, new Point(0, 1));

                        return CanSlideAcrossTileBoundary(entity.MapPosition, westTile) &&
                            CanSlideAcrossTileBoundary(westTile, southWestTile) &&
                            CanSlideAcrossTileBoundary(southWestTile, southTile);
                    }
                    break;

                case Common.Direction.North:
                    if (moveVector.X < 0f && corner == Corner.TopRight)
                    {
                        Point eastTile = Common.AddPoints(entity.MapPosition, new Point(1, 0));
                        Point northEastTile = Common.AddPoints(entity.MapPosition, new Point(1, -1));
                        Point northTile = Common.AddPoints(entity.MapPosition, new Point(0, -1));

                        return CanSlideAcrossTileBoundary(entity.MapPosition, eastTile) &&
                            CanSlideAcrossTileBoundary(eastTile, northEastTile) &&
                            CanSlideAcrossTileBoundary(northEastTile, northTile);

                    }
                    else if (moveVector.X > 0f && corner == Corner.TopLeft)
                    {
                        Point westTile = Common.AddPoints(entity.MapPosition, new Point(-1, 0));
                        Point northWestTile = Common.AddPoints(entity.MapPosition, new Point(-1, -1));
                        Point northTile = Common.AddPoints(entity.MapPosition, new Point(0, -1));

                        return CanSlideAcrossTileBoundary(entity.MapPosition, westTile) &&
                            CanSlideAcrossTileBoundary(westTile, northWestTile) &&
                            CanSlideAcrossTileBoundary(northWestTile, northTile);
                    }
                    break;

                case Common.Direction.West:
                    if (moveVector.Y < 0f && corner == Corner.BottomLeft)
                    {
                        Point westTile = Common.AddPoints(entity.MapPosition, new Point(-1, 0));
                        Point southWestTile = Common.AddPoints(entity.MapPosition, new Point(-1, 1));
                        Point southTile = Common.AddPoints(entity.MapPosition, new Point(0, 1));

                        return CanSlideAcrossTileBoundary(entity.MapPosition, southTile) &&
                            CanSlideAcrossTileBoundary(southTile, southWestTile) &&
                            CanSlideAcrossTileBoundary(southWestTile, westTile);

                    }
                    else if (moveVector.Y > 0f && corner == Corner.TopLeft)
                    {
                        Point westTile = Common.AddPoints(entity.MapPosition, new Point(-1, 0));
                        Point northWestTile = Common.AddPoints(entity.MapPosition, new Point(-1, -1));
                        Point northTile = Common.AddPoints(entity.MapPosition, new Point(0, -1));

                        return CanSlideAcrossTileBoundary(entity.MapPosition, northTile) &&
                            CanSlideAcrossTileBoundary(northTile, northWestTile) &&
                            CanSlideAcrossTileBoundary(northWestTile, westTile);
                    }
                    break;

                case Common.Direction.East:
                    if (moveVector.Y < 0f && corner == Corner.BottomRight)
                    {
                        Point eastTile = Common.AddPoints(entity.MapPosition, new Point(1, 0));
                        Point southEastTile = Common.AddPoints(entity.MapPosition, new Point(1, 1));
                        Point southTile = Common.AddPoints(entity.MapPosition, new Point(0, 1));

                        return CanSlideAcrossTileBoundary(entity.MapPosition, southTile) &&
                            CanSlideAcrossTileBoundary(southTile, southEastTile) &&
                            CanSlideAcrossTileBoundary(southEastTile, eastTile);
                    }
                    else if (moveVector.Y > 0f && corner == Corner.TopRight)
                    {
                        Point eastTile = Common.AddPoints(entity.MapPosition, new Point(1, 0));
                        Point northEastTile = Common.AddPoints(entity.MapPosition, new Point(1, -1));
                        Point northTile = Common.AddPoints(entity.MapPosition, new Point(0, -1));

                        return CanSlideAcrossTileBoundary(entity.MapPosition, northTile) &&
                            CanSlideAcrossTileBoundary(northTile, northEastTile) &&
                            CanSlideAcrossTileBoundary(northEastTile, eastTile);


                    }
                    break;
            }
            return false;
        }
        */

        /*
        private enum Corner { TopLeft, TopRight, BottomRight, BottomLeft, None }
        private Corner DetectIsAtCornerOfTile(Vector3 location)
        {
            float xRelative, yRelative;
            Vector2 relative = MapManager.WorldPosToPositionWithinTileFromCorner(location);
            xRelative = relative.X;
            yRelative = relative.Y;

            if (xRelative + yRelative < GameData.Instance.AIConstants.DistanceToReverseSliding)
            {// top left corner
                return Corner.TopLeft;
            }

            if (MapManager.tileSize - xRelative + yRelative < GameData.Instance.AIConstants.DistanceToReverseSliding)
            { // top right corner
                return Corner.TopRight;
            }

            if (xRelative + (MapManager.tileSize - yRelative) < GameData.Instance.AIConstants.DistanceToReverseSliding)
            {
                // bottom left
                return Corner.BottomLeft;
            }

            if ((MapManager.tileSize - xRelative) + (MapManager.tileSize - yRelative) < GameData.Instance.AIConstants.DistanceToReverseSliding)
            {
                return Corner.BottomRight;
            }

            return Corner.None;
        }
        */

        private Vector3 SlideNorthOrSouth(Vector3 moveVector)
        {
            moveVector.Y += Math.Sign(moveVector.Y) * 0.5f * Math.Abs(moveVector.X);
            moveVector.X = 0f;
            TimeSpentSliding += The.Sim.GameTime.ElapsedGameTime.TotalSeconds;
            return moveVector;
        }

        private Vector3 SlideEastOrWest(Vector3 moveVector)
        {
            moveVector.X += Math.Sign(moveVector.X) * 0.5f * Math.Abs(moveVector.Y);
            moveVector.Y = 0f;
            // we don't want to slide forever!
            TimeSpentSliding += The.Sim.GameTime.ElapsedGameTime.TotalSeconds;
            return moveVector;
        }

        /*
        private bool CanSlideAcrossTileBoundary(int dx, int dy)
        {
            Point nextTile = entity.MapPosition;
            nextTile.X += dx;
            nextTile.Y += dy;
            if (UWGame.SimSide.Instance.Map.TileIsOnMap(nextTile))
            {
                return UWGame.SimSide.Instance.Map.GetTerrainCost(entity.GetTransportType(), entity.MapPosition, nextTile) > 0;
            }
            else return false;
        }

        private bool CanSlideAcrossTileBoundary(Point from, Point to)
        {
            if (UWGame.SimSide.Instance.Map.TileIsOnMap(from) && UWGame.SimSide.Instance.Map.TileIsOnMap(to))
            {
                return UWGame.SimSide.Instance.Map.GetTerrainCost(entity.GetTransportType(), from, to) > 0;
            }
            else return false;
        }
        */

     /*   private bool TestNeighbour(Point from, Common.Direction directionToTest, out Point neighbour)
        {
            neighbour = new Point(entity.MapPosition.X + 1, entity.MapPosition.Y);
            MapManager map = UWGame.SimSide.Instance.map;

            switch (directionToTest)
            {
                case Common.Direction.East:
            }
            return map.TileIsOnMap(neighbour.X, neighbour.Y);
        }*/

      

        public bool TestForEnteringCenterOfTile(Vector3 newLocation, Entity vehicle)
        {
            //STERAIN: keep this in order to create footpaths.
            if (MapManager.DetectIsAtCenterOfTile(newLocation))
            {
                // we have reached the center
                tileProgress = GoalTraverseEdgeBetweenWaypoints.TileProgress.MovingFromCenter;

                Common.Direction oldDirection = detectedDirection;
                bool oldMovesAlong8Dir = movesAlong8Dir;

                if (The.Map.DetectMovementAlong8Dir(newLocation,
                    NormalizedVectorToWaypoint.Value,
                    //entity.NormalizedMoveDir, 
                    ref detectedDirection))
                {
                    movesAlong8Dir = true;
                }
                else
                {
                    movesAlong8Dir = false;
                }

                if (oldMovesAlong8Dir)
                {   
                    if (!movesAlong8Dir || oldDirection != detectedDirection)
                    { // leave trails if we have moved a complete segment:
                        GoalTraverseEdgeBetweenWaypoints.LeaveTrails(entity, oldDirection, entity.MapPosition.Value, vehicle);
                    }
                }

                //STERAIN: No more! done at subtile detection.
                //velocity = GoalTraverseEdgeBetweenWaypoints.SetVelocity(entity, entity.MapPosition, movesAlong8Dir, detectedDirection);

                return true;
            }

            return false;
        }



        protected override void ProcessWhileActive(GameTime elapsed)
        {
            if (entity.ToString().Contains("onlan"))
            {

            }
            //if status is inactive, call Activate()
         /*   ActivateIfInactive();
           
            if (Status == Status.Active)                
            {*/
            if (GoalTraverseEdgeBetweenWaypoints.IsAtWaypoint(entity.PlaySiteLocation, WaypointRay, To, permittedDistanceSquared))
                {
                    Status = Status.Completed;
                }
                else
                {
                    float headingDifference;

                    Entity vehicleEntity = null;
                    if (!entity.GetDrivenVehicle(out vehicleEntity))
                    {
                        // handle vehicle blown up...
                        Status = Goals.Status.Failed;
                        return; // return Status;
                    }

                    SteerableFrontWheels steerableFrontWheels = null;

                    if (vehicleEntity != null)
                    {     
                        // set the vehicle location for rendering:
                       
                        Vector3 moveVector;
                        Vector3 newDirection;
                        float newRotation, newSpeed;

                        vehicleEntity.Find(out steerableFrontWheels);

                        Move(vehicleEntity.EntityType.LocomotorType.MaxAngularSpeed,
                            vehicleEntity.Rotation, 0,
                            vehicleEntity.PlaySiteLocation,
                            elapsed.ElapsedGameTime.TotalSeconds,
                            ((VehicleContainerType)vehicleEntity.EntityType.ContainerType).MaxAcceleration,
                            vehicleEntity.Locomotor.MoveSpeed,
                            vehicleEntity.Locomotor.CurrentMaximumSpeed,
                            out moveVector,
                            out newRotation,
                            out newDirection,
                            out newSpeed,
                            out headingDifference);

                      
                        // test if this is a legal move. Otherwise modify the vector:
                        TestProgress(vehicleEntity.PlaySiteLocation, ref moveVector, vehicleEntity);

                        if (Status == Status.Active)
                        {   // update the position with the modified? move vector:
                            moveVector.Z = 0f;

                            entity.Location = vehicleEntity.Location + moveVector; 
                          

                            newDirection.Z = 0f;
                       //     entity.DrivingVehicle.Renderable.RenderAsModel.SetRotationSpeed(entity.Locomotor.Rotation, newRotation, elapsed.ElapsedGameTime.TotalSeconds);
                            vehicleEntity.Rotation = newRotation;
                            vehicleEntity.FacingNormal = newDirection;
                            vehicleEntity.Locomotor.MoveSpeed = newSpeed;

                          
                            if (vehicleEntity.Locomotor.MoveSpeed > 0f)
                            {
                                float intensity = MathHelper.Clamp(vehicleEntity.Locomotor.MoveSpeed / GameData.Instance.Constants.VehicleSpeedCausingMaximumDust,
                                    0f, 1f) * The.Map.GetTile(vehicleEntity.MapPosition.Value).GetDustFactor();

                                The.Client.ParticleManager.AddDust(
                                    //entity.DrivingVehicle, entity.DrivingVehicle.Location - 30f * entity.DrivingVehicle.Vehicle.Direction,
                                    vehicleEntity, vehicleEntity.PlaySiteLocation - 30f * entity.FacingNormal,
                                    intensity, GameData.Instance.Constants.VehicleDustScale);                               
                            }
                        }

                    }
                    else // move person/animal
                    {
                        int cornerNo;
                        float cornerRotation = GoalTurnToFace.FindClosestCorner(entity, entity.PlaySiteLocation, To.ToVector2(), entity.Rotation, out cornerNo);

                        if (cornerNo != 0)
                        {
                            // do the flip!!! hope no-one notices...
                            entity.Locomotor.FlipFourSidedSymmetryCreature(cornerRotation);
                        }

                        entity.Find(out steerableFrontWheels);

                        Vector3 moveVector;
                        Vector3 newDirection;
                        float newRotation, newSpeed;
                        Move(entity.EntityType.LocomotorType.MaxAngularSpeed,                           
                           // cornerRotation, //
                            entity.Rotation,        
                            cornerNo,
                            entity.PlaySiteLocation,
                            elapsed.ElapsedGameTime.TotalSeconds,
                            entity.Locomotor.GetAcceleration(), 
                            entity.Locomotor.MoveSpeed,
                            entity.Locomotor.CurrentMaximumSpeed,
                            out moveVector,
                            out newRotation,
                            out newDirection, out newSpeed, out headingDifference);

                      
                        // test if this is a legal move. Otherwise modify the vector:
                        TestProgress(entity.PlaySiteLocation, ref moveVector, vehicleEntity);

                        if (Status == Status.Active)
                        {   // update the position with the modified? move vector:

                            System.Diagnostics.Debug.Assert(The.Map.WorldLocationIsOnMap((entity.PlaySiteLocation + moveVector).ToVector2()), "about to move off map!");

                            moveVector.Z = 0f;
                            newDirection.Z = 0f;

                            /// - Order Dependant - 
                            // Right now when we set the location of an entity that entity can detect new entities
                            // If this entity for example is doing an hunting job he will then get an PreyIsNear message goal
                            // sent to GoalMoveToPosition that removes that job and sets move speed to 0
                            // If this is the case then this goal should not modify the movespeed after this has occured
                            // So this is the reason why this is currently order dependant.
                            // While this goal is still allowed to end its process properly.
                            // 
                            ///
                            if (newSpeed != entity.Locomotor.MoveSpeed)
                            {
                                entity.Locomotor.MoveSpeed = newSpeed;
                            }
                           
                            entity.Location += moveVector; 
                            //
                            //
                                                     
                            entity.Rotation = newRotation;
                            entity.FacingNormal = newDirection;


                            if (entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.Run)
                            {
                                // spend energy:
                                BiologicalEntity bioEntity;
                                if (entity.Find(out bioEntity))
                                {
                                    Common.DecreaseValueBetweenZeroAndOneBySecondsAmount(bioEntity.OxygenAndMuscleEnergy, 
                                        GameData.Instance.Constants.DecreaseInOxygenMuscleEnergyWhenRunningPerSecond, elapsed.ElapsedGameTime.TotalSeconds);
                                }
                            }
                                                      
                        }
                    }

                    if (steerableFrontWheels != null)
                    {
                        steerableFrontWheels.SetAngle(headingDifference, (float)elapsed.ElapsedGameTime.TotalSeconds); // .SetWheelsAngle(previousMoveSpeed, headingDifference, elapsedTime);
                    }

                    // goal must only last 0.2 seconds:
                    goalProgress += elapsed.ElapsedGameTime.TotalSeconds;
                    if (goalProgress > maxPeriodInSeconds && Status != Status.Failed)
                    {
                        Status = Status.Completed;
                    }
                  
                }           
        }

        

       

        private void Move(float maxAngularVelocity, float rotation, int cornerNo, Vector3 location, double elapsedTotalSeconds, 
            float? maxAcceleration, float? currentSpeed, float targetSpeed,
            out Vector3 moveVector, out float newRotation, out Vector3 newDirection, out float newSpeed, out float headingDifference)
        {

            float facingDirection; // = (float)Math.Atan2(Aircraft.Direction.Y, Aircraft.Direction.X);
          //  float headingDifference;

            
            facingDirection = GoalFlyToPosition.TurnToFace(location, To.ToVector2(),
                    rotation, (float)(maxAngularVelocity * elapsedTotalSeconds),
                    out headingDifference);

            // update rotation:
         //   facingDirection = GoalTurnToFace.ComputeModelRotationFromCornerRotation(cornerNo, facingDirection);

            newRotation = facingDirection;
            newDirection = new Vector3(
                (float)Math.Cos(facingDirection),
                (float)Math.Sin(facingDirection), 0f);

           // float desiredMoveSpeed;
            float absHeadingDifference = Math.Abs(headingDifference);
            float headingDifferenceToStartDriving = MathHelper.PiOver2;
            float movespeed;
            if (absHeadingDifference > headingDifferenceToStartDriving)
            {
                movespeed = 0f; // stay put while turning
            }
            else
            {
                float modifier = 1f;

                if (GroupMoveActivity != null)
                {
                    modifier = (GroupMoveActivity.IsLeader(entity) ? 1f : speedModifier);
                }

                if (Common.IsZero(absHeadingDifference))
                {

                    movespeed = modifier * targetSpeed;                    
                }
                else
                {
                    movespeed = MathHelper.Lerp(0f, modifier * targetSpeed, 1f - absHeadingDifference / (headingDifferenceToStartDriving)); // start...                   
                }

                // If we have lookahead (for vehicles) at the next waypoint, slow down as we approach this waypoint
                // in proportion to the turn we will have to make after that.
                
                // is this code taken by walkers as well..?
                if (lookAheadDirection.HasValue)
                {
                    Vector2 dirToCurrent = To.ToVector2();
                    dirToCurrent.X = dirToCurrent.X - location.X;
                    dirToCurrent.Y = dirToCurrent.Y - location.Y;
                    float distanceToCurrent = dirToCurrent.Length();

                    if (distanceToCurrent > 0) // avoid divide by zero!
                    {

                        dirToCurrent /= distanceToCurrent;

                        float distanceFactor = Common.ClampTop(distanceToCurrent, 30f);
                        distanceFactor = distanceFactor / 30f;

                        // how much will we be needing to turn?
                        float neededTurn = Vector2.Dot(dirToCurrent, lookAheadDirection.Value);
                        // normalize to 0-1
                        neededTurn = 0.5f + neededTurn / 2f;
                        neededTurn = 1f - neededTurn;

                        float maxMoveSpeed = 200f;
                        maxMoveSpeed = MathHelper.Lerp(200f, 80f, neededTurn * distanceFactor);

                        movespeed = Common.ClampTop(movespeed, maxMoveSpeed);
                    }
 
                }

                // do acceleration:
                if (maxAcceleration != null)
                {
                    movespeed = MathHelper.Clamp(movespeed,
                            (float)(currentSpeed.Value - maxAcceleration.Value * elapsedTotalSeconds),
                            (float)(currentSpeed.Value + maxAcceleration.Value * elapsedTotalSeconds));

                }
            }

            moveVector = (float)(elapsedTotalSeconds * movespeed) * newDirection; //entity.NormalizedMoveDir;
            
          /*  if (float.IsNaN(movespeed) || float.IsNaN(moveVector.X))
            {
                // error...
                throw new Exception();
            }*/

            // return the new speed:
            newSpeed = movespeed;            

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

            this.To = sn.DoVector3(To);
            this.toTile = sn.DoPoint(toTile);
            this.WaypointRay = sn.DoVector3(WaypointRay);
            this.lookAheadDirection = sn.DoVector2Nullable(lookAheadDirection);
            this.normalizedVectorToWaypoint = sn.DoVector2Nullable(normalizedVectorToWaypoint);            
            this.speedModifier = sn.DoFloat(speedModifier);
            this.goalProgress = sn.DoDouble(goalProgress);
            this.MovingOutOfHarmsWay = sn.DoBool(MovingOutOfHarmsWay);            
            this.permittedDistanceSquared = sn.DoFloat(permittedDistanceSquared);
            this.TimeSpentSliding = sn.DoDouble(TimeSpentSliding);
            this.detectedDirection = (Common.Direction)sn.DoEnum(detectedDirection);
            this.movesAlong8Dir = sn.DoBool(movesAlong8Dir);
            this.tileProgress = (GoalTraverseEdgeBetweenWaypoints.TileProgress)sn.DoEnum(tileProgress);
            this.isLastNode = sn.DoBool(isLastNode);
            this.hasEverBeenOnUnblockedTile = sn.DoBool(hasEverBeenOnUnblockedTile);

            sn.Ignore(freeGoals);
            sn.Ignore(GroupMoveActivity);

            return this;
        }

        #endregion
         
    }

    
}
