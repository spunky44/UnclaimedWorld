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
    public class AttackArea : Control.Commands.Command
    {
        public long EntityGroupID;

        public bool GiveClientFeedback;

        public Priority? Priority;

        public ZoneCommand ZoneCommand;

       // public List<string> Targets;


        public bool AttackVermin;
        public bool AttackThreats;

        public int NoOfAttackers;

        public AttackArea()
        {
        }


        public AttackArea(ZoneID zoneID, bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackThreats, /*List<EntityType> targets,*/ int noOfPatrollers) 
        {
            this.ZoneCommand = new ZoneCommand(zoneID);

            Init(giveClientFeedback, entityGroupID, attackVermin, attackThreats, /* targets,*/ noOfPatrollers);
        }

        public AttackArea(MapArea mapArea, bool giveClientFeedback, EntityGroupID entityGroupID, bool attackVermin, bool attackThreats,  /*List<EntityType> targets,*/ int noOfPatrollers) 
        {
            this.ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);

            Init(giveClientFeedback, entityGroupID, attackVermin, attackThreats, /* targets,*/ noOfPatrollers);
          //  this.Priority = priority;
        }

        private void Init(bool giveClientFeedback,EntityGroupID entityGroupID, bool attackVermin, bool attackThreats,  /*List<EntityType> targets,*/ int noOfPatrollers)
        {
            this.GiveClientFeedback = giveClientFeedback;
            this.EntityGroupID = (long)entityGroupID;

            this.AttackVermin = attackVermin;
            this.AttackThreats = attackThreats;
           // this.Targets = targets.Select(e => e.KeyName).ToList();
         
            this.NoOfAttackers = noOfPatrollers;
        }

        public override void Execute(bool giveClientFeedback)
        {
            EntityGroup entityGroupToUse;

            Zone zoneToPatrolIn = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);

            bool patrolSuccesful = DoAttackArea(zoneToPatrolIn, entityGroupToUse);

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (patrolSuccesful)
                {
                    The.Client.OnPatrolOrAttackArea(zoneToPatrolIn);
                }
            }
        }


        private bool DoAttackArea(Zone zoneToPatrolIn, EntityGroup expeditionOwner)
        {
           // List<EntityType> targets = this.Targets.Select(t => GameData.Instance.AllEntityTypes[t]).ToList();
           // zoneToPatrolIn.AttackAreaJob = new AttackAreaJob(zoneToPatrolIn, expeditionOwner,  targets, NoOfAttackers);
            zoneToPatrolIn.AttackAreaJob = new AttackAreaJob(zoneToPatrolIn, expeditionOwner, AttackVermin, AttackThreats, NoOfAttackers);

            if (Priority.HasValue)
            {
                zoneToPatrolIn.AttackAreaJob.Priority = Priority.Value;
            }

            return true;
        }
    }
}
