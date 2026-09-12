using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.XmlCollections;
using System.Xml.Serialization;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Overland.Missions.Templates
{
    /// <summary>
    /// Buys and loads!
    /// 
    /// it seems like buy/sell is the same operation, only with the viewpoints exchanged.
    /// 
    /// </summary>
    public class BuySellActionTemplate : MissionActionTemplate
    {
        
        public bool BuyReducedAmountsIfNeeded = true;

        public decimal? MaxAmountToSpend;

        
        public bool isSelling;


        /// <summary>
        /// the corresponding Contract is saved in Mission so it survives BuyAction becoming completed and removed...
        /// </summary>
        public ContractTemplate ContractTemplate;
        [XmlIgnore]
        ContractTemplateID snapshotContractTemplate;

        public override string Name
        {
            get
            {
                if (isSelling)
                {
                    return "Sell";
                }
                else
                {
                    return "Buy";
                }
            }
        }

      

        public BuySellActionTemplate() 
        { 
            // needed for XmlSerializer 
        }

        public BuySellActionTemplate(MissionTemplate parent, MissionStopTemplate missionStopTemplate, 
            //Dictionary<EntityType, int> orders, 
            Dictionary<EntityType, List<EntityID>> orders, 
            bool buyReducedAmountsIfNeeded, decimal? maxAmountToSpend, OwnerID buyerID, OwnerID sellerID, bool allowDeleting)
            : base(missionStopTemplate, allowDeleting)
        {
            System.Diagnostics.Debug.Assert(buyerID != sellerID, "Illegal contract...");

            ContractTemplate = new ContractTemplate()
            {
               // Goods = new SerializableDictionary<string, int>(orders.ToDictionary(k => k.Key.KeyName, k => k.Value)), // cannot cast generic types
                Entities = new SerializableDictionary<string, List<long>>(orders.ToDictionary(k => k.Key.KeyName, k => k.Value.Select(e => (long)e).ToList())), // cannot cast generic types
                BuyerID = (long)buyerID,
                SellerID = (long)sellerID
            };

            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
            IOwner seller = LookUpOwners.FindByID((OwnerID)ContractTemplate.SellerID);
            if (seller.Allegiance == thisAllegiance)
            {
                isSelling = true;
            }
            else
            {
                isSelling = false;
            }

            this.BuyReducedAmountsIfNeeded = buyReducedAmountsIfNeeded;
            this.MaxAmountToSpend = maxAmountToSpend;

        }

        public static bool ValidateWorkingTerminalCanBuy(MissionTemplate parent, IKnownEntityData terminalData, ref List<string> errors)
        {
            if (!ValidateWorkingTradeTerminal(terminalData, ref errors))
            {
                return false;
            }

            if (terminalData.OwnedBy != null
             && terminalData.OwnedBy == (OwnerID)parent.OwnerID) //The.InGameUI.GetUIOwnerID())
            {
                Common.AddToList(ref errors, "The terminal is owned by us.");
                return false;
            }

            // add more rules when more complex mission are allowed...

            return true;

        }

        public static bool ValidateWorkingTerminalCanSell(MissionTemplate parent, IKnownEntityData terminalData, ref List<string> errors)
        {
            if (!ValidateWorkingTradeTerminal(terminalData, ref errors))
            {
                return false;
            }

            
            if (terminalData.OwnedBy == null
             || terminalData.OwnedBy != (OwnerID)parent.OwnerID) //The.InGameUI.GetUIOwnerID())
            {
                Common.AddToList(ref errors, "The terminal is not owned by us.");
                return false;
            }

            return true;
        }

       

        private static bool ValidateWorkingTradeTerminal(IKnownEntityData terminalData, ref List<string> errors)
        {
            if (terminalData.EntityType.ContainerType == null || !(terminalData.EntityType.ContainerType is TerminalContainerType))
            {
                Common.AddToList(ref errors, "The terminal does not support trading.");
                return false;
            }

            if(!ValidateWorkingTerminal(terminalData, ref errors))
            {
                return false;
            }
            

            return true;

        }

      

        public override bool Validate(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors)
        {
            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);                       

            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;
            Site site;
            if (thisAllegiance == null 
                || !MissionStopTemplate.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                return false;
            }

            if (isSelling)
            {
                if (!ValidateWorkingTerminalCanSell(parent, terminal, ref errors))
                {
                    return false;
                }
            }
            else
            {
                if (!ValidateWorkingTerminalCanBuy(parent, terminal, ref errors))
                {
                    return false;
                }
            }

            // get the home expedition for policy check. (NPC expeditions don't have policies.)
            Expedition homeExpedition = parent.GetHomeExpedition();
          
            SharedKnowledge sharedKnowledge = allegiance.SharedKnowledge;
            if (ContractTemplate != null)
            {
                OwnerID sellerID = (OwnerID)ContractTemplate.SellerID;
                   
                // validate that all entities are in the terminal:
                foreach (var list in ContractTemplate.Entities)
                {
                    for (int i = list.Value.Count - 1; i >= 0; i--)
                    {
                        hasMeaning = true;
  
                        EntityID entityId = (EntityID)list.Value[i];

                        if (!ItemIsValidForSale(sharedKnowledge, entityId, terminal, sellerID, homeExpedition))
                        {
                            AddInvalidItemError(ref errors, expedition);
                            return false;
                        }                      
                    }
                }        
            }

            return true;
        }


        /// <summary>
        /// must be inside the terminal, must be owned by seller...
        /// NEW: no broken items
        /// </summary>
        /// <param name="sharedKnowledge"></param>
        /// <param name="entityID"></param>
        /// <param name="terminalData"></param>
        /// <param name="sellerID"></param>
        /// <returns></returns>
        public static bool ItemIsValidForSale(SharedKnowledge sharedKnowledge, EntityID entityID, IKnownEntityData terminalData, OwnerID sellerID, Expedition tradingExpeditionWithPolicy)
        {
            IKnownEntityData entityData;
            if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID, out entityData)))
            {
                if (entityData.ContainedBy != terminalData.EntityID
                    || entityData.OwnedBy != sellerID
                    || entityData.PartOfID != null
                    || !entityData.IsCompleted()
                    || !Entity.IsFunctional(entityData)
                    || (tradingExpeditionWithPolicy != null && !tradingExpeditionWithPolicy.Policy.CanProduceOrTrade(entityData.EntityType.TierOrAreaType)))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private static List<string> AddInvalidItemError(ref List<string> errors, Expedition expedition)
        {
            Common.AddToList(ref errors, string.Format("Some of the items ordered at {0} are no longer valid. Review the order.", expedition.Name));

            return errors;
        }

        public override ActionTypes ActionType
        {
            get
            {
                if (isSelling)
                {
                    return ActionTypes.Sell;
                }
                else
                {
                    return ActionTypes.Buy;
                }
            }
        }

        public override MissionAction CreateMissionAction(Mission mission)
        {
            return new BuySellAction(mission, this);
        }

        public override float ComputeTotalCargoBulk()
        {
            float totalBulk = 0;
            float? bulk;

                     
            if (ContractTemplate.Entities /* Goods*/ != null)
            {
                foreach (var item in ContractTemplate.Entities) // .Goods)
                {
                    EntityType itemType = GameData.Instance.AllEntityTypes[item.Key];

                    if (itemType.ItemType != null
                        && itemType.ItemType.MaximumBulk.HasValue) // items without this should not be traded...
                    {
                        bulk = itemType.ItemType.MaximumBulk.Value;

                        totalBulk += item.Value.Count * bulk.Value;
                    }

                }
            }

            return totalBulk;
        }

        public override decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost)
        {           
            decimal totalPrice = 0;
            
            boughtItemsCost = 0;
            soldItemsCost = 0;

            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);

            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;
            Site site;
            if (thisAllegiance == null 
                || !MissionStopTemplate.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal))
            {
                return 0;
            }

            IOwner buyer = LookUpOwners.FindByID((OwnerID)ContractTemplate.BuyerID);
            IOwner seller = LookUpOwners.FindByID((OwnerID)ContractTemplate.SellerID);

            EntityType entityType;
            if (ContractTemplate.Entities != null) 
            {
                decimal? price;

                foreach (var item in ContractTemplate.Entities) 
                {
                    entityType = GameData.Instance.AllEntityTypes[item.Key];
                    price = GetTradePrice(entityType, buyer.OwnedEntities, seller.OwnedEntities);

                    totalPrice += item.Value.Count * price.Value;

                    /*
                    // get the value of the items we are going to buy:
                    price = expedition.OwnedEntities.GetSellPrice(GameData.Instance.AllEntityTypes[item.Key]); // !!!

                    totalPrice += item.Value.Count * (decimal)(price.Value);
                    */
                }
            }

            // if the seller is the same entity as the mission owner, then return a negative cost          
            if (isSelling)
            {
                soldItemsCost = totalPrice;

                return -1m * totalPrice;
            }
            else
            {
                boughtItemsCost = totalPrice;

                return totalPrice;
            }
           

            /* OLD:

            // if the seller is the same entity as the mission owner, then return a negative cost          
            //IOwner seller = LookUpOwners.FindByID((OwnerID)ContractTemplate.SellerID);
            if (isSelling) // seller.Allegiance == thisAllegiance)
            {
                return -1m * totalPrice;
            }
            else
            {
                return totalPrice;
            }*/
        }

        /// <summary>
        /// players cannot set prices...
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="buyer"></param>
        /// <param name="seller"></param>
        /// <returns></returns>
        public static decimal? GetTradePrice(EntityType entityType, EntityGroup buyer, EntityGroup seller)
        {
            if (buyer == null || buyer.GetAllegiance().AllegianceType == AllegianceType.Player)
            {
                return (decimal?)seller.GetSellPrice(entityType);
            }
            else
            {
                return (decimal?)buyer.GetBuyPrice(entityType);
            }
        }

        public override void AssignIDs()
        {
            base.AssignIDs();

            ContractTemplate.AssignIDs();
        }

        public override void Destroy()
        {
            base.Destroy();

            ContractTemplate.Destroy();
        }

        public override Snapshots.ISnapshot DoSnapshot(Snapshots.Snapshotter sn)
        {
            base.DoSnapshot(sn);

            snapshotContractTemplate = (ContractTemplateID)sn.SnapshotID<ContractTemplate, ContractTemplateID>(ContractTemplate); 
            //Orders = sn.DoSerializableDictionary(Orders); 
            BuyReducedAmountsIfNeeded = sn.DoBool(BuyReducedAmountsIfNeeded);
            MaxAmountToSpend = sn.DoDecimalNullable(MaxAmountToSpend);
           // BuyerID = sn.DoInt64(BuyerID);
            isSelling = sn.DoBool(isSelling);

            return this;
        }

        public override void LoadPostProcess(Snapshots.Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            ContractTemplate = LookUp<ContractTemplate, ContractTemplateID>.FindByID(snapshotContractTemplate);

        }
    }
}
