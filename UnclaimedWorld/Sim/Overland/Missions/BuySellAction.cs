using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.AI;
using UWGame.SimSide.Communication;

namespace UWGame.SimSide.Overland.Missions
{
    /// <summary>
    /// it seems like buy/sell is the same operation, only with the viewpoints exchanged.
    /// 
    /// we can only buy/sell at the moment the mission starts if we are in communication.
    /// Otherwise, delay the transaction
    /// </summary>
    public class BuySellAction : MissionAction
    {
        public BuySellActionTemplate BuyActionType;
        private MissionActionTemplateID snapshotActionTemplateID;


        public bool TransactionIsFinished;
       

        public BuySellAction(Mission parent, BuySellActionTemplate buyActionType) //, decimal maxAmountToSpend, bool buyReducedAmountsIfNeeded) // Dictionary<EntityType, int> orders)
            : base(parent)
        {
            this.BuyActionType = buyActionType;
          //  this.BuyReducedAmountsIfNeeded = buyReducedAmountsIfNeeded;
          //  this.MaxAmountToSpend = maxAmountToSpend;

            //this.Orders = orders;
        }

      

        public BuySellAction()
        {
        }



        public override bool Update(Microsoft.Xna.Framework.GameTime elapsed)
        {
          

            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)this.parent.MissionTemplate.Allegiance);

            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;
            Site site;
            if (thisAllegiance != null && parent.CurrentLocation.Value.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                if (!TransactionIsFinished)
                {
                    bool transactionIsOK;
                    MakeTransaction(expedition, out transactionIsOK);

                    if (!transactionIsOK)
                    {
                        HandleFailedAction();
                        return true;
                    }
                }

                // load:
                LoadItems();

                
                return true;
            }
            else
            {
                HandleFailedAction();
                return true; // ??
            }
        }


        private void MakeTransaction(Expedition expedition, out bool isOK)
        {
            isOK = true;

            IOwner buyer = LookUpOwners.FindByID((OwnerID)BuyActionType.ContractTemplate.BuyerID);
            //     EntityGroup buyer = LookUp<EntityGroup, EntityGroupID>.FindByID(BuyActionType.BuyerID);

            if (buyer == null)
            {
                isOK = false;               
                return; 
            }

            List<Entity> boughtItems = null;
            decimal spentAmount;

         //   Dictionary<EntityType, int> order = BuyActionType.ContractTemplate.Goods.ToDictionary(k => GameData.Instance.AllEntityTypes[k.Key], k => k.Value);
            Dictionary<EntityType, List<EntityID>> order = BuyActionType.ContractTemplate.Entities.ToDictionary(k => GameData.Instance.AllEntityTypes[k.Key], k => k.Value.Select(e => (EntityID)e).ToList());

            // buy:
            // TODO: check load capacity again?
            // sometimes the whole action should fail if the order cannot be fulfilled...?   
            if (!expedition.OwnedEntities.Buy(buyer, order,
                        BuyActionType.BuyReducedAmountsIfNeeded, BuyActionType.MaxAmountToSpend, out spentAmount, out boughtItems))
            {
                isOK = false;
                return; 
            }

            // create a contract object to allow for refunds/mission aborts...
            Contract contract = new Contract()
            {
                ContractTemplate = BuyActionType.ContractTemplate,
                PaidAmount = spentAmount,
                TransferredEntities = boughtItems.Select(e => e.ID).ToList()
            };

            parent.Contracts.Add(contract);

            TransactionIsFinished = true;

        }

        public override void StartMission()
        {
            if (!TransactionIsFinished)
            {
                Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)this.parent.MissionTemplate.Allegiance);

                Allegiance allegiance;
                Expedition expedition;
                IKnownEntityData terminal;
                Site site;
                if (this.BuyActionType.MissionStopTemplate.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
                {
                    CommunicationMethod? method;
                    if (allegiance != null && Communication.Communicates.IsInCommunicationRange(thisAllegiance, allegiance, out method))
                    {

                        // if in comm range, transfer ownership now. otherwise we will do it on arrival.
                        bool isOK;
                        MakeTransaction(expedition, out isOK);
                    }
                }
                else 
                {
                    // error, should not happen, we just validated the mission.
                }
            }
        }

        private void LoadItems()
        {
           
            Entity container = parent.GetVehicleToLoad();
            // if on playsite, now we should set up hauling jobs to load the items
            // and monitor when they are loaded.
            // for now, load instantly.
            // when should ownership change? It has to be done when loaded, because otherwise the haulers will never see the items...

            MissionJob missionJob = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);

            Contract contract = parent.Contracts.FirstOrDefault(c => c.ContractTemplate == this.BuyActionType.ContractTemplate);

            
            // off site, we just load the items directly. - this can transfer ownership as well!
         /*   if (!parent.IsOnPlaySite())
            {*/
                foreach (var item in contract.TransferredEntities)
                {
                    // TODO: buying large vehicles? add them to the convoy instead.
                    Entity entity = Entity.FindByID(item);

                    if (entity != null)
                    {

                        if (entity.ContainedBy != null)
                        {
                            Entity containingEntity = Entity.FindByID(entity.ContainedBy.Value);
                            if (containingEntity != null)
                            {
                                containingEntity.Contains.Uncontain(entity, placeInStorageEntity: container);
                            }
                        }
                        else
                        {
                            // take from the ground: 
                            container.Contains.AddToContain(entity); //, compartment, placeInStorage);   

                        }

                        entity.AssignedToJob = missionJob.ID; // assign it...
                    }           
                }
         //   }
                       
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            snapshotActionTemplateID = (MissionActionTemplateID)sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(BuyActionType);

            TransactionIsFinished = sn.DoBool(TransactionIsFinished);

            sn.Ignore(BuyActionType);

            return base.DoSnapshot(sn);


        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            BuyActionType = (BuySellActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);

            base.LoadPostProcess(sn);
        }
    }
}
