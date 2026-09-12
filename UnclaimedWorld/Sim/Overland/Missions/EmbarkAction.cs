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

namespace UWGame.SimSide.Overland.Missions
{
    public class EmbarkAction : MissionAction
    {
        public EmbarkActionTemplate EmbarkActionTemplate;
        private MissionActionTemplateID snapshotActionTemplateID;


        public EmbarkAction(Mission parent, EmbarkActionTemplate actionType) //, decimal maxAmountToSpend, bool buyReducedAmountsIfNeeded) // Dictionary<EntityType, int> orders)
            : base(parent)
        {
            this.EmbarkActionTemplate = actionType;
          //  this.BuyReducedAmountsIfNeeded = buyReducedAmountsIfNeeded;
          //  this.MaxAmountToSpend = maxAmountToSpend;

            //this.Orders = orders;
        }



        public EmbarkAction()
        {
        }



        public override bool Update(Microsoft.Xna.Framework.GameTime elapsed)
        {
           // IOwner buyer = LookUpOwners.FindByID((OwnerID)EmbarkActionType.ContractTemplate.BuyerID);
       //     EntityGroup buyer = LookUp<EntityGroup, EntityGroupID>.FindByID(BuyActionType.BuyerID);

           /* if (buyer == null)
            {
                HandleFailedAction();
                return true; // ??
            }*/

            Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)this.parent.MissionTemplate.Allegiance);

            Allegiance allegiance;
            Expedition expedition;
            IKnownEntityData terminal;
            Site site;

            if (thisAllegiance != null 
                && parent.CurrentLocation.Value.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal)
                && parent.CurrentLocation.Equals(EmbarkActionTemplate.PassengerListTemplate.StartingLocation)
                && Common.ListContainsRange(expedition.Members, EmbarkActionTemplate.PassengerListTemplate.Passengers.Select(p => (EntityID)p).ToList()))
            {
              /*  List<Entity> boughtItems = null;
                decimal spentAmount;                
              
                Dictionary<EntityType, int> order = EmbarkActionType.ContractTemplate.Goods.ToDictionary(k => GameData.Instance.AllEntityTypes[k.Key], k => k.Value);
                */
                // buy:
                // TODO: check load capacity again?
                // sometimes the whole action should fail if the order cannot be fulfilled...?   
               /* if (!expedition.OwnedEntities.Buy(buyer, order, out spentAmount, out boughtItems))
                {
                    HandleFailedAction();
                    return true; // ??
                }*/

                // create a contract object to allow for refunds/mission aborts...
              /*  Contract contract = new Contract()
                {
                    ContractTemplate = EmbarkActionType.ContractTemplate,
                    PaidAmount = spentAmount,
                    TransferredEntities = boughtItems.Select(e => e.ID).ToList()
                };

                parent.Contracts.Add(contract);*/

                List<Entity> entities = new List<Entity>();
                Entity entity;
                foreach (var item in EmbarkActionTemplate.PassengerListTemplate.Passengers)
                {
                    entity = Entity.FindByID((EntityID)item);
                    if (entity != null)
                    {
                        entities.Add(entity);

                    }
                    else
                    {
                        HandleFailedAction();
                        return true; // ??
                    }
                }

                // load:
                EmbarkPassengers(entities);

                
                return true;
            }
            else
            {
                HandleFailedAction();
                return true; // ??
            }
        }

       

        private void EmbarkPassengers(List<Entity> passengers)
        {
            Entity container = parent.GetVehicleToLoad();

            // from LOAD:
            // if on playsite, now we should set up hauling jobs to load the items
            // and monitor when they are loaded.
            // for now, load instantly.
            // when should ownership change? It has to be done when loaded, because otherwise the haulers will never see the items...


            if (container != null)
            {
                if (!parent.IsOnPlaySite())
                {
                    foreach (var item in passengers)
                    {
                        container.Contains.AddToContain(item); // TODO: place in cabin...          
                    }
                }
            }

            foreach (var item in passengers)
            {    
                // this won't work with agents...
                // agents should not be online, or..?
               // item.AssignedToJob = missionJob; // assign it...
            }
        }

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            snapshotActionTemplateID = (MissionActionTemplateID)sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(EmbarkActionTemplate);

            return base.DoSnapshot(sn);


        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            EmbarkActionTemplate = (EmbarkActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);

            base.LoadPostProcess(sn);
        }
    }
}
