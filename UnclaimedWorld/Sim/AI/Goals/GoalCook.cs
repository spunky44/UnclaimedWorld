using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    /*
    /// <summary>
    /// 1. Move to site. 2. Start producing.
    /// </summary>
    class GoalCook: CompositeGoal
    {
        public CookingJob job;

        private Owner ownerOfMeals;

        Entity fireplace = null;

        public GoalCook(Entity owner, CookingJob job, Owner ownerOfMeals, List<Owner> ownersVehicles)
            : base(owner)
        {
            this.job = job;
            this.ownersOfVehicles = ownersVehicles;
            this.ownerOfMeals = ownerOfMeals;
        }


        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

       

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            throw new Exception("THIS CLASS IS OBSOLETE");
            return this;
        }

        public GoalCook()
        {
        }



        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            
            fireplace = FindCampfire(job.ProductionSite);

            if (fireplace != null)
            {
                job.Fireplace = fireplace;

                // let's cook over fire...
                AddSubgoal(new GoalMoveToPosition(entity,
                    Common.GetAbsolutePosition(fireplace.Location, fireplace.EntityType.StructureType.FireplaceType.CookingPosition, fireplace.FlipHorizontally), 
                    ownersOfVehicles) { IsFinalDestination = true });

                fireplace.Structure.Fireplace.Seat1TakenBy = entity;

                AddSubgoal(new GoalTurnToFace(entity, fireplace.Location.ToVector2()));
            }
            else
            {
                // move to entrance
                AddSubgoal(new GoalMoveToPosition(entity, job.ProductionSite.Location, ownersOfVehicles) { IsFinalDestination = true }); 
            }

        
          //  AddSubgoal(new GoalDoCook(entity, job, ownerOfMeals));         
           
        }

        public static Entity FindCampfire(Entity building)
        {
            Entity fireplace = null;

            if (building.Structure != null && building.Structure.AddOns != null)
            {
                // does the site have a campfire/fireplace? - do this in evaluator...?
                IAddon result = building.Structure.AddOns.FindLast(a => a is Entity && ((Entity)a).Structure != null && ((Entity)a).Structure.Fireplace != null);
                if (result != null)
                {
                    fireplace = (Entity)result;
                }
            }
            return fireplace;
        }

        protected override bool ArePreconditionsOK()
        {
               

            return true;
        }

        public override string GetStatus()
        {
            return "Cooking";
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
           // ActivateIfInactive();

            if (!ArePreconditionsOK())
            {
                Status = Goals.Status.Failed;
            }
            else
            {
                //process the subgoals
                Status = ProcessSubgoals(elapsed);
            }

            if (Status == Status.Completed)
            {   // very important!
                job.Destroy(true);               
            }
            else if (Status == Status.Failed)
            {   // we don't want it anymore...
                job.Abandon(entity);                
            }

           
          //  return Status;
        }

        public override bool IsSame(Job job)
        {
            return job == this.job;
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
                  case Message.MessageTypes.CancelJob:
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

        public override void OnExit()
        {
            base.OnExit();

            if (fireplace != null && fireplace.Structure.Fireplace.Seat1TakenBy == entity)
            {
                fireplace.Structure.Fireplace.Seat1TakenBy = entity;
            }
        }
*/
      /*  public override void Terminate()
        {
            if (fireplace != null && fireplace.Structure.Fireplace.Seat1TakenBy == entity)
            {
                fireplace.Structure.Fireplace.Seat1TakenBy = entity;
            }

            base.Terminate();
        }*/
    
}
