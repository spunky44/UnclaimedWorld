using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.AI.Pathfinding;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Renderables; //TODO DECOUPLE
using GameStateManagement;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    class GoalTraverseEdgeBetweenWaypoints: CompositeGoal
    {       
        public Vector3 To;
        public Vector3 WaypointRay;
        public int Number; // the ID of this waypoint.

        public Point toTile;

       // private float distanceBetweenWaypoints;

        public Vector2? LookAheadDirection;

        private double timeSpentSliding = 0;

      
       // private Vector2 normalizedMoveDir;
        private Common.Direction detectedDirection;
        private bool movesAlong8Dir = false;
        /// <summary>
        /// This is used to determine if the tile boundaries and the center should be tested to see if the entity enters them.
        /// </summary>
        public enum TileProgress { NoDescription = 0, MovingFromEdge = 1, MovingFromCenter = 2 }
        private TileProgress tileProgress = TileProgress.NoDescription;

       // private float? terrainMoveFactor = null;

        private bool isLastNode;

        public bool StopMovingWhenTerminating = true;

     //   private float velocity;

        public bool MovingOutOfHarmsWay = false;

        public AI.Activities.GroupMoveActivity GroupMoveActivity;

        //This list is maintained by the group leader. It contains those group members who have sent 'Wait for me' messages to the leader.
        private List<Entity> GroupMoveStragglers;
      //  private bool GroupMoveWaitForStraggler = false;


        public float PermittedDistanceSquaredToDestination = GameData.Instance.Constants.DistanceSquaredLimitForWaypoints; // default


        public GoalTraverseEdgeBetweenWaypoints(Entity owner, Vector3 to, Vector2? lookAheadDirection, Vector3 waypointRay, int number, bool isLastEdge, bool movingOutOfHarmsWay, List<EntityGroupID> ownersOfVehicles, AI.Activities.GroupMoveActivity groupMoveActivity)
            : base(owner)
        {
            To = to;
            // will be zero if we are at the waypoint:
            WaypointRay = waypointRay; //

            Number = number;

            LookAheadDirection = lookAheadDirection;

       /*     if (WaypointRay.X < 0f)
            {

            }
            */
            toTile = MapManager.WorldPosToTile(To);

            isLastNode = isLastEdge;
            
            this.ownersOfVehicles = ownersOfVehicles;
            this.GroupMoveActivity = groupMoveActivity;

            MovingOutOfHarmsWay = movingOutOfHarmsWay;
        }

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

            this.detectedDirection = sn.DoEnum(detectedDirection);
            this.isLastNode = sn.DoBool(isLastNode);
            this.LookAheadDirection = sn.DoVector2Nullable(LookAheadDirection);
            this.movesAlong8Dir = sn.DoBool(movesAlong8Dir);
            this.MovingOutOfHarmsWay = sn.DoBool(MovingOutOfHarmsWay);
            this.Number = sn.DoInt32(Number);
            this.PermittedDistanceSquaredToDestination = sn.DoFloat(PermittedDistanceSquaredToDestination);
            this.StopMovingWhenTerminating = sn.DoBool(StopMovingWhenTerminating);
            this.tileProgress = sn.DoEnum(this.tileProgress);
            this.timeSpentSliding = sn.DoDouble(timeSpentSliding);
            this.To = sn.DoVector3(To);
            this.toTile = sn.DoPoint(toTile);
            this.WaypointRay = sn.DoVector3(WaypointRay);

            sn.Postpone(GroupMoveStragglers);
            sn.Postpone(GroupMoveActivity);

            return this;
        }

        public GoalTraverseEdgeBetweenWaypoints()
        {
        }



        protected override void Activate()
        {
         
            if (entity.HasStance()) // entity.Locomotor.LeggedLocomotor != null)
            {
                entity.Locomotor.Stance.CurrentStance = entity.EntityType.LocomotorType.StancesType.MovingStanceType;
                //entity.Locomotor.LeggedLocomotor.CurrentStance = LeggedLocomotor.Stance.Standing;
            }

            if (IsAtWaypoint(entity.PlaySiteLocation, WaypointRay, To, PermittedDistanceSquaredToDestination))
            {
                Status = Status.Completed;
                return;
            }                     
            else
            {
                // start:

                Entity vehicleEntity = null;
                if (!entity.GetDrivenVehicle(out vehicleEntity))
                {
                    // handle vehicle blown up...
                    Status = Goals.Status.Failed;
                    return;
                }


                // not needed?
           /*     entity.NormalizedMoveDir = To - new Vector2(entity.Location.X, entity.Location.Y);
                entity.NormalizedMoveDir.Normalize();
                */
                Common.Direction detectedDirection = Common.Direction.North;

                Vector2 normalizedVectorToWaypoint;
                normalizedVectorToWaypoint.X = To.X - entity.PlaySiteLocation.X;
                normalizedVectorToWaypoint.Y = To.Y - entity.PlaySiteLocation.Y;
                normalizedVectorToWaypoint.Normalize();

                if (The.Map.DetectMovementAlong8Dir(entity.PlaySiteLocation, 
                    normalizedVectorToWaypoint,
                    ref detectedDirection))
                {
                    movesAlong8Dir = true;
                }
                else
                {
                    movesAlong8Dir = false;
                }

                Point subtilePos = MapManager.WorldPosToSubtile(entity.PlaySiteLocation);
               // velocity = SetVelocityFromTerrain(entity, subtilePos, vehicleEntity);
             
                GoalTraverseEdgeBetweenWaypointsAtomic newGoal =
                    GoalTraverseEdgeBetweenWaypointsAtomic.GetGoal(entity, To, LookAheadDirection, toTile, WaypointRay, timeSpentSliding, isLastNode, MovingOutOfHarmsWay,
                    /*distanceBetweenWaypoints,*/ detectedDirection, movesAlong8Dir, tileProgress, GroupMoveActivity, PermittedDistanceSquaredToDestination);
                
                AddSubgoal(newGoal);
                              
                                                       
                SelectMoveSpeed(entity);


                if (GroupMoveActivity != null && GroupMoveActivity.IsFollower(entity))
                {
                    GroupMoveTestStatus();
                }

                Status = Status.Active;
            }
  
        }

       /* public static void DetectIsHauling(Entity entity, ref bool isHauling)
        {
            isHauling = false;
            if (entity.CarryItems != null)
            {
                foreach (Item item in entity.CarryItems.CarriedItems)
                {
                    if (item.AssignedToJob != null)
                    {
                        // will this also work with hauling to storage? is there a 'job' for that?
                        // what about items/weapons to use? they should not be counted...
                        isHauling = true;
                        break;
                    }
                }
            }
        }*/

      /*  public override float GetExertionLevel()
        {
            if (entity.DrivingVehicle == null)
            {
                return PhysicalWork.Medium;
            }
            else return PhysicalWork.None;
        }*/



      /// <summary>
      /// continously monitor and set our speed according to physical capabilities and other factors...
      /// </summary>
      /// <param name="entity"></param>
        public static void SelectMoveSpeed(Entity entity)
        {
            if (entity.EntityType.IntelligenceType.IsMobile
               && entity.Locomotor.LeggedLocomotor != null) // wheels also? probably..
            {
                bool isHauling = entity.IsHaulingNothingMounted();

                if (isHauling)
                {
                    entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Haul; //??
                }
                else if (entity.DrivingVehicle != null)
                {
                    // we are already playing the driving animation...

                }
                else
                {
                    if (entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.Run)
                    {
                        // see if we can still run...
                        SetFleeingSpeedType(entity);
                    }
                }
            }
        }
        
        

        public static Vector3 ComputeWaypointRay(Vector3 waypoint, Entity entity)
        {
            return Vector3.Cross(waypoint - entity.PlaySiteLocation, Vector3.UnitZ);
        }

        private void AddStraggler(Entity member)
        {
            if (GroupMoveStragglers == null)
            {
                GroupMoveStragglers = new List<Entity>();
            }

            if (!GroupMoveStragglers.Contains(member))
            {
                GroupMoveStragglers.Add(member);
            }

        }

        /// <summary>
        /// Return true if all stragglers are ready
        /// </summary>
        /// <returns></returns>
        private bool AreAllStragglersReady()
        {
            if (GroupMoveStragglers == null)
            {
                return true;
            }

            Entity member;
            for(int i = GroupMoveStragglers.Count - 1; i >= 0; i--)
            {
                member = GroupMoveStragglers[i];
                if (!GoalTraverseEdgeBetweenWaypoints.IsAtWaypoint(member.PlaySiteLocation, member.Intelligence.GroupMoveAssignedWaypointRay.Value, member.Intelligence.GroupMoveAssignedWaypoint.Location, PermittedDistanceSquaredToDestination))
                {
                    return false;
                }
                else
                {                    
                    GroupMoveStragglers.RemoveAt(i);
                    i--;
                }
            }
            return true;
        }

        private void GroupMoveTestStatus()
        {
            if (!GroupMoveActivity.IsLeader(entity))
            {
                float distanceToWaypoint = Common.DistanceOctile(entity.PlaySiteLocation, To);
                if (distanceToWaypoint > GameData.Instance.AIConstants.DistancePromptingHaltRequest)
                {
                    float leadersDistanceToWaypoint = Common.DistanceOctile(GroupMoveActivity.Leader.PlaySiteLocation, To);
                    if (leadersDistanceToWaypoint < distanceToWaypoint)
                    {
                        // say: wait for me! to leader.
                        GroupMoveActivity.Leader.SendMessage(new Message(entity, Message.MessageTypes.WaitForMeGroup, null));
                        //entity.GroupMoveIsCatchingUp = true;
                        entityIntelligence.FollowerStatus = FollowerStatus.IsCatchingUp;
                    }
                   
                }
                /*else if (distanceToWaypoint > AIConstants.DistanceSquaredPromptingSlowdownRequest)
                {
                    foreach (Entity member in GroupMoveActivity.Members)
                    {
                        if (entity != member)
                        {   // say: slow down!
                            member.HandleMessage(new Message(Message.MessageTypes.SlowdownGroup));
                            GroupMoveIsCatchingUp = true;
                        }
                    }
                }*/
            }
        }

        public static void LeaveTrails(Entity entity, Common.Direction dir, Point p, Entity vehicle)
        {


            // // // // // HACK HACK HACK 
            // // // // // HACK HACK HACK 
            // // // // // HACK HACK HACK 

            return;   //MLo: disabling the AddToFootPath stuff, becuase it sometimes erases blocked subtiles

            // // // // // HACK HACK HACK 
            // // // // // HACK HACK HACK 
            // // // // // HACK HACK HACK 



            // persons leave trails on terrain 
            if (entity.PersonEntity != null)
            {
                TerrainTile tile = The.Map.TileMap[p.X][p.Y];
                if (vehicle == null)
                {
                    tile.AddToFootPath(dir);  // 0.1f                 
                }
                else
                {
                    tile.AddToWheelPath(dir,
                        GameData.Instance.Constants.AmountToAddToPathOnTraversal *
                        MathHelper.Clamp(((VehicleContainerType)vehicle.EntityType.ContainerType).UnladenWeight / 20f, 1f, 5f));                    
                }
            }
        }

        /*
        public static float SetVelocityFromTerrain(Entity entity, Point sampleSubtile, Entity vehicle)
        {
            
            float currentMaximumSpeed;
            if (vehicle == null)
            {
                currentMaximumSpeed = entity.Locomotor.CurrentMaximumSpeed;
            }
            else
            {
                currentMaximumSpeed = vehicle.Locomotor.CurrentMaximumSpeed;
            }

            if (currentMaximumSpeed > GameData.Instance.Constants.MinimumSpeedForTerrainToHaveEffect)
            {
                // use the terrain map - not yet implemented on pathfinder...
               
               // Terrain terrain = The.Map.GetTerrain(sampleSubtile);

                // checks for trees also
                float roughness = The.Map.GetRoughness(sampleSubtile); 

               
                float negatedRoughness = roughness * (1f - entity.EntityType.LocomotorType.LeggedLocomotorType.TerrainNegateFactor);

                // TODO: set anim flag when moving through difficult terrain
                return Common.Clamp((1f - negatedRoughness) * currentMaximumSpeed, 
                    GameData.Instance.Constants.MinimumSpeedForTerrainToHaveEffect, currentMaximumSpeed);
            }

            return currentMaximumSpeed;

        }
        */

        /* OLD:
        public static float SetVelocity(Entity entity, Point mapPosition, bool movesAlong8Dir, Common.Direction detectedDirection)
        {
            float v;
            if (entity.DrivingVehicle == null)
            {
                v = entity.Locomotor.CurrentMaximumSpeed; 
            }
            else
            {
                v = entity.DrivingVehicle.Locomotor.CurrentMaximumSpeed;
            }

            Map.TerrainTile tile = UWGame.SimSide.Instance.Map.TileMap[mapPosition.X, mapPosition.Y];
            if (movesAlong8Dir)
            {
                // grab the edge:  
              
                float cost = (float)(UWGame.SimSide.Instance.Map.TerrainCosts[(int)entity.GetTransportType(), mapPosition.X, mapPosition.Y, (int)detectedDirection]);
                if ((int)detectedDirection > 3) // diagonal
                {  
                    // CORRECT THE COST ON DIAGONAL EDGES
                    cost = cost / MapManager.DiagonalFactor;
                }
                return v / cost;
                //return v / tile.TerrainType.Cost(entity.GetTransportType(), tile.HighestRankedPathFeature((int)detectedDirection));
            }
            else
            {
                // sample the terrain:
                return v / tile.TerrainType.Cost(entity.GetTransportType(), TerrainType.TerrainFeatures.None);

            }

        }*/


        public static bool IsAtWaypoint(Vector3 worldLocation, Vector3 waypoint)
        {
           // let's make sure we are in the target subtile...
            
            return MapManager.WorldPosToSubtile(worldLocation) == MapManager.WorldPosToSubtile(waypoint)
                    && Vector2.DistanceSquared(worldLocation.ToVector2(), waypoint.ToVector2()) < GameData.Instance.Constants.DistanceSquaredLimitForWaypoints;
        }
      
        public static bool IsAtWaypoint(Vector3 location, Vector3 waypointRay, Vector3 to, float permittedDistanceSquared)
        {
            // first check nearness:
            if (Vector2.DistanceSquared(location.ToVector2(), to.ToVector2()) < permittedDistanceSquared) // GameData.Instance.Constants.DistanceSquaredLimitForWaypoints)
            {
                return true;
            }
            else if (waypointRay != Vector3.Zero)
            {
                // next test line crossing:
                Vector3 lineToWaypoint = new Vector3(location.X - to.X, location.Y - to.Y, 0f);
              
                // see if we have crossed the line, perpendicular to the movement direction - this is indicated by the z-vector
                // changing sign:
                return Vector3.Cross(lineToWaypoint, waypointRay).Z < 0f;
            }

            return false;

        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
         /*   ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                //process the subgoals
                // Status = ProcessSubgoals(elapsed);

                Status subgoalStatus = ProcessSubgoals(elapsed);
                
                if (subgoalStatus == Status.Completed)
                {
                    // NEW:
                    ITopLevelGoal newGoal;
                    if (entityIntelligence.Brain.ArbitrateWhileBusy(out newGoal))
                    {
                        // we have a new, better goal. All subgoals have been removed, but on the way back up the goal stack, they still exist.
                        // let's set status to Failed:
                        HandleSubstitutedGoalByArbitrator();

                    }
                    else if (IsAtWaypoint(entity.PlaySiteLocation, WaypointRay, To, PermittedDistanceSquaredToDestination))
                    {
                        Status = Status.Completed;

                        if (GroupMoveActivity != null)
                        {
                            if (entityIntelligence.FollowerStatus == FollowerStatus.IsCatchingUp
                                || (entityIntelligence.FollowerStatus == FollowerStatus.IsFollowingPathToWaypoint && isLastNode))//entity.GroupMoveIsCatchingUp == true)
                            {
                                // signal to the leader that we have caught up.
                                GroupMoveActivity.Leader.SendMessage(new Message(Message.MessageTypes.StartGroupMovement));
                                entityIntelligence.FollowerStatus = FollowerStatus.Normal;
                                //entity.GroupMoveIsCatchingUp = false;
                            }
                            else if (!AreAllStragglersReady()) // GroupMoveWaitForStraggler)
                            {   // we are the leader. Wait here for the follower to catch up. All the others will wait as well...                            
                                Status = Status.Active;
                                AddSubgoal(new GoalWait(entity));
                            }

                            if (isLastNode &&
                                (GroupMoveActivity.IsLeader(entity) ||
                                (GroupMoveActivity.IsFollower(entity) && entityIntelligence.FollowerStatus != FollowerStatus.IsFollowingPathToWaypoint &&
                                    entityIntelligence.GroupMoveAssignedWaypoint != null && entityIntelligence.GroupMoveAssignedWaypoint.Number == Number)))
                            {   // signal to the other group members that we are at the destination. 
                                entityIntelligence.IsAtGroupMoveDestination = true;
                            }
                        }
                    }
                    else
                    {
                        // salvage the computed values from the completed atomic goal:
                        if (Subgoals.Count > 0)
                        {
                            GoalTraverseEdgeBetweenWaypointsAtomic oldGoal = Subgoals.Peek() as GoalTraverseEdgeBetweenWaypointsAtomic;
                            if (oldGoal != null)
                            {
                                this.movesAlong8Dir = oldGoal.movesAlong8Dir;
                                this.detectedDirection = oldGoal.detectedDirection;
                                this.tileProgress = oldGoal.tileProgress;
                                this.timeSpentSliding = oldGoal.TimeSpentSliding;
                            }
                        }
                        if (ID == GoalID.Invalid)
                        {
                            int i = 0;
                        }
                        // Test if is safe to move on. Keep moving, fail or turn around...
                        ValidateSafetyAndTakeAction();

                    }

                }
                else if (subgoalStatus == Status.Failed)
                {
                    Status = Status.Failed;
                }
          /*  }

            ExitIfFailedOrCompleted();

            return Status;*/

        }


        
        /// <summary>
        /// 1. test the current tile to see if it is not blocking
        /// 2. test the next tile if reasonably close.
        /// </summary>
        private void ValidateSafetyAndTakeAction()
        {
            if (!MovingOutOfHarmsWay)
            {
                DiscomfortMap dMap = entityIntelligence.Allegiance.SharedKnowledge.GetDiscomfortMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);
                
                if (dMap.Map.GetIsBlocked(entity.MapPosition.Value))
                {

                    float discomfortLevel = dMap.Map.GetValue(entity.MapPosition.Value);
                   
                    if (discomfortLevel > entityIntelligence.PanicLevel)
                    {
                        PerformPanicFleeing(dMap);
                        return; //We are panic fleeing. New goals has been added etc...
                    }
                    else
                    {
                        // NEW: just fail...
                        Status = Goals.Status.Failed;
                        return;

                        // OLD:
                        // not panicking, just moving away:
                       /* List<AI.Pathfinding.PathFinderNode> path = FindPathToSafety(dMap, entity);

                      
                        if (path != null)
                        {// we found a path out of this mess:
                            GoalFollowPath followPath = new GoalFollowPath(entity, path, null, ownersOfVehicles, To, null);
                            followPath.MovingOutOfHarmsWay = true; // ignore discomfort along the way
                            AddSubgoal(followPath);
                            return;
                        }
                        else
                        {
                            // we are fucked...
                            // keep moving...                            
                            AddSubgoal(GoalTraverseEdgeBetweenWaypointsAtomic.GetGoal(entity, To, LookAheadDirection, toTile, WaypointRay, timeSpentSliding, isLastNode, MovingOutOfHarmsWay,
                                //distanceBetweenWaypoints, 
                                detectedDirection, movesAlong8Dir, tileProgress, GroupMoveActivity, PermittedDistanceSquaredToDestination));
                            Status = Status.Active;
                            return;
                        }*/

                    }
                }

                //OK, current tile is fine, now look at the next:
               // Point waypointTile = MapManager.WorldPosToTile(To);
                if (toTile != entity.MapPosition)
                {                       
                    // our waypoint is not inside the current tile. 
                    // sample the tile we are moving towards:
                    Point nextTile = MapManager.WorldPosToTile(
                        entity.PlaySiteLocation
                        + entity.FacingNormal * MapManager.tileSizeOver2 );

                    nextTile = The.Map.ClampTileMapPosition(nextTile);

                    if (nextTile != entity.MapPosition)
                    {   // we are closer than half a tile from the next. Evaluate blocked status of that tile:
                        if (dMap.Map.GetIsBlocked(nextTile))
                        {   // Danger ahead!
                            //
                            // we are safe where we are, just fail and repath
                            Status = Status.Failed;
                            return;

                        }
                    }
                }               

                // everything is dandy, keep moving...
                SkipThisAssert = true;
                AddSubgoal(GoalTraverseEdgeBetweenWaypointsAtomic.GetGoal(entity, To, LookAheadDirection, toTile, WaypointRay, timeSpentSliding, isLastNode, MovingOutOfHarmsWay,
                            //distanceBetweenWaypoints, 
                            detectedDirection, movesAlong8Dir, tileProgress, GroupMoveActivity, PermittedDistanceSquaredToDestination));
                SkipThisAssert = false;
                
                Status = Status.Active;                    
                return;
                
            }

            // we are following a path to safety:
            AddSubgoal(GoalTraverseEdgeBetweenWaypointsAtomic.GetGoal(entity, To, LookAheadDirection, toTile, WaypointRay, timeSpentSliding, isLastNode, MovingOutOfHarmsWay,
                               //distanceBetweenWaypoints, 
                               detectedDirection, movesAlong8Dir, tileProgress, GroupMoveActivity, GameData.Instance.Constants.DistanceSquaredLimitForWaypoints));
            Status = Status.Active;  
        }

        public override void Deactivate()
        {
           
         //   RemoveAllSubgoals(); not needed..?
        //    base.Terminate();

            // we don't want to stop moving after each GoalTraverse...
            // the isLastNode is false if we are waiting for passengers to get on board... so we set StopMoving in the Wait goal instead.
            if (isLastNode && StopMovingWhenTerminating)
            {
                Entity vehicleEntity = null;
                if (!entity.GetDrivenVehicle(out vehicleEntity))
                {
                    // handle vehicle blown up...
                    Status = Goals.Status.Failed;
                    return;
                }

                StopMoving(entity, vehicleEntity);
            }

           // entity.Renderable.ClearAnimationStateFlag(AnimState.Moving); // MOVEANIMOLD
            //TODO this is icky inside the goal class... all state flags having to do with movement or rotation or speed
            //should be set, cleared and managed in Locomotor, period. -MLo
            
        }

        public static void StopMoving(Entity entity, Entity vehicle)
        {           

            if (vehicle != null)
            {   // stop!
                vehicle.Locomotor.MoveSpeed = 0f;              
            }
            else
            {
                entity.Locomotor.MoveSpeed = 0f;              
            }
        }


       


        public override bool HandleMessage(Message message)
        {
             //We are a composite goal. Therefore, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            if (!handled)
            {
                switch (message.MessageType)
                {
                    case Message.MessageTypes.StartGroupMovement:
                        if (Subgoals.Count > 0 && !(Subgoals.Peek() is GoalWait))
                        {
                            return false;
                        }
                        return true; // ignore this message, we are already moving - we don't need to add a new GoalTraverseEdge!!!

                    case Message.MessageTypes.WaitForMeGroup:
                        //GroupMoveWaitForStraggler = true;
                        AddStraggler(message.Sender);

                        return true;
                    case Message.MessageTypes.CancelJobOrItemInUse:
                    case Message.MessageTypes.CancelJobForAIReset:

                        Entity vehicleEntity = null;
                        if (!entity.GetDrivenVehicle(out vehicleEntity))
                        {
                            // handle vehicle blown up...
                            Status = Goals.Status.Failed;
                            return true;
                        }

                        StopMoving(entity, vehicleEntity);

                        if (message.OtherInfo is Message.CancelJobKeepVehicle &&
                            (Message.CancelJobKeepVehicle)message.OtherInfo == Message.CancelJobKeepVehicle.LeaveVehicle
                            && vehicleEntity != null)
                        {   // we are asked to leave the vehicle - we need his vehicle NOW?
                            entityIntelligence.Brain.AddSubgoal(new GoalExitVehicle(entity, vehicleEntity.EntityID));
                        }
                        
                        return false; // handle this higher up also, to cleanup the rest of the job.

                    case Message.MessageTypes.WaitAndMakeRoom:
                        entity.Locomotor.CollisionResponder.WaitsToGiveRoomToOtherAgent = true;

                        double waitTime = Common.ClampBottom(The.Sim.GameplayRandomGenerator.RandomNormalDistribution(2, 0.5), 0.5f);

                        AddSubgoal(new GoalWait(entity, waitTime));

                        return true;


                }

                return false;
            }
            else return true;
        }
    }

    
}
