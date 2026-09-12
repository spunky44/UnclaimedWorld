using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Overland.Missions.Templates
{
    /// <summary>
    /// hired crew also???
    /// </summary>
    public class TransportationTemplate: ISnapshot
    {

        public long? HiredFromOwner;


        /// <summary>
        /// type / amount pairs
        /// null if on foot???
        /// 
        /// Tuples cannot be xmlserialized!
        /// </summary>
        public List<Pair<string, int>> Vehicles;

        /// <summary>
        /// km/day
        /// 
        /// there is also GetRealizedTravelSpeed() in MissionJob!
        /// </summary>
        /// <returns></returns>
        public double GetEstimatedTravelSpeed()
        {
            float slowestSpeed = 1000000f;
            float currentSpeed;
            if (Vehicles != null)
            {
                foreach (var item in Vehicles)
                {
                    EntityType vehicle = GameData.Instance.AllEntityTypes[item.First];

                    currentSpeed = ((VehicleContainerType)vehicle.ContainerType).AverageOverlandTravelSpeed;
                    if (currentSpeed < slowestSpeed)
                    {
                        slowestSpeed = currentSpeed;
                    }                        
                   
                }

                return slowestSpeed;
            }
            else
            {
                return GameData.Instance.Constants.AverageOverlandSpeedOnFoot;
            }
        }

        public EntityType GetMainTransportation()
        {
            if (Vehicles != null
                && Vehicles.Count > 0)
            {
                string entityTypeKey = Vehicles[0].First;

                return GameData.Instance.AllEntityTypes[entityTypeKey];
            }

            return null;
        }

        public decimal ComputeTransportationCost(MissionTemplate parent, out decimal startFee, out decimal totalDistanceCost, out decimal costPerKilometer)
        {
            decimal total = 0;
            startFee = 0;
            totalDistanceCost = 0;
            costPerKilometer = 0;

            if (HiredFromOwner.HasValue) // TransportsAreHired) // Payer.HasValue) 
            {
                Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);

                if (thisAllegiance != null)
                {
                    /* Allegiance allegiance;
                     Expedition fromExpedition;
                     IKnownEntityData terminal;
                     Site site;*/

                    // get the expedition that we are hiring from:
                    IOwner hiredFrom = LookUpOwners.FindByID((OwnerID)HiredFromOwner.Value);

                    if (hiredFrom != null) //StartMissionStopTemplate.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out fromExpedition, out terminal))
                    {
                        double distance = parent.GetTotalDistance();

                        foreach (var vehicle in Vehicles)
                        {
                            int noOfVehicles = vehicle.Second;

                            decimal? pricePerKilometer;
                            decimal? price = hiredFrom.OwnedEntities.GetVehicleForHirePrice(GameData.Instance.AllEntityTypes[vehicle.First], out pricePerKilometer);

                            costPerKilometer += (decimal)((pricePerKilometer ?? 0) * noOfVehicles);

                            decimal thisFee = noOfVehicles * (price ?? 0);
                            decimal thisCostPerKilometer = noOfVehicles * (decimal)distance * (pricePerKilometer ?? 0);

                            //total += price.Value * vehicle.Second;
                            
                            startFee += thisFee;
                            totalDistanceCost += thisCostPerKilometer;

                           // total = total + this //(price.Value + (decimal)distance * (pricePerKilometer ?? 0)) * noOfVehicles;
                        }
                    }
                }
            }

            total = startFee + totalDistanceCost;

            return total;

        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Vehicles = sn.DoList(Vehicles);
            this.HiredFromOwner = sn.DoInt64Nullable(HiredFromOwner);

            return this;

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        [XmlIgnore]
        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}
