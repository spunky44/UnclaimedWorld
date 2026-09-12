using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands
{
    public class DeleteZone : Control.Commands.Command
    {
        public long zoneID;


        public DeleteZone()
        {
        }
        public DeleteZone(ZoneID zoneID)
        {
            this.zoneID = (long)zoneID;

        }

        public override void Execute(bool giveClientFeedback)
        {
            Zone zone = LookUp<Zone, ZoneID>.FindByID((ZoneID)zoneID);

            zone.Destroy();

         

        }
    }
}
