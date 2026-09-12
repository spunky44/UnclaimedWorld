using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// Take a leisurely walk in the company of others...
    /// </summary>
    class GoalLeisureWalk: CompositeGoal, ITopLevelGoal
    {
        double timeToRest;
        private GroupMoveActivity activity;

        private double timeWaited = 0;
        private const double maxTimeToWait = 6; // seconds

        public int minimumMembers = 2;// 14; //2

        bool hasStarted = false;
        public double TimeSpentInTopLevelGoal { get; set; }

        public GoalLeisureWalk(Entity owner, GroupMoveActivity activity) 
            : base(owner)
        {
            //this.timeToRest = timeToRest;
            this.activity = activity;
        }


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
            throw new Exception("THIS CLASS IS OBSOLETE");
            return this;
        }

        public GoalLeisureWalk()
        {
        }

        public double ScoreGoal()
        {
            return entityIntelligence.GetCurrentGoalUtility().Value;
        }

        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            if (activity == null)
            {
                activity = new LeisureWalkActivity(personEntity.Household.OwnedEntities.Activities, new Vector3(90, 40, 0));
            }

            activity.AddMember(entity);
            //activity.Members.Add(entity);
           
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
         /*   ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                // if enough people have joined in, then we can start:
                if (activity.Members.Count >= minimumMembers)
                {
                    if (!activity.HasStarted)
                    {
                        // become leader
                        if (activity.Leader == null)
                        {
                            activity.Leader = entity;
                        }

                        activity.HasStarted = true;

                    }

                    if (!hasStarted)
                    {
                        // get a path and move out!
                        GoalMoveToPosition move = new GoalMoveToPosition(entity, activity.Destination, null, GoalMoveToPosition.VehicleUse.NoVehicle);
                        move.GroupMoveActivity = activity;

                        AddSubgoal(move);

                        hasStarted = true;
                    }

                    //process the subgoals
                    Status = ProcessSubgoals(elapsed);

                }
                else if (activity.HasStarted == false && timeWaited < maxTimeToWait)
                {
                    timeWaited += elapsed.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    // we waited long enough, or the activity members left. Fail.
                    Status = Status.Failed;

                    if (activity.HasStarted == false)
                    {
                        // it never got off the ground... pick another goal next.
                        entityIntelligence.Memory.TimePointOfFailedLeisureWalkAttempt = The.Sim.TotalUnPausedGameTime.TotalSeconds;

                    }
                }
           /* }

            ExitIfFailedOrCompleted();

            return Status;*/
        }

        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }

        public override void Deactivate()
        {            
            activity.LeaveActivity(entity);

            if (!activity.HasEnded && activity.EveryoneElseIsReady(entity))
            {
                // send the message: We are done!!!
                activity.SendMessageToEveryoneElse(entity, new Message(Message.MessageTypes.EndGroupMovement));
              /*  foreach (Entity member in activity.Members)
                { 
                    member.HandleMessage(new Message(Message.MessageTypes.EndGroupMovement));
                }*/
            }

        }

    }
}
