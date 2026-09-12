using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.InGameEvents.Actions;
namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// used by an agent that is moving away on their own
    /// </summary>
    class GoalEmigrate : CompositeGoal, ITopLevelGoal
    {
        AllegianceID newAllegiance;
        ExpeditionID newExpedition;
        SiteID newSite;
        EntityID terminalToLeaveFrom;
        RouteID? routeID;

        public double TimeSpentInTopLevelGoal { get; set; }

        public GoalEmigrate(Entity owner, SiteID newSite, AllegianceID newAllegiance, ExpeditionID newExpedition, EntityID terminalToLeaveFrom, RouteID? routeToUse, List<EntityGroupID> ownersOfVehicles)
            : base(owner)
        {
            this.newSite = newSite;
            this.newAllegiance = newAllegiance;
            this.newExpedition = newExpedition;
            this.terminalToLeaveFrom = terminalToLeaveFrom;
            this.routeID = routeToUse;
            this.ownersOfVehicles = ownersOfVehicles;
        }


        public GoalEmigrate()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        protected override bool ArePreconditionsOK()
        {
            IKnownEntityData terminalData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(terminalToLeaveFrom, out terminalData)))
            {
                return false;
            }

            return true;
        }

        protected override void Activate()
        {
            Status = Status.Active;

            //make sure the subgoal list is clear.
            RemoveAllSubgoals();

            // move to the terminal/edge


            IKnownEntityData terminalData;
            if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(terminalToLeaveFrom, out terminalData)))
            {
                return;
            }

            // perhaps create the Job for the mission here... and assign any weapons we bring along to it...
            // for now the job is created in the Mission ctor, and taken there.

            DropAllCarriedItems(); // drop everything now.

            AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, terminalData, GoalMoveToPosition.VehicleUse.FreeUpAfterUse));
            AddSubgoal(new GoalDoEmigrate(entity, newSite, newAllegiance, newExpedition, routeID));
            AddSubgoal(new GoalWait(entity, 4d));    // we need a Wait goal because the Mission takes 2 frames to embark and leave the map. During that time, we don't want the Arbitrator to run again


            List<ActionSets> defaultActionSets;
            entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.StartsToLeaveAllegiance, out defaultActionSets); // player only..?
           // entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.DecidedToLeaveAllegiance, out defaultActionSets); // player only..?

            Goal.FireEventActions(entity, null, defaultActionSets, null);


        }



        protected override void ProcessWhileActive(GameTime elapsed)
        {
            if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
            {
                //process the subgoals
                Status = ProcessSubgoals(elapsed);

            }

        }

        public override string GetStatus()
        {
            return "Leaving site";
        }

        public double ScoreGoal()
        {
            return entityIntelligence.GetCurrentGoalUtility().Value;
        }

        #region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.newAllegiance = sn.DoEnum(newAllegiance);
            this.newSite = sn.DoEnum(newSite);
            this.newExpedition = sn.DoEnum(newExpedition);
            this.terminalToLeaveFrom = sn.DoEnum(terminalToLeaveFrom);
            this.routeID = sn.DoEnumNullable(routeID);
            this.TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);

            return this;
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


        #endregion
    }
}
