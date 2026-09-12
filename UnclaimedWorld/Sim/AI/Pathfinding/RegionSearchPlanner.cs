using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding
{
    /// <summary>
    /// has a list of searches/clients from the same region. Will send the result to all of them.
    /// </summary>
    public class RegionSearchPlanner: ICyclable, ISnapshot
    {
       
        public List<RegionSearchRequestID> SearchRequests = new List<RegionSearchRequestID>();

        RegionSearcher searcher;

        CyclableID regionMapID;

        public bool IsPaused { get; private set; }

        public double StartedOnTimeInSeconds { get; set; }

        public bool IsBFS;

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

        public RegionSearchPlanner()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        /// <summary>
        /// BFS search
        /// </summary>
        /// <param name="regionMap"></param>
        /// <param name="start"></param>
        public RegionSearchPlanner(RegionMap regionMap, ushort start) 
        {
            IsBFS = true;
            searcher = new RegionSearcher(regionMap, start);
            this.regionMapID = regionMap.ID;

            Init(regionMap);
        }

        /// <summary>
        /// AStar search
        /// </summary>
        /// <param name="regionMap"></param>
        /// <param name="start"></param>
        /// <param name="destination"></param>
        public RegionSearchPlanner(RegionMap regionMap, ushort start, ushort destination)
        {
            searcher = new RegionSearcher(regionMap, start, destination);
            this.regionMapID = regionMap.ID;

            Init(regionMap);
        }


        private void Init(RegionMap regionMap)
        {
            AddToLookup(); // should remove its ID once the search is complete

            
            regionMap.AddSearchPlanner(this);

            // only register the search after the region map has been built!
            The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
        }

      


        public void Destroy()
        {
            The.Sim.CycleManager.UnRegister(this);

            RemoveIDEntry();
        }

     
        public void PrintInfo(StringBuilder text)
        {
            if (IsBFS)
            {
                text.Append(string.Format("Region search from {0}, requests: {1}, {2}", searcher.Start, SearchRequests.Count, searcher.PrintInfo()));
            }
            else
            {
                text.Append(string.Format("Region search from {0}, to {1}, requests: {2}, {3}", searcher.Start, searcher.destination, SearchRequests.Count, searcher.PrintInfo()));
          
            }

        }

        /// <summary>
        /// Get the path directly using AStar. Don't create a planner for this.
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public static void GetPathDirectly(RegionSearcher.PathInfo pathInfo, RegionMap regionMap, 
            ushort start, ushort end, 
            ref RegionPath pathList, ref float cost) 
        {

            RegionSearcher search = new RegionSearcher(regionMap, start, end);     
           
            search.FindPath(pathInfo, ref pathList, ref cost); 
        }


        public static ushort GetClosestRegionToDestination(RegionSearcher.PathInfo pathInfo, RegionMap regionMap,
            ushort start, int searchLimit, Vector3 destination) 
        {
            RegionSearcher search = new RegionSearcher(regionMap, start, 0); // <- hack?   
            search.DestinationCenterLocation = destination.ToVector2(); // <- hack?

            search.SearchLimit = searchLimit;
            search.ReturnPathToClosestPoint = true;

            float distance = 0f;
            RegionPath pathList = null;

            search.FindPath(pathInfo, ref pathList, ref distance);

            return search.ClosestRegion.Value;
        }


      /*  public Dictionary<ushort, RegionPathFinderNodeBFS> GetBFSResult()
        {
            return searcher.GetBFSResult();
        }*/

        public ushort Start
        {
            get 
            {
                return searcher.Start;
            }
        }

        public ushort Destination
        {
            get
            {
                return searcher.Destination;
            }
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

        /// <summary>
        /// LoadPostProcess must be called AFTER (Dependent)RegionMap
        /// </summary>
        public int LoadPostProcessOrder
        {
            get
            {
                return 10;
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
            RegionMap rMap = (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(regionMapID);
            if (rMap.IDName == "ExposedHumanNormalFoot")
            {

            }

            foreach (RegionSearchRequestID searchRequestID in SearchRequests)
            {
                RegionSearchRequest searchRequest = LookUp<RegionSearchRequest, RegionSearchRequestID>.FindByID(searchRequestID);
                
                rMap.AssertSearch(this, searchRequest);
            }
#endif
            bool isFinished = false;

            if (IsBFS)
            {
                RegionSearcher.SearchStatus status = searcher.CycleBFS();
                if (status == RegionSearcher.SearchStatus.Finished)
                {  
                    isFinished = true;

                    RegionMap regionMap = (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(regionMapID);

                    regionMap.PlannerBFSFinished(this, searcher.GetBFSResult());

                  
                }
            }
            else 
            { 
                isFinished = true;

                float distance = 0f;
                RegionPath regionPath = null;
                // complete one full search each cycle...
                searcher.FindPath(RegionSearcher.PathInfo.FullPath, ref regionPath, ref distance);

                RegionMap regionMap = (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(regionMapID);
                regionMap.PlannerAStarFinished(this, regionPath);

            }

            if (isFinished)
            {                
                RemoveIDEntry(); // don't call destroy...

                return true; // this will unregister
            }
        
            return false; 
        }


        public void AddSearchRequest(RegionSearchRequest request)
        {
            SearchRequests.Add(request.ID);

#if DEBUG
           
         /*   RegionMap rMap = (RegionMap)LookUp<ICyclable, CyclableID>.FindByID(regionMapID);
           
            ushort fromRegion = rMap.GetRegion(request.FromSubtile.X, request.FromSubtile.Y).Color;
            SubtileSector fromSector = rMap.GetSector(request.FromSubtile);
           
           // rMap.getsec

             request.AddLog(string.Format("Added to planner, from: {0}, region: {1}, sector created on: {2}", request.FromSubtile, fromRegion, fromSector.CreatedOn));
            */
#endif
        }
      

        public bool UnregisterBeforeSnapshot
        {
            get
            {
                return false; // keep the planner registered. The search instance will also be kept, but will start over.
            }
        }

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);

            this.SearchRequests = (List<RegionSearchRequestID>)sn.DoList(SearchRequests);
            this.ID = sn.DoEnum(ID);
            this.IsPaused = sn.DoBool(IsPaused);
            this.searcher = (RegionSearcher)sn.DoISnapshot(searcher);
            this.regionMapID = sn.DoEnum(regionMapID);
            this.IsBFS = sn.DoBool(IsBFS);

            sn.Ignore(totalComputationAllInstancesInSeconds);
            sn.Ignore(ComputationTimeSpentInSeconds);
            sn.Ignore(StartedOnTimeInSeconds);

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

            if (searcher != null)
            {
                searcher.LoadPostProcess(sn);
            }

        }
    }
}
