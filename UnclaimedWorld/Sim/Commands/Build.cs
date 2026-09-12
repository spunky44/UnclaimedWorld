using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using WindowSystem;
using UWGame.SimSide.Expeditions;
using UWGame.Control.Commands;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands
{
    /// <summary>
    /// places a build order
    /// </summary>
    public class Build: Command
    {
        public long EntityGroupID;

        //XML serializer can only serialize public fields by default
        public string EntityTypeKey; // store string for serialization...
        public Vector3 Location;
        public bool GiveClientFeedback;


        public Build()
        { 
        }

        public Build(EntityType structureType, Vector3 location, bool giveClientFeedback, EntityGroupID entityGroupID)
        {
            this.EntityTypeKey = structureType.KeyName;
            this.Location = location;
            this.GiveClientFeedback = giveClientFeedback;
            this.EntityGroupID = (long)entityGroupID;
        }

        // starts a building job and places a ghost entity on the map
        //Sim.StartBuild(EntityType structureType, Vector3 location, Owner owner)  // OwnerID ?
        // Sim.StartBuild(EntityType structureType, Vector3 location, Allegiance allegiance) // for creatures... AllegianceID ?
        // Client.StartBuild(EntityType structureType, Vector3 location) // TODO

        public override void Execute(bool giveClientFeedback)
        {
            // call a method in the SIM class that places the building job, such as PrepareAndStartBuildingJob or a refactoring - MAKE it clean!!!

            bool buildSuccesful = StartBuild();

            if (giveClientFeedback && GiveClientFeedback)
            {
                // call a method in the CLIENT that gives feedback to the player (blinking/beep) IF NEEDED! Not needed when the command is passed from an AI
                // call Client method
                if (buildSuccesful)
                {
                    The.Client.StartBuild();
                }
            }   
        }


        private bool StartBuild()
        {
           // Expedition expedition = The.Map.GetClosestExpedition(location);
            EntityGroup entityGroupToUse = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)EntityGroupID);

          //  return Structure.PrepareAndStartBuildingJob(structureType, expedition.OwnedEntities, expedition, Priority, location);

            return Structure.PrepareAndStartBuildingJob(EntityTypeKey, entityGroupToUse, (IOwner)entityGroupToUse.Parent, Location);
        }
                            
    }
}
