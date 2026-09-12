using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Snapshots;
using System.Xml.Serialization;

namespace UWGame.SimSide.Overland.Missions.Templates
{
    public enum ContractTemplateID : long
    {
        Invalid = long.MaxValue,
        Max = Invalid,
        First = 1
    }

    /// <summary>
    /// don't store the price since it can change before we commit...
    /// </summary>
    public class ContractTemplate : ISnapshot, ILookUp<ContractTemplate, ContractTemplateID>
    {
       
       // public SerializableDictionary<string, int> Goods; // make it serializable

        public SerializableDictionary<string, List<long>> Entities; // make it serializable
      //  public List<long> Entities;


        /// <summary>
        /// OwnerID for who will be billed and will receive ownership of the bought goods
        /// This should probably always be the payer/owner of the mission
        /// </summary>
        public long BuyerID;

        /// <summary>
        /// OwnerID for who will be paid and give away the goods
        /// This should probably always be the owner of the terminal
        /// </summary>
        public long SellerID;

        public void AssignIDs()
        {
            AddToLookup();
        }

        public void Destroy()
        {
            RemoveIDEntry();
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            id = sn.DoEnum(id);
            IDCounter = (ContractTemplateID)sn.DoEnum(IDCounter);

            Entities = sn.DoSerializableMultiMap(Entities);
            //Goods = sn.DoSerializableDictionary(Goods);
            BuyerID = sn.DoInt64(BuyerID);
            SellerID = sn.DoInt64(SellerID);

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


        #region ILookup

        private ContractTemplateID id = ContractTemplateID.Invalid;
        static ContractTemplateID IDCounter = ContractTemplateID.First;





        /// <summary>
        /// make sure we don't attempt to xmlserialize this. It should not exist before the Command has executed. That way, we can cancel out of the dialog without affecting the Sim
        /// 
        /// The ID will get created manually in the CreateMissionTemplate command when executing.
        /// </summary>
        [XmlIgnore]
        public ContractTemplateID ID
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

        public ContractTemplateID GetUniqueID()
        {
            IDCounter++;
            if (IDCounter >= ContractTemplateID.Max)
            {
                throw new Exception("Astounding, ContractTemplateID just exceeded 64 bits. Something seriously wrong has happened.");
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
            if (ID != ContractTemplateID.Invalid)
                LookUp<ContractTemplate, ContractTemplateID>.Add(ID, this);
        }

        public void SetInvalid()
        {
            id = ContractTemplateID.Invalid;
        }

        public void RemoveIDEntry()
        {
            LookUp<ContractTemplate, ContractTemplateID>.Remove(this);
        }

        void ILookUp<ContractTemplate, ContractTemplateID>.ResetIDCounter() // interface method - does nothing...
        {
        }

        public static void ResetIDCounter() // called by invoke, do not remove
        {
            IDCounter = ContractTemplateID.First;
        }


        void ILookUp<ContractTemplate, ContractTemplateID>.CreateLookupCollection() // interface method - does nothing...
        {
        }

        public static void CreateLookupCollection()
        {
            LookUp<ContractTemplate, ContractTemplateID>.Create();
        }


        #endregion

    }
}
