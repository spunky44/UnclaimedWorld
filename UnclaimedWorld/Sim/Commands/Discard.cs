using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Buildings;

namespace UWGame.SimSide.Commands
{
    public class Discard : Control.Commands.Command
    {
        /// <summary>
        /// the item to discard
        /// </summary>
        public long EntityID;

       
        /// <summary>
        /// for SharedKnowledge lookup of entity
        /// </summary>
        public long AllegianceID;


        public bool GiveClientFeedback;


        public Discard()
        { 
        }
        public Discard(EntityID entityID, AllegianceID allegianceID, bool giveClientFeedback)
        {
            this.EntityID = (long)entityID;
            this.AllegianceID = (long)allegianceID;
            this.GiveClientFeedback = giveClientFeedback;
        }

        public override void Execute(bool giveClientFeedback)
        {
            bool discardSuccesful = DiscardEntity();

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (discardSuccesful)
                {
                    The.Client.OnDiscardEntity();
                }
            }   
        }


        private bool DiscardEntity()
        {
            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)AllegianceID);

            IKnownEntityData entityData;
            EntityResult result = allegiance.SharedKnowledge.GetKnownData((EntityID)EntityID, out entityData);

            if (entityData != null)
            {

                DoDiscard(entityData);

                return true;
            }
            else return false;
        }



        public static void DoDiscard(IKnownEntityData entityData)
        {
          /*  EntityGroup ownerOfItem = null;
            IOwner iowner;
            LookUpOwners.ResolveEntityOwner(entityData, out iowner); // we don't need this if we can call ChangeOwnership instead...

            if (iowner != null)
                ownerOfItem = iowner.OwnedEntities;


            if (ownerOfItem != null)
            {*/
                // even if the item no longer existed, we have now given up ownership of it. For reclaim, we must be able to see it.

                entityData.ChangeOwnership(null, Entity.GiveNewOwnerKnowledge.No);

              /*  ownerOfItem.DeleteEntity(entityData.EntityID, entityData.EntityType);
                entityData.OwnedBy = null;*/


                entityData.AssignedToJob = null; // NEW: workaround for AI bugs...


               /*
                if (entityData.Households != null)
                {
                    for (int i = entityData.Households.Count - 1; i >= 0; i--)
                    {
                        Household household = LookUp<Household, HouseholdID>.FindByID(entityData.Households[i]);
                        if (household != null)
                        {
                            if (household.Home == entityData.EntityID)
                            {
                                Residence.RemoveHousehold(ownerOfItem.GetAllegiance().SharedKnowledge, household);

                                //household.Home = null;
                            }
                        }                        
                    }


                    entityData.Households.Clear();
                }   */            
         //   }
        }
    }
}
