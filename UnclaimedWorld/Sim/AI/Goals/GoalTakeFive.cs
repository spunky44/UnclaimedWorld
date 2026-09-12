using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using Xclna.Xna.Animation;
using GameStateManagement;
using UWGame.SimSide.Maps;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// This goal can also move to a safe spot before idling. The actual act of doing nothing is in GoalDoTakeFive.
    /// 
    /// This goal gets created every 5 secs or so
    /// </summary>
    class GoalTakeFive : CompositeGoal, ITopLevelGoal
    {
       // double? timeToRest;

        public double TimeSpentInTopLevelGoal { get; set; }

      /*  public GoalTakeFive(Entity entity, double timeToRest) 
            : base(entity)
        {
            this.timeToRest = timeToRest;           
        }*/

        public GoalTakeFive(Entity entity)
            : base(entity)
        {
            //this.timeToRest = timeToRest;

            
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

            //this.timeToRest = sn.DoDoubleNullable(timeToRest);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            return this;
        }

        public GoalTakeFive()
        {
        }


        public double ScoreGoal()
        {
            return GameData.Instance.AIConstants.IdleGoalDesirability;
        }

        public override Goal.DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            if (!requiresExamineAction)
            {
                return DetectionFactor.DetectSome;
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

       // const int shortDistance = 7;
      //  static SubtileInfluence influenceMap = new SubtileInfluence(shortDistance * 2, shortDistance * 2);
   

        private void MoveShortDistance()
        {           

            if (entity.ContainedBy == null
                && entity.EntityType.IntelligenceType.IsMobile)
            {
                float? chance = entity.GetChanceToIdleWalkShortDistanceAway();

                float moveAbility = entity.Locomotor.MoveAbility;

                if (chance.HasValue && moveAbility > 0.15f)
                {
                    float energyLevelFactor = 1f;
                    if (entity.BiologicalEntity != null)
                    {
                        energyLevelFactor = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyIdleWalkFactor, 1f, entity.BiologicalEntity.EnergyLevel);
                    }

                    if (The.Sim.GameplayRandomGenerator.NextDouble("GoalTakeFive") < energyLevelFactor * chance.Value * moveAbility)
                    {
                        MovementMap footMap =
                            entityIntelligence.Allegiance.SharedKnowledge.
                                GetMovementMap(entity.Intelligence.ProtectionLevel, entity.EntityType /*.Intelligence.Allegiance.RepresentativeEntityType.ThreatCategory*/, 
                                entityIntelligence.ThreatStance);



                        float minRange = entity.GetShortIdleWalkMinDistance() ?? 48f;
                        float maxRange = entity.GetShortIdleWalkMaxDistance() ?? 110f;

                        int width = (int)(maxRange * 2.5f * MapManager.oneOverSubtileSize);

                        SubtileInfluence influenceMap = new SubtileInfluence(entity.Location.Value, width);

                        influenceMap.DrawRadius(entity.PlaySiteLocation, minRange, maxRange, 4); // prefer a short, not micro, distance

                        influenceMap.DrawDistanceGradientOnInfluenceMap(entityIntelligence.CurrentExpedition.Center.Value, 1f, 4f); // draw nearer to expedition

                        influenceMap.DrawNegativeInfluenceFromEntities(entity, true, true); // avoid items and agents


                        influenceMap.BlockOutBlockedSubtiles(footMap.Layers[SurfaceType.TransportType.Foot], true); // avoid blocked and reserved areas

                        influenceMap.AddWhiteNoise(4); // make it more random

                        List<Tuple<byte, SubtilePos>> bestPositions = influenceMap.GetBestRelativePositions(10, true);

                        int positionsTried = 0;

                        foreach (var item in bestPositions)
                        {
                            SubtilePos absolutePos = item.Item2 + new SubtilePos(influenceMap.TopLeftSubtilePositionOfMap);

                            if (MapManager.WorldPosToSubtilePos(entity.PlaySiteLocation) != absolutePos)
                            {
                                float distance = 0f;
                                RegionMap.Result result =
                                   footMap.Layers[SurfaceType.TransportType.Foot].RegionMap.GetDistance(entity, MapManager.WorldPosToSubtile(entity.AccessPoint.Value), // #ACCESS PlaySiteLocation), 
                                   absolutePos.ToPoint(),
                                    ref distance, false);

                                if (result == RegionMap.Result.OK)
                                {

                                    //WorldLocation 
                                    Vector3 moveTo = MapManager.SubTileToWorldPos3(absolutePos.ToPoint());
                                    moveTo = MapManager.VaryLocationWithinSubtile(moveTo);

                                    AddSubgoal(new GoalMoveToPosition(entity, moveTo, null));

                                    if (entity.Locomotor.LeggedLocomotor != null) // wheels also? probably..
                                    {
                                        entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.WalkSlowly;
                                    }

                                    return;
                                }
                                else if (result == RegionMap.Result.NoAccess)
                                {
                                    positionsTried++;
                                    if (positionsTried >= 10)
                                    {
                                        return;
                                    }

                                    continue; // look at next option...
                                }
                                else if (result == RegionMap.Result.Wait)
                                {
                                    return; // nevermind...
                                }
                            }
                        }
                    }
                }
            }

        }

        public override void OnExit()
        {
            base.OnExit();

            if (entity.Locomotor != null 
                && entity.Locomotor.LeggedLocomotor != null
                && entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.WalkSlowly) 
            {
                entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal;
            }
        }

        static byte SetValue(MapManager.SubtileValue value)
        {

            bool isReserved = MapManager.TestForFlag(value, MapManager.SubtileValue.Reserved);

            if (isReserved)
            {
                return 0;
            }

            if (MapManager.IsBlocked(value))
            {
                return 0;
            }

            return 1;

        }

        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            if (!entity.EntityType.IntelligenceType.IsMobile)
            {
                // for the sentry... also for incapacitated beings?
                GoalDoTakeFive takeFive = new GoalDoTakeFive(entity); //, timeToRest); //, MobileEntity.Stance.Standing);
                takeFive.TestForDanger = false; // ignore danger - nowhere to go...
                AddSubgoal(takeFive); // add as the root goal
                Status = Status.Active;

            }
            else
            {

                // this call will make us flee if we are in the panic zone
                if (ValidateSafetyAndTakeAction(null))
                {
                    // OK, we are not panicking. See if we are comfortable then - if not, we are capable of moving a short distance
                    // to start executing the goal!
                    DiscomfortMap dMap = entityIntelligence.Allegiance.SharedKnowledge.GetDiscomfortMap(entityIntelligence.ProtectionLevel, 
                        entity.EntityType, entityIntelligence.ThreatStance);

                    // the discomfort map doesn't take account of buildings...
                    int discomfort = (int)dMap.Map.GetValue(entity.MapPosition.Value);

                    if (entity.ContainedBy.HasValue) 
                    {
                        discomfort -= GameData.Instance.AIConstants.ComfortBonusFromBeingInsideBuildingsOrVehicles;
                    }

                    if (discomfort <= GameData.Instance.AIConstants.HighestDiscomfortLevelForLeisureActivityToStart)
                    {
                        // we are safe in this tile.     
                        // move a short distance as it will appear more lifelike
                        MoveShortDistance();
                        
                        AddSubgoal(new GoalDoTakeFive(entity)); 
                        Status = Status.Active;
                    }
                    else
                    {
                        // this area is not comfortable for us.

                        // if we are inside a building/vehicle, step out first!
                        Entity insideBuilding;
                        if (!entity.GetContainedBy(out insideBuilding))
                        {
                            Status = Goals.Status.Failed;
                            return;
                        }

                        if (insideBuilding != null)
                        {
                            AddSubgoal(new GoalExit(entity));

                            // End this goal here. We will test the door entrance for discomfort next time around.
                            Status = Status.Active;
                        }
                        else if (entity.IsInsideVehicle())
                        {
                            AddSubgoal(new GoalExitVehicle(entity, entity.InsideVehicle.Value));
                            // End this goal here. We will test the door entrance for discomfort next time around.
                            Status = Status.Active;
                        }
                        else
                        {
                            //on foot: move away from this area - use BFS on the discomfort map

                            List<AI.Pathfinding.PathFinderNode> path = FindPathToComfort(dMap, entity, GameData.Instance.AIConstants.HighestDiscomfortLevelForLeisureActivityToStart);

                            if (path != null)
                            {

                                // we found a path out of this mess:
                                // only go half the way - we may be moving towards someone else, who are also moving towards us...
                                if (path.Count > 6) // in subtiles. 3 subtiles = 1 tile! first node is often current position...
                                {
                                    int wayPointsToRemove = path.Count / 2;
                                    path.RemoveRange(path.Count - wayPointsToRemove , wayPointsToRemove);
                                }

                                GoalFollowPath followPath = new GoalFollowPath(entity, path, null, null, null, null);
                                followPath.MovingOutOfHarmsWay = true;
                                AddSubgoal(followPath);

                                // THEN rest.
                                AddSubgoal(new GoalDoTakeFive(entity)); //, timeToRest)); //, stanceToTake));
                                Status = Status.Active;

                            }
                            else
                            {
                                // no better spot found near here... just stay in this spot for a bit.
                                GoalDoTakeFive takeFive = new GoalDoTakeFive(entity); //, timeToRest); //, stanceToTake);
                                takeFive.TestForDanger = false; // ignore danger - nowhere to go...
                                AddSubgoal(takeFive); // add as the root goal
                                Status = Status.Active;
                            }
                        }
                    }

                }
                
            }
        }

        



        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
           /* ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                //process the subgoals
                Status = ProcessSubgoals(elapsed);
           /* }

            ExitIfFailedOrCompleted();

            return Status;*/
        }

        public const string IdlingText = "Idling";

        public override string GetStatus()
        {
            return IdlingText;
        }
        

    /*    public override bool HandleMessage(Message message)
        {
            //first, pass the message down the goal hierarchy
          bool handled = ForwardMessageToFrontMostSubgoal(message);

          //if the msg was not handled, test to see if this goal can handle it
          if (handled == false)
          {
              switch (message.MessageType)
              {
                  // someone wants us to stop doing this job:
                  case Message.MessageTypes.CancelJob:

                      Status = Status.Failed;
                      if (job.TakenBy.Contains(entity))
                      {
                          job.TakenBy.Remove(entity);
                      }

                      return true; //msg handled

                  default: return false;
              }
          }
          else
          {
              return true;
          }
        }*/
    }
}
