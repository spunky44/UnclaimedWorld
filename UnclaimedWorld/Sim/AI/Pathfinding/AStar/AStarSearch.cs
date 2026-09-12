//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//

using System;
using System.Text;
//using System.Drawing;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
//using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.AI.Pathfinding
{
   
    public class AStarSearch: ISnapshot 
    {
        public enum HeuristicFormula
        {
            Manhattan = 1,
            MaxDXDY = 2,
            DiagonalShortCut = 3,
            Euclidean = 4,
            EuclideanNoSQR = 5,
            Custom1 = 6,
            None = 7

        }

        #region Structs
        [Author("Franco, Gustavo")]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct PathFinderNodeFast
        {
            #region Variables Declaration
            public int F; // f = gone + heuristic
            public int G;
            public ushort PX; // Parent
            public ushort PY;
            public byte Status;
            // TODO: add a 'state' variable - telling which vehicle (type) we are onboard

            #endregion
        }
        #endregion
      
        #region Events
      //  public event PathFinderDebugHandler PathFinderDebug;
        #endregion

        #region Variables Declaration
     
     


       
        //public MapManager.SubtileValue[][] mGrid = null;
        public SubtileLayers Layers; // MapManager.SubtileValue[][] mGrid = null;
        private SubtileLayersID snapshotLayersID; // for snapshotting

        private RegionPath highLevelPath;
        private RegionPathID? snapshotHighLevelPath;
        private int highLevelPathIndex = 0;

        private PriorityQueueB<PathFinderNode> mOpen = null;
        private List<PathFinderNode> pathNodes = new List<PathFinderNode>();
        private bool mStop = false;
        private bool mStopped = true;
     
        private HeuristicFormula mFormula = HeuristicFormula.Manhattan;      
        private const int mHEstimate = 2;// what is this..?

       // private bool mPunishChangeDirection = false;
       
        /// <summary>
        /// From now on, we should require high level paths and not permit excessive point searches.
        /// </summary>
        private int mSearchLimit = 10000; // 500000; 
        private double mCompletedTime = 0;
      
        public byte StatusOpen = 1;
        public byte StatusClosed = 2;

        //Promoted local variables to member variables to avoid recreation between calls
        private int mH = 0;
     
       
        private ushort relativeX = 0;
        private ushort relativeY = 0;
      
        private ushort absoluteX = 0;
        private ushort absoluteY = 0;
        private ushort absoluteNextX = 0;
        private ushort absoluteNextY = 0;

        private int closeNodeCounter = 0;

        /// <summary>
        /// map width
        /// </summary>
        private ushort width = 0;

        /// <summary>
        /// map height
        /// </summary>
        private ushort height = 0;

     

     //   private bool mFound = false;

        private static sbyte[][] mDirection = new sbyte[][] { new sbyte[] { 0, -1 }, new sbyte[] { 1, 0 }, new sbyte[] { 0, 1 }, new sbyte[] { -1, 0 }, new sbyte[] { 1, -1 }, 
            new sbyte[] { 1, 1 }, new sbyte[]{ -1, 1 }, new sbyte[]{ -1, -1 } };

      
      //  private int mNewG = 0;

        /// <summary>
        /// circle area (BFS): pi * r^2 where r is the max distance between regions
        /// </summary>
        private readonly int searchLimitForHighLevelPath = (int)Math.Ceiling(3.5f * (Math.Pow(SubtileSector.RegionInterval, 2)));
      
        private Dictionary<int, PathFinderNode> openAndClosed;

        private bool returnPathToClosestPoint = false;

        public Microsoft.Xna.Framework.Point? ClosestPointToDestination = null;


        private Microsoft.Xna.Framework.Point start;
        private Microsoft.Xna.Framework.Point destination;
        private Microsoft.Xna.Framework.Point finalDestination;

        public delegate bool DijkstraTestNodeDelegate(int X, int Y); //, float mapWidthReciprocal, float mapHeightReciprocal);


        #endregion

        public enum SearchStatus { TargetFound, TargetNotFound, Incomplete };
        public List<PathFinderNode> Path
        {
            get
            {
                return pathNodes;
            }
        }

        #region Constructors

        public AStarSearch(SubtileLayers layers, Microsoft.Xna.Framework.Point start, Microsoft.Xna.Framework.Point destination, RegionPath highLevelPath) :
            this(layers, start)
        {
            this.highLevelPath = highLevelPath;

            if (this.highLevelPath == null)
            {                
               
            }

            this.destination = destination;
            this.finalDestination = destination;

            Initialize();
        }

        public AStarSearch(SubtileLayers layers, Microsoft.Xna.Framework.Point start)
        {
            this.Layers = layers;        

            this.start = start;
            this.Formula = AStarSearch.HeuristicFormula.None; // BFS
       
            Initialize();
        }

        public AStarSearch()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");       
        }

        #endregion


        public string PrintInfo()
        {
            string text;
            text = string.Format("nodes: {0}", closeNodeCounter);
           

            return text;

        }

        /// <summary>
        /// is also called after Load to restart the search
        /// </summary>
        private void Initialize()
        {

            width = (ushort)The.Map.mapSubtileWidth;
            height = (ushort)The.Map.mapSubtileHeight; 
                 
           
            openAndClosed = new Dictionary<int, PathFinderNode>();

            mOpen = new PriorityQueueB<PathFinderNode>(new ComparePFNodeDictionary()); 

            
            mStop = false;
            mStopped = false;
            closeNodeCounter = 0;
            StatusOpen += 2; //??
            StatusClosed += 2; // ?? the same??
            mOpen.Clear();
            pathNodes.Clear();

            ushort sectorX, sectorY;

            Layers.Layers[0].GetSectorFromAbsoluteCoords(start.X, start.Y, out sectorX, out sectorY);
            
            PathFinderNode startNode = new PathFinderNode();
            startNode.AbsoluteX = (ushort)start.X;
            startNode.AbsoluteY = (ushort)start.Y;
            startNode.G = 0;
            startNode.F = mHEstimate;

            System.Diagnostics.Debug.Assert(sectorX < 256, "Clamping error...");
            System.Diagnostics.Debug.Assert(sectorY < 256, "Clamping error...");

            startNode.SectorX = (byte)sectorX;
            startNode.SectorY = (byte)sectorY;
            startNode.ParentAbsoluteX = (ushort)start.X;
            startNode.ParentAbsoluteY = (ushort)start.Y;
            startNode.Status = StatusOpen;

            SetStartingNode(startNode);
        
            if (highLevelPath != null)
            {
                highLevelPathIndex = 1; // skip the first node...

                // sometimes we get high level paths with a single node. Don't use those paths...
                if (highLevelPath.PathNodes.Count > 1)
                {
                    destination = highLevelPath.PathNodes[highLevelPathIndex].RegionCenterInSubtiles;
                }
            }
            
        }

        #region Properties
        public bool Stopped
        {
            get { return mStopped; }
        }

        public HeuristicFormula Formula
        {
            get { return mFormula; }
            set { mFormula = value; }
        }

        /*
        public bool Diagonals
        {
            get { return mDiagonals; }
            set 
            { 
                mDiagonals = value; 
                if (mDiagonals)
                    mDirection = new sbyte[8,2]{{0,-1} , {1,0}, {0,1}, {-1,0}, {1,-1}, {1,1}, {-1,1}, {-1,-1}};
                else
                    mDirection = new sbyte[4,2]{{0,-1} , {1,0}, {0,1}, {-1,0}};
            }
        }
        */
        /*  public bool HeavyDiagonals
          {
              get { return mHeavyDiagonals; }
              set { mHeavyDiagonals = value; }
          }*/

       /* public int HeuristicEstimate
        {
            get { return mHEstimate; }
            set { mHEstimate = value; }
        }*/

     /*   public bool PunishChangeDirection
        {
            get { return mPunishChangeDirection; }
            set { mPunishChangeDirection = value; }
        }*/
               

        public bool ReturnPathToClosestPoint
        {
            get { return returnPathToClosestPoint; }
            set { returnPathToClosestPoint = value; }
        }

        public int SearchLimit
        {
            get { return mSearchLimit; }
            set { mSearchLimit = value; }
        }

        /*    public double CompletedTime
            {
                get { return mCompletedTime; }
                set { mCompletedTime = value; }
            }
            */
       /* public bool DebugProgress
        {
            get { return mDebugProgress; }
            set { mDebugProgress = value; }
        }*/

        #endregion

        #region Methods
        public void FindPathStop()
        {
            mStop = true;
        }


        private bool ComputeSuccessor(PathFinderNode node, SubtileSector sector, int directionIndex, /*ushort sectorX, ushort sectorY,*/ out PathFinderNode nextNode) //, out int newG)
        {
            MapManager.SubtileValue? value;
            nextNode = null;

            sbyte[] mDirectionColumn = mDirection[directionIndex];
              
            int relativeNextX = relativeX + mDirectionColumn[0];
            int relativeNextY = relativeY + mDirectionColumn[1];

            if (relativeNextX == 40 && relativeNextY == 8)
            {

            }

            /*  absoluteNextX = (ushort)(sectorX + relativeNextX);
              absoluteNextY = (ushort)(sectorY + relativeNextY);*/


            // when x/y is on the sector edge, look up into the layers
            // otherwise, look up in current sector.

            /* if (absoluteNextX >= width || absoluteNextY >= height)
                 continue; // reached the border. why not test for less than 0?
             */
            int nextSectorX, nextSectorY = -1;

            if (relativeNextX >= sector.SubtileArea.Width) // MapManager.SectorSizeInSubtiles)
            {               
                if (sector.Coords.X == The.Map.NoOfSectorsAcrossWidth - 1)
                {
                    return false;                  
                }

                // crossed the right sector edge
                relativeNextX = 0;

                nextSectorX = sector.Coords.X + 1;                
            }
            else if (relativeNextX < 0)
            {
                if (sector.Coords.X == 0)
                {
                    return false;
                    //continue; // reached the border
                }

                // crossed the left sector edge
                
                relativeNextX = MapManager.SectorSizeInSubtiles - 1;

                nextSectorX = sector.Coords.X - 1;
            }
            else
            {
                nextSectorX = sector.Coords.X;
            }

            if (relativeNextY >= sector.SubtileArea.Height) // MapManager.SectorSizeInSubtiles)
            {
                if (sector.Coords.Y == The.Map.NoOfSectorsAcrossHeight - 1)
                {
                    return false;
                    //continue;
                }

                // crossed the bottom sector edge
                relativeNextY = 0;
                
                nextSectorY = sector.Coords.Y + 1;
            }
            else if (relativeNextY < 0)
            {
                if (sector.Coords.Y == 0)
                {
                    return false;
                    //continue; // reached the border
                }

                // crossed the top sector edge
                
                relativeNextY = MapManager.SectorSizeInSubtiles - 1;

                nextSectorY = sector.Coords.Y - 1;
            }
            else
            {
                nextSectorY = sector.Coords.Y;
            }

            SubtileSector nextSector;
            if (nextSectorX != sector.Coords.X ||
                nextSectorY != sector.Coords.Y)
            {
                nextSector = Layers.GetSector(nextSectorX, nextSectorY);
            }
            else
            {
                nextSector = sector;
            }

            value = nextSector.GetValue(relativeNextX, relativeNextY).Value; 
            

            if (value == null)
            {
                return false;
            }

            // check if tile is blocked:
            if (MapManager.IsBlocked(value.Value))
            {
                return false;
            }

            byte cost = MapManager.GetCost(value.Value);

            // add the cost of the tile we are moving into:    
            int newG;       
            if (directionIndex < 4)
            {
                newG = node.G + cost;
            }
            else
            {   // diagonal edge:
                //mNewG = node.G + (int)(1.414f * (float)mGrid[nextNodeX][nextNodeY]); // huh? should be GetCost here also??
                newG = node.G + (int)(1.414f * (float)cost);
            }

          /*  absoluteNextX = (ushort)(sectorX + relativeNextX);// FEJL
            absoluteNextY = (ushort)(sectorY + relativeNextY);*/

           // SubtileSector nextSector = Layers.GetSector(sectorX, sectorY);
              
            absoluteNextX = (ushort)(nextSector.SubtileArea.X + relativeNextX);
            absoluteNextY = (ushort)(nextSector.SubtileArea.Y + relativeNextY);


            int newLocation = absoluteNextY * width + absoluteNextX;
            if (openAndClosed.TryGetValue(newLocation, out nextNode))
            {
                //Is it open or closed?                         
                if (nextNode.Status == StatusOpen || nextNode.Status == StatusClosed)
                {
                    // The current node has less cost than the previous? then skip this node
                    if (nextNode.G <= newG)
                        return false;
                }
            }
            else
            {
                nextNode = new PathFinderNode();
                nextNode.AbsoluteX = absoluteNextX;
                nextNode.AbsoluteY = absoluteNextY;

                System.Diagnostics.Debug.Assert(nextSectorX < 256, "Clamping error...");
                System.Diagnostics.Debug.Assert(nextSectorY < 256, "Clamping error...");

                nextNode.SectorX = (byte)nextSectorX;
                nextNode.SectorY = (byte)nextSectorY;
                openAndClosed.Add(newLocation, nextNode);
            }

            nextNode.ParentAbsoluteX = absoluteX;
            nextNode.ParentAbsoluteY = absoluteY;
            nextNode.G = newG;

            return true;

        }


        public void Draw()
        {
            int subTileSize = MapManager.subTileSize;

            byte alpha = 100;

            byte lowAlpha = 70;
            byte hiAlpha = 100;

            Vector2 from, to;
            Color color = Color.Black;
            color.A = alpha;

            int maxY = 3 * (The.MapUI.mapWindowTileY + The.MapUI.noOfTilesToDisplayVertically);
            int maxX = 3 * (The.MapUI.mapWindowTileX + The.MapUI.noOfTilesToDisplayHorizontally);


            int startY = The.MapUI.mapWindowTileY * 3;
            int startX = The.MapUI.mapWindowTileX * 3;


            int mNewLocation;
            int subTileWidth = 3 * The.Map.mapTileWidth;

            UWGame.SimSide.AI.Pathfinding.PathFinderNode node;

            for (int y = startY; y < maxY; y++)
            {
                for (int x = startX; x < maxX; x++)
                {
                    from = The.MapUI.SubtileEdgeToScreen(x, y);

                    if (MapManager.IsBlocked(Layers.GetValue(x, y)))
                    {
                        color = Color.Red;

                        color.A = alpha;

                        //Kensei.Dev.Shape.Line(from, to, color);
                        Kensei.Dev.Shape.Box(from, new Vector2(from.X + subTileSize, from.Y + subTileSize), color, true);
                    }

                    mNewLocation = y * subTileWidth + x;
                    if (openAndClosed.TryGetValue(mNewLocation, out node))
                    {
                        //Is it open or closed?
                        //if (mCalcGrid[mNewLocation].Status == StatusOpen || mCalcGrid[mNewLocation].Status == StatusClosed)

                        if (node.Status == StatusOpen)
                        {
                            color = Color.Gold;
                        }
                        else if (node.Status == StatusClosed)
                        {
                            color = Color.DarkGreen;
                        }

                        color.A = alpha;

                        //Kensei.Dev.Shape.Line(from, to, color);
                        Kensei.Dev.Shape.Box(from, new Vector2(from.X + subTileSize, from.Y + subTileSize), color, true);

                    }

                    alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);
                }

                alpha = (alpha == hiAlpha ? lowAlpha : hiAlpha);
            }

            from = The.MapUI.SubtileEdgeToScreen(start.X, start.Y);
            color = Color.White;
            color.A = 160;
            Kensei.Dev.Shape.Box(from, new Vector2(from.X + subTileSize, from.Y + subTileSize), color, true);

            from = The.MapUI.SubtileEdgeToScreen(finalDestination.X, finalDestination.Y);
            color = Color.Black;
            color.A = 160;
            Kensei.Dev.Shape.Box(from, new Vector2(from.X + subTileSize, from.Y + subTileSize), color, true);

            if (highLevelPath != null && highLevelPath.PathNodes.Count > 1)
            {

                for (int i = 1; i < highLevelPath.PathNodes.Count; i++)
                {
                    if (highLevelPathIndex < i)
                    {
                        color = Color.DarkRed;
                    }
                    else if (highLevelPathIndex == i)
                    {
                        color = Color.Beige;
                    }
                    else
                    {
                        color = Color.Cyan;
                    }

                    RegionPathNode highNode = highLevelPath.PathNodes[i];
                    from = The.MapUI.SubtileEdgeToScreen(highNode.RegionCenterInSubtiles);
                    Kensei.Dev.Shape.Box(from, new Vector2(from.X + subTileSize, from.Y + subTileSize), color, true);
                   
                }
                          

            }
        }



        const int nodesPerCycle = 40; // 10;
       
        /// <summary>
        /// Execute one cycle of the search.
        /// </summary>
        /// <returns></returns>
        public SearchStatus CycleSearch()
        {
            bool destinationFound = false;
            int cycleCounter = 0;


            if (mOpen.Count == 0)
            {   // the queue is empty. target not found.
                return SearchStatus.TargetNotFound;
            }

            if (MapManager.IsBlocked(Layers.GetValue(destination))
             || MapManager.IsBlocked(Layers.GetValue(start)))
            {
                // NEW:
                // the move map was updated and one of the endpoints is now blocked, making the search impossible.
                // there are other search modes that permit blocked endpoints.
                return SearchStatus.TargetNotFound;
            }
        

            if (closeNodeCounter > 10000)
            {

            }

            ushort sectorX, sectorY;

            PathFinderNode node, nextNode;

            while (cycleCounter < nodesPerCycle && mOpen.Count > 0 && !mStop)
            {
                node = mOpen.Pop();
                
                //Is it in closed list? means this node was already processed
                if (node.Status == StatusClosed)
                    continue;

                absoluteX = node.AbsoluteX; 
                absoluteY = node.AbsoluteY;
              
                if (absoluteX == destination.X && absoluteY == destination.Y)
                {
                    if (highLevelPath != null)
                    {
                        if (highLevelPathIndex == highLevelPath.PathNodes.Count - 2) // if on the second-to-last point, we may want to skip the last point to avoid backtracking around an obstacle that cannot be smoothed away :
                        {
                            highLevelPathIndex += 2;
                            // skip the last node. go for the final destination instead, to avoid detours:
                            SetNextDestinationPoint(node, finalDestination);


                           /* Point lastRegionCenter = highLevelPath.PathNodes[highLevelPathIndex + 1].RegionCenterInSubtiles;
                            if (Common.DistanceOctile(destination, finalDestination) <
                                Common.DistanceOctile(destination, lastRegionCenter))
                            {
                                highLevelPathIndex += 2;
                                // skip the last node. go for the final destination instead, as it is closer:
                                SetNextDestinationPoint(node, finalDestination);
                            }
                            else
                            {
                                // move to the final high level node:
                                highLevelPathIndex++;
                                SetNextDestinationPoint(node, highLevelPath.PathNodes[highLevelPathIndex].RegionCenterInSubtiles);
                            }*/

                        }                       
                        else if (highLevelPathIndex < highLevelPath.PathNodes.Count - 1) // if not on the last point:
                        {
                            highLevelPathIndex++;
                            
                            SetNextDestinationPoint(node, highLevelPath.PathNodes[highLevelPathIndex].RegionCenterInSubtiles);
                        }                       
                        else
                        {
                            // do the final stretch from the last region center to the destination:
                            if (destination != finalDestination)
                            {                               
                                SetNextDestinationPoint(node, finalDestination);
                            }
                            else 
                            {
                                // done.
                                node.Status = StatusClosed;
                                destinationFound = true;
                                break; // FINISH!!!
                            }
                        }
                    }
                    else
                    {
                        node.Status = StatusClosed;
                        destinationFound = true;
                        break; // FINISH!!! - why was this commented out?
                    }
                }
                else if (closeNodeCounter > mSearchLimit
                    || (highLevelPath != null && openAndClosed.Count > searchLimitForHighLevelPath))
                {
                    mStopped = true;
                    return SearchStatus.TargetNotFound;
                }
                else
                {
                    sectorX = node.SectorX;
                    sectorY = node.SectorY;

                    SubtileSector sector = Layers.GetSector(sectorX, sectorY);
                    relativeX = (ushort)(absoluteX - sector.SubtileArea.X); // sector.Coords.X);
                    relativeY = (ushort)(absoluteY - sector.SubtileArea.Y); // sector.Coords.Y);

                    //calculate the successors             
                    for (int i = 0; i < 8; i++)
                    {
                        if (!ComputeSuccessor(node, sector, i, out nextNode))
                        {
                            continue;
                        }

                        // DistanceOctile
                        int dx = Math.Abs(absoluteNextX - destination.X);
                        int dy = Math.Abs(absoluteNextY - destination.Y);
                        if (dx > dy)
                        {
                            // distance between diagonally adjacent tiles: 1.5f
                            // multiply by two (mHEstimate) since the lowest cost from one tile to another is 2.
                            mH = (int)(mHEstimate * (dx + (float)dy * 0.5f));
                        }
                        else
                        {
                            mH = (int)(mHEstimate * (dy + (float)dx * 0.5f));
                        }

                        nextNode.F = nextNode.G + mH;

                        mOpen.Push(nextNode);
                        nextNode.Status = StatusOpen;
                    }

                    node.Status = StatusClosed; // done with this node.

                    // TODO: after looking at the 8 edges, see if there are special edges representing available vehicles here...

                }

                closeNodeCounter++;
              //  node.Status = StatusClosed;

                ++cycleCounter;
            }

            if (destinationFound)
            {
              //  pathNodes.Clear();

                CreatePathSegment(destination);

                pathNodes.Reverse(); // NEW: reverse.

                mStopped = true;

                return SearchStatus.TargetFound;
            }

            return SearchStatus.Incomplete;


        }

        private void CreatePathSegment(Point destinationToUse)
        {
            int posX = destinationToUse.X;
            int posY = destinationToUse.Y;

            PathFinderNode currentNode = openAndClosed[destinationToUse.Y * width + destinationToUse.X];

            int counter = 0;

            int index = 0;

            bool pathNodesIsEmpty = pathNodes.Count == 0;

            // create the path backwards...
            while (currentNode.AbsoluteX != currentNode.ParentAbsoluteX || currentNode.AbsoluteY != currentNode.ParentAbsoluteY)
            {
                //pathNodes.Add(currentNode); 
                pathNodes.Insert(index, currentNode); // insert before any previous segment. in the end, the whole list will need to be reversed.

                posX = currentNode.ParentAbsoluteX;
                posY = currentNode.ParentAbsoluteY;
                currentNode = openAndClosed[posY * width + posX];

                index++;

                counter++;
                if (counter > 30)
                {

                }
            }

            if (pathNodesIsEmpty)
            {   // avoid duplicates. only add the destination the first time
                pathNodes.Add(currentNode);
            }
        }

        private void SetNextDestinationPoint(PathFinderNode newStartingNode, Point newDestination)
        {
            // clear the open list
            // set a new start node and destination node
            // continue cycling

            // create the path segment before we clear:
            CreatePathSegment(destination);

            destination = newDestination;

            mOpen.Clear();           
            openAndClosed.Clear(); // necessary, because we may have to backtrack a bit to reach the next point...

            SetStartingNode(newStartingNode);// we have 1 starting point again, don't revisit the starting node

            
            newStartingNode.Status = StatusOpen;
            newStartingNode.ParentAbsoluteX = newStartingNode.AbsoluteX;
            newStartingNode.ParentAbsoluteY = newStartingNode.AbsoluteY;
        }

        private void SetStartingNode(PathFinderNode startingNode)
        {
            mOpen.Push(startingNode); 
            openAndClosed.Add(startingNode.AbsoluteY * width + startingNode.AbsoluteX, startingNode); 
        }


        #region OLD
        /*  public SearchStatus CycleSearch()
        {
         
                int cycleCounter = 0;


                if (mOpen.Count == 0)
                {   // the queue is empty. target not found.
                    return SearchStatus.TargetNotFound;
                }


                PathFinderNode node, nextNode;
                sbyte[] mDirectionColumn;

                while (cycleCounter < 10 && mOpen.Count > 0 && !mStop)
                {                   
                    node = mOpen.Pop();
                    
                    //Is it in closed list? means this node was already processed
                    if (node.Status == StatusClosed) 
                        continue;
                    
                    nodeX = node.X; // optimization!
                    nodeY = node.Y;

                    if (nodeX == destination.X && nodeY == destination.Y)
                    {
                      
                        node.Status = StatusClosed;
                        mFound = true;
                        // break; // FINISH!!!
                    }

                    if (mCloseNodeCounter > mSearchLimit)
                    {
                        mStopped = true;

                        return SearchStatus.TargetNotFound;
                    }

                    if (mPunishChangeDirection)
                    {
                        mHoriz = nodeX - node.PX;
                    }


                    //Lets calculate each successors                   
                    for (int i = 0; i < 8; i++)
                    {
                        // get/create nextNode here????
                        mDirectionColumn = mDirection[i];
                        nextNodeX = (ushort)(nodeX + mDirectionColumn[0]);
                        nextNodeY = (ushort)(nodeY + mDirectionColumn[1]);

                        if (nextNodeX >= mGridX || nextNodeY >= mGridY)
                            continue;

                        // check if tile is blocked:
                        if (MapManager.IsBlocked(mGrid[nextNodeX][nextNodeY]))
                        {
                            continue;
                        }


                        // add the cost of the tile we are moving into:                       
                        if (i < 4)
                        {
                            mNewG = node.G + MapManager.GetCost(mGrid[nextNodeX][nextNodeY]);
                        }
                        else
                        {   // diagonal edge:
                            mNewG = node.G + (int)(1.414f * (float)mGrid[nextNodeX][nextNodeY]);
                        }
                       
                        if (mPunishChangeDirection)
                        {                       
                            if ((nextNodeX - nodeX) != 0)
                            {
                                if (mHoriz == 0)
                                    mNewG += Math.Abs(nextNodeX - destination.X) + Math.Abs(nextNodeY - destination.Y);
                            }

                            if ((nextNodeY - nodeY) != 0)
                            {
                                if (mHoriz != 0)
                                    mNewG += Math.Abs(nextNodeX - destination.X) + Math.Abs(nextNodeY - destination.Y);
                            }
                        }

                        // can be null? then create new...
                        mNewLocation = nextNodeY * mGridX + nextNodeX;
                        if (openAndClosed.TryGetValue(mNewLocation, out nextNode))
                        {
                            //Is it open or closed?                         
                            if (nextNode.Status == StatusOpen || nextNode.Status == StatusClosed)
                            {
                                // The current node has less cost than the previous? then skip this node
                                if (nextNode.G <= mNewG)
                                    continue;
                            }
                        }
                        else
                        {   // ???
                            nextNode = new PathFinderNode();
                            nextNode.X = nextNodeX;
                            nextNode.Y = nextNodeY;
                            openAndClosed.Add(mNewLocation, nextNode);

                        }
                       
                        nextNode.PX = nodeX;
                        nextNode.PY = nodeY;
                        nextNode.G = mNewG;
                        
                        // DistanceOctile
                        int dx = Math.Abs(nextNodeX - destination.X);
                        int dy = Math.Abs(nextNodeY - destination.Y);
                        if (dx > dy)
                        {
                            // distance between diagonally adjacent tiles: 1.5f
                            // multiply by two (mHEstimate) since the lowest cost from one tile to another is 2.
                            mH = (int)(mHEstimate * (dx + (float)dy * 0.5f));
                        }
                        else
                        {                           
                            mH = (int)(mHEstimate * (dy + (float)dx * 0.5f));
                        }  
                        
                        nextNode.F = mNewG + mH;                       

                        mOpen.Push(nextNode);
                        nextNode.Status = StatusOpen;
                    }

                    // TODO: after looking at the 8 edges, see if there are special edges representing available vehicles here...

                    mCloseNodeCounter++;
                    node.Status = StatusClosed;
                   
                    ++cycleCounter;
                }

                if (mFound)
                {
                    mClose.Clear();
                    int posX = destination.X;
                    int posY = destination.Y;

                    PathFinderNode fNode = openAndClosed[destination.Y * mGridX + destination.X];
                 
                    while (fNode.X != fNode.PX || fNode.Y != fNode.PY)
                    {
                        mClose.Add(fNode); // why...?
                        
                        posX = fNode.PX;
                        posY = fNode.PY;
                        fNode = openAndClosed[posY * mGridX + posX];
                    }

                    mClose.Add(fNode);
                    
                    mStopped = true;

                    return SearchStatus.TargetFound;
                }

                return SearchStatus.Incomplete;


        }*/
        #endregion


        /// <summary>
        /// Get the path directly. Cannot use high-level paths!
        /// </summary>
        /// <returns></returns>
        public List<PathFinderNode> FindPathDirectly(DijkstraTestNodeDelegate testNodePredicate)
        {
            bool destinationFound = false;           
            PathFinderNode node, nextNode;           

            ushort sectorX, sectorY;

            while (mOpen.Count > 0 && !mStop)
            {
                node = mOpen.Pop();

                //Is it in closed list? means this node was already processed
                if (node.Status == StatusClosed)
                    continue;

                absoluteX = node.AbsoluteX; 
                absoluteY = node.AbsoluteY;

                if (testNodePredicate != null)  // Dijkstra
                {
                    if (testNodePredicate(absoluteX, absoluteY))
                    {
                        // Dijkstra search completed:

                        node.Status = StatusClosed;
                        destination.X = absoluteX;
                        destination.Y = absoluteY;

                        ClosestPointToDestination = destination;

                        destinationFound = true;
                        break; // FINISH!!!
                    }
                }
                else if (absoluteX == destination.X 
                      && absoluteY == destination.Y) // we will never have both a predicate and an end point.
                {
                    ClosestPointToDestination = destination;

                    node.Status = StatusClosed;
                    destinationFound = true;
                    break; // FINISH!!!
                }

                if (closeNodeCounter > mSearchLimit)
                {
                    if (!returnPathToClosestPoint)
                    {
                        mStopped = true;
                        return null;
                    }
                    else
                    {
                        // the search failed within the limit, but we are requested to return the closest point we got to it.
                        int minDistance = 10000000;
                        int currentDistance;
                        Microsoft.Xna.Framework.Point closestPoint = new Microsoft.Xna.Framework.Point(-1, -1);

                        int currentX, currentY;

                        // search open and closed lists for closest point:
                        foreach (KeyValuePair<int, PathFinderNode> kvp in openAndClosed)
                        {
                            if (kvp.Value.Status == StatusOpen || kvp.Value.Status == StatusClosed)
                            {

                                currentX = kvp.Value.AbsoluteX;
                                currentY = kvp.Value.AbsoluteY;

                                currentDistance = Math.Abs(currentX - destination.X) + Math.Abs(currentY - destination.Y);
                                if (currentDistance < minDistance)
                                {
                                    minDistance = currentDistance;
                                    closestPoint.X = currentX;
                                    closestPoint.Y = currentY;

                                    if (minDistance == 1)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        ClosestPointToDestination = closestPoint;

                        break;
                    }
                }

                sectorX = node.SectorX;
                sectorY = node.SectorY;

                SubtileSector sector = Layers.GetSector(sectorX, sectorY);
                relativeX = (ushort)(absoluteX - sector.SubtileArea.X);
                relativeY = (ushort)(absoluteY - sector.SubtileArea.Y); //.Coords.Y);
               
                //calculate successors
               
                for (int i = 0; i < 8; i++)
                {
                    if (!ComputeSuccessor(node, sector, i, out nextNode))
                    {
                        continue;
                    }

                    mH = GetHeuristic();

                    //nextNode.F = mNewG + mH;
                    nextNode.F = nextNode.G + mH;

                    mOpen.Push(nextNode);
                    nextNode.Status = StatusOpen;
                }

                closeNodeCounter++;
                node.Status = StatusClosed;

            }

            if (destinationFound)
            {
                pathNodes.Clear();
                int posX = destination.X;
                int posY = destination.Y;

                PathFinderNode lastNode = openAndClosed[destination.Y * width + destination.X];

                while (lastNode.AbsoluteX != lastNode.ParentAbsoluteX || lastNode.AbsoluteY != lastNode.ParentAbsoluteY)
                {
                    // work backwards...
                    pathNodes.Add(lastNode);

                    posX = lastNode.ParentAbsoluteX;
                    posY = lastNode.ParentAbsoluteY;

                    lastNode = openAndClosed[posY * width + posX];

                }

                pathNodes.Add(lastNode);

                // NEW: reverse:
                pathNodes.Reverse();


                mStopped = true;
                return pathNodes;
            }

            mStopped = true;
            return null;

        }

        private int GetHeuristic()
        {
            switch (mFormula)
            {
                default:
                case HeuristicFormula.Manhattan:
                    mH = mHEstimate * (Math.Abs(absoluteNextX - destination.X) + Math.Abs(absoluteNextY - destination.Y));
                    break;
                case HeuristicFormula.MaxDXDY:
                    mH = mHEstimate * (Math.Max(Math.Abs(absoluteNextX - destination.X), Math.Abs(absoluteNextY - destination.Y)));
                    break;
                case HeuristicFormula.DiagonalShortCut:
                    int h_diagonal = Math.Min(Math.Abs(absoluteNextX - destination.X), Math.Abs(absoluteNextY - destination.Y));
                    int h_straight = (Math.Abs(absoluteNextX - destination.X) + Math.Abs(absoluteNextY - destination.Y));
                    mH = (mHEstimate * 2) * h_diagonal + mHEstimate * (h_straight - 2 * h_diagonal);
                    break;
                case HeuristicFormula.Euclidean:
                    mH = (int)(mHEstimate * Math.Sqrt(Math.Pow((absoluteNextY - destination.X), 2) + Math.Pow((absoluteNextY - destination.Y), 2)));
                    break;
                case HeuristicFormula.EuclideanNoSQR:
                    mH = (int)(mHEstimate * (Math.Pow((absoluteNextX - destination.X), 2) + Math.Pow((absoluteNextY - destination.Y), 2)));
                    break;
                case HeuristicFormula.Custom1:
                    Point dxy = new Point(Math.Abs(destination.X - absoluteNextX), Math.Abs(destination.Y - absoluteNextY));
                    int Orthogonal = Math.Abs(dxy.X - dxy.Y);
                    int Diagonal = Math.Abs(((dxy.X + dxy.Y) - Orthogonal) / 2);
                    mH = mHEstimate * (Diagonal + Orthogonal + dxy.X + dxy.Y);
                    break;
                case HeuristicFormula.None: // Dijkstra BFS!
                    mH = 0;
                    break;
            }

            return mH;
        }

        #endregion

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // after snapshot: scrap the progress and start over!
            this.destination = sn.DoPoint(destination);
            this.start = sn.DoPoint(start);
            this.finalDestination = sn.DoPoint(finalDestination);
            this.SearchLimit = sn.DoInt32(SearchLimit);
         
            this.snapshotLayersID = (SubtileLayersID)sn.SnapshotID<SubtileLayers, SubtileLayersID>(Layers);
           
            this.returnPathToClosestPoint = sn.DoBool(returnPathToClosestPoint);
          //  this.mPunishChangeDirection = sn.DoBool(mPunishChangeDirection);
            this.mFormula = sn.DoEnum(mFormula);


            if (sn.mode != Snapshotter.Mode.Load && highLevelPath != null)
            {
                // path may be destroyed!
                if (highLevelPath.ID != RegionPathID.Invalid)
                {
                    this.snapshotHighLevelPath = highLevelPath.ID;
                }
                else
                {
                    this.snapshotHighLevelPath = null;
                }
            }

            this.snapshotHighLevelPath = sn.DoEnumNullable(snapshotHighLevelPath);

            //this.snapshotHighLevelPath = sn.SnapshotID<RegionPath, RegionPathID>(highLevelPath); // sn.DoList(highLevelPath);

           
            sn.Ignore(highLevelPath);

            // ignore all fields set in Initialize:
            sn.Ignore(highLevelPathIndex);
            sn.Ignore(Layers);
            sn.Ignore(ClosestPointToDestination);    
            sn.Ignore(mStop);
            sn.Ignore(mStopped);
            sn.Ignore(StatusOpen);
            sn.Ignore(StatusClosed);
            sn.Ignore(width);
            sn.Ignore(height);
            sn.Ignore(mCompletedTime);
            sn.Ignore(openAndClosed);
            sn.Ignore(mOpen);
            sn.Ignore(pathNodes);
            sn.Ignore(mH);
            sn.Ignore(mDirection);
            sn.Ignore(absoluteNextX);
            sn.Ignore(absoluteNextY);
            sn.Ignore(absoluteX);
            sn.Ignore(absoluteY);
            sn.Ignore(relativeX);
            sn.Ignore(relativeY);
            sn.Ignore(closeNodeCounter);
            sn.Ignore(searchLimitForHighLevelPath);

            return this;

        }

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


        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Layers = LookUp<SubtileLayers, SubtileLayersID>.FindByID(snapshotLayersID);
            highLevelPath = LookUp<RegionPath, RegionPathID>.FindByID(snapshotHighLevelPath);

            // sequence error here? Layers.Layers is null?? It only happens after restart, because the sort order is different - they have the same key.
            // LookUp collection sequence... Cyclable (PathPlanner) vs. SubtileLayers

            // restart:
            Initialize();
        }

        #endregion


        #region Inner Classes
        internal class ComparePFNodeDictionary : IComparer<PathFinderNode>
        {
            #region Variables Declaration
            // Dictionary<int, PathFinderNodeNew> dictionary;
            //Dictionary<int, ushort> dictionary;
            #endregion

            #region Constructors
            // public ComparePFNodeDictionary(Dictionary<int, ushort> dictionary) //PathFinderNodeNew> dictionary)
            public ComparePFNodeDictionary()
            {
            }
            #endregion

            #region IComparer Members
            public int Compare(PathFinderNode nodeA, PathFinderNode nodeB)
            {

                if (nodeA.F > nodeB.F)
                    return 1;
                else if (nodeA.F < nodeB.F)
                    return -1;
                return 0;

                /*
                dictionary.TryGetValue(a, out costA);
                dictionary.TryGetValue(b, out costB);
                if (costA > costB)
                    return 1;
                else if (costA < costB)
                    return -1;
                return 0;*/
            }
            #endregion
        }

               

        [Author("Franco, Gustavo")]
        internal class ComparePFNodeMatrix : IComparer<int>
        {
            #region Variables Declaration
            PathFinderNodeFast[] mMatrix;
            #endregion

            #region Constructors
            public ComparePFNodeMatrix(PathFinderNodeFast[] matrix)
            {
                mMatrix = matrix;
            }
            #endregion

            #region IComparer Members
            public int Compare(int a, int b)
            {
                if (mMatrix[a].F > mMatrix[b].F)
                    return 1;
                else if (mMatrix[a].F < mMatrix[b].F)
                    return -1;
                return 0;
            }
            #endregion
        }
        #endregion
    }
}
