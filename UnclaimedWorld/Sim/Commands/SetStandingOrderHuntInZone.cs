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
    /// there is also SetStandingOrder which sets the number of carcasses to store
    /// </summary>
    public class SetStandingOrderHuntInZone : Control.Commands.Command
    {      
       // public string ResourceItemType;     
 
        /// <summary>
        /// critter??
        /// </summary>
        public string EntityType;
       
        public ZoneCommand ZoneCommand;

        public bool Enable;


        public bool GiveClientFeedback;

        public long EntityGroupID;

        public Priority? Priority;

        public SetStandingOrderHuntInZone()
        {
        }

        public SetStandingOrderHuntInZone(ZoneID zoneID, bool giveClientFeedback, string entityType, bool enable, EntityGroupID entityGroupID)
        {
            this.ZoneCommand = new ZoneCommand(zoneID);            

            //ResourceItemType = resourceItemType;           
            EntityType = entityType;
            GiveClientFeedback = giveClientFeedback;
            Enable = enable;

            this.EntityGroupID = (long)entityGroupID;
         
        }

        public SetStandingOrderHuntInZone(MapArea mapArea, bool giveClientFeedback, string entityType, bool enable, EntityGroupID entityGroupID)
        {
            this.ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
          
           // ResourceItemType = resourceItemType;           
            EntityType = entityType;
            GiveClientFeedback = giveClientFeedback;
            Enable = enable;

            this.EntityGroupID = (long)entityGroupID;
            
        }

        public override void Execute(bool giveClientFeedback)
        {
            EntityType entityType = GameData.Instance.AllEntityTypes[EntityType];         
          //  ResourceType resourceType = GameData.Instance.AllResourceTypes[EntityType];


            EntityGroup entityGroupToUse;
            Zone zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);

            if (Enable)
            {
                zone.ZoneHunt.AllowStandingOrderHunt.Add(entityType);
            }
            else
            {
                zone.ZoneHunt.AllowStandingOrderHunt.Remove(entityType);
            }


            if (GiveClientFeedback && giveClientFeedback)
            {
                The.InGameUI.ContextMenu.OnSetStandingGatherOrder(zone);
            }

        }


        

    }
}
