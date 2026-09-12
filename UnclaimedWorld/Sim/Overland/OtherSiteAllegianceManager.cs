using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.Control.Replays;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland.Missions;

namespace UWGame.SimSide.Overland
{
    /// <summary>
    /// simulates the manager of another allegiance. 
    /// iterate over contracts - decide if shipment should be sent
    /// 
    /// The amount sent depends on the relation between the allegiances.
    /// </summary>
    public class OtherSiteAllegianceManager: ISnapshot
    {      

       
        private Mission transportWaitingToDepart;
        MissionID? snapshotWaitingTransport;

        public Allegiance Parent;
        AllegianceID snapshotParent;

        Regulator missionRegulator;

       
        private List<EntityID> waitingImmigrants = new List<EntityID>();

     //   private List<Transport> transportsToRemove;

        public OtherSiteAllegianceManager()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");     
        }

        public OtherSiteAllegianceManager(Allegiance allegiance)
        {
            Parent = allegiance;

            CreateRegulators();
        }

        void CreateRegulators()
        {
            missionRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.1, "OtherSiteAllegianceManager");

        }

        // look at contracts - make decision to send transport
        // decide on cargo
        // if it is time to send supplies, book a transport
        // for each contract:
        // if no transport has been sent this interval and no transport has been booked
        // book transport x time in advance to simulate loading etc
        // create the load (produce items).
        // any immigrants will have to wait until the next transport is due

        public void Update(GameTime gameTime)
        {
            // TODO: still use contracts?
            return; 

            if (waitingImmigrants.Count > 0)
            {
                Entity entity = Entity.FindByID(waitingImmigrants[0]);
               
            }

           
            if (missionRegulator.IsReady())
            {
              /*  List<ContractOLD> contracts = The.Sim.World.GetContractsFromAllegiance(Parent.ID); // get all contracts

                if (contracts != null)
                {
                    foreach (ContractOLD contract in contracts)
                    {
                        DateAndTime.TimeDateYear nextDelivery = contract.GetNextDeliveryTime();

                        //Looks for a non-fulfilled contract, then makes it our current contract and books a transport.
                        if (!contract.IsContractFulfilledForThisInterval() 
                            //&& transportWaitingToDepart == null // ?? do we need a reference here??
                            && contract.Mission == null)
                        {                           
                            StartTradeMission(contract, nextDelivery);
                        }
                    }
                }

                //Check wether or not we past the delivery time. If we are, send the transport.
                if (transportWaitingToDepart != null)
                {

                    if (DateAndTime.CompareDates(The.Sim.DateAndTime.CurrentTimeDateYear, transportWaitingToDepart.Departure) == 1)
                    {
                        transportWaitingToDepart.TakeOff();

                       
                        //currentContract.AssignedTransport = transportWaitingToDepart;
                        transportWaitingToDepart = null;
                    }
                }*/

            }

        }


        private bool IsContractFulfilledForThisInterval()
        {
            return true;
        }

        /// <summary>
        /// the transport will be booked some time in advance. Any immigrants that appear, or are waiting, will have a chance to get on it.
        /// </summary>
        /*   private void StartTradeMission(ContractOLD contract, DateAndTime.TimeDateYear departureTime)
        {

            // TODO

            // create the next transport. assign cargo (supplies) and any waiting immigrants.
          Mission mission = new Mission(Parent);
            transportWaitingToDepart = mission;

            mission.Contract = contract.ID;
            contract.Mission = mission;


            if (contract.AllegianceA.ID == Parent.ID)
            {
                mission.DestinationAllegiance = contract.AllegianceB.ID;
            }
            else
            {
                mission.DestinationAllegiance = contract.AllegianceA.ID;
            }

            mission.Cargo = CreateCargo(contract);

            //WaitingImmigrants = new List<EntityID> {};

            if (waitingImmigrants != null)
            {
                mission.Immigrants = waitingImmigrants;
                waitingImmigrants = new List<EntityID>();
            }

            mission.StartMissionLocation.Actions.Enqueue(new LoadAction(mission));


            TravelLocation from = new TravelLocation();
            from.SetAllegiance(contract.AllegianceA);
            from.Expedition = contract.AllegianceA.GetFirstExpedition().ID;

            TravelLocation to = new TravelLocation();
            to.SetAllegiance(contract.AllegianceB);
            to.Expedition = contract.AllegianceB.GetFirstExpedition().ID;

            mission.AddTravelLocation(to);

           
           mission.Actions.Actions.Enqueue(new TravelAction(mission.Actions, from, to));

            mission.Actions.Actions.Enqueue(new UnloadAction(mission.Actions));

        }*/

        
        /*
        /// <summary>
        /// Creates a list of Entities by ID from the supply options in the contract.
        /// </summary>
        private List<EntityID> CreateCargo(ContractOLD contract)
        {
            List<EntityType> supplyOptions = contract.GetSupplyOptions(Parent.ID);
            List<EntityID> cargo = new List<EntityID>();

            float bulkLimit = contract.GetBulkPerInterval(Parent.ID);
            float currentCargoBulk = 0;

            while (currentCargoBulk < bulkLimit)
            {

                Entity cargoItemEntity = new Entity(Common.GetRandomListMember(supplyOptions, The.Sim.GameplayRandomGenerator));

                cargoItemEntity.Initialize(Parent.Site, Parent, null);
                currentCargoBulk += cargoItemEntity.Bulk;

                if (currentCargoBulk < bulkLimit)
                {
                    cargo.Add(cargoItemEntity.EntityID);
                }

              
            }

            return cargo;

        }*/


        public void AddEmigrantToQueue(EntityID emigrantID)
        {
            Common.AddToList(ref waitingImmigrants, emigrantID);
        }

        public int GetAmountOfWaitingEmigrants()
        {
            if (waitingImmigrants != null)
                return waitingImmigrants.Count;
            else return 0;
        }

       /* public void RemoveTransportWaitingToDepart()
        {
            transportWaitingToDepart = null;
        }*/


        #region ISnapshot

              
        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.snapshotParent = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(Parent);
                                   

            snapshotWaitingTransport = sn.SnapshotID<Mission, MissionID>(transportWaitingToDepart);
            this.waitingImmigrants = sn.DoList(waitingImmigrants);
            
            
            return this;

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
           return version;
        }

        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            Parent = LookUp<Allegiance, AllegianceID>.FindByID(snapshotParent);


            transportWaitingToDepart = LookUp<Mission, MissionID>.FindByID(snapshotWaitingTransport);

            CreateRegulators();
        }

        #endregion
    }
}
