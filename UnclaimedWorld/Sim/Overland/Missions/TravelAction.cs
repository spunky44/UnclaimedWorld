using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Overland.Missions
{
    /// <summary>
    /// an action that simulates travel off map (does it start/end at the edge?)
    /// </summary>
    public class TravelAction: MissionAction
    {
      //  public DateAndTime.TimeDateYear Departure;

        public TravelActionTemplate TravelActionTemplate;
        private MissionActionTemplateID snapshotActionTemplateID;

       

       // public MissionLocation FromMissionLocation;
        //public TravelLocation FromTravelLocation;

       // public TravelLocation ToTravelLocation;

        public MissionStop ToMissionStop;
        private MissionStopID snapshotToMissionStop;


       // private float travelTime;

      

        /// <summary>
        /// km
        /// </summary>
        public double DistanceProgress
        {
            get;
            private set;
        }

      
        GeodeticCoordinate startCoords;

        public enum OverlandProgress { StartSite, BetweenSites, DestinationSite /*, Docked*/ }

        private OverlandProgress progress = OverlandProgress.StartSite;
        public OverlandProgress Progress
        {
            get
            {
                return progress;
            }
            private set
            {
                progress = value;
            }
        }

        //bool isUnderway = false;
      /*  public bool IsUnderway
        {
            get
            {
                return isUnderway;
            }
        }*/


        public TravelAction()
        {

        }

        public TravelAction(Mission parent, TravelActionTemplate template)
            : base(parent)
        {
            this.TravelActionTemplate = template;

            if (template.ToMissionStop != null)
            {
                ToMissionStop = new MissionStop(parent, template.ToMissionStop); 
            }
        }


        public TravelAction(Mission parent, MissionStop to)
            : base(parent) 
        {
            
            ToMissionStop = to;
          
        }


        /// <summary>
        /// returns the number of days and hours until arrival
        /// </summary>
        /// <returns></returns>
         public DateAndTime.TimeDateYear GetETA()
        {
            double remainingDistance = TravelActionTemplate.Distance - DistanceProgress;

            MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);
            double timeLeftInDays = remainingDistance / job.GetRealizedTravelSpeed();

            DateAndTime.TimeDateYear date = The.Sim.DateAndTime.CurrentTimeDateYear;
            date.AddTime(timeLeftInDays);

            return date;
        }

       /* public string GetETA()
        {
            double remainingDistance = TravelActionTemplate.Distance - DistanceProgress;

            MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);
            double timeLeftInDays = remainingDistance / job.GetRealizedTravelSpeed();

            DateAndTime.TimeDateYear timeStruct = new DateAndTime.TimeDateYear(timeLeftInDays);          
        
            return timeStruct.ToIntervalString();
        }*/

        public override void StartMission()
        {
            ToMissionStop.StartMission();
        } 


        public override void Destroy()
        {
            base.Destroy();

            // destroy the whole chain:
            ToMissionStop.Destroy();
        }


        private void SetCoords(Locations.GeodeticCoordinate coords)
        {
            MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);

            job.IterateVehicles(e => e.Coords = coords);
        }


        /// <summary>
        /// TODO: call this when the transport reaches the edge of the map.
        /// </summary>
        private void LeaveSite()
        {
                                  
            // GoOffline()..?
            // Entity.MoveOffMap()

            Start();

           
            NotifyNextStopAllegiance();            
        }

        private void NotifyNextStopAllegiance()
        {
            string arrival = GetETA().ToString();
            Allegiance destinationAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)TravelActionTemplate.ToMissionStop.TravelLocation.AllegianceID);

            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.MissionTemplate.Allegiance);
            if (destinationAllegiance != null)
            {
                CommunicationMethod? workingMethod;
                if (Communicates.IsInCommunicationRange(destinationAllegiance, thisAllegiance, out workingMethod))
                //destinationAllegiance.IsInCommunicationRange(thisAllegiance, out workingMethod)) // tests using base equipment, not transport comms...
                {
                    Allegiance fromAllegiance;
                    Expedition fromExpedition;
                    IKnownEntityData fromTerminal;
                    Site site;
                    if (thisAllegiance != null
                        && parent.StartMissionStop.MissionStopTemplate.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out fromAllegiance, out fromExpedition, out fromTerminal))
                    {
                        The.Client.AddLogEvent(destinationAllegiance, The.Client.Log.GeneralEvent, null,
                            "A transport from " + site.Name + " is on its way. ETA: " + arrival); //"A " + weaponEntity.EntityType.Name.ToLower(Config.Culture) + " broke while " + entity + " was using it.");
                    }
                }
            }
        }

        /// <summary>
        /// computes the distance and bearing, and starts moving towards the target from where we currently are.
        /// </summary>
        public void Start()
        {
            Progress = OverlandProgress.BetweenSites; // setting this value will make Mission.CurrentLocation no longer return a Site location.

            GeodeticCoordinate coords = parent.Coords.Value;

            startCoords = coords;
           // Site currentSite = LookUp<Site, SiteID>.FindByID((SiteID)parent.CurrentLocation.SiteID);

            // TODO: setting location to null on Entity should remove them from the Map
           // SetLocationOfMembers(null, null, coords); 
            
            // moving off-map now - no longer at the site, but has the same coords.
            IterateMembers(e => SetLocationOfMember(e, null, null, coords), null, null);
                              //  e => e.TransferToPlaySite(null, null, null, null), null); // don't change location on contained entities
            
            /*
            if (TravelActionTemplate.Route != null)
            {
                Route route = LookUp<Route, RouteID>.FindByID((RouteID?)TravelActionTemplate.Route);
                Distance = route.Length; // TODO: resume from the current position along the route...
            }
            else
            {
                Site destinationSite = LookUp<Site, SiteID>.FindByID((SiteID)TravelActionTemplate.ToMissionStop.TravelLocation.SiteID);

                Distance = The.Sim.World.GetAirDistance(
                    coords,
                    destinationSite.Coords);

                bearing = DistanceCalculator.GetBearing(coords, destinationSite.Coords);
            }
            */
        }

       

        private void SetLocationOfMember(Entity entity, Site newSite, Vector3? playSiteLocation = null, GeodeticCoordinate? coords = null)
        {
            Site oldSite = entity.Site;

            if (newSite == null && oldSite != null && oldSite.IsPlaySite)
            {
                // this call affects contained entities also:
                entity.PlaceEntityOffSite(coords.Value);
            }
            else
            {
               
                entity.Site = newSite;
                entity.Coords = coords;
                entity.Location = playSiteLocation;
            }

        }

        /// <summary>
        /// helper method
        /// </summary>
        /// <param name="topLevelContainerFunction"></param>
        /// <param name="containedFunction"></param>
        /// <param name="allEntityFunction"></param>
        private void IterateMembers(Action<Entity> topLevelContainerFunction, Action<Entity> containedFunction, Action<Entity> allEntityFunction)
        {
            MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);

            if (allEntityFunction != null)
            {
                job.IterateVehicles(allEntityFunction);
                job.IterateVehicleContents(allEntityFunction);
            }

            if (topLevelContainerFunction != null)
            {
                job.IterateVehicles(topLevelContainerFunction);
            }

            if (containedFunction != null)
            {
                job.IterateVehicleContents(containedFunction);
            }

           

            // do the job takers as well, (in case they are outside the vehicles??):
            for (int i = 0; i < job.TakenBy.Count; i++)
            {
                Entity taker = job.TakenBy.Get(i);

                if (taker.ContainedBy == null)
                {
                    if (topLevelContainerFunction != null)
                    {
                        topLevelContainerFunction(taker);
                    }

                    if (allEntityFunction != null)
                    {
                        allEntityFunction(taker);                        
                    }

                    if (taker.Contains != null)
                    {
                        if (containedFunction != null)
                        {
                            taker.Contains.IterateContained(e => containedFunction(e));
                        }

                        if (allEntityFunction != null)
                        {
                            taker.Contains.IterateContained(e => allEntityFunction(e));
                        }
                    }                   
                }
            } 
        }

        

        /// <summary>
        /// sets agents that are OnLine to a wait goal
        /// </summary>
        /// <param name="agent"></param>
        private void SetPassengerGoal(Entity agent)
        {
            if (agent.EntityType.IntelligenceType != null)
            {
                agent.Intelligence.SetTopLevelGoal(new GoalWaitAsPassenger(agent),
                    1d);
            }
        }

        //Show up at site. Possibly calculate point of entry from site world positions.      
        private void ArriveAtSite()
        {
            Progress = OverlandProgress.DestinationSite;

            TravelLocation toLocation = TravelActionTemplate.ToMissionStop.TravelLocation;
            
            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)this.parent.MissionTemplate.Allegiance);
            SharedKnowledge sharedKnowledge = thisAllegiance.SharedKnowledge;

            Site thisSite;
            Allegiance allegiance;
            IKnownEntityData terminal;
            Expedition expedition;
            if (toLocation.ResolveLocation(sharedKnowledge, out thisSite, out allegiance, out expedition, out terminal))
            {
                /*  Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)toLocation.AllegianceID);
                  if (allegiance != null)
                  {*/
                parent.CurrentMissionStop = ToMissionStop;

                //   Site thisSite = LookUp<Site, SiteID>.FindByID((SiteID)toLocation.SiteID);

                

                if (thisSite.IsPlaySite)
                {

                    Vector3? location;
                    if (LocateLandingArea(out location))
                    {
                        TransferToPlaySite(thisSite, location.Value, expedition);

                    }
                    else
                    {
                        // action failed... now what?
                        HandleFailedTravelAction();
                    }
                }
                else
                {
                    TransferToOtherSite(thisSite, expedition);

                    //OLD
                    //IterateMembers(e => SetLocationOfMember(e, thisSite, null, null), null, null);

                }
                // }
                /* else
                 {
                
                     HandleFailedTravelAction();
                 */
            }
            else
            {
                HandleFailedTravelAction();
            }
        }

        private void TransferToPlaySite(Site site, Vector3 location, Expedition expedition)
        {           
            // set the top level containers' location, but not the contained entities'
            // we set expedition now, because entities belonging to othersite expeditions, but being presenton the play site is not supported ... they have no SharedKnowledge instance is one problem...
          
            IterateMembers(e => e.TransferToPlaySite(location, null, null, expedition),
                           e => e.TransferToPlaySite(null, null, null, expedition), null); // don't change location on contained entities, they should stay contained
                        

            MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);

            // for agents:          
            // give agents passenger goals
            
            // the agents are still on the barge/transport at this point. Some of them may disembark with a Disembark action -> GoalExit.
            // The GoalExit must update before the barge leaves though!

            job.IterateVehicleContents(e => SetPassengerGoal(e));
            
           
        }


        private void TransferToOtherSite(Site site, Expedition expedition)
        {
            // set the top level containers' location, but not the contained entities'

            IterateMembers(e => e.TransferToOtherSite(site, null, expedition),
                           e => e.TransferToOtherSite(site, null, expedition), null); // don't change location on contained entities

                      

        }

        /*
        private void HandleDelayedOwnershipChange()
        {
            MissionJob missionJob = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);

          //  if (missionJob.DelayedOwnershipChangeList != null)
            if (parent.Contracts != null)
            {
               
                Expedition exp;
                Site site;
                Allegiance allegiance;
                Entity terminal;
                parent.CurrentLocation.ResolveLocation(out site, out allegiance, out exp, out terminal);

                if (exp != null)
                {
                    OwnerID currentExpedition;
                    currentExpedition = ((IOwner)exp).ID;

                    // if the buyer is an expedition on the current playsite map, complete the ownership change now.
                    foreach (var contract in parent.Contracts)
                    {
                        contract.HandleDelayedOwnershipChange(currentExpedition);                       
                    }
                }               
            }
        }*/


        private void HandleFailedTravelAction()
        {
            parent.Abort();
        }


        /// <summary>
        ///  Look for landingpad owned by DestinationAllegiance. If none exists, find their start position/camp.       
        /// </summary>
        /// <returns></returns>
        private bool LocateLandingArea(out Vector3? dockingLocation)
        {
            TravelLocation toLocation = TravelActionTemplate.ToMissionStop.TravelLocation;

            dockingLocation = null;
            Allegiance destinationAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)toLocation.AllegianceID);

            if (destinationAllegiance == null)
                return false;

           
            Vector3? location;
            if (TravelActionTemplate.ToMissionStop.TravelLocation.ExpeditionID != null)
            {
                Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)toLocation.ExpeditionID);
                if (expedition != null)
                {
                    if (toLocation.TerminalEntityID.HasValue)
                    {
                        Entity terminal = Entity.FindByID((EntityID)toLocation.TerminalEntityID.Value);
                        if (terminal != null)
                        {
                            dockingLocation = GetTerminalLocation(terminal);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    location = LocateLandingAreaInExpedition(expedition);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                foreach (var expedition in destinationAllegiance.Expeditions)
                {
                    location = LocateLandingAreaInExpedition(expedition);

                    if (location != null)
                    {
                        dockingLocation = location.Value;
                        return true;
                    }
                }
            }

            dockingLocation = EntityGroup.GetFreeGroundLocation(destinationAllegiance.Expeditions[0]); // .Center;
            return true;
        }

        private Vector3? LocateLandingAreaInExpedition(Expedition expedition)
        {
            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.MissionTemplate.Allegiance);
            EntityType vehicleType = parent.MissionTemplate.TransportationType.GetMainTransportation();
            VehicleContainerType vehicle = vehicleType.ContainerType as VehicleContainerType;

            TerminalType.TypesOfTerminal terminalType = TerminalType.TypesOfTerminal.Land;
            if (vehicle != null)
            {
                terminalType = vehicle.CanUseTerminal ?? TerminalType.TypesOfTerminal.Land;
            }

            List<IKnownEntityData> terminals = expedition.GetWorkingTerminals(allegiance.SharedKnowledge, terminalType);

            if (terminals != null && terminals.Count > 0)
            {
                return GetTerminalLocation(terminals[0]); 
            }


            return null;

            //return destinationAllegiance.Expeditions[0].GetFreeGroundLocation(); // .Center;
        }


        private Vector3 GetTerminalLocation(IKnownEntityData terminal)
        {
            return terminal.PlaySiteLocation; // docking point = access point?
        }

        public override bool Update(Microsoft.Xna.Framework.GameTime elapsed)
        {
            base.Update(elapsed);

            if (Progress == OverlandProgress.StartSite) // !isUnderway)
            {
                LeaveSite();
            }

            MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);
            double progress = job.GetRealizedTravelSpeed() * The.Sim.DateAndTime.DaysPerSecond * elapsed.ElapsedGameTime.TotalSeconds;

          
            DistanceProgress += progress;
            

            if (DistanceProgress >= TravelActionTemplate.Distance)
            {
                ArriveAtSite();

                return true; // done.
            }
            else 
            {
                // update coords:
               // GeodeticCoordinate currentCoords = DistanceCalculator.CoordFromDistance(startCoords, TravelActionTemplate.Bearing.Value, DistanceProgress, The.Sim.World.WorldRadius); // test this!

                // relayed communication is not in the game. If we want it, the transport should gain/lose contact with allegiances here

                Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.MissionTemplate.Allegiance);

                Site site;
                Allegiance allegiance;
                Expedition expedition;
                IKnownEntityData terminal;
                if (thisAllegiance != null && ToMissionStop.MissionStopTemplate.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
                {

                    GeodeticCoordinate currentCoords = DistanceCalculator.GetIntermediatePoint(startCoords, site.Coords, DistanceProgress / TravelActionTemplate.Distance, TravelActionTemplate.Distance, The.Sim.World.WorldRadius);

                    IterateMembers(e => SetLocationOfMember(e, null, null, currentCoords), null, null);
                }
                else
                {
                    HandleFailedTravelAction();
                }
         
            }

            return false;

        }

        public void SetParentPostLoad(Mission parent)
        {
            this.parent = parent;

            ToMissionStop = LookUp<MissionStop, MissionStopID>.FindByID(snapshotToMissionStop);

            ToMissionStop.SetParentPostLoad(parent);

            //ToMissionStop.mission = parent;
        }


        #region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

           // this.Distance = sn.DoDouble(Distance);
            this.DistanceProgress = sn.DoDouble(DistanceProgress);
            this.progress = sn.DoEnum(progress);
         //   this.bearing = sn.DoDoubleNullable(bearing);
            this.startCoords = sn.DoGeodeticCoordinate(startCoords); // (GeodeticCoordinate)sn.DoISnapshot(startCoords);

            this.snapshotToMissionStop = (MissionStopID)sn.SnapshotID<MissionStop, MissionStopID>(ToMissionStop);

            if (sn.mode == Snapshotter.Mode.Save && snapshotToMissionStop == (MissionStopID)0)
            {
                throw new Exception();
            }

            snapshotActionTemplateID = (MissionActionTemplateID)sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(TravelActionTemplate);
                       

            sn.Ignore(TravelActionTemplate);

            return this;

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn);

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        //public bool IsSnapshotted { get; set; }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            TravelActionTemplate = (TravelActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);


            /* moved to SetParentPostLoad:
            ToMissionStop = LookUp<MissionStop, MissionStopID>.FindByID(snapshotToMissionStop);
            ToMissionStop.mission = parent;*/

           // ToMissionStop.LoadPostProcess(sn);

        }

        #endregion
    }
}
