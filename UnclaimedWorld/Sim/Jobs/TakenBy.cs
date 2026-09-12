using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// This class contains the list of workers on the (way to the) job.
    /// 
    /// remove this class...
    /// </summary>
    public class TakenBy : ISnapshot
    {
        private Job job;
        private JobID snapshotJobID;

        /// <summary>
        /// Don't use SortedList, it doesn't allow duplicate keys!
        /// </summary>
        private List<Entity> takenBy = new List<Entity>();
        private List<EntityID> takenByIDs = new List<EntityID>();


        public TakenBy()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");    
     
        }

        public TakenBy(Job job)
        {
            this.job = job;
        }


        public void Iterate(Action<Entity> function)
        {
            foreach (var taker in takenBy)
            {
                function(taker);
            }
        }

       

        private static int CompareCurrentUtility(Entity e1, Entity e2)
        {
            // recompute?
            Intelligence e1Intelligence = e1.Intelligence;
            Intelligence e2Intelligence = e2.Intelligence;

            e1Intelligence.GetScore();
            e2Intelligence.GetScore();

            if (e1Intelligence.GetCurrentGoalUtility() == null)
            {
                if (e2Intelligence.GetCurrentGoalUtility() == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                if (e2Intelligence.GetCurrentGoalUtility() == null)
                {
                    return 1;
                }
                else if (e1Intelligence.GetCurrentGoalUtility() > e2Intelligence.GetCurrentGoalUtility())
                {
                    return 1;
                }
                else if (e1Intelligence.GetCurrentGoalUtility() == e2Intelligence.GetCurrentGoalUtility())
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
        }


        /// <summary>
        /// call Job.Abandon() instead!
        /// </summary>
        /// <param name="entity"></param>
        public bool TryRemove(Entity entity)
        {
            /*  if (entity.ToString().Contains("Tarkov"))
              {
                  throw new Exception();
              }*/


            if (takenBy.Contains(entity))
            {                
                Remove(entity);

                return true;
            }

            return false;
        }

        public void RemoveAll()
        {
            if (job.ID == (JobID)3288)
            {

            }

            while (takenBy.Count != 0)
            {
                Remove(takenBy[0]);
            }
        }

        public void Remove(Entity entity)
        {
            if (job.ID == (JobID)3288)
            {

            }

            takenBy.RemoveAll(e => e == entity); // remove all appearances as a precaution against signing up more than once...

            ProcessJob pJob = job as ProcessJob;
        
           /* if (pJob != null)
            {
                // recompute efficiency
                pJob.AverageLaborEfficiency = (float)ProcessJob.GetAverageLaborReturn(takenBy.Count, job.MaxJobPositions);
            }*/
        }

        public bool Contains(Entity entity)
        {
            return takenBy.Contains(entity);
        }

        public bool Contains(EntityID entityID)
        {
            return takenBy.Exists(e => e.EntityID == entityID);
        }

        public int Count
        {
            get
            {
                return takenBy.Count;
            }
        }

        public void Add(Entity entity)
        {           
            if (job.ID == (JobID)3288)
            {

            }

            takenBy.Add(entity);

         //   ProcessJob pJob = job as ProcessJob;          
          /*  if (pJob != null)
            {
                // recompute efficiency
                pJob.AverageLaborEfficiency = (float)ProcessJob.GetAverageLaborReturn(takenBy.Count, job.MaxJobPositions);
            }*/
            // sort here?
        }

        public Entity GetLowestScorer()
        {
            takenBy.Sort(CompareCurrentUtility);
            return takenBy[0];
        }

        public List<Entity> GetSortedByDistance(Vector3 location)
        {
            return takenBy.OrderByDescending(e => e.IsOnPlaySite()? Common.DistanceOctile(e.PlaySiteLocation, location): float.MaxValue).ToList();
          
        }

        public Entity Get(int index)
        {
            return takenBy[index];
        }

        public Entity Get(Func<Entity, bool> matches)
        {
            return takenBy.FirstOrDefault(matches);
        }

        public bool IsScoreGreaterThanAnyTaker(double utilityToTest)
        {
            // TODO: introduce a waiting period before starting a goal. During that period, anyone with a higher score may take the job. Outside it, apply the required difference as now.

            // sort, then we only have to consider the lowest score.
            //takenBy.Sort(CompareCurrentUtility);

            if (takenBy.Count > 0)
            {
                // recompute the scores if necessary:
                Entity entity;
                for (int i = 0; i < takenBy.Count; i++)
                {
                    entity = takenBy[i];

                    if (IsScoreGreater(utilityToTest, entity))
                    {
                        return true;
                    }

                }

                return false;
            }
            else return true;
        }

        public static bool IsScoreGreater(double utilityToTest, Entity entity)
        {
            if (entity.Intelligence.GetScore() < utilityToTest)
            {
                return true;
            }
            else return false;
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
            if (sn.mode != Snapshotter.Mode.Load)
            {
                this.takenByIDs = takenBy.Select(e => e.EntityID).ToList();
            }

            this.takenByIDs = (List<EntityID>)sn.DoList(takenByIDs);
            this.snapshotJobID = (JobID)sn.SnapshotID<Job, JobID>(job);

            sn.Ignore(takenBy);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            // reconnect after load:
            takenBy = takenByIDs.Select(c => LookUp<Entity, EntityID>.FindByID(c)).ToList();

            job = LookUp<Job, JobID>.FindByID(snapshotJobID);
                       
            takenByIDs.Clear();
        }

        #endregion
    }
}
