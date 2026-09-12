using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    class GoalWaitForPassenger: Goal
    {
        double? maxPeriodInSeconds;
        double waitProgress = 0d;

        Entity passenger;

        public GoalWaitForPassenger(Entity owner, double? maxPeriod, Entity passenger)
            : base(owner) 
        {
            this.maxPeriodInSeconds = maxPeriod;
            this.passenger = passenger;
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
            sn.DoUnknownObject(passenger);

            return this;
        }

        public GoalWaitForPassenger()
        {
        }

        protected override void Activate()
        {
            Status = Status.Active;

            passenger.SendMessage(new Message(entity, Message.MessageTypes.HopOnBoard, null)); 
                       
        }
        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }
      


        protected override void ProcessWhileActive(GameTime elapsed)
        {
          /*  ActivateIfInactive();

            if (Status == Goals.Status.Active)
            {*/
                Entity vehicle;

                if (!entity.GetDrivenVehicle(out vehicle)
                    || vehicle == null)
                {
                    Status = Goals.Status.Failed;

                   // ExitIfFailedOrCompleted();
                    return; // Status;
                }

                Vehicles.Vehicle vehicleComponent = vehicle.Vehicle;

                if (vehicleComponent.WaitingForPassengers == 0) // we may have missed a message form a boading passenger
                {
                    Status = Status.Completed;

                   // ExitIfFailedOrCompleted();
                    return; // Status;
                }

                if (maxPeriodInSeconds == null)
                {
                    // do nothing... just wait...

                   
                }
                else
                {
                    waitProgress += elapsed.ElapsedGameTime.TotalSeconds;

                    if (waitProgress > maxPeriodInSeconds)
                    {
                        SetNoLongerWaiting(vehicleComponent);
                        Status = Status.Completed;
                    }
                   
                }
          /*  }

            ExitIfFailedOrCompleted();

            return Status;*/

        }

        private void SetNoLongerWaiting(Vehicles.Vehicle vehicle)
        {
            
            vehicle.WaitingForPassengers--;
            if (vehicle.WaitingForPassengers < 0)
            {
                vehicle.WaitingForPassengers = 0;
            }
           

        }

        public override bool HandleMessage(Message message)
        {
            switch (message.MessageType)
            {
                case Message.MessageTypes.OKImOn:

                    Entity vehicle;
                    if (!entity.GetDrivenVehicle(out vehicle)
                        || vehicle == null)
                    {
                        Status = Goals.Status.Failed;
                        return true;
                    }

                    Vehicles.Vehicle vehicleComponent = vehicle.Vehicle;
                    
                    SetNoLongerWaiting(vehicleComponent);

                    // Stop waiting!!!
                    Status = Status.Completed;
                    return true;              
            }           

            return false;
        }

       
    }
}
