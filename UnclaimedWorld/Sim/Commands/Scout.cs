using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Items;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Jobs;

namespace UWGame.SimSide.Commands
{
    public class Scout : Control.Commands.Command
    {
        public long EntityGroupID;

        public bool GiveClientFeedback;

        public Priority? Priority;


        public ZoneCommand ZoneCommand;
        
        public Scout()
        {
        }

        public Scout(ZoneID zoneID, bool giveClientFeedback, EntityGroupID entityGroupID) //, Priority priority = Jobs.Priority.Normal)
        {
            this.ZoneCommand = new ZoneCommand(zoneID);
                        
            this.GiveClientFeedback = giveClientFeedback;
            this.EntityGroupID = (long)entityGroupID;
            //this.Priority = priority;
        }

        public Scout(MapArea mapArea, bool giveClientFeedback, EntityGroupID entityGroupID) //, Priority priority = Jobs.Priority.Normal)
        {
            this.ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
            /*
            startDragTileLocation = mapArea.StartDragTile.Value;
            this.tileLocations = mapArea.GetTileLocations();*/

            this.GiveClientFeedback = giveClientFeedback;
            this.EntityGroupID = (long)entityGroupID;
            //this.Priority = priority;
        }

        public override void Execute(bool giveClientFeedback) 
        {
            EntityGroup entityGroupToUse;

            Zone zoneToScout = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);

            bool scoutSuccesful = ScoutArea(zoneToScout, entityGroupToUse);

            if (giveClientFeedback && GiveClientFeedback)
            {
                // call a method in the CLIENT that gives feedback to the player (blinking/beep) IF NEEDED! Not needed when the command is passed from an AI
                // call Client method
                if (scoutSuccesful)
                {
                    The.Client.OnScoutArea(zoneToScout);
                }
            }
        }

        private bool ScoutArea(Zone zoneToScout, EntityGroup entityGroupToUse)
        {            
            zoneToScout.ScoutingJob = new ScoutingJob(zoneToScout, entityGroupToUse, false); //, Priority); // entityGroupToUse.GetWorkPriority()); // expedition.PlayerSetWorkPriority);

            if (Priority.HasValue)
            {
                zoneToScout.ScoutingJob.Priority = Priority.Value;
            }

            return true;

        }

        
    }
}
