using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.Containers;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Snapshots;
using System.Diagnostics;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Maps.Regions;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// if a target entity is provided, and it is inside a container, the goal will try to enter it.
    /// </summary>
    public class GoalMoveToPosition : CompositeGoal
    {
        //move to EITHER a location:
        public WorldLocation? Destination = null;
        public TilePos? DestinationTilePos = null;
      
        // OR an entity:
        
        /// <summary>
        /// this can be an entity that we are attacking or a building that we want to enter or something else
        /// </summary>
        public EntityID? DestinationEntity = null;


        // for static (ground) destinations:
        public float PermittedDistanceSquaredToDestination = GameData.Instance.Constants.DistanceSquaredLimitForWaypoints; // default


        AttackType attackType;
        bool useMeleeLocationAsDestination;

        /// <summary>
        /// this callback is used to get an offset location to the Entity we are moving towards (for example in melee combat)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
     /*   public delegate bool GetDestinationEntityLocation(Entity thisEntity, IKnownEntityData target,
                                                            out Vector3? meleeLocation,
                                                            out bool isCenterLocation); //  

        private GetDestinationEntityLocation getDestinationEntityLocation;*/

        /// <summary>
        /// this variable tells the agent if it is OK to stop when within range of the target.
        /// </summary>
        public bool IsChasingTargetCenterLocation = true;

        public bool IsFinalDestination;

        /// <summary>
        /// This state variable makes it possible to disable further decomposition of the movement goal into
        /// foot/vehcile stretches when moving by vehicle,
        /// thereby avoiding infinite recursion.
        /// </summary>
        Nesting nesting = Nesting.NotNested;


        //    public bool MayUseVehicle = true;
        //  public bool FreeUpAnyVehicleAfterUse = true;
        public EntityID? UsedVehicle;
        public VehicleUse VehicleUseByGoal;

        public AI.Activities.GroupMoveActivity GroupMoveActivity;

        private int noOfFailures = 0;


        private Vector3 pathStartLocation;



        //*********
        // progress variables:
        private bool isEvaluatingPersonalTransport = false;

        // repathing:
        private Regulator repathRegulator;
        private Vector3 lastTargetLocation;

        private TimeSpan timePointForLastRepath; //= The.Sim.TotalUnPausedGameTime;

       
        /// <returns></returns>
      //  public delegate void RepathDone(float pathLength);
      //  public event RepathDone RepathDoneEvent; 

        /// <summary>
        /// use this to get updates about the new distance to the target when repathing is performed
        /// </summary>
        public IDActionEvent<float> RepathDoneEvent;

        // ********


        public enum VehicleUse { NoVehicle, FreeUpAfterUse, KeepVehicle }
        public enum Nesting { IsNested, NotNested }

        public GoalMoveToPosition(Entity entity, Vector3 destination, Nesting nesting, List<EntityGroupID> ownersOfVehicles, 
            EntityID? targetEntity = null, 
            //GetDestinationEntityLocation getDestinationEntityLocation = null, 
            bool useMeleeLocationAsDestination = false,
            AttackType attackType = null)
            : base(entity)
        {
            ResetDestination(destination);

            this.nesting = nesting;
            this.VehicleUseByGoal = VehicleUse.FreeUpAfterUse;
            this.ownersOfVehicles = ownersOfVehicles;

            this.DestinationEntity = targetEntity;
            this.useMeleeLocationAsDestination = useMeleeLocationAsDestination;
            //this.getDestinationEntityLocation = getDestinationEntityLocation;
            this.attackType = attackType;

            Init();
        }


        public GoalMoveToPosition(Entity entity, Vector3 destination, List<EntityGroupID> ownersOfVehicles, VehicleUse vehicleUse = VehicleUse.FreeUpAfterUse, 
            EntityID? targetEntity = null,
            bool useMeleeLocationAsDestination = false,
            //GetDestinationEntityLocation getDestinationEntityLocation = null, 
            AttackType attackType = null)
            : base(entity)
        {

            this.VehicleUseByGoal = vehicleUse;
            this.ownersOfVehicles = ownersOfVehicles;

            this.DestinationEntity = targetEntity;
            this.useMeleeLocationAsDestination = useMeleeLocationAsDestination;
          //  this.getDestinationEntityLocation = getDestinationEntityLocation;
            this.attackType = attackType;

            ResetDestination(destination);

            Init();

        }


        /*  public GoalMoveToPosition(Entity entity, Vector3 destination, List<Owner> ownersOfVehicles, EntityID? targetEntity = null, GetDestinationEntityLocation getDestinationEntityLocation = null, AttackType attackType = null)            
              : base(entity)
          {
            
              this.VehicleUseByGoal = VehicleUse.FreeUpAfterUse;
              this.ownersOfVehicles = ownersOfVehicles;

              this.DestinationEntity = targetEntity;
              this.getDestinationEntityLocation = getDestinationEntityLocation;
              this.attackType = attackType;

              ResetDestination(destination);
           
          }*/


        /// <summary>
        /// use this overload to correctly move to a target entity like an item that may be inside a builidng or container, or may be on the ground outside.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ownersOfVehicles"></param>
        /// <param name="targetEntityData"></param>
        /// <param name="getDestinationEntityLocation"></param>
        /// <param name="attackType"></param>
        public GoalMoveToPosition(Entity entity, List<EntityGroupID> ownersOfVehicles, IKnownEntityData targetEntityData, GoalMoveToPosition.VehicleUse vehicleUse = VehicleUse.FreeUpAfterUse, 
            //GetDestinationEntityLocation getDestinationEntityLocation = null,
            bool useMeleeLocationAsDestination = false,
            AttackType attackType = null)
            : base(entity)
        {

            this.VehicleUseByGoal = vehicleUse;
            this.ownersOfVehicles = ownersOfVehicles;
            this.useMeleeLocationAsDestination = useMeleeLocationAsDestination;

            if (targetEntityData.ContainedBy.HasValue)
            {
                this.DestinationEntity = targetEntityData.ContainedBy.Value;
            }
            else
            {
                Vector3 destination = targetEntityData.AccessPoint.Value;
                ResetDestination(destination);
            }

           // this.getDestinationEntityLocation = getDestinationEntityLocation;
            this.attackType = attackType;

            Init();
        }


        private void Init()
        {
            if (entity.ID == (EntityID)12615)
            {

            }

             timePointForLastRepath = The.Sim.TotalUnPausedGameTime;
        }

        public GoalMoveToPosition()
        {
        }     
       


        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), DestinationTilePos);
        }

        protected override void Activate()
        {
            // Warning: any subgoals added are removed when the path is received... take care.

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            ActivateAndGetPath();

            //TODO: "tell visit place that this entity is leaving, by way of mobile entity";

        }

        protected override void CreateRegulators()
        {
            base.CreateRegulators();

            repathRegulator = new Regulator(The.Sim.GameplayRandomGenerator,5, "GoalMoveToPositionPath");
        }

        private void ActivateAndGetPath()
        {
            Status = Status.Active;

            pathStartLocation = entity.AccessPoint.Value; 

            // see if we are on the target subtile:
            if (Destination.HasValue && MapManager.WorldPosToSubtile(entity.PlaySiteLocation) == MapManager.WorldPosToSubtile(Destination.Value))
            {
                Status = Status.Completed;
                return;
            }

            IKnownEntityData destinationData = null;
            if (DestinationEntity.HasValue)
            {
                EntityResult result = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(DestinationEntity.Value, out destinationData);
                if (EntityResultCausesFailedGoal(result))
                {
                    return;
                }
                else if (destinationData == entity) // safeguard against chasing ourselves (if we already contain the item we want to pick up)
                {
                    Status = Goals.Status.Completed;
                    return;
                }
            }

            UpdateMoveTargetForLerping();



            Entity insideBuilding;
            if (!entity.GetContainedBy(out insideBuilding))
            {
              
                Status = Goals.Status.Failed;
                return;
            }

            if (insideBuilding != null) 
            {               
               // AddSubgoal(new GoalExit(entity)); now added after we have a path... let's see if it still works.

                pathStartLocation = insideBuilding.AccessPoint.Value;
                
                //TODO make sure pathStartLocation is compatible with the exit door location in IExit 

                // tilePosPathStart = entity.InsideBuilding.TileLayout.Building.FrontDoorTilePos;
                //}
            }


            // get up...
          //  ChangeStance(null, Entities.Locomotors.LeggedLocomotor.Stance.Standing);

            // now continue with the path...

            if (GroupMoveActivity != null)
            {   // move as a group
                if (GroupMoveActivity.IsLeader(entity))
                {
                    entityIntelligence.IsAtGroupMoveDestination = false;
                    // request a path
                    GetPath(pathStartLocation, destinationData);
                }
                else if (GroupMoveActivity.IsFollower(entity))
                {
                    // move toward assigned waypoint, or wait...
                    ActivateGroupFollower(pathStartLocation);
                }
            }
            else if (entity.DrivingVehicle == null && VehicleUseByGoal != VehicleUse.NoVehicle)
            {
                if (UsedVehicle != null)
                {   // HAULTEST - NEW!
                    // do we already have as designated vehicle, but are not sitting in it?
                    IKnownEntityData vehicleData;
                    if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(UsedVehicle.Value, out vehicleData)))
                    {
                        return;
                    }

                    EnterVehicleAndGo(vehicleData);
                }
                else
                {
                    // see if we can find one:
                    DecideOnVehicleOrGoByFoot(destinationData);
                }
            }
            else if (entity.DrivingVehicle != null)
            {
                IKnownEntityData vehicleData;
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entity.DrivingVehicle.Value, out vehicleData)))
                {
                    return;
                }

                if (nesting != Nesting.IsNested && VehicleUseByGoal != VehicleUse.NoVehicle)
                {
                    if (((VehicleContainerType)vehicleData.EntityType.ContainerType).Aircraft != null)
                    {
                        // go by plane:
                        // TODO: Find Landing spot!
                        AddSubgoal(new GoalTakeoff(entity));
                        AddSubgoal(new GoalFlyToPosition(entity, DestinationTilePos.Value.ToPoint()));
                        AddSubgoal(new GoalLand(entity));

                        return;
                    }
                    else
                    {
                        // we are sitting in a vehicle that we are allowed to use
                        // register the trigger
                        Trigger trigger = new Trigger(entity, null, GameData.Instance.AllTriggerTypes["drivenVehicle"]); //   TriggerPriority.Normal, 5, DestinationTilePos);
                        entity.AttachTrigger(trigger);

                        GoByVehicle(vehicleData);

                        return;
                    }
                }
                else if (VehicleUseByGoal == VehicleUse.NoVehicle)
                {
                    // we are sitting in a vehicle that we are not allowed to use                
                    AddSubgoal(new GoalExitVehicle(entity, entity.DrivingVehicle.Value));
                    // go by foot:
                    GetPath(pathStartLocation, destinationData);

                    return;
                }
                /* else if (VehicleUseByGoal != VehicleUse.NoVehicle && vehicleData.EntityType.VehicleType.Aircraft != null)
                 { // go by plane:
                     // TODO: Find Landing spot!
                     AddSubgoal(new GoalTakeoff(entity));
                     AddSubgoal(new GoalFlyToPosition(entity, DestinationTilePos));
                     AddSubgoal(new GoalLand(entity));

                     return;
                 }*/
            }
            else
            {
                GetPath(pathStartLocation, destinationData);
            }
        }

        private void UpdateMoveTargetForLerping() //IKnownEntityData destinationData)
        {
            IKnownEntityData destinationData = null;
            if (DestinationEntity.HasValue)
            {
                EntityResult result = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(DestinationEntity.Value, out destinationData);
            }

            // only an approximate destination is needed... it controls the lerping amount for the renderable
            if (destinationData != null)
            {
                entity.Locomotor.CurrentMoveTarget = destinationData.AccessPoint.Value;
            }
            else if (Destination.HasValue)
            {
                entity.Locomotor.CurrentMoveTarget = Destination.Value.ToVector3();
            }
            else
            {
                entity.Locomotor.CurrentMoveTarget = entity.PlaySiteLocation;
            }
        }


        private void ResetDestination(Vector3 destination)
        {

            Destination = new WorldLocation(destination);
            DestinationTilePos = MapManager.WorldPosToTilePos(destination);
         
        }

        private bool DecideOnVehicleOrGoByFoot(IKnownEntityData destinationData)
        {
            // see if we can get a vehicle:

            // commented out since a crash was reported in this method... and we don't even have vehicles yet.

          /*  if (entity.PersonEntity != null && ownersOfVehicles != null && Destination.HasValue) // only people may drive... and only to ground locations?
            {
                IKnownEntityData vehicleToUse;

                RegionMap.Result result = GetVehicleForPersonTransport(entity, DestinationTilePos.Value.ToPoint(), out vehicleToUse, ownersOfVehicles,
                    entityIntelligence.ProtectionLevel, entity.EntityType.ThreatCategory, entityIntelligence.ThreatStance);

                if (Status == Goals.Status.Failed)
                    return false;


                if (result == RegionMap.Result.Wait)
                {
                    //We are waiting. We must now do something to prevent being seen as completed. 
                    // test this...!
                    AddSubgoal(new GoalWait(entity));
                    isEvaluatingPersonalTransport = true;
                    return true;
                }

                if (vehicleToUse != null)
                {
                    EnterVehicleAndGo(vehicleToUse);

                }
                else
                {   
                    GetPath(pathStartLocation, destinationData);
                    // Hitch a ride:
                    //entityIntelligence.ListeningForTriggers[(int)TriggerTypes.DrivingVehicle] = true;
                }
            }
            else
            {*/
                // go there by foot. get a path:
                GetPath(pathStartLocation, destinationData);
           // }

            return true;
        }

        private void EnterVehicleAndGo(IKnownEntityData vehicleToUse)
        {
                     
            entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(vehicleToUse.EntityID, entity.EntityID);          

            UsedVehicle = vehicleToUse.EntityID;

            Vector3 entryLocation, seatLocation;

            PassengerOrCargoSlot driversSlot = vehicleToUse.GetFreeDriversSlot();
            driversSlot.GetEntryPoints(out entryLocation, out seatLocation);
            //vehicleToUse.Vehicle.GetEntryPoint(Vehicles.Place.Driver, out entryLocation, out seatLocation); 

            // move to vehicle:
            GoalMoveToPosition moveToVehicle =
                new GoalMoveToPosition(entity, entryLocation, ownersOfVehicles, VehicleUse.NoVehicle); // avoid infinite recursion

            AddSubgoal(moveToVehicle);

           // DrivingVehicle trigger = new DrivingVehicle(entity, TriggerPriority.Normal, 5, DestinationTilePos);
            AddSubgoal(new GoalEnterVehicleAsDriver(entity, vehicleToUse.EntityID, new Trigger(entity, null, GameData.Instance.AllTriggerTypes["drivenVehicle"])));

            GoByVehicle(vehicleToUse);
        }


        /// <summary>
        /// will set Status to Failed if the ownerID failed to resolve.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="to"></param>
        /// <param name="bestVehicle"></param>
        /// <param name="ownersOfVehicles"></param>
        /// <param name="protectionLevel"></param>
        /// <param name="threat"></param>
        /// <param name="approach"></param>
        /// <returns></returns>
        private RegionMap.Result GetVehicleForPersonTransport(Entity entity, Point to, out IKnownEntityData bestVehicle, List<EntityGroupID> ownersOfVehicles,
            ProtectionLevel protectionLevel, /*ThreatCategory threat,*/ ThreatStance approach)
        {
            bestVehicle = null;

            if (entity.PersonEntity != null && entity.PersonEntity.CanDrive())
            {

                RegionMap footRegionMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity).Layers[SurfaceType.TransportType.Foot].RegionMap; // UWGame.SimSide.Instance.Map.RegionMapManager.HumanFootExposedNormalRegionMap;
                RegionMap vehicleRegionMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity).Layers[SurfaceType.TransportType.OffRoad].RegionMap; // UWGame.SimSide.Instance.Map.RegionMapManager.HumanVehicleExposedNormalRegionMap;

                // UWGame.SimSide.Instance.Map.RegionMapManager.RegionMaps[TerrainType.TransportType.OffRoad];

                Point destinationSubtilePos = MapManager.TileCenterToSubTile(to);
                Vector3 destinationWorldLocation = MapManager.TileToWorldPos(to);

                Point entitySubtilePos = MapManager.WorldPosToSubtile(entity.AccessPoint.Value);  // #ACCESS .PlaySiteLocation);
                // pixels
                float distanceOnFoot = -1f; // = Common.DistanceOctile(entity.Location, MapManager.TileToWorldPos(to));
                RegionMap.Result result1 = footRegionMap.GetDistance(entity, entitySubtilePos, destinationSubtilePos, ref distanceOnFoot);

                if (result1 != RegionMap.Result.OK)
                {
                    return result1;
                }

                if (distanceOnFoot < GameData.Instance.AIConstants.ShortestDistanceToConsiderAVehicle)
                    return RegionMap.Result.OK; // disregard ultra short trips

                // time to get there on foot:
                double bestTime = distanceOnFoot / entity.Locomotor.CurrentMaximumSpeedNoTerrain;

                double travelTimeByFootScore = EvaluateHaulingJobs.ScoreTravelTime(bestTime);
                double comfortByFootScore = ScoreComfort(null);

                double bestScore = 0.6 * travelTimeByFootScore + 0.1 * comfortByFootScore + 0.3; // we will be comparing this against vehicles


                double time;
                //Vehicle vehicleComponent;

                float distanceToVehicle = 0f;
                float distanceFromVehicleDestination = 0f;

                double travelTimeByVehicleScore;

                float timeByVehicleOverTerrain, timeByFootToVehicle;

                Allegiances.Allegiance allegiance = entity.Intelligence.Allegiance;


                // pick the fastest vehicle:
                IKnownEntityData vehicleData;
                EntityID vehicleID;
                foreach (EntityGroupID ownerOfVehiclesID in ownersOfVehicles) // 1 or more sets of vehicles can be searched...
                {
                    EntityGroup ownerOfVehicles = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerOfVehiclesID);
                    if (ownerOfVehicles == null)
                    {
                        // this goal is invalid.
                        Status = Goals.Status.Failed;
                        return RegionMap.Result.NoAccess;
                    }

                    for (int i = ownerOfVehicles.Vehicles.Count - 1; i >= 0; i--)
                    {
                        vehicleID = ownerOfVehicles.Vehicles[i];

                        if (GoalEvaluator.HandleOwnerDataResult(allegiance.SharedKnowledge, vehicleID, ownerOfVehicles, out vehicleData))
                        {
                            //vehicle.Find(out vehicleComponent);

                            EntityID? inUseBy = entityIntelligence.Allegiance.SharedKnowledge.GetInUseBy(vehicleData.EntityID);
                            //TODO: TaskProcess
                            if (//vehicleComponent.DrivenBy == null && ?????
                                inUseBy == null && // vehicleComponent.TakenBy == null &&
                                // !The.Sim.IsKnownToBeDestroyed(allegiance, vehicle) && // NEW
                                Entity.IsFunctional(vehicleData) &&  //EvaluateHaulingJobs.ScoreIsEntityFunctional(vehicleData) > 0.0 && 
                                !The.Map.IsNoParkingSpot(vehicleData.EntityType, protectionLevel, entity.EntityType, approach, to))
                            {
                                // NEW:
                                Vector3 vehicleLocation = vehicleData.PlaySiteLocation; // The.Sim.GetKnownLocation(allegiance, vehicle);
                                Point vehicleSubtilePos = MapManager.WorldPosToSubtile(vehicleLocation);

                                // move to vehicle by foot:
                                RegionMap.Result result2 = footRegionMap.GetDistance(entity, entitySubtilePos, vehicleSubtilePos, ref distanceToVehicle);

                                if (result2 == RegionMap.Result.Wait)
                                {
                                    return result2;
                                }

                                if (result2 == RegionMap.Result.NoAccess)
                                {   // can't reach this vehicle - look at the next one:
                                    continue;
                                }

                                timeByFootToVehicle = distanceToVehicle / entity.Locomotor.CurrentMaximumSpeedNoTerrain;

                                VehicleContainerType vehicleContainerType = (VehicleContainerType)vehicleData.EntityType.ContainerType;
                                if (vehicleContainerType.Transport == SurfaceType.TransportType.Air)
                                {
                                    timeByVehicleOverTerrain = Common.DistanceOctile(vehicleLocation, destinationWorldLocation) / vehicleData.CurrentMaximumSpeed.Value; //  .Locomotor.CurrentMaximumSpeed;

                                    // aircraft take some time taking off and landing.     
                                    timeByVehicleOverTerrain += 2 * vehicleContainerType.Aircraft.EstimatedTakeOffLandingTime;
                                }
                                else
                                {
                                    // move to destination:
                                    RegionMap.Result result3 = footRegionMap.GetDistance(entity, vehicleSubtilePos, destinationSubtilePos, ref distanceFromVehicleDestination);

                                    if (result3 == RegionMap.Result.Wait)
                                    { // we will have to wait and start over...
                                        return result3;
                                    }

                                    if (result3 == RegionMap.Result.NoAccess)
                                    {   // can't reach this vehicle - look at the next one:
                                        continue;
                                    }

                                    timeByVehicleOverTerrain = distanceFromVehicleDestination / vehicleData.CurrentMaximumSpeed.Value;
                                }

                                time = timeByFootToVehicle + timeByVehicleOverTerrain;


                                travelTimeByVehicleScore = EvaluateHaulingJobs.ScoreTravelTime(time); // use time on foot...?

                                double vehicleConditionScore = EvaluateHaulingJobs.ScoreIsEntityFunctional(vehicleData);

                                //prefer no dual functions, no high loading etc...
                                double suitabilityScore = ScoreMainVehicleFunction(vehicleData.EntityType);

                                double comfortScore = ScoreComfort(vehicleData);

                                double score = 0.6 * travelTimeByVehicleScore + 0.1 * vehicleConditionScore + 0.15 * suitabilityScore + 0.1 * comfortScore + 0.05;

                                if (score > bestScore)
                                {
                                    bestVehicle = vehicleData;
                                    bestScore = score;
                                }                               
                            }
                        }
                    }
                }
            }

            return RegionMap.Result.OK;
        }

        private static double ScoreSafety()
        {
            // we feel safer in a vehicle...

            return 1;
        }

        private static double ScoreComfort(IKnownEntityData vehicle)
        {
            // it's nicer to drive...
            // score weather conditions etc.

            if (vehicle != null)
            {
                return 1;
            }
            else return 0.3;
        }

        private static double ScoreMainVehicleFunction(EntityType vehicle)
        {
            VehicleContainerType vehicleContainerType = (VehicleContainerType)vehicle.ContainerType;
            if (vehicleContainerType.MainFunction == VehicleContainerType.Function.PersonalTransport)
            {
                return 1.0;
            }
            else if (vehicleContainerType.MainFunction == VehicleContainerType.Function.Hauling)
            {
                return 0.4;
            }
            else return 0;

        }

        private void GoByVehicle(IKnownEntityData vehicleToUse)
        {
            //  Vector2 parkAtOffset;
            //  Point parkAtTile;
            Vector3 parkAtLocation;
            Rectangle parkingArea;

            parkingArea = Vehicle.GetSurroundingAreaUsingEntityRadius(vehicleToUse, DestinationTilePos.Value.ToPoint());

            // NEW:
            // we use the vehicle's transport type instead of the driver's.
            if (Vehicle.FindParkingSpot(vehicleToUse.MapPosition.Value, entity.Intelligence.Allegiance, vehicleToUse, parkingArea, //targetBuilding.GetSurroundingAreaUsingEntityRadius(vehicleToUse),
                ((VehicleContainerType)vehicleToUse.EntityType.ContainerType).Transport, entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance,
                DestinationTilePos.Value.ToPoint(), out parkAtLocation)) // out parkAtTile, out parkAtOffset))
            {
                // Nesting.IsNested: To make sure we don't end here again in an infinite recursion!
                AddSubgoal(new GoalMoveToPosition(entity, parkAtLocation, Nesting.IsNested, ownersOfVehicles));

                // is this necessary:
                AddSubgoal(new GoalExitVehicle(entity, vehicleToUse.EntityID));


                // walk the last bit: this is not wanted when we are unloading from a vehicle. The character will walk to here, and then turn right around when Unload unfolds...
                // but for GoalDoHarvest, it is needed!
                if (IsFinalDestination)
                {
                    AddSubgoal(new GoalMoveToPosition(entity, Destination.Value.ToVector3(), ownersOfVehicles, VehicleUse.NoVehicle));
                }

            }
            else
            {
                // we cannot park here.
                // TODO: Find a way to check for this situation before selecting a vehicle!!!

            }

            /* OLD:
            Buildings.Structure targetBuilding = UWGame.SimSide.Instance.map.TileMap[Destination.X, Destination.Y].StructureOnTile;
                       
            // TODO: Could it be that there are also other destinations 
            // where we want to park away from and then walk to?
            if (targetBuilding != null)
            {
                //park outside...
                Vector2 parkAtOffset;
                Point parkAtTile;

                // we use the vehicle's transport type instead of the driver's.
                if (Vehicle.FindParkingSpot(UsedVehicle.MapPosition, UsedVehicle, targetBuilding.GetSurroundingAreaUsingEntityRadius(vehicleToUse),
                    vehicleToUse.EntityType.VehicleType.Transport, entity.ProtectionLevel, entity.EntityType.ThreatCategory, entity.ThreatStance,
                    Destination, out parkAtTile, out parkAtOffset))
                {
                    // Nesting.IsNested: To make sure we don't end here again in an infinite recursion!
                    AddSubgoal(new GoalMoveToPosition(entity, parkAtTile, parkAtOffset, Nesting.IsNested, ownersOfVehicles));

                    // is this necessary:
                    AddSubgoal(new GoalExitVehicle(entity, vehicleToUse));

                    if (parkAtTile != Destination)
                    { // walk the last bit if necessary
                        AddSubgoal(new GoalMoveToPosition(entity, Destination, VehicleUse.NoVehicle, ownersOfVehicles));
                    }
                }
                else
                {
                    // we cannot park here.
                    // TODO: Find a way to check for this situation before selecting a vehicle!!!

                }

            }
            else
            {
                // go directly to destination tile:          
                // Nesting.IsNested: To make sure we don't end here again in an infinite recursion!

                // Find parking spot ??? For example when loading items from the ground
                AddSubgoal(new GoalMoveToPosition(entity, Destination, Nesting.IsNested, ownersOfVehicles));
                AddSubgoal(new GoalExitVehicle(entity, vehicleToUse));

            }*/
        }





        /// <summary>
        /// destinationData can be null if we are going to a location
        /// </summary>
        /// <param name="from"></param>
        /// <param name="destinationData"></param>
        private void GetPath(Vector3 from, IKnownEntityData destinationData)
        {
            // See if we have to break this movement down further. Are we going into a building?
            
            // should the goal be given more parameters to control this behaviour...?

            bool destinationIsABuildingWeCanEnter = false;
            bool destinationIsAContainerWeCanTransactWith = false;
            if (destinationData != null && destinationData.EntityType.ContainerType != null) 
            {
                if (destinationData.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
                {
                    destinationIsABuildingWeCanEnter = true;
                }
                if (destinationData.EntityType.ContainerType.CanTransactWithContainer(entity.EntityType))
                {
                    destinationIsAContainerWeCanTransactWith = true;
                }
                else if (destinationData.EntityType.ContainerType is AgentStorageType && attackType == null)// This could be improbved to something more precise
                {    // fail if the destination entity is carried by an intelligent agent (we never unload from people; they should be told to drop the item)
                                   
                    Status = Goals.Status.Failed;
                    return;
                }
            }

            bool destinationIsInsideABuildingWeCanEnter = false;
            if (destinationData != null && destinationData.ContainedBy != null)                   
            {
                IKnownEntityData containingBuilding = null;

                EntityResult result = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(destinationData.ContainedBy.Value, out containingBuilding);
                if (EntityResultCausesFailedGoal(result))
                {
                    return;
                }
              
                // is the destination an entity INSIDE a building that we can enter?
                if (containingBuilding.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
                {
                    destinationIsInsideABuildingWeCanEnter = true;
                }
            }


            //!Common.IsEqual(destinationData.Location, destinationData.AccessPoint)

            // is this a tree or building...? then convert the move order to the access point or entrance.
            if (destinationData != null
                && (destinationIsABuildingWeCanEnter ||
                    destinationIsInsideABuildingWeCanEnter ||
                   //  ||
                    !Common.IsLocationEqual(destinationData.PlaySiteLocation, destinationData.AccessPoint.Value))) // && destinationData.EntityType.HasSpecialAccessPoint() 
            {

                AddSubgoal(new GoalMoveToPosition(entity, destinationData.AccessPoint.Value, null, GoalMoveToPosition.VehicleUse.NoVehicle));
                //   AddSubgoal(new GoalTurnToFace(entity, destinationData.EntityID));


                if (destinationIsABuildingWeCanEnter)
                {
                    // GoalEnter will pick an entrance...
                    AddSubgoal(new GoalEnter(entity, DestinationEntity.Value));
                }
                else if (destinationIsInsideABuildingWeCanEnter)
                {
                    AddSubgoal(new GoalEnter(entity, destinationData.ContainedBy.Value));
                }                
            }
            else if (Destination.HasValue || destinationIsAContainerWeCanTransactWith)
            {
                // If we do not have a destination but instead a container we can transact with:                
                if (Destination == null && destinationIsAContainerWeCanTransactWith)
                {
                    Destination = new WorldLocation(destinationData.AccessPoint.Value);
                }

                if (Common.DistanceOctile(from, Destination.Value) < 210f) // 96f) 
                {
                    // get the short path directly:
                    MovementMap moveMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel,
                        entity.EntityType, entityIntelligence.ThreatStance);

                    List<AI.Pathfinding.PathFinderNode> path = entityIntelligence.PathPlanner.FindShortPathDirectly(moveMap.Layers[entity.GetTransportType()], 
                          from, Destination.Value.ToVector3(), 2000);

                    if (path != null)
                    {
                        FollowNewPath(path);
                    }
                    else
                    {   // no path found:
                      
                        Status = Status.Failed;
                    }

                    return;
                }
                else
                {
                    RequestPath(entity.GetTransportType(), from);
                }
            }
            else
            {
                if (entity.ToString().Contains("August"))
                {

                }

                // something is wrong...
             //   Debug.Assert(false, "This should not happen.");
                Status = Goals.Status.Failed;
            }
        }

        private void FollowNewPath(List<AI.Pathfinding.PathFinderNode> path)
        {
            // used in repathing 
            // in one case this was called on an Inactive goal, Destination was null in that case. The request must have come from another goal...
            // NEW: just happened with an Active goal, Destination was null...

            // before terminating current moves in order to follow the new path, make sure we don't stop (it looks wrong)

            foreach (Goal subgoal in Subgoals)
            {
                SetDontStopFlagOnMoveGoals(subgoal);
            }

            //clear any existing goals
            RemoveAllSubgoals(); // works???
           
            Entity insideBuilding;
            if (!entity.GetContainedBy(out insideBuilding))
            {
                Status = Goals.Status.Failed;
                return;
            }

            // get up...
            if (entity.HasStance())
            {
                ChangeStance(entity.EntityType.LocomotorType.StancesType.MovingStanceType); // entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.MovingStance, entity.EntityType.LocomotorType.StancesType.DefaultStanceType));
                                                //Entities.Locomotors.LeggedLocomotor.Stance.Standing);
            }

            if (insideBuilding != null)
            { // move out...
                AddSubgoal(new GoalExit(entity));
                pathStartLocation = insideBuilding.AccessPoint.Value;
            }

            AddSubgoal(
                new GoalFollowPath(entity, path, this, ownersOfVehicles, Destination.Value.ToVector3(), GroupMoveActivity) 
                { PermittedDistanceSquaredToDestination = this.PermittedDistanceSquaredToDestination });


            UpdateMoveTargetForLerping();
        }

        /*   private float GetPathLength(List<AI.Pathfinding.PathFinderNode> path)
           {
               foreach (var item in path)
               {
                   item.
               }
           }*/

        private void SetDontStopFlagOnMoveGoals(Goal subgoal)
        {
            GoalTraverseEdgeBetweenWaypoints traverseEdge = subgoal as GoalTraverseEdgeBetweenWaypoints;
            if (traverseEdge != null)
            {
                traverseEdge.StopMovingWhenTerminating = false;
            }
            else
            {
                CompositeGoal compositeGoal = subgoal as CompositeGoal;
                if (compositeGoal != null)
                {
                    foreach (Goal subsubgoal in compositeGoal.Subgoals)
                    {
                        SetDontStopFlagOnMoveGoals(subsubgoal);
                    }
                }
            }
        }


        private bool GetFollowerPathToWaypoint(Vector3 from)
        {
            //   Point waypointTile = MapManager.WorldPosToTile(entityIntelligence.GroupMoveAssignedWaypoint.Value.Location);
            //   Vector2 offset = MapManager.WorldPosToPositionWithinTile(entityIntelligence.GroupMoveAssignedWaypoint.Value.Location);
            Vector3 waypointLocation = entityIntelligence.GroupMoveAssignedWaypoint.Location;

            // get path directly:
            MovementMap moveMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel,
                entity.EntityType, entityIntelligence.ThreatStance);
            List<AI.Pathfinding.PathFinderNode> path = entityIntelligence.PathPlanner.FindShortPathDirectly(moveMap.Layers[entity.GetTransportType()], //MapCosts,                 
                from, waypointLocation, 10000);

            if (path != null)
            {
                AddSubgoal(new GoalFollowPath(entity, path, this, ownersOfVehicles, waypointLocation, GroupMoveActivity));
                // tell the leader to wait up:

                // Test this! Does it work?
                //    GroupMoveActivity.Leader.HandleMessage(new Message(entity, Message.MessageTypes.WaitForMeGroup, null));

                entityIntelligence.FollowerStatus = FollowerStatus.IsFollowingPathToWaypoint;
                //entity.GroupMoveIsCatchingUp = true;
                return true;
            }
            else
            {   // no path found:
               // The.Map.RegisterBlockedPath(moveMap, entity.GetTransportType(), from, waypointLocation);
                Status = Status.Failed;

                return false;
            }

        }

        private void RequestPath(SurfaceType.TransportType transportType, Vector3 start) 
        {
            //requests a path to the target position from the path planner. 
            MovementMap moveMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel,
                    entity.EntityType, entityIntelligence.ThreatStance);

            
            SubtileLayers layers = moveMap.Layers[transportType];

            // NEW: get a high level path, if available:    
            if (entityIntelligence.PathPlanner.FindPathByRequest(layers, start, Destination.Value.ToVector3())) 
            {
                /*We are waiting. We must now do something to prevent being seen as completed. */
                //ChangeStance(Entities.Locomotors.LeggedLocomotor.Stance.Standing); // NEW: change stance first; GoalWait will blend to standing Idle if we are sitting down.
                AddSubgoal(new GoalWait(entity));

                The.Sim.AddWaitingAgent(entity, Sim.WaitingFor.Path);
            }
            else
            {   // no path found...
                //The.Map.RegisterBlockedPath(moveMap, transportType, start, Destination.Value.ToVector3());
                Status = Status.Failed;
            }

        }

        private void ActivateGroupFollower(Vector3 from)
        {
            if (entityIntelligence.GroupMoveAssignedWaypoint != null)
            {
                StartGroupFollowerMoving(from);
            }
            else
            {
                /*We are waiting. We must now do something to prevent being seen as completed. */
                AddSubgoal(new GoalWait(entity));
            }
        }

        /// <summary>
        /// The follower has a waypoint to move to. See if he can move there directly, or if he needs a path.
        /// </summary>
        private void StartGroupFollowerMoving(Vector3 from) //Point from)
        {
            entityIntelligence.IsAtGroupMoveDestination = false;

            MovementMap moveMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);
            SurfaceType.TransportType transport = entity.GetTransportType();

            if (MapManager.IsPathClearToPoint(entity.PlaySiteLocation, entityIntelligence.GroupMoveAssignedWaypoint.Location, moveMap, transport))
            {
                AddSubgoal(new GoalTraverseEdgeBetweenWaypoints(entity, entityIntelligence.GroupMoveAssignedWaypoint.Location, null, entityIntelligence.GroupMoveAssignedWaypointRay.Value, entityIntelligence.GroupMoveAssignedWaypoint.Number, entityIntelligence.GroupMoveAssignedWaypoint.IsLastWaypoint, false, ownersOfVehicles, GroupMoveActivity));

            }
            else
            {
                // add the GoalFollow:
                if (GetFollowerPathToWaypoint(from))
                {   // tack the lsat waypoint on the end - we need the Number and IsLastEdge information to detect the final destination
                    AddSubgoal(new GoalTraverseEdgeBetweenWaypoints(entity, entityIntelligence.GroupMoveAssignedWaypoint.Location, null, entityIntelligence.GroupMoveAssignedWaypointRay.Value, entityIntelligence.GroupMoveAssignedWaypoint.Number, entityIntelligence.GroupMoveAssignedWaypoint.IsLastWaypoint, false, ownersOfVehicles, GroupMoveActivity));
                }
            }

        }



        /// <summary>
        /// we want to repath more often as we get closer to the target
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        private TimeSpan GetTimeBetweenRepaths(float distance)
        {
            if (distance < 70f) // close chase
            {
                return new TimeSpan(0, 0, 0, 0, 500);
            }
            else if (distance < 200f) // 5 tiles
            {
                return new TimeSpan(0, 0, 2);
            }
            else if (distance < 400f)
            {
                return new TimeSpan(0, 0, 4);
            }
            else if (distance < 1000f)
            {
                return new TimeSpan(0, 0, 8);
            }
            else return new TimeSpan(0, 0, 12);

        }


        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
          /*  ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                
                //process the subgoals

                Status = ProcessSubgoals(elapsed);

                //if any of the subgoals have failed then this goal re-plans
               
                if (Status == Status.Failed)
                {
                   
                    // the goal can also fail in HandleMessage (PathNotFound)!
                    // how do we handle this???

                    noOfFailures++;

                    if (!hasTerminated  // <- the goal has been substituted by ArbitrateWhileBusy() and should finish NOW
                        && noOfFailures < 3) 
                    {
                        Activate();
                    }
                }               
                else
                { 
                    // Are we trying to close range on a (moving) Entity?
                    if (attackType != null)
                    {
                        // if we are within range, then stop! and attack.

                        IKnownEntityData targetData;
                        EntityResult result = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(DestinationEntity.Value, out targetData);

                        if (!EntityResultCausesFailedGoal(result))
                        {
                            bool isSeen = (result == EntityResult.SeenDirectly);

                            if (IsWithinRangeOfEntity(targetData, isSeen))
                            {
                                Status = Goals.Status.Completed;
                            }
                            else
                            {
                                HandlePeriodicRepathing(targetData, isSeen);
                            }
                        }
                    }
                }
           // }



            if (Status == Status.Completed && GroupMoveActivity != null && !GroupMoveActivity.HasEnded)
            {   // Group behaviour!
                PerformGroupBehaviour();
            }

      

            /*ExitIfFailedOrCompleted();

            return Status;*/

        }



        private bool IsRangedAttack()
        {
            return attackType.RangeType != AttackType.RangeTypes.Melee;
        }

        private bool IsRangedAttackWithinRange(bool isSeen, IKnownEntityData targetData)
        {
            Vector3 targetLocation = targetData.PlaySiteLocation;

            if (isSeen)
            {
                Entity seenEntity = (Entity)targetData;

                if (IsTargetEntityMoving(seenEntity))
                {
                    // do some crude reckoning of the target's future location
                    // TODO MLo this is just three times the move speed in the facing direction
                    Vector3 lookAhead = seenEntity.Locomotor.MoveSpeed * seenEntity.FacingNormal; // distance moved in 1 second...
                    targetLocation += lookAhead;

                }


                Vector3 offset = entity.PlaySiteLocation - targetLocation;
                float distance = offset.LengthSquared();

                if (distance < attackType.MaxRangeSquared)
                {
                    return true;
                }
            }

            return false;

        }

        private bool IsWithinRangeOfEntity(IKnownEntityData targetData, bool isSeen)
        {
            /*  if (DestinationEntity == null)
                  return false;*/



            if (IsRangedAttack())
            {
                return IsRangedAttackWithinRange(isSeen, targetData);
            }
            else
            {
                // melee attack

                if (isSeen) // do we see our target?
                {
                    Entity seenEntity = (Entity)targetData; // ok to cast!

                    //Only allow this range test if the target is moving, or if we are going directly to the target position! 
                    //Otherwise, we may never get to our melee position. 
                    if (IsTargetEntityMoving(seenEntity) || IsChasingTargetCenterLocation)
                    {
                        return AttackJob.IsCorrectMeleeDistanceRoundedToSubtiles(entity, seenEntity);
                    }
                }

                return false;
            }

        }

        private bool IsTargetEntityMoving(Entity e)
        {            
            return e.Locomotor.IsMoving();
        }


        private void HandlePeriodicRepathing(IKnownEntityData targetData, bool isSeen)
        {
            // see if it is time to repath:
            if (repathRegulator.IsReady())
            {
                // see if the target moved:
                Vector3 currentLocation = targetData.PlaySiteLocation; 
                if (currentLocation != lastTargetLocation)
                {
                    lastTargetLocation = currentLocation;

                    float distance = Common.DistanceOctile(currentLocation, entity.PlaySiteLocation);
                    if (The.Sim.TotalUnPausedGameTime.Subtract(timePointForLastRepath).CompareTo(GetTimeBetweenRepaths(distance)) > 0)
                    {
                        timePointForLastRepath = The.Sim.TotalUnPausedGameTime;

                        // find the new location to path to
                        Vector3? destination = null;
                        if (distance < 350f && useMeleeLocationAsDestination == true) //getDestinationEntityLocation != null) // only compute the offset location if we are close enough to matter.
                        {
                            //alternately, choose a ranged path destination, here

                            if (!CombatInfo.GetMeleeLocation(entity, targetData, out destination, out IsChasingTargetCenterLocation))
                                //!getDestinationEntityLocation(entity, targetData, out destination, out IsChasingTargetCenterLocation))
                            {
                                Status = Status.Failed; // we failed...
                                return;
                            }
                        }
                        else
                        {
                            destination = targetData.Location;
                        }

                        ResetDestination(destination.Value);

                        // continue moving along current path (keep subgoals) but request a new path:                    
                        ActivateAndGetPath();
                    }
                }
            }
        }

        private void PerformGroupBehaviour()
        {
            // test group status - are everyone at their destinations?
            if (GroupMoveActivity.AllAreReady())
            {
                GroupMoveActivity.HasEnded = true;

                // send the message: We are done!!!
                GroupMoveActivity.SendMessageToEveryoneElse(entity, new Message(Message.MessageTypes.EndGroupMovement));
                /*   foreach (Entity member in GroupMoveActivity.Members)
                   { 
                       member.HandleMessage(new Message(Message.MessageTypes.EndGroupMovement));
                   }*/
            }
            else
            {
                if (Subgoals.Count > 0)
                {
                    GoalTraverseEdgeBetweenWaypoints oldGoal = Subgoals.Peek() as GoalTraverseEdgeBetweenWaypoints;
                    if (oldGoal != null && entityIntelligence.GroupMoveAssignedWaypoint != null
                        && oldGoal.Number < entityIntelligence.GroupMoveAssignedWaypoint.Number)
                    {
                        Status = Status.Active;
                        // we already have a new waypoint:
                        StartGroupFollowerMoving(entity.AccessPoint.Value); // #ACCESS .PlaySiteLocation);
                        return;
                    }
                }
                // wait for the next waypoint / rest of the group to reach destination...
                // still active:
                Status = Status.Active;
                AddSubgoal(new GoalWait(entity));
            }

        }

        public override void Deactivate()
        {
            
            // no longer hitching:
            //entityIntelligence.ListeningForTriggers[(int)TriggerTypes.DrivingVehicle] = false;


            entity.Locomotor.MoveSpeed = 0f; // ?? is this the best place?

            // let other people use the vehicle now:
            if (UsedVehicle != null && VehicleUseByGoal == VehicleUse.FreeUpAfterUse)
            {
                IKnownEntityData vehicle;
                entityIntelligence.GetKnownData(UsedVehicle.Value, out vehicle);
                if (vehicle != null)
                {                  
                    entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(vehicle.EntityID, entity.EntityID);                     
                }
            }
        }

        public void RegisterPathDoneSubscriber(Action<float> method, IIDEventSubscriber subscriber, out MethodID? methodID)
        {
            if (RepathDoneEvent == null)
                RepathDoneEvent = new IDActionEvent<float>();

            RepathDoneEvent.AddAndRegister(method, subscriber, out methodID); 

        }

        public override bool HandleMessage(Message message)
        {
            // should Messages be handled by Inactive goals?? does not seem right

            //first, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            //if the msg was not handled, test to see if this goal can handle it
            if (handled == false)
            {
                switch (message.MessageType)
                {
                    case Message.MessageTypes.PathFound:
                                                
                        // will crash if Destination is null!
                        if (Destination.HasValue)
                        {
                            List<PathFinderNode> newPath = entityIntelligence.PathPlanner.Path;
                            FollowNewPath(newPath);

                            if (RepathDoneEvent != null)
                            {
                                RepathDoneEvent.Invoke(newPath.Count * MapManager.subTileSize); // approximate length of path
                            }
                        }
                        else
                        { 
                            Status = Goals.Status.Failed; // NEW: This will allow the agent to retry later... 
#if !RELEASE
                            throw new Exception("Received a path when no Destination has been set?? Is it an old path???"); //where did the message come from? An old request?
#endif
                           
                        }

                        return true; //msg handled

                    case Message.MessageTypes.PathNotFound:
                        // register as blocked for the next 5 seconds or so:
                      /*  The.Map.RegisterBlockedPath(entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel,
                            entity.EntityType.ThreatCategory, entityIntelligence.ThreatStance), entity.GetTransportType(),
                            entity.Location, Destination.Value.ToVector3());*/

                        Status = Status.Failed;

                        return true; //msg handled
                    case Message.MessageTypes.DistanceFound:
                    case Message.MessageTypes.DistanceFoundNoAccess:
                        if (isEvaluatingPersonalTransport)
                        {
                            isEvaluatingPersonalTransport = false;

                            // clear any Wait goal:
                            RemoveAllSubgoals();

                            IKnownEntityData destinationData = null;
                            if (DestinationEntity.HasValue)
                            {
                                EntityResult result = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(DestinationEntity.Value, out destinationData);
                                if (EntityResultCausesFailedGoal(result))
                                {
                                    return true;
                                }
                            }

                            DecideOnVehicleOrGoByFoot(destinationData);

                            return true;
                        }

                        return false;

                    case Message.MessageTypes.StartGroupMovement:
                        if (GroupMoveActivity != null && GroupMoveActivity.IsFollower(entity))
                        {
                            //  entity.IsAtGroupMoveDestination = false;

                            // Move out!

                            // clear any Wait goal:
                            RemoveAllSubgoals();

                            StartGroupFollowerMoving(entity.AccessPoint.Value); // #ACCESS .PlaySiteLocation);
                            //    AddSubgoal(new GoalTraverseEdgeBetweenWaypoints(entity, entity.GroupMoveAssignedWaypoint.Value.Position, entity.GroupMoveAssignedWaypointRay.Value, entity.GroupMoveAssignedWaypoint.Value.Number, entity.GroupMoveAssignedWaypoint.Value.IsLastWaypoint, false, ownersOfVehicles, GroupMoveActivity));

                        }
                        return true;
                    case Message.MessageTypes.EndGroupMovement:
                        if (GroupMoveActivity != null) // && GroupMoveActivity.IsFollower(entity))
                        {
                            //RemoveAllSubgoals();

                            // we're done!
                            Status = Status.Completed;

                        }
                        return true;

                    default: return false;
                }
            }

            //handled by subgoals
            return true;
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

            this.Destination = sn.DoWorldLocationNullable(Destination);
            this.DestinationTilePos = sn.DoTilePosNullable(DestinationTilePos);
            this.DestinationEntity = sn.DoEntityIDNullable(DestinationEntity);
            this.PermittedDistanceSquaredToDestination = sn.DoFloat(PermittedDistanceSquaredToDestination);
            this.attackType = sn.DoGameData(attackType);

          
            this.IsChasingTargetCenterLocation = sn.DoBool(IsChasingTargetCenterLocation);
            this.IsFinalDestination = sn.DoBool(IsFinalDestination);
            this.nesting = sn.DoEnum(nesting);
            this.UsedVehicle = sn.DoEntityIDNullable(UsedVehicle);
            this.VehicleUseByGoal = sn.DoEnum(VehicleUseByGoal);
           // sn.DoObject_______NotYetSupported_______(GroupMoveActivity); //TODO
            this.noOfFailures = sn.DoInt32(noOfFailures);
            this.pathStartLocation = sn.DoVector3(pathStartLocation);
            this.isEvaluatingPersonalTransport = sn.DoBool(isEvaluatingPersonalTransport);       
            this.lastTargetLocation = sn.DoVector3(lastTargetLocation);
            this.timePointForLastRepath = sn.DoTimeSpan(timePointForLastRepath);
            this.useMeleeLocationAsDestination = sn.DoBool(useMeleeLocationAsDestination);

            this.RepathDoneEvent = (IDActionEvent<float>)sn.DoISnapshot(RepathDoneEvent);
           

            sn.Ignore(repathRegulator);
            sn.Ignore(GroupMoveActivity);

            return this;
        }

        #endregion
    }
}
