using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// we don't put this data in ThreatJob because an entity may be attacked by multiple allegiances
    /// </summary>
    public class CombatInfo: ISnapshot
    {
      
        /// <summary>
        /// what is this entity currently targeting for attack?
        /// </summary>
        public EntityID? Target;


       

        public CombatInfo()
        {
            
        }



        public static bool GetRangedLocation(Entity attacker, IKnownEntityData target, AttackType attackType, out Vector3 rangedLocation )
        {
            rangedLocation = attacker.PlaySiteLocation; // with any luck, attacker is already in range

            if (attackType.MaxRange == null)
                return false;

            Vector3 offsetToTarget = attacker.PlaySiteLocation - target.PlaySiteLocation;
            double distanceToTargetSquared = offsetToTarget.LengthSquared();
            //float dist = offsetToTarget.Length();
           
            // if already in attack range, serendipity, just assign attacker's current position 
            if (distanceToTargetSquared < attackType.MaxRangeSquared)
                return true;//attacker already in range

            // MLo TODO not sure if thie works correctly
            // TODO MLo ranged attack should chase the current 'best' location within attack range
            Common.GetLocationAtDistance(target.Location.Value, attacker.Location.Value, (float)attackType.MaxRange, out rangedLocation);

            return false; //attacker is not already in range 
        }





        public static bool GetMeleeLocation(Entity attacker, IKnownEntityData target, out Vector3? meleeLocation, out bool isCenterLocation)
        {
           
            if (target.IsMoving == true
                && Common.DistanceOctile(target.PlaySiteLocation, attacker.PlaySiteLocation) > 22f)
            {
                // don't bother with positions when the target is still moving around - just chase the center location and hack when in range...
                meleeLocation = target.Location; // attacker.Intelligence.Allegiance.SharedKnowledge.GetLocation(target);

                isCenterLocation = true;
            }
            else
            { 
                // target is standing still or very close - compute an attack position

                isCenterLocation = false;
               
                Intelligence intelligence = attacker.Intelligence;
                                
             
            //    Point topLeftSubtilePositionOfMap;
                SubtileInfluence subTileMap = DrawMeleePositionInfluenceMap(attacker, target);

                // the proposed member position:
               // Vector3 attackPosition; // = groupMoveActivity.ComputePositionFromLeader(attacker, leadersNextWaypoint.Location, rotMatrix);
                
                Point bestSubtilePos;
                if (InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subTileMap.Values, out bestSubtilePos) == -1)
                {
                    meleeLocation = null;
                    return false; // no room at all
                }
                else
                {
                    //Note that we may get assigned a position that isn't within range of the target! 'Holding' pattern?
                    meleeLocation = MapManager.SubTileToWorldPos(new Point(subTileMap.TopLeftSubtilePositionOfMap.X + bestSubtilePos.X, subTileMap.TopLeftSubtilePositionOfMap.Y + bestSubtilePos.Y)).ToVector3();

                }

                // see if there is a clear line of sight (and movement) from the melee position to the target
            /*    MovementMap memberMoveMap = UWGame.SimSide.Instance.Map.GetMovementMap(intelligence.ProtectionLevel, attacker.EntityType.ThreatCategory, intelligence.ThreatStance);
                TerrainType.TransportType attackerTransport = attacker.GetTransportType();

                if (MapManager.IsPathClearToPoint(attackPosition, Entity.GetLocationWhenInVehicle(), memberMoveMap, attackerTransport))
                {
                    newWaypoint = new Waypoint(leadersNextWaypoint.Number, attackPosition, isLastEdge);
                }
                else
                {
                    // the line is blocked.
                    // make a map search to find closest valid waypoint
                    MovementMap moveMap = UWGame.SimSide.Instance.Map.GetMovementMap(intelligence.ProtectionLevel,
                        member.EntityType.ThreatCategory, intelligence.ThreatStance);

                    Point closestPoint = leader.Intelligence.PathPlanner.GetClosestPointToDestination(moveMap.Map[member.GetTransportType()],
                        attackPosition, 400, leader.Location);

                    newWaypoint = new Waypoint(leadersNextWaypoint.Number, MapManager.TileToWorldPos(closestPoint), isLastEdge);
                }*/
            }

            return true;
        }

        private static SubtileInfluence DrawMeleePositionInfluenceMap(Entity attacker, IKnownEntityData target)
        {
           // byte[][] subtileMap = null;

            // create an influence map with the target in the center
            float sizeOfMap = 2 * target.EntityType.LocomotorType.MeleeRadius + 4 * attacker.EntityType.LocomotorType.MeleeRadius;
            int sizeOfMapInSubtiles = (int)(MapManager.oneOverSubtileSize * sizeOfMap);


            Vector3 targetLocation = target.PlaySiteLocation; // attacker.Intelligence.Allegiance.SharedKnowledge.GetLocation(target);

         /*     topLeftSubtilePositionOfMap = MapManager.WorldPosToSubtile(targetLocation);
            topLeftSubtilePositionOfMap.X -= sizeOfMapInSubtiles / 2;
            topLeftSubtilePositionOfMap.Y -= sizeOfMapInSubtiles / 2;

            // clamp it to stay within the game map:
            int minX, maxX, minY, maxY;
            MapManager.GetClampedRectangularMapAreaUsingSubtiles(topLeftSubtilePositionOfMap, sizeOfMapInSubtiles, sizeOfMapInSubtiles, out minX, out maxX, out minY, out maxY);

            topLeftSubtilePositionOfMap.X = minX;
            topLeftSubtilePositionOfMap.Y = minY;

            int widthInSubtiles = maxX - minX;
            int heightInSubtiles = maxY - minY;
            */


            SubtileInfluence subtileMap = new SubtileInfluence(targetLocation, sizeOfMapInSubtiles); // new SubtileInfluence(widthInSubtiles, heightInSubtiles);

            subtileMap.DrawDistanceGradientOnInfluenceMap(attacker.PlaySiteLocation);

            DrawBestMeleeRadius(attacker, target, targetLocation, subtileMap.TopLeftSubtilePositionOfMap, subtileMap.Values);

            DrawNegativeInfluenceFromEntities(attacker, target, subtileMap.TopLeftSubtilePositionOfMap, subtileMap.Values);

            // let's use the move map for obstructed subtiles instead of just the terrain map, so we can avoid fires etc.
            // always bold stance when attacking
            MovementMap moveMap = attacker.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(attacker.Intelligence.ProtectionLevel, attacker.EntityType, ThreatStance.Bold); // attacker.Intelligence.ThreatStance);

            subtileMap.BlockOutBlockedSubtiles(moveMap.Layers[SurfaceType.TransportType.Foot], false); // always attack on foot.

            return subtileMap;
        }

      

       


        private const byte valueForCorrectDistance = 10;

        /// <summary>
        /// mark the tiles with the best distance to fight from
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="subtileMap"></param>
        private static void DrawBestMeleeRadius(Entity attacker, IKnownEntityData target, Vector3 targetLocation, Point subTilePositionOfMap, byte[][] subtileMap)
        {
            Point targetSubtilePosition = MapManager.WorldPosToSubtile(targetLocation);
            Point targetRelativeSubtilePosition = new Point(targetSubtilePosition.X - subTilePositionOfMap.X, targetSubtilePosition.Y - subTilePositionOfMap.Y);

            float correctMeleeDistance = target.EntityType.LocomotorType.MeleeRadius + attacker.EntityType.LocomotorType.MeleeRadius;

            float currentDistanceFromTarget;

            for (int x = 0; x < Common.GetJaggedArrayWidth(subtileMap); x++)
            {
                for (int y = 0; y < Common.GetJaggedArrayHeight(subtileMap); y++)
                {
                    currentDistanceFromTarget = MapManager.subTileSize * Common.DistanceOctile(targetRelativeSubtilePosition, new Point(x, y));

                    if (AttackJob.IsCorrectMeleeDistance(currentDistanceFromTarget, correctMeleeDistance))
                    {
                        subtileMap[x][y] += valueForCorrectDistance;
                    }
                }
            }
            
        }


       // private HashSet<Tuple<Entity, Vector3>> setOfEntitiesToDraw = new HashSet<Tuple<Entity, Vector3>>();

        private static Dictionary<Entity, Vector3> setOfEntitiesToDraw = new Dictionary<Entity, Vector3>();

        /// <summary>
        /// draw both current entities and entities heading this way
        /// </summary>
        /// <param name="subtileMap"></param>
        /// <param name="centerValue"></param>
        private static void DrawNegativeInfluenceFromEntities(Entity attacker, IKnownEntityData target, Point topLeftSubtilePosition, byte[][] subtileMap)
        {
            TerrainTile tile;

            Point topLeftTilePosition = MapManager.SubTileToTilePos(topLeftSubtilePosition);
            Point bottomRightTilePosition = MapManager.SubTileToTilePos(new Point(topLeftSubtilePosition.X + Common.GetJaggedArrayWidth(subtileMap), 
                topLeftSubtilePosition.Y + Common.GetJaggedArrayHeight(subtileMap)));

          //  HashSet<Entity> setOfEntitiesToDraw = new HashSet<Entity>();
            setOfEntitiesToDraw.Clear();

            // draw 'taken' attack spots
            Dictionary<EntityID, Vector3> meleeAttackers;
            if (The.Sim.MeleeAttackers.TryGetValue(target.EntityID, out meleeAttackers))
            {
              
                Entity otherAttacker;

                List<EntityID> invalidIDs = null;
                foreach (var entity in meleeAttackers)
                {
                    otherAttacker = Entity.FindByID(entity.Key);

                    if (otherAttacker != null)
                    {
                        setOfEntitiesToDraw.Add(otherAttacker, entity.Value);
                    }
                    else
                    {
                        // remove the invalid attacker:
                        Common.AddToList(ref invalidIDs, entity.Key);
                    }
                }

                // clean up the invalid attackers:
                if (invalidIDs != null)
                {
                    foreach (var item in invalidIDs)
                    {
                        meleeAttackers.Remove(item);
                    }
                    if (meleeAttackers.Count == 0)
                    {
                        The.Sim.MeleeAttackers.Remove(target.EntityID);
                    }
                }
            }

          
            for (int x = topLeftTilePosition.X; x <= bottomRightTilePosition.X; x++)
            {
                for (int y = topLeftTilePosition.Y; y <= bottomRightTilePosition.Y; y++)
                {
                    tile = The.Map.TileMap[x][y];

                    if (tile.EntitiesOnTile != null)
                    {
                        foreach (Entity entity in tile.EntitiesOnTile)
	                    {
                            if (entity != target && entity != attacker && entity.Intelligence != null && entity.Renderable.RenderAsModel != null && !entity.Locomotor.IsMoving()) // only draw immobile entities, not ones passing through
                            {
                                if (!setOfEntitiesToDraw.ContainsKey(entity))
                                {
                                    setOfEntitiesToDraw.Add(entity, entity.PlaySiteLocation);
                                }
                              
                            }
                        }
                    }
                }
            }

            

            Point relativeSubtilePos;
            Vector3 topLeftLocation = MapManager.SubTileEdgeToWorldPos3(topLeftSubtilePosition);

            int radius;
         
            foreach (KeyValuePair<Entity, Vector3> kvp in setOfEntitiesToDraw)
	        {
                if (kvp.Key != attacker)
                {
                    relativeSubtilePos = MapManager.WorldPosToRelativeSubtile(kvp.Value, topLeftLocation);

                    if (relativeSubtilePos.X >= 0 && relativeSubtilePos.Y >= 0)
                    {
                        radius = (int)(kvp.Key.EntityType.LocomotorType.MeleeRadius * MapManager.oneOverSubtileSize);

                        // perhaps only current attackers should block completely...?                
                        InfluenceMap.DrawLinearInfluenceCircle(subtileMap, relativeSubtilePos, 0, InfluenceMap.Operation.SetValue, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, radius);
                    }
                }
	        }

            setOfEntitiesToDraw.Clear();
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
            this.Target = sn.DoEnumNullable(Target);

            sn.Ignore(setOfEntitiesToDraw);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

           
        }

        #endregion
    }
}
