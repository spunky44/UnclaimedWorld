using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Jobs.JobTypes;
namespace UWGame.SimSide.Jobs
{
    public enum Priority { Normal, High, Low }


    public enum JobID : ulong
    {
        Invalid = uint.MaxValue,
        Max = Invalid,
        First = 1
    }

    public abstract class Job : ISnapshot, ILookUp<Job, JobID>
    {
        // LOG: FOR DEBUG ONLY
        public List<Tuple<double, string>> Log = new List<Tuple<double, string>>();

       // public int MaxJobPositions = 1;

        /// <summary>
        /// for non-work process jobs, the list will be empty after job start.
        /// </summary>
        public TakenBy TakenBy;

        /// <summary>
        /// Does not include inertia
        /// </summary>
        public double DebugScore;


        public double DebugScoreNoTools;


        /// <summary>
        /// we keep an ID so we can remove the job 
        /// </summary>
        public EntityGroupID EntityGroupID;

        public double Timestamp;
        public virtual Priority Priority
        {
            get;
            set;
        }


        // private bool jobTypeIsDirty = true;
        protected JobType jobType;


      
        public Job(EntityGroup entityGroup, //Priority priority, 
            bool addToJobsGroupNow = true) // hacky...
        {
            EntityGroupID = entityGroup.ID;

            AddToLookup();

            if (addToJobsGroupNow)
            {               
                entityGroup.AddJob(this);
            }

                     
            Timestamp = The.Sim.TotalUnPausedGameTime.TotalMilliseconds;
           
            TakenBy = new TakenBy(this);

        
        
           // SetDefaultPriority(entityGroup);
                
            AddLog("Created");

        }

        /// <summary>
        /// to be called from subclass ctor!
        /// </summary>
        /// <param name="entityGroup"></param>
        protected void SetDefaultPriority(EntityGroup entityGroup)
        {
            JobType jobType = GetJobType();
            if (jobType != null)
            {
                // Set default priority
                Priority = entityGroup.Policy.GetPriority(jobType);
            }
        }

        public Job( /*parameterless*/)
        {
        }

   
        public static string GetPriorityAsString(Priority priority)
        {
            switch(priority)
            {
                case Priority.Low:
                    return "LOW";

                case Priority.Normal:
                    return "NORMAL";

                case Priority.High:
                    return "HIGH";                    

            }

            return null;
        }

       #region ILookup

       private JobID id = JobID.Invalid;
       static JobID IDCounter = JobID.First;

       public JobID ID
       {
           get
           {
               return id;
           }

           private set
           {            
               if (value == JobID.Invalid)
               {
                                  
               }

               id = value;
           }
       }

       public JobID GetUniqueID()
       {
           IDCounter++;
           if (IDCounter >= JobID.Max)
           {
               throw new Exception("Astounding, JobID just exceeded 64 bits. Something seriously wrong has happened.");
           }

           return IDCounter;
       }

       public JobID SnapshotID(Snapshotter sn, JobID id)
       {
           return (JobID)sn.DoEnum(id);
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

           if (ID == (JobID)423)
           {

           }

           if (ID != JobID.Invalid)
               LookUp<Job, JobID>.Add(ID, this);
       }

       public void SetInvalid()
       {
           id = JobID.Invalid;
       }

       public void RemoveIDEntry()
       {
           // LookUp<Entity, EntityID>.Remove(ID);
           LookUp<Job, JobID>.Remove(this);
       }

       void UWGame.SimSide.Snapshots.ILookUp<Job, JobID>.ResetIDCounter() // interface method - does nothing...
       {
       }

       public static void ResetIDCounter() // called by invoke, do not remove
       {
           IDCounter = JobID.First;
       }

       void ILookUp<Job, JobID>.CreateLookupCollection() // interface method - does nothing...
       {
       }

       public static void CreateLookupCollection()
       {
           LookUp<Job, JobID>.Create();
       }

       #endregion

       #region ISnapshot

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

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            IDCounter = (JobID)sn.DoEnum(IDCounter);
            id = SnapshotID(sn, id);

            this.Priority = (Priority)sn.DoEnum(Priority);
            this.TakenBy = (TakenBy)sn.DoISnapshot(TakenBy);
            this.Timestamp = sn.DoDouble(Timestamp);

            this.EntityGroupID = (EntityGroupID)sn.DoEnum(EntityGroupID);
            this.jobType = sn.DoGameData(jobType);

            Log = sn.DoList(Log);


            sn.Ignore(DebugScore);
            sn.Ignore(DebugScoreNoTools);

          //  sn.Ignore(Log);

            return this;
        }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);
            
            TakenBy.LoadPostProcess(sn);
        }

        #endregion

        public void AddLog(string text)
        {

#if DEBUG || PROFILE
                        
            Log.Add(new Tuple<double, string>(The.Sim.TotalUnPausedGameTimeInSeconds, text));
#endif
        }

        public virtual int MaxJobPositions
        {
            get { return 1; }
        }

       
        public virtual void TakeJob(Entity entity)
        {
            System.Diagnostics.Debug.Assert(ID != JobID.Invalid, "Invalid ID!!");


            if (TakenBy.Contains(entity))
            {
                return; // added as a precaution...
            }
            else
            {
                #if DEBUG || PROFILE
                string logText = GetTakenByText(entity);
              /*  bool previousActionExists = Log.Exists(l => l.Item1 == The.Sim.TotalUnPausedGameTimeInSeconds && l.Item2 == logText);

                System.Diagnostics.Debug.Assert(previousActionExists == false, "Repeated action? Why...");
                */

                AddLog(logText);

                #endif

                TakenBy.Add(entity);

            }
        }

        protected virtual void ComputeJobType()
        {
            // allow designer process tags to override static process job types:
            
            foreach (var item in GameData.Instance.AllJobTypes)
            {
                if (item.Value.IsType(this))
                {
                    jobType = item.Value;
                    break;
                }
            }
        }

        public JobType GetJobType()
        {
            return jobType;
        }

        private string GetAbandonedByText(Entity entity)
        {
            string text = "Abandoned by: " + entity.ToString();
            return text;
        }

        private string GetTakenByText(Entity entity)
        {
            string logText = "Taken by: " + entity.ToString();
            return logText;
        }

        public double? GetTakerScore()
        {
            if (TakenBy.Count > 0)
            {
                return TakenBy.Get(0).Intelligence.Brain.ScoreTopLevelGoal();
            }

            return null;
        }

        public virtual void Abandon(Entity entity, 
            bool isDestroyingJob = false) // fo debugging only
        {

            if (TakenBy.TryRemove(entity))
            {
#if DEBUG || PROFILE
                string text = GetAbandonedByText(entity);

                /*
                string takenByText = GetTakenByText(entity);
                bool previousActionExists = Log.Exists(l => l.Item1 == The.Sim.TotalUnPausedGameTimeInSeconds && l.Item2 == takenByText);
                System.Diagnostics.Debug.Assert(previousActionExists == false, "Abandoning job in same frame? Why...");
            */
              /*  bool previousActionExists = Log.Exists(l => l.Item1 == The.Sim.TotalUnPausedGameTimeInSeconds && l.Item2 == text);

                System.Diagnostics.Debug.Assert(previousActionExists == false, "Repeated action? Why...");
                */
                if (!isDestroyingJob)
                {
                    foreach (var item in entity.Intelligence.Brain.Subgoals)
                    {
                        GoalHaul goalHaul = item as GoalHaul;
                        if (goalHaul != null)
                        {
                            if (goalHaul.job != null)
                            {
                                if (goalHaul.job.TakenBy.Contains(entity)) // only if the top goal is still taken/active...
                                {
                                    // assert that we are not abandoning a nested job - but this is alright if we are destroying it also!
                                    goalHaul.AssertAllSubgoalJobsTaken(); // 
                                }
                            }
                        }
                    }
                }

                AddLog(text);
#endif
            }
        }

       

        

        public virtual bool RequiresBoldStance
        {
            get { return false; }
        }

        /// <summary>
        /// don't cancel takers when finished..?
        /// </summary>
        /// <param name="cancelTakers"></param>
        public virtual void Destroy(bool cancelTakers, Entity entityToExclude = null)
        {
            if (ID == JobID.Invalid)
                return;

            AddLog("Destroyed");

           
            

            /// When we cancel takers, their goals must be removed immediately. Snapshotting them will cause a crash... but cleanup is now handled in SendMessage.
            if (cancelTakers)
            {
                CancelAllTakers(entityToExclude);
            }

            EntityGroup entityGroup;
            if (ResolveOwner(out entityGroup))
            {
                entityGroup.RemoveJob(this);             
            }
           
                        
            The.Client.DestroyAccessibility(ID);



            RemoveIDEntry();

            
        }

        public void CancelAllTakers(Entity entityToExclude)
        {
            for (int i = TakenBy.Count - 1; i >= 0; i--) // loop in reverse, because members will remove themselves from this list as we go...
            {
                Entity jobTaker = TakenBy.Get(i);

                if (jobTaker != entityToExclude)
                {
                    AddLog("Sent Cancel Job message to: " + jobTaker.ToString());
                    jobTaker.SendMessage(new Message(null, Message.MessageTypes.CancelJobOrItemInUse, Message.CancelJobKeepVehicle.KeepVehicle));
                }
            }
        }

        public bool ResolveOwner(out EntityGroup owner)
        {
            owner = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);

            /*if (owner == null) // unlikely to ever be null?
            {
                HandleDestroyedJobOwner();
            }*/

            return owner != null; 
        }

       
       
        public virtual bool UserCanCancel(out string tooltip)
        {
            tooltip = "";
            return true;
        }

        public Entity GetAssignedWorker()
        {
            if (TakenBy.Count > 0)
            {
                return TakenBy.Get(0);  //"ASSIGNED TO: " + TakenBy.Get(0).Name; //MP changed from Worker:
            }
            else
            {
                return null; // "Worker not assigned";
            }
        }

       /* public virtual string GetNoCancelOptionText()
        {
            return "Cannot cancel this task.";
        }*/

        public virtual void GetLocation(out Point? tilePos, out EntityID? targetEntity, out ZoneID? zoneID)
        {
            tilePos = null;
            targetEntity = null;
            zoneID = null;
        }


        public abstract Vector3? GetCircaLocation();
        
       
        public virtual string GetName()
        {
            return "";
        }

        /*   public virtual AI.Goals.GoalEvaluator.CalculateResult ScoreThisJob(RegionMap regionMap, Entity entity, Jobs.Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating, double? fitnessScore = null)
           {


           }*/


        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public List<Entity> GetTakersSortedByDistance()
        {
            Vector3? jobLocation = GetCircaLocation();

            if (jobLocation.HasValue)
            {
                return TakenBy.GetSortedByDistance(jobLocation.Value);
            }
            else
            {
                List<Entity> takers = new List<Entity>();
                TakenBy.Iterate(e => takers.Add(e));
                return takers;
            }

           // ((ProcessJob)job).GetCurrentJobLocation(out jobLocation);
           
        }
    }

  
}
