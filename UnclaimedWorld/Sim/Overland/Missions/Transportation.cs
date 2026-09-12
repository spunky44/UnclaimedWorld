using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Entities.Owners;

namespace UWGame.SimSide.Overland.Missions
{
    public class Transportation: ISnapshot
    {
        // move to Job???
      //  public List<EntityID> Vehicles;

      //  public List<EntityID> Crew; // moved to Job

        /// <summary>
        /// supplies for the crew and vehicles
        /// </summary>
        public List<EntityID> Supplies;

        public Mission mission;


        public Transportation(Mission mission) //, List<EntityID> vehicles)
        {
            this.mission = mission;
            //this.Vehicles = vehicles;
        }

        public Transportation()
        {

        }

        public void SetParentPostLoad(Mission parent)
        {
            this.mission = parent;
        }

        public void Update(GameTime gameTime)
        {
            // TODO: consume food here
        }


        public void StartMission()
        {
            // pay for the transport costs (if hired)
            if (mission.MissionTemplate.TransportationType.HiredFromOwner.HasValue)
            {
                OwnerID buyerID, sellerID;
                buyerID = (OwnerID)mission.MissionTemplate.OwnerID;
                sellerID = (OwnerID)mission.MissionTemplate.TransportationType.HiredFromOwner.Value; // HACK!!! TODO: support non-hired missions also.

                IOwner buyer = LookUpOwners.FindByID(buyerID);
                IOwner seller = LookUpOwners.FindByID(sellerID);

                decimal startFee, totalDistanceCost, costPerKilometer;
                decimal total = mission.MissionTemplate.ComputeTransportationCost(out startFee, out totalDistanceCost, out costPerKilometer);

                EntityGroup.MakeTradeCreditsTransaction(buyer, seller, total);
            }
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            //Vehicles = sn.DoList(Vehicles);
            //Crew = sn.DoList(Crew);


            sn.Postpone(Supplies);

            sn.Ignore(mission);

            //Supplies = sn.DoList(Supplies);

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

        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}
