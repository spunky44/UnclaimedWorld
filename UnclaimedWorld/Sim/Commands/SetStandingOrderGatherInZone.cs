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
    /// only sets the enabled flag true/false. The JobManager will create any jobs.
    /// 
    /// there is also SetStandingOrder which sets the number of items to store
    /// </summary>
    public class SetStandingOrderGatherInZone : Control.Commands.Command
    {      
       // public string ResourceItemType;      
        public string ResourceType;
       
        public ZoneCommand ZoneCommand;

        public bool Enable;


        public bool GiveClientFeedback;

        public long EntityGroupID;

        public Priority? Priority;

        public SetStandingOrderGatherInZone()
        {
        }

        public SetStandingOrderGatherInZone(ZoneID zoneID, bool giveClientFeedback, string resourceType, bool enable, EntityGroupID entityGroupID)
        {
            this.ZoneCommand = new ZoneCommand(zoneID);            

            //ResourceItemType = resourceItemType;           
            ResourceType = resourceType;
            GiveClientFeedback = giveClientFeedback;
            Enable = enable;

            this.EntityGroupID = (long)entityGroupID;
         
        }

        public SetStandingOrderGatherInZone(MapArea mapArea, bool giveClientFeedback, string resourceType, bool enable, EntityGroupID entityGroupID)
        {
            this.ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
          
           // ResourceItemType = resourceItemType;           
            ResourceType = resourceType;
            GiveClientFeedback = giveClientFeedback;
            Enable = enable;

            this.EntityGroupID = (long)entityGroupID;
            
        }

        public override void Execute(bool giveClientFeedback)
        {
          //  EntityType resourceItemType = GameData.Instance.AllEntityTypes[ResourceItemType];         
            ResourceType resourceType = GameData.Instance.AllResourceTypes[ResourceType];


            EntityGroup entityGroupToUse;
            Zone zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);

            if (Enable)
            {
                zone.AllowStandingOrderHarvest.Add(resourceType);
            }
            else
            {
                zone.AllowStandingOrderHarvest.Remove(resourceType);
            }


            if (GiveClientFeedback && giveClientFeedback)
            {
                The.InGameUI.ContextMenu.OnSetStandingGatherOrder(zone);
            }

        }


        

    }
}
