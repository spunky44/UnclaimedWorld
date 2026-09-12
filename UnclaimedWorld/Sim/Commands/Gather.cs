using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Jobs;
using UWGame.ClientSide.Interface.HUD_Windows;

namespace UWGame.SimSide.Commands
{
    /// <summary>
    /// used for direct gather orders.
    /// standing gather orders use the 2 commands SetStandingOrder and SetStandingOrderGatherInZone
    /// </summary>
    public class Gather : Control.Commands.Command
    {
        public int JobDifference;

        public string ResourceItemType;
        public string ProcessType;
        public string ResourceType;
       
        public ZoneCommand ZoneCommand;

        public bool GiveClientFeedback;

        public long EntityGroupID;

        public Priority? Priority;

        public Gather()
        {
        }

        public Gather(ZoneID zoneID, bool giveClientFeedback, int jobDifference, string resourceItemType, string processType, string resourceType, EntityGroupID entityGroupID)
        {
            this.ZoneCommand = new ZoneCommand(zoneID);            

            JobDifference = jobDifference;
            ResourceItemType = resourceItemType;
            ProcessType = processType;
            ResourceType = resourceType;
            GiveClientFeedback = giveClientFeedback;

            this.EntityGroupID = (long)entityGroupID;
         
        }

        public Gather(MapArea mapArea, bool giveClientFeedback, int jobDifference, string resourceItemType, string processType, string resourceType, EntityGroupID entityGroupID)
        {
            this.ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
          
            JobDifference = jobDifference;
            ResourceItemType = resourceItemType;
            ProcessType = processType;
            ResourceType = resourceType;
            GiveClientFeedback = giveClientFeedback;

            this.EntityGroupID = (long)entityGroupID;
            
        }

        public override void Execute(bool giveClientFeedback)
        {
            EntityType resourceItemType = GameData.Instance.AllEntityTypes[ResourceItemType];
            ProcessType processType = GameData.Instance.AllProcessTypes[ProcessType];
            ResourceType resourceType = GameData.Instance.AllResourceTypes[ResourceType];
            
            
            EntityGroup entityGroupToUse;
            Zone zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);

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

                JobManager.DestroyJobsIntelligently(existingJobs, 
                    noOfJobsToRemove);

            }

            if (GiveClientFeedback && giveClientFeedback)
            {
                The.InGameUI.ContextMenu.OnGather(zone);
            }
        }


        


        

    }
}
