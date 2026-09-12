using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.AI.Pathfinding;
using System.Linq;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.IngameEvents;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Systems.TimeSlicing
{
    public class CycleManager: ISnapshot
    {
        private List<ICyclable> highPriorityCycleUpdateRequests = new List<ICyclable>();
        private List<CyclableID> highPriorityCycleUpdateRequestIDs = new List<CyclableID>();

        private List<ICyclable> mediumPriorityCycleUpdateRequests = new List<ICyclable>();
        private List<CyclableID> mediumPriorityCycleUpdateRequestIDs = new List<CyclableID>();


        /// <summary>
        ///   1 frame, or update, takes 0.016 s
        /// </summary>
      //  public double TimeAllocatedInSeconds = 0.0045; //0.0005; to debug pathfinder

        HighResolutionTime timer;

        public CycleManager()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                timer = new HighResolutionTime();
            }
        }

        double timeTaken = 0;

        // call from Update()!!
        public void Update()
        {            
            timer.Start(); // starts at 0

            timeTaken = 0;

            CycleRequests(highPriorityCycleUpdateRequests, ref nextHighPriorityCyclable, timer); //, TimeAllocatedInSeconds);
            // don't process paths unless the movement maps have finished updating:
            CycleRequests(mediumPriorityCycleUpdateRequests, ref nextMediumPriorityCyclable, timer); //, TimeAllocatedInSeconds);


            TotalComputation += timeTaken;

        }

        /// <summary>
        /// prevent starvation by storing the cyclable that was next in line
        /// </summary>
        private CyclableID? nextHighPriorityCyclable, nextMediumPriorityCyclable;

        public double TotalComputation;

        private void CycleRequests(List<ICyclable> requests, ref CyclableID? nextCyclable, HighResolutionTime timer) //, double TimeAllocatedInSeconds)
        {
            if (timeTaken > GameData.Instance.AIConstants.TimeAllocatedInSecondsForTimeSlicedSystems) //TimeAllocatedInSeconds)
                return;


            double singleTimeTaken = 0;

            int startIndex = requests.Count - 1;

            if (nextCyclable.HasValue)
            {
                CyclableID nextCyclableValue = nextCyclable.Value;
                int lastIndex = requests.FindIndex(c => c.ID == nextCyclableValue);
                if (lastIndex >= 0)
                {
                    startIndex = lastIndex;
                }
            }

            nextCyclable = null;

            List<ICyclable> requestsForIterating = new List<ICyclable>();
            requestsForIterating.AddRange(requests);

            while (requestsForIterating.Count > 0 && timeTaken < GameData.Instance.AIConstants.TimeAllocatedInSecondsForTimeSlicedSystems)
            {
                ICyclable cyclable;

                for (int i = startIndex; i >= 0; i--)
                {
                    cyclable = requestsForIterating[i];

                    if (!cyclable.IsPaused)
                    {                       
                        bool result = cyclable.CycleOnce();

                        singleTimeTaken = timer.GetTime() - timeTaken;                       

                        timeTaken += singleTimeTaken;

                        cyclable.TotalComputationAllInstancesInSeconds += singleTimeTaken;
                        cyclable.ComputationTimeSpentInSeconds += singleTimeTaken;
                        
                      /*  cyclable.StartedOnTimeInSeconds += singleTimeTaken;

                        if ( cyclable.StartedOnTimeInSeconds > 3)
                        {

                        }*/

                        // the game can have ended here, and Sim may be destroyed!
                        if (The.Sim != null && The.Sim.TotalUnPausedGameTimeInSeconds - cyclable.StartedOnTimeInSeconds >= 5)
                        {

                        }

                        if (result)
                        {
                           
                            //requests.Remove(cyclable); // unregister
                            UnRegister(cyclable); // unregister

                            requestsForIterating.Remove(cyclable);
                        }

                        if (The.Sim == null ||
                            The.Sim.IsGameOver)
                        {   // events may shut down the game...
                            return;
                        }

                        if (timeTaken >= GameData.Instance.AIConstants.TimeAllocatedInSecondsForTimeSlicedSystems) // TimeAllocatedInSeconds)
                        {
                            // prevent starvation by storing counter/index
                            int nextIndex = i - 1;
                            if (nextIndex >= 0 && nextIndex < requests.Count)
                            {
                                nextCyclable = requestsForIterating[nextIndex].ID;
                            }

                            return;
                        }
                    }
                    else
                    {                                     
                        requestsForIterating.Remove(cyclable); // keep it registered
                    }
                }

                startIndex = requestsForIterating.Count - 1;
            }
        }

        public void PrintPerformance(StringBuilder text)
        {
            text.AppendLine("Unpaused time (s): " + The.Sim.TotalUnPausedGameTimeInSeconds);            
            text.AppendLine("Total computation time (s): " + Common.DecimalToString(TotalComputation));
            text.AppendLine(string.Format("Usage of allocated time ({0} s): {1}", Common.DecimalToString(GameData.Instance.AIConstants.TimeAllocatedInSecondsForTimeSlicedSystems), Common.PercentageToString(timeTaken / GameData.Instance.AIConstants.TimeAllocatedInSecondsForTimeSlicedSystems)));

            text.AppendLine("Waiting agents: " + The.Sim.WaitingAgents.Count);

            text.AppendLine("Average agent waiting time (s): " + Common.DecimalToString(The.Sim.WaitingAgents.Count == 0? 0d : The.Sim.WaitingAgents.Average(a => The.Sim.TotalUnPausedGameTimeInSeconds - a.Value.Item2)));

            text.AppendLine("High priority cyclables (movement maps): " + highPriorityCycleUpdateRequests.Count);
            text.AppendLine("Medium priority cyclables (searches): " + mediumPriorityCycleUpdateRequests.Count);
            
            text.AppendLine("");
           

            PrintCyclableClassInfo(text, "EventManager", EventManager.totalComputationAllInstancesInSeconds);
            PrintCyclableClassInfo(text, "MovementMap", MovementMap.totalComputationAllInstancesInSeconds);
            PrintCyclableClassInfo(text, "PathPlanner", PathPlanner.totalComputationAllInstancesInSeconds);
            PrintCyclableClassInfo(text, "RegionSearchPlanner", RegionSearchPlanner.totalComputationAllInstancesInSeconds);
            PrintCyclableClassInfo(text, "HaulingJobManager", HaulingJobManager.totalComputationAllInstancesInSeconds);
            PrintCyclableClassInfo(text, "Terrain region map", RegionMap.totalComputationAllInstancesInSeconds);
            text.AppendLine("");

            text.AppendLine("High prio cyclables:");

            foreach (var item in highPriorityCycleUpdateRequests)
            {
                PrintCyclable(text, item);
                /*
                MovementMap moveMap = item as MovementMap;
                if (moveMap != null)
                {
                    moveMap.PrintInfo(text);
                }*/
            }

            text.AppendLine("");
            text.AppendLine("Other cyclables:");

            foreach (var item in mediumPriorityCycleUpdateRequests)
            {
                PrintCyclable(text, item);
               
                /*
                RegionSearchPlanner regionSearchPlanner = item as RegionSearchPlanner;
                if (regionSearchPlanner != null)
                {
                    regionSearchPlanner.PrintInfo(text);
                }
                else
                {
                    PathPlanner pathPlanner = item as PathPlanner;
                    if (pathPlanner != null)
                    {
                        pathPlanner.PrintInfo(text);
                    }

                }*/
                
            }

            text.AppendLine("");


            text.AppendLine("Waiting agents:");

            List<EntityID> removes = null;
            var orderedList = The.Sim.WaitingAgents.OrderByDescending(a => The.Sim.TotalUnPausedGameTimeInSeconds - a.Value.Item2);
            foreach (var item in orderedList)
            {
                Entity entity = Entity.FindByID(item.Key);
                if (entity != null)
                {
                    text.AppendLine(string.Format("{0} ({1}) {2} waiting for {3} : {4} s", entity.GetDisplayName(), entity.ID, MapManager.WorldPosToSubtile(entity.PlaySiteLocation), item.Value.Item1, Common.DecimalToString(The.Sim.TotalUnPausedGameTimeInSeconds - item.Value.Item2)));
                }
                else
                {
                    Common.AddToList(ref removes, item.Key);
                }

            }

            if (removes != null)
            {
                foreach (var item in removes)
                {
                    The.Sim.WaitingAgents.Remove(item);
                }
            }
        }

        private void PrintCyclable(StringBuilder text, ICyclable item)
        {
            double timeWaiting = The.Sim.TotalUnPausedGameTimeInSeconds - item.StartedOnTimeInSeconds;
            item.PrintInfo(text);
            text.Append(string.Format(" {0} s,", Common.DecimalToString(timeWaiting)));         
            text.Append(" (");
            text.Append(SecondsAsMilliseconds(item.ComputationTimeSpentInSeconds));
            text.Append(" ms)");

            if (item.UpdateInterval.HasValue && item.UpdateInterval.Value < timeWaiting)
            {
                text.Append("!!!");
            }

            text.AppendLine();
        }

        private string SecondsAsMilliseconds(double seconds)
        {
            return string.Format("{0}", (int)(seconds * 1000d));

        }

        public void PrintCyclableClassInfo(StringBuilder description,  string name, double totalComputation /* ICyclable cyclable*/)
        {
            description.AppendLine(string.Format("{0}: {1} ({2})", 
                name,
                Common.DecimalToString(totalComputation),
                Common.PercentageToString(totalComputation / TotalComputation)));             

        }

        /*

        private void CycleRequests(List<ICyclable> requests, ref double timeTaken)
        {            
            int noOfCyclesRemaining;
            double singleTimeTaken = 0;

          
            while(requests.Count > 0 && timeTaken < TimeAllocatedInSeconds)
            {
                // do a few cycles between checking the time...
                noOfCyclesRemaining = 5; //noOfSearchCyclesPerUpdate;

                ICyclable cyclable;

                while (noOfCyclesRemaining > 0 && requests.Count > 0)
                {
                    for (int i = requests.Count - 1; i >= 0; i--)
                    {
                        cyclable = requests[i];

                        if (!cyclable.IsPaused)
                        {
                            //newTimeTaken = The.Sim.GameTime.TotalGameTime.TotalSeconds;

                            HighResolutionTime.Start();

                            bool result = cyclable.CycleOnce();

                            singleTimeTaken = HighResolutionTime.GetTime();
                            //newTimeTaken = The.Sim.GameTime.TotalGameTime.TotalSeconds - newTimeTaken;

                            timeTaken += singleTimeTaken;

                            if (result)
                            {
                                requests.Remove(cyclable);
                            }

                            if (The.Sim == null ||
                                The.Sim.IsGameOver)
                            {   // events may shut down the game...
                                return;
                            }
                        }
                        else
                        {
                            requests.Remove(cyclable);
                        }

                    }
                    noOfCyclesRemaining--;
                }
              //  timeTaken = HighResolutionTime.GetTime();

            }
            
        }*/

        public bool IsRegistered(ICyclable request)
        {
            return mediumPriorityCycleUpdateRequests.Contains(request) 
                || highPriorityCycleUpdateRequests.Contains(request);
        }

        public enum Priority { Medium, High }
        public void Register(ICyclable request, Priority priority)
        {
            request.StartedOnTimeInSeconds = The.Sim.TotalUnPausedGameTimeInSeconds; // 0;
            request.ComputationTimeSpentInSeconds = 0d;

            if (request is UWGame.SimSide.Maps.Regions.DependentRegionMap)
            {

            }

            if (priority == Priority.High)
            {
                highPriorityCycleUpdateRequests.Add(request);
            }
            else if (priority == Priority.Medium)
            {
                mediumPriorityCycleUpdateRequests.Add(request);
            }
        }

        public void UnRegister(ICyclable request)
        {
            mediumPriorityCycleUpdateRequests.Remove(request);
            highPriorityCycleUpdateRequests.Remove(request);
        }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            // the clients are snapshotted somewhere else. Here we only snapshot the IDs.

            // unregister the clients that do not snapshot their progress:
            if (sn.mode != Snapshotter.Mode.CRC)
            {
                UnregisterBeforeSnapshot(highPriorityCycleUpdateRequests);
                UnregisterBeforeSnapshot(mediumPriorityCycleUpdateRequests);               
            }
                   
            // get the CyclableID from each instance, store that. After load, reconnect via Cyclable.AllCyclables            
            mediumPriorityCycleUpdateRequestIDs = /*(List<CyclableID>)*/mediumPriorityCycleUpdateRequests.Select(c => c.ID).ToList();
            mediumPriorityCycleUpdateRequestIDs = (List<CyclableID>)sn.DoList(mediumPriorityCycleUpdateRequestIDs);

            highPriorityCycleUpdateRequestIDs = highPriorityCycleUpdateRequests.Select(c => c.ID).ToList();
            highPriorityCycleUpdateRequestIDs = (List<CyclableID>)sn.DoList(highPriorityCycleUpdateRequestIDs);

           // double TimeAllocatedInSeconds = 0;
          //  TimeAllocatedInSeconds = sn.DoDouble(TimeAllocatedInSeconds); // #MIGRATE


            sn.Ignore(timeTaken);
            sn.Ignore(TotalComputation);
            sn.Ignore(highPriorityCycleUpdateRequests);
            sn.Ignore(mediumPriorityCycleUpdateRequests);
            sn.Ignore(nextHighPriorityCyclable);
            sn.Ignore(nextMediumPriorityCyclable);
            sn.Ignore(timer);

            return this;
        }


        private void UnregisterBeforeSnapshot(List<ICyclable> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ICyclable cyclable = list[i];
                if (cyclable.UnregisterBeforeSnapshot)
                {
                    UnRegister(cyclable);
                }
            }
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

            // reconnect after load:
            mediumPriorityCycleUpdateRequests = /*(List<ICyclable>)*/mediumPriorityCycleUpdateRequestIDs.Select(c => LookUp<ICyclable, CyclableID>.FindByID(c)).ToList();
            highPriorityCycleUpdateRequests = /*(List<ICyclable>)*/highPriorityCycleUpdateRequestIDs.Select(c => LookUp<ICyclable, CyclableID>.FindByID(c)).ToList();

            timer = new HighResolutionTime();
        }
    }
}
