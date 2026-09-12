using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities.Owners;

namespace UWGame.SimSide.Overland.Missions
{
    /// <summary>
    /// Actually a receipt. Corresponds to a ContractTemplate in BuyActionTemplate.
    /// 
    /// here we store contract data that we might need in case we have to refund sales.
    /// </summary>
    public class Contract: ISnapshot
    {
        public ContractTemplate ContractTemplate;
        ContractTemplateID snapshotContractTemplate;


        /// <summary>
        /// the amount that has been paid
        /// </summary>
        public decimal PaidAmount;

        /// <summary>
        /// the items/entities that have been bought. Stored here to make it easier to revert. Can be null.
        /// </summary>
        public List<EntityID> TransferredEntities;


      

        public void Revert()
        {
          
            IOwner buyer = LookUpOwners.FindByID((OwnerID)ContractTemplate.BuyerID);
            IOwner seller = LookUpOwners.FindByID((OwnerID)ContractTemplate.SellerID);

            if (buyer != null && seller != null)
            {
                EntityGroup.MakeTradeCreditsTransaction(buyer, seller, PaidAmount);

                if (TransferredEntities != null)
                {
                    foreach (var item in TransferredEntities)
                    {
                        Entity entity = Entity.FindByID(item);
                        if (entity != null)
                        {
                            entity.ChangeOwnership(seller);

                            if (buyer.OwnedEntities.TradeManager != null)
                            {
                                buyer.OwnedEntities.TradeManager.Buy(entity.EntityType, -1);
                            }
                        }
                    }
                }
            }
        }

       



        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            PaidAmount = sn.DoDecimal(PaidAmount);

            snapshotContractTemplate = (ContractTemplateID)sn.SnapshotID<ContractTemplate, ContractTemplateID>(ContractTemplate);
            TransferredEntities = sn.DoList(TransferredEntities);

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


            ContractTemplate = LookUp<ContractTemplate, ContractTemplateID>.FindByID(snapshotContractTemplate);

        }

        #endregion
    }
}
