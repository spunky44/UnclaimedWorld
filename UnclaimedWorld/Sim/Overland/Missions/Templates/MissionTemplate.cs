using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities;
using System.Xml.Serialization;
using System.Diagnostics;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Overland.Missions.Templates
{
    
    public enum MissionTemplateID : long
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }

    public enum FieldError { Transport, Start, Destination }

    /// <summary>
    /// an xml-serializable class that can be used to instantiate runtime Missions.
    /// 
    /// Perhaps the player can create a type and reuse it for different missions..? Trade routes?
    /// 
    /// should this be immutable? or only when in use..?
    /// 
    /// could a scripter also define these for the scenario? not if AllegianceID, SiteID, EntityID etc. is used in TravelLocation... Perhaps a MissionTemplateData is needed?
    /// </summary>
    public class MissionTemplate : ISnapshot, ILookUp<MissionTemplate, MissionTemplateID>
    {
        
        public long Allegiance;

 
        /// <summary>
        /// is this the owner of the mission??? the group that pays...
        /// </summary>
        public long OwnerID;

        /// <summary>
        /// contains hire info too
        /// </summary>
        public TransportationTemplate TransportationType;


      //  public bool TransportsAreHired = false;

        /// <summary>
        /// OwnerID?
        /// who pays for the mission / transportation costs. Only filled if we are using hired vehicles/crew
        /// 
        /// Note that each BuyAction can have its own different BuyerID
        /// </summary>
       // public long? Payer;


        /// <summary>
        /// the starting point for a series of MissionStopTemplates - linked by TravelActions
        /// </summary>
        public MissionStopTemplate StartMissionStopTemplate;
        MissionStopTemplateID snapshotStartMissionStopTemplate;
             


        public MissionTemplate(AllegianceID allegiance, OwnerID owner, bool createID)
        {
            this.Allegiance = (long)allegiance;
            this.OwnerID = (long)owner;

            if (createID)
            {
                AddToLookup();
            }
        }

        public MissionTemplate()
        {
            // xml uses this...         
        }

        /// <summary>
        /// adds another destination to the end of the chain
        /// </summary>
        /// <param name="destination"></param>
        public void AddMissionLocation(MissionStopTemplate destination)
        {
            StartMissionStopTemplate.AddMissionLocation(destination, 1);

        }

        public Queue<MissionActionTemplate> GetActionsAtLocation(MissionStopTemplate location)
        {
            return StartMissionStopTemplate.GetActionsAtLocation(location); // locationActions[location];

        }


        public void AddAction(MissionActionTemplate action, MissionStopTemplate location)
        {
            // insert the action at the correct place:
            StartMissionStopTemplate.AddActionAtLocation(action, location);

        }

        public bool SelectRoutes()
        {
            if (TransportationType != null)
            {
                EntityType transport = TransportationType.GetMainTransportation();

                return StartMissionStopTemplate.SelectRoute(this, transport);
            }

            return true;
        }


        /// <summary>
        /// instead of storing a reference...
        /// </summary>
        /// <returns></returns>
        public PassengerListTemplate GetPassengerListTemplate()
        {
            if (StartMissionStopTemplate != null)
                return StartMissionStopTemplate.GetPassengerListTemplate();

            return null;
        }
       
        public bool ContainsLocation(MissionStopTemplate m)
        {
            if (StartMissionStopTemplate == m)
                return true;

            if (StartMissionStopTemplate.TravelAction != null)
            {
                return StartMissionStopTemplate.TravelAction.ToMissionStop.ContainsLocation(m);
            }

            return false;
        }

        public void Destroy()
        {
            RemoveIDEntry();

            if (StartMissionStopTemplate != null)
            {
                StartMissionStopTemplate.Destroy();
            }
        }

        /// <summary>
        /// for missions that involve buy/sell, this method returns (one of?) the home expeditions on the route.
        /// The home expedition is any expedition belonging to the mission's owning allegiance.
        /// </summary>
        /// <returns></returns>
        public Expedition GetHomeExpedition()
        {
            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)Allegiance);

            if (StartMissionStopTemplate != null)
                return StartMissionStopTemplate.GetAllegianceExpeditionOnRoute(thisAllegiance);

            return null;
        }


        public Expedition GetExpeditionMatchingPredicate(Predicate<Expedition> matches)
        {
            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)Allegiance);

            if (StartMissionStopTemplate != null)
                return StartMissionStopTemplate.GetExpeditionOnRoute(thisAllegiance.SharedKnowledge, matches);

            return null;
        }

        /// <summary>
        /// returns an expedition on the route
        /// it can be used as a trading partner...
        /// </summary>
        /// <param name="expedition"></param>
        /// <returns></returns>
        public bool GetOwner(SharedKnowledge sharedKnowledge, Predicate<EntityGroup> matchesPredicate, //  Allegiance allegiance, 
            out EntityGroup otherOwner) 
        {
            if (StartMissionStopTemplate != null)
            {
                return StartMissionStopTemplate.GetOwner(sharedKnowledge, matchesPredicate /* allegiance*/, out otherOwner); 
            }

            otherOwner = null;
            return true;
        }

        public float GetTotalCargoCapacity()
        {
            if (TransportationType != null)
            {
                EntityType vehicleType = TransportationType.GetMainTransportation();
                return  ((VehicleContainerType)vehicleType.ContainerType).ItemStorageType.GetTotalCapacity();
            }

            return 0f;
        }


        public double GetTotalDistance()
        {
            if (StartMissionStopTemplate != null)
            {
                return StartMissionStopTemplate.GetTotalDistance(0d);
            }

            return 0;
        }

        public float ComputeTotalCargoBulk()
        {
            float total = 0;
            if (StartMissionStopTemplate != null)
            {
                total += StartMissionStopTemplate.ComputeTotalCargoBulk();
            }

            return total;

        }

        public decimal ComputeTotalCost(out decimal transportCost, out decimal boughtItemsCost, out decimal soldItemsCost)
        {
            decimal total = 0;
            boughtItemsCost = 0;
            soldItemsCost = 0;

            if (StartMissionStopTemplate != null)
            {
                total += StartMissionStopTemplate.ComputeTotalCost(this, out boughtItemsCost, out soldItemsCost);
            }

            decimal startFee, totalDistanceCost, costPerKilometer;
            transportCost = ComputeTransportationCost(out startFee, out totalDistanceCost, out costPerKilometer);
            total += transportCost;

            return total;
        }

        /// <summary>
        /// compute cost of hired vehicles
        /// </summary>
        /// <returns></returns>
        public decimal ComputeTransportationCost(out decimal startFee, out decimal totalDistanceCost, out decimal costPerKilometer)
        {
            if (TransportationType != null)
            {
                return TransportationType.ComputeTransportationCost(this, out startFee, out totalDistanceCost, out costPerKilometer);
            }

            startFee = 0;
            totalDistanceCost = 0;
            costPerKilometer = 0;

            return 0;       
        }


        public bool Validate(ref List<string> errors, ref FieldError? errorFieldCode)
        {
            if (StartMissionStopTemplate == null)
            {
                Common.AddToList(ref errors, "A starting location has not been selected");
                errorFieldCode = FieldError.Start;
                return false;
            }
            if (StartMissionStopTemplate.TravelAction == null)
            {
                Common.AddToList(ref errors, "A destination has not been selected");
                errorFieldCode = FieldError.Destination; // 1;
                return false;
            }

            

            // TODO: validate routes vs. vehicle type also
            if (TransportationType == null
                    || TransportationType.Vehicles == null)
            {
                Common.AddToList(ref errors, "A vehicle has not been selected.");
                errorFieldCode = FieldError.Transport; // 2;
                return false;
            }

            // validate actions (content)
            bool hasMeaning = false;
            if (!StartMissionStopTemplate.ValidateMissionActions(this, ref hasMeaning, ref errors))
            {
                if (!hasMeaning)
                {
                    Common.AddToList(ref errors, "The mission has no purpose, try adding some actions.");                   
                }

                return false;
            }

            /*
            if (!hasMeaning)
            {
                Common.AddToList(ref errors, "The mission has no purpose, try adding some actions.");
              //  errorFieldCode = 3;
                return false;
            }*/

            float totalCargo = ComputeTotalCargoBulk();

            float totalCapacity = GetTotalCargoCapacity();

            if (totalCargo > totalCapacity)
            {
                Common.AddToList(ref errors, "Too much cargo.");
              //  errorFieldCode = 3;
                return false;
            }

            decimal totalBoughtCost, totalSoldCost, transportCost;
            decimal totalMissionCost = ComputeTotalCost(out transportCost, out totalBoughtCost, out totalSoldCost);

            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)Allegiance);

            if (totalMissionCost > thisAllegiance.TradeCredits)
            {
                Common.AddToList(ref errors, "The total cost is more than we can afford.");
                //  errorFieldCode = 3;
                return false;
            }


            return true;
        }



        public bool ValidateMissionStops()
        {         

            return StartMissionStopTemplate.ValidateMissionStops(this);            

        }



        /// <summary>
        /// happens on START RUN or Command.Execute()
        /// </summary>
        public void AssignIDs()
        {
            AddToLookup();
            StartMissionStopTemplate.AssignIDs();
        }

        #region ILookup

        private MissionTemplateID id = MissionTemplateID.Invalid;
        static MissionTemplateID IDCounter = MissionTemplateID.First;

        /// <summary>
        /// make sure we don't attempt to xmlserialize this. It should not exist before the Command has executed. That way, we can cancel out of the dialog without affecting the Sim
        /// 
        /// The ID will get created manually in the CreateMissionTemplate command when executing.
        /// </summary>
        [XmlIgnore]
        public MissionTemplateID ID
        {
            get
            {
                return id;
            }

            private set
            {
                id = value;
            }
        }

        public MissionTemplateID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= MissionTemplateID.Max)
            {
                throw new Exception("Astounding, MissionTemplateID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }


        public int LoadPostProcessOrder
        {
            get
            {
                return 0;
            }
        }


        public void AddToLookup()
        {
            ID = GetUniqueID();
            if (ID != MissionTemplateID.Invalid)
                LookUp<MissionTemplate, MissionTemplateID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = MissionTemplateID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<MissionTemplate, MissionTemplateID>.Remove(this);
        }

        void ILookUp<MissionTemplate, MissionTemplateID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = MissionTemplateID.First;
        }

        void ILookUp<MissionTemplate, MissionTemplateID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<MissionTemplate, MissionTemplateID>.Create();
        }


        #endregion



        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = sn.DoEnum(id);
            IDCounter = sn.DoEnum(IDCounter);

            Allegiance = sn.DoInt64(Allegiance);
            OwnerID = sn.DoInt64(OwnerID);

           // this.TransportsAreHired = sn.DoBool(TransportsAreHired);
            //this.Payer = sn.DoInt64Nullable(Payer);
            this.TransportationType = (TransportationTemplate)sn.DoISnapshot(TransportationType);

            this.snapshotStartMissionStopTemplate = (MissionStopTemplateID)sn.SnapshotID<MissionStopTemplate, MissionStopTemplateID>(StartMissionStopTemplate); // (MissionStopTemplate)sn.DoISnapshot(StartMissionStopTemplate);


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

            StartMissionStopTemplate = LookUp<MissionStopTemplate, MissionStopTemplateID>.FindByID(snapshotStartMissionStopTemplate);
        }

        #endregion




    }
}
