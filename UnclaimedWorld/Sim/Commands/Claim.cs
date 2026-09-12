using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities.Owners;

namespace UWGame.SimSide.Commands
{
    public class Claim : Control.Commands.Command
    {
        /// <summary>
        /// the item to claim
        /// </summary>
        public long EntityID; 

        /// <summary>
        /// the new owner of the item
        /// </summary>
        public long NewOwner;
      

        /// <summary>
        /// for SharedKnowledge lookup of entity
        /// </summary>
        public long AllegianceID;


        public bool GiveClientFeedback;

        public Claim()
        { 
        }

        public Claim(EntityID entityID, AllegianceID allegianceID, OwnerID newOwner, bool giveClientFeedback)
        {
            this.EntityID = (long)entityID;
            this.AllegianceID = (long)allegianceID;
            this.NewOwner = (long)newOwner;
            this.GiveClientFeedback = giveClientFeedback;
        }

        public override void Execute(bool giveClientFeedback)
        {
            bool success = DoClaim();

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (success)
                {
                    The.Client.OnClaimEntity();
                }
            }   
        }


        private bool DoClaim()
        {

            Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)AllegianceID);

            IKnownEntityData entityData;
            allegiance.SharedKnowledge.GetKnownData((EntityID)EntityID, out entityData);
            Entity itemEntity = entityData as Entity;

            IOwner newOwner = LookUpOwners.FindByID((OwnerID)NewOwner);


            // can only claim seen items!
            if (itemEntity != null)
            {                
                itemEntity.ChangeOwnership(newOwner);  //               expedition);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
