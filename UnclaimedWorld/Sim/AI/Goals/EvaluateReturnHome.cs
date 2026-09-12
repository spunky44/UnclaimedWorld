using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using GameStateManagement;
using UWGame.SimSide.AI.Pathfinding;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// return home when we have strayed far and are doing nothing.
    /// 
    /// The teleport fix will prevent characters being held in very tiny prisons...
    /// </summary>
    public class EvaluateReturnHome: GoalEvaluator
    {        
       
        List<EntityGroup> ownersOfVehicles;

        bool useBoldStance = false;

      //  bool hasConsideredTeleporting = false;
        Vector3? teleportTo = null;


        Regulator trappedDetectionRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.01, "EvaluateReturnHome");


        public EvaluateReturnHome(Entity entity, List<EntityGroup> ownersOfVehicles = null)
            : base(entity)
        {

            this.ownersOfVehicles = ownersOfVehicles;
        }

        public override float Priority
        {
            get
            {
                return 1f;
            }
        }

        public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
        {
            if (entity.Name == "Ward Conlan")
            {

            }

            if(entityIntelligence.CurrentExpedition == null) // binal rats have no expedition? or is it only on test? 
            {
                result = 0;
                return CalculateResult.Done;
            }

            bool isPastDesperationTime;
            bool isPastTeleportTime;
            double idleTimeScore = ScoreIdleTime(out isPastDesperationTime, out isPastTeleportTime);
            if (idleTimeScore > 0.3)
            {
                double distanceScore;

                useBoldStance = false;
                if (isPastDesperationTime)
                {
                    useBoldStance = true; // make a run for it...
                }

                RegionMap.Result distanceResult = ScoreDistanceFromHome(useBoldStance, out distanceScore);

                if (distanceResult == RegionMap.Result.Wait)
                {
                    // wait for the result...                   
                    return CalculateResult.Processing;
                }
                else if (distanceResult == RegionMap.Result.NoAccess)
                {
                    if (DoTeleport(isPastTeleportTime))
                    {
                       // hasConsideredTeleporting = false; // allow it again

                        distanceScore = 1d; // we will teleport a short distance
                    }
                    else
                    {
                        result = bestScore = 0;
                        return CalculateResult.Done;
                    }                   
                }

                if (distanceScore > 0)
                {
                   
                    // the goal should only be a little better than idling, but not better than any other jobs.
                    //bestScore = 1.1 * GameData.Instance.AIConstants.IdleGoalDesirability + (1 - entity.Intelligence.Morale); 
                    bestScore = 1.1 * (GameData.Instance.AIConstants.IdleGoalDesirability + GameData.Instance.AIConstants.CurrentGoalInertia) 
                        + (1 - entity.Intelligence.Morale); 
                    
                }
                else
                {
                    bestScore = 0;
                }
            }
            else
            {
                bestScore = 0;
            }

            result = bestScore;

            return CalculateResult.Done;            
        }


        private bool DoTeleport(bool isPastTeleportTime)
        {
            if (entity.EntityType.IntelligenceType.AllowEscapeFromTinyAreas == true //Person != null // only persons...
                && isPastTeleportTime) // && !hasConsideredTeleporting)
            {
                if (trappedDetectionRegulator.IsReady())
                {
                    if (IsBlockedInTinyArea())
                    {
                        teleportTo = FindTeleportLocation();
                        return true;
                    }

                 //   hasConsideredTeleporting = true;
                }
            }

            return false;
        }


        private bool IsBlockedInTinyArea()
        {
            // do a flood fill from agent location. if the fill distance is greater thatn X, return false.

            FloodFill floodFill = new FloodFill();

            SubtileLayers terrainCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];


            // create rectagnel centered on the agent. clamp it to the map bounds:
            int subtileRadius = (int)(GameData.Instance.AIConstants.MaximumRadiusOfTrapAreaToTeleportFrom / (float)MapManager.subTileSize);
            Point agentPos = MapManager.WorldPosToSubtile(entity.PlaySiteLocation);
            Point topLeft = new Point(agentPos.X - subtileRadius, agentPos.Y - subtileRadius);
            int width = 2 * subtileRadius;
            int height = 2 * subtileRadius;
            Rectangle subtileArea = The.Map.GetClampedMapAreaUsingSubTiles(topLeft, width, height);

            ushort[][] nodes = null;
            byte[][] nodeDistances = null;

            Common.InitJaggedArray(ref nodes, subtileArea.Width, subtileArea.Height);
            Common.InitJaggedArray(ref nodeDistances, subtileArea.Width, subtileArea.Height);


            if (floodFill.DoFloodFillOfArea(terrainCosts, nodes, MapManager.WorldPosToSubtile(entity.AccessPoint.Value), subtileArea, nodeDistances, subtileRadius) 
                == FloodFill.FloodFillResult.MaxRadiusReached)
            {
                return false;
            }


            return true;
     
        }

        private Vector3? FindTeleportLocation()
        {
            // search radially for a location that has access to home
            // use accesspoint code??
            Vector3 startLocation = entity.AccessPoint.Value;

            RegionMap regionMap = The.Map.TerrainCosts[SurfaceType.TransportType.Foot].RegionMap;
            Point destination = GetHomeDestination();

            Vector3 location = MapManager.FindUnblockedLocation(startLocation, GameData.Instance.AIConstants.MaximumRadiusOfTrapAreaToTeleportFrom,
                MapManager.ScanMethod.HalfCircle, entityIntelligence.CurrentExpedition.Center, true, s => SubtileCanReachDestination(s, regionMap, destination));

            if (MapManager.WorldPosToSubtile(startLocation) == MapManager.WorldPosToSubtile(location))
            {
                return null;
            }
            else return location;

        }

        private bool SubtileCanReachDestination(SubtilePos pos, RegionMap regionMap, Point destinationSubtilePos)
        {
           // Point fromSubtilePos = MapManager.WorldPosToSubtile(entity.AccessPoint.Value);
          //  Point destinationSubtilePos = MapManager.WorldPosToSubtile(entityIntelligence.CurrentExpedition.Center);
          //  Point destinationSubtilePos = GetHomeDestination();

            float distance = -1f;

            RegionMap.Result result1 = regionMap.GetDistance(entity, pos.ToPoint(), destinationSubtilePos, ref distance);

            if (result1 != RegionMap.Result.OK)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        /// <summary>
        /// gives a higher score the further we are from home
        /// </summary>
        /// <param name="distanceScore"></param>
        /// <returns></returns>
        private RegionMap.Result ScoreDistanceFromHome(bool useBoldStance, out double distanceScore)
        {
            distanceScore = 0;
            ThreatStance threatStanceToUse;
            RegionMap regionMapToUse = GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse, useBoldStance);


            Point fromSubtilePos = MapManager.WorldPosToSubtile(entity.AccessPoint.Value);
            Point destinationSubtilePos = GetHomeDestination();

            float distance = -1f;


            RegionMap.Result result1 = regionMapToUse.GetDistance(entity, fromSubtilePos, destinationSubtilePos, ref distance);

            if (result1 != RegionMap.Result.OK)
            {
                return result1;
            }
            else
            {
                // got the distance. Compute the score:
               // double timeCost;

                if (distance > GameData.Instance.AIConstants.DistanceFromExpeditionToReturnHome) 
                {
                    distanceScore = Common.Clamp(distance * 0.0001f, 0f, 1f);
                }
                else distanceScore = 0f;

            }

            return result1;
        }

        private Point GetHomeDestination()
        {
            Point destinationSubtilePos = MapManager.WorldPosToSubtile(entityIntelligence.CurrentExpedition.Center.Value); // what if this is blocked?
            return destinationSubtilePos;
        }

        private double ScoreIdleTime(out bool isPastDesperationTime, out bool isPastTeleportTime)
        {         
            double totalSeconds = entityIntelligence.Memory.TimeSpentIdling.TotalSeconds;
            double timeToSpendBeforeHeadingHome = GameData.Instance.AIConstants.TimeSpentIdlingToConsiderReturningHome * (entity.Intelligence.Morale);

            isPastDesperationTime = false;
            isPastTeleportTime = false;

            if (totalSeconds > timeToSpendBeforeHeadingHome) // 5)
            {
                if (totalSeconds > GameData.Instance.AIConstants.TimeSpentIdlingToConsiderReturningHomeWithBoldStance)
                {
                    isPastDesperationTime = true;
                }

                if (totalSeconds > GameData.Instance.AIConstants.TimeSpentIdlingToConsiderTeleporting)
                {
                    isPastTeleportTime = true;
                }

                return Common.Clamp((totalSeconds - timeToSpendBeforeHeadingHome)
                    / (4 * timeToSpendBeforeHeadingHome), 0.0, 1.0);
            }
            else return 0;
        }





        public override bool CancelCurrentTakers()
        {
            return true; throw new NotImplementedException();
        }
        public override bool CanTakeGoal()
        {
            return true;// throw new NotImplementedException();
        }

       
      
        public override bool SetGoal()
        {
            base.SetGoal();

            entityIntelligence.SetTopLevelGoal(new GoalReturnHome(entity, GetOwnerIDs(ownersOfVehicles), useBoldStance, teleportTo), bestScore);
           
            teleportTo = null;

            return true;    
        }
    }
}
