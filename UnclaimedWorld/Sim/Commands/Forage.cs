using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;

namespace UWGame.SimSide.Commands
{
    public class Examine : Control.Commands.Command
    {
        public long EntityGroupID;

        public bool GiveClientFeedback;

        public ZoneCommand ZoneCommand;

        public Priority? Priority;

        public Examine()
        {
        }


        public Examine(ZoneID zoneID, bool giveClientFeedback, EntityGroupID entityGroupID)
        {
            this.ZoneCommand = new ZoneCommand(zoneID);
                       
            this.GiveClientFeedback = giveClientFeedback;
            this.EntityGroupID = (long)entityGroupID;
        }

        public Examine(MapArea mapArea, bool giveClientFeedback, EntityGroupID entityGroupID)
        {
            this.ZoneCommand = new ZoneCommand(mapArea);
          
            this.GiveClientFeedback = giveClientFeedback;
            this.EntityGroupID = (long)entityGroupID;
        }

        public override void Execute(bool giveClientFeedback)
        {
            EntityGroup entityGroupToUse;

            Zone zoneToScout = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);


            bool forageSuccesful = ExamineArea(zoneToScout, entityGroupToUse);

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (forageSuccesful)
                {
                    The.Client.OnForageArea(zoneToScout);
                }
            }
        }

        private bool ExamineArea(Zone zone, EntityGroup entityGroupToUse)
        {

            zone.ExamineJob = new ScoutingJob(zone, entityGroupToUse, true); // expedition.PlayerSetWorkPriority); // The.Sim.Site.PlayerAllegiance.ScoutingJobs);
           // zone.ExamineJob.Examine = true;

            if (Priority.HasValue)
            {
                zone.ExamineJob.Priority = Priority.Value;
            }

            return true;
        }
    }
}
