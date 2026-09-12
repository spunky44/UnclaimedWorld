using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities.Owners;

namespace UWGame.SimSide.Communication
{
    /// <summary>
    /// this component enables commmunication outside the site
    /// </summary>
    public class Communicator: Component
    {

        public Communicator(Entity parent): base(parent)
        {           
           
        }

         public Communicator()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");       
        }


         public bool IsCommunicatorWorkingAndInRange(double distance)
         {
             if (Parent.IsCompleted()
                && Entity.IsFunctional(Parent)              
                && Parent.EntityType.CommunicatorType.IsInRange(distance))
             {
                 return true;
             }

             return false;

         }


         public void GainContact()
         {
             Allegiance thisAllegiance = Parent.GetAllegianceOrOwner();
            
             // gain contact in both directions
             foreach (var item in The.Sim.World.AllSites)
             {
                 foreach (var allegiance in item.Value.Allegiances)
                 {
                     if (allegiance != thisAllegiance
                         && allegiance.RepresentativeEntityType.IntelligenceType.CanTradeAndCommunicate == true            
                         && !thisAllegiance.AllegiancesWeAreInContactWith.Contains(allegiance.ID))
                     {
                         CommunicationMethod? method;
                         if (Communicates.IsInCommunicationRange(thisAllegiance, allegiance, out method))
                         {
                             allegiance.GainContact(thisAllegiance);
                             thisAllegiance.GainContact(allegiance);
                         }
                     }
                 }
             }

         }


         public void Destroy()
         {
             // re-evaluate all the contacts AFTER the communicator has been removed from the owner.
             if (NonLivingEntity.IsCompleted(Parent.Progress))
             {
                 UpdateCommunications();
             }
         }

         private void UpdateCommunications()
         {
             Allegiance thisAllegiance = Parent.GetAllegianceOrOwner();

             // lose contact in both directions.
             if (thisAllegiance != null)
             {
                 List<AllegianceID> invalidIDs = null;
                 List<Allegiance> allegiancesWithLostContact = null;
                 foreach (var item in thisAllegiance.AllegiancesWeAreInContactWith)
                 {
                     Allegiance otherAllegiance = LookUp<Allegiance, AllegianceID>.FindByID(item);
                     if (otherAllegiance != null)
                     {
                         CommunicationMethod? method;
                         if (!Communication.Communicates.IsInCommunicationRange(thisAllegiance, otherAllegiance, out method))
                         {
                             otherAllegiance.LoseContact(thisAllegiance);
                             Common.AddToList(ref allegiancesWithLostContact, otherAllegiance);
                         }
                     }
                     else
                     {
                         Common.AddToList(ref invalidIDs, item);
                     }

                 }

                 if (allegiancesWithLostContact != null)
                 {
                     foreach (var item in allegiancesWithLostContact)
                     {
                         thisAllegiance.LoseContact(item);
                     }
                 }

                 if (invalidIDs != null)
                 {
                     foreach (var item in invalidIDs)
                     {
                         thisAllegiance.AllegiancesWeAreInContactWith.Remove(item);
                     }
                 }
             }
         }
    }
}
