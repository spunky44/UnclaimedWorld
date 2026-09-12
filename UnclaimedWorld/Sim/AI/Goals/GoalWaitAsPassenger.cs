using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    class GoalWaitAsPassenger: Goal, ITopLevelGoal
    {
        double? maxPeriodInSeconds;
        double waitProgress = 0f;

     /*   public GoalWaitAsPassenger(Entity owner, double? maxPeriod)
            : base(owner) 
        {
            this.maxPeriodInSeconds = maxPeriod;
        }
        */
        public GoalWaitAsPassenger(Entity owner)
            : base(owner)
        {

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
            base.DoSnapshot(sn);

            this.maxPeriodInSeconds = sn.DoDoubleNullable(maxPeriodInSeconds);
            this.waitProgress = sn.DoDouble(waitProgress);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            return this;
        }

        public GoalWaitAsPassenger()
        {
        }

        protected override void Activate()
        {
            Status = Status.Active;
        }

        public double TimeSpentInTopLevelGoal { get; set; }

        public double ScoreGoal()
        {
            return 1d; // ??
        }

        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }


        protected override void ProcessWhileActive(GameTime elapsed)
        {
            if (maxPeriodInSeconds == null)
            {
                // do nothing... just wait...

            }
            else
            {
                waitProgress += elapsed.ElapsedGameTime.TotalSeconds;

                if (waitProgress > maxPeriodInSeconds)
                {
                    Status = Status.Completed;
                }

            }
        }

        public override bool HandleMessage(Message message)
        {
            switch (message.MessageType)
            {                
                case Message.MessageTypes.GetOff:              
                    // Stop waiting!!!
                    Status = Status.Completed;
                    return true;       
       
               /* case Message.MessageTypes.Disembark:

                    Status = Status.Completed;
                    return true;  */
            }           

            return false;
        }


       /* protected override double ScoreThisTopLevelGoal()
        {
            return GetCurrentGoalScore();
        }*/

    }
}
