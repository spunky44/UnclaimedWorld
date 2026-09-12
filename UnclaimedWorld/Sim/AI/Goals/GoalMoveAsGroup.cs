using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI.Activities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    public class GoalMoveAsGroup: CompositeGoal
    {
        GroupMoveActivity activity;

        public GoalMoveAsGroup(Entity owner, GroupMoveActivity activity) 
            : base(owner)
        {
            this.activity = activity;
        
        }

        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();
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

        public GoalMoveAsGroup()
        {
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
           /* ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                //process the subgoals
                Status = ProcessSubgoals(elapsed);

                if (Status == Status.Completed)
                {   // very important!

                }
        /*    }

            ExitIfFailedOrCompleted();

            return Status;*/
        }
    }
}
