using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Maps;
using GameStateManagement;
using UWGame.ClientSide.Renderables;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// only activate this goal when we have reached the edge of the area we want searched.
    /// </summary>
    class GoalSearchArea : CompositeGoal
    {
        
        private Zone zone;
        ZoneID snapshotZone;
      
        private List<Vector2> locationsStillToVisit;
      //  private List<Vector2> visitedPoints;
        //private int locationIndex;
        private float lowestDistance;
        private Vector2 closestLocation;
        private int currentDestinationIndex;
        private float distance = 0f;
        private bool isStealthy, examine;
        private int numberOfVisitedLocations;


        public GoalSearchArea(Entity owner, Zone mapArea, List<EntityGroupID> ownersOfVehicles, bool isStealthy, bool examine, float sampleDistance = 100f)
            : base(owner)
        {
            this.ownersOfVehicles = ownersOfVehicles;
            this.sampleDistance = sampleDistance;
            //   this.location = new Point(mapArea.BottomLeftTile.X, mapArea.BottomLeftTile.Y);
            this.zone = mapArea;

            // move to activate
            // remove points outside coverage
            //retry x times if result is 0
            // else fail goal


            //pointsOfDisc = UniformPoissonDiskSampler.SampleCircle(new Vector2( centerPos.X, centerPos.Y ), 100f, 50f, 5);

            //locationIndex = 0;
            lowestDistance = float.MaxValue;

            this.isStealthy = isStealthy;
            this.examine = examine;
        }

  

        public GoalSearchArea()
        {
        }


        protected override void Activate()
        {
            Status = Status.Active;

            if (isStealthy)
            {
                UpdateSneaking();
            }

          //  visitedPoints = new List<Vector2>();

            ComputeLocationsToVisit();

            RemoveAllSubgoals();
        }

        private bool FindClosestLocation()
        {
            Point entitySubtilePos = MapManager.WorldPosToSubtile(entity.AccessPoint.Value); // #ACCESS.PlaySiteLocation);
            RegionMap map = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity).Layers[Maps.SurfaceType.TransportType.Foot].RegionMap;

            //This loop tries to find the closest location and sets it to the current location if it is walkable
            int index;
            for (index = 0; index < locationsStillToVisit.Count; index++)
            {
                RegionMap.Result result =
                    map.GetDistance(entity, entitySubtilePos, MapManager.WorldPosToSubtile(locationsStillToVisit[index]),                   
                     ref distance);

                if (result == RegionMap.Result.OK)
                {
                    if (distance < lowestDistance)
                    {
                        lowestDistance = distance;
                        closestLocation = locationsStillToVisit[index];
                        currentDestinationIndex = index;
                    }
                }
                else if (result == RegionMap.Result.NoAccess)
                {
                    locationsStillToVisit.RemoveAt(index);
                    index--;
                }
                else if (result == RegionMap.Result.Wait)
                {
                    AddSubgoal(new GoalWait(entity));//If the locations cannot be provided right now we wait for them to be provided

                    waitingForDistance = true;

                    return false;
                }   
            }
            return true;
        }

        private float sampleDistance = 100f;
        private void ComputeLocationsToVisit()
        {
            int retries = 0;
            while ((locationsStillToVisit == null || locationsStillToVisit.Count == 0) && retries < 3)
            {
                Rectangle? mapAreaBoundingTiles = zone.MapArea.GetBoundingBoxInTiles();

                if (mapAreaBoundingTiles != null)
                {
                    //Rectangle boundingTiles = (Rectangle)mapAreaBoundingTiles;

                    locationsStillToVisit = UniformPoissonDiskSampler.SampleRectangle(MapManager.TileToWorldPos(mapAreaBoundingTiles.Value.Location).ToVector2() //boundingBox.Left, boundingBox.Top)
                        , MapManager.TileToWorldPos(new Point(mapAreaBoundingTiles.Value.Right - 1, mapAreaBoundingTiles.Value.Bottom - 1)).ToVector2(), sampleDistance); // make distance into parameter. in GoalPatrol, after first time round, increase by 50%-100%

                    for (int i = 0; i < locationsStillToVisit.Count; i++)
                    {
                        Vector2 currentLocation = locationsStillToVisit[i];
                        bool remove = true;

                        Point currentTilePos = MapManager.WorldPosToTile(currentLocation);

                        // remove locations outside the designated area
                        zone.MapArea.IterateAreaBreakOnTrue(tile =>
                        {
                            if (currentTilePos.X == tile.X && currentTilePos.Y == tile.Y)
                            { // this location is OK - break area iteration and look at next location
                                remove = false;
                                return true;
                            }
                            else
                            {
                                // continue iterating the area
                                return false;
                            }

                            //if (MapManager.WorldPosToTile(currentLocation))
                            /* if (tile.X < currentLocation.X && tile.X + 1 > currentLocation.X && tile.Y < currentLocation.Y && tile.Y + 1 > currentLocation.Y)
                                 remove = false;

                             return !remove;*/
                        });

                        if (remove)
                            locationsStillToVisit.RemoveAt(i);
                    }
                }
                retries++;
            }

            if (locationsStillToVisit == null || locationsStillToVisit.Count == 0)
            {
                // failed to find points...
                Status = Goals.Status.Completed; // Lars: I changed this to Completed after an agent repeatedly failed to compute points in single tile he was standing in. 
                                                 //Since the agent is standing on the edge of the area, I think it makes sense...
                //Status = Goals.Status.Failed; 
            }
        }


        public override Goal.DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            
            if (!requiresExamineAction || examine)
            {
                return DetectionFactor.DetectGood;
            }
            else return DetectionFactor.CannotDetect;
        }

        public override Goal.DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
        {
            
            if (!requiresExamineAction || examine)
            {
                return DetectionFactor.DetectGood;
            }
            else return DetectionFactor.CannotDetect;
        }



        protected override bool ArePreconditionsOK()
        {
            return true;
        }

        public override string GetStatus()
        {
            return "Searching area";
        }

        /*
        /// <summary>
        /// in seconds!!
        /// scouting/hunting:
        /// </summary>         
        public static float MinimumChanceToStopAndLookWhenSearching = 0.1f;       
        public static float ChanceToStopAndLookWhenSearchingFactor = 1f;       

        public static float TimeToWaitWhenStoppedAndSearchingMean = 8f; //3f
        public static float TimeToWaitWhenStoppedAndSearchingStdDev = 1f;

        // foraging:
        public static float MinimumChanceToStopAndLookWhenSearchingResources = 0.1f;
        public static float ChanceToStopAndLookWhenSearchingResourcesFactor = 1f;

        public static float TimeToWaitWhenStoppedAndSearchingResourcesMean = 3f;
        public static float TimeToWaitWhenStoppedAndSearchingResourcesStdDev = 1f;
        */

        private bool LocationsAreLeft()
        {
            if(locationsStillToVisit.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override void CreateRegulators()
        {
            base.CreateRegulators();
            setSneakingRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1, "GoalSearchAreaSneaking");
        }

        Regulator setSneakingRegulator;

        protected override void ProcessWhileActive(GameTime elapsed)
        {
        /*    ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                if (!ArePreconditionsOK())
                {
                    Status = Goals.Status.Failed;                  
                    return; // return Status;
                }
                else
                {

                    if (isStealthy)
                    {
                        if (setSneakingRegulator.IsReady())
                        {
                            // don't sneak around other people, it looks daft
                            UpdateSneaking();
                        }
                    }

                    // if we have reached the current waypoint, get the next target:
                    if (this.Subgoals.Count == 0)
                    {
                        if (FindClosestLocation() == false)
                        {
                         //   return Goals.Status.Active;

                            Status = Goals.Status.Active;
                            return;
                        }
                       
                        // if we cannot reach the remaining points, consider the goal failed unless we visited at least 1 (or more?) spot...
                        if (LocationsAreLeft() == false)
                        {
                            if (numberOfVisitedLocations > 0)
                            {
                                Status = Goals.Status.Completed;
                            }
                            else
                            {
                                Status = Goals.Status.Failed;
                            }
                        }
                        else
                        {
                            // have a chance to stop and look around in this spot (the chance is greater the longer we have to turn to move to the next waypoint)         
                            CheckToStopAndLookAround();

                            // then go on
                            AddSubgoal(new GoalMoveToPosition(entity,
                                closestLocation.ToVector3(), ownersOfVehicles));
                        }
                    }

                    if (Status != Goals.Status.Failed)
                    {
                        Status = ProcessSubgoals(elapsed);

                        if (Status == Goals.Status.Completed)
                        {
                            if (currentDestinationIndex < locationsStillToVisit.Count)
                            {
                                locationsStillToVisit.RemoveAt(currentDestinationIndex);
                            }

                            //locationIndex = 0;
                            lowestDistance = float.MaxValue;
                            numberOfVisitedLocations++;
                            if (locationsStillToVisit.Count != 0)
                            {
                                Status = Goals.Status.Active;
                            }
                        }
                    }

                }
         /*   }

            ExitIfFailedOrCompleted();

            return Status;*/
        }

        private void UpdateSneaking()
        {
            
                if (GoalHunt.OtherActiveAllegianceMembersAreStandingNearby(entity, entityIntelligence, 90f))
                {
                    entity.SetSneaking(false);
                }
                else
                {
                    entity.SetSneaking(true);
                }
           
        }

        /// <summary>
        /// when patrolling the second time around, take longer and more pauses
        /// </summary>
        private void CheckToStopAndLookAround()
        {
            // have a chance to stop and look around in this spot (the chance is greater the longer we have to turn to move to the next waypoint)
            Vector2 nextWaypointVector = closestLocation - entity.PlaySiteLocation.ToVector2();
            nextWaypointVector.Normalize();

            float chanceToStopAndLookWhenSearchingFactor, minimumChanceToStopAndLookWhenSearching, timeToWaitWhenStoppedAndSearchingMean, timeToWaitWhenStoppedAndSearchingStdDev;

            if (examine)
            {
                chanceToStopAndLookWhenSearchingFactor = GameData.Instance.AIConstants.ChanceToStopAndLookWhenSearchingResourcesFactor;
                minimumChanceToStopAndLookWhenSearching = GameData.Instance.AIConstants.MinimumChanceToStopAndLookWhenSearchingResources;
                timeToWaitWhenStoppedAndSearchingMean = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingResourcesMean;
                timeToWaitWhenStoppedAndSearchingStdDev = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingResourcesStdDev;
            }
            else
            {
                chanceToStopAndLookWhenSearchingFactor = GameData.Instance.AIConstants.ChanceToStopAndLookWhenSearchingFactor;
                minimumChanceToStopAndLookWhenSearching = GameData.Instance.AIConstants.MinimumChanceToStopAndLookWhenSearching;
                timeToWaitWhenStoppedAndSearchingMean = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingMean;
                timeToWaitWhenStoppedAndSearchingStdDev = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingStdDev;
            }

            bool isFirstStopInTinyZone = numberOfVisitedLocations == 0 && locationsStillToVisit.Count == 1;

            bool stopHere;

            if (!isFirstStopInTinyZone)
            {
                // -1: opposite direction, 0: right angle, 1: same direction
                double chanceToStopAndLook = Vector2.Dot(entity.FacingNormal.ToVector2(), nextWaypointVector); // -1: opposite direction, 0: right angle, 1: same direction
                chanceToStopAndLook *= -1f; // 1: opposite direction, 0: right angle, -1: same direction
                chanceToStopAndLook *= 0.5f; // 0.5: opposite direction, 0: right angle, -0.5f: same direction
                chanceToStopAndLook += 0.5f; // 1: opposite direction, 0.5: right angle, 0f: same direction
                chanceToStopAndLook = chanceToStopAndLook * (1f - minimumChanceToStopAndLookWhenSearching) + minimumChanceToStopAndLookWhenSearching;

                chanceToStopAndLook *= chanceToStopAndLookWhenSearchingFactor;

                double c = The.Sim.GameplayRandomGenerator.NextDouble("GoalSearchArea");
                stopHere = c < chanceToStopAndLook;
            }
            else
            {
                // always stop and wait once in tiny zones
                stopHere = true;
            }


            if (stopHere)
            {
                //double timeToWait = 1.0 + 3.0 * Globals.Instance.RandomPredictable.NextDouble();
                double timeToWait;

                if (isFirstStopInTinyZone)
                {
                    timeToWait = timeToWaitWhenStoppedAndSearchingMean;
                }
                else
                {
                    timeToWait = The.Sim.GameplayRandomGenerator.RandomNormalDistribution(timeToWaitWhenStoppedAndSearchingMean, timeToWaitWhenStoppedAndSearchingStdDev);
                }
                
                StanceType stanceToTake;
                if (timeToWait > 1.0) // wait for a minimum of 1 second
                {
                    if (entity.HasStance())
                    {
                        if (timeToWait > 2.0 && The.Sim.GameplayRandomGenerator.NextDouble("GoalSearchArea") > 0.4f)
                        {
                            // don't kneel when very brief wait
                            stanceToTake = entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.SearchStancesLongerWait, entity.EntityType.LocomotorType.StancesType.DefaultStanceType);
                        }
                        else
                        {
                            stanceToTake = entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.SearchStancesBriefWait, entity.EntityType.LocomotorType.StancesType.DefaultStanceType);
                        }

                        ChangeStance(stanceToTake);

                        if (stanceToTake.AnimModifier.HasValue)
                        {
                            AddSubgoal(new GoalWait(entity, timeToWait, AnimAction.Scouting, stanceToTake.AnimModifier.Value));
                        }
                        else
                        {
                            AddSubgoal(new GoalWait(entity, timeToWait, AnimAction.Scouting));
                        }
                    }
                    else
                    {
                        AddSubgoal(new GoalWait(entity, timeToWait, AnimAction.Scouting));
                    }
                }
            }
        }

        private bool waitingForDistance = false;

        public override bool HandleMessage(Message message)
        {
            //first, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            //if the msg was not handled, test to see if this goal can handle it
            if (handled == false)
            {
                switch (message.MessageType)
                {
                    // someone wants us to stop doing this job: (how can we handle this msg with no job???)
                  /*  case Message.MessageTypes.CancelJob:
                    case Message.MessageTypes.CancelJobForAIReset:

                        Status = Status.Failed;

                        return true; //msg handled
                    */
                    case Message.MessageTypes.DistanceFound: 
                    case Message.MessageTypes.DistanceFoundNoAccess:
                        //GoalWait handler ikke DistanceFound, så den kommer her tilbage. Fjern all subgoals: RemoveAllGoals når vi får et svar tilbage.

                        /*
                        if (distance < lowestDistance)
                        {
                            lowestDistance = distance;
                            closestLocation = new Vector2(locationsStillToVisit[locationIndex].X, locationsStillToVisit[locationIndex].Y);
                            currentDestinationIndex = locationIndex;
                            hasFoundIndex = true;
                        }
                        
                        locationIndex++;*/

                        if (waitingForDistance)
                        {
                            RemoveAllSubgoals();
                            waitingForDistance = false;
                           
                            return true;
                        }

                        return false;

                    default: return false;
                }
            }
            else
            {
                return true;
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

            this.closestLocation = sn.DoVector2(closestLocation);
            this.currentDestinationIndex = sn.DoInt32(currentDestinationIndex);
            this.distance = sn.DoFloat(distance);
            this.isStealthy = sn.DoBool(isStealthy);
            this.locationsStillToVisit = sn.DoList(locationsStillToVisit);
            this.lowestDistance = sn.DoFloat(lowestDistance);
            this.numberOfVisitedLocations = sn.DoInt32(numberOfVisitedLocations);
            this.examine = sn.DoBool(examine);
            this.waitingForDistance = sn.DoBool(waitingForDistance);
            this.snapshotZone = (ZoneID)sn.SnapshotID<Zone, ZoneID>(zone);            
            
            sn.Ignore(this.setSneakingRegulator);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone);
        }

        #endregion
    }
}
