using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances
{
     public enum AllegianceRelationID : ulong
    {
        Invalid = ulong.MaxValue,
        Max = Invalid,
        First = 1
    }


    /// <summary>
    /// defines the current relations between 2 human? allegiances
    /// </summary>
    public class AllegianceRelation: ISnapshot, ILookUp<AllegianceRelation, AllegianceRelationID>
    {

        public Allegiance AllegianceA;
        AllegianceID snapshotA;
        public Allegiance AllegianceB;
        AllegianceID snapshotB;

        /// <summary>
        /// 0 - 1: 0 war/hate, 1: alliance?
        /// </summary>
        public float Relation;


        public bool AcceptImigration;
        public bool AllowTrade;
        public bool AllowPassage;


      //  public List<ContractOLD> Contracts;
      //  List<ContractOLDID> snapshotContracts;

        public AllegianceRelation()
        {
            if (!Snapshotter.IsSnapshotting)
            {
                AddToLookup();
            }
        }

        public static AllegianceRelation CreateRelationFromData(AllegianceRelationData allegianceRelationData)
        {

            AllegianceRelation allegianceRelation = new AllegianceRelation(){
              AllegianceA = The.Sim.World.GetAllegianceFromKey(allegianceRelationData.Allegiance1),
              AllegianceB = The.Sim.World.GetAllegianceFromKey(allegianceRelationData.Allegiance2),
              Relation = allegianceRelationData.Relation
            };

            if (allegianceRelation.AllegianceA == null || allegianceRelation.AllegianceB == null)
            {
                return null;
            }

            //Look for and deny relation between allegiances with a pre-excisting relation.
            if(The.Sim.World.GetRelationBetweenAllegianceIDs(allegianceRelation.AllegianceA.ID, allegianceRelation.AllegianceB.ID) != null)
            {
                return null;
            }

          //  allegianceRelation.Contracts = new List<ContractOLD>();

            List<AllegianceRelation> list = new List<AllegianceRelation>();

            //Add the same relation instance to both allegiances relation list
            The.Sim.World.Relations.TryGetValue(allegianceRelation.AllegianceA.ID, out list);

            if (list == null) list = new List<AllegianceRelation>();

            list.Add(allegianceRelation);
            The.Sim.World.Relations.Add(allegianceRelation.AllegianceA.ID, list);


            The.Sim.World.Relations.TryGetValue(allegianceRelation.AllegianceB.ID, out list);
            if (list == null) list = new List<AllegianceRelation> { };

            list.Add(allegianceRelation);
            The.Sim.World.Relations.Add(allegianceRelation.AllegianceB.ID, list);

            return allegianceRelation;

        }


        public void Destroy()
        {
            RemoveIDEntry();

          /*  ContractOLD contract;
            for (int i = Contracts.Count - 1; i >= 0; i--)
            {
                contract = Contracts[i];

                contract.Destroy(); // this call will remove the item from the list we are iterating
            }*/
                       

        }

        #region ILookup

        private AllegianceRelationID id = AllegianceRelationID.Invalid;
        static AllegianceRelationID IDCounter = AllegianceRelationID.First;

        public AllegianceRelationID ID
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

        public AllegianceRelationID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= AllegianceRelationID.Max)
            {
                throw new Exception("Astounding, AllegianceRelationID just exceeded 64 bits. Something seriously wrong has happened.");
            }

            return IDCounter;
        }

        public AllegianceRelationID SnapshotID(Snapshotter sn, AllegianceRelationID id)
        {
            return (AllegianceRelationID)sn.DoEnum(id);
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
            if (ID != AllegianceRelationID.Invalid)
                LookUp<AllegianceRelation, AllegianceRelationID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = AllegianceRelationID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<AllegianceRelation, AllegianceRelationID>.Remove(this);
        }

        void ILookUp<AllegianceRelation, AllegianceRelationID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = AllegianceRelationID.First;
        }

        void ILookUp<AllegianceRelation, AllegianceRelationID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<AllegianceRelation, AllegianceRelationID>.Create();
        }

        #endregion

        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = SnapshotID(sn, id);
            IDCounter = (AllegianceRelationID)sn.DoEnum(IDCounter);

          /*  if (Contracts != null)
            {
                snapshotContracts = Contracts.Select(c => c.ID).ToList();
            }

            this.snapshotContracts = sn.DoList(snapshotContracts);
            */
            this.snapshotA = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(AllegianceA);
            this.snapshotB = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(AllegianceB);

            this.Relation = sn.DoFloat(Relation);
            this.AcceptImigration = sn.DoBool(AcceptImigration);
            this.AllowPassage = sn.DoBool(AllowPassage);
            this.AllowTrade = sn.DoBool(AllowTrade);

          //  sn.Ignore(Contracts);

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

          /*  if (snapshotContracts != null)
            {
                Contracts = snapshotContracts.Select(c => LookUp<Contract, ContractID>.FindByID(c)).ToList();
                snapshotContracts = null;
            }*/

            AllegianceA = LookUp<Allegiance, AllegianceID>.FindByID(snapshotA);
            AllegianceB = LookUp<Allegiance, AllegianceID>.FindByID(snapshotB);
        }

        #endregion

    }
}
