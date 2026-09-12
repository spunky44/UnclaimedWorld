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
    public class PatrolArea : Control.Commands.Command
    {
        public long EntityGroupID;

        public bool GiveClientFeedback;

        public Priority? Priority;

        public ZoneCommand ZoneCommand;

        public bool AttackVermin = false;
        public bool AttackTargetsOutsideZone = false;

        public int NoOfPatrollers;

        public PatrolArea()
        {
        }


        public PatrolArea(ZoneID zoneID, bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackTargetsOutsideZone, int noOfPatrollers) //, Priority priority = Jobs.Priority.Normal)
        {
            this.ZoneCommand = new ZoneCommand(zoneID);

            Init(giveClientFeedback, entityGroupID, attackVermin, attackTargetsOutsideZone, noOfPatrollers);
        }

        public PatrolArea(MapArea mapArea, bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackTargetsOutsideZone, int noOfPatrollers) //, Priority priority = Jobs.Priority.Normal)
        {
            this.ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);

            Init(giveClientFeedback, entityGroupID, attackVermin, attackTargetsOutsideZone, noOfPatrollers);
          //  this.Priority = priority;
        }

        private void Init(bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackTargetsOutsideZone, int noOfPatrollers)
        {
            this.GiveClientFeedback = giveClientFeedback;
            this.EntityGroupID = (long)entityGroupID;
            this.AttackVermin = attackVermin;
            this.AttackTargetsOutsideZone = attackTargetsOutsideZone;
            this.NoOfPatrollers = noOfPatrollers;
        }

        public override void Execute(bool giveClientFeedback)
        {
            EntityGroup entityGroupToUse;

            Zone zoneToPatrolIn = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);

            bool patrolSuccesful = DoPatrolArea(zoneToPatrolIn, entityGroupToUse);

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (patrolSuccesful)
                {
                    The.Client.OnPatrolOrAttackArea(zoneToPatrolIn);
                }
            }
        }


        private bool DoPatrolArea(Zone zoneToPatrolIn, EntityGroup expeditionOwner)
        {

            zoneToPatrolIn.PatrolJob = new PatrolJob(zoneToPatrolIn, expeditionOwner, /*Priority,*/ AttackVermin, AttackTargetsOutsideZone, NoOfPatrollers);

            if (Priority.HasValue)
            {
                zoneToPatrolIn.PatrolJob.Priority = Priority.Value;
            }

            return true;
        }
    }
}
