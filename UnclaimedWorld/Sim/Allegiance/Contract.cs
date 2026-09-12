using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland.Missions;

namespace UWGame.SimSide.Allegiances
{
   /* public enum ContractOLDID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }


    /// <summary>
    /// a contract between 2 allegiances for exchange of goods, or aid...
    /// could be for supplies, or for payment...
    /// 
    /// DELETE THIS???
    /// </summary>
    public class ContractOLD: ISnapshot, ILookUp<ContractOLD, ContractOLDID>
    {
        //public enum SupplyTypes { Food, FoodAndGear, ConstructionMaterials };
       // public Dictionary<string, List<EntityType>> SupplyOptions;

        public Allegiance AllegianceA;
        AllegianceID snapshotA;
        public Allegiance AllegianceB;
        AllegianceID snapshotB;

        /// <summary>
        /// allegiance A must provide wares from this list
        /// </summary>
        public List<EntityType> SupplyOptionsA;

        /// <summary>
        /// allegiance A must provide this amount each interval
        /// </summary>
        public float BulkPerIntervalA;

        /// <summary>
        /// allegiance B must provide wares from this list (can be empty)
        /// </summary>
        public List<EntityType> SupplyOptionsB;

        /// <summary>
        /// allegiance B must provide this amount each interval
        /// </summary>
        public float? BulkPerIntervalB;

        public float IntervalInDays; // years?

        private bool isFulfilled;

        public DateAndTime.TimeDateYear ActiveFromDate;

        public DateAndTime.TimeDateYear ExpirationDate;

        private DateAndTime.TimeDateYear nextDelivery;

        public Mission Mission;
        MissionID? snapshotTransport;

        public ContractOLD()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();
            }
        }


        public bool IsContractFulfilledForThisInterval()
        {
            return isFulfilled;
        }


        public List<EntityType> GetSupplyOptions(AllegianceID id)
        {

            if (AllegianceA.ID == id)
                return SupplyOptionsA;

            if (AllegianceB.ID == id)
                return SupplyOptionsB;

            return null;

        }

        public float GetBulkPerInterval(AllegianceID id)
        {
            if (AllegianceA.ID == id)
                return BulkPerIntervalA;

            if (AllegianceB.ID == id)
                return (float)BulkPerIntervalB;

            return 0;
        }


        public static ContractOLD CreateFromContractData(ContractData contractData)
        {
            
                return null;
           // }
        }


        public DateAndTime.TimeDateYear GetNextDeliveryTime()
        {
            //Calculates time for next contract delivery if the current delivery has arrives
            
            nextDelivery = ActiveFromDate;

            while (DateAndTime.CompareDates(The.Sim.DateAndTime.CurrentTimeDateYear, nextDelivery) == 1 && isFulfilled)
            {
                //Add onto the last delivery time
                nextDelivery.AddTime(IntervalInDays);

                //Once we are above current time, set isFullfilled to false
                if (DateAndTime.CompareDates(The.Sim.DateAndTime.CurrentTimeDateYear, nextDelivery) == -1)
                {
                    isFulfilled = false;
                }
            }
           
       
            
            return nextDelivery;
        }

        public void Fullfill()
        {
            Mission = null;
            isFulfilled = true;
        }

        public void AddSupplyOption(AllegianceID id, EntityType entityType)
        {
            if (AllegianceA.ID == id)
                Common.AddToList(ref SupplyOptionsA, entityType);

            if (AllegianceB.ID == id)
                Common.AddToList(ref SupplyOptionsB, entityType);
        }

        public void RemoveSupplyOption(AllegianceID id, EntityType entityType){
            if (AllegianceA.ID == id)
                SupplyOptionsA.Remove(entityType);

            if (AllegianceB.ID == id)
                SupplyOptionsB.Remove(entityType);
        }

        public void Destroy()
        {
           /* The.Sim.World.GetRelationBetweenAllegianceIDs(AllegianceA.ID, AllegianceB.ID).Contracts.Remove(this);

            if (Mission != null)
            {
                Mission.Destroy(); // perhaps this should inform the transport entity to find a different destination..?
            }

            RemoveIDEntry();
        }

        #region ILookup

        private ContractOLDID id = ContractOLDID.Invalid;
        static ContractOLDID IDCounter = ContractOLDID.First;

        public ContractOLDID ID
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

        public ContractOLDID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ContractOLDID.Max)
            {
                throw new Exception("Astounding, ContractID just exceeded 64 bits. Something seriously wrong has happened.");
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
            if (ID != ContractOLDID.Invalid)
                LookUp<ContractOLD, ContractOLDID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = ContractOLDID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<ContractOLD, ContractOLDID>.Remove(this);
        }

        void ILookUp<ContractOLD, ContractOLDID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ContractOLDID.First;
        }

        #endregion

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = sn.DoEnum(id);
            IDCounter = sn.DoEnum(IDCounter);

            this.snapshotA = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(AllegianceA);
            this.snapshotB = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(AllegianceB);

            this.ActiveFromDate = sn.DoTimeDateYear(ActiveFromDate);
            this.BulkPerIntervalA = sn.DoFloat(BulkPerIntervalA);
            this.BulkPerIntervalB = sn.DoFloatNullable(BulkPerIntervalB);
            this.ExpirationDate = sn.DoTimeDateYear(ExpirationDate);
            this.IntervalInDays = sn.DoFloat(IntervalInDays);
            this.isFulfilled = sn.DoBool(isFulfilled);
            this.nextDelivery = sn.DoTimeDateYear(nextDelivery);
            this.SupplyOptionsA = sn.DoList(SupplyOptionsA);
            this.SupplyOptionsB = sn.DoList(SupplyOptionsB);
            this.snapshotTransport = sn.SnapshotID<Mission, MissionID>(Mission);

            sn.Ignore(AllegianceA);
            sn.Ignore(AllegianceB);

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

            AllegianceA = LookUp<Allegiance, AllegianceID>.FindByID(snapshotA);
            AllegianceB = LookUp<Allegiance, AllegianceID>.FindByID(snapshotB);

            Mission = LookUp<Mission, MissionID>.FindByID(snapshotTransport);
        }

        #endregion
    }*/
}
