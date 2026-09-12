using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using System.Text;
using GameStateManagement;
using System.Threading.Tasks;
using UWGame.Control;
using UWGame.Control.Replays;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps
{
    public class Dependence: ISnapshot
    {
        public IMap Child;
        IMapID snapshotChild;

        public float Weight;
        
        public Dependence(IMap child, float weight)
        {
            this.Child = child;
            this.Weight = weight;
           
        }

        public Dependence()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            Weight = sn.DoFloat(Weight);
            snapshotChild = (IMapID)sn.SnapshotID<IMap, IMapID>(Child);

            return this;
        }

        public bool IsSnapshotted { get; set; }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Child = LookUp<IMap, IMapID>.FindByID(snapshotChild);
                        
        }
    }

   /// <summary>
   /// not inherited by DiscomfortMap, only by ThreatMap...
   /// </summary>
    public abstract class InfluenceMap : IMap, ISnapshot
    {
        public string IDName;

        private List<Dependence> children = new List<Dependence>();
        public List<Dependence> Children 
        { 
            get { return children; } 
            set { children = value; } 
        }


        /// <summary>
        /// this is set true when the map has finished updating itself (could take several cycles) and can be added to the parent map(s)
        /// </summary>
        public bool IsReady{ get; set;} // = false;

        private const float subTileWidthReciprocal = 0.33333f;

        /// <summary>
        /// the limit for considering a tile blocked (cost = 0)
        /// </summary>
        public byte BlockingLimit;

     /*   protected byte[][] map; 

        /// <summary>        
        /// move the logic to TileSector
        /// Warning! for external use only. Use 'map' instead when redrawing. 
        /// </summary>
        public byte[][] Map 
        {
            get
            {
                // double buffering - if we are currently redrawing, present the old map:
                if (IsReady)
                {
                    return map;
                }
                else
                {
                    if (map == backBufferMap1)
                    {
                        return backBufferMap2;
                    }
                    else return backBufferMap1;
                }
            }
            set { map = value; } 
        }*/

        /// <summary>
        /// we keep 2 maps in order to compare them after redrawing and detect changes to the sectors.
        /// then we switch them around.
        /// </summary>
       /* protected byte[][] backBufferMap1;
        protected byte[][] backBufferMap2;
        */

      /*  protected bool[][] isBlocked;

        /// <summary>
        /// double buffering not needed...
        /// </summary>
        public bool[][] IsBlocked { get { return isBlocked; } set { isBlocked = value;} }
        */


        /// <summary>
        /// NEW - can have holes.
        /// </summary>
        public TileLayer Map { get; protected set; }



        /// <summary>
        /// we divide the map into sectors in order to track changes (optimization)
        /// </summary>
      /*  private Sector[][] sectors;
        public Sector[][] Sectors { get { return sectors; } set { sectors = value; } }
        */

        
        public InfluenceMap()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
   
        }

        public InfluenceMap(int mapWidth, int mapHeight)
        {
            AddToLookup();

           /* Common.InitJaggedArray(ref backBufferMap1, mapWidth, mapHeight);
            Common.InitJaggedArray(ref backBufferMap2, mapWidth, mapHeight);

            map = backBufferMap1;

            Common.InitJaggedArray(ref isBlocked, mapWidth, mapHeight);
            

            InitTileSectors(ref sectors); //, SectorSizeInTiles, mapWidth, mapHeight);
            */
        }

      /*  public static void InitTileSectors(ref Sector[][] sectors) //, int sectorWidth, int mapWidth, int mapHeight)
        {
            Common.InitJaggedArray(ref sectors, The.Map.MapTileSectorWidth, The.Map.MapTileSectorHeight); // (int)Math.Ceiling((double)mapWidth / (double)sectorWidth), (int)Math.Ceiling((double)mapHeight / (double)sectorWidth));

            for (int x = 0; x < Common.GetJaggedArrayWidth(sectors); x++)
            {
                for (int y = 0; y < Common.GetJaggedArrayHeight(sectors); y++)
                {
                    sectors[x][y] = new Sector(new Point(x, y), MapManager.SectorSizeInTiles, The.Map.mapTileWidth, The.Map.mapTileHeight); 
                }
            }
        }*/

    
      /*  public virtual bool IsDirty()
        {           
            return false;
        }*/

       


        /// <summary>
        /// accessed via reflection... probably should change this.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="subtileX"></param>
        /// <param name="subtileY"></param>
        /// <returns></returns>
        public static bool DiscomfortTileIsFree(DiscomfortMap /*InfluenceMap*/ map, int subtileX, int subtileY)
        {
            Point tilePos = MapManager.SubTileToTilePos(new Point(subtileX, subtileY));
            return !map.Map.GetIsBlocked(tilePos.X , tilePos.Y); // .IsBlocked[(int)(subTileWidthReciprocal * subtileX)][(int)(subTileWidthReciprocal * subtileY)];
            
        }

        public class DiscomfortTileIsBelowValueParameters
        {
            public DiscomfortMap /*InfluenceMap*/ map;
            public byte belowOrEqualToValue;

            public DiscomfortTileIsBelowValueParameters(DiscomfortMap /*InfluenceMap*/ map, byte belowOrEqualToValue)
            {
                this.map = map;
                this.belowOrEqualToValue = belowOrEqualToValue;
            }
        }

        /// <summary>
        /// accessed via reflection... probably should change this.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="subtileX"></param>
        /// <param name="subtileY"></param>
        /// <returns></returns>
        public static bool DiscomfortTileIsBelowValue(DiscomfortTileIsBelowValueParameters parameters, int subtileX, int subtileY) 
        {
            Point tile = MapManager.SubTileToTilePos(new Point(subtileX, subtileY));
            return parameters.map.Map.GetValue(tile) <= parameters.belowOrEqualToValue;
           // return parameters.map.Map[subtileX, subtileY] <= parameters.belowOrEqualToValue;
           // return map.Map[x, y] <= belowOrEqualToValue;
        }

      /*  public static void ClearTilesInSector(Sector sector, byte[][] map, bool[][] isBlocked) // Point sectorCoords)
        {
            byte[] column;
            bool[] isBlockedColumn;
            for (int x = sector.TileArea.Left; x < sector.TileArea.Right; x++)
            {
                column = map[x];
                isBlockedColumn = isBlocked[x];

                for (int y = sector.TileArea.Top; y < sector.TileArea.Bottom; y++)
                {
                    column[y] = 0;
                    isBlockedColumn[y] = false;
                }
            }
        }*/

        public static void AddTilesInSector(Rectangle sectorArea, float weight, byte[][] map, bool[][] isBlocked, byte[][] mapToAdd, bool[][] isBlockedToAdd) 
        {
            byte[] column, columnToAdd;
            bool[] isBlockedColumn, isBlockedToAddColumn;
            for (int x = sectorArea.Left; x < sectorArea.Right; x++)
            {
                column = map[x];
                isBlockedColumn = isBlocked[x];

                columnToAdd = mapToAdd[x];
                isBlockedToAddColumn = isBlockedToAdd[x];

                for (int y = sectorArea.Top; y < sectorArea.Bottom; y++)
                {
                    column[y] = (byte)(Common.ClampTop(column[y] + weight * columnToAdd[y], 255));
                    isBlockedColumn[y] = isBlockedColumn[y] || isBlockedToAddColumn[y];                                     
                }
            }
        }

      /*  public bool SectorIsDirty(int x, int y) // Point sectorCoords)
        {
            return Sectors[x][y].IsDirty;
        }*/

        /// <summary>
        /// not used???
        /// 
        /// add maps by sector
        /// only clear and add sectors where something is dirty
        /// otherwise keep the sector as it is!
        /// </summary>
        /// <param name="mapToAdd"></param>
        /// <param name="weight"></param>
      /*  public void AddMaps(InfluenceMap mapToAdd, float weight)
        {
            int width = Common.GetJaggedArrayWidth(Map);
            int height = Common.GetJaggedArrayHeight(Map);
                   
            int sectorWidth = Common.GetJaggedArrayWidth(Sectors);
            int sectorHeight = Common.GetJaggedArrayHeight(Sectors);

            Parallel.For(0, sectorWidth, (x) =>
            {
                Sector[] thisColumn;
                thisColumn = Sectors[x];

                Sector[] otherColumn;
                otherColumn = mapToAdd.Sectors[x];

                Sector thisSector;
                for (int y = 0; y < sectorHeight; y++)
			    {
                    thisSector = thisColumn[y];

			        if (thisSector.IsDirty || otherColumn[y].IsDirty)
                    {
                        // clear the tiles in this sector and add them
                        ClearTilesInSector(thisSector, Map, IsBlocked);
                    }
                }                

            });           

        }*/

        
        /*
        public void ClearMap()
        {
            
            // switch the buffers:
            if (map == backBufferMap1)
            {
                map = backBufferMap2;
              //  IsBlocked = backBufferIsBlocked2;
            }
            else
            {
                map = backBufferMap1;
              //  IsBlocked = backBufferIsBlocked1;
            }

            // clear the new ones
            Common.ClearJaggedArray(map);
            Common.ClearJaggedArray(IsBlocked);

            // init...
            ClearSectors(Sectors);

            
        }*/

     /*   public static void ClearSectors(Sector[][] sectors)
        {
            for (int x = 0; x < Common.GetJaggedArrayWidth(sectors); x++)
            {
                for (int y = 0; y < Common.GetJaggedArrayHeight(sectors); y++)
                {                  
                    sectors[x][y].SubtilesAreDirty = false;
                }
            }
        }*/

        public void Destroy()
        {
            RemoveIDEntry();
        }

     
      
        public IMap GetCurrent()
        {
            // NEW: we redraw periodically, not on demand
            return this;
        }


        public override string ToString()
        {
            return IDName;
        }

     /*   public static string VisualizeByteArray(byte[] array, int width, int height)
        {
           // int height = array.GetLength(1) / width;
            byte[,] newArray = new byte[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    newArray[i, j] = array[i + j * width];
                }
            }

            return VisualizeByteArray(newArray);
        }*/

        public static string VisualizeByteArray(byte[,] array, Point from, Point to)
        {
            StringBuilder s = new StringBuilder();

            s.Append("(x, y)\t");
            for (int x = from.X; x <= to.X; x++)
            {
                s.Append(x);
                s.Append("\t");
            }

            s.Append("\r\n");
            s.Append("---------------------------------------------------------------------------------------------------------------------------");
            s.Append("\r\n");

            for (int y = from.Y; y <= to.Y; y++)
            {
                s.Append(y);
                s.Append("\t");
                s.Append("|");

                for (int x = from.X; x <= to.X; x++)
                {
                    s.Append(array[x, y]);
                    s.Append("\t");
                }

                s.Append("\r\n");

            }

            return s.ToString();
        }

        private static string spacer = "\t";

       /* public static string VisualizeByteArray(byte[,] array)
        {
            StringBuilder s = new StringBuilder();

            s.Append("(x, y)\t");
            for (int x = 0; x <= array.GetUpperBound(0); x++)
            {
                s.Append(x);
                s.Append(spacer);
            }

            s.Append("\r\n");
            s.Append("---------------------------------------------------------------------------------------------------------------------------");
            s.Append("\r\n");

            for (int y = 0; y <= array.GetUpperBound(1); y++)
            {
                s.Append(y);
                s.Append(spacer);
                s.Append("|");

                for (int x = 0; x <= array.GetUpperBound(0); x++)
                {
                    s.Append(array[x, y]);
                    s.Append(spacer);
                }

                s.Append("\r\n");

            }

            return s.ToString();
        }*/

        /// <summary>
        /// debugger byte array visualizer
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
      /*  public static string VisualizeByteArray(bool[,] array)
        {
            StringBuilder s = new StringBuilder();

            s.Append("(x, y)\t");
            for (int x = 0; x <= array.GetUpperBound(0); x++)
            {
                s.Append(x);
                s.Append("\t");
            }

            s.Append("\r\n");
            s.Append("---------------------------------------------------------------------------------------------------------------------------");
            s.Append("\r\n");

            for (int y = 0; y <= array.GetUpperBound(1); y++)
            {
                s.Append(y);
                s.Append("\t");
                s.Append("|");

                for (int x = 0; x <= array.GetUpperBound(0); x++)
                {
                    s.Append(array[x, y]? 1: 0);
                    s.Append("\t");
                }

                s.Append("\r\n");

            }

            return s.ToString();
        }*/

        /// <summary>
        /// debugger byte array visualizer
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string VisualizeByteArray<T>(T[][] array, Point? from = null, Point? to = null)
        {
            StringBuilder s = new StringBuilder();

            

            int maxX, maxY;

            int fromX, fromY;

            if (to.HasValue)
            {
                maxY = to.Value.Y;
                maxX = to.Value.X;
            }
            else
            {
                maxY = array[0].GetUpperBound(0);
                maxX = array.GetUpperBound(0);
            }

            if (from.HasValue)
            {
                fromX = from.Value.Y;
                fromY = from.Value.X;
            }
            else
            {
                fromY = 0;
                fromX = 0; 
            }

            s.Append("(x, y)\t");
            for (int x = fromX; x <= maxX; x++)
            {
                s.Append(x);
                s.Append(spacer);
            }

            s.Append("\r\n");
            s.Append("---------------------------------------------------------------------------------------------------------------------------");
            s.Append("\r\n");

            for (int y = fromY; y <= maxY; y++)
            {
                s.Append(y);
                s.Append(spacer);
                s.Append("|");

                for (int x = fromX; x <= maxX; x++)
                {
                    s.Append(array[x][y]);
                    s.Append(spacer);
                }

                s.Append("\r\n");

            }

            return s.ToString();
        }

        public enum GradientDirection { LeftToRight, TopToBottom }
        /// <summary>
        /// start value is to the left if LeftToRight is chosen, and at the top if TopToBottom
        /// </summary>
        /// <param name="map"></param>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="addToExistingValues"></param>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="maxRandomToAdd"></param>
        /// <param name="dir"></param>
        public static void DrawGradientRectangle(byte[][] map, Point topLeft, Point bottomRight, bool addToExistingValues, byte startValue, byte endValue, int maxRandomToAdd, GradientDirection dir)
        {
            double falloffPerTile;
            double value = startValue;
            byte valueToAdd;
            if (dir == GradientDirection.LeftToRight)
            {
                falloffPerTile = ((double)(endValue - startValue)) / Math.Abs((double)(bottomRight.X - topLeft.X));
                for (int x = topLeft.X; x <= bottomRight.X; x++)
                {
                    for (int y = topLeft.Y; y <= bottomRight.Y; y++)
                    {
                        if (maxRandomToAdd != 0)
                        {
                            valueToAdd = (byte)(value + The.Sim.GameplayRandomGenerator.Next(maxRandomToAdd + 1,"InfluenceMap"));
                        }
                        else
                        {
                            valueToAdd = (byte)value;
                        }

                        if (!addToExistingValues)
                        {
                            map[x][y] = valueToAdd;
                        }
                        else
                        {
                            map[x][y] = (byte)Common.Clamp(map[x][y] + valueToAdd, 0, 255);
                        }
                    }
                    value = value + falloffPerTile;
                    
                }
            }
            else
            {   // mostly duplicated code...
                //falloffPerTile = Math.Abs(((double)(endValue - startValue)) / ((double)(bottomRight.Y - topLeft.Y)));
                falloffPerTile = (double)(endValue - startValue) / Math.Abs((double)(bottomRight.Y - topLeft.Y));
                for (int y = topLeft.Y; y <= bottomRight.Y; y++)
                {
                    for (int x = topLeft.X; x <= bottomRight.X; x++)
                    {
                        if (maxRandomToAdd != 0)
                        {
                            valueToAdd = (byte)(value + The.Sim.GameplayRandomGenerator.Next(maxRandomToAdd + 1, "InfluenceMap"));
                        }
                        else
                        {
                            valueToAdd = (byte)value;
                        }

                        if (!addToExistingValues)
                        {
                            map[x][y] = valueToAdd;
                        }
                        else
                        {
                            map[x][y] = (byte)Common.Clamp(map[x][y] + valueToAdd, 0, 255);
                        }

                    }
                 //   value = value - falloffPerTile;
                    value = value + falloffPerTile;
                    
                }
            }            

        }

        public enum CircleParameter{FalloffEachTile, Radius}
        public enum Operation { SetValue, AddToExisting }
        public enum Falloff { Yes, No }

        /// <summary>
        /// draws a circle with the specified fall-off or radius per tile.
        /// radius = 0 is a point. radius = 1 is a point with a 'cross' of half the center value.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="centerValue"></param>
        /// <param name="addToExistingValues"></param>
        public static void DrawLinearInfluenceCircle(byte[][] map, Point pos, int centerValue, Operation operation, Falloff falloffYesNo, 
            CircleParameter circleParam, int paramValue, HashSet<Point> affectedSectors = null, int? sectorSize = null, int? maxRadius = null)
        {
            bool isDrawingAPositiveCircle;
            double falloffEachTile;
            int minX;
            int minY;
            int maxX;
            int maxY;

            int width = Common.GetJaggedArrayWidth(map);
            int height = Common.GetJaggedArrayHeight(map);

            pos = ComputeLinearCircle(width, height, pos, centerValue, falloffYesNo, circleParam, paramValue, maxRadius, out isDrawingAPositiveCircle, out falloffEachTile, out minX, out minY, out maxX, out maxY);

            float dist;
            double res = centerValue;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {                    
                    if (falloffYesNo == Falloff.Yes)
                    {
                        dist = Common.DistanceOctile(new Point(x, y), pos); 
                        res = centerValue + (dist * falloffEachTile);
                    }

                    if (!isDrawingAPositiveCircle)
                    {
                        res = Common.ClampTop(res, 0);
                    }
                    else
                    {
                        res = Common.ClampBottom(res, 0);
                    }


                    if (operation == Operation.SetValue)
                    {
                        map[x][y] = (byte)Common.Clamp(res, 0, 255);
                    }
                    else
                    {
                        if (res != 0) // >= 0)
                        {
                            map[x][y] = (byte)Common.Clamp(map[x][y] + res, 0, 255);
                        }
                    }
                }
            }

            if (sectorSize.HasValue)
            {
                // return list of affected sectors:
                int minSectorX = minX / sectorSize.Value;
                int maxSectorX = maxX / sectorSize.Value;
                int minSectorY = minY / sectorSize.Value;
                int maxSectorY = maxY / sectorSize.Value;

                for (int x = minSectorX; x <= maxSectorX; x++)
                {
                    for (int y = minSectorY; y <= maxSectorY; y++)
                    {
                        affectedSectors.Add(new Point(x, y));
                    }
                }
            }

        }

        public static Point ComputeLinearCircle(int mapWidth, int mapHeight,/* byte[][] map,*/ 
            Point pos, int centerValue, Falloff falloffYesNo, CircleParameter circleParam, int paramValue, int? maxRadius, out bool isDrawingAPositiveCircle, out double falloffEachTile, out int minX, out int minY, out int maxX, out int maxY)
        {
            isDrawingAPositiveCircle = centerValue > 0;
            int radius;
            falloffEachTile = 0; // not used
            if (circleParam == CircleParameter.FalloffEachTile)
            {
                radius = Math.Abs(centerValue / paramValue); //20;
                falloffEachTile = -paramValue; //-32;

                if (maxRadius.HasValue)
                {
                    radius = Common.ClampTop(radius, maxRadius.Value);
                }
            }
            else
            {
                radius = paramValue;
                // we have a radius, compute the falloff:
                if (falloffYesNo == Falloff.Yes)
                {
                    if (isDrawingAPositiveCircle)
                    {
                        // the circle center is positive                    

                        falloffEachTile = (double)centerValue / (double)(radius + 1);
                        falloffEachTile = -falloffEachTile;

                    }
                    else
                    {
                        // the circle center is negative, make the falloff positive                    
                        falloffEachTile = -(double)centerValue / (double)(radius + 1);
                    }
                }
            }

            minX = Math.Max(0, pos.X - radius);
            minY = Math.Max(0, pos.Y - radius);

          
            maxX = Math.Min(mapWidth - 1, pos.X + radius);
            maxY = Math.Min(mapHeight - 1, pos.Y + radius);
            return pos;
        }

        public static int GetBestSubtileLocationThatIsntBlocked(byte[][] subtileMap, bool[][] isBlockedMap, out Point bestSubtilePoint)
        {
            bestSubtilePoint = new Point(-1, -1);
            int bestValue = -1;

            bool isBlocked;

            int width = Common.GetJaggedArrayWidth(subtileMap);
            int height = Common.GetJaggedArrayHeight(subtileMap);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    isBlocked = isBlockedMap[x][y];

                    if (!isBlocked)
                    {    // not blocked - get the score:
                        if (subtileMap[x][y] > bestValue)
                        {
                            bestSubtilePoint = new Point(x, y);
                            bestValue = subtileMap[x][y];
                        }
                    }
                }
            }

            return bestValue;
        }

        public static int GetBestSubtileLocationThatIsntBlocked(byte[][] subtileMap, Tuple<bool,Vector2>[][] isBlockedData, out Vector2 bestSubtileWorldPosition)
        {

            bestSubtileWorldPosition = new Vector2(-1, -1);
            int bestValue = -1;
            int width = Common.GetJaggedArrayWidth(isBlockedData);
            int height = Common.GetJaggedArrayHeight(isBlockedData);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (isBlockedData[x][y].Item1 == false)
                    {
                        if (subtileMap[x][y] > bestValue)
                        {
                            bestValue = subtileMap[x][y];
                            bestSubtileWorldPosition = isBlockedData[x][y].Item2;
                        }
                    }
                }
                
            }
            return bestValue;
            //for(int i = 0; i < isBlockedData.Count; i++)
            //{

            //    if (isBlockedData[i].First == false)
            //        {    // not blocked - get the score:
            //          //  if (subtileMap[x][y] > bestValue)
            //            {
            //              //  bestSubtilePoint = new Point(x, y);
            //                bestSubtilePosition = isBlockedData[i].Second;
                            
            //            }
            //        }
            //}

        }

        /// <summary>
        /// values = 0  are considered blocked...
        /// return -1 if no non-blocked areas are found...
        /// </summary>
        /// <param name="subtileMap"></param>
        /// <param name="bestSubtilePoint"></param>
        /// <returns></returns>
        public static int GetBestSubtileLocationThatIsntBlocked(byte[][] subtileMap, out Point bestSubtilePoint)
        {
            bestSubtilePoint = new Point(-1, -1);
            int bestValue = -1;
                        
            int width = Common.GetJaggedArrayWidth(subtileMap);
            int height = Common.GetJaggedArrayHeight(subtileMap);

            byte currentValue;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    currentValue = subtileMap[x][y];
                    
                    if (currentValue != 0)
                    {    // not blocked - get the score:
                        if (currentValue > bestValue)
                        {
                            bestSubtilePoint = new Point(x, y);
                            bestValue = currentValue;
                        }
                    }
                }
            }

            return bestValue;
        }

        /// <summary>
        /// draws a circle with the specified fall-off or radius per tile.
        /// radius = 0 is a point. radius = 1 is a point with a 'cross' of half the center value.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="centerValue"></param>
        /// <param name="addToExistingValues"></param>
        public static void DrawLinearInfluenceCircle(ushort[][] map, Point pos, int centerValue, Operation operation, Falloff falloffYesNo, CircleParameter circleParam, int paramValue) 
        {
            bool isDrawingAPositiveCircle = centerValue > 0;
            int radius;
            double falloffEachTile = 0; // not used
            if (circleParam == CircleParameter.FalloffEachTile)
            {
                radius = Math.Abs(centerValue / paramValue); //20;
                falloffEachTile = -paramValue; //-32;
            }
            else
            {
                radius = paramValue;
                // we have a radius, compute the falloff:
                if (falloffYesNo == Falloff.Yes)
                {
                    if (isDrawingAPositiveCircle)
                    {
                        // the circle center is positive                    

                        falloffEachTile = (double)centerValue / (double)(radius + 1);
                        falloffEachTile = -falloffEachTile;

                    }
                    else
                    {
                        // the circle center is negative, make the falloff positive                    
                        falloffEachTile = -(double)centerValue / (double)(radius + 1);
                    }
                }
            }

            int minX = Math.Max(0, pos.X - radius);
            int minY = Math.Max(0, pos.Y - radius);

            int width = Common.GetJaggedArrayWidth(map);
            int height = Common.GetJaggedArrayHeight(map);

            int maxX = Math.Min(width - 1, pos.X + radius);
            int maxY = Math.Min(height - 1, pos.Y + radius);

            float dist;
            double res = centerValue;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (falloffYesNo == Falloff.Yes)
                    {
                        dist = Common.DistanceOctile(new Point(x, y), pos);
                        res = centerValue + (dist * falloffEachTile);
                    }

                    if (!isDrawingAPositiveCircle)
                    {
                        res = Common.ClampTop(res, 0);
                    }
                    else
                    {
                        res = Common.ClampBottom(res, 0);
                    }

                    if (res != 0) // >= 0)
                    {
                        if (operation == Operation.SetValue)
                        {
                            // use unchecked to truncate???
                            map[x][y] = (ushort)Common.Clamp(res, 0, UInt16.MaxValue);
                        }
                        else
                        {
                            map[x][y] = (ushort)Common.Clamp(map[x][y] + res, 0, UInt16.MaxValue);
                        }
                    }
                }
            }
        }

       

        /// <summary>
        /// draws a circle with the specified fall-off or radius per tile.
        /// radius = 0 is a point. radius = 1 is a point with a 'cross' of half the center value.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="centerValue"></param>
        /// <param name="addToExistingValues"></param>
     /*   public static void DrawLinearInfluenceCircle(byte[][] map, Point pos, int centerValue, Operation operation, Falloff falloffYesNo, CircleParameter circleParam, int paramValue)
        {
            bool isDrawingAPositiveCircle = centerValue > 0;
            int radius;
            double falloffEachTile = 0; // not used
            if (circleParam == CircleParameter.FalloffEachTile)
            {
                radius = Math.Abs(centerValue / paramValue); //20;
                falloffEachTile = -paramValue; //-32;
            }
            else
            {
                radius = paramValue;
                // we have a radius, compute the falloff:
                if (falloffYesNo == Falloff.Yes)
                {
                    if (isDrawingAPositiveCircle)
                    {
                        // the circle center is positive                    

                        falloffEachTile = (double)centerValue / (double)(radius + 1);
                        falloffEachTile = -falloffEachTile;

                    }
                    else
                    {
                        // the circle center is negative, make the falloff positive                    
                        falloffEachTile = -(double)centerValue / (double)(radius + 1);
                    }
                }
            }

            int minX = Math.Max(0, pos.X - radius);
            int minY = Math.Max(0, pos.Y - radius);
            int maxX = Math.Min(map.Length - 1, pos.X + radius);
            int maxY = Math.Min(map[0].Length - 1, pos.Y + radius);

            float dist;
            double res = centerValue;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (falloffYesNo == Falloff.Yes)
                    {
                        dist = Common.DistanceOctile(new Point(x, y), pos);
                        res = centerValue + (dist * falloffEachTile);
                    }

                    if (!isDrawingAPositiveCircle)
                    {
                        res = Common.ClampTop(res, 0);
                    }
                    else
                    {
                        res = Common.ClampBottom(res, 0);
                    }

                    if (res != 0) // >= 0)
                    {
                        if (operation == Operation.SetValue)
                        {
                            map[x][y] = (byte)Common.Clamp(res, 0, 255);
                        }
                        else
                        {
                            map[x][y] = (byte)Common.Clamp(map[x][y] + res, 0, 255);
                        }
                    }
                }
            }
        }*/

        public static void DrawLinearInfluenceCircle(bool[][] map, Point pos, bool value, int radius) // Operation operation, Falloff falloffYesNo, CircleParameter circleParam, int paramValue)
        {            
            int minX = Math.Max(0, pos.X - radius);
            int minY = Math.Max(0, pos.Y - radius);
            int maxX = Math.Min(Common.GetJaggedArrayWidth(map) - 1, pos.X + radius);
            int maxY = Math.Min(Common.GetJaggedArrayHeight(map) - 1, pos.Y + radius);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    map[x][y] = value;
                      
                }
            }
        }

        public abstract bool DoCycle();

       // public abstract bool IsWaiting{get;}



        #region ILookup

        private IMapID id = IMapID.Invalid;

        //=================== ILookup Methods =====================
        public IMapID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public IMapID GetUniqueID()
        {
            return IMapCounter.GetUniqueID();
        }

        public IMapID SnapshotID(Snapshotter sn, IMapID id)
        {
            return (IMapID)sn.DoEnum(id);
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != IMapID.Invalid)
                LookUp<IMap, IMapID>.Add(ID, this);
        }

        public void RemoveIDEntry()
        {
            LookUp<IMap, IMapID>.Remove(this);
        }

        public void SetInvalid()
        {
            id = IMapID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing... Sim will call Cyclable.ResetIDCounter.
        {
        }

        void ILookUp<IMap, IMapID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<IMap, IMapID>.Create();
        }

        #endregion



        #region ISnapshot

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);

            this.children = sn.DoList(children);
            this.IDName = sn.DoString(IDName);

            this.BlockingLimit = sn.DoByte(BlockingLimit);

            this.Map = (TileLayer)sn.DoISnapshot(Map);

          /*  this.isBlocked = sn.DoJaggedArray(isBlocked);        
            this.map = sn.DoJaggedArray(map);
            this.backBufferMap1 = sn.DoJaggedArray(backBufferMap1);
            this.backBufferMap2 = sn.DoJaggedArray(backBufferMap2);

            this.sectors = sn.DoJaggedArray(sectors);*/
            this.IsReady = sn.DoBool(IsReady);
           
            sn.Ignore(spacer);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            foreach (var item in children)
            {
                item.LoadPostProcess(sn);
            }

            Map.LoadPostProcess(sn);

        }

        #endregion
    }
}