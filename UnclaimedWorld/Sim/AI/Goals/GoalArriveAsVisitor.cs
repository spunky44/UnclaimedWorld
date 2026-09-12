using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.GatheringSites;


namespace UWGame.SimSide.AI.Goals
{
    class GoalArriveAsVisitor : CompositeGoal
    {
        EntityID? entityToArriveAt = null;
        ExpeditionID? expeditionToArriveAt = null;
        private Vector3 seat = Vector3.Zero;

        public GoalArriveAsVisitor(Entity entity, EntityID? newPlace, List<EntityGroupID> ownersOfVehicles)
            : base(entity)
        {
           
            this.entityToArriveAt = newPlace;
            this.ownersOfVehicles = ownersOfVehicles;
        }

        public GoalArriveAsVisitor(Entity entity, ExpeditionID? newPlace, List<EntityGroupID> ownersOfVehicles)
            : base(entity)
        {
            
            this.expeditionToArriveAt = newPlace;
            this.ownersOfVehicles = ownersOfVehicles;
        }
      /*  public Vector3 Seat
        {
            get
            {
                return seat;
            }
        }*/


      //  public bool IsFinalDestination;

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

            this.entityToArriveAt = sn.DoEntityIDNullable(entityToArriveAt);
            this.expeditionToArriveAt = sn.DoEnumNullable(expeditionToArriveAt);
            this.seat = sn.DoVector3(seat);

            return this;
        }

        public GoalArriveAsVisitor()
        {
        }


        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            //TODO here, have the place entity figure out where we should sit, and lets set that as our MoveToPosition, while
            //monitoring whether that is still available and viable, adjusting while en-route

            IKnownEntityData gatheringSiteData = null;
            Vector2? seatToUse;
            if (entityToArriveAt.HasValue)
            {
                if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entityToArriveAt.Value, out gatheringSiteData)))
                {
                    return;
                }

                seat = gatheringSiteData.AccessPoint.Value;

                seatToUse = gatheringSiteData.GatheringSite.AddVisitor(ref entity);
            }
            else
            {
                Expedition expedition = Expedition.FindByID(expeditionToArriveAt.Value);
                seat = expedition.Center.Value;
                seatToUse = expedition.GatheringSite.AddVisitor(ref entity);
            }
            
            if (seatToUse.HasValue)
            {
                seat.X = seatToUse.Value.X;//The seat from GatheringSite are in world coords, not local
                seat.Y = seatToUse.Value.Y;
            }

            //TODO, GoalMoveToPosition is not an atomic goal type, and should not be used here.
            AddSubgoal(new GoalMoveToPosition(entity, seat, null, GoalMoveToPosition.VehicleUse.NoVehicle));

            if (gatheringSiteData != null)
            {
                AddSubgoal(new GoalTurnToFace(entity, gatheringSiteData.PlaySiteLocation.ToVector2()));
            }
            else
            {
                AddSubgoal(new GoalTurnToFace(entity, Expedition.FindByID(expeditionToArriveAt.Value).Location.Value.ToVector2()));
            }

        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {           
        
            //if status is inactive, call Activate()
           /* ActivateIfInactive();

            if (Status == Status.Active)
            {*/
                //process the subgoals
                Status = ProcessSubgoals(elapsed);

                //here, we want to measure progress thru the various stages of docking with the GatheringSite

                //1) Approach Rally reached
                if (Status == Status.Active)
                {

                }
                //2) Dock point reached
                else if (Status == Status.Completed)
                {
                    IKnownEntityData gatheringSiteData;

                    if (entityToArriveAt.HasValue)
                    {
                        entityIntelligence.GetKnownData(entityToArriveAt.Value, out gatheringSiteData);
                        if (gatheringSiteData != null)
                        {
                            gatheringSiteData.GatheringSite.OnDockReached(ref entity);
                        }
                    }
                    else
                    {
                        GatheringSite gatheringSite;

                        gatheringSite = Expedition.FindByID(expeditionToArriveAt.Value).GatheringSite;
                        gatheringSite.OnDockReached(ref entity);
                    }


                    //do stuff associated with getting into our new seating location
                    //hack to make our visit last forever... testing the GatheringSite
                    // entity.Intelligence.DisableAI = true;
                }
                else if (Status == Status.Failed)
                {
                    //do stuff in reaction to not getting to our seat
                }
          /*  }

            ExitIfFailedOrCompleted();

            return Status;*/
        }


        public override void OnExit()
        {
            base.OnExit();

            // do stuff here to unreserve a spot with the place we would have been visiting
        }




    }
}
