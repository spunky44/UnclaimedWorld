using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    
    class GoalScouting: CompositeGoal, ITopLevelGoal
    {
       
        private Vector3 locationOfScoutPoint;

        private ScoutingJob job;
        JobID? snapshotJob;

        /// <summary>
        /// only filled when searching an area:
        /// </summary>
        private Vector3? locationInSearchArea;

        public double TimeSpentInTopLevelGoal { get; set; }

        public GoalScouting(Entity owner, ScoutingJob job, List<EntityGroupID> ownersVehicles, Vector3? locationInSearchArea)
            : base(owner)
        {
            this.job = job;
            this.ownersOfVehicles = ownersVehicles;

            this.locationInSearchArea = locationInSearchArea;
        }

      

        public GoalScouting()
        {
        }

       
        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            job.TakeJob(entity);

            Vector3? jobCenter = job.Location ?? locationInSearchArea;

            List<ItemType.TaskType> gearTasks;
            if (job.Examine)
            {
                gearTasks = new List<ItemType.TaskType>() { ItemType.TaskType.Examining };
            }
            else
            {
                gearTasks = new List<ItemType.TaskType>() { ItemType.TaskType.Scouting };
            }
           
            AddNightActivityGear(ref gearTasks);

            FindOptionalEquipmentIfNeeded(jobCenter.Value, job, taskTypesForGear: gearTasks);

            if (Common.DistanceOctile(jobCenter.Value, entity.PlaySiteLocation) > 1500) // 30 tiles
            {

            }

            if (job.Location.HasValue)
            {             
                // add subgoal SearchArea if job.MapArea is set, else GoalMove
              
                locationOfScoutPoint = (Vector3)job.Location;
                AddSubgoal(new GoalMoveToPosition(entity, locationOfScoutPoint, ownersOfVehicles, GoalMoveToPosition.VehicleUse.FreeUpAfterUse) { IsFinalDestination = true });

                locationOfScoutPoint = entityIntelligence.CurrentExpedition.Center.Value;
            }
            else
            {                               
                // move to edge first, like in FindPrey (evaluator and goal should match!)
                AddSubgoal(new GoalMoveToPosition(entity, locationInSearchArea.Value, ownersOfVehicles));
                AddSubgoal(new GoalSearchArea(entity, job.Zone, ownersOfVehicles, false, job.Examine));
            }                       
        }




        public double ScoreGoal()
        {

            // ScoreJobGoal(, null, null, null, null);

            //double currentRating;
            //RegionMap regionMapToUse = EvaluateScoutingJobs.GetRegionMapToUseForEntity(entity);
            //int proposedNumberOfWorkers = Common.Clamp(job.TakenBy.Count + 1, 0, job.MaxJobPositions);

            //(GoalEvaluator as EvaluateScoutingJobs).ScoreThisJob(regionMapToUse, entity, job, proposedNumberOfWorkers, ageContribution, timeContribution, out currentRating, null, null, null);
            if (entity.Name != null && entity.Name.Contains("onlan")) // (this as GoalAttack != null))
            {
                int i = 0;
            }

            //return GetCurrentGoalScore();
            return ScoreJobGoal(job);
        }

        protected override bool ArePreconditionsOK()
        {
            /*if (job. entity.PersonEntity.CurrentExpedition.Center != locationOfExpedition)
            {
                return false;
            }     */        

            return true;
        }

        public override string GetStatus()
        {
            return "Scouting";
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            if (entity.Name != null && entity.Name.Contains("onlan")) // (this as GoalAttack != null))
            {
                int i = 0;
            }


            if (!ArePreconditionsOK())
            {
                Status = Goals.Status.Failed;
            }
            else
            {
                //process the subgoals
                Status = ProcessSubgoals(elapsed);
            }

            if (Status == Goals.Status.Completed)
            {

                if (job.Location.HasValue)
                {
                    if (Common.DistanceOctile(entity.PlaySiteLocation, (Vector3)job.Location) < 10f)
                    {
                        // very important!            
                        DestroyJobAndRemoveLocks(ref job);
                        Status = Goals.Status.Completed;
                    }
                    else
                    {
                        Status = Goals.Status.Failed;
                    }
                }
                else
                {
                 
                    DestroyJobAndRemoveLocks(ref job);

                    Status = Goals.Status.Completed;
                }
            }

        }

        public override bool IsSame(Jobs.Job job)
        {
            return job == this.job;           
        }

       

        public override void Deactivate()
        {
            if (job != null)
            {
                RemoveLocksFromJob(job);
                //job.Abandon(entity);

                //UnAssignOptionalEquipment(job);
            }
        }


        /*
        public override Goal.DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
        {
            // TOD: subgoal SearchArea should decide
            if (!requiresExamineAction || job.Examine)
            {
                return DetectionFactor.DetectGood;
            }
            else return DetectionFactor.CannotDetect;
        }

        public override Goal.DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
        {
            // TOD: subgoal SearchArea should decide
            if (!requiresExamineAction || job.Examine)
            {
                return DetectionFactor.DetectGood;
            }
            else return DetectionFactor.CannotDetect;
        }
        */


        public override bool HandleMessage(Message message)
        {
            //first, pass the message down the goal hierarchy
            bool handled = ForwardMessageToFrontMostSubgoal(message);

            //if the msg was not handled, test to see if this goal can handle it
            if (handled == false)
            {
                switch (message.MessageType)
                {
                    // someone wants us to stop doing this job:
                    case Message.MessageTypes.CancelJobOrItemInUse:
                    case Message.MessageTypes.CancelJobForAIReset:

                        Status = Status.Failed;
                      
                        job.Abandon(entity);

                        return true; //msg handled

                    default: return false;
                }
            }
            else
            {
                return true;
            }
        }


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.snapshotJob = sn.SnapshotID<Job, JobID>(job);
            this.locationInSearchArea = sn.DoVector3Nullable(locationInSearchArea);
            this.locationOfScoutPoint = sn.DoVector3(locationOfScoutPoint);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);


            sn.Ignore(job);

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            if (snapshotJob != null)
            {
                job = (ScoutingJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }
        }


        #endregion
        
    }
}
