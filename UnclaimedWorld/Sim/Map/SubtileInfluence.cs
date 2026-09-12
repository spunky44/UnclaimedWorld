using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Maps
{
    /// <summary>
    /// THE way to use influence maps!!
    /// </summary>
    public class SubtileInfluence
    {
        byte[][] values;

        int width;
        int height;

        int right;
        int bottom;

        public Point TopLeftSubtilePositionOfMap;
     /*   {
            get;
            private set;
        }*/

        public byte[][] Values
        {
            get
            {
                return values;
            }
        }

        public SubtileInfluence(int width, int height)
        {
            Init(width, height);
        }

        private void Init(int width, int height)
        {
            Common.InitJaggedArray(ref values, width, height);

            this.width = width;
            this.height = height;

            right = width;
            bottom = height;
        }

        /// <summary>
        /// create an influence map with the target in the center
        /// </summary>
        /// <param name="targetLocation"></param>
        public SubtileInfluence(Vector3 targetLocation, int sizeOfMapInSubtiles)
        {

            // 
            TopLeftSubtilePositionOfMap = MapManager.WorldPosToSubtile(targetLocation);
            TopLeftSubtilePositionOfMap.X -= sizeOfMapInSubtiles / 2;
            TopLeftSubtilePositionOfMap.Y -= sizeOfMapInSubtiles / 2;

            // clamp it to stay within the game map:
            int minX, maxX, minY, maxY;
            MapManager.GetClampedRectangularMapAreaUsingSubtiles(TopLeftSubtilePositionOfMap, sizeOfMapInSubtiles, sizeOfMapInSubtiles, out minX, out maxX, out minY, out maxY);

            TopLeftSubtilePositionOfMap.X = minX;
            TopLeftSubtilePositionOfMap.Y = minY;

            int widthInSubtiles = maxX - minX;
            int heightInSubtiles = maxY - minY;

            Init(widthInSubtiles, heightInSubtiles);

            right = TopLeftSubtilePositionOfMap.X + width;
            bottom = TopLeftSubtilePositionOfMap.Y + height;
        }


        public SubtileInfluence Clear()
        {
            foreach (var item in values)
            {
                Array.Clear(item, 0, item.Length);                
            }

            return this;

        }

     /*   public SubtileInfluence SetValues(MapManager.SubtileValue[][] from, int startX, int startY, Func<MapManager.SubtileValue, byte> setValue) //, int width, int height)
        {
            Common.CopyValues(from, values, startX, startY, width, height, setValue); //, 0, 0);
            
            return this;
        }*/



        /// <summary>
        /// add to points within a radius
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="subtileMap"></param>
        public void DrawRadius(Vector3 targetLocation, float innerRadius, float outerRadius, byte valueToAdd)
        {
            Point targetSubtilePosition = MapManager.WorldPosToSubtile(targetLocation);
                    

            for (int x = TopLeftSubtilePositionOfMap.X; x < right; x++)
            {
                for (int y = TopLeftSubtilePositionOfMap.Y; y < bottom; y++)
                {
                    float distance = MapManager.subTileSize * Common.DistanceOctile(targetSubtilePosition, new Point(x, y));

                    if (distance >= innerRadius && distance <= outerRadius)
                    {
                        values[x - TopLeftSubtilePositionOfMap.X][y - TopLeftSubtilePositionOfMap.Y] += valueToAdd;
                    }
                }
            }

        }

        public void AddWhiteNoise(byte valueToAdd)
        {
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (values[x][y] != 0)
                    {
                        
                        values[x][y] += (byte)The.Sim.GameplayRandomGenerator.Next(valueToAdd, null, false);
                        
                    }
                }
            }

        }

        List<SubtilePos> bestPositions = new List<SubtilePos>();

        public List<SubtilePos> GetBestRelativePositionsWithSameScore(bool randomize = true)
        {
            bestPositions.Clear();

            byte[] column;
            byte bestValue = 0;
          
            for (int x = 0; x < width; x++)
            {
                column = values[x];

                for (int y = 0; y < height; y++)
                {
                    byte value = column[y];
                    if (value > 0)
                    {
                        if (value == bestValue)
                        {
                            bestPositions.Add(new SubtilePos((ushort)x, (ushort)y));
                        }
                        else if (value > bestValue)
                        {
                            bestPositions.Clear();

                            bestValue = value;
                            bestPositions.Add(new SubtilePos((ushort)x, (ushort)y));
                        }
                    }

                }
            }

            if (randomize)
            {
                bestPositions = Common.Randomize(bestPositions, The.Sim.GameplayRandomGenerator);
            }

            return bestPositions;
        }

        List<Tuple<byte, SubtilePos>> bestPositionsAndValues = new List<Tuple<byte, SubtilePos>>();

        public List<Tuple<byte, SubtilePos>> GetBestRelativePositions(int maxResults, bool randomize = true)
        {
            bestPositionsAndValues.Clear();

            byte[] column;
            byte bestValue = 0;

            for (int x = 0; x < width; x++)
            {
                column = values[x];

                for (int y = 0; y < height; y++)
                {
                    byte value = column[y];
                    if (value > 0)
                    {
                        if (value >= bestValue)
                        {
                            bestPositionsAndValues.Add(new Tuple<byte, SubtilePos>(value, new SubtilePos((ushort)x, (ushort)y)));                                                  
                            bestValue = value;                           
                        }
                    }

                }
            }

            if (bestPositionsAndValues.Count > maxResults)
            {
                bestPositionsAndValues = bestPositionsAndValues.OrderByDescending(t => t.Item1).ToList();
                bestPositionsAndValues.RemoveRange(maxResults, bestPositionsAndValues.Count - maxResults);
            }

            if (randomize)
            {
                bestPositionsAndValues = Common.Randomize(bestPositionsAndValues, The.Sim.GameplayRandomGenerator);
            }

            return bestPositionsAndValues;
        }

        public SubtilePos? GetBestPos()
        {
            byte[] column;
            byte bestValue = 0;
            SubtilePos? bestPos = null;
            for (int x = 0; x < width; x++)
            {
                column = values[x];

                for (int y = 0; y < height; y++)
                {
                    byte value = column[y];
                    if (value > bestValue)
                    {
                        bestValue = value;
                        bestPos = new SubtilePos((ushort)x, (ushort)y);
                    }

                }
            }

            return bestPos;
        }

        private static void GetMinMaxDistances(ref Point fromLocation, ref Point toLocation, ref float minDistance, ref float maxDistance)
        {
            float currentDistance = Common.DistanceOctile(fromLocation, toLocation);
            minDistance = Math.Min(minDistance, currentDistance);
            maxDistance = Math.Max(maxDistance, currentDistance);
        }

        private static void GetMinMaxDistances(ref Vector2 fromLocation, ref Vector2 toLocation, ref float minDistance, ref float maxDistance)
        {
            float currentDistance = Common.DistanceOctile(fromLocation, toLocation);
            minDistance = Math.Min(minDistance, currentDistance);
            maxDistance = Math.Max(maxDistance, currentDistance);
        }

        public static SubtileInfluence FindFreeSpotNearLocation(Vector3 targetLocation, int sizeOfMapInSubtiles, Vector3? fromLocation, Entity entity, bool useMovementMap)
        {
           // byte[][] subtileMap = null;

            // create an influence map with the target in the center

            SubtileInfluence subtileMap = new SubtileInfluence(targetLocation, sizeOfMapInSubtiles);

         /*   topLeftSubtilePositionOfMap = MapManager.WorldPosToSubtile(targetLocation);
            topLeftSubtilePositionOfMap.X -= sizeOfMapInSubtiles / 2;
            topLeftSubtilePositionOfMap.Y -= sizeOfMapInSubtiles / 2;

            // clamp it to stay within the game map:
            int minX, maxX, minY, maxY;
            MapManager.GetClampedRectangularMapAreaUsingSubtiles(topLeftSubtilePositionOfMap, sizeOfMapInSubtiles, sizeOfMapInSubtiles, out minX, out maxX, out minY, out maxY);

            topLeftSubtilePositionOfMap.X = minX;
            topLeftSubtilePositionOfMap.Y = minY;

            int widthInSubtiles = maxX - minX;
            int heightInSubtiles = maxY - minY;

            Common.InitJaggedArray(ref subtileMap, widthInSubtiles, heightInSubtiles);
            */

            if (fromLocation.HasValue)
            {
                subtileMap.DrawDistanceGradientOnInfluenceMap(fromLocation.Value);
            }
            else
            {
                // fill a constant value:
                subtileMap.FillArray(10);
            }

            subtileMap.DrawNegativeInfluenceFromEntities(entity, true, true); //, topLeftSubtilePositionOfMap);

            // let's use the move map for obstructed subtiles instead of just the terrain map, so we can avoid fires etc.
            if (useMovementMap)
            {
                MovementMap moveMap = entity.Intelligence.Allegiance.SharedKnowledge.GetMovementMap(entity.Intelligence.ProtectionLevel, entity.EntityType, entity.Intelligence.ThreatStance);

                subtileMap.BlockOutBlockedSubtiles(moveMap.Layers[SurfaceType.TransportType.Foot], false);
            }

            return subtileMap;
        }

        /// <summary>
        /// this method uses a zero to designate a blocked subtile!
        /// </summary>
        /// <param name="topLeftSubtilePosition"></param>
        /// <param name="subtileMap"></param>
        /// <param name="moveMap"></param>
        public void BlockOutBlockedSubtiles(SubtileLayers moveMap, bool blockReserved)
        {
           MapManager.SubtileValue value;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    value = moveMap.GetValue(x + TopLeftSubtilePositionOfMap.X, y + TopLeftSubtilePositionOfMap.Y);
                    if (MapManager.IsBlocked(value)
                        || ( blockReserved && MapManager.TestForFlag(value, MapManager.SubtileValue.Reserved)))
                    {
                        values[x][y] = 0;
                    }
                }
            }
        }


        public void DrawNegativeInfluenceFromEntities(Entity entityToExclude, bool drawStationaryAgents = true, bool drawItems = false) 
        {
            TerrainTile tile;

            Point topLeftTilePosition = MapManager.SubTileToTilePos(TopLeftSubtilePositionOfMap);
            Point bottomRightTilePosition = MapManager.SubTileToTilePos(new Point(TopLeftSubtilePositionOfMap.X + width, TopLeftSubtilePositionOfMap.Y + height));


            Point relativeSubtilePos;
            Vector3 topLeftLocation = MapManager.SubTileEdgeToWorldPos3(TopLeftSubtilePositionOfMap);

            int radius = 1;

            for (int x = topLeftTilePosition.X; x <= bottomRightTilePosition.X; x++)
            {
                for (int y = topLeftTilePosition.Y; y <= bottomRightTilePosition.Y; y++)
                {
                    tile = The.Map.TileMap[x][y];

                    if (tile.EntitiesOnTile != null)
                    {
                        foreach (Entity entityOnTile in tile.EntitiesOnTile)
                        {

                            if (entityOnTile != entityToExclude)
                            {
                                bool isItem = entityOnTile.EntityType.ItemType != null;
                                bool isAgent = entityOnTile.EntityType.IntelligenceType != null
                                       && entityOnTile.Locomotor != null
                                       && !entityOnTile.Locomotor.IsMoving();// only draw immobile entities, not ones passing through

                                if ((drawStationaryAgents == true
                                       && isAgent) 
                                    || (drawItems == true
                                        && isItem
                                    )) 
                                {
                                    relativeSubtilePos = MapManager.WorldPosToRelativeSubtile(entityOnTile.Location.Value, topLeftLocation);

                                    if (relativeSubtilePos.X >= 0 && relativeSubtilePos.Y >= 0)
                                    {

                                        if (isAgent)
                                            radius = 2;

                                        if (isItem)
                                            radius = 0;// point

                                        //radius = (int)(entityOnTile.Renderable.Renderable.RenderAsModelType.MeleeRadius * MapManager.oneOverSubtileSize);

                                        InfluenceMap.DrawLinearInfluenceCircle(values, relativeSubtilePos, -5, InfluenceMap.Operation.AddToExisting,
                                            InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, radius);
                                    }
                                }

                            }
                        }
                    }
                }
            }

        }


        public void FillArray(byte value)
        {
           
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    values[x][y] = value;
                }
            }
        }


        /// <summary>
        /// draws a distance gradient on the influence map
        /// closest distances get highest value
        /// </summary>
        /// <param name="fromLocation"></param>
        /// <param name="subTilePosition"></param>
        /// <param name="subtileMap"></param>
        public void DrawDistanceGradientOnInfluenceMap(Vector3 fromLocation, float minValueToDraw = 0, float maxValueToDraw = 10) //, Point subTilePosition) //, byte[][] subtileMap)
        {

            Point fromSubtilePosition = MapManager.WorldPosToSubtile(fromLocation);

            Point upperLeftSubtileLocation = this.TopLeftSubtilePositionOfMap;
            Point upperRightSubtileLocation = new Point(TopLeftSubtilePositionOfMap.X + width, TopLeftSubtilePositionOfMap.Y);
            Point lowerLeftSubtileLocation = new Point(TopLeftSubtilePositionOfMap.X, TopLeftSubtilePositionOfMap.Y + height);
            Point lowerRightSubtileLocation = new Point(TopLeftSubtilePositionOfMap.X + width, TopLeftSubtilePositionOfMap.Y + height);
            Point centerSubtileLocation = new Point(TopLeftSubtilePositionOfMap.X + width / 2, TopLeftSubtilePositionOfMap.Y + height / 2);


            // let's examine the distance to 5 specific points on the map so we can determine min and max and apply a proper scaling:
            // use sub tile distances so we don't do unnecessary calculations (only relative distances matter anyway)
            float minDistance = 1000000f, maxDistance = 0f;
            GetMinMaxDistances(ref fromSubtilePosition, ref upperLeftSubtileLocation, ref minDistance, ref maxDistance);
            GetMinMaxDistances(ref fromSubtilePosition, ref upperRightSubtileLocation, ref minDistance, ref maxDistance);
            GetMinMaxDistances(ref fromSubtilePosition, ref lowerLeftSubtileLocation, ref minDistance, ref maxDistance);
            GetMinMaxDistances(ref fromSubtilePosition, ref lowerRightSubtileLocation, ref minDistance, ref maxDistance);
            GetMinMaxDistances(ref fromSubtilePosition, ref centerSubtileLocation, ref minDistance, ref maxDistance);

            float oneOverDistanceSpread = 1f / (maxDistance - minDistance);

            float currentDistance;
            float value;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    currentDistance = Common.DistanceOctile(fromSubtilePosition, new Point(TopLeftSubtilePositionOfMap.X + x, TopLeftSubtilePositionOfMap.Y + y));
                    // closest distances get highest value
                    value = MathHelper.Lerp(maxValueToDraw, minValueToDraw, (currentDistance - minDistance) * oneOverDistanceSpread);

                    values[x][y] = (byte)value;
                }
            }
        }

       // private static Dictionary<Entity, Vector3> setOfEntitiesToDraw = new Dictionary<Entity, Vector3>();

        /// <summary>
        /// draw both current entities and entities heading this way
        /// </summary>
        /// <param name="subtileMap"></param>
        /// <param name="centerValue"></param>
        public void DrawNegativeInfluenceFromEntities(List<Tuple<Vector3, float>> circlesToDraw, Point topLeftSubtilePosition)
        {
           
          //  Point topLeftTilePosition = MapManager.SubTileToTilePos(topLeftSubtilePosition);
        //    Point bottomRightTilePosition = MapManager.SubTileToTilePos(new Point(topLeftSubtilePosition.X + width,
           //     topLeftSubtilePosition.Y + height));

          
            Point relativeSubtilePos;
            Vector3 topLeftLocation = MapManager.SubTileEdgeToWorldPos3(topLeftSubtilePosition);

           
            foreach (var kvp in circlesToDraw)
            {

                relativeSubtilePos = MapManager.WorldPosToRelativeSubtile(kvp.Item1, topLeftLocation);

                if (relativeSubtilePos.X >= 0 && relativeSubtilePos.Y >= 0)
                {                       
                    InfluenceMap.DrawLinearInfluenceCircle(values, relativeSubtilePos, 0, InfluenceMap.Operation.SetValue, InfluenceMap.Falloff.No, InfluenceMap.CircleParameter.Radius, (int)kvp.Item2);
                }

            }

          //  setOfEntitiesToDraw.Clear();
        }
    }
}
