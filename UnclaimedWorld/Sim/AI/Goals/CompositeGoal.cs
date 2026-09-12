using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Buildings;
using GameStateManagement;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Resources;
using UWGame.Control;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.Containers.Components;
namespace UWGame.SimSide.AI.Goals
{
    public abstract class CompositeGoal : Goal
    {
        public Queue<Goal> Subgoals = new Queue<Goal>();
        Queue<GoalID> snapshotSubgoals = new Queue<GoalID>();


        protected List<EntityGroupID> ownersOfVehicles;

        // TODO: make this indpendent of Jobs - use InUseBy to set a lock...
        private List<EntityID> optionalEquipmentAssignedToThisJob = new List<EntityID>();

       // private Dictionary<EntityID, float> cachedDistances;




        public CompositeGoal() { }


        public CompositeGoal(Entity entity)
            : base(entity)
        {

        }

        /*   public override string ToString()
           {
               System.Text.StringBuilder builder = new StringBuilder();
               string delim = "\r\n";
               builder.Append(GetType().ToString().Remove(0, GetType().ToString().LastIndexOf(".") + 1));
               builder.Append(delim);
               foreach(Goal goal in Subgoals)
               {
                
                   builder.Append(goal.ToString());
                   builder.Append(delim);
               }

               return builder.ToString();
           }*/

        /*
        protected override double ScoreThisGoal()
        {
            if (Subgoals.Count > 0)
            {
                return Subgoals.Peek().ScoreTopLevelGoal(); 
            }
            else return base.GetCurrentGoalScore();

        }*/

       

        public override bool IsSame(Jobs.Job job)
        {
            // why can't we just check the TakenBy list..?
            if (job != null && job.TakenBy.Contains(entity))
                return true;

            // is this really needed? perhaps for non-jobs?
            if (Subgoals.Count > 0)
            {
                return Subgoals.Peek().IsSame(job);
            }
            else return false;

        }

        public override string GetSkillInUseName()
        {
            if (Subgoals.Count > 0
                && Subgoals.Peek().Status == Goals.Status.Active)
            {
                return Subgoals.Peek().GetSkillInUseName();
            }
            else
            {
                return base.GetSkillInUseName();
            }
        }

        public override string GetToolInUseName()
        {
            if (Subgoals.Count > 0
                && Subgoals.Peek().Status == Goals.Status.Active)
            {
                return Subgoals.Peek().GetToolInUseName();
            }
            else
            {
                return base.GetToolInUseName();
            }
        }

        public override float? GetSkillProductivity()
        {
            if (Subgoals.Count > 0
                && Subgoals.Peek().Status == Goals.Status.Active)
            {
                return Subgoals.Peek().GetSkillProductivity();
            }
            else
            {
                return base.GetSkillProductivity();
            }
        }

        public override float? GetCurrentTotalProductivity()
        {
            if (Subgoals.Count > 0 
                && Subgoals.Peek().Status == Goals.Status.Active)
            {
                return Subgoals.Peek().GetCurrentTotalProductivity();
            }
            else
            {
                return base.GetCurrentTotalProductivity();
            }
        }

        public override float? GetToolProductivity()
        {
            if (Subgoals.Count > 0
                && Subgoals.Peek().Status == Goals.Status.Active)
            {
                return Subgoals.Peek().GetToolProductivity();
            }
            else
            {
                return base.GetToolProductivity();
            }
        }

        public override float GetExertionLevel()
        {
            if (Subgoals.Count > 0
                && Subgoals.Peek().Status == Goals.Status.Active)
            {
                return Subgoals.Peek().GetExertionLevel();
            }
            else
            {
                return base.GetExertionLevel();
            }

        }

        public override bool CanDropRequestedItem(Entity item)
        {
            if (Subgoals.Count > 0)
            {
                return Subgoals.Peek().CanDropRequestedItem(item);
            }

            return true;
        }

        public override bool CanReactToInterest()
        {
            if (Subgoals.Count > 0
                && Subgoals.Peek().Status == Goals.Status.Active)
            {
                return Subgoals.Peek().CanReactToInterest();
            }
            else
            {
                return base.CanReactToInterest();
            }
        }

        public override Goal.DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            if (Subgoals.Count > 0
                && Subgoals.Peek().Status == Goals.Status.Active)
            {
                return Subgoals.Peek().GetDetectAgentsFactor(typeOfAgent, requiresExamineAction);
            }
            else
            {
                return base.GetDetectAgentsFactor(typeOfAgent, requiresExamineAction);
            }
        }

        public override Goal.DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
        {
            if (Subgoals.Count > 0
                && Subgoals.Peek().Status == Goals.Status.Active)
            {
                return Subgoals.Peek().GetDetectResourcesFactor(resourceType, requiresExamineAction);
            }
            else
            {
                return base.GetDetectResourcesFactor(resourceType, requiresExamineAction);
            }
        }

        public override Goal GetFrontMostGoal()
        {
            if (Subgoals.Count > 0)
            {
                return Subgoals.Peek();
            }
            else
            {
                return this;
            }
        }


        public override string ComposeIndentedString(string indent)
        {
            System.Text.StringBuilder builder = new StringBuilder();
            string delim = "\r\n";
            builder.Append(indent);
            //  builder.Append(GetType().ToString().Remove(0, GetType().ToString().LastIndexOf(".") + 1));
            builder.Append(this.ToString());
            builder.Append(delim);
            foreach (Goal goal in Subgoals)
            {
                //builder.Append("  ");
                builder.Append(goal.ComposeIndentedString(" " + indent));
                builder.Append(delim);
            }

            return builder.ToString();
        }

        public List<Waypoint> GetWaypointPath()
        {
            //List<AI.Goals.GoalFollowPath.Waypoint> path = null;

            foreach (Goal goal in Subgoals)
            {
                if (goal is GoalFollowPath)
                {
                    return ((GoalFollowPath)goal).WaypointPath;
                }
                else if (goal is CompositeGoal)
                {
                    return ((CompositeGoal)goal).GetWaypointPath();
                }
            }

            return null;
        }


        protected void DropAllCarriedItems()
        {
            if (entity.AgentStorage != null)
            {
                entity.AgentStorage.IterateContained(
                    e => AddSubgoal(
                        new GoalDropItem(entity, e.EntityID)));

            }
        }


        protected void EnableCollisions(bool enable)
        {
            // HACK:
            // ignore collisions with sleepers/collapsed...
            // this is a temporary fix because the pathfinder cannot see and avoid them. When we get footprints from entities implemented, delete this.
            if (enable)
            {
                entity.EnableCollisions();
            }
            else
            {
                entity.DisableCollisions();
            }
        }



        /// <summary>
        /// returns Active as long as there are still subgoals in the queue that have not completed or failed.
        /// </summary>
        /// <param name="elapsed"></param>
        /// <returns></returns>
        public Status ProcessSubgoals(GameTime elapsed)
        {
            Status statusOfLastSubGoal = Status.Completed;
          /*  bool hasFailedSubgoal;

            statusOfLastSubGoal = CleanupSubgoals(statusOfLastSubGoal, out hasFailedSubgoal);
            */
            bool hasFailedSubgoal = false;

            statusOfLastSubGoal = CleanupSubgoals(statusOfLastSubGoal, ref hasFailedSubgoal);
           

            if (hasFailedSubgoal)
            {   // NEW
                // only GoalMoveToMosition will tolerate this (x times), and replan (Activate).
                return Status.Failed;
            }
            else
            {

                if (Subgoals.Count > 0)
                {
                    Status statusOfSubGoals = Subgoals.Peek().Process(elapsed);
                    if (statusOfSubGoals == Status.Completed && Subgoals.Count > 1)
                    {
                        return Status.Active;
                    }

                    return statusOfSubGoals;

                }
                else
                {

                    if (statusOfLastSubGoal == Status.Completed)
                    {
                        return Status.Completed;
                    }
                    else if (statusOfLastSubGoal == Status.Failed)
                    {
                        return Status.Failed;
                    }

                    return Status.Completed;
                }
            }
        }

        protected void HandleSubstitutedGoalByArbitrator()
        {
            Status = Goals.Status.Failed; // the goal was interrupted and will never complete.
        }

       // protected Status CleanupSubgoals(Status statusOfLastSubGoal, out bool hasFailedSubgoal)
        protected Status CleanupSubgoals(Status statusOfLastSubGoal, ref bool hasFailedSubgoal) 
        {
            // if the list of subgoals contains a failed goal report it.
            // NEW: can see failed subgoals beneath the first level too...
           // hasFailedSubgoal = false;

          //  List<Goal> subgoalsToRemove = null;
            //foreach (var subGoal in Subgoals)
            while (Subgoals.Count > 0)
            {
                Goal subGoal = Subgoals.Peek();

                if (subGoal.IsCompleted() ||
                    subGoal.HasFailed())
                {
                    if (subGoal.HasFailed())
                    {
                        hasFailedSubgoal = true;
                    }

                    statusOfLastSubGoal = subGoal.Status;
                    subGoal.Terminate();

                //    Common.AddToList(ref subgoalsToRemove, subGoal);
                    RemoveFirstSubgoal();

                    if (Subgoals.Count > 0)
                    {
                        Subgoals.Peek().EnterIfNew(); // i kind of feel this should be in a different place..
                    }
                }
                else
                {
                    // NEW: also iterate into subgoals that are still Active in order to clean out any failed or completed subgoals inside them:
                    CompositeGoal compositeGoal = subGoal as CompositeGoal;
                    if (compositeGoal != null)
                    {
                        compositeGoal.CleanupSubgoals(statusOfLastSubGoal, ref hasFailedSubgoal);
                        // don't return statusOfLastSubGoal here, the caller only needs the status at the 1st level.
                    }

                    break; // don't iterate the rest. It shouldn't be needed, and would require a List anyway in order to remove from the middle
                }
            }

           /* if (subgoalsToRemove != null)
            {
                foreach (var subgoal in subgoalsToRemove)
                {
                    Subgoals.Remove(subgoal);
                    DestroyGoal(subgoal); 
                }
            }
            */
                       

          /*  while (Subgoals.Count > 0 &&
                (Subgoals.Peek().IsCompleted() ||
                Subgoals.Peek().HasFailed()))
            {
                Goal subGoal = Subgoals.Peek();

                if (subGoal.HasFailed())
                {
                    hasFailedSubgoal = true;
                }

                statusOfLastSubGoal = subGoal.Status;
                subGoal.Terminate();
                

                RemoveFirstSubgoal();

                if (Subgoals.Count > 0)
                {
                    Subgoals.Peek().EnterIfNew(); // i kind of feel this should be in a different place..
                }

            }*/


            return statusOfLastSubGoal;
        }

       

        protected Goal RemoveFirstSubgoal()
        {
            Goal subgoal = Subgoals.Dequeue();

            DestroyGoal(subgoal);

            return subgoal;
        }

        private static void DestroyGoal(Goal subgoal)
        {
            // NOW it is safe to return the goal to the pool (and remove its ID):
            subgoal.RemoveIDEntry(); // goals in the pool should not have an ID...
            subgoal.RetireGoal(); // do this regardless of Enter or Activate
        }

        /* protected Status CleanupSubgoals(Status statusOfLastSubGoal)
         {
             Goal currentGoal;

             while (Subgoals.Count > 0 &&
                 (Subgoals.Peek().isComplete() ||
                 Subgoals.Peek().hasFailed()))
             {
                 statusOfLastSubGoal = Subgoals.Peek().Status;
                 Subgoals.Peek().Terminate();
                 Subgoals.Dequeue();
             }
             return statusOfLastSubGoal;
         }
         */

        private const int searchLimit = 6000;
        public static List<AI.Pathfinding.PathFinderNode> FindPathToSafety(DiscomfortMap dMap, Entity entity)
        {
            // why is this needed...? because of the arguments?
            AStarSearch.DijkstraTestNodeDelegate findFreeTileDelegate =
                (AStarSearch.DijkstraTestNodeDelegate)Delegate.CreateDelegate(typeof(AStarSearch.DijkstraTestNodeDelegate), dMap, The.Map.InfluenceMapTileIsFreeInfo);

            // find a path on the non-blocked map:
            // TODO: time slice these searches!
            return entity.Intelligence.PathPlanner.FindItemAndGetPath(
                entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(ProtectionLevel.Protected,
                entity.EntityType, ThreatStance.Bold).Layers[entity.GetTransportType()],
                findFreeTileDelegate, searchLimit, entity.PlaySiteLocation);


        }

        /// <summary>
        /// use the Bold map to find a way to comfort...
        /// </summary>
        /// <param name="dMap"></param>
        /// <param name="entity"></param>
        /// <param name="discomfortBelowValue"></param>
        /// <returns></returns>
        public static List<AI.Pathfinding.PathFinderNode> FindPathToComfort(DiscomfortMap dMap, Entity entity, byte discomfortBelowValue)
        {
            // we can only pass one argument to the function - encapsulate them in a struct.
            AStarSearch.DijkstraTestNodeDelegate findComfortableTileDelegate =
                (AStarSearch.DijkstraTestNodeDelegate)Delegate.CreateDelegate(typeof(AStarSearch.DijkstraTestNodeDelegate),
                new InfluenceMap.DiscomfortTileIsBelowValueParameters(dMap, discomfortBelowValue),
                The.Map.InfluenceMapTileIsComfortableInfo);


            Intelligence intelligenceComponent = entity.Intelligence;

            // use entity's current stance to find the nearest comfortable tile:
            // TODO: time slice these searches!
            SubtileLayers mapCosts = intelligenceComponent.Allegiance.SharedKnowledge.GetMovementMap(intelligenceComponent.ProtectionLevel,
                entity.EntityType, ThreatStance.Bold /* intelligenceComponent.ThreatStance*/).Layers[entity.GetTransportType()];

            return intelligenceComponent.PathPlanner.FindItemAndGetPath(mapCosts,
                findComfortableTileDelegate, searchLimit, entity.PlaySiteLocation);

        }

        /// <summary>
        /// Unless this method is called during the goal, the agent will not be able to flee approaching dangers/threats...
        /// returns true if everything is alright, false if action was taken. The test against comfort level simple sets status=failed.
        /// </summary>
        /// <param name="discomfortLevelCausingFail"></param>
        /// <returns></returns>
        protected bool ValidateSafetyAndTakeAction(byte? discomfortLevelCausingFail)
        {
            if (entity.EntityType.IntelligenceType.IsMobile)
            {
                DiscomfortMap dMap = entityIntelligence.Allegiance.SharedKnowledge.GetDiscomfortMap(entityIntelligence.ProtectionLevel, entity.EntityType, entityIntelligence.ThreatStance);


                float discomfortLevel = dMap.Map.GetValue(entity.MapPosition.Value);

                if (entityIntelligence.ThreatStance != ThreatStance.Bold && discomfortLevel > entity.Intelligence.PanicLevel)
                {   // Bold discomfort maps are never blocked. So if we are bold, don't flee... wait until the stance changes.
                    return PerformPanicFleeing(dMap);

                }
                else if (discomfortLevelCausingFail != null && discomfortLevel > discomfortLevelCausingFail.Value)//AIConstants.HighestDiscomfortLevelForLeisureActivity)
                {
                    //end the activity
                    Status = Status.Failed;
                    return false;
                }
            }

            return true;

        }

        protected bool PerformPanicFleeing(DiscomfortMap dMap)
        {
            if (entity.Locomotor.LeggedLocomotor != null)
            {
                // do panic
                // drop everything:
                entityIntelligence.Brain.RemoveAllSubgoals();

                // random chance to also drop what we are holding in our hands:
               /* if (entity.AgentStorage != null && entity.AgentStorage.MountedToolOrWeapon != null)
                {
                    if (The.Sim.GameplayRandomGenerator.NextDouble("CompositeGoal") < GameData.Instance.Constants.ChanceToDropWeaponWhenFleeing)
                    {
                        Entity weaponOrTool = Entity.FindByID(entity.AgentStorage.MountedToolOrWeapon.Value);
                        if (weaponOrTool != null)
                        {
                            entity.AgentStorage.Uncontain(weaponOrTool);
                        }
                    }
                }*/

                List<AI.Pathfinding.PathFinderNode> path = FindPathToSafety(dMap, entity);

                if (path != null)
                {
                    // we found a path out of this mess:
                    GoalFollowPath followPath = new GoalFollowPath(entity, path, null, null, null, null);
                    followPath.MovingOutOfHarmsWay = true; // ignore discomfort along the way
                    entityIntelligence.Brain.AddSubgoal(followPath); // add as the root goal

                    // set the move target for proper lerping:
                    PathFinderNode destination = path[path.Count - 1];
                    entity.Locomotor.CurrentMoveTarget = MapManager.SubTileEdgeToWorldPos3(new Point(destination.AbsoluteX, destination.AbsoluteY));  // Destination.Value.ToVector3();

                    if (entity.DrivingVehicle != null)
                    {   // remember to exit the vehicle! also flee in a vehicle...?
                        entityIntelligence.Brain.AddSubgoal(new GoalExitVehicle(entity, entity.DrivingVehicle.Value));
                    }
                    else
                    {

                        // The.Client.Log.AddLogEvent(The.Client.Log.GeneralEvent, entity, "is fleeing!");

                        SetFleeingSpeedType(entity);
                    }


                    // fire triggers, events etc:
                    // also fire when no esape route found..?
                    List<ActionSets> defaultActionSets;
                    entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.Fleeing, out defaultActionSets);
                    Goal.FireEventActions(entity, null, defaultActionSets, null);

                    Status = Status.Active;
                    return false;
                }

            }

            // we cannot escape - we are fucked... stay in this spot for a bit.
            /*  if (entity.EntityType.Name != "Diamond bird")
              {
                  The.Client.Log.AddLogEvent(The.Client.Log.GeneralEvent, entity, "is unable to flee!");

              }*/

            GoalDoTakeFive takeFive = new GoalDoTakeFive(entity, 2);
            takeFive.TestForDanger = false;
            entityIntelligence.Brain.AddSubgoal(takeFive); // add as the root goal
            Status = Status.Active;
            return false;
        }



        protected static void SetFleeingSpeedType(Entity entity)
        {
            if (entity.EntityType.LocomotorType.CanRun)
            {
                if (entity.Locomotor.LeggedLocomotor != null)
                {
                    bool canRun = false;

                    if (entity.EntityType.BiologicalType != null)
                    {
                        if (entity.BiologicalEntity.OxygenAndMuscleEnergy > GameData.Instance.Constants.OxygenEnergyRequiredToStartRunning)
                        {
                            canRun = true;
                        }
                    }
                    else
                    {
                        canRun = true; // robots never get tired..
                    }

                    if (canRun)
                    {
                        entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Run;
                    }
                    else
                    {
                        entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.WalkFast;
                    }
                }
            }
        }

        /// <summary>
        /// can handle a null argument - in that case Standing stance is the default
        /// </summary>
        /// <param name="possibleStancesToTake"></param>
       /* protected void ChangeStance(List<StanceType> possibleStancesToTake)
        {
            StanceType stanceToTake = LeggedLocomotor.Stance.Standing; // default
            if (possibleStancesToTake != null)
            {
                stanceToTake = Common.GetRandomListMember(possibleStancesToTake, The.Sim.GameplayRandomGenerator);
            }

            ChangeStance(stanceToTake);
        }*/

        protected void ChangeStance(StanceType stanceToTake)
        {
            AddSubgoal(new GoalChangeStance(entity, stanceToTake));

        }

        /// <summary>
        /// can switch stances now and then
        /// </summary>
        /// <param name="changeStanceRegulator"></param>
        /// <param name="possibleStances"></param>
        protected void HandleStanceChange(Regulator changeStanceRegulator, Dictionary<StancesType, List<ChanceToTakeStance>> possibleStances) // StanceType[] possibleStances)
        {
            if (entity.HasStance() && changeStanceRegulator.IsReady())
            {
               // StanceType stanceToTake;

                ChangeStance(entity.Locomotor.Stance.PickRandomProcessStance(possibleStances));

                /*
                SelectStance(possibleStances, out stanceToTake);

                ChangeStance(stanceToTake);*/
            }
        }


        /// <summary>
        /// Sets status to Failed if the tools have problems. otherwise returns their data
        /// </summary>
        /// <param name="tools"></param>
        /// <param name="toolsData"></param>
        /// <returns></returns>
        protected bool ResolveTools(List<EntityID> tools, ref List<IKnownEntityData> toolsData)
        {
            if (tools != null && tools.Count > 0)
            {
                toolsData = new List<IKnownEntityData>();
                IKnownEntityData toolData;
                foreach (var tool in tools)
                {
                    if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(tool, out toolData)))
                    {
                        return false;
                    }
                    else toolsData.Add(toolData);
                }
            }

            return true;
        }

        protected bool CheckHandTools(List<IKnownEntityData> toolsData, Job job)
        {
            if (toolsData != null)
            {
                foreach (var item in toolsData)
                {
                    if (!ToolType.IsImmovable(item.EntityType)
                        && item.EntityType.ToolType.ToolHandling == ToolHandlingType.HandTool) // IsHandTool == true)
                    {
                        if (!entity.AgentStorage.Contains(item.EntityID)
                            || (job != null && item.AssignedToJob != job.ID))
                        {
                            // a pickup goal and a lock on the item should have prevented this situation.
                            // TODO: add this to GoalDoHarvest etc.
                            // System.Diagnostics.Debug.Assert(false, "Tool is not carried or assigned to this job!");
                            Status = Status.Failed;

                            return false;
                        }
                    }
                }
            }

            return true;
        }

        protected void MountTool(List<IKnownEntityData> toolsData)
        {
            if (entity.AgentStorage != null)
            {
                IKnownEntityData toolToMount = null;
                if (toolsData != null)
                {
                   
                    foreach (var tool in toolsData)
                    {
                        // put one tool in the hand...
                        // for hammer/ bellows combo, pick the one that has flags defined. (hacky)
                        // mountedEntity.EntityType.ItemType.AnimStatesWhenAttached
                        if (!ToolType.IsImmovable(tool.EntityType) && tool.EntityType.ToolType.ToolHandling == ToolHandlingType.HandTool) // .IsHandTool == true)
                        {
                            if (toolToMount == null || toolToMount.EntityType.ItemType.AnimStatesWhenAttached == null)
                            {
                                toolToMount = tool;
                            }                           
                        }
                    }
                }

                if (toolToMount != null)
                {
                    entity.AgentStorage.MountedToolOrWeapon = toolToMount.EntityID;
                }
                else
                {
                    // else make sure the hands are free:
                    entity.AgentStorage.MountedToolOrWeapon = null;
                }
            }

        }


        protected void MountReplenishTarget(Entity replenishTarget)
        {
            if (entity.AgentStorage != null)
            {
                if (replenishTarget != null
                    && replenishTarget.EntityType.IsMountable()
                    && entity.ContainsEntity(replenishTarget.ID)) // NEW: don't mount if not already carried.
                {

                    entity.AgentStorage.MountedToolOrWeapon = replenishTarget.ID;
                    return;
                }

                // else make sure the hands are free:
                entity.AgentStorage.MountedToolOrWeapon = null;
            }
        }

        /// <summary>
        /// returns false and sets a failed goal state if there was a problem.
        /// </summary>
        /// <param name="itemData"></param>
        /// <param name="carriedBySelf"></param>
        /// <param name="buildingWeCanEnter"></param>
        /// <param name="containerWeCanUnloadFrom"></param>
        /// <returns></returns>
        protected bool GetInputInsideContainer(IKnownEntityData itemData, out bool carriedBySelf, out bool insideContainerWeCannotUse, out IKnownEntityData buildingWeCanEnter, out IKnownEntityData containerWeCanUnloadFrom)
        {
            carriedBySelf = false;
            buildingWeCanEnter = null;
            containerWeCanUnloadFrom = null;
            insideContainerWeCannotUse = false;

            if (itemData.ContainedBy.HasValue)
            {
                EntityID containedBy = itemData.ContainedBy.Value;

                IKnownEntityData containedByData;
                if (!EntityResultCausesFailedGoal(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(containedBy, out containedByData)))
                {
                    // if contained by self, uncontain...
                    if (containedByData == entity)
                    {
                        carriedBySelf = true;
                        return true;
                    }

                    if (containedByData.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
                    {
                        buildingWeCanEnter = containedByData;
                        return true;
                    }
                    else
                    {
                        if (containedByData.EntityType.ContainerType.CanTransactWithContainer(entity.EntityType))
                        {
                            containerWeCanUnloadFrom = containedByData;
                            return true;
                        }

                        // inside a container we cannot transact with - such as another agent:
                        insideContainerWeCannotUse = true;
                        return true;
                    }
                }
                else return false;
            }

            return true;
        }



        protected bool UnloadOrDropUnToGround(IKnownEntityData itemData)
        {
            bool carriedBySelf = false;
            bool insideContainerWeCannotUse = false;
            IKnownEntityData buildingWeCanEnter = null;
            IKnownEntityData containerWeCanUnloadFrom = null;

            bool success = GetInputInsideContainer(itemData, out carriedBySelf, out insideContainerWeCannotUse, out buildingWeCanEnter, out containerWeCanUnloadFrom);

            if (success)
            {
                // if contained by self, uncontain...
                if (carriedBySelf)
                {
                    AddSubgoal(new GoalDropItem(entity, itemData.EntityID));

                    return true;
                }

                if (insideContainerWeCannotUse)
                {
                    // fail if the destination entity is carried by an intelligent agent (we never unload from people; they should be told to drop the item)
                    Status = Goals.Status.Failed;
                    return false;
                }

                if (containerWeCanUnloadFrom != null)
                {
                    AddSubgoal(new GoalUnload(entity, itemData.ContainedBy.Value, itemData.EntityID));
                }
            }

            return success;


            /* OLD:
            if (itemData.ContainedBy.HasValue)
            {
                EntityID containedBy = itemData.ContainedBy.Value;

                IKnownEntityData containedByData;
                if (!EntityResultCausesFailedGoal(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(containedBy, out containedByData)))
                {
                    // if contained by self, uncontain...
                    if (containedByData == entity)
                    {
                        AddSubgoal(new GoalDropItem(entity, itemData.EntityID, null, null));                        

                        return true;
                    }

                    if (!containedByData.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
                    {
                        if (containedByData.EntityType.ContainerType.AgentStorageType != null)
                        {    // fail if the destination entity is carried by an intelligent agent (we never unload from people; they should be told to drop the item)
                            Status = Goals.Status.Failed;
                            return false;
                        }
                        else
                        {
                            AddSubgoal(new GoalUnload(entity, itemData.ContainedBy.Value, itemData.EntityID));
                        }
                    }
                }
                else return false;
            }

            return true;*/

        }

        /// <summary>
        /// inserts an unload goal if the item that we want to pick up is inside a container that we cannot enter
        /// 
        /// sets status to Failed and returns false if there was a problem
        /// </summary>
        /// <param name="itemData"></param>
        protected bool PickupItemOrUnloadFirst(IKnownEntityData itemData, bool mountAfterPickup = false, bool bendDown = true, bool standUpAfterwards = true, StorageCompartment compartment = StorageCompartment.Haul)
        {
            if (itemData.ContainedBy.HasValue)
            {
                EntityID containedBy = itemData.ContainedBy.Value;

                IKnownEntityData containedByData;
                if (!EntityResultCausesFailedGoal(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(containedBy, out containedByData)))
                {
                    // if contained by self, we don't have to do anything:
                    if (containedByData == entity)
                    {
                        if (mountAfterPickup)
                        {
                            // mount it...
                            entity.AgentStorage.MountedToolOrWeapon = itemData.EntityID;
                            //((Entity)itemData).Item.MountIfPossible(entity);                            
                        }

                        return true;
                    }

                    if (!containedByData.EntityType.ContainerType.AllowedInContainer(entity.EntityType))
                    {
                        if (containedByData.EntityType.ContainerType is AgentStorageType)
                        {    // fail if the destination entity is carried by an intelligent agent (we never unload from people; they should be told to drop the item)
                            Status = Goals.Status.Failed;
                            return false; // not always handled??
                        }
                        else
                        {
                            AddSubgoal(new GoalUnload(entity, itemData.ContainedBy.Value, itemData.EntityID));
                        }
                    }
                }
                else return false;
            }


            AddSubgoal(new GoalPickup(entity, itemData.EntityID, null, compartment, mountAfterPickup, bendDown, standUpAfterwards));

            return true;
        }

       

        #region Optional equipment


        private bool IsFoodForEquipment(EntityID entityID)
        {
            IKnownEntityData entityData;

            if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entityID, out entityData)))
            {
                if (entityData.EntityType.ItemType != null && entityData.EntityType.ItemType.FoodType != null)
                {
                    if (entity.IsOwnedByUs(entityData)) // IsToolOrWeaponOK(entityData))
                    {
                        if (entity.BiologicalEntity.IsEatable(entityData)
                            && GoalEvaluator.IsValidPlaysiteItem(entity, entityData, true))
                        {
                            return true;
                        }
                    }

                }
            }


            return false;
        }

        private bool IsGadgetForTask(EntityID entityID, List<ItemType.TaskType> taskTypes, bool taskIsBeyondNormalRange)
        {
            IKnownEntityData entityData;

            if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entityID, out entityData)))
            {
                if (entityData.EntityType.ItemType != null 
                    && entityData.EntityType.ItemType.TaskAppropriateLevels != null 
                    && entityData.EntityType.ItemType.FinalEffectsWhenEquipped != null
                    && (entityData.EntityType.ItemType.UseGearAtAnyDistanceFromExpedition || taskIsBeyondNormalRange))
                {
                    if (entity.IsOwnedByUs(entityData)) // IsToolOrWeaponOK(entityData))
                    {
                        if (GoalEvaluator.IsValidPlaysiteItem(entity, entityData, true))
                        {
                            return true;
                        }
                    }

                }
            }


            return false;
        }

        private bool IsWeaponForEquipment(EntityID entityID)
        {
            IKnownEntityData entityData;

            if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entityID, out entityData)))
            {
                if (entityData.EntityType.ItemType != null && entityData.EntityType.ItemType.WeaponType != null)
                {
                    if (entity.IsOwnedByUs(entityData)) // IsToolOrWeaponOK(entityData))
                    {
                        if (EvaluateAttackJobs.IsValidPlaysiteWeapon(entity, entityData, null, null, null, true))
                        {
                            return true;                           
                        }
                    }

                }
            }
            

            return false;
        }

       /// <summary>
       /// returns the list of entities that can be reached by foot from the expedition, the agent itself and what the agent is carrying
       /// </summary>
       /// <param name="radiusToLookIn"></param>
       /// <param name="results"></param>
       /// <param name="stanceToUse"></param>
       /// <param name="footRegionMap"></param>
       /// <param name="filter"></param>
        protected GoalEvaluator.CalculateResult GetNearbyEntities(float radiusToLookIn, out List<IKnownEntityData> results, ThreatStance stanceToUse, RegionMap footRegionMap, Predicate<EntityID> filter, Dictionary<EntityID, float> cachedDistances)
        {            
            results = new List<IKnownEntityData>();

            SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;           
            List<Pair<EntityID, Vector2>> airDistanceResultList = null;
            
            // let's look near the expedition:
            sharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetEntitiesInRange(entityIntelligence.CurrentExpedition.Location.Value.ToVector2(), 
                                                                      radiusToLookIn,
                                                                      filter,
                                                                      ref airDistanceResultList);

            // NEW: add nearby items too:
            List<Pair<EntityID, Vector2>> nearEntityDistanceResultList = null;            
            sharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), 
                                                                      80f,
                                                                      (e) => (airDistanceResultList == null || !airDistanceResultList.Exists(n => n.First == e))
                                                                        && filter(e),
                                                                      ref nearEntityDistanceResultList);
         
            if (nearEntityDistanceResultList != null)
            {
                if (airDistanceResultList != null)
                {
                    airDistanceResultList.AddRange(nearEntityDistanceResultList);
                }
                else
                {
                    airDistanceResultList = nearEntityDistanceResultList;
                }
            }

            // add carried items if they have not been added yet (this will happen if the agent is outside the expedition radius):
            entity.AgentStorage.IterateContained(e =>
                {
                    if ((airDistanceResultList == null || !airDistanceResultList.Exists(n => n.First == e.EntityID))
                        && filter(e.EntityID))
                    {
                        airDistanceResultList.Add(new Pair<EntityID, Vector2>(e.EntityID, e.PlaySiteLocation.ToVector2()));
                    }

                });           

            if (airDistanceResultList != null)
            {
                float distance = 0;
                foreach (var entityIDandPosition in airDistanceResultList)
                {                  
                    IKnownEntityData entityData;

                    if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(entityIDandPosition.First, out entityData)))
                    {                        
                        RegionMap.Result result = footRegionMap.GetDistanceToEntity(entity, entity, entityData, ref distance);

                        //Pathing is ok and Pathing distance is not longer than the radius we are looking for items inside.
                        if (result == RegionMap.Result.OK /*&& distance < radiusToLookIn*/) // no handling of Processing result, hmmm... There should be a Wait goal!
                        {
                            results.Add(entityData);

                            cachedDistances.Add(entityData.EntityID, distance);
                        }
                        else if (result == RegionMap.Result.Wait)
                        {
                            return Goals.GoalEvaluator.CalculateResult.Processing;
                        }
                    }                  
                }
            }

            return Goals.GoalEvaluator.CalculateResult.Done;
                     
        }

        private bool GetCarriedAmmunition(Entity agent, List<ItemDistance> itemListToFill, EntityType ammunitionType)
        {
            foreach (var item in entity.AgentStorage.Equipment.StoredItems)
            {
                Entity targetEntity = Entity.FindByID(item);

                bool continueNow = false;
                foreach (var itemInList in itemListToFill)
                {
                    if (itemInList.Entity == targetEntity)
                    {
                        continueNow = true;
                    }
                }
                if (continueNow)
                {
                    continue;
                }


                if (targetEntity.EntityType == ammunitionType)
                {
                    ItemDistance itemDistance = new ItemDistance();
                    itemDistance.Entity = targetEntity;
                    itemDistance.Distance = 0f;

                    itemListToFill.Add(itemDistance);
                }
            }
            return itemListToFill.Count > 0;
        }

        /// <summary>
        /// allows hauling jobs that are not for inputs, like HaulSpecificItem to a stockpile - but only if they have not been assigned yet.
        /// 
        /// Why not test InUseBy here???
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool ItemIsNotAssignedToImportantJobs(IKnownEntityData data, SharedKnowledge sharedKnowledge)
        {
            if (!data.IsUnassigned(sharedKnowledge)) 
            {
                return false;
            }

            Job assignedToJob = EvaluateJob.ResolveAssignedToJob(data);

            return assignedToJob == null
               || (assignedToJob.TakenBy.Count == 0
                   && assignedToJob as HaulingJob != null
                   && (assignedToJob as HaulingJob).RequiredByProcessJob == null);

          /*  return data.AssignedToJob == null 
                || (data.AssignedToJob.TakenBy.Count == 0
                    && data.AssignedToJob as HaulingJob != null 
                    && (data.AssignedToJob as HaulingJob).RequiredByProcessJob == null);
           */
        }

        private void ScoreOptionalFoodOrGear(List<IKnownEntityData> items, 
           out IKnownEntityData bestItem,
           SharedKnowledge sharedKnowledge,
           RegionMap footRegionMap, float radiusToLookIn, Dictionary<EntityID, float> cachedDistances, List<ItemType.TaskType> taskTypes)
        {
           
            bestItem = null;
            
            float distance = 0;
            double bestScore = 0;

            foreach (var item in items)
            {                
                if (cachedDistances.TryGetValue(item.EntityID, out distance))
                {
                    if (ItemIsNotAssignedToImportantJobs(item, sharedKnowledge))
                    {
                        float desirability;
                        if (item.EntityType.ItemType.FoodType == null)
                        {
                            desirability = item.EntityType.ItemType.GetTaskAppropriateLevel(taskTypes); 
                        }
                        else
                        {
                            // too many food items exist, use the same value for all.
                            desirability = 0.5f; // should it depend on task type?
                        }

                        if (desirability > 0f)
                        {
                            double distanceScore = EvaluateAttackJobs.GetTravelScoreFromDistance(entity, distance);
                         
                            double conditionScore;

                            if (item.EntityType.ItemType.FoodType == null)
                            {
                                conditionScore = 1d; // we don't care about gear condition
                            }
                            else
                            {
                                conditionScore = EvaluateEat.ScoreCondition(item, 0.5d); 
                            }

                            double score = distanceScore * 0.3f + desirability * 0.5f + conditionScore * 0.2f;

                            if (score >= bestScore)
                            {
                                bestItem = item;
                                bestScore = score;
                            }
                        }
                    }
                }
            }
        }

       

        private void ScoreOptionalWeapons(List<IKnownEntityData> weapons,
            out List<EntityID> ammunitionToUse, out EntityType ammunitionType, out IKnownEntityData bestWeapon,
            RegionMap footRegionMap, float radiusToLookIn, Dictionary<EntityID, float> cachedDistances)
        {
            ammunitionToUse = null;
            ammunitionType = null;
            bestWeapon = null;
            if (weapons.Count == 0)
            {
                return; //Bailout
            }

            float distance = 0;
            double bestScore = 0;

            SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;

            bool hasTriedToSetThisWeaponAsBestWeapon = false;
            foreach (var weapon in weapons)
            {
                EntityGroup weaponOwner;
                // if we cannot resolve the owner, skip this item.
                if (!LookUpOwners.ResolveEntityOwner(weapon, out weaponOwner)
                    || weaponOwner == null)
                {
                    continue;
                }


                if (cachedDistances.TryGetValue(weapon.EntityID, out distance))
                {

                    hasTriedToSetThisWeaponAsBestWeapon = false;
                    //float scoreModifier = weapon.EntityType.ItemType.WeaponType.DesirabilityForUseDefensive;
                    //distance /= scoreModifier;

                    if (ItemIsNotAssignedToImportantJobs(weapon, sharedKnowledge))
                    {
                        double distanceScore = EvaluateAttackJobs.GetTravelScoreFromDistance(entity, distance);
                        //  double score = distanceScore * 0.6f + weapon.EntityType.ItemType.WeaponType.DesirabilityForUseDefensive * 0.4f;

                        float desirability = weapon.EntityType.ItemType.GetTaskAppropriateLevel(ItemType.TaskType.LongerJourneys);
                        double score = distanceScore * 0.3f + desirability * 0.7f;


                        //If weapon uses ammo see if that is available and in that case decide to use ranged weapon

                        if ((bestWeapon == null || score > bestScore))
                        {
                            foreach (var attackType in weapon.EntityType.ItemType.WeaponType.AttackTypes)
                            {
                                // pick a weapon that can be supplied with ammo (ammo must be in range)
                                if (attackType.UsesAmmo != null)
                                {
                                    // List<ItemDistance> ammunitionForThisWeapon = new List<ItemDistance>();

                                    hasTriedToSetThisWeaponAsBestWeapon = true;
                                    if (!weapon.HasEnoughAmmo(attackType.UsesAmmoType, attackType.RoundsToSpend.Value))
                                    {

                                        //If we do not have enough ammunition.
                                        List<ItemDistance> allAmmunitionFoundUsableForThisWeaponWithinRange = new List<ItemDistance>();

                                        //Get all ammunition available
                                        GoalEvaluator.GetAllReplenishItemsSortedByDistance(entity, attackType.UsesAmmoType, weaponOwner,
                                                            entityIntelligence.Allegiance.SharedKnowledge,
                                                            footRegionMap,
                                                            ref allAmmunitionFoundUsableForThisWeaponWithinRange, radiusToLookIn,
                                                            The.Sim.PlaySite.GetFirstPlayerExpedition().Location.Value.ToVector2(),
                                                            radiusToLookIn);

                                        GetCarriedAmmunition(entity,
                                                            allAmmunitionFoundUsableForThisWeaponWithinRange,
                                                            attackType.UsesAmmoType);

                                        if (allAmmunitionFoundUsableForThisWeaponWithinRange.Count > 0)
                                        {

                                            int ammoCount = 0;
                                            List<EntityID> replenishItemIDs = new List<EntityID>();
                                            foreach (var ammo in allAmmunitionFoundUsableForThisWeaponWithinRange)
                                            {
                                                if (ItemIsNotAssignedToImportantJobs(ammo.Entity, sharedKnowledge))
                                                {
                                                    ammoCount += ammo.Entity.NoOfRounds.Value;
                                                    replenishItemIDs.Add(ammo.Entity.EntityID);

                                                    if (ammoCount >= attackType.RoundsToSpend)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }

                                            if (ammoCount >= attackType.RoundsToSpend)
                                            {
                                                ammunitionToUse = replenishItemIDs;
                                                ammunitionType = attackType.UsesAmmoType;
                                                bestWeapon = weapon;
                                                bestScore = score;
                                            }
                                            else
                                            {
                                                continue; //Not enough ammunition found
                                            }
                                        }
                                        else
                                        {
                                            continue;//No ammunition found.
                                        }
                                    }
                                    else
                                    {
                                        // assign currently loaded ammo:
                                        ammunitionToUse = null; // replenishITemIDS; // ??? will not get assigned until we expose loaded ammo in MemoryFact...
                                        bestWeapon = weapon;
                                        bestScore = score;
                                    }
                                }

                            }

                            //
                            //If the weapon is not using ammunition
                            //
                            if (hasTriedToSetThisWeaponAsBestWeapon == false) //This weapon was not set as best when looking at the ammunition, set it now.
                            {

                                ammunitionToUse = null;
                                bestScore = score;
                                bestWeapon = weapon;
                            }
                        }
                    }
                }
            }
        }

        static List<ItemType.TaskType> longerJourneyTask = new List<ItemType.TaskType>() { ItemType.TaskType.LongerJourneys };


        protected GoalEvaluator.CalculateResult FindOptionalEquipmentIfNeeded(Vector3 destination, Job job, bool equipWeapon = true, bool equipFood = true, bool mountWeapon = true, List<ItemType.TaskType> taskTypesForGear = null)
        {
            if (entity.AgentStorage == null || entity.AgentStorage.Equipment == null ||
                (!entity.EntityType.IntelligenceType.CanMountToolsOrWeapons())
                || job == null) // for now, we need a job to set the lock on weapons and food... but it should be easy to set the InUseBy lock instead, or?
            {
                return Goals.GoalEvaluator.CalculateResult.Done;
            }

            RegionMap regionMapToUse;
            float? radiusToLookIn;
            ThreatStance stanceToUse;

            bool distanceOKToBringWeapons = false;
            bool distanceOKToBringFood = false;
            bool distanceOKToBringEquipment = false;

            if (RequiresBoldStance())
            {
                stanceToUse = ThreatStance.Bold;
            }
            else
            {
                stanceToUse = entity.Intelligence.ThreatStance;
            }

            float groundDistanceToDestinationFromCamp = Common.DistanceOctile(entityIntelligence.CurrentExpedition.Location.Value, destination);

            radiusToLookIn = Math.Min(groundDistanceToDestinationFromCamp, GameData.Instance.AIConstants.MaxRadiusFromExpeditionToGatherOptionalEquipment); // AI constant?

            regionMapToUse = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(
                                            ProtectionLevel.Exposed,
                                            entity.EntityType,
                                            stanceToUse).Layers[SurfaceType.TransportType.Foot].RegionMap;

            IsEquipmentNeeded(groundDistanceToDestinationFromCamp, out distanceOKToBringEquipment, out distanceOKToBringWeapons, out distanceOKToBringFood);

            /*  if (IsEquipmentNeeded(destination, out regionMapToUse, out radiusToLookIn, out stanceToUse))
              {*/
            //gather the potential items before we score their distance. This way, the caller can re-call this method after receiving the distance result

            List<IKnownEntityData> weapons = null;
            List<IKnownEntityData> food = null;
            List<IKnownEntityData> gear = null;

            // store the distances for scoring
            Dictionary<EntityID, float> cachedDistances = new Dictionary<EntityID, float>();

            if (equipWeapon
                && distanceOKToBringWeapons
                && entity.EntityType.IntelligenceType.CanUseWeapons == true)
            {
                GoalEvaluator.CalculateResult result = GetNearbyEntities(radiusToLookIn.Value, out weapons,
                    stanceToUse, regionMapToUse, IsWeaponForEquipment, cachedDistances);

                if (result == Goals.GoalEvaluator.CalculateResult.Processing)
                    return Goals.GoalEvaluator.CalculateResult.Processing;
            }


            if (taskTypesForGear != null
              //  && distanceOKToBringEquipment
                && entity.EntityType.IntelligenceType.CanUseGadgets == true)
            {
                GoalEvaluator.CalculateResult result = GetNearbyEntities(radiusToLookIn.Value, out gear,
                    stanceToUse, regionMapToUse, (e) => IsGadgetForTask(e, taskTypesForGear, distanceOKToBringEquipment), cachedDistances);

                if (result == Goals.GoalEvaluator.CalculateResult.Processing)
                    return Goals.GoalEvaluator.CalculateResult.Processing;
            }


            if (equipFood
                && distanceOKToBringFood
                && entity.EntityType.BiologicalType != null)
            {
                GoalEvaluator.CalculateResult result = GetNearbyEntities(radiusToLookIn.Value, out food,
                    stanceToUse, regionMapToUse, IsFoodForEquipment, cachedDistances);

                if (result == Goals.GoalEvaluator.CalculateResult.Processing)
                    return Goals.GoalEvaluator.CalculateResult.Processing;
            }

            SharedKnowledge sharedKnowledge = entityIntelligence.Allegiance.SharedKnowledge;

            if (weapons != null)
            {
                ScoreOptionalWeaponsAndEquipIt(stanceToUse, job, regionMapToUse, radiusToLookIn.Value, weapons, cachedDistances, mountWeapon);
            }

            if (food != null)
            {
                ScoreOptionalFoodOrGearAndEquipIt(stanceToUse, job, regionMapToUse, sharedKnowledge, radiusToLookIn.Value, food, cachedDistances, longerJourneyTask);
            }

            if (gear != null)
            {
                ScoreOptionalFoodOrGearAndEquipIt(stanceToUse, job, regionMapToUse, sharedKnowledge, radiusToLookIn.Value, gear, cachedDistances, taskTypesForGear);
            }


            return Goals.GoalEvaluator.CalculateResult.Done;
        }


        protected static void AddNightActivityGear(ref List<ItemType.TaskType> gearTasks)
        {
            if (!The.Sim.DateAndTime.SunIsUp)
            {
                Common.AddToList(ref gearTasks, ItemType.TaskType.NightActivities);
            }
        }


        private void ScoreOptionalFoodOrGearAndEquipIt(ThreatStance stanceToUse, Job job, RegionMap regionMapToUse, SharedKnowledge sharedKnowledge, float radiusToLookIn, 
            List<IKnownEntityData> food, Dictionary<EntityID, float> cachedDistances,
            List<ItemType.TaskType> taskTypes)
        {
            IKnownEntityData bestFoodItem;
            ScoreOptionalFoodOrGear(food, out bestFoodItem, sharedKnowledge, regionMapToUse, radiusToLookIn, cachedDistances, taskTypes); // ItemType.TaskType.LongerJourneys);

            if (bestFoodItem != null)
            {              
                if (!PickupItemOrUnloadFirst(bestFoodItem, false, true, true, StorageCompartment.Equipment))
                {
                    return;
                }

                SetLockOnOptionalEquipment(job, bestFoodItem);      
            }
        }

        private void ScoreOptionalWeaponsAndEquipIt(ThreatStance stanceToUse, Job job, RegionMap regionMapToUse, float radiusToLookIn, 
            List<IKnownEntityData> weapons, Dictionary<EntityID, float> cachedDistances, bool mountWeapon)
        {
           
            List<EntityID> ammunitionToUse;
            EntityType ammunitionType;
            IKnownEntityData bestWeapon;
            ScoreOptionalWeapons(weapons, out ammunitionToUse, out ammunitionType, out bestWeapon, regionMapToUse, radiusToLookIn, cachedDistances);


            if (bestWeapon != null)
            {
                
                // add goalpickup, mount depending on job? or maybe GoalHaul will automatically dismount weapon
                if (!PickupItemOrUnloadFirst(bestWeapon, mountWeapon, true, true, StorageCompartment.Equipment))
                {
                    return;
                }

                if (ammunitionToUse != null)// is null if already loaded!
                {
                    ProcessType process = bestWeapon.EntityType.ContainerType.GetReplenishProcesses()[ammunitionType];
                    AddSubgoal(new GoalReplenish(entity, bestWeapon.GetAsEntityAndRoot(), ammunitionToUse, process, null, job, StorageCompartment.Equipment));
                    foreach (var id in ammunitionToUse)
                    {
                        IKnownEntityData ammoData = null;
                        if ((EntityResultCausesFailedGoal(entity.Intelligence.GetKnownData(id, out ammoData))))
                        {
                            return;
                        }

                        SetLockOnOptionalEquipment(job, ammoData);
                    }
                }

                SetLockOnOptionalEquipment(job, bestWeapon);             

            }
        }

        private void SetLockOnOptionalEquipment(Job job, IKnownEntityData data)
        {
            // NEW: don't assign intrinsic tools/weapons. 
            if (data.EntityType.IsIntrinsic())
            {
                return;
            }

            data.AssignedToJob = job != null? job.ID : (JobID?)null;
            optionalEquipmentAssignedToThisJob.Add(data.EntityID);
        }

        /// <summary>
        /// tests the distance of the journey to see if bringing optional equiment is warranted.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="job"></param>
        /// <param name="regionMapToUse"></param>
        /// <param name="radiusToLookIn"></param>
        /// <param name="stanceToUse"></param>
        /// <returns></returns>
        private void IsEquipmentNeeded(float groundDistanceToDestinationFromCamp, 
            out bool distanceOKToBringWeapons,
            out bool distanceOKToBringFood,
            out bool distanceOKToBringEquipment)
        {
            distanceOKToBringWeapons = false;
            distanceOKToBringFood = false;
            distanceOKToBringEquipment = false;

            if (groundDistanceToDestinationFromCamp > GameData.Instance.AIConstants.JobDistanceFromExpeditionToBringOptionalWeapons)
            {
                distanceOKToBringWeapons = true;                
            }

            if (groundDistanceToDestinationFromCamp > GameData.Instance.AIConstants.JobDistanceFromExpeditionToBringOptionalFood)
            {
                distanceOKToBringFood = true;
            }

            if (groundDistanceToDestinationFromCamp > GameData.Instance.AIConstants.JobDistanceFromExpeditionToBringOptionalEquipment)
            {
                distanceOKToBringEquipment = true;
            }
        }

       /* private void EquipWeaponIfNeeded(Vector3 destination, Job job, ThreatStance stanceToUse) 
        {

            float groundDistanceToDestinationFromCamp = Common.DistanceOctile(entityIntelligence.CurrentExpedition.Location, destination);
            float limit = 6 * MapManager.tileSize; // AI constant?


            float radiusToLookIn = Math.Min(groundDistanceToDestinationFromCamp, 8 * MapManager.tileSize); // AI constant?

            RegionMap footRegionMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(
                                            ProtectionLevel.Exposed,
                                            entityIntelligence.Allegiance.RepresentativeEntityType.ThreatCategory,
                                            stanceToUse)
                                            .RegionMap[SurfaceType.TransportType.Foot];

           
           
            if (groundDistanceToDestinationFromCamp > limit)
            {
                List<IKnownEntityData> weapons;

                GetNearbyEntities(radiusToLookIn, out weapons, stanceToUse, footRegionMap, IsWeaponForEquipment);

                List<EntityID> ammunitionToUse;
                IKnownEntityData bestWeapon;
                ScoreNearbyWeaponsAndSelectOne(weapons, job, out ammunitionToUse, out bestWeapon, footRegionMap, radiusToLookIn);


                if (bestWeapon != null)
                {
                    // add goalpickup, mount depending on job? or maybe GoalHaul will automatically dismount weapon
                    PickupItemOrUnloadFirst(bestWeapon, false, true, true, Compartment.Equipment);
                    if (ammunitionToUse != null)
                    {
                        AddSubgoal(new GoalReplenish(entity, bestWeapon.EntityID, ammunitionToUse, GoalReplenish.ReplenishAction.Reload, null, job));
                        foreach (var id in ammunitionToUse)
                        {
                            IKnownEntityData data = null;
                            if ((EntityResultCausesFailedGoal(entity.Intelligence.GetKnownData(id, out data))))
                            {
                                return;
                            }

                            data.AssignedToJob = job;
                            optionalEquipmentAssignedToThisJob.Add(id);
                        }
                    }

                    // set assigned property to job? to prevent others from taking it/forcing us to drop
                    bestWeapon.AssignedToJob = job;

                    // when top level goal OnExit - deassign the weapon (don't drop it though, the agent will carry it but others can now take it)
                    optionalEquipmentAssignedToThisJob.Add(bestWeapon.EntityID);//TODO: create as parameter instead

                }
            }
          
        }*/

        protected void RemoveLocksOnOptionalEquipment(JobID? jobID)
        {
            
            if (optionalEquipmentAssignedToThisJob != null)
            {
                foreach (var id in optionalEquipmentAssignedToThisJob)
                {
                    RemoveLockOnToolOrWeapon(jobID, id);
                }
            }
        }

        #endregion

        /// <summary>
        /// gets called from preconditions, and other places
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        protected bool IsToolOrWeaponOK(EntityID entityID, out IKnownEntityData weaponData, bool setStatusToFailed = true)
        {
            // lookup in sharedknowledge - handle null result with call to cleanup method - but without crashing foreach!!! use established patterns/methods to prevent bugs
          //  IKnownEntityData data;

            weaponData = null;

            EntityResult result = entityIntelligence.GetKnownData(entityID, out weaponData);

            if (UsedEntityResultShouldFailGoal(result))
            {
                if (setStatusToFailed)
                {
                    Status = Goals.Status.Failed;                     
                }
                
                return false;
            }

            return IsToolOrWeaponOK(weaponData);
        }

        protected bool IsToolOrWeaponOK(IKnownEntityData entityData)
        {
            if (!Entity.IsFunctional(entityData) //GoalEvaluator.ScoreIsEntityFunctional(entityData) <= 0d // test for broken parts 
                || entityData.OwnedBy == null) // test not discarded...
            {
                return false;
            }

            return true;
        }

        protected bool IsToolOrWeaponOK(EntityAndRoot? entityID, out IKnownEntityData weaponData)
        {
            weaponData = null;
            if (entityID.HasValue)
            {
                IsToolOrWeaponOK(entityID.Value.Entity, out weaponData);
            }

            return true;
        }

        protected bool IsToolOrWeaponOK(EntityID? entityID, out IKnownEntityData weaponData)
        {
            weaponData = null;
            if (entityID.HasValue)
            {
                IsToolOrWeaponOK(entityID.Value, out weaponData);
            }

            return true;
        }


        protected bool AreToolsOK(List<EntityID> tools)
        {
            if (tools != null)
            {
                foreach (var tool in tools)
                {
                    IKnownEntityData weaponData;
                    if (!IsToolOrWeaponOK(tool, out weaponData))
                        return false;
                }
            }

            return true;
        }

        protected bool IsOutputOKAndNotCompleted(ProcessJob job, bool mustExist)
        {
            if (job != null)
            {
                bool isCompleted;
                if (!job.IsCompleted(out isCompleted)
                    || isCompleted == true)
                {
                    return false;
                }

                bool outputExists;
                if (mustExist
                    && (!job.OutputExists(out outputExists) || !outputExists))
                {
                    return false;
                }

               /* IKnownEntityData outputData;
                if (!job.GetFirstOutputEntityData(out outputData)) // replace with IsCompleted and OutputExists
                {
                    return false;
                }

                if (outputData != null)
                {
                    return !outputData.IsCompleted();
                }
                else if (mustExist)
                {
                    return false;
                }*/
            }

            return true;
        }


        protected void DropUnneededItemsToMakeCapacity(float neededCapacity, Predicate<Entity> okToDrop, out float totalDroppedItems, StorageCompartment compartment)
        {
            ItemStorage itemStorage;

            totalDroppedItems = 0;

            itemStorage = entity.AgentStorage.GetCompartment(compartment);

            //First check if we need to drop any items
            if (!itemStorage.HasCapacityForItem(neededCapacity))
            {
                //We need to drop items to be able to do the job.
                //Calculate how much bulk we need to free
                float needToDrop = neededCapacity - itemStorage.UnusedCapacity;
                float totalDropped;

                //Drop unneeded items until we have enough bulk to pick up what ever we needed for the job
                DropUnneededItems(okToDrop, out totalDropped, compartment, needToDrop);
                totalDroppedItems = totalDropped;
            }


        }

        /// <summary>
        /// This method is used for weapon. 
        /// It will also check if it has to drop items to be able to carry the ammunition needed if there is any
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="ownersOfVehicles"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        protected bool GatherToolsOrWeapons(EntityAndRoot? weapon, List<EntityGroupID> ownersOfVehicles, Job job, StorageCompartment compartment)
        {

            if (weapon.HasValue)
            {
                IKnownEntityData itemData;
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(weapon.Value.Entity, out itemData)))
                {
                    return false;
                }
                List<IKnownEntityData> data = new List<IKnownEntityData>();
                data.Add(itemData);


                GatherToolsOrWeapons(data, ownersOfVehicles, job, true, compartment);

            }

            return true;
        }

        /// <summary>
        /// creates pickup goals and sets locks on items.
        /// returns false and sets status to Failed if there was a problem
        /// </summary>
        /// <param name="items"></param>
        /// <param name="ownersOfVehicles"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        protected bool GatherToolsOrWeapons(List<IKnownEntityData> items, List<EntityGroupID> ownersOfVehicles, Job job, bool mountAfterPickup, StorageCompartment compartment)
        {
            // the tools were sorted so that the closest one is first

            if (items != null)
            {
                // we use this to keep track of carrying capacity as we add drop and pickup goals:
                float? computedCapacity = null;  

                foreach (var item in items)
                {
                    // NEW: don't assign intrinsic tools/weapons. (check that they are in place?)
                    if (item.EntityType.IsIntrinsic())
                    {
                        continue;
                    }

                    if (!ToolType.IsImmovable(item.EntityType))
                    {
                        /*bool mountAfterPickup = false;
                        if (isWeapon == true)
                        {
                            mountAfterPickup = true;
                        }*/

                        if (!entity.AgentStorage.Contains(item.EntityID))
                        {

                            AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, item) { IsFinalDestination = true });


                            //if (!entity.AgentStorage.ItemStorage.HasCapacityForItem(tool.Bulk))
                            //{
                            //    // create drop goals to drop any items that we don't need if necessary to make room for the tool:
                            //    float needToDrop = tool.Bulk - computedCapacity;

                            //TODO:  Modify the predicate to take replenishment items for the tools into account!
                            float totalDropped;

                            DropUnneededItemsToMakeCapacity(item.Bulk, e => !items.Contains(e), out totalDropped, compartment);

                            if (computedCapacity == null)
                            {
                                // compute this late since some agents do not have storage, only intrinsic weapons/tools
                                computedCapacity = entity.AgentStorage.GetCompartment(compartment).UnusedCapacity;
                            }

                            computedCapacity = computedCapacity - item.Bulk + totalDropped;
                          

                            // OLD: 
                        /*    EntityGroup group = entity.Intelligence.Allegiance.SharedKnowledge.AllKnownEntities;
                            item.SetInUseBy(group, entity.EntityID);
                            */

                            if (!PickupItemOrUnloadFirst(item, mountAfterPickup, compartment: compartment)) 
                            {
                                return false;
                            }

                        }
                        else
                        {
                            // we had the tool already. make sure it is in the Haul compartment:
                            if (mountAfterPickup)
                            {
                                entity.AgentStorage.MountedToolOrWeapon = item.EntityID;
                            }
                            else
                            {
                                entity.AgentStorage.MoveCarriedItemToCompartment((Entity)item, compartment); // Compartment.Haul);
                            }
                        }
                    }
                }
            }


            // NEW: set the InUseBy lock on ALL tools
            SetLocksOnToolsOrWeapons(items, job);


            return true;
        }




        protected bool AssignWeapon(Job job, EntityAndRoot? weapon)
        {
            if (weapon.HasValue)
            {
                IKnownEntityData itemData;
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(weapon.Value.Entity, out itemData)))
                {
                    return false;
                }

                /*   if (assign)
                   {*/
                if (job != null)
                {
                    itemData.AssignedToJob = job.ID;
                }
                else
                {
                    itemData.AssignedToJob = null;
                }
                /*  }
                  else
                  {
                      if (itemData.AssignedToJob == job)
                      {
                          itemData.AssignedToJob = null;
                      }

                      // really needed..?
                      EntityGroup group = entity.Intelligence.Allegiance.SharedKnowledge.AllKnownEntities;

                      EntityID? inUseBy = itemData.GetInUseBy(group);
                      if (inUseBy != null && inUseBy == entity.EntityID)
                      {
                          itemData.ClearInUseBy(group);
                          //itemData.InUseBy = null;
                      }
                  }*/
            }

            return true;

        }




        protected bool SetLocksOnReplenishItems(Job job, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishActions)
        {
            if (replenishActions != null)
            {
                foreach (var item in replenishActions)
                {
                    if (!SetLocksOnReplenishItems(job, item.Value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected bool RemoveLocksOnReplenishItems(JobID? job, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishActions)
        {
            if (replenishActions != null)
            {
                foreach (var item in replenishActions)
                {
                    if (!RemoveLocksOnReplenishItems(job, item.Value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected bool SetLocksOnReplenishItems(Job job, List<ReplenishItemsForAction> replenishActions) //, bool assign)
        {
            IKnownEntityData itemData;
            if (replenishActions != null)
            {
                foreach (var action in replenishActions)
                {
                    foreach (var replenishItem in action.Items)
                    {
                        if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(replenishItem, out itemData)))
                        {
                            /* if (assign == true)
                             {*/
                            return false; // abort!!
                            /*  }
                              else
                              {
                                  continue; // continue with the rest if there was a problem with this one...
                              }*/
                        }

                        if (itemData != null && job != null)
                        {                            
                            itemData.AssignedToJob = job.ID;                           
                        }

                    }
                }
            }

            return true;

        }


        protected bool RemoveLocksOnReplenishItems(JobID? job, List<ReplenishItemsForAction> replenishActions)
        {
           // IKnownEntityData itemData;
            if (replenishActions != null)
            {
                foreach (var action in replenishActions)
                {
                    foreach (var replenishItem in action.Items)
                    {
                        RemoveLockOnToolOrWeapon(job, replenishItem);
                    }
                }

                // NEW: also deassign any surplus ammo items that may have been created and carried:
                if (entity.Contains != null && job.HasValue)
                {
                    entity.Contains.IterateContained((e) => RemoveLocksOnSurplusItems(job.Value, replenishActions, e));
                }

            }

            return true;

        }

        /// <summary>
        /// handle any new surplus items also
        /// </summary>
        /// <param name="job"></param>
        /// <param name="replenishActions"></param>
        /// <param name="item"></param>
        private void RemoveLocksOnSurplusItems(JobID job, List<ReplenishItemsForAction> replenishActions, Entity item)
        {
            if (item.AssignedToJob == job
                && replenishActions.Exists(p => p.Action.InputsByType.ContainsKey(item.EntityType)))
            {
                RemoveLockOnToolOrWeapon(job, item.ID);

               // item.AssignedToJob = null;
            }
        }

        /// <summary>
        /// drop any items that we don't need. the bulk amount needed to drop is optional
        /// </summary>
        /// <param name="neededItems"></param>
        private void DropUnneededItems(Predicate<Entity> okToDrop, out float totalDropped, StorageCompartment compartment, float? bulkNeededToDrop = null)
        {
            //float bulkAlreadyDropped = 0f;

            float totalDroppedParam = 0f;

            // here I add some extra parameters to the delegate. this trick is called "currying a method": 
            entity.AgentStorage.ItemStorage.IterateContainedBreakOnTrue((itemEntity) => DropUnneededItem(itemEntity, entity, okToDrop, bulkNeededToDrop, ref totalDroppedParam)); // bulkAlreadyDropped)); 

            // the compiler demanded this..:
            totalDropped = totalDroppedParam;
                      

        }

        public bool DropUnneededItem(Entity itemEntity, Entity carrierEntity, Predicate<Entity> okToDrop, float? bulkNeededToDrop, ref float bulkAlreadyDropped)
        {
            if (okToDrop(itemEntity))
            {
                AddSubgoal(new GoalDropItem(carrierEntity, itemEntity.EntityID));

                bulkAlreadyDropped += itemEntity.Bulk;

                if (bulkNeededToDrop.HasValue)
                {
                    if (Common.IsLessThanOrEqual(bulkNeededToDrop.Value, bulkAlreadyDropped))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        //protected void DestroyJobAndRemoveLocks<T>(ref T jobToDestroy, bool cancelTakers) where T : Job
        //{
        //    jobToDestroy.Abandon(entity);



        //    DestroyJob(ref jobToDestroy, cancelTakers);
        //}



       /* protected void RemoveLocksFromJob<T>(T jobToUnlock, List<EntityID> Tools = null,
            Dictionary<EntityID, List<ReplenishItemsForAction>> replenishActionsDictionary = null,
            List<ReplenishItemsForAction> replenishActionsList = null,
            EntityID? weapon = null) where T : Job*/





        /*         
         * Unassign similar to GoalProduce.Activate
         * 
           1. if cancelling: 
         remove InUseBy locks on ALL tools
         NEW: Remove AssignedToJob on non-immobile tools which are not in use for a started, unattended process! 

         2. if completing the process:
         remove InUseBy locks on ALL tools
         remove AssignedToJob locks on ALL tools (is handled by Job.CleanupAfterDestroyedProcess())

         3. if completing a goal and leaving an ongoing process after starting it:
         remove InUseBy locks on ALL tools
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobToUnlock"></param>
        /// <param name="Tools"></param>
        /// <param name="replenishActionsDictionary"></param>
        /// <param name="replenishActionsList"></param>
        /// <param name="weapon"></param>
        protected void RemoveProcessToolLocks(ProcessJob jobToUnlock, List<EntityID> Tools = null,
           Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishActionsDictionary = null,
           List<ReplenishItemsForAction> replenishActionsList = null,
           EntityAndRoot? weapon = null)
        {
         
            // cooking: remove the lock on the cooking pot since the job is attended.
            // harvest sap: leave the lock on the tapping bucket since the job is unattended.
            List<EntityID> toolsToUnassign;

            List<EntityID> handTools, mobileTools;

            GoalDoProduce.GetMobileTools(Tools, out mobileTools); // not immobile
            GoalDoProduce.GetHandTools(Tools, out handTools); // hand tools only

            JobID? jobID = jobToUnlock != null ? (JobID?)jobToUnlock.ID : null;


            if (jobToUnlock.ProcessType.WorkNeeded == WorkerNeededOptions.WorkerNeeded)
            {
               // GoalDoProduce.GetMobileTools(Tools, out toolsToUnassign);
                toolsToUnassign = mobileTools;
            }
            else
            {
                bool isStarted;
                if (jobToUnlock.IsStarted(out isStarted) && isStarted)
                { 
                    // for unattended jobs, don't release the lock on stationary tools (tapping bucket) when the worker leaves.
                    // it will be unassigned when the job is destroyed.
                    // but if the job hasn't started yet, then it's ok to release the lock.

                    toolsToUnassign = handTools;
                }
                else
                {                   
                    // GoalDoProduce.GetHandTools(Tools, out toolsToUnassign);
                    toolsToUnassign = mobileTools;
                }

                ClearInUseBy(entity, mobileTools); // clear inUseBy on the stationary tool also!           
            }
           
            
            // this removes both AssignedToJob and InUseBy locks:
            RemoveLocksFromJob(jobID,
                toolsToUnassign, replenishActionsDictionary,
                replenishActionsList, weapon);


            List<EntityID> immobileTools;
            GoalDoProduce.GetImmobileTools(Tools, out immobileTools);

            // this leaves assignedTo for immobile tools:
            ClearInUseBy(entity, immobileTools);
            

        }

      
        protected void ClearInUseBy(Entity entity, List<EntityID> tools)
        {
           
            IKnownEntityData toolOrWeaponData;

            foreach (var tool in tools)
            {
                entityIntelligence.GetKnownData(tool, out toolOrWeaponData);

                if (toolOrWeaponData != null)
                {
                    // don't assign intrinsic tools/weapons. 
                    if (toolOrWeaponData.EntityType.IsIntrinsic())
                    {
                        return;
                    }

                    entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(toolOrWeaponData.EntityID, entity.EntityID);  
                }
            }            
        }


        protected void RemoveLocksFromJob(Job jobToUnlock, List<EntityID> Tools = null,
            Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishActionsDictionary = null,
            List<ReplenishItemsForAction> replenishActionsList = null,
            EntityAndRoot? weapon = null, bool isDestroyingJob = false)
        {
            RemoveLocksFromJob(jobToUnlock != null ? (JobID?)jobToUnlock.ID : null,
                Tools, replenishActionsDictionary,
                replenishActionsList, weapon, isDestroyingJob);

        }

        protected void RemoveLocksFromJob(JobID? jobID, List<EntityID> Tools = null,
            Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishActionsDictionary = null,
            List<ReplenishItemsForAction> replenishActionsList = null,
            EntityAndRoot? weapon = null, bool isDestroyingJob = false)
        {
            // I want to remove as many locks as possible, even when Job is null or JobID is null

            Job job = LookUp<Job, JobID>.FindByID(jobID);
            if (job != null)
            {
                job.Abandon(entity, isDestroyingJob);
            }

            ProcessJob pJob = job as ProcessJob;
            if (pJob != null)
            {
                RemoveLockOnActingOn(pJob); // NEW
            }

            RemoveLocksOnOptionalEquipment(jobID);

            if (Tools != null)
            {
                RemoveLocksOnTools(Tools, jobID);
            }

            if (replenishActionsDictionary != null)
            {
                // de-assign replenish items:
                RemoveLocksOnReplenishItems(jobID, replenishActionsDictionary);
            }
            else if (replenishActionsList != null)
            {
                RemoveLocksOnReplenishItems(jobID, replenishActionsList);
            }

            if (weapon != null)
            {
                RemoveLockOnToolOrWeapon(jobID, weapon.Value);
            }
        }

        private void RemoveLockOnActingOn(ProcessJob pJob)
        {
            EntityID? actingOnID;
            if (pJob.GetActingOnEntity(out actingOnID)
                && actingOnID.HasValue)
            {               
                IKnownEntityData entityData;
                entityIntelligence.GetKnownData(actingOnID.Value, out entityData);

                if (entityData != null)
                {
                    if (entityData.AssignedToJob != null && entityData.AssignedToJob == pJob.ID)
                    {
                        entityData.AssignedToJob = null;
                    }
                }
            }
        }


        /// <summary>
        /// Generic because C# does not support polymorphism with the ref keyword!
        /// 
        /// Sets job reference to null, makes its ID invalid too! Make sure that the goal ends without referencing it again, for example in Deactivate...
        ///          
        /// 
        /// OLD:
        /// When choosing to cancelTakers make sure you do no longer use the goal after this call as it will get invalidated. 
        /// Do not add any subgoals or any other thing that can cause the snappshotting to crash and this is due to all references to this goal
        /// should now have been removed.
        ///       
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="goalHasBeenDestroyed"></param>
        /// <param name="jobToDestroy"></param>
        /// <param name="cancelTakers"></param>
        /// <param name="Tools"></param>
        /// <param name="replenishItemsForTools"></param>
        /// <param name="weapon"></param>        
        protected void DestroyJobAndRemoveLocks<T>(ref T jobToDestroy, 
            List<EntityID> Tools = null,
            Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools = null,
            List<ReplenishItemsForAction> replenishActions = null,
            EntityAndRoot? weapon = null
            ) where T : Job
        {

            RemoveLocksFromJob(jobToDestroy, Tools, replenishItemsForTools, replenishActions, weapon, true);
            
            // don't send a message to cancel goal owner...
            jobToDestroy.Destroy(true, entity);
            jobToDestroy = null;

        }


        protected void RemoveLocksOnTools(List<EntityID> tools, JobID? job)
        {            
            foreach (var tool in tools)
            {
                RemoveLockOnToolOrWeapon(job, tool);
            }
        }


      /*  protected bool DeassignWeapon(JobID job, EntityID? weapon)
        {
            if (weapon.HasValue)
            {
                IKnownEntityData itemData;
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(weapon.Value, out itemData)))
                {
                    return false;
                }


                if (itemData.AssignedToJob != null && itemData.AssignedToJob.ID == job)
                {
                    itemData.AssignedToJob = null;
                }

                // really needed..?
                EntityGroup group = entity.Intelligence.Allegiance.SharedKnowledge.AllKnownEntities;

                itemData.ClearInUseBy(group, entity.EntityID);
                //itemData.InUseBy = null;

            }

            return true;

        }*/

       /* protected void RemoveLockOnToolOrWeapon(Job job, EntityID? toolOrWeapon)
        {
            RemoveLockOnToolOrWeapon(job != null ? (JobID?)job.ID : null, toolOrWeapon);
        }*/

        protected void RemoveLockOnToolOrWeapon(JobID? jobID, EntityAndRoot? toolOrWeapon)
        {
            if (toolOrWeapon == null)
            {
                return;
            }

            RemoveLockOnToolOrWeapon(jobID, toolOrWeapon.Value.Entity);
        }

        protected void RemoveLockOnToolOrWeapon(JobID? jobID, EntityID? toolOrWeapon)
        {
            if (toolOrWeapon == null)
            {
                return;
            }

            IKnownEntityData toolOrWeaponData;

            entityIntelligence.GetKnownData(toolOrWeapon.Value, out toolOrWeaponData);

            if (toolOrWeaponData != null)
            {
                // don't assign intrinsic tools/weapons. 
                if (toolOrWeaponData.EntityType.IsIntrinsic())
                {
                    return;
                }

                Job job = LookUp<Job, JobID>.FindByID(jobID);
                if (job != null)
                {
                    // for process jobs
                    ProcessJob processJob = job as ProcessJob;

                    if (processJob != null)
                    {
                        float progress;
                        processJob.GetKnownProgress(out progress);

                        if (!ToolType.IsImmovable(toolOrWeaponData.EntityType)
                       || NonLivingEntity.IsCompleted(progress))
                        {
                            // NEW: don't let agents unassign immovable tools on unfinished processes, for instance when being cancelled by others... 
                            // this is because the job depends on an assigned tool in order to have a location.
                            // if the tool should be unassigned, do it from the evaluator.

                            // remove two-way assigment, used in client feedback (task panel)
                            processJob.UnassignTool(toolOrWeaponData);
                        }
                    }
                
                }

                // for other jobs:
                if (toolOrWeaponData.AssignedToJob != null && toolOrWeaponData.AssignedToJob == jobID)
                {
                    toolOrWeaponData.AssignedToJob = null;
                }
              
                entityIntelligence.Allegiance.SharedKnowledge.ClearInUseBy(toolOrWeaponData.EntityID, entity.EntityID);     
                
            }
        }

        protected void SetLocksOnToolsOrWeapons(List<IKnownEntityData> items, Job job)
        {           
            if (items != null)
            {
                foreach (var item in items)
                {
                    SetLocksOnToolOrWeapon(job, item);
                }
            }
        }

        protected void SetLocksOnToolsOrWeapons(List<EntityID> toolsOrWeapons, Job job)
        {
            foreach (var tool in toolsOrWeapons)
            {
                SetLocksOnToolOrWeapon(job, tool);
            }
        }

        protected void SetLocksOnToolOrWeapon(Job job, EntityID toolOrWeapon)
        {
            IKnownEntityData toolOrWeaponData;
            entityIntelligence.GetKnownData(toolOrWeapon, out toolOrWeaponData);


            if (toolOrWeaponData != null)
            {
                SetLocksOnToolOrWeapon(job, toolOrWeaponData);
            }
        }

        private void SetLocksOnToolOrWeapon(Job job, IKnownEntityData toolOrWeapon)
        {
            // NEW: don't assign intrinsic tools/weapons. 
            if (toolOrWeapon.EntityType.IsIntrinsic())
            {
                return;
            }


            ProcessJob pJob = job as ProcessJob;
           
            entityIntelligence.Allegiance.SharedKnowledge.SetInUseBy(toolOrWeapon.EntityID, entity.EntityID);          

            if (pJob != null && toolOrWeapon.EntityType.ToolType != null)
            {
                // only tools, not weapons, have a 2-way assignment
                pJob.AssignTool(toolOrWeapon);
            }
            else
            {
                toolOrWeapon.AssignedToJob = job.ID;
            }

        }

        
       


        protected void ReplenishToolsOrWeapons(
             // Dictionary<EntityID, List<ReplenishItemsForAction>> replenishItemsForTools,
            Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools,
            List<EntityGroupID> ownersOfVehicles, Job job, StorageCompartment compartment = StorageCompartment.Haul)
        {
            // now replenish if needed (the evaluator has already determined that it is possible with this set of tools):
            if (replenishItemsForTools != null)
            {
                foreach (var item in replenishItemsForTools)
                {
                    foreach (var replenishAction in item.Value)
                    {
                        AddSubgoal(new GoalReplenish(entity, item.Key,
                            replenishAction.Items,
                            replenishAction.Action,
                            ownersOfVehicles, job, compartment));
                    }
                }
            }
        }


        protected void PrepareTools(List<IKnownEntityData> tools, List<EntityGroupID> ownersOfVehicles)
        {
            if (tools != null)
            {
                //RequiresEnergy energy;
                foreach (var tool in tools)
                {
                    if (tool.EntityType.ToolType.PrepareProcessType != null) //RequiresEnergyType.RequiresFuelType != null) // tool.Find(out energy))
                    {
                        if (tool.IsPrepared.HasValue && tool.IsPrepared.Value == false) // job.FireSite.Structure.Fireplace.IsLit)
                        {
                          //  AddSubgoal(new GoalPrepareTool(entity, tool.EntityType.ToolType.PrepareAction, tool.EntityID, ownersOfVehicles));
                            AddSubgoal(new GoalProduce(entity, tool.EntityType.ToolType.PrepareProcessType, tool.GetAsEntityAndRoot(),  null, ownersOfVehicles, 
                                null, null, null, null)); // tools not supported for this process.

                        }
                    }
                }
            }
        }

        /// <summary>
        /// for non-hand tools
        /// </summary>
        /// <param name="tools"></param>
        /// <param name="ownersOfVehicles"></param>
        /// <param name="job"></param>
        protected void PlaceStationaryToolsAtWorkSite(List<IKnownEntityData> tools, Vector3? location)
        {
            if (tools != null)
            {
                foreach (var tool in tools)
                {
                    if (tool.EntityType.ToolType.ToolHandling == ToolHandlingType.Stationary
                        && !ToolType.IsImmovable(tool.EntityType)) 
                    {
                        // TODO: spread the tools out a bit...
                       /* if (location == null)
                        {                           
                            if (!job.GetCurrentJobLocation(out location) 
                                || !location.HasValue)
                            {
                                Status = Goals.Status.Failed;
                                return false;
                            }
                        }*/

                        AddSubgoal(new GoalDropItem(entity, tool.EntityID, null, location, null));
                    }
                }
            }

          //  return true;
        }



        public override string GetStatus()
        {
            if (Subgoals.Count > 0)
            {
                string status = Subgoals.Peek().GetStatus();
                if (status != "")
                {
                    return status;
                }
                else return base.GetStatus();
                // return Subgoals.Peek().GetStatus();
            }
            else return base.GetStatus();
        }

        /// <summary>
        /// this gets called to get a clean slate. Only the goals that have been Entered will have their OnExit called.
        /// </summary>
        public virtual void RemoveAllSubgoals()
        {
           
            while (Subgoals.Count > 0)
            {
                Goal subGoal = RemoveFirstSubgoal();
                                             
                subGoal.Terminate();

                /*
#if DEBUG
                AssertAllLocksReleased(subGoal);
#endif*/
            }

        }


        public override void Terminate()
        {
            RemoveAllSubgoals();
            base.Terminate();
        }

        /// <summary>
        /// Skipps assert added in SystemDiagnostics as we want to ignore it when it happens in an GoalTraverseEdgeBetweenWaypoints.
        /// </summary>
        public bool SkipThisAssert = false;

        /// <summary>
        /// also calls OnEnter if the goal is the first in the queue...
        /// </summary>
        /// <param name="g"></param>
        public override void AddSubgoal(Goal g)
        {
            //We should not add subgoals to a Goal that is invalid.
            if (ID == GoalID.Invalid)
            {
                //This was happening to goal attack repeatedly. Solve this first then add assert again. Both here and in GoalThink (Why duplicate places with this code?)
                if (SkipThisAssert == false)
                {
                    System.Diagnostics.Debug.Assert(ID != GoalID.Invalid, "Invalid ID. Never add subgoal to Goal with invalid ID.");
                }
                return;
            }
            //add the new goal to the end of the list


            if (entity.PersonEntity != null && g is GoalWait && Subgoals.Count > 2 && Subgoals.Peek() is GoalWait)
            {

            }

            bool firstSubgoal = Subgoals.Count == 0;

            Subgoals.Enqueue(g);

            if (firstSubgoal)
            {
                g.EnterIfNew();
            }
        } 

        //if a child class of Goal_Composite does not define a message handler
        //the default behavior is to forward the message to the front-most
        //subgoal
        public override bool HandleMessage(Message message)
        {
            return ForwardMessageToFrontMostSubgoal(message);
        }

        //  passes the message to the goal at the front of the queue
        protected bool ForwardMessageToFrontMostSubgoal(Message message)
        {
            if (The.Sim.TotalUnPausedGameTimeInSeconds > 24 &&
                entity.ID == (Entities.EntityID)19 && 
                ( message.MessageType == Message.MessageTypes.Hit || message.MessageType == Message.MessageTypes.HitAndCollapse))
            {

            }

            // should Messages be handled by Inactive goals?? does not seem right

            if (Subgoals.Count > 0)
            {
                if (Subgoals.Peek().isActive()) //NEW: there was a crash when an Inactive GoalMove received a PathFound message.
                { 
                    return Subgoals.Peek().HandleMessage(message);
                }
            }

            //return false if the message has not been handled
            return false;
        }

        //if a child class of Goal_Composite does not define a trigger handler
        //the default behavior is to forward the trigger to the front-most
        //subgoal
        /*    public override bool HandleTrigger(Trigger trigger)
            {
                return ForwardTriggerToFrontMostSubgoal(trigger);
            }

            //  passes the trigger to the goal at the front of the queue
            protected bool ForwardTriggerToFrontMostSubgoal(Trigger trigger)
            {
                if (Subgoals.Count > 0)
                {
                    return Subgoals.Peek().HandleTrigger(trigger);
                }

                //return false if the trigger has not been handled
                return false;
            }*/

        #region ISnapshot

        Snapshotter.Version version;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.optionalEquipmentAssignedToThisJob = sn.DoList(optionalEquipmentAssignedToThisJob);
            this.ownersOfVehicles = sn.DoList(ownersOfVehicles);
                            

            if (sn.mode != Snapshotter.Mode.Load)
            {
                foreach (var item in Subgoals)
                {
                    System.Diagnostics.Debug.Assert(item.ID != GoalID.Invalid, "Invalid ID???");

                    this.snapshotSubgoals.Enqueue(item.ID);
                }
            }
            this.snapshotSubgoals = sn.DoQueue(snapshotSubgoals);

            sn.Ignore(SkipThisAssert);
            sn.Ignore(Subgoals);
            sn.Ignore(longerJourneyTask);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
         

            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            if (entity.ToString().Contains("Millet"))
            {

            }

            foreach (var item in snapshotSubgoals)
            {
                System.Diagnostics.Debug.Assert(item != GoalID.Invalid, "Invalid ID???");

                Subgoals.Enqueue(LookUpGoals.FindByID(item));
            }
            snapshotSubgoals.Clear();

        }

        #endregion
    }
}
