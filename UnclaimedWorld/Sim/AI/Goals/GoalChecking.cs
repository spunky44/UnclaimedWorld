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
    
    class GoalChecking: CompositeGoal, ITopLevelGoal
    {
       
        private Vector3 locationOfScoutPoint;

        private CheckProcessJob job;
        JobID? snapshotJob;

       
      //  private Vector3? locationInSearchArea;

        public double TimeSpentInTopLevelGoal { get; set; }

        public GoalChecking(Entity owner, CheckProcessJob job, List<EntityGroupID> ownersVehicles)
            : base(owner)
        {
            this.job = job;
            this.ownersOfVehicles = ownersVehicles;

           // this.locationInSearchArea = locationInSearchArea;
        }



        public GoalChecking()
        {
        }

       
        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            job.TakeJob(entity);

           // Vector3? jobCenter = job.Location ?? locationInSearchArea;

            List<ItemType.TaskType> gearTasks = null;
           /* if (job.Examine)
            {
                gearTasks = new List<ItemType.TaskType>() { ItemType.TaskType.Examining };
            }
            else
            {
                gearTasks = new List<ItemType.TaskType>() { ItemType.TaskType.Scouting };
            }*/
           
            AddNightActivityGear(ref gearTasks);

            FindOptionalEquipmentIfNeeded(job.Location, job, taskTypesForGear: gearTasks);
                       
            locationOfScoutPoint = (Vector3)job.Location;
            AddSubgoal(new GoalMoveToPosition(entity, locationOfScoutPoint, ownersOfVehicles, GoalMoveToPosition.VehicleUse.FreeUpAfterUse) { IsFinalDestination = true });

            locationOfScoutPoint = entityIntelligence.CurrentExpedition.Center.Value;
                              
        }




        public double ScoreGoal()
        {           
            return ScoreJobGoal(job);
        }

       /* protected override bool ArePreconditionsOK()
        {
          

            return true;
        }*/

        public override string GetStatus()
        {
            return "Checking progress";
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
           
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
            }
        }



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
                job = (CheckProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
            }
        }


        #endregion
        
    }
}
