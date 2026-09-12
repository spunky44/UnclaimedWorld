


using System;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;


namespace UWGame.SimSide.AI.Pathfinding
{
   
    public class RegionSearcher : ISnapshot
    {
        #region Structs     
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct PathFinderNodeFast
        {
            #region Variables Declaration
            public int F; // f = gone + heuristic
            public int G;
            public ushort PX; // Parent
            public ushort PY;
            public byte Status;
           
            #endregion
        }
        #endregion
             

        private const int nodesPerCycle = 20;

        #region Variables Declaration

        private PriorityQueueB<RegionPathFinderNodeBFS> BFSOpen = null;
        private PriorityQueueB<RegionPathFinderNodeAStar> AStarOpen = null;

        /// <summary>
        /// give two sets of collections, divided by layers, pick the one to look up in by the color range.
        /// </summary>
       // public Dictionary<ushort, Dictionary<ushort, RegionEdge>> bottomRegionGraph;
       // private Dictionary<ushort, UWGame.SimSide.Maps.Region> baseRegions;

       // public Dictionary<ushort, Dictionary<ushort, RegionEdge>> overridingRegionGraph; // NEW
    

      //  private Dictionary<ushort, Dictionary<ushort, RegionEdge>> bottomToUpperLayerConnectors; // connections from the bottom graph to the overriding sectors.
      //  private Dictionary<ushort, List<ushort>> /* HashSet<Tuple<ushort, ushort>>*/ bottomBlockedEdges; // blocked edges on the bottom graph when there are overriding sectors

        RegionMap regionMap;
        private CyclableID regionMapID;

        public const byte StatusOpen = 1;
        public const byte StatusClosed = 2;

               
        private int mCloseNodeCounter = 0;
       

        private float newG = 0;

        private Dictionary<ushort, RegionPathFinderNodeBFS> BFSOpenAndClosed;
        private Dictionary<ushort, RegionPathFinderNodeAStar> AStarOpenAndClosed;


        private ushort start;

        public ushort Start
        {
            get
            {
                return start;
            }
        }

        /// <summary>
        /// a region. 0 for no region destiantion.
        /// </summary>
        public ushort destination;

        public ushort Destination
        {
            get
            {
                return destination;
            }
        }


        /// <summary>
        /// this can be set to a coordinate with destination=0. Then a search for the nearest region to that coordinate will be performed
        /// </summary>
        public Vector2 DestinationCenterLocation;

        private SearchType searchType;

        #endregion

        public enum SearchStatus { Finished, Incomplete };
      

        #region Constructors

        public enum SearchType { AllPathCosts, SinglePath }

        public RegionSearcher()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        /// <summary>
        /// BFS - no destination
        /// </summary>
        /// <param name="regionGraph"></param>
        /// <param name="start"></param>
        public RegionSearcher(RegionMap regionMap, /*Dictionary<ushort, Dictionary<ushort, RegionEdge>> regionGraph,*/ ushort start) 
        {
            searchType = SearchType.AllPathCosts;

         //   this.bottomRegionGraph = regionMap.RegionGraph; // regionGraph;

            this.regionMap = regionMap;
           
            if (start == 0)
            {
                // error here???
                throw new Exception();
            }            

            this.start = start;

            InitializeBFS();

        }

       

        /// <summary>
        /// AStar - with destination
        ///     
        /// </summary>
        /// <param name="regionGraph"></param>
        /// <param name="regions"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public RegionSearcher(RegionMap regionMap, 
            ushort start, ushort end)
        {
            this.searchType = SearchType.SinglePath;

        //    this.bottomRegionGraph = regionMap.RegionGraph; //regionGraph;
          //  this.baseRegions = regionMap.regions; // regions;

            this.regionMap = regionMap;
          
            //mStartingTransport = (int)startingTransport;

            if (start == 0)
            {
                // error here???
                throw new Exception();
            }

            this.start = start;
            this.destination = end;

            InitializeAStarSearch();
        }

        #endregion
        
        

        #region Properties
       
             
        public int? SearchLimit;
        

        public bool ReturnPathToClosestPoint = false;
        public ushort? ClosestRegion = null;

        #endregion

        #region Methods

        public string PrintInfo()
        {
            string text;
            if (searchType == SearchType.AllPathCosts)
            {
                text = string.Format("nodes: {0}", this.BFSOpenAndClosed.Count);
            }
            else
            {
                text = string.Format("{0}, nodes: {1}", searchType, this.AStarOpenAndClosed.Count);
            }

            return text;

        }

        /// <summary>
        /// is also called after Load to restart the search
        /// </summary>
        private void InitializeAStarSearch()
        {
            // replace with a List of objects (not struct values)?
            //Dictionary<int, ushort> costToThisNode = new Dictionary<int, ushort>();
            AStarOpenAndClosed = new Dictionary<ushort, RegionPathFinderNodeAStar>();

            // how do we do this differently???
            /* utilized to store the destination nodes of the edges on the search frontier, in order of increasing 
             distance (cost) from the source node.
             The node at the front will be the node not already on the shortest path tree that is closest to the source node.
             accumulated cost: F = actual (G) + heuristic (H) 
             
             * Change to Indexed Priority Queue! p. 238
             */

            AStarOpen = new PriorityQueueB<RegionPathFinderNodeAStar>(new CompareRegionPFNodeDictionaryAStar());



            mCloseNodeCounter = 0;
            //StatusOpen += 2; //??
           // StatusClosed += 2; // ??

            if (destination > 0) // will be 0 if we are searching for closest region to destination
            {
                DestinationCenterLocation = regionMap.GetRegion(destination).CenterLocation; // regionMap.regions[destination].CenterLocation;
            }

            RegionPathFinderNodeAStar startNode = new RegionPathFinderNodeAStar();
            startNode.Color = start;
            startNode.Parent = start;
            startNode.CenterLocation = regionMap.GetRegion(start).CenterLocation; // regionMap.regions[start].CenterLocation;

            startNode.G = 0;

            startNode.Status = StatusOpen;

            AStarOpen.Push(startNode);
            AStarOpenAndClosed.Add(startNode.Color, startNode);
            
        }

        private void InitializeBFS()
        {
            // replace with a List of objects (not struct values)?
            //Dictionary<int, ushort> costToThisNode = new Dictionary<int, ushort>();
            BFSOpenAndClosed = new Dictionary<ushort, RegionPathFinderNodeBFS>();

            // how do we do this differently???
            /* utilized to store the destination nodes of the edges on the search frontier, in order of increasing 
             distance (cost) from the source node.
             The node at the front will be the node not already on the shortest path tree that is closest to the source node.
             accumulated cost: F = actual (G) + heuristic (H) 
             
             * Change to Indexed Priority Queue! p. 238
             */

            BFSOpen = new PriorityQueueB<RegionPathFinderNodeBFS>(new CompareRegionPFNodeDictionary());


            mCloseNodeCounter = 0;
           // StatusOpen += 2; //??
          //  StatusClosed += 2; // ??
            BFSOpen.Clear();


            RegionPathFinderNodeBFS startNode = new RegionPathFinderNodeBFS();
            startNode.Color = start;

            startNode.G = 0;

            startNode.Status = StatusOpen;

            BFSOpen.Push(startNode);
            BFSOpenAndClosed.Add(startNode.Color, startNode);
        }


        public Dictionary<ushort, RegionPathFinderNodeBFS> GetBFSResult()
        {
            return BFSOpenAndClosed;
        }

      

      
        

        /// <summary>
        /// Execute one cycle of the BFS search.
        /// </summary>
        /// <returns></returns>
        public SearchStatus CycleBFS()
        {

            int cycleCounter = 0;

            if (BFSOpen.Count == 0)
            {
                return SearchStatus.Finished;
            }


            RegionPathFinderNodeBFS node, nextNode;

            while (cycleCounter < nodesPerCycle && BFSOpen.Count > 0)
            {
                node = BFSOpen.Pop();

                //Is it in closed list? means this node was already processed
                if (node.Status == StatusClosed)
                    continue;

                Dictionary<ushort, RegionEdge> connections;

                if (regionMap.GetConnectors(node.Color, out connections)) //  //bottomRegionGraph.TryGetValue(node.Color, out connections))
                {
                    //Lets calculate each successors
                    foreach (KeyValuePair<ushort, RegionEdge> kvp in connections)
                    {
                        // also do road cost???
                        newG = node.G + kvp.Value.Length; 

                        // can be null? then create new...
                        if (BFSOpenAndClosed.TryGetValue(kvp.Key, out nextNode))
                        {
                            //Is it open or closed?                                
                            if (nextNode.Status == StatusOpen || nextNode.Status == StatusClosed)
                            {
                                // The current node has less cost than the previous? then skip this node
                                if (nextNode.G <= newG)
                                    continue;
                            }
                        }
                        else
                        {
                            nextNode = new RegionPathFinderNodeBFS();
                            nextNode.Color = kvp.Key;

                            BFSOpenAndClosed.Add(kvp.Key, nextNode);
                        }

                        nextNode.G = newG;

                        BFSOpen.Push(nextNode);

                        nextNode.Status = StatusOpen;

                    }                   
                }

                mCloseNodeCounter++;
                node.Status = StatusClosed;

                ++cycleCounter;
            }

            return SearchStatus.Incomplete;
        }

         /// <summary>
        /// Execute one cycle of the BFS search.
        /// </summary>
        /// <returns></returns>
      /*  public SearchStatus CycleAStar()
        {

            int cycleCounter = 0;

            if (BFSOpen.Count == 0)
            {
                return SearchStatus.Finished;
            }


            RegionPathFinderNodeBFS node, nextNode;

            while (cycleCounter < nodesPerCycle && BFSOpen.Count > 0)
            {


            }
        }*/


        public enum PathInfo { CostOnly, FullPath }

        /// <summary>
        /// Get the path directly via AStar
        /// </summary>
        /// <returns></returns>
        public void FindPath(PathInfo pathInfo, ref RegionPath pathList, ref float distance) 
        {
                                  
            if (AStarOpen.Count == 0)
            {
                return; 
            }

            RegionPathFinderNodeAStar node = null, nextNode;

            bool pathFound = false;

            while (AStarOpen.Count > 0)
            {
                node = AStarOpen.Pop();
                
                //Is it in closed list? means this node was already processed
                if (node.Status == StatusClosed)
                {
                    continue;
                }

               
                if (node.Color == destination) 
                {
                   
                    node.Status = StatusClosed;
                    pathFound = true;
                    break; // FINISH!!!
                }

                if (SearchLimit.HasValue && mCloseNodeCounter > SearchLimit.Value)
                {
                    if (!ReturnPathToClosestPoint)
                    {                       
                        return;
                    }
                    else
                    {
                        FindClosestRegion();
                       
                        return;                        
                    }
                }

                Dictionary<ushort, RegionEdge> connections;

                if (node.Color == 40105)
                {

                }

                if (regionMap.GetConnectors(node.Color, out connections)) 
                {
                    //Lets calculate each successors
                 

                    foreach (KeyValuePair<ushort, RegionEdge> kvp in connections)
                    {
                        if (kvp.Key >= 1530 && kvp.Key <= 1545) //kvp.Key >= 1533 && kvp.Key <= 1548)
                        {

                        }
                        /*
                        if ((kvp.Key >= 61 && kvp.Key <= 69)
                            || (kvp.Key >= 89 && kvp.Key <= 92)
                             || (kvp.Key >= 212 && kvp.Key <= 214)) //kvp.Key >= 1533 && kvp.Key <= 1548)
                        {

                        }*/
                       
                        // also do road cost???
                        newG = node.G + kvp.Value.Length; //1; 
                        
                        // can be null? then create new...
                        if (AStarOpenAndClosed.TryGetValue(kvp.Key, out nextNode)) 
                        {
                            //Is it open or closed?
                            if (nextNode.Status == StatusOpen || nextNode.Status == StatusClosed)
                            {
                                // The current node has less cost than the previous? then skip this node
                                if (nextNode.G <= newG)
                                    continue;
                            }
                        }
                        else
                        {   
                            nextNode = new RegionPathFinderNodeAStar();
                            nextNode.Color = kvp.Key; // nextNodeColor;

                            nextNode.CenterLocation = regionMap.GetRegion(nextNode.Color).CenterLocation; // #CRASH1 here April 2016 - terrain map region did not exist

                            AStarOpenAndClosed.Add(kvp.Key, nextNode); 
                        }

                        nextNode.Parent = node.Color;
                      
                        nextNode.G = newG;

                        // Distance Octile
                        float dx = Math.Abs(nextNode.CenterLocation.X - DestinationCenterLocation.X);
                        float dy = Math.Abs(nextNode.CenterLocation.Y - DestinationCenterLocation.Y);

                        float h = 0;
                        if (dx > dy)
                        {                            
                            h = dx + dy * 0.5f;
                        }
                        else
                        {
                            h = dy + dx * 0.5f;
                        }

                        nextNode.F = newG + h;

                        AStarOpen.Push(nextNode);

                        nextNode.Status = StatusOpen;

                    }
                }

                mCloseNodeCounter++;
                node.Status = StatusClosed;

            }

            if (pathFound)
            {               

                if (pathInfo == PathInfo.FullPath)
                {                   
                    
                    RegionPathFinderNodeAStar pathNode = AStarOpenAndClosed[destination];

                    pathList = new RegionPath(pathNode.G); 


                    while (pathNode.Color != pathNode.Parent) 
                    {
                        pathList.AddNode(pathNode);
                        
                        //get the parent node
                        pathNode = AStarOpenAndClosed[pathNode.Parent];

                    }

                    pathList.AddNode(pathNode);

                    pathList.PathNodes.Reverse();

                    distance = pathList.Cost;

                }
                else
                {
                    distance = node.G; // check this...
                }

            }
            else
            {
                if (!ReturnPathToClosestPoint)
                {                    
                    return; 
                }
                else
                {
                    FindClosestRegion();
                   
                    return;
                }
            }

        }

        private void FindClosestRegion()
        {
            // the search failed within the limit, but we are requested to return the closest point we got to it.
            float minDistance = 10000000;
            float currentDistance;

            ushort? closestRegion = null;

            float currentX, currentY;
            // search open and closed lists for closest point:

            foreach (var kvp in AStarOpenAndClosed)
            {
                if (kvp.Value.Status == StatusOpen || kvp.Value.Status == StatusClosed)
                {

                    currentX = kvp.Value.CenterLocation.X;
                    currentY = kvp.Value.CenterLocation.Y;
                    
                    float dx = Math.Abs(currentX - DestinationCenterLocation.X);
                    float dy = Math.Abs(currentY - DestinationCenterLocation.Y);
                    if (dx > dy)
                    {
                        currentDistance = dx + dy * 0.5f;
                    }
                    else
                    {
                        currentDistance = dy + dx * 0.5f;
                    }

                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;

                        closestRegion = kvp.Key;

                        if (minDistance == 1)
                        {
                            break;
                        }
                    }
                }
            }

            ClosestRegion = closestRegion;
        }

       
        #endregion

        #region Inner Classes
        internal class CompareRegionPFNodeDictionary : IComparer<RegionPathFinderNodeBFS>
        {
          

            #region Constructors
            // public ComparePFNodeDictionary(Dictionary<int, ushort> dictionary) //PathFinderNodeNew> dictionary)
            public CompareRegionPFNodeDictionary()
            {
            }
            #endregion

            #region IComparer Members
            public int Compare(RegionPathFinderNodeBFS nodeA, RegionPathFinderNodeBFS nodeB)
            {
                if (nodeA.G > nodeB.G)
                    return 1;
                else if (nodeA.G < nodeB.G)
                    return -1;
                return 0;

                /*
                if (nodeA.F > nodeB.F)
                    return 1;
                else if (nodeA.F < nodeB.F)
                    return -1;
                return 0;
                        */       
            }
            #endregion
        }

        internal class CompareRegionPFNodeDictionaryAStar : IComparer<RegionPathFinderNodeAStar>
        {


            #region Constructors
            // public ComparePFNodeDictionary(Dictionary<int, ushort> dictionary) //PathFinderNodeNew> dictionary)
            public CompareRegionPFNodeDictionaryAStar()
            {
            }
            #endregion

            #region IComparer Members
            public int Compare(RegionPathFinderNodeAStar nodeA, RegionPathFinderNodeAStar nodeB)
            {              

              
                if (nodeA.F > nodeB.F)
                    return 1;
                else if (nodeA.F < nodeB.F)
                    return -1;
                return 0;
                        
            }
            #endregion
        }

        /*
        internal class ComparePFNodeDictionary : IComparer<int>
        {
            #region Variables Declaration
            Dictionary<int, PathFinderNodeNew> dictionary;
            //Dictionary<int, ushort> dictionary;
            #endregion

            #region Constructors
           // public ComparePFNodeDictionary(Dictionary<int, ushort> dictionary) //PathFinderNodeNew> dictionary)
            public ComparePFNodeDictionary(Dictionary<int, PathFinderNodeNew> dictionary)
            {
                this.dictionary = dictionary;
            }
            #endregion

            #region IComparer Members
            public int Compare(int a, int b)
            {
                //ushort costA = 0, costB = 0;
                PathFinderNodeNew nodeA, nodeB;
                dictionary.TryGetValue(a, out nodeA);
                dictionary.TryGetValue(b, out nodeB);
                if (nodeA.F > nodeB.F)
                    return 1;
                else if (nodeA.F < nodeB.F)
                    return -1;
                return 0;

               
            }
            #endregion
        }*/

        

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

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // after snapshot: scrap the progress and start over!
            this.destination = sn.DoUInt16(destination);
            this.DestinationCenterLocation = sn.DoVector2(DestinationCenterLocation);
            this.start = sn.DoUInt16(start);
            this.SearchLimit = sn.DoInt32Nullable(SearchLimit);
            this.searchType = sn.DoEnum(searchType);
            this.regionMapID = (CyclableID)sn.SnapshotID<ICyclable, CyclableID>(regionMap);
            this.ReturnPathToClosestPoint = sn.DoBool(ReturnPathToClosestPoint);

            sn.Ignore(StatusClosed);
            sn.Ignore(StatusOpen);
            sn.Ignore(AStarOpen);
            sn.Ignore(BFSOpen);
            sn.Ignore(AStarOpenAndClosed);
            sn.Ignore(BFSOpenAndClosed);
            sn.Ignore(this.ClosestRegion);
            sn.Ignore(this.mCloseNodeCounter);
            sn.Ignore(regionMap);
        /*    sn.Ignore(this.baseRegions);
            sn.Ignore(this.bottomRegionGraph);
            */

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

            regionMap = (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(regionMapID);
            //baseRegions = regionMap.regions;
           
            if (searchType == SearchType.SinglePath)
            {
                InitializeAStarSearch();
            }
            else
            {
                InitializeBFS();
            }
        }
    }
}
