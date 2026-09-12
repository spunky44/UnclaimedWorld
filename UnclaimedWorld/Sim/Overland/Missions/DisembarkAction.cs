using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Allegiances;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.AI;

namespace UWGame.SimSide.Overland.Missions
{
    public class DisembarkAction: MissionAction
    {
        DisembarkActionTemplate template;
        private MissionActionTemplateID snapshotActionTemplateID;

        public DisembarkAction()
        {

        }

        public DisembarkAction(Mission /*MissionActions*/ parent, DisembarkActionTemplate template)
             : base(parent) 
        {
            this.template = template;    
          
        }

        

        public override bool Update(GameTime elapsed)
        {
            base.Update(elapsed);

            Unload();

            return true;
        }


        private void Unload() 
        {
            Allegiance destinationAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.CurrentLocation.Value.AllegianceID);  //The.Sim.World.GetAllegianceWithID(DestinationAllegiance);
            Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)parent.CurrentLocation.Value.ExpeditionID);
        
          //  Entity vehicleToUnload = parent.GetVehicleOrAgentToLoad();

           
              //  vehicleToUnload.Contains.IterateContained(e => vehicleToUnload.Contains.Uncontain(e));
               

            PassengerListTemplate passengerTemplate = parent.MissionTemplate.GetPassengerListTemplate();
            if (passengerTemplate != null)
            {
                foreach (EntityID entityID in passengerTemplate.Passengers)
                {
                    UnloadEntity(entityID, destinationAllegiance); //, location); 
                }
            }
        }

        private void UnloadEntity(EntityID entityID, Allegiance destinationAllegiance) //Expedition expedition) //, Vector3? location)
        { 
            Entity entity;
            entity = Entity.FindByID(entityID);

            Entity vehicleToUnload = parent.GetVehicleToLoad();

            
           // MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);

          /*  Allegiance thisAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)this.parent.MissionTemplate.Allegiance);

            Site site;
            Allegiance allegiance;
            Expedition sellingExpedition;
            IKnownEntityData terminal;
            template.MissionStopTemplate.TravelLocation.ResolveLocation(thisAllegiance.SharedKnowledge, out site, out allegiance, out expedition, out terminal);
            */
            if (vehicleToUnload != null)
            {
                // tell everyone to get off:
                // if on play site, set an AI goal, if on othersite, simply uncontain.
                if (destinationAllegiance.Site != null && destinationAllegiance.Site.IsPlaySite)
                {
                    SendGetOffMessage(entity);
                    //job.IterateVehicleContents(e => SendGetOffMessage(e));
                }
                else
                {
                    vehicleToUnload.Contains.Uncontain(entity);
                }
            }
        
            /*
            if (isNewPlayerImmigrant)
            {
                // fire triggers, events etc. after entity is fully init'ed:  

                List<ActionSets> defaultActionSets;
                entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.SwitchedToPlayerAllegiance, out defaultActionSets);

                Goal.FireEventActions(entity, null, defaultActionSets, null);
            }

            */

            //Ownership needs to be called after placement has occured.
         /*   if (entity.EntityType.IntelligenceType == null)
                entity.ChangeOwnership(The.Sim.PlaySite.GetFirstPlayerExpedition());

            Contract contract = LookUp<Contract, ContractID>.FindByID(parent.Contract);
            if (contract != null)
            {
                contract.Fullfill();
            }*/

           
        }


        private void SendGetOffMessage(Entity agent)
        {
            if (agent.EntityType.IntelligenceType != null)
            {               
                if (agent.IsAwakeAndActive())
                {
                    Message message = new Message(Message.MessageTypes.Disembark); // GetOff);

                    agent.Intelligence.Brain.SendMessage(message);
                }
                else
                { 
                    // if the character is unconscious, simply uncontain:
                    Entity container;
                    if (agent.GetContainedBy(out container))
                    {
                        container.Contains.Uncontain(agent);
                    }
                }                    
            }
        }



        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            snapshotActionTemplateID = (MissionActionTemplateID)sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(template);

            sn.Ignore(template);

            return base.DoSnapshot(sn);


        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            template = (DisembarkActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);

            base.LoadPostProcess(sn);
        }
    }
}
