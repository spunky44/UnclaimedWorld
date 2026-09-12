using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using System.Xml.Serialization;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Overland.Missions.Templates
{
    public class TravelActionTemplate : MissionActionTemplate
    {
        /// <summary>
        /// optional land/sea route to follow along the way
        /// RouteID
        /// </summary>    
        public long? Route;

        public bool? UsesAirRoute;
       // public double? AirRouteDistance;

        public MissionStopTemplate ToMissionStop;
        MissionStopTemplateID snapshotToMissionStop;

        public override string Name
        {
            get { return TemplateName; }
        }

        public static string TemplateName
        {
            get { return "Travel"; }
        }

        public override ActionTypes ActionType
        {
            get { return ActionTypes.Travel; }
        }


        [XmlIgnore]
        /// <summary>
        /// km
        /// </summary>
        public double Distance
        {
            get;
            private set;
        }

        [XmlIgnore]
        public double? Bearing
        {
            get;
            private set;
        }

        /// <summary>
        /// we can select a route after a transport has been selected
        /// </summary>
        /// <param name="missionStopTemplate"></param>
        /// <param name="to"></param>
        /// <param name="route"></param>
        public TravelActionTemplate(MissionStopTemplate missionStopTemplate, MissionStopTemplate to) //, Route route)
            : base(missionStopTemplate, false)           
        {           
            ToMissionStop = to;
           // SetRoute(missionStopTemplate);
        }

        /// <summary>
        /// needed for serializer
        /// </summary>
        public TravelActionTemplate()
        {
        }

        public void SetRoute(Route route, bool useAirRoute) //, MissionStopTemplate missionStopTemplate)
        {
            this.UsesAirRoute = null;
            this.Route = null;
          //  this.AirRouteDistance = null;

            if (route != null)
            {
                // Route route = LookUp<Route, RouteID>.FindByID((RouteID?)Route);
                Distance = route.Length; // TODO: resume from the current position along the route...
                Route = (long)route.ID;
            }
            else if (useAirRoute)
            {
                this.UsesAirRoute = true;
              //  this.AirRouteDistance = airRouteDistance;

                // compute air distance and bearing
                Site startingSite = LookUp<Site, SiteID>.FindByID((SiteID)base.MissionStopTemplate.TravelLocation.SiteID);

                Site destinationSite = LookUp<Site, SiteID>.FindByID((SiteID)ToMissionStop.TravelLocation.SiteID);

                Distance = The.Sim.World.GetAirDistance(
                    startingSite.Coords,
                    destinationSite.Coords);

                Bearing = DistanceCalculator.GetBearing(startingSite.Coords, destinationSite.Coords);
            }
        }

       

        public override void AssignIDs()
        {
            base.AssignIDs();

            ToMissionStop.AssignIDs();
        }


        private bool ValidateRoute(VehicleContainerType vehicle, ref List<string> errors)
        {
            return ValidateRoute(Route, UsesAirRoute, 
                Distance, vehicle, ref errors);
        }

        public static bool ValidateRoute(long? routeID, bool? usesAirRoute, double distance, VehicleContainerType vehicle, ref List<string> errors)
        {
            bool routeIsValid = true;
            if (usesAirRoute == true || routeID.HasValue)
            {
                RouteType? routeType = null;
                Route route;
                // double distance;
                bool isAirRoute = false;
                if (routeID.HasValue)
                {
                    route = LookUp<Route, RouteID>.FindByID((RouteID)routeID.Value);
                    routeType = route.RouteType;
                    // distance = route.Length;
                }
                else
                {
                    //distance = Distance; AirRouteDistance.Value;
                    isAirRoute = true;
                }


                if (!vehicle.CanUseRoute(routeType, isAirRoute, distance))
                {
                    routeIsValid = false;
                }
            }
            else
            {
                routeIsValid = false;
            }


            if (!routeIsValid)
            {
                Common.AddToList(ref errors, "No valid route exists.");
                return false;
            }

            return true;

        }

        public override bool Validate(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors)
        {
            // validate route/transport here..
            EntityType transportType = parent.TransportationType.GetMainTransportation();
            if (transportType != null)
            {
                VehicleContainerType vehicle = transportType.ContainerType as VehicleContainerType;

                if (!ValidateRoute(vehicle, ref errors))
                {
                    return false;
                }

                return CanUseTerminal(parent, ToMissionStop.TravelLocation, vehicle, ref errors);

            }
            else
            {
                Common.AddToList(ref errors, "No transportation selected.");
                return false;
            }
        }

        public static bool CanUseTerminal(MissionTemplate parent, TravelLocation travelLocation, VehicleContainerType vehicle, ref List<string> errors)
        {
            TerminalType.TypesOfTerminal? canUseTerminalType = vehicle.CanUseTerminal;
            if (canUseTerminalType.HasValue)
            {
                Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);

                Allegiance allegiance;
                Expedition expedition;
                IKnownEntityData terminal;
                Site site;
                if (travelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
                {
                    if (canUseTerminalType != terminal.EntityType.TerminalType.TypeOfTerminal)
                    {
                        Common.AddToList(ref errors, GetWrongTerminalError(terminal));
                        return false;
                    }
                }
            }

            return true;
        }

        public static string GetWrongTerminalError(IKnownEntityData terminal)
        {
            return terminal.EntityType.Name + " is the wrong terminal type for this vehicle.";
        }

        public DateAndTime.TimeDateYear GetTravelTime(MissionTemplate parent)
        {
            double speed = parent.TransportationType.GetEstimatedTravelSpeed();
            
            double timeInDays = Distance / speed;

            DateAndTime.TimeDateYear timeStruct = new DateAndTime.TimeDateYear(timeInDays);

            return timeStruct;

           // float timeInDays = (float)The.Sim.DateAndTime.DaysPerSecond * value;
         
        }  

        public override float ComputeTotalCargoBulk()
        {
            return ToMissionStop.ComputeTotalCargoBulk();            
        }

        public override decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost)
        {
            decimal costForTravel = 0; 
            return costForTravel + ToMissionStop.ComputeTotalCost(parent, out boughtItemsCost, out soldItemsCost);
                            
        }

        public override MissionAction CreateMissionAction(Mission mission)
        {
            return new TravelAction(mission, this);
        }


        #region ISnapshot

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.Route = sn.DoInt64Nullable(Route);
            this.UsesAirRoute = sn.DoBoolNullable(UsesAirRoute);
          //  this.AirRouteDistance = sn.DoDoubleNullable(AirRouteDistance);

            //snapshotRoute = sn.SnapshotID<Route, RouteID>(Route);
                      
            this.snapshotToMissionStop = (MissionStopTemplateID)sn.SnapshotID<MissionStopTemplate, MissionStopTemplateID>(ToMissionStop);
            this.Distance = sn.DoDouble(Distance);
            this.Bearing = sn.DoDoubleNullable(Bearing);

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


        public override void LoadPostProcess(Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            sn.RegisterLoadPostProcessCall(this);

            ToMissionStop = (MissionStopTemplate)LookUp<MissionStopTemplate, MissionStopTemplateID>.FindByID(snapshotToMissionStop);

           // Route = LookUp<Route, RouteID>.FindByID(snapshotRoute);
        }

        #endregion

        
    }
}
