using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;
using UWGame.SimSide.Entities.Owners;

namespace UWGame.SimSide.Commands
{
    /// <summary>
    /// store the data needed to recreate a mission... we cannot XML-serialize the actual objects.
    /// </summary>
    public class CreateMission : Control.Commands.Command
    {       
       
        public long MissionTemplate;

        /// <summary>
        /// the vehicles should get a Mission job lock set on them when Mission executes
        /// 
        /// type + id
        /// </summary> 
        public SerializableDictionary<string, List<long>> Vehicles;

        /// <summary>
        /// The owner of the Mission, and the Job. This can be an NPC allegiance if the player has hired the vehicles
        /// </summary>
        public long OwnerID;

        /*
        /// <summary>
        /// The Payer for the mission, if any
        /// </summary>
        public MissionPayment Payment;
        */


        public CreateMission(MissionTemplateID templateID, EntityGroupID ownerID, Dictionary<EntityType, List<Entity>> vehicles/*, MissionPayment payment*/)
        {
            this.MissionTemplate = (long)templateID;

            if (vehicles != null)
            {
                this.Vehicles = new SerializableDictionary<string, List<long>>();

                foreach (var item in vehicles)
                {
                    foreach (var vehicle in item.Value)
                    {
                        Common.AddToMultiList(Vehicles, item.Key.KeyName, (long)vehicle.EntityID);
                    }
                }
            }

            this.OwnerID = (long)ownerID;
           // this.Payment = payment;
        }

        public CreateMission()
        {
        }

        public override void Execute(bool giveClientFeedback)
        {
           // Allegiance allegiance = (Allegiance)LookUp<Allegiance, AllegianceID>.FindByID(allegianceID);

            MissionTemplate missionTemplate = LookUp<MissionTemplate, MissionTemplateID>.FindByID((MissionTemplateID)MissionTemplate);

            EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)OwnerID);

            Dictionary<EntityType, List<EntityID>> vehicles = null;
            if (Vehicles != null)
            {
                vehicles = new Dictionary<EntityType, List<EntityID>>();

                foreach (var item in Vehicles)
                {
                    foreach (var vehicle in item.Value)
                    {
                        Common.AddToMultiList(vehicles, GameData.Instance.AllEntityTypes[item.Key], (EntityID)vehicle);
                    }
                }
            }

            Mission mission = new Mission(entityGroup, missionTemplate, vehicles);


            mission.StartMission(); // make transactions now...

          /*  if (Payment != null)
            {
                Payment.Pay();
            }*/
                        

           // mission.IsActive = true;      

        }


        /// <summary>
        /// for now, we can only transfer money between 2 owners/expeditions...
        /// </summary>
      /*  public class MissionPayment
        {
            /// <summary>
            /// the owner of the mission
            /// </summary>
            public long BuyerID;

           
            public long SellerID;


            /// <summary>
            /// payment, if any, for the mission
            /// </summary>
            public decimal? Amount;

            /// <summary>
            /// TODO: negative cost should be deducted from sellers...
            /// make a series of transactions
            /// </summary>
           // public long Payer;


            public void Pay()
            {
                IOwner buyer = LookUpOwners.FindByID((OwnerID)BuyerID);
                IOwner seller = LookUpOwners.FindByID((OwnerID)SellerID);

                if (Amount > 0)
                {
                    buyer.OwnedEntities.Parent.TradeCredits -= Amount.Value;
                    seller.OwnedEntities.Parent.TradeCredits += Amount.Value;
                }
                else
                {
                    buyer.OwnedEntities.Parent.TradeCredits += Amount.Value;
                    seller.OwnedEntities.Parent.TradeCredits -= Amount.Value;            
                }
               
            }
        }*/
    }
}
