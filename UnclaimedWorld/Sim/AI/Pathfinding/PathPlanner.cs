using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Pathfinding
{
    public class PathPlanner : ICyclable, ISnapshot
    {        
        EntityID entityID;

        // public for debugging only!
        public AStarSearch search;

      //  Point destination;

        private const int searchLimitToGetUnblocked = 800;

        public double StartedOnTimeInSeconds { get; set; }

        public static double totalComputationAllInstancesInSeconds;
        public double TotalComputationAllInstancesInSeconds
        {
            get
            {
                return totalComputationAllInstancesInSeconds;
            }
            set
            {
                totalComputationAllInstancesInSeconds = value;
            }
        }

        public double ComputationTimeSpentInSeconds { get; set; }


        public double? UpdateInterval 
        {
            get
            {
                return null;
            }
        }

        public PathPlanner(Entity entity)
        {           
            this.entityID = entity.EntityID;

            AddToLookup();
        }

        public PathPlanner()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public void Destroy()
        {
            The.Sim.CycleManager.UnRegister(this);

            RemoveIDEntry();
        }

        public bool IsPaused { get; set; }

        /// <summary>
        /// Gets the path directly by cycling in a loop.   
        /// 
        /// shorter paths become less direct when usign region nodes. And they cannot be smoothed when in an area with low vegetation or smaller obstacles.
        /// so don't use the high level path for shorter searches.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public List<PathFinderNode> FindShortPathDirectly(SubtileLayers layers, Vector3 start, Vector3 destination, int searchLimit) 
        {          
            Point startSubtile = MapManager.WorldPosToSubtile(start);
            Point destinationSubtile = MapManager.WorldPosToSubtile(destination);
            

            if (MapManager.IsBlocked(layers.GetValue(destinationSubtile)))
                return null;//can not path into a blocked subtile

            if (MapManager.IsBlocked(layers.GetValue(startSubtile)))
            {
                // switch...
                startSubtile = GetClosestPointToDestination(layers, start, searchLimitToGetUnblocked, destination); 
             
                if (startSubtile == null)
                    return null;
            }

            RegionPathID? highLevelPath = null; // layers.RegionMap.GetRegionPath(startSubtile, destinationSubtile);
            // do accessibility check instead:
            float distance = 0f;
            if (layers.RegionMap.GetDistance(null, startSubtile, destinationSubtile, ref distance, registerIfNotReady: false) == RegionMap.Result.NoAccess)
            {
                return null;
            }

            //search = new AStarSearch(mapCosts, endSubtile, fromSubtile, highLevelPath); //switched order
            search = new AStarSearch(layers, startSubtile, destinationSubtile, LookUp<RegionPath, RegionPathID>.FindByID(highLevelPath)); //NEW: normal order
            search.Formula = AStarSearch.HeuristicFormula.Manhattan;
            search.SearchLimit = searchLimit;

            List<PathFinderNode> path = null;
            // NEW - use the high level path if we have it.
            AStarSearch.SearchStatus status = AStarSearch.SearchStatus.Incomplete; 
            while(status == AStarSearch.SearchStatus.Incomplete)
            {
                status = search.CycleSearch();
            }
           
            if (status == AStarSearch.SearchStatus.TargetFound)
            {
                path = search.Path;
            }
            //path = search.FindPathDirectly(null); // OLD


            return path;
        }


        /// <summary>
        /// get the path by registering it with the path manager and waiting for the result
        ///       
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public bool FindPathByRequest(SubtileLayers layers, Vector3 start, Vector3 destination)
        {
            Point startSubtile = MapManager.WorldPosToSubtile(start);
            Point destinationSubtile = MapManager.WorldPosToSubtile(destination);

            RegionPathID? highLevelPath = layers.RegionMap.GetRegionPath(startSubtile, destinationSubtile);


            The.Sim.CycleManager.UnRegister(this); // necessary?


            if (MapManager.IsBlocked(layers.GetValue(destinationSubtile)))
                return false;//can not path into a blocked subtile

            if (MapManager.IsBlocked(layers.GetValue(startSubtile)))
            {
                startSubtile = GetClosestPointToDestination(layers, start, searchLimitToGetUnblocked, destination);
                if (startSubtile == null)
                    return false;
            }


            //  search = new AStarSearch(layers, destinationSubtile, startSubtile, highLevelPath); //switched
            search = new AStarSearch(layers, startSubtile, destinationSubtile,
                LookUp<RegionPath, RegionPathID>.FindByID(highLevelPath));


            The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);

            return true;

        }


        /// <summary>
        /// we are not interested in the path, we want to find the closest point to the inaccessible node
        /// </summary>
        /// <param name="mapCosts"></param>
        /// <param name="transportType"></param>
        /// <param name="destination"></param>
        /// <param name="returnPathToClosestPoint"></param>
        /// <param name="searchLimit"></param>
        /// <returns></returns>
        public Point GetClosestPointToDestination(SubtileLayers mapCosts, Vector3 destination, int searchLimit, Vector3 start) 
        {
            Point startSubtile = MapManager.WorldPosToSubtile(start); 
            Point destinationSubtile = MapManager.WorldPosToSubtile(destination); 

            // don't search backwards from a blocked destination:
            search = new AStarSearch(mapCosts, startSubtile, destinationSubtile, null); 
            search.Formula = AStarSearch.HeuristicFormula.Manhattan;  
            search.SearchLimit = searchLimit;          
            search.ReturnPathToClosestPoint = true;

            List<PathFinderNode> path = search.FindPathDirectly(null);

            if (search.ClosestPointToDestination.HasValue)
                return search.ClosestPointToDestination.Value;

            return startSubtile;//MLo: tragic, but the point search failed, so stay where you are
        }

        /// <summary>
        /// Get the path to an item satisfying the predicate directly:
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<PathFinderNode> FindItemAndGetPath(SubtileLayers mapCosts, AStarSearch.DijkstraTestNodeDelegate testNodePredicate, int searchLimit, Vector3 start) //Point from)
        {
            Point startSubtile = MapManager.WorldPosToSubtile(start); 

            search = new AStarSearch(mapCosts, startSubtile); 
            
            search.SearchLimit = searchLimit; // (int)NumSearchLimit.Value;
           
            List<PathFinderNode> path = search.FindPathDirectly(testNodePredicate);//end, new Point(owner.MapPosition.X, owner.MapPosition.Y));
          
            /*if (path != null)
            { // need to switch start and end nodes:
                path.Reverse();
            }*/
            
            return path;
        }


      /*  public List<PathFinderNode> GetPath(byte[][] mapCosts, AStarSearch.DijkstraTestNodeDelegate testNodePredicate, int searchLimit, Vector3 start) //Point from)
        {
            Point startSubtile = MapManager.WorldPosToSubtile(start); // .TileCenterToSubTile(start);

            search = new AStarSearch(mapCosts, startSubtile); //new Point(owner.MapPosition.X, owner.MapPosition.Y));
            search.Formula = AStarSearch.HeuristicFormula.None; // BFS
            //   search.Diagonals = true; // ChkDiagonals.Checked;
            //   search.HeavyDiagonals = true; // ChkHeavyDiagonals.Checked;
            search.HeuristicEstimate = 2; // (int)NumUpDownHeuristic.Value;
            search.PunishChangeDirection = false; // ChkPunishChangeDirection.Checked;
            //  search.TieBreaker = false; // ChkTieBraker.Checked;
            search.SearchLimit = searchLimit; // (int)NumSearchLimit.Value;
            search.DebugProgress = false; // ChlShowProgress.Checked;
            search.DebugFoundPath = true;

            List<PathFinderNode> path = search.FindPath(testNodePredicate);//end, new Point(owner.MapPosition.X, owner.MapPosition.Y));
            if (path != null)
            { // need to switch start and end nodes:
                path.Reverse();
            }

            return path;
        }
        */


        public List<PathFinderNode> Path
        {
            get
            {
                return search.Path;
            }
        }



       

        public void PrintInfo(StringBuilder text)
        {
            text.Append(string.Format("Pathfinding: {0}, {1}", ID, search.PrintInfo()));

        }


        #region ILookup

        private CyclableID id = CyclableID.Invalid;
      
        //=================== ILookup Methods =====================
        public CyclableID ID
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

        public CyclableID GetUniqueID()
        {
            return Cyclable.GetUniqueID();
        }

        public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
        {
            return (CyclableID)sn.DoEnum(id);
        }



        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != CyclableID.Invalid)
                LookUp<ICyclable, CyclableID>.Add(ID, this);
        }

        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }

        public void RemoveIDEntry()
        {
            LookUp<ICyclable, CyclableID>.Remove(this);
        }

        public void SetInvalid()
        {
            id = CyclableID.Invalid;
        }

        public void ResetIDCounter() // interface method - does nothing... Sim will call Cyclable.ResetIDCounter.
        {
        }

        void ILookUp<ICyclable, CyclableID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ICyclable, CyclableID>.Create();
        }

        #endregion


        public bool CycleOnce()
        {

#if DEBUG
           
           // System.Diagnostics.Debug.Assert(!MapManager.IsBlocked(search.Layers.GetValue(search.destination)), "blocked destination!");
           // System.Diagnostics.Debug.Assert(!MapManager.IsBlocked(search.Layers.GetValue(search.start)), "blocked start!");

            
#endif

            AStarSearch.SearchStatus status;
            
            status = search.CycleSearch();

            if (status == AStarSearch.SearchStatus.TargetNotFound
                || status == AStarSearch.SearchStatus.TargetFound)
            {
                if (entityID == (EntityID)24374)
                {

                }

                Entity entity = Entity.FindByID(entityID);

                if (entity != null)
                {
                    if (status == AStarSearch.SearchStatus.TargetNotFound)
                    {
                        entity.SendMessage(new Message(Message.MessageTypes.PathNotFound));                       
                    }
                    else if (status == AStarSearch.SearchStatus.TargetFound)
                    {
                        entity.SendMessage(new Message(Message.MessageTypes.PathFound));                       
                    }
                }

                return true;
            }

            return false; // status;

        }


        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return false; // keep the planner registered. The search, however, will start over.
            }
        }

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);
            this.entityID = sn.DoEnum(entityID);
            this.search = (AStarSearch)sn.DoISnapshot(search);
            this.IsPaused = sn.DoBool(IsPaused);

            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

            return this;
        }

        Snapshotter.Version version;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            if (search != null)
            {
                search.LoadPostProcess(sn);
            }
        }

        public bool IsSnapshotted
        {
            get;
            set;
        }

        #endregion

    }
}
