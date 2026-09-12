using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using GameStateManagement;
using UWGame.SimSide.Maps;
using UWGame.Control;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals
{
    class GoalWander: CompositeGoal, ITopLevelGoal
    {
        private Rectangle stayInside;
       // MovementSpeeds movementSpeed = MovementSpeeds.Wander;

        public double TimeSpentInTopLevelGoal { get; set; }

        public GoalWander(Entity owner, Rectangle stayInside)
            : base(owner)
        {
            this.stayInside = stayInside;
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

            base.DoSnapshot(sn);

            this.stayInside = sn.DoRectangle(stayInside);

            return this;
        }

        public GoalWander()
        {
        }

        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            // TODO: make it so that the agent moves a short distance from its present location...

            entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.WalkSlowly;
         
            GoalMoveToPosition move = new GoalMoveToPosition(entity,
                MapManager.TileToWorldPos(new Point(The.Sim.GameplayRandomGenerator.Next(stayInside.Left, stayInside.Right, "GoalWander"), The.Sim.GameplayRandomGenerator.Next(stayInside.Top, stayInside.Bottom, "GoalWander"))),
                null,
                 GoalMoveToPosition.VehicleUse.NoVehicle, null);

            AddSubgoal(move);
            
        }

        public override void Deactivate()
        {
           
            entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal;
         
          //  entity.Locomotor.RecalculateAndSetCurrentSpeed();
        }


        public double ScoreGoal()
        {
            return entityIntelligence.GetCurrentGoalUtility().Value;
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
                       
                        return true; //msg handled

                    default: return false;
                }
            }
            else
            {
                return true;
            }
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
         /*   ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                //process the subgoals
                Status = ProcessSubgoals(elapsed);
           // }

            //if any of the subgoals have failed then this goal re-plans
            ReactivateIfFailed();

          /*  ExitIfFailedOrCompleted();

            return Status;*/
        }
    }
}
