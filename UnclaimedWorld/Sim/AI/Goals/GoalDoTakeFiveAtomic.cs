using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.AllGameData.Constants;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    // this goal is for briefly idling in the same spot when there is nothing else to do.
    class GoalDoTakeFiveAtomic: Goal
    {
        double timeToRest;
        public double TimeAlreadyRested = 0;
        double startedAt;

        const double maxPeriodInSeconds = 0.2;

        private static Pool<GoalDoTakeFiveAtomic> freeGoals = new Pool<GoalDoTakeFiveAtomic>(40);


        public GoalDoTakeFiveAtomic()
            : base()
        {            
        }


      

        private void Init(Entity owner, double timeAlreadyRested, double timeToRest)
        {      
            this.timeToRest = timeToRest;
            startedAt = timeAlreadyRested;
            this.TimeAlreadyRested = timeAlreadyRested;
            base.Init(owner);
        }






        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }

        public static GoalDoTakeFiveAtomic GetGoal(Entity owner, double timeAlreadyRested, double timeToRest)
        {
            GoalDoTakeFiveAtomic goal = freeGoals.Get();

            goal.Init(owner, timeAlreadyRested, timeToRest);

            System.Diagnostics.Debug.Assert(goal.ID != GoalID.Invalid, "Invalid ID???");

            return goal;
        }

        public override void RetireGoal()
        {
            freeGoals.Retire(this);
        }

        protected override void Activate()
        {
            Status = Status.Active;
                        
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()           
           /* ActivateIfInactive();

            if (Status == Status.Active)
            {   */           
                TimeAlreadyRested += elapsed.ElapsedGameTime.TotalSeconds;

                if (TimeAlreadyRested > timeToRest)
                {
                    Status = Status.Completed;
                }
                else
                {
                    // goal must only last 0.2 seconds:                   
                    if (TimeAlreadyRested - startedAt > maxPeriodInSeconds)
                    {
                        Status = Status.Completed;
                    }
                }
         /*   }

            ExitIfFailedOrCompleted();

            return Status;*/
        }

        public override float GetExertionLevel()
        {

            if (entity.HasStance())
            {
                return entity.Locomotor.Stance.CurrentStance.IdleExertionLevel;
            }
            else
            {
                PhysicalWork workConstants = GameData.Instance.Constants.PhysicalWork;
                return workConstants.IdleExertionDefault; //.Lying;
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

            this.timeToRest = sn.DoDouble(timeToRest);
            this.TimeAlreadyRested = sn.DoDouble(TimeAlreadyRested);
            this.startedAt = sn.DoDouble(startedAt);
            
            sn.Ignore(freeGoals);
            sn.Ignore(maxPeriodInSeconds);

            return this;
        }


        #endregion


        public static void ClearPool()
        {
            freeGoals.Clear();           
        }
    }
}
