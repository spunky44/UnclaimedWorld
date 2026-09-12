using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI.Goals;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.AI.Goals
{
    /// <summary>
    /// can only execute when in a terminal (or at the edge?)
    /// </summary>
    public class GoalDoEmigrate: Goal
    {
        AllegianceID newAllegiance;
        ExpeditionID newExpedition;
        SiteID newSite;
        RouteID? routeID;

        public GoalDoEmigrate(Entity owner, SiteID newSite, AllegianceID newAllegiance, ExpeditionID newExpedition, RouteID? routeID)
            : base(owner)
        {
            this.newSite = newSite;
            this.newAllegiance = newAllegiance;
            this.newExpedition = newExpedition;
            this.routeID = routeID;
        }


        public GoalDoEmigrate()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");      
     
        }


        protected override void Activate()
        {
            Status = Status.Active;


            // perhaps add a wait goal to make time for spoken lines "I bid you adieu" 
            List<ActionSets> defaultActionSets;
            entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.LeavesSiteForNewAllegiance, out defaultActionSets); // player only..?

            Goal.FireEventActions(entity, null, defaultActionSets, null);


            // create a Mission and leave the map

         
            StartMission();


            Status = Goals.Status.Completed;

          
        }

        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {

            //process the subgoals
           // Status = ProcessSubgoals(elapsed);


            // perhaps add a wait goal to make time for spoken lines "I bid you adieu" 
         /*   List<ActionSets> defaultActionSets;
            entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.LeavesSiteForNewAllegiance, out defaultActionSets); // player only..?

            Goal.FireEventActions(entity, null, defaultActionSets, null);


            // create a Mission and leave the map

            StartMission();
            */

         
        }


        void StartMission()
        {
            OwnerID ownerID = ((IOwner)entityIntelligence.CurrentExpedition).ID;

            MissionTemplate missionTemplate = new MissionTemplate(entityIntelligence.Allegiance.ID, ownerID, false);
            missionTemplate.TransportationType = new TransportationTemplate();

            TravelLocation start = new TravelLocation(entityIntelligence.Allegiance, (long)entityIntelligence.CurrentExpedition.ID, null); 

            // add a start location to the mission:
            MissionStopTemplate startTemplate = new MissionStopTemplate(false);         
            startTemplate.TravelLocation = start;
            missionTemplate.StartMissionStopTemplate = startTemplate;

            // add destination:
            TravelLocation destination = new TravelLocation((long)newSite, (long)newAllegiance, (long)newExpedition, null);
            MissionStopTemplate destinationTemplate = new MissionStopTemplate(false); //Mission);
            destinationTemplate.TravelLocation = destination; 

            // add travel action
            TravelActionTemplate travelAction = new TravelActionTemplate(missionTemplate.StartMissionStopTemplate, destinationTemplate);

            missionTemplate.StartMissionStopTemplate.TravelAction = travelAction;
            Route route = null;
            if (routeID.HasValue)
            {
                route = LookUp<Route, RouteID>.FindByID(routeID);
                travelAction.SetRoute(route, false);
            }
            else
            {
                travelAction.SetRoute(null, true);
            }
           
            List<EntityID> passengers = new List<EntityID>();
            passengers.Add(entity.ID);
            startTemplate.Actions.Enqueue(new EmbarkActionTemplate(startTemplate, passengers, false)); 

         
            destinationTemplate.Actions.Enqueue(new UnloadActionTemplate(destinationTemplate, false)); 
            destinationTemplate.Actions.Enqueue(new DisembarkActionTemplate(destinationTemplate, false)); 


              
          //  missionTemplate.AddMissionLocation(destinationTemplate);

            missionTemplate.AssignIDs();

            Mission mission = new Mission(entityIntelligence.CurrentExpedition.OwnedEntities, missionTemplate, null);

            // take the mission job (still owned by player allegiance):
            Job job = LookUp<Job, JobID>.FindByID(mission.MissionJob);
            job.TakeJob(entity);

            // start
            mission.StartMission();

            entityIntelligence.Memory.SetEmigrateDecision(null, entity); // reset              
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

            this.newAllegiance = sn.DoEnum(newAllegiance);
            this.newSite = sn.DoEnum(newSite); 
            this.newExpedition = sn.DoEnum(newExpedition);
            this.routeID = sn.DoEnumNullable(routeID);

            return this;
        }

        #endregion

    }
}
