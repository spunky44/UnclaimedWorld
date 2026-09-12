using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands
{
    /// <summary>
    /// used for direct hunt orders. Since unlike Gather there is a cap on FindPreyJobs in the zone, we cannot create all jobs at once. The JobManager has to add them.
    /// 
    /// standing hunt orders use the 2 commands SetStandingOrder and SetStandingOrderHuntInZone
    /// </summary>
    public class HuntArea : Control.Commands.Command
    {
        public string EntityType;
        public int Amount; //JobDifference;
               


        /// <summary>
        /// the owner of the job
        /// </summary>
        public long EntityGroupID;

        public bool GiveClientFeedback;

        public ZoneCommand ZoneCommand;

        public bool RemoveAfterFirstSuccessfulHunt;

        public Priority? Priority;

        /// <summary>
        /// the created zone
        /// </summary>
        private Zone zone;

        public HuntArea()
        {
        }


        public HuntArea(ZoneID zoneID, bool giveClientFeedback, EntityType creatureToHunt, int amount, bool removeAfterSuccessfulHunt, EntityGroupID entityGroupID) //, Priority priority = Jobs.Priority.Normal)
        {
            this.ZoneCommand = new ZoneCommand(zoneID);
            this.Amount = amount;
            this.RemoveAfterFirstSuccessfulHunt = removeAfterSuccessfulHunt;
            this.EntityType = creatureToHunt.KeyName;
            this.GiveClientFeedback = giveClientFeedback;
            this.EntityGroupID = (long)entityGroupID;
           // this.Priority = priority;
        }

        public HuntArea(MapArea mapArea, bool giveClientFeedback, EntityType creatureToHunt, int amount, bool removeAfterSuccessfulHunt, EntityGroupID entityGroupID) //, Priority priority = Jobs.Priority.Normal)
        {
            this.ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
            this.Amount = amount;
            this.RemoveAfterFirstSuccessfulHunt = removeAfterSuccessfulHunt;
            this.EntityType = creatureToHunt.KeyName;
            this.GiveClientFeedback = giveClientFeedback;
            this.EntityGroupID = (long)entityGroupID;
           // this.Priority = priority;
        }

        public override void Execute(bool giveClientFeedback) 
        {
            EntityGroup entityGroupToUse;

            zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);


            bool huntSuccesful = DoHuntArea(zone, entityGroupToUse);

            if (giveClientFeedback && GiveClientFeedback)
            {
                if (huntSuccesful)
                {
                    The.Client.OnHuntArea(zone);
                }
            }
        }

        public Zone GetZone()
        {
            return zone;
        }

        /*
           Dictionary<ResourceType, List<ProcessJob>> jobsInZone = zone.HarvestJobs;

            if (JobDifference > 0)
            {
                // add jobs
                JobManager.AddHarvestJobs(JobDifference, entityGroupToUse, resourceItemType, processType, resourceType, zone, Priority);

            }
            else if (JobDifference < 0)
            {
                // remove some jobs
                int noOfJobsToRemove = Math.Abs(JobDifference);

                List<ProcessJob> existingJobs;
                jobsInZone.TryGetValue(resourceType, out existingJobs); // GetAllHarvestJobsInArea(); // JobManager.GetProductionJobs(expeditionOwner, item.Key.ResourceItem);

                JobManager.DestroyJobsIntelligently(existingJobs, noOfJobsToRemove);

            }
         
         
         */

        private bool DoHuntArea(Zone zoneToHuntIn, EntityGroup entityGroupToUse)
        {
                       
            /*
            if (Priority.HasValue)
            {
                zoneToHuntIn.FindPreyJob.Priority = Priority.Value;
            }*/

          
            // otherJobManager has to create the jobs, based on the zone cap.
            EntityType creature = GameData.Instance.AllEntityTypes[EntityType];

            zoneToHuntIn.ZoneHunt.CreaturesToHunt[creature] = Amount;
            

            return true;
        }

        /*

        private bool DoHuntArea(Zone zoneToHuntIn, EntityGroup entityGroupToUse)
        {
           
            zoneToHuntIn.FindPreyJob = new FindPreyJob(zoneToHuntIn, entityGroupToUse); //, Priority);

            if (Priority.HasValue)
            {
                zoneToHuntIn.FindPreyJob.Priority = Priority.Value;
            }

            return true;
        }*/
    }
}

