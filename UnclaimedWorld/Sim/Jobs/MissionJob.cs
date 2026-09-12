using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// should keep the data that is in constant use along the whole trip. Cargo should be tracked by Mission(Stop?).
    /// </summary>
    public class MissionJob: Job
    {
        public Dictionary<EntityType, List<EntityID>> Vehicles;


        /// <summary>
        /// HACK HACK
        /// </summary>
       /* public List<EntityID> DelayedOwnershipChangeList;
        public OwnerID? DelayedBuyerID;
        */

       

        public MissionJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public MissionJob(EntityGroup entityGroup, /*Priority priority,*/ Dictionary<EntityType, List<EntityID>> vehicles)
            : base(entityGroup) //, priority)
        {
            this.Vehicles = vehicles;

            // assign the vehicle:
            if (Vehicles != null)
            {
                IKnownEntityData vehicleData;

                foreach (var item in Vehicles)
                {
                    foreach (var vehicle in item.Value)
                    {
                        entityGroup.GetKnownData(vehicle, out vehicleData);
                        if (vehicleData != null)
                        {
                            vehicleData.AssignedToJob = this.ID;
                        }
                        else
                        {
                            Destroy(true); // not sure if we need to do this. will it ever happen?
                        }
                    }
                }
            }

            ComputeJobType();
            SetDefaultPriority(entityGroup);

        }


        public override Vector3? GetCircaLocation()
        {         
            return null;
        }

        /// <summary>
        /// km/day
        /// 
        /// perhaps we could include extra factors here, depending on skill, luck and condition...
        /// 
        /// there is also GetEstimatedTravelSpeed in MissionTemplate
        /// </summary>
        /// <returns></returns>
        public double GetRealizedTravelSpeed()
        {
            float slowestSpeed = 1000000f;
            float currentSpeed;
            if (Vehicles != null)
            {
                foreach (var item in Vehicles)
                {
                    foreach (var vehicleID in item.Value)
                    {
                        Entity vehicle = Entity.FindByID(vehicleID);
                        if (vehicle != null)
                        {
                            currentSpeed = ((VehicleContainerType)vehicle.EntityType.ContainerType).AverageOverlandTravelSpeed;
                            if (currentSpeed < slowestSpeed)
                            {
                                slowestSpeed = currentSpeed;
                            }
                        }
                    }
                }

                return slowestSpeed;
            }
            else
            {
                return GameData.Instance.Constants.AverageOverlandSpeedOnFoot;
            }           
        }


        public void IterateVehicles(Action<Entity> function)
        {
            if (Vehicles != null)
            {
                foreach (var item in Vehicles)
                {
                    foreach (var vehicleID in item.Value)
                    {
                        Entity vehicle = Entity.FindByID(vehicleID);
                        if (vehicle != null)
                        {
                            function(vehicle);
                        }
                    }
                }
            }
        }

        public void IterateVehicleContents(Action<Entity> function)
        {
            if (Vehicles != null)
            {
                foreach (var item in Vehicles)
                {
                    foreach (var vehicleID in item.Value)
                    {
                        Entity vehicle = Entity.FindByID(vehicleID);
                        if (vehicle != null)
                        {
                            vehicle.Contains.IterateContained(e => function(e));                           
                        }
                    }
                }
            }
        }


        public override void Destroy(bool cancelTakers, Entity entityToExclude = null)
        {
            JobID thisID = ID; // save before destroying it...

            base.Destroy(cancelTakers, entityToExclude);


            if (Vehicles != null)
            {
                IKnownEntityData vehicleData;
                EntityGroup entityGroup;
                if (ResolveOwner(out entityGroup))
                {
                    foreach (var item in Vehicles)
                    {
                        foreach (var vehicle in item.Value)
                        {
                            entityGroup.GetKnownData(vehicle, out vehicleData);
                            if (vehicleData != null)
                            {
                                if (vehicleData.AssignedToJob == thisID)
                                {
                                    vehicleData.AssignedToJob = null;
                                }
                            }
                        }
                    }
                }

                Vehicles.Clear();
            }
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            //Crew = sn.DoList(Crew); // use TakenBy
            Vehicles = sn.DoMultiMap(Vehicles);
          
            /*DelayedOwnershipChangeList = sn.DoList(DelayedOwnershipChangeList);
            DelayedBuyerID = sn.DoEnumNullable(DelayedBuyerID);
            */

            return this;

        }
    }
}
