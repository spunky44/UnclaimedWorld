using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
namespace UWGame.SimSide.AI.Goals
{
    
    class GoalReturnHome: CompositeGoal, ITopLevelGoal
    {
       
        private Vector3 locationOfExpedition;
        public double TimeSpentInTopLevelGoal { get; set; }

        private bool isBold;

        Vector3? teleportTo;

        public GoalReturnHome(Entity owner, List<EntityGroupID> ownersVehicles, bool isBold, Vector3? teleportTo)
            : base(owner)
        {

            this.isBold = isBold;
            this.teleportTo = teleportTo;
            this.ownersOfVehicles = ownersVehicles;            
        }


        public GoalReturnHome()
        {
        }


        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            if (teleportTo.HasValue)
            {
                if (entity.ContainedBy.HasValue)
                {
                    Entity container;
                    if (entity.GetContainedBy(out container))
                    {
                        container.Contains.Remove(entity);
                    }
                }

                entity.Location = teleportTo.Value;
            }

            Vector3? returnTo = GetReturnLocation();

            if (returnTo.HasValue)
            {
                AddSubgoal(new GoalMoveToPosition(entity, returnTo.Value, ownersOfVehicles, GoalMoveToPosition.VehicleUse.FreeUpAfterUse) { IsFinalDestination = true });

                if (isBold)
                {
                    // set our stance to 'Bold' when attacking to prevent blocking/fleeing.                    
                    entity.Intelligence.SetBoldStance();
                }

                locationOfExpedition = entityIntelligence.CurrentExpedition.Center.Value;
            }
            else
            {
                Status = Goals.Status.Failed;
            }         
           
        }

        private Vector3? GetReturnLocation()
        {
            SubtileInfluence subTileMap = SubtileInfluence.FindFreeSpotNearLocation(entityIntelligence.CurrentExpedition.Center.Value, 15, entity.Location, entity, true);

            Point bestSubtilePos;
            if (InfluenceMap.GetBestSubtileLocationThatIsntBlocked(subTileMap.Values, out bestSubtilePos) == -1)
            {
                return null;
            }
            else
            {
                return MapManager.SubTileToWorldPos(new Point(subTileMap.TopLeftSubtilePositionOfMap.X + bestSubtilePos.X, subTileMap.TopLeftSubtilePositionOfMap.Y + bestSubtilePos.Y)).ToVector3();

            }
        }

        protected override bool ArePreconditionsOK()
        {
            if (entityIntelligence.CurrentExpedition.Center != locationOfExpedition)
            {
                return false;
            }             

            return true;
        }

        public override string GetStatus()
        {
            return "Returning home";
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()

            if (!ArePreconditionsOK())
            {
                Status = Goals.Status.Failed;
            }
            else
            {
                //process the subgoals
                Status = ProcessSubgoals(elapsed);
            }

        }


        public override bool RequiresBoldStance()
        {
            return isBold;
        }


        public double ScoreGoal()
        {
            return entityIntelligence.GetCurrentGoalUtility().Value;

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

            this.locationOfExpedition = sn.DoVector3(locationOfExpedition);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            this.isBold = sn.DoBool(isBold);
            this.teleportTo = sn.DoVector3Nullable(teleportTo);

            return this;
        }

        #endregion
    }
}
