using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Pathfinding
{
    public class FloodFill
    {
        //int[] visited;

        //Queue<int> openQueue;

        Queue<Point> openQueue = new Queue<Point>();

       // private sbyte[,] mDirection = new sbyte[8, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
        private sbyte[][] mDirection = new sbyte[][] { new sbyte[] { 0, -1 }, new sbyte[] { 1, 0 }, new sbyte[] { 0, 1 }, new sbyte[] { -1, 0 }, new sbyte[] { 1, -1 }, 
            new sbyte[] { 1, 1 }, new sbyte[]{ -1, 1 }, new sbyte[]{ -1, -1 } };

        const byte distance = 1;

       // const byte radius = 10;


        public enum FloodFillResult { MaxRadiusReached, StayedWithinRadius }

        /// <summary>
        /// needs fill arrays of similar size to the search array
        /// </summary>
        /// <param name="terrainGrid"></param>
        /// <param name="allNodes">used to check radius</param>
        /// <param name="from"></param>
        /// <param name="regionColor"></param>
        /// <param name="radius"></param>
        public FloodFillResult DoFloodFill(MapManager.SubtileValue[][] terrainGrid, ushort[][] allNodes, byte[][] allNodeDistances, Point from, ushort regionColor, int radius) //int radius) how do we get the radius/depth?
        {
            // color = 0: blocked tile, or unpainted tile...

            Point currentTile;
            byte newDistance;

            int width, height;

            width = Common.GetJaggedArrayWidth(allNodes); 
            height = Common.GetJaggedArrayHeight(allNodes); 

           // int closedCounter = 0;

            ushort mNewLocationX, mNewLocationY;

            // test if this point is completely blocked? or does the caller do it...
            // clear the open queue:
            openQueue.Clear();

            // place starting point for the flood fill:
            openQueue.Enqueue(from);
            allNodes[from.X][from.Y] = regionColor;

            sbyte[] mDirectionColumn;
            while (openQueue.Count > 0) // && !mStop)
            {
                //mLocation = openQueue.Dequeue();
                currentTile = openQueue.Dequeue();
                newDistance = (byte)(allNodeDistances[currentTile.X][currentTile.Y] + distance);

                if (newDistance >= radius)
                {
                    return FloodFillResult.MaxRadiusReached;
                }

            
                for (int i = 0; i < 8; i++)
                {
                    
                    mDirectionColumn = mDirection[i];

                    mNewLocationX = (ushort)(currentTile.X + mDirectionColumn[0]); // mDirection[i, 0]);
                    mNewLocationY = (ushort)(currentTile.Y + mDirectionColumn[1]); // mDirection[i, 1]);
                 
                    if (mNewLocationX >= width || mNewLocationY >= height) // mNewLocationX >= mGridX || mNewLocationY >= mGridY)
                        continue;


                    // check to see if subtile is blocked:
                    if (MapManager.IsBlocked(terrainGrid[mNewLocationX][mNewLocationY])) 
                    {
                        continue;
                    }

                    //Is it in closed list? means this node was already processed - also if allNodes already has a color...
                    if (allNodes[mNewLocationX][mNewLocationY] > 0)
                        continue;

                
                    openQueue.Enqueue(new Point(mNewLocationX, mNewLocationY));

                    /*
                    if (regionColor == 523 && mNewLocationX == 118 && mNewLocationY == 61)
                    {
                        // was 321?
                    }*/

                    allNodes[mNewLocationX][mNewLocationY] = regionColor;
                    allNodeDistances[mNewLocationX][mNewLocationY] = newDistance;
                }

            }

            return FloodFillResult.StayedWithinRadius;

        }


        /// <summary>
        /// only fills within a rectangular area.   
        /// Max radius is optional
        /// </summary>
        /// <param name="terrainGrid"></param>
        /// <param name="allNodes">gets painted/filled in</param>
        /// <param name="fromSubtile"></param>
        /// <param name="areaSubtiles"></param>
        public FloodFillResult DoFloodFillOfArea(SubtileLayers terrainGrid, ushort[][] allNodes, Point fromSubtile, Rectangle areaSubtiles, byte[][] allNodeDistances = null, int? radius = null) 
        {
            // color = 0: blocked tile, or unpainted tile...

            Point currentTile;
            byte newDistance = 0;

            int width, height;

            width = Common.GetJaggedArrayWidth(allNodes);
            height = Common.GetJaggedArrayHeight(allNodes);

            ushort visited = 1;

            // int closedCounter = 0;

            ushort mNewLocationX, mNewLocationY;

            // test if this point is completely blocked? or does the caller do it...
            // clear the open queue:
            openQueue.Clear();
            // place starting point for the flood fill:

            int x = Common.Clamp((fromSubtile.X), 0, The.Map.mapSubtileWidth);
            int y = Common.Clamp((fromSubtile.Y), 0, The.Map.mapSubtileHeight);
            openQueue.Enqueue(new Point(x,y));
            
            // 0-based array with results:
            allNodes[x - areaSubtiles.X][y - areaSubtiles.Y] = visited;

            int indexX;
            int indexY;

            sbyte[] mDirectionColumn;
            while (openQueue.Count > 0) 
            {               
                currentTile = openQueue.Dequeue();

                if (allNodeDistances != null)
                {
                    newDistance = (byte)(allNodeDistances[currentTile.X - areaSubtiles.X][currentTile.Y - areaSubtiles.Y] + distance);

                    if (newDistance >= radius)
                    {
                        return FloodFillResult.MaxRadiusReached;
                    }
                }

                for (int i = 0; i < 8; i++)
                {

                    mDirectionColumn = mDirection[i];

                    mNewLocationX = (ushort)(currentTile.X + mDirectionColumn[0]); 
                    mNewLocationY = (ushort)(currentTile.Y + mDirectionColumn[1]); 

                    if (mNewLocationX >= areaSubtiles.Right || mNewLocationY < areaSubtiles.Top
                        || mNewLocationX < areaSubtiles.Left || mNewLocationY >= areaSubtiles.Bottom) 
                        continue;


                    // check to see if subtile is blocked:
                    if (MapManager.IsBlocked(terrainGrid.GetValue(mNewLocationX, mNewLocationY)))
                    {
                        continue;
                    }

                    indexX = mNewLocationX - areaSubtiles.X;
                    indexY = mNewLocationY - areaSubtiles.Y;

                    //Is it in closed list? means this node was already processed - also if allNodes already has a color...
                    if (allNodes[indexX][indexY] > 0)
                        continue;


                    openQueue.Enqueue(new Point(mNewLocationX, mNewLocationY));


                    allNodes[indexX][indexY] = visited;
                    if (allNodeDistances != null)
                    {
                        allNodeDistances[indexX][indexY] = newDistance;
                    }
                   
                }

            }

            return FloodFillResult.StayedWithinRadius;
        }

    }
}
